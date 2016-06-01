﻿using System;
using NotificationsExtensions.Tiles;
using NextPlayerUWPDataLayer.Constants;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using NotificationsExtensions;

namespace NextPlayerUWPDataLayer.Services
{
    public class TileUpdater
    {
        public void UpdateAppTile(string title, string artist, string coverUri)
        {
            var notification = PrepareTileNotification(title, artist, coverUri);
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
            if (title == null) title = "";
            if (artist == null) artist = "";
            if (string.IsNullOrEmpty(coverUri)) coverUri = AppConstants.AlbumCover;

            TileBindingContentAdaptive smallBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                   new AdaptiveImage()
                   {
                       Source = AppConstants.AppLogoSmall71,
                       HintRemoveMargin = true
                   }
                }
            };

            TileBindingContentAdaptive mediumBindingContent = new TileBindingContentAdaptive()
            {
                PeekImage = new TilePeekImage()
                {
                    Source = coverUri
                },

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

            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
            {
                BackgroundImage = new TileBackgroundImage()
                {
                    Source = coverUri,
                    HintOverlay = 60
                },

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