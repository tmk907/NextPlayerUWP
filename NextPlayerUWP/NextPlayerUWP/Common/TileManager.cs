﻿using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace NextPlayerUWP.Common
{
    public class TileManager
    {
        /// <summary>
        /// Deletes unused tile images from App folder
        /// </summary>
        /// <returns></returns>
        public static async Task ManageSecondaryTileImages()
        {
            var tiles = await SecondaryTile.FindAllAsync();
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            var files = await localFolder.GetFilesAsync();
            bool exist;
            foreach (var file in files)
            {
                // image name = id + ".jpg"
                // secondary tile id = AppConstants.TileId + id.ToString()
                if (file.FileType.Equals(".jpg") && file.DisplayName.StartsWith(AppConstants.TileId))
                {
                    exist = false;
                    foreach (var tile in tiles)
                    {
                        if (tile.TileId == file.DisplayName) exist = true;
                    }
                    if (!exist)
                    {
                        await file.DeleteAsync(StorageDeleteOption.Default);
                    }
                }
            }
        }

        public static async Task CreateTile(MusicItem item)
        {
            int id = ApplicationSettingsHelper.ReadTileIdValue() + 1;
            string tileId = AppConstants.TileId + id.ToString();
            ApplicationSettingsHelper.SaveTileIdValue(id);
            string parameter = item.GetParameter();

            string displayName = AppConstants.AppName;
            string tileActivationArguments = parameter;
            Uri square150x150Logo = new Uri("ms-appx:///Assets/AppImages/Logo/Logo.png");

            SecondaryTile secondaryTile = new SecondaryTile(tileId,
                                                displayName,
                                                tileActivationArguments,
                                                square150x150Logo,
                                                TileSize.Square150x150);
            secondaryTile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/AppImages/WideLogo/WideLogo.png");
            secondaryTile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/AppImages/Square71x71Logo/Square71x71LogoTr.png");

            ResourceLoader loader = new ResourceLoader();
            string hasImage = "no";
            string name = "";
            string type = "";
            switch (MusicItem.ParseType(parameter))
            {
                case MusicItemTypes.album:
                    name = ((AlbumItem)item).Album;
                    type = loader.GetString("Album");
                    string imageName = await ImagesManager.SaveAlbumCover(((AlbumItem)item).AlbumParam, tileId);
                    if (imageName.Contains(tileId))
                    {
                        hasImage = "yes";
                    }
                    break;
                case MusicItemTypes.artist:
                    name = ((ArtistItem)item).Artist;
                    type = loader.GetString("Artist");
                    break;
                case MusicItemTypes.folder:
                    name = ((FolderItem)item).Folder;
                    type = loader.GetString("Folder");
                    break;
                case MusicItemTypes.genre:
                    name = ((GenreItem)item).Genre;
                    type = loader.GetString("Genre");
                    break;
                case MusicItemTypes.plainplaylist:
                    name = ((PlaylistItem)item).Name;
                    type = loader.GetString("Playlist");
                    break;
                case MusicItemTypes.smartplaylist:
                    name = ((PlaylistItem)item).Name;
                    type = loader.GetString("Playlist");
                    break;
                case MusicItemTypes.song:
                    name = ((SongItem)item).Artist + " - " + ((SongItem)item).Title;
                    type = loader.GetString("Song");
                    break;
            }

            ApplicationSettingsHelper.SaveTileData(new TileData() {
                Id = tileId,
                Name = name,
                Type = type,
                HasImage = hasImage }
            );

            App.OnNewTilePinned = UpdateNewSecondaryTile;

            await secondaryTile.RequestCreateAsync();
        }

        public static void UpdateNewSecondaryTile()
        {
            List<TileData> list = ApplicationSettingsHelper.ReadTileData();
            foreach(var tileData in list)
            {
                string name = tileData.Name;
                string id = tileData.Id;
                string type = tileData.Type;
                string hasImage = tileData.HasImage;

                XmlDocument tileXml;
                XmlDocument wideTile;

                if (hasImage == "yes")
                {
                    tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150PeekImageAndText02);
                    var tileImageAttributes = tileXml.GetElementsByTagName("image");
                    tileImageAttributes[0].Attributes.GetNamedItem("src").NodeValue = "ms-appdata:///local/" + id + ".jpg";
                }
                else
                {
                    tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text02);
                }

                XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
                tileTextAttributes[0].InnerText = type;
                tileTextAttributes[1].InnerText = name;

                wideTile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text09);
                XmlNodeList textAttr = wideTile.GetElementsByTagName("text");
                textAttr[0].InnerText = type;
                textAttr[1].InnerText = name;

                IXmlNode node = tileXml.ImportNode(wideTile.GetElementsByTagName("binding").Item(0), true);
                tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

                TileNotification tileNotification = new TileNotification(tileXml);
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(id).Update(tileNotification);
            }
        }

    }
}
