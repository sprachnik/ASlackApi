namespace SlackApi.Core.Settings
{
    public class ConnectionStrings
    {
        public string? SqlConnection { get; set; }
        public string? RedisCache { get; set; }
        public string? TableStorage { get; set; }
    }
}
