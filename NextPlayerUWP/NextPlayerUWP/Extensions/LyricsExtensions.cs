using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;

namespace NextPlayerUWP.Extensions
{
    public class LyricsExtensions : BaseExtensionsHelper, IExtensionsHelper
    {
        public LyricsExtensions()
        {
            appExtensionName = "Next-Player-Lyrics";
            catalog = AppExtensionCatalog.Open(appExtensionName);
            catalog.PackageInstalled += Catalog_PackageInstalled;
            catalog.PackageUninstalling += Catalog_PackageUninstalling;
        }

        private void Catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
            extensions = null;
        }

        private void Catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
            extensions = null;
        }

        public async Task<List<AppExtensionInfo>> GetExtensionsInfo()
        {
            if (extensions == null)
            {
                extensions = new List<AppExtensionInfo>();
                foreach (var extension in await catalog.FindAllAsync())
                {
                    var properties = await extension.GetExtensionPropertiesAsync() as PropertySet;

                    if (properties != null && properties.ContainsKey("Service"))
                    {
                        PropertySet service = properties["Service"] as PropertySet;
                        extensions.Add(new AppExtensionInfo()
                        {
                            DisplayName = extension.DisplayName,
                            Id = extension.Id,
                            PackageName = extension.Package.Id.FamilyName,
                            ServiceName = service["#text"].ToString(),
                            Type = ExtensionType.Lyrics,
                            Priority = -1
                        });
                    }
                }
                ApplyPriorities(extensions);
            }
            return extensions;
        }
    }
}
