using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Enums
{
    public sealed class SPUtility
    {
        public class SortBy
        {
            public static readonly string Title = "Title";
            public static readonly string Album = "Album";
            public static readonly string Artist = "Artist";
            public static readonly string Genre = "Genre";
            public static readonly string HighestRating = "HighestRating";
            public static readonly string LowestRating = "LowestRating";
            public static readonly string MostOftenPlayed = "MostOftenPlayed";
            public static readonly string LeastOftenPlayed = "LeastOftenPlayed";
            public static readonly string MostRecentlyAdded = "MostRecentlyAdded";
            public static readonly string LeastRecentlyAdded = "LeastRecentlyAdded";
            public static readonly string MostRecentlyPlayed = "MostRecentlyPlayed";
            public static readonly string LeastRecentlyPlayed = "LeastRecentlyPlayed";
        }

        public enum ItemType
        {
            Date,
            Time,
            Number,
            String,
            Unknown
        }

        public class Item
        {
            public static readonly string Title = "Title";
            public static readonly string Album = "Album";
            public static readonly string Artist = "Artist";
            public static readonly string AlbumArtist = "AlbumArtist";
            public static readonly string Composer = "Composer";
            public static readonly string Genre = "Genre";
            public static readonly string Duration = "Duration";
            public static readonly string Year = "Year";
            public static readonly string Rating = "Rating";
            public static readonly string PlayCount = "PlayCount";
            public static readonly string FilePath = "FilePath";
            public static readonly string DateAdded = "DateAdded";
            public static readonly string LastPlayed = "LastPlayed";

            public static bool IsDateType(string value)
            {
                return (value == SPUtility.Item.DateAdded || value == SPUtility.Item.LastPlayed);
            }

            public static bool IsTimeType(string value)
            {
                return (value == SPUtility.Item.Duration);
            }

            public static bool IsNumberType(string value)
            {
                return (value == SPUtility.Item.PlayCount || value == SPUtility.Item.Rating || value == SPUtility.Item.Year);
            }

            public static bool IsStringType(string value)
            {
                return (value == SPUtility.Item.Album || value == SPUtility.Item.AlbumArtist || value == SPUtility.Item.Artist ||
                    value == SPUtility.Item.Composer || value == SPUtility.Item.FilePath || value == SPUtility.Item.Genre ||
                    value == SPUtility.Item.Title);
            }

            public static ItemType GetItemType(string item)
            {
                if (IsStringType(item)) return ItemType.String;
                if (IsDateType(item)) return ItemType.Date;
                if (IsNumberType(item)) return ItemType.Number;
                if (IsTimeType(item)) return ItemType.Time;
                return ItemType.Unknown;
            }
        }

        public class Comparison
        {
            public static readonly string Is = "IsEqual";
            public static readonly string IsNot = "IsNot";
            public static readonly string IsGreater = "IsGreater";
            public static readonly string IsLess = "IsLess";
            public static readonly string Contains = "Contains";
            public static readonly string DoesNotContain = "DoesNotContain";
            public static readonly string StartsWith = "StartsWith";
            public static readonly string EndsWith = "EndsWith";
        }

        public class ComparisonEx
        {
            public static readonly string IsGreaterOR = "IsGreaterOR";
            public static readonly string IsLessOR = "IsLessOR";
        }

        public class Operator
        {
            public static readonly string Or = "OR";
            public static readonly string And = "AND";
        }

        internal static Dictionary<string, string> SPsorting = new Dictionary<string, string>()
        {
            {SortBy.Title,"Title"},
            {SortBy.Album, "Album"},
            {SortBy.Artist, "Artists"},
            {SortBy.Genre, "Genres"},
            {SortBy.HighestRating, "Rating DESC"},
            {SortBy.LowestRating, "Rating"},
            {SortBy.MostOftenPlayed, "PlayCount DESC"},
            {SortBy.LeastOftenPlayed, "PlayCount"},
            {SortBy.MostRecentlyAdded,"DateAdded DESC"},
            {SortBy.LeastRecentlyAdded,"DateAdded"},
            {SortBy.MostRecentlyPlayed,"LastPlayed DESC"},
            {SortBy.LeastRecentlyPlayed,"LastPlayed"}
        };

        internal static Dictionary<string, string> SPConditionItem = new Dictionary<string, string>()
        {
            {Item.Title, "Title"},
            {Item.Album, "Album"},
            {Item.Artist, "Artists"},
            {Item.AlbumArtist, "AlbumArtist"},
            {Item.Composer, "Composers"},
            {Item.Genre, "Genres"},
            {Item.Duration, "Duration"},
            {Item.Year, "Year"},
            {Item.Rating, "Rating"},
            {Item.PlayCount, "PlayCount"},
            {Item.DateAdded, "DateAdded"},
            {Item.LastPlayed, "LastPlayed"},
            {Item.FilePath, "Path" }
        };

        internal static Dictionary<string, string> SPConditionComparison = new Dictionary<string, string>()
        {
            {Comparison.Is, "="},
            {Comparison.IsNot, "<>"},
            {Comparison.IsGreater, ">"},
            {Comparison.IsLess, "<"},
            {Comparison.Contains, "LIKE"},
            {Comparison.DoesNotContain, "NOT LIKE"},
            {Comparison.StartsWith, "LIKE"},
            {Comparison.EndsWith,"LIKE"},
            {ComparisonEx.IsGreaterOR,ComparisonEx.IsGreaterOR},
            {ComparisonEx.IsLessOR,ComparisonEx.IsLessOR}
        };
    }
    
    //public enum SPSorting
    //{
    //    Title = 0,
    //    Album = 1,
    //    Artist = 2,
    //    Genre = 3,
    //    HighestRating = 4,
    //    LowestRating = 5,
    //    MostOftenPlayed = 6,
    //    LeastOftenPlayed = 7,
    //    MostRecentlyAdded = 8,
    //    LeastRecentlyAdded = 9,
    //}

    //public enum SPConditionItem
    //{
    //    Title = 0,
    //    Album = 1,
    //    Artist = 2,
    //    Genre = 3,
    //    Duration = 4,
    //    Year = 5,
    //    Rating = 6,
    //    PlayCount = 7,
    //    FilePath = 8,
    //    DateAdded = 9,
    //    LastPlayed = 10,
    //}

    //public enum SPConditionComparison
    //{
    //    Is = 0,
    //    IsNot = 1,
    //    IsGreater = 2,
    //    IsLess = 3,
    //    Contains = 4,
    //    DoesNotContain = 5,
    //    StartsWith = 6,
    //    EndsWith = 7,
    //}
}
