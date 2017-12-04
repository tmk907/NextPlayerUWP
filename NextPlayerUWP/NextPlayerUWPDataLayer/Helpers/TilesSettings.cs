using System;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class TilesSettings
    {
        private IApplicationSettings appSettings;

        public TilesSettings(IApplicationSettings applicationSettings)
        {
            appSettings = applicationSettings;
        }

        public int ReadTileIdValue()
        {
            object value = appSettings.ReadSettingsValue(SettingsKeys.TileIdValue);
            if (value != null)
            {
                return Int32.Parse(value.ToString());
            }
            else
            {
                SaveTileIdValue(1);
                return 1;
            }
        }

        public void SaveTileIdValue(int id)
        {
            appSettings.SaveSettingsValue(SettingsKeys.TileIdValue, id);
        }

        private const string separator = "|!@#$|";

        public void SaveTileData(TileData tileData)
        {
            string val = (appSettings.ReadSettingsValue(SettingsKeys.TileId) ?? "").ToString() + tileData.Id + separator;
            appSettings.SaveSettingsValue(SettingsKeys.TileId, val);
            val = (appSettings.ReadSettingsValue(SettingsKeys.TileName) ?? "").ToString() + tileData.Name + separator;
            appSettings.SaveSettingsValue(SettingsKeys.TileName, val);
            val = (appSettings.ReadSettingsValue(SettingsKeys.TileType) ?? "").ToString() + tileData.Type + separator;
            appSettings.SaveSettingsValue(SettingsKeys.TileType, val);
            val = (appSettings.ReadSettingsValue(SettingsKeys.TileImage) ?? "").ToString() + tileData.HasImage + separator;
            appSettings.SaveSettingsValue(SettingsKeys.TileImage, val);
        }

        public List<TileData> ReadTileData()
        {
            List<TileData> list = new List<TileData>();
            object id = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileId);
            object name = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileName);
            object type = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileType);
            object image = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileImage);

            if (id != null)
            {
                try
                {

                }
                catch (Exception ex)
                {
                    Diagnostics.Logger2.Current.WriteMessage("ReadTileData " + Environment.NewLine + ex.Message, NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                }
                string[] ids = id.ToString().Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                string[] names = name.ToString().Split(new string[] { separator }, StringSplitOptions.None);
                string[] types = type.ToString().Split(new string[] { separator }, StringSplitOptions.None);
                string[] images = image.ToString().Split(new string[] { separator }, StringSplitOptions.None);

                for (int i = 0; i < ids.Length; i++)
                {
                    list.Add(new TileData()
                    {
                        Id = ids[i],
                        Name = names[i],
                        Type = types[i],
                        HasImage = images[i]
                    });
                }
            }
            return list;
        }
    }
}
