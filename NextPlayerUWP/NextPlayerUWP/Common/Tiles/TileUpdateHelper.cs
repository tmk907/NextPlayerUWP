using System;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Constants;

namespace NextPlayerUWP.Common.Tiles
{
    public class TileUpdateHelper
    {
        public static void ClearTile()
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task UpdateAppTile(string title, string artist, string coverUri)
        {
            TileImageHelper ti = new TileImageHelper();
            try
            {
                coverUri = await ti.PrepareImage(coverUri);
            }
            catch (Exception ex)
            {
                coverUri = AppConstants.SongCoverBig;
            }
            var factory = new TileContentFactory(title, artist, coverUri);
            var notification = PrepareTileNotification(factory);
            SendNotification(notification);
        }

        public async Task UpdateAppTile(List<string> titles, List<string> artists, string coverUri)
        {
            TileImageHelper ti = new TileImageHelper();
            try
            {
                coverUri = await ti.PrepareImage(coverUri);
            }
            catch (Exception ex)
            {
                coverUri = AppConstants.SongCoverBig;
            }
            TileContentFactory factory = new TileContentFactory(titles, artists, coverUri);
            TileNotification notification = PrepareTileNotification(factory);
            SendNotification(notification);
        }

        private void SendNotification(TileNotification notification)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SendNotification tile");
                var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication("App");//.Update(notification);
                tileUpdater.Update(notification);
            }
            catch (Exception ex)
            {

            }
        }

        private TileNotification PrepareTileNotification(TileContentFactory contentFactory)
        {
            TileBindingContentAdaptive mediumBindingContent = contentFactory.GetMediumBindingContent();
            TileBindingContentAdaptive wideBindingContent = contentFactory.GetWideBindingContent();
            TileBindingContentAdaptive largeBindingContent = contentFactory.GetLargeBindingContent();

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
                    //TileSmall = smallBinding,
                    TileMedium = mediumBinding,
                    TileWide = wideBinding,
                    TileLarge = largeBinding
                }
            };

            XmlDocument doc = content.GetXml();
            var notification = new TileNotification(doc);
            return notification;
        }

        //public static void ChangeAppTileToDefaultTransparent()
        //{
        //    TileContent content = new TileContent()
        //    {
        //        Visual = new TileVisual()
        //        {
        //            TileSmall = new TileBinding()
        //            {
        //                Content = new TileBindingContentAdaptive()
        //                {
        //                    BackgroundImage = new TileBackgroundImage()
        //                    {
        //                        Source = @"Assets\Visual Assets\Square71\Small3.png",
        //                    }
        //                }
        //            },
        //            TileMedium = new TileBinding()
        //            {
        //                Content = new TileBindingContentAdaptive()
        //                {
        //                    BackgroundImage = new TileBackgroundImage()
        //                    {
        //                        Source = @"Assets\Visual Assets\Square150\Medium3.png",
        //                    }
        //                }
        //            },
        //            TileWide = new TileBinding()
        //            {
        //                Content = new TileBindingContentAdaptive()
        //                {
        //                    BackgroundImage = new TileBackgroundImage()
        //                    {
        //                        Source = @"Assets\Visual Assets\Wide310\Wide3.png",
        //                    }
        //                }
        //            }
        //        }
        //    };

        //    var notification = new TileNotification(content.GetXml());
        //    try
        //    {
        //        TileUpdateManager.CreateTileUpdaterForApplication("App").Update(notification);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }
}
