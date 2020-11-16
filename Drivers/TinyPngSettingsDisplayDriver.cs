using Etch.OrchardCore.TinyPNG.Models;
using Etch.OrchardCore.TinyPNG.ViewModels;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using System.Threading.Tasks;

namespace Etch.OrchardCore.TinyPNG.Drivers
{
    public class TinyPngSettingsDisplayDriver : SectionDisplayDriver<ISite, TinyPngSettings>
    {
        public override IDisplayResult Edit(TinyPngSettings section, BuildEditorContext context)
        {
            return Initialize<TinyPngSettingsViewModel>("TinyPngSettings_Edit", model =>
            {
                model.ApiKey = section.ApiKey;
            }).Location("Content:3").OnGroup(Constants.GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(TinyPngSettings section, BuildEditorContext context)
        {
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
