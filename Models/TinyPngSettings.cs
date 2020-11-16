namespace Etch.OrchardCore.TinyPNG.Models
{
    public class TinyPngSettings
    {
        public string ApiKey { get; set; }

        public bool HasApiKey
        {
            get { return !string.IsNullOrWhiteSpace(ApiKey); }
        }
    }
}
