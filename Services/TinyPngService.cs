using Etch.OrchardCore.TinyPNG.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Settings;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinifyAPI;

namespace Etch.OrchardCore.TinyPNG.Services
{
    public class TinyPngService : ITinyPngService
    {
        #region Dependencies

        private readonly ILogger _logger;
        private readonly ISiteService _siteService;

        #endregion

        #region Constructor

        public TinyPngService(ILogger<TinyPngService> logger, ISiteService siteService)
        {
            _logger = logger;
            _siteService = siteService;
        }

        #endregion

        #region Implementation

        public async Task<Stream> OptimiseAsync(Stream stream, string fileName)
        {
            if (!CanOptimise(fileName))
            {
                return stream;
            }

            var tinyPngSettings = (await _siteService.GetSiteSettingsAsync()).As<TinyPngSettings>();

            if (!tinyPngSettings.HasApiKey)
            {
                _logger.LogWarning("Unable to compress images with TinyPNG without API key.");
                return stream;
            }

            Tinify.Key = tinyPngSettings.ApiKey;

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return new MemoryStream(await Tinify.FromBuffer(memoryStream.ToArray()).ToBuffer());
        }

        #endregion

        #region Helper Methods

        private bool CanOptimise(string fileName)
        {
            return new string[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase);
        }

        #endregion
    }
}
