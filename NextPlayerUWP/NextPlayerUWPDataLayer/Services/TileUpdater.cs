using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotificationsExtensions.Tiles;
using NextPlayerUWPDataLayer.Constants;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace NextPlayerUWPDataLayer.Services
{
    public class TileUpdater
    {
        public void UpdateAppTile(string title, string artist, string coverUri)
        {
            var notification = PrepareTileNotification(title, artist, coverUri);
            // Generate WinRT notification
            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Update(notification);
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateAppTileBG(string title, string artist, string coverUri)
        {
            var notification = PrepareTileNotification(title, artist, coverUri);
            // Generate WinRT notification
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication("App").Update(notification);
            }
            catch (Exception ex)
            {

            }
        }

        private TileNotification PrepareTileNotification(string title, string artist, string coverUri)
        {
            TileBindingContentAdaptive smallBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                   new TileImage()
                   {
                       Source = new TileImageSource(AppConstants.AppLogoSmall71),
                       RemoveMargin = true
                   }
                }
            };

            TileBindingContentAdaptive mediumBindingContent = new TileBindingContentAdaptive()
            {
                PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(coverUri)
                },

                Children =
                {
                    new TileText()
                    {
                        Text = title,
                        Wrap = true,
                        Style = TileTextStyle.Caption
                    },

                    new TileText()
                    {
                        Text = artist,
                        Wrap = true,
                        Style = TileTextStyle.CaptionSubtle
                    }
                }
            };

            TileBindingContentAdaptive wideBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileGroup()
                    {
                        Children =
                        {
                            new TileSubgroup()
                            {
                                Children =
                                {
                                   new TileImage()
                                   {
                                       Source = new TileImageSource(coverUri),
                                       Align = TileImageAlign.Stretch,
                                       RemoveMargin = true
                                   }
                                },
                                Weight = 1
                            },

                            new TileSubgroup()
                            {
                                Children =
                                {
                                    new TileText()
                                    {
                                        Text = title,
                                        Wrap = true,
                                        Style = TileTextStyle.Caption
                                    },

                                    new TileText()
                                    {
                                        Text = artist,
                                        Wrap = true,
                                        Style = TileTextStyle.CaptionSubtle
                                    }
                                },
                                Weight = 2
                            }
                        }
                    }
                }
            };

            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
            {
                BackgroundImage = new TileBackgroundImage()
                {
                    Source = new TileImageSource(coverUri),
                    Overlay = 60
                },

                Children =
                {
                    new TileText()
                    {
                        Text = title,
                        Wrap = true,
                        Style = TileTextStyle.Caption
                    },

                    new TileText()
                    {
                        Text = artist,
                        Wrap = true,
                        Style = TileTextStyle.CaptionSubtle
                    }
                }
            };

            TileBinding smallBinding = new TileBinding()
            {
                Branding = TileBranding.None,
                DisplayName = "Next-Player",
                Content = smallBindingContent
            };

            TileBinding mediumBinding = new TileBinding()
            {
                Branding = TileBranding.Name,
                DisplayName = "Next-Player",
                Content = mediumBindingContent
            };

            TileBinding wideBinding = new TileBinding()
            {
                Branding = TileBranding.Name,
                DisplayName = "Next-Player",
                Content = wideBindingContent
            };

            TileBinding largeBinding = new TileBinding()
            {
                Branding = TileBranding.Name,
                DisplayName = "Next-Player",
                Content = largeBindingContent
            };

            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = smallBinding,
                    TileMedium = mediumBinding,
                    TileWide = wideBinding,
                    TileLarge = largeBinding
                }
            };

            XmlDocument doc = content.GetXml();
            var notification = new TileNotification(doc);
            return notification;
        }
    }
}
