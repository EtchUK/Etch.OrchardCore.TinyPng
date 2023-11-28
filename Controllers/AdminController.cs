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
        private static readonly char[] ExtensionSeperator = new char[] { ' ', ',' };
        private static readonly HashSet<string> EmptySet = new();

        private readonly IAuthorizationService _authorizationService;
        private readonly IChunkFileUploadService _chunkFileUploadService;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly ILogger _logger;
        private readonly IMediaFileStore _mediaFileStore;
        private readonly IMediaNameNormalizerService _mediaNameNormalizerService;
        private readonly MediaOptions _mediaOptions;
        private readonly IStringLocalizer S;
        private readonly ITinyPngService _tinyPngService;

        public AdminController(
            IAuthorizationService authorizationService,
            IChunkFileUploadService chunkFileUploadService,
            IContentTypeProvider contentTypeProvider,
            ILogger<AdminController> logger,
            IMediaFileStore mediaFileStore,
            IMediaNameNormalizerService mediaNameNormalizerService,
            IOptions<MediaOptions> options,
            IStringLocalizer<AdminController> stringLocalizer,
            ITinyPngService tinyPngService
            )
        {
            _authorizationService = authorizationService;
            _chunkFileUploadService = chunkFileUploadService;
            _contentTypeProvider = contentTypeProvider;
            _logger = logger;
            _mediaFileStore = mediaFileStore;
            _mediaNameNormalizerService = mediaNameNormalizerService;
            _mediaOptions = options.Value;
            S = stringLocalizer;
            _tinyPngService = tinyPngService;
        }

        [Route("/admin/media/upload")]
        [HttpPost]
        [MediaSizeLimit]
        public async Task<IActionResult> Upload(string path, string extensions)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMedia))
            {
                return Forbid();
            }

            var allowedExtensions = GetRequestedExtensions(extensions, true);

            return await _chunkFileUploadService.ProcessRequestAsync(
                Request,

                // We need this empty object because the frontend expects a JSON object in the response.
                (_, _, _) => Task.FromResult<IActionResult>(Ok(new { })),
                async (files) =>
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        path = string.Empty;
                    }

                    var result = new List<object>();

                    // Loop through each file in the request.
                    foreach (var file in files)
                    {
                        var extension = Path.GetExtension(file.FileName);

                        if (!allowedExtensions.Contains(extension))
                        {
                            result.Add(new
                            {
                                name = file.FileName,
                                size = file.Length,
                                folder = path,
                                error = S["This file extension is not allowed: {0}", extension].ToString()
                            });

                            if (_logger.IsEnabled(LogLevel.Information))
                            {
                                _logger.LogInformation("File extension not allowed: '{File}'", file.FileName);
                            }

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

                    return Ok(new { files = result.ToArray() });
                });
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
                mime = contentType ?? "application/octet-stream",
                mediaText = String.Empty,
                anchor = new { x = 0.5f, y = 0.5f },
                attachedFileName = String.Empty
            };
        }

        private HashSet<string> GetRequestedExtensions(string exts, bool fallback)
        {
            if (!string.IsNullOrWhiteSpace(exts))
            {
                var extensions = exts.Split(ExtensionSeperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var requestedExtensions = _mediaOptions.AllowedFileExtensions
                    .Intersect(extensions)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (requestedExtensions.Count > 0)
                {
                    return requestedExtensions;
                }
            }

            if (fallback)
            {
                return _mediaOptions.AllowedFileExtensions
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            return EmptySet;
        }
    }
}
