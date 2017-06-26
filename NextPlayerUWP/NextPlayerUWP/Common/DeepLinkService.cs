using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Windows.ApplicationModel.Activation;

namespace NextPlayerUWP.Common
{
    public class DeepLinkService
    {
        public const string protocol = "next-player:";
        public class AvailableParameters
        {
            public const string Page = "page";
            public const string AlbumName = "albumname";
            public const string AlbumId = "albumid";
            public const string ArtistName = "artistname";
            public const string ArtistId = "artistid";
            public const string AlbumArtistName = "albumartistname";
            public const string AlbumArtistId = "albumartistid";
            public const string GenreName = "genrename";
            public const string GenreId = "genreid";
            public const string FolderName = "foldername";
            public const string FolderId = "folderid";
            public const string PlaylistName = "playlistname";
            public const string PlaylistId = "playlistid";
            public const string PlaylistPath = "playlistpath";
            public const string IsSmart = "issmart";
            public const string SongTitle = "songtitle";
            public const string SongArtist = "songartist";
            public const string SongId = "songid";
        }

        private DeepLinkParser parser;

        public string CreateDeepLink(string root, Dictionary<string,string> parameters)
        {
            StringBuilder sb = new StringBuilder(protocol);
            sb.Append(root);
            if (parameters.Count > 0)
            {
                sb.Append('?');
                bool first = true;
                foreach (var param in parameters)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append('&');
                    }
                    sb.Append(param.Key).Append('=').Append(param.Value);
                }
            }
            return sb.ToString();
        }

        public string CreateDeepLink(AppPages.Pages page, Dictionary<string, string> parameters)
        {
            parameters.Add(AvailableParameters.Page, App.AppPages.PagesToKeys[page]);
            return CreateDeepLink("", parameters);
        }

        public DeepLinkParser ParseDeepLink(Uri uri)
        {
            return DeepLinkParser.Create(uri);
        }

        public DeepLinkParser ParseDeepLink(IActivatedEventArgs e)
        {
            return DeepLinkParser.Create(e);
        }

        public Collection<KeyValuePair<string, string>> ParseUriParameters(Uri uri)
        {
            var paramCollection = new QueryParameterCollection(uri);
            return paramCollection;
        }
    }
}
