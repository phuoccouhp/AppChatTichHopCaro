// This file contains the corrected frmHome constructor
// Copy this code to replace the frmHome constructor in ChatAppClient/Forms/frmHome.cs

public frmHome(List<UserStatus> initialUsers)
{
    try
    {
        Logger.Info("[frmHome] Constructor starting...");
        
        InitializeComponent();
        Logger.Info("[frmHome] InitializeComponent completed");
        
        openChatControls = new Dictionary<string, ChatViewControl>();
        openGameForms = new Dictionary<string, frmCaroGame>();
        openTankGameForms = new Dictionary<string, frmTankGame>();
        _initialUsers = initialUsers;
        Logger.Info("[frmHome] Collections initialized");
        
        // Button events
        try
        {
            if (btnSettings != null)
            {
                btnSettings.MouseEnter += (s, e) => btnSettings.BackColor = Color.FromArgb(80, 83, 95);
                btnSettings.MouseLeave += (s, e) => btnSettings.BackColor = Color.FromArgb(55, 58, 70);
                Logger.Info("[frmHome] Button events assigned");
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"[frmHome] Button event error (non-critical): {ex.Message}");
        }
        
        // UI Layout setup
        try
        {
            Color separatorColor = Color.FromArgb(32, 34, 37);
            Logger.Info("[frmHome] Starting layout setup...");

            // Add line to header
            if (pnlHeader != null)
            {
                Panel lineHeader = new Panel { Height = 2, Dock = DockStyle.Bottom, BackColor = separatorColor };
                pnlHeader.Controls.Add(lineHeader);
            }

            // Setup sidebar
            if (pnlSidebar != null)
            {
                Panel lineVertical = new Panel { Width = 2, Dock = DockStyle.Right, BackColor = separatorColor };
                Panel lineSidebarTitle = new Panel { Height = 2, Dock = DockStyle.Top, BackColor = separatorColor };

                pnlSidebar.Controls.Clear();
                pnlSidebar.Controls.Add(lineVertical);
                
                if (flpFriendsList != null)
                {
                    pnlSidebar.Controls.Add(flpFriendsList);
                }

                if (pnlSearchBox != null)
                {
                    pnlSearchBox.Dock = DockStyle.Top;
                    pnlSidebar.Controls.Add(pnlSearchBox);
                }

                pnlSidebar.Controls.Add(lineSidebarTitle);

                if (lblFriendsTitle != null)
                {
                    lblFriendsTitle.AutoSize = false;
                    lblFriendsTitle.Height = 50;
                    lblFriendsTitle.Dock = DockStyle.Top;
                    lblFriendsTitle.TextAlign = ContentAlignment.MiddleLeft;
                    pnlSidebar.Controls.Add(lblFriendsTitle);
                }
            }
            
            Logger.Info("[frmHome] Layout setup completed");
        }
        catch (Exception ex)
        {
            Logger.Error($"[frmHome] Layout error: {ex.Message}", ex);
            throw;
        }
        
        Logger.Info("[frmHome] Constructor completed successfully");
    }
    catch (Exception ex)
    {
        Logger.Error($"[frmHome] Constructor fatal error: {ex.Message}", ex);
        throw;
    }
}
