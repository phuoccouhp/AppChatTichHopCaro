using ChatApp.Shared;
using System;
using System.Collections.Generic;

namespace ChatAppServer
{
    // Simple server-side manager for tank game scores/hits
    public class TankGameManager
    {
        private class SessionData
        {
            public string GameID { get; set; }
            public string Player1 { get; set; }
            public string Player2 { get; set; }
            public Dictionary<string,int> Hits = new Dictionary<string,int>();
            public Dictionary<string,int> Score = new Dictionary<string,int>();
        }

        private readonly Dictionary<string, SessionData> _sessions = new Dictionary<string, SessionData>();

        public void CreateSession(string gameId, string p1, string p2)
        {
            var s = new SessionData{ GameID = gameId, Player1 = p1, Player2 = p2 };
            s.Hits[p1]=0; s.Hits[p2]=0; s.Score[p1]=0; s.Score[p2]=0;
            lock(_sessions) _sessions[gameId]=s;
        }

        public void RemoveSession(string gameId)
        {
            lock(_sessions) { _sessions.Remove(gameId); }
        }

        // Called when a shot hits a victim. Returns (shooterHits, shooterScore)        
        public (int hits,int score) ProcessHit(TankHitPacket hit)
        {
            if (hit==null) return (0,0);
            lock(_sessions)
            {
                if (!_sessions.TryGetValue(hit.GameID,out var s)) return (0,0);
                if (!s.Hits.ContainsKey(hit.SenderID)) s.Hits[hit.SenderID]=0;
                if (!s.Score.ContainsKey(hit.SenderID)) s.Score[hit.SenderID]=0;
                s.Hits[hit.SenderID] += 1;
                s.Score[hit.SenderID] += 10; // each hit gives 10 points

                // If shooter reached 10 hits -> add 100 bonus (but keep also per-hit points counted).
                if (s.Hits[hit.SenderID] >= 10)
                {
                    // grant 100 points bonus once per 10 hits; we ensure bonus only once by subtracting 10 from hits counter after awarding bonus                    
                    s.Score[hit.SenderID] += 100;
                    s.Hits[hit.SenderID] = 0; // reset hit counter for next bonus cycle
                }
                return (s.Hits[hit.SenderID], s.Score[hit.SenderID]);
            }
        }
    }
}
