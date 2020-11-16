using Etch.OrchardCore.TinyPNG.Models;
using Etch.OrchardCore.TinyPNG.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Etch.OrchardCore.TinyPNG.Drivers
{
    public class TinyPngSettingsDisplayDriver : SectionDisplayDriver<ISite, TinyPngSettings>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public TinyPngSettingsDisplayDriver(
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(TinyPngSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, TinyPngPermissions.ManageTinyPng))
            {
                return null;
            }

            return Initialize<TinyPngSettingsViewModel>("TinyPngSettings_Edit", model =>
            {
                model.ApiKey = section.ApiKey;
            }).Location("Content:3").OnGroup(Constants.GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(TinyPngSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, TinyPngPermissions.ManageTinyPng))
            {
                return null;
            }

            if (context.GroupId == Constants.GroupId)
            {
                var model = new TinyPngSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.ApiKey = model.ApiKey;
            }

            return await EditAsync(section, context);
        }
    }
}
