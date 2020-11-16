using OrchardCore.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etch.OrchardCore.TinyPNG
{
    public class TinyPngPermissions : IPermissionProvider
    {
        public static readonly Permission ManageTinyPng = new Permission("ManageTinyPNG", "Manage TinyPNG");

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[] { ManageTinyPng }.AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = new[] { ManageTinyPng }
                }
            };
        }
    }
}
