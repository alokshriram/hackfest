namespace CalendarSyncApp.Model
{

    public static class AppState
    {
        public static string TenantId { get; set; }
        public static string ClientId { get; set; }
        public static string ClientSecret { get; set; }
        public static string GraphEndPoint { get; set; }
        public static string TokenEndPoint { get; set; }
        public static string BotEndPoint { get; set; }
        public static string Scope { get => "https://graph.microsoft.com/.default";}
        public static string Token { get; set; }
    }
}
