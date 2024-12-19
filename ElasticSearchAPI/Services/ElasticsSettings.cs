namespace ElasticSearchAPI.Services
{
    public class ElasticsSettings
    {
        public string? Url { get; set; }

        public string DefaultIndex { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }
    }
}