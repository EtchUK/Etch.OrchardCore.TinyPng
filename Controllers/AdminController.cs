using Etch.OrchardCore.TinyPNG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Etch.OrchardCore.TinyPNG.Controllers
{
    public class AdminController : Controller
    {
        private readonly HashSet<string> _allowedFileExtensions;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly ILogger _logger;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
        private readonly IStringLocalizer S;
        private readonly ITinyPngService _tinyPngService;

        public AdminController(
            IAuthorizationService authorizationService,
            IContentTypeProvider contentTypeProvider,
            ILogger<AdminController> logger,
            IMediaFileStore mediaFileStore,
            IMediaNameNormalizerService mediaNameNormalizerService,
            IOptions<MediaOptions> options,
            IStringLocalizer<AdminController> stringLocalizer,
            ITinyPngService tinyPngService
            )
        {
            _allowedFileExtensions = options.Value.AllowedFileExtensions;
            _authorizationService = authorizationService;
            _contentTypeProvider = contentTypeProvider;
            _logger = logger;
            _mediaFileStore = mediaFileStore;
            _mediaNameNormalizerService = mediaNameNormalizerService;
            S = stringLocalizer;
            _tinyPngService = tinyPngService;
        }

        [Route("/admin/media/upload")]
        [HttpPost]
        [MediaSizeLimit]
        public async Task<ActionResult<object>> Upload(
            string path,
            ICollection<IFormFile> files)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(path))
            {
                path = "";
            }

            var result = new List<object>();

            // Loop through each file in the request
            foreach (var file in files)
            {
                // TODO: support clipboard

                if (!_allowedFileExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(new
                    {
                        name = file.FileName,
                        size = file.Length,
                        folder = path,
                        error = S["This file extension is not allowed: {0}", Path.GetExtension(file.FileName)].ToString()
                    });

                    _logger.LogInformation("File extension not allowed: '{File}'", file.FileName);

                    continue;
                }

                var fileName = _mediaNameNormalizerService.NormalizeFileName(file.FileName);

                Stream stream = null;
                try
                {
                    var mediaFilePath = _mediaFileStore.Combine(path, fileName);
                    stream = file.OpenReadStream();
                    mediaFilePath = await _mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, await _tinyPngService.OptimiseAsync(stream, file.FileName));

                    var mediaFile = await _mediaFileStore.GetFileInfoAsync(mediaFilePath);

                    result.Add(CreateFileResult(mediaFile));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while uploading a media");

                    result.Add(new
                    {
                        name = fileName,
                        size = file.Length,
                        folder = path,
                        error = ex.Message
                    });
                }
                finally
                {
                    stream?.Dispose();
                }
            }

            return new { files = result.ToArray() };
        }

        private object CreateFileResult(IFileStoreEntry mediaFile)
        {
            _contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

            return new
            {
                name = mediaFile.Name,
                size = mediaFile.Length,
                folder = mediaFile.DirectoryPath,
                url = _mediaFileStore.MapPathToPublicUrl(mediaFile.Path),
                mediaPath = mediaFile.Path,
                mime = contentType ?? "application/octet-stream"
            };
        }
    }
}
