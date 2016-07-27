using System;
using NextPlayerUWPDataLayer.Constants;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NotificationsExtensions;
using NotificationsExtensions.Tiles;

namespace NextPlayerUWPDataLayer.Services
{
    public class TileUpdater
    {
        public static void ChangeAppTileToDefaultTransparent()
        {
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileSmall = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = @"Assets\Visual Assets\Square71\Small3.png",
                            }
                        }
                    },
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = @"Assets\Visual Assets\Square150\Medium3.png",
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = @"Assets\Visual Assets\Wide310\Wide3.png",
                            }
                        }
                    }
                }
            };

            var notification = new TileNotification(content.GetXml());
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication("App").Update(notification);
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateAppTile(string title, string artist, string coverUri)
        {
            var notification = PrepareTileNotification(title, artist, coverUri);
            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.Clear();
                updater.Update(notification);
            }
            catch (Exception ex)
            {

            }
        }

        public void UpdateAppTileBG(string title, string artist, string coverUri)
        {
            var notification = PrepareTileNotification(title, artist, coverUri);
            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication("App");
                updater.Clear();
                updater.Update(notification);
            }
            catch (Exception ex)
            {

            }
        }

        private TileNotification PrepareTileNotification(string title, string artist, string coverUri)
        {
            if (title == null) title = "";
            if (artist == null) artist = "";
            if (string.IsNullOrEmpty(coverUri)) coverUri = AppConstants.AlbumCover;

            //TileBindingContentAdaptive smallBindingContent = new TileBindingContentAdaptive()
            //{
            //    Children =
            //    {
            //       new AdaptiveImage()
            //       {
            //           Source = AppConstants.AppLogoSmall71,
            //           HintRemoveMargin = true
            //       }
            //    }
            //};

            TileBindingContentAdaptive mediumBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new AdaptiveText()
                    {
                        Text = title,
                        HintWrap = true,
                        HintStyle = AdaptiveTextStyle.Caption
                    },

                    new AdaptiveText()
                    {
                        Text = artist,
                        HintWrap = true,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                },

                PeekImage = new TilePeekImage()
                {
                    Source = coverUri,
                }
            };

            TileBindingContentAdaptive wideBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                   new AdaptiveImage()
                                   {
                                       Source = coverUri,
                                       HintAlign = AdaptiveImageAlign.Stretch,
                                       HintRemoveMargin = true
                                   }
                                },
                                HintWeight = 1
                            },

                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = title,
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.Caption
                                    },

                                    new AdaptiveText()
                                    {
                                        Text = artist,
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                },
                                HintWeight = 2
                            }
                        }
                    }
                }
            };

            //TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
            //{
            //    BackgroundImage = new TileBackgroundImage()
            //    {
            //        Source = coverUri,
            //        HintOverlay = 60
            //    },

            //    Children =
            //    {
            //        new AdaptiveText()
            //        {
            //            Text = title,
            //            HintWrap = true,
            //            HintStyle = AdaptiveTextStyle.Caption
            //        },

            //        new AdaptiveText()
            //        {
            //            Text = artist,
            //            HintWrap = true,
            //            HintStyle = AdaptiveTextStyle.CaptionSubtle
            //        }
            //    }
            //};

            //TileBinding smallBinding = new TileBinding()
            //{
            //    Branding = TileBranding.None,
            //    Content = smallBindingContent
            //};

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

            //TileBinding largeBinding = new TileBinding()
            //{
            //    Branding = TileBranding.Name,
            //    DisplayName = "Next-Player",
            //    Content = largeBindingContent
            //};

            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    //TileSmall = smallBinding,
                    TileMedium = mediumBinding,
                    TileWide = wideBinding,
                    //TileLarge = largeBinding
                }
            };

            XmlDocument doc = content.GetXml();
            var notification = new TileNotification(doc);
            return notification;
        }
    }
}
