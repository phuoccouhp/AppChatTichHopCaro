using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChatAppClient.Helpers
{
    /// <summary>
    /// Service ?? t?i và cache Fluent UI Emoji t? CDN
    /// </summary>
    public static class FluentEmojiService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly ConcurrentDictionary<string, Image> _imageCache = new ConcurrentDictionary<string, Image>();
        private static readonly string _cacheFolder;

        // Base URL cho Fluent UI Emoji - s? d?ng CDN c?a Microsoft
        private const string FLUENT_EMOJI_CDN = "https://em-content.zobj.net/source/microsoft-teams/363/";

        static FluentEmojiService()
        {
          // T?o th? m?c cache
  _cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChatApp", "EmojiCache");
         Directory.CreateDirectory(_cacheFolder);
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
     _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
        }

        /// <summary>
 /// Danh sách emoji ph? bi?n v?i mã Unicode hex
    /// Key: Unicode emoji, Value: (Mã hex cho URL, Danh m?c)
        /// </summary>
        public static readonly Dictionary<string, (string HexCode, string Category)> PopularEmojis = new Dictionary<string, (string, string)>
        {
            // ?? Smileys & Emotion
            { "??", ("1f600", "Smileys") },
     { "??", ("1f603", "Smileys") },
  { "??", ("1f604", "Smileys") },
            { "??", ("1f601", "Smileys") },
 { "??", ("1f606", "Smileys") },
            { "??", ("1f605", "Smileys") },
            { "??", ("1f923", "Smileys") },
    { "??", ("1f602", "Smileys") },
        { "??", ("1f642", "Smileys") },
  { "??", ("1f60a", "Smileys") },
     { "??", ("1f607", "Smileys") },
       { "??", ("1f970", "Smileys") },
      { "??", ("1f60d", "Smileys") },
      { "??", ("1f929", "Smileys") },
  { "??", ("1f618", "Smileys") },
            { "??", ("1f617", "Smileys") },
            { "??", ("1f61a", "Smileys") },
     { "??", ("1f60b", "Smileys") },
        { "??", ("1f61b", "Smileys") },
         { "??", ("1f61c", "Smileys") },
            { "??", ("1f92a", "Smileys") },
    { "??", ("1f61d", "Smileys") },
         { "??", ("1f911", "Smileys") },
       { "??", ("1f917", "Smileys") },
  { "??", ("1f92d", "Smileys") },
    { "??", ("1f92b", "Smileys") },
            { "??", ("1f914", "Smileys") },
  { "??", ("1f60e", "Smileys") },
    { "??", ("1f973", "Smileys") },
{ "??", ("1f60f", "Smileys") },
          
            // ?? Sad & Crying
{ "??", ("1f60c", "Sad") },
 { "??", ("1f614", "Sad") },
    { "??", ("1f62a", "Sad") },
  { "??", ("1f924", "Sad") },
 { "??", ("1f634", "Sad") },
   { "??", ("1f637", "Sad") },
      { "??", ("1f912", "Sad") },
    { "??", ("1f915", "Sad") },
            { "??", ("1f922", "Sad") },
       { "??", ("1f92e", "Sad") },
        { "??", ("1f975", "Sad") },
  { "??", ("1f976", "Sad") },
     { "??", ("1f974", "Sad") },
    { "??", ("1f635", "Sad") },
  { "??", ("1f92f", "Sad") },
   { "??", ("1f615", "Sad") },
         { "??", ("1f61f", "Sad") },
     { "??", ("1f641", "Sad") },
  { "??", ("1f62e", "Sad") },
            { "??", ("1f62f", "Sad") },
            { "??", ("1f632", "Sad") },
    { "??", ("1f633", "Sad") },
     { "??", ("1f97a", "Sad") },
         { "??", ("1f622", "Sad") },
     { "??", ("1f62d", "Sad") },
     { "??", ("1f631", "Sad") },
      { "??", ("1f616", "Sad") },
            { "??", ("1f61e", "Sad") },
     { "??", ("1f629", "Sad") },
          { "??", ("1f62b", "Sad") },
            
        // ?? Angry
  { "??", ("1f624", "Angry") },
     { "??", ("1f621", "Angry") },
  { "??", ("1f620", "Angry") },
            { "??", ("1f92c", "Angry") },
            { "??", ("1f47f", "Angry") },
   { "??", ("1f480", "Angry") },
       { "??", ("1f479", "Angry") },
 { "??", ("1f47a", "Angry") },
   { "??", ("1f4a9", "Angry") },
   { "??", ("1f921", "Angry") },
     
       // ?? Gestures
            { "??", ("1f44d", "Gestures") },
            { "??", ("1f44e", "Gestures") },
         { "??", ("1f44f", "Gestures") },
      { "??", ("1f64c", "Gestures") },
  { "??", ("1f450", "Gestures") },
            { "??", ("1f91d", "Gestures") },
            { "??", ("1f64f", "Gestures") },
  { "??", ("1f4aa", "Gestures") },
            { "??", ("1f44b", "Gestures") },
   { "?", ("270b", "Gestures") },
            { "??", ("1f44c", "Gestures") },
   { "??", ("270c", "Gestures") },
   { "??", ("1f91e", "Gestures") },
         { "??", ("1f91f", "Gestures") },
 { "??", ("1f918", "Gestures") },
            { "??", ("1f919", "Gestures") },
            { "??", ("1f448", "Gestures") },
{ "??", ("1f449", "Gestures") },
            { "??", ("1f446", "Gestures") },
        { "??", ("1f447", "Gestures") },
    { "??", ("1f44a", "Gestures") },
 { "?", ("270a", "Gestures") },
            { "??", ("1f91b", "Gestures") },
            { "??", ("1f91c", "Gestures") },
          
   // ?? Hearts & Love
       { "??", ("2764", "Hearts") },
            { "??", ("1f9e1", "Hearts") },
         { "??", ("1f49b", "Hearts") },
    { "??", ("1f49a", "Hearts") },
            { "??", ("1f499", "Hearts") },
            { "??", ("1f49c", "Hearts") },
  { "??", ("1f5a4", "Hearts") },
            { "??", ("1f90d", "Hearts") },
     { "??", ("1f90e", "Hearts") },
            { "??", ("1f494", "Hearts") },
            { "??", ("1f495", "Hearts") },
  { "??", ("1f49e", "Hearts") },
            { "??", ("1f493", "Hearts") },
            { "??", ("1f497", "Hearts") },
         { "??", ("1f496", "Hearts") },
            { "??", ("1f498", "Hearts") },
            { "??", ("1f49d", "Hearts") },
            { "??", ("1f49f", "Hearts") },
     
         // ?? Celebration
   { "??", ("1f389", "Celebration") },
            { "??", ("1f38a", "Celebration") },
   { "??", ("1f381", "Celebration") },
            { "??", ("1f382", "Celebration") },
  { "??", ("1f388", "Celebration") },
    { "??", ("1f380", "Celebration") },
            { "??", ("1f3c6", "Celebration") },
     { "??", ("1f947", "Celebration") },
  { "??", ("1f948", "Celebration") },
            { "??", ("1f949", "Celebration") },
       { "??", ("1f3c5", "Celebration") },
         { "???", ("1f396", "Celebration") },
            { "???", ("1f397", "Celebration") },
  { "??", ("1f384", "Celebration") },
{ "??", ("1f383", "Celebration") },
  { "??", ("1f386", "Celebration") },
            { "??", ("1f387", "Celebration") },
      { "?", ("2728", "Celebration") },
          { "??", ("1f38b", "Celebration") },
   { "??", ("1f38d", "Celebration") },
            
    // ?? Symbols & Objects
            { "??", ("1f525", "Symbols") },
       { "?", ("2b50", "Symbols") },
         { "??", ("1f31f", "Symbols") },
        { "??", ("1f4ab", "Symbols") },
            { "??", ("1f4a5", "Symbols") },
       { "??", ("1f4a2", "Symbols") },
      { "??", ("1f4a6", "Symbols") },
            { "??", ("1f4a8", "Symbols") },
            { "??", ("1f4a3", "Symbols") },
       { "??", ("1f4ac", "Symbols") },
{ "??", ("1f4ad", "Symbols") },
  { "??", ("1f4a4", "Symbols") },
      { "??", ("1f4af", "Symbols") },
        { "??", ("1f3b5", "Symbols") },
   { "??", ("1f3b6", "Symbols") },
  { "??", ("1f514", "Symbols") },
    { "??", ("1f515", "Symbols") },
      { "??", ("1f3a4", "Symbols") },
 { "??", ("1f3a7", "Symbols") },
{ "??", ("1f3ae", "Symbols") },
         
       // ?? Food & Drink
            { "??", ("1f355", "Food") },
            { "??", ("1f354", "Food") },
          { "??", ("1f35f", "Food") },
       { "??", ("1f32d", "Food") },
      { "??", ("1f37f", "Food") },
            { "??", ("1f369", "Food") },
            { "??", ("1f366", "Food") },
        { "??", ("1f370", "Food") },
     { "??", ("1f9c1", "Food") },
          { "??", ("1f36b", "Food") },
    { "??", ("1f36d", "Food") },
     { "??", ("1f36c", "Food") },
{ "?", ("2615", "Food") },
            { "??", ("1f375", "Food") },
    { "??", ("1f964", "Food") },
            { "??", ("1f37a", "Food") },
            { "??", ("1f37b", "Food") },
            { "??", ("1f942", "Food") },
{ "??", ("1f377", "Food") },
       { "??", ("1f943", "Food") },
  
            // ?? Animals
      { "??", ("1f431", "Animals") },
    { "??", ("1f436", "Animals") },
            { "??", ("1f42d", "Animals") },
      { "??", ("1f439", "Animals") },
    { "??", ("1f430", "Animals") },
            { "??", ("1f98a", "Animals") },
            { "??", ("1f43b", "Animals") },
        { "??", ("1f43c", "Animals") },
   { "??", ("1f428", "Animals") },
            { "??", ("1f42f", "Animals") },
            { "??", ("1f981", "Animals") },
  { "??", ("1f42e", "Animals") },
       { "??", ("1f437", "Animals") },
     { "??", ("1f438", "Animals") },
       { "??", ("1f435", "Animals") },
  { "??", ("1f648", "Animals") },
            { "??", ("1f649", "Animals") },
            { "??", ("1f64a", "Animals") },
            { "??", ("1f414", "Animals") },
            { "??", ("1f427", "Animals") },
          { "??", ("1f426", "Animals") },
            { "??", ("1f984", "Animals") },
            { "??", ("1f41d", "Animals") },
          { "??", ("1f98b", "Animals") },
     };

      /// <summary>
        /// L?y danh sách các danh m?c emoji
        /// </summary>
   public static List<string> GetCategories()
        {
            return new List<string> { "Smileys", "Sad", "Angry", "Gestures", "Hearts", "Celebration", "Symbols", "Food", "Animals" };
        }

        /// <summary>
        /// L?y emoji theo danh m?c
    /// </summary>
        public static List<string> GetEmojisByCategory(string category)
        {
            var result = new List<string>();
        foreach (var kvp in PopularEmojis)
         {
      if (kvp.Value.Category == category)
  {
       result.Add(kvp.Key);
        }
            }
          return result;
   }

        /// <summary>
        /// T?i ?nh emoji t? cache ho?c CDN
        /// </summary>
        public static async Task<Image?> GetEmojiImageAsync(string emoji, int size = 32)
  {
   if (!PopularEmojis.TryGetValue(emoji, out var info))
        return null;

    string cacheKey = $"{emoji}_{size}";
   
         // Ki?m tra memory cache
     if (_imageCache.TryGetValue(cacheKey, out var cachedImage))
            return cachedImage;

            // Ki?m tra file cache
 string cacheFile = Path.Combine(_cacheFolder, $"{info.HexCode}_{size}.png");
   if (File.Exists(cacheFile))
 {
    try
     {
  using var fs = new FileStream(cacheFile, FileMode.Open, FileAccess.Read);
         var img = Image.FromStream(fs);
     var resized = new Bitmap(img, new Size(size, size));
              _imageCache[cacheKey] = resized;
        return resized;
                }
       catch { }
     }

            // T?i t? CDN
            try
   {
    // URL format: https://em-content.zobj.net/source/microsoft-teams/363/grinning-face_1f600.png
           string url = $"{FLUENT_EMOJI_CDN}{info.HexCode}.png";
       var data = await _httpClient.GetByteArrayAsync(url);
  
          // L?u file cache
   string originalCacheFile = Path.Combine(_cacheFolder, $"{info.HexCode}.png");
       await File.WriteAllBytesAsync(originalCacheFile, data);
          
                using var ms = new MemoryStream(data);
                var original = Image.FromStream(ms);
     var resized = new Bitmap(original, new Size(size, size));
              _imageCache[cacheKey] = resized;
     return resized;
       }
  catch (Exception ex)
            {
     System.Diagnostics.Debug.WriteLine($"Failed to load emoji {emoji}: {ex.Message}");
        return null;
            }
        }

        /// <summary>
        /// Preload t?t c? emoji ph? bi?n
   /// </summary>
     public static async Task PreloadEmojisAsync(int size = 32, IProgress<int>? progress = null)
      {
            int count = 0;
       int total = PopularEmojis.Count;
            
    foreach (var emoji in PopularEmojis.Keys)
{
   await GetEmojiImageAsync(emoji, size);
     count++;
           progress?.Report((count * 100) / total);
            }
   }

        /// <summary>
        /// Xóa cache
        /// </summary>
  public static void ClearCache()
   {
     _imageCache.Clear();
   if (Directory.Exists(_cacheFolder))
            {
         try
                {
      Directory.Delete(_cacheFolder, true);
 Directory.CreateDirectory(_cacheFolder);
   }
   catch { }
            }
        }
    }
}
