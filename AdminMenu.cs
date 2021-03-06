﻿using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace Etch.OrchardCore.TinyPNG
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Configuration"], configuration => configuration
                    .Add(S["Tiny PNG"], S["Tiny PNG"].PrefixPosition(), settings => settings
                    .AddClass("tinyPNG").Id("tinyPNG")
                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = Constants.GroupId })
                        .Permission(TinyPngPermissions.ManageTinyPng)
                        .LocalNav())
                    );
            }

            return Task.CompletedTask;
        }
    }
}
