namespace SlackApi.Core.Settings
{
    public class ApplicationSettings
    {
        public string? Name { get; set; }

        public double DefaultWeeklyBalance { get; set; }
        public double MaxTransactionSize { get; set; }
        public string? ApiToken { get; set; }
        public SlackSettings? SlackSettings { get; set; }
    }

    public class SlackSettings
    {
        public string? DefaultSlackWebookUri { get; set; }
        public string? OutgoingOAuthToken { get; set; }
        public string? IncomingToken { get; set; }
        public string? SigningSecret { get; set; }
        public string DefaultVersionNo { get; set; } = "v0";
    }
}
