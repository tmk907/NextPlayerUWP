using Microsoft.Toolkit.Uwp.Notifications;
using NextPlayerUWPDataLayer.Constants;
using System.Collections.Generic;

namespace NextPlayerUWP.Common.Tiles
{
    public class TileContentFactory
    {
        private string title;
        private string artist;
        private List<string> titles;
        private List<string> artists;
        private string coverUri;
        private string coverUriLarge;

        public TileContentFactory(string title, string artist, string coverUri)
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

        public TileContentFactory(List<string> titles, List<string> artists, string coverUri)
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

        public TileBindingContentAdaptive GetSmallBindingContent()
        {
            return GetSmallBindingContent(title, artist, coverUri);
        }

        public TileBindingContentAdaptive GetMediumBindingContent()
        {
            return GetMediumBindingContent(title, artist, coverUri);
        }

        public TileBindingContentAdaptive GetWideBindingContent()
        {
            return GetWideBindingContent(title, artist, coverUri);
        }

        public TileBindingContentAdaptive GetLargeBindingContent()
        {
            if (titles.Count < 3)
            {
                return GetLargeBindingContent(title, artist, coverUriLarge);
            }
            else
            {
                return GetLargeBindingContent(titles, artists, coverUriLarge);
            }
        }

        private TileBindingContentAdaptive GetSmallBindingContent(string title, string artist, string coverUri)
        {
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
            return smallBindingContent;
        }

        private TileBindingContentAdaptive GetMediumBindingContent(string title, string artist, string coverUri)
        {
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
            return mediumBindingContent;
        }

        private TileBindingContentAdaptive GetWideBindingContent(string title, string artist, string coverUri)
        {
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
            return wideBindingContent;
        }

        private TileBindingContentAdaptive GetLargeBindingContentBackgroundImage(string title, string artist, string coverUri)
        {
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
            return largeBindingContent;
        }

        private TileBindingContentAdaptive GetLargeBindingContent(string title, string artist, string coverUri)
        {
            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup() { HintWeight = 1 },
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                        Source = coverUri,
                                    }
                                },
                                HintWeight = 3
                            },
                            new AdaptiveSubgroup() { HintWeight = 1 }
                        }
                    },
                    new AdaptiveText()
                    {
                        Text = title,
                        HintWrap = true,
                        HintStyle = AdaptiveTextStyle.Body,
                        HintAlign = AdaptiveTextAlign.Center
                    },

                    new AdaptiveText()
                    {
                        Text = artist,
                        HintWrap = true,
                        HintStyle = AdaptiveTextStyle.BodySubtle,
                        HintAlign = AdaptiveTextAlign.Center
                    }
                },
                TextStacking = TileTextStacking.Center
            };
            return largeBindingContent;
        }

        private TileBindingContentAdaptive GetLargeBindingContent(List<string> titles, List<string> artists, string coverUri)
        {
            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
            {
                BackgroundImage = new TileBackgroundImage()
                {
                    Source = coverUri,
                    HintOverlay = 60
                },

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
                                    new AdaptiveText()
                                    {
                                        Text = titles[0],
                                        HintWrap = false,
                                        HintStyle = AdaptiveTextStyle.BodySubtle
                                    },

                                    new AdaptiveText()
                                    {
                                        Text = artists[0],
                                        HintWrap = false,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }                                
                            }
                        }
                    },
                    new AdaptiveText() { },
                    new AdaptiveGroup()
                    {
                        Children  =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = titles[1],
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.Body
                                    },

                                    new AdaptiveText()
                                    {
                                        Text = artists[1],
                                        HintWrap = true,
                                        HintStyle = AdaptiveTextStyle.Caption
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveText() { },
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = titles[2],
                                        HintWrap = false,
                                        HintStyle = AdaptiveTextStyle.BodySubtle
                                    },

                                    new AdaptiveText()
                                    {
                                        Text = artists[2],
                                        HintWrap = false,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    }
                },
                TextStacking = TileTextStacking.Center
            };
            return largeBindingContent;
        }
    }
}
