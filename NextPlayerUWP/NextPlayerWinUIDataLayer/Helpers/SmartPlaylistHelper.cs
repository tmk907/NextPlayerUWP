using NextPlayerWinUIDataLayer.Constants;
using System;

namespace NextPlayerWinUIDataLayer.Helpers
{
    public class SmartPlaylistHelper
    {
        public static bool IsDefaultSmartPlaylist(int id)
        {
            if (id == int.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.OstatnioDodane).ToString())) return true;
            if (id == int.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.OstatnioOdtwarzane).ToString())) return true;
            if (id == int.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.NajczesciejOdtwarzane).ToString())) return true;
            if (id == int.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.NajrzadziejOdtwarzane).ToString())) return true;
            if (id == int.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.NajgorzejOceniane).ToString())) return true;
            if (id == int.Parse(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.NajlepiejOceniane).ToString())) return true;
            return false;
        }
    }
}
