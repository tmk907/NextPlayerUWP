using Microsoft.Toolkit.Uwp.Notifications;
using NextPlayerUWPDataLayer.Constants;
using System.Collections.Generic;

namespace NextPlayerUWP.Common.Tiles
{
    public class TileWithoutImage : ITileContentFactory
    {
        private string title;
        private string artist;
        private List<string> titles;
        private List<string> artists;

        public TileWithoutImage(string title, string artist)
        {
            titles = new List<string>();
            this.title = title;
            artists = new List<string>();
            this.artist = artist;
        }

        public TileWithoutImage(List<string> titles, List<string> artists)
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
        }

        public TileBindingContentAdaptive GetSmallBindingContent()
        {
            return GetSmallBindingContent(title, artist);
        }

        public TileBindingContentAdaptive GetMediumBindingContent()
        {
            return GetMediumBindingContent(title, artist);
        }

        public TileBindingContentAdaptive GetWideBindingContent()
        {
            return GetWideBindingContent(title, artist);
        }

        public TileBindingContentAdaptive GetLargeBindingContent()
        {
            if (titles.Count < 3)
            {
                return GetLargeBindingContent(title, artist);
            }
            else
            {
                return GetLargeBindingContent(titles, artists);
            }
        }

        private TileBindingContentAdaptive GetSmallBindingContent(string title, string artist)
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

        private TileBindingContentAdaptive GetMediumBindingContent(string title, string artist)
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
                }
            };
            NextPlayerUWPDataLayer.Diagnostics.Logger.Save(mediumBindingContent.ToString());
            return mediumBindingContent;
        }

        private TileBindingContentAdaptive GetWideBindingContent(string title, string artist)
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
                            }
                        }
                    }
                }
            };
            return wideBindingContent;
        }

        private TileBindingContentAdaptive GetLargeBindingContent2(string title, string artist)
        {
            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
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
                }
            };
            return largeBindingContent;
        }

        private TileBindingContentAdaptive GetLargeBindingContent(string title, string artist)
        {
            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
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

        private TileBindingContentAdaptive GetLargeBindingContent(List<string> titles, List<string> artists)
        {
            TileBindingContentAdaptive largeBindingContent = new TileBindingContentAdaptive()
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
