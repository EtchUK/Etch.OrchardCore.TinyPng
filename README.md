# Etch.OrchardCore.TinyPNG

[Orchard Core](https://github.com/orchardcms/orchardcore) module that uses [TinyPNG](https://tinypng.com/) to optimise images uploaded to media library.

## Build Status

[![NuGet](https://img.shields.io/nuget/v/Etch.OrchardCore.TinyPNG.svg)](https://www.nuget.org/packages/Etch.OrchardCore.TinyPNG)

## Orchard Core Reference

This module is referencing a stable build of Orchard Core ([`1.4.0`](https://www.nuget.org/packages/OrchardCore.Module.Targets/1.4.0)).

## Installing

This module is available on [NuGet](https://www.nuget.org/packages/Etch.OrchardCore.TinyPNG). Add a reference to your Orchard Core web project via the NuGet package manager. Search for "Etch.OrchardCore.TinyPNG", ensuring include prereleases is checked.

Alternatively you can [download the source](https://github.com/etchuk/Etch.OrchardCore.TinyPNG/archive/master.zip) or clone the repository to your local machine. Add the project to your solution that contains an Orchard Core project and add a reference to Etch.OrchardCore.TinyPNG.

## Usage

Enable "TinyPNG" feature within the admin dashboard. Next you'll need an [API key from TinyPNG](https://tinypng.com/developers) by registering with your name and email address. Under the "Configuration" menu option will be "TinyPNG", which is where you can set your API key. Once configured, any JPG or PNG files uploaded to the media library will be compressed by TinyPNG.

## Packaging

When the module is compiled (using `dotnet build`) it's configured to generate a `.nupkg` file (this can be found in `\bin\Debug\` or `\bin\Release`).
