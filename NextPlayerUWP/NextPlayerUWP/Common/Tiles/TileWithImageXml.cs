using System.Collections.Generic;
using NextPlayerUWPDataLayer.Constants;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace NextPlayerUWP.Common.Tiles
{
    public class TileWithImageXml : ITileXml
    {
        private string title;
        private string artist;
        private List<string> titles;
        private List<string> artists;
        private string coverUri;
        private string coverUriLarge;

        public TileWithImageXml(string title, string artist, string coverUri)
        {
            titles = new List<string>();
            this.title = title;
            artists = new List<string>();
            this.artist = artist;

            this.coverUri = coverUri;
            this.coverUriLarge = coverUri;
            if (string.IsNullOrEmpty(coverUri) || coverUriLarge.StartsWith("ms-appx"))
            {
                coverUri = AppConstants.AppLogoMedium;
                coverUriLarge = AppConstants.AppLogoLarge;
            }
        }

        public TileWithImageXml(List<string> titles, List<string> artists, string coverUri)
        {
            this.titles = titles;
            this.artists = artists;
            if (titles.Count == 3)
            {
                title = titles[1];
                artist = artists[1];
            }
            else if (titles.Count == 0)
            {
                title = "";
                artist = "";
            }
            else
            {
                title = titles[0];
                artist = artists[0];
            }

            this.coverUri = coverUri;
            this.coverUriLarge = coverUri;
            if (string.IsNullOrEmpty(coverUri) || coverUri.StartsWith("ms-appx"))
            {
                this.coverUri = AppConstants.AppLogoMedium;
                coverUriLarge = AppConstants.AppLogoLarge;
            }
        }

        private string GetContent()
        {
            string content;
            if (titles.Count < 3)
            {
                content = $@"
                <tile>
                    <visual>
                        <binding template=""TileMedium"" branding=""name"" displayName=""Next-Player"">
                            <image src=""{coverUri}"" placement=""peek""/>
                            <text hint-style=""caption"" hint-wrap=""true"">{title}</text>
                            <text hint-style=""captionSubtle"" hint-wrap=""true"">{artist}</text>
                        </binding>
                        <binding template=""TileWide"" branding=""name"" displayName=""Next-Player"">
                            <group>
                                <subgroup hint-weight=""1"">
                                    <image src=""{coverUri}"" hint-align=""stretch"" hint-removeMargin=""true""/>
                                </subgroup>
                                <subgroup hint-weight=""2"">
                                    <text hint-style=""caption"" hint-wrap=""true"">{title}</text>
                                    <text hint-style=""captionSubtle"" hint-wrap=""true"">{artist}</text>
                                </subgroup>
                            </group>
                        </binding>
                        <binding template=""TileLarge"" branding=""name"" displayName=""Next-Player"" hint-textStacking=""center"">
                            <image src=""{coverUri}"" placement=""background"" hint-overlay=""60""/>
                            <text hint-align=""center"" hint-style=""body"" hint-wrap=""true"">{title}</text>
                            <text hint-align=""center"" hint-style=""bodySubtle"" hint-wrap=""true"">{artist}</text>
                        </binding>
                    </visual>
                </tile>
                ";
            }
            else
            {
                content = $@"
                <tile>
                    <visual>
                        <binding template=""TileMedium"" branding=""name"" displayName=""Next-Player"">
                            <image src=""{coverUri}"" placement=""peek""/>
                            <text hint-style=""caption"" hint-wrap=""true"">{titles[1]}</text>
                            <text hint-style=""captionSubtle"" hint-wrap=""true"">{artists[1]}</text>
                        </binding>
                        <binding template=""TileWide"" branding=""name"" displayName=""Next-Player"">
                            <group>
                                <subgroup hint-weight=""1"">
                                    <image src=""{coverUri}"" hint-align=""stretch"" hint-removeMargin=""true""/>
                                </subgroup>
                                <subgroup hint-weight=""2"">
                                    <text hint-style=""caption"" hint-wrap=""true"">{title}</text>
                                    <text hint-style=""captionSubtle"" hint-wrap=""true"">{artist}</text>
                                </subgroup>
                            </group>
                        </binding>
                        <binding template=""TileLarge"" branding=""name"" displayName=""Next-Player"" hint-textStacking=""center"">
                            <image src=""{coverUri}"" placement=""background"" hint-overlay=""60""/>
                            <group>
				                <subgroup>
					                <text hint-style=""bodySubtle"" hint-wrap=""false"">{titles[0]}</text>
                                    <text hint-style=""captionSubtle"" hint-wrap=""false"">{artists[0]}</text>
                                </subgroup>
                            </group>
                            <text/>
                            <group>
				                <subgroup>
					                <text hint-style=""body"" hint-wrap=""false"">{titles[1]}</text>
                                    <text hint-style=""caption"" hint-wrap=""false"">{artists[1]}</text>
                                </subgroup>
                            </group>
                            <text/>
                            <group>
				                <subgroup>
					                <text hint-style=""bodySubtle"" hint-wrap=""false"">{titles[2]}</text>
                                    <text hint-style=""captionSubtle"" hint-wrap=""false"">{artists[2]}</text>
                                </subgroup>
                            </group>
                        </binding>
                    </visual>
                </tile>
                ";
            }

            return content;
        }

        public TileNotification GetNotification()
        {
            string content = GetContent();    
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            var notification = new TileNotification(doc);
            return notification;
        }
    }
}
