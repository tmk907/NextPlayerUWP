﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NextPlayerUWPDataLayer.SpotifyAPI.Web.Enums;
using NextPlayerUWPDataLayer.SpotifyAPI.Web.Models;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web
{
    /// <summary>
    /// SpotifyAPI URL-Generator
    /// </summary>
    public class SpotifyWebBuilder
    {
        public const string APIBase = "https://api.spotify.com/v1";

        #region Search

        /// <summary>
        ///     Get Spotify catalog information about artists, albums, tracks or playlists that match a keyword string.
        /// </summary>
        /// <param name="q">The search query's keywords (and optional field filters and operators), for example q=roadhouse+blues.</param>
        /// <param name="type">A list of item types to search across.</param>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first result to return. Default: 0</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code or the string from_token.</param>
        /// <returns></returns>
        public string SearchItems(String q, SearchType type, int limit = 20, int offset = 0, String market = "")
        {
            limit = Math.Min(50, limit);
            StringBuilder builder = new StringBuilder(APIBase + "/search");
            builder.Append("?q=" + q);
            builder.Append("&type=" + type.GetStringAttribute(","));
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        #endregion Search

        #region Albums

        /// <summary>
        ///     Get Spotify catalog information about an album’s tracks. Optional parameters can be used to limit the number of
        ///     tracks returned.
        /// </summary>
        /// <param name="id">The Spotify ID for the album.</param>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first track to return. Default: 0 (the first object).</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public string GetAlbumTracks(String id, int limit = 20, int offset = 0, String market = "")
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/albums/" + id + "/tracks");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        /// <summary>
        ///     Get Spotify catalog information for a single album.
        /// </summary>
        /// <param name="id">The Spotify ID for the album.</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public string GetAlbum(String id, String market = "")
        {
            if (String.IsNullOrEmpty(market))
                return $"{APIBase}/albums/{id}";
            return $"{APIBase}/albums/{id}?market={market}";
        }

        /// <summary>
        ///     Get Spotify catalog information for multiple albums identified by their Spotify IDs.
        /// </summary>
        /// <param name="ids">A list of the Spotify IDs for the albums. Maximum: 20 IDs.</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public string GetSeveralAlbums(List<String> ids, String market = "")
        {
            if (String.IsNullOrEmpty(market))
                return $"{APIBase}/albums?ids={string.Join(",", ids.Take(20))}";
            return $"{APIBase}/albums?market={market}&ids={string.Join(",", ids.Take(20))}";
        }

        #endregion Albums

        #region Artists

        /// <summary>
        ///     Get Spotify catalog information for a single artist identified by their unique Spotify ID.
        /// </summary>
        /// <param name="id">The Spotify ID for the artist.</param>
        /// <returns></returns>
        public string GetArtist(String id)
        {
            return $"{APIBase}/artists/{id}";
        }

        /// <summary>
        ///     Get Spotify catalog information about artists similar to a given artist. Similarity is based on analysis of the
        ///     Spotify community’s listening history.
        /// </summary>
        /// <param name="id">The Spotify ID for the artist.</param>
        /// <returns></returns>
        public string GetRelatedArtists(String id)
        {
            return $"{APIBase}/artists/{id}/related-artists";
        }

        /// <summary>
        ///     Get Spotify catalog information about an artist’s top tracks by country.
        /// </summary>
        /// <param name="id">The Spotify ID for the artist.</param>
        /// <param name="country">The country: an ISO 3166-1 alpha-2 country code.</param>
        /// <returns></returns>
        public string GetArtistsTopTracks(String id, String country)
        {
            return $"{APIBase}/artists/{id}/top-tracks?country={country}";
        }

        /// <summary>
        ///     Get Spotify catalog information about an artist’s albums. Optional parameters can be specified in the query string
        ///     to filter and sort the response.
        /// </summary>
        /// <param name="id">The Spotify ID for the artist.</param>
        /// <param name="type">
        ///     A list of keywords that will be used to filter the response. If not supplied, all album types will
        ///     be returned
        /// </param>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first album to return. Default: 0</param>
        /// <param name="market">
        ///     An ISO 3166-1 alpha-2 country code. Supply this parameter to limit the response to one particular
        ///     geographical market
        /// </param>
        /// <returns></returns>
        public string GetArtistsAlbums(String id, AlbumType type = AlbumType.All, int limit = 20, int offset = 0, String market = "")
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/artists/" + id + "/albums");
            builder.Append("?album_type=" + type.GetStringAttribute(","));
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        /// <summary>
        ///     Get Spotify catalog information for several artists based on their Spotify IDs.
        /// </summary>
        /// <param name="ids">A list of the Spotify IDs for the artists. Maximum: 50 IDs.</param>
        /// <returns></returns>
        public string GetSeveralArtists(List<String> ids)
        {
            return $"{APIBase}/artists?ids={string.Join(",", ids.Take(50))}";
        }

        #endregion Artists

        #region Browse

        /// <summary>
        ///     Get a list of Spotify featured playlists (shown, for example, on a Spotify player’s “Browse” tab).
        /// </summary>
        /// <param name="locale">
        ///     The desired language, consisting of a lowercase ISO 639 language code and an uppercase ISO 3166-1
        ///     alpha-2 country code, joined by an underscore.
        /// </param>
        /// <param name="country">A country: an ISO 3166-1 alpha-2 country code.</param>
        /// <param name="timestamp">A timestamp in ISO 8601 format</param>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first item to return. Default: 0</param>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetFeaturedPlaylists(String locale = "", String country = "", DateTime timestamp = default(DateTime), int limit = 20, int offset = 0)
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/browse/featured-playlists");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(locale))
                builder.Append("&locale=" + locale);
            if (!String.IsNullOrEmpty(country))
                builder.Append("&country=" + country);
            if (timestamp != default(DateTime))
                builder.Append("&timestamp=" + timestamp.ToString("yyyy-MM-ddTHH:mm:ss"));
            return builder.ToString();
        }

        /// <summary>
        ///     Get a list of new album releases featured in Spotify (shown, for example, on a Spotify player’s “Browse” tab).
        /// </summary>
        /// <param name="country">A country: an ISO 3166-1 alpha-2 country code.</param>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first item to return. Default: 0</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetNewAlbumReleases(String country = "", int limit = 20, int offset = 0)
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/browse/new-releases");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(country))
                builder.Append("&country=" + country);
            return builder.ToString();
        }

        /// <summary>
        ///     Get a list of categories used to tag items in Spotify (on, for example, the Spotify player’s “Browse” tab).
        /// </summary>
        /// <param name="country">
        ///     A country: an ISO 3166-1 alpha-2 country code. Provide this parameter if you want to narrow the
        ///     list of returned categories to those relevant to a particular country
        /// </param>
        /// <param name="locale">
        ///     The desired language, consisting of an ISO 639 language code and an ISO 3166-1 alpha-2 country
        ///     code, joined by an underscore
        /// </param>
        /// <param name="limit">The maximum number of categories to return. Default: 20. Minimum: 1. Maximum: 50. </param>
        /// <param name="offset">The index of the first item to return. Default: 0 (the first object).</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetCategories(String country = "", String locale = "", int limit = 20, int offset = 0)
        {
            limit = Math.Min(50, limit);
            StringBuilder builder = new StringBuilder(APIBase + "/browse/categories");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(country))
                builder.Append("&country=" + country);
            if (!String.IsNullOrEmpty(locale))
                builder.Append("&locale=" + locale);
            return builder.ToString();
        }

        /// <summary>
        ///     Get a single category used to tag items in Spotify (on, for example, the Spotify player’s “Browse” tab).
        /// </summary>
        /// <param name="categoryId">The Spotify category ID for the category.</param>
        /// <param name="country">
        ///     A country: an ISO 3166-1 alpha-2 country code. Provide this parameter to ensure that the category
        ///     exists for a particular country.
        /// </param>
        /// <param name="locale">
        ///     The desired language, consisting of an ISO 639 language code and an ISO 3166-1 alpha-2 country
        ///     code, joined by an underscore
        /// </param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetCategory(String categoryId, String country = "", String locale = "")
        {
            StringBuilder builder = new StringBuilder(APIBase + "/browse/categories/" + categoryId);
            if (!String.IsNullOrEmpty(country))
                builder.Append("?country=" + country);
            if (!String.IsNullOrEmpty(locale))
                builder.Append((country == "" ? "?locale=" : "&locale=") + locale);
            return builder.ToString();
        }

        /// <summary>
        ///     Get a list of Spotify playlists tagged with a particular category.
        /// </summary>
        /// <param name="categoryId">The Spotify category ID for the category.</param>
        /// <param name="country">A country: an ISO 3166-1 alpha-2 country code.</param>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first item to return. Default: 0</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetCategoryPlaylists(String categoryId, String country = "", int limit = 20, int offset = 0)
        {
            limit = Math.Min(50, limit);
            StringBuilder builder = new StringBuilder(APIBase + "/browse/categories/" + categoryId + "/playlists");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(country))
                builder.Append("&country=" + country);
            return builder.ToString();
        }

        #endregion Browse

        #region Follow

        /// <summary>
        ///     Get the current user’s followed artists.
        /// </summary>
        /// <param name="limit">The maximum number of items to return. Default: 20. Minimum: 1. Maximum: 50. </param>
        /// <param name="after">The last artist ID retrieved from the previous request.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetFollowedArtists(int limit = 20, String after = "")
        {
            limit = Math.Min(limit, 50);
            const FollowType followType = FollowType.Artist; //currently only artist is supported.
            StringBuilder builder = new StringBuilder(APIBase + "/me/following?type=" + followType.GetStringAttribute(""));
            builder.Append("&limit=" + limit);
            if (!String.IsNullOrEmpty(after))
                builder.Append("&after=" + after);
            return builder.ToString();
        }

        /// <summary>
        ///     Add the current user as a follower of one or more artists or other Spotify users.
        /// </summary>
        /// <param name="followType">The ID type: either artist or user.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string Follow(FollowType followType)
        {
            return $"{APIBase}/me/following?type={followType.GetStringAttribute("")}";
        }

        /// <summary>
        ///     Remove the current user as a follower of one or more artists or other Spotify users.
        /// </summary>
        /// <param name="followType">The ID type: either artist or user.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string Unfollow(FollowType followType)
        {
            return $"{APIBase}/me/following?type={followType.GetStringAttribute("")}";
        }

        /// <summary>
        ///     Check to see if the current user is following one or more artists or other Spotify users.
        /// </summary>
        /// <param name="followType">The ID type: either artist or user.</param>
        /// <param name="ids">A list of the artist or the user Spotify IDs to check</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string IsFollowing(FollowType followType, List<String> ids)
        {
            return $"{APIBase}/me/following/contains?type={followType.GetStringAttribute("")}&ids={string.Join(",", ids)}";
        }

        /// <summary>
        ///     Add the current user as a follower of a playlist.
        /// </summary>
        /// <param name="ownerId">The Spotify user ID of the person who owns the playlist.</param>
        /// <param name="playlistId">
        ///     The Spotify ID of the playlist. Any playlist can be followed, regardless of its public/private
        ///     status, as long as you know its playlist ID.
        /// </param>
        /// <param name="showPublic">
        ///     If true the playlist will be included in user's public playlists, if false it will remain
        ///     private.
        /// </param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string FollowPlaylist(String ownerId, String playlistId, bool showPublic = true)
        {
            return $"{APIBase}/users/{ownerId}/playlists/{playlistId}/followers";
        }

        /// <summary>
        ///     Remove the current user as a follower of a playlist.
        /// </summary>
        /// <param name="ownerId">The Spotify user ID of the person who owns the playlist.</param>
        /// <param name="playlistId">The Spotify ID of the playlist that is to be no longer followed.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string UnfollowPlaylist(String ownerId, String playlistId)
        {
            return $"{APIBase}/users/{ownerId}/playlists/{playlistId}/followers";
        }

        /// <summary>
        ///     Check to see if one or more Spotify users are following a specified playlist.
        /// </summary>
        /// <param name="ownerId">The Spotify user ID of the person who owns the playlist.</param>
        /// <param name="playlistId">The Spotify ID of the playlist.</param>
        /// <param name="ids">A list of Spotify User IDs</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string IsFollowingPlaylist(String ownerId, String playlistId, List<String> ids)
        {
            return $"{APIBase}/users/{ownerId}/playlists/{playlistId}/followers/contains?ids={string.Join(",", ids)}";
        }

        #endregion Follow

        #region Library

        /// <summary>
        ///     Save one or more tracks to the current user’s “Your Music” library.
        /// </summary>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string SaveTracks()
        {
            return APIBase + "/me/tracks/";
        }

        /// <summary>
        ///     Get a list of the songs saved in the current Spotify user’s “Your Music” library.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first object to return. Default: 0 (i.e., the first object)</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetSavedTracks(int limit = 20, int offset = 0, String market = "")
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/me/tracks");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        /// <summary>
        ///     Remove one or more tracks from the current user’s “Your Music” library.
        /// </summary>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string RemoveSavedTracks()
        {
            return APIBase + "/me/tracks/";
        }

        /// <summary>
        ///     Check if one or more tracks is already saved in the current Spotify user’s “Your Music” library.
        /// </summary>
        /// <param name="ids">A list of the Spotify IDs.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string CheckSavedTracks(List<String> ids)
        {
            return APIBase + "/me/tracks/contains?ids=" + string.Join(",", ids);
        }

        /// <summary>
        ///     Save one or more albums to the current user’s "Your Music" library.
        /// </summary>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string SaveAlbums()
        {
            return $"{APIBase}/me/albums";
        }

        /// <summary>
        ///     Get a list of the albums saved in the current Spotify user’s "Your Music" library.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first object to return. Default: 0 (i.e., the first object)</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetSavedAlbums(int limit = 20, int offset = 0, string market = "")
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/me/albums");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!string.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        /// <summary>
        ///     Remove one or more albums from the current user’s "Your Music" library.
        /// </summary>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string RemoveSavedAlbums()
        {
            return APIBase + "/me/albums/";
        }

        /// <summary>
        ///     Check if one or more albums is already saved in the current Spotify user’s "Your Music" library.
        /// </summary>
        /// <param name="ids">A list of the Spotify IDs.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string CheckSavedAlbums(List<String> ids)
        {
            return APIBase + "/me/tracks/contains?ids=" + string.Join(",", ids);
        }

        #endregion Library

        #region Playlists

        /// <summary>
        ///     Get a list of the playlists owned or followed by a Spotify user.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="limit">The maximum number of playlists to return. Default: 20. Minimum: 1. Maximum: 50. </param>
        /// <param name="offset">The index of the first playlist to return. Default: 0 (the first object)</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetUserPlaylists(String userId, int limit = 20, int offset = 0)
        {
            limit = Math.Min(limit, 50);
            StringBuilder builder = new StringBuilder(APIBase + "/users/" + userId + "/playlists");
            builder.Append("?limit=" + limit);
            builder.Append("&offset=" + offset);
            return builder.ToString();
        }

        /// <summary>
        ///     Get a playlist owned by a Spotify user.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="fields">
        ///     Filters for the query: a comma-separated list of the fields to return. If omitted, all fields are
        ///     returned.
        /// </param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetPlaylist(String userId, String playlistId, String fields = "", String market = "")
        {
            StringBuilder builder = new StringBuilder(APIBase + "/users/" + userId + "/playlists/" + playlistId);
            builder.Append("?fields=" + fields);
            if (!String.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        /// <summary>
        ///     Get full details of the tracks of a playlist owned by a Spotify user.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="fields">
        ///     Filters for the query: a comma-separated list of the fields to return. If omitted, all fields are
        ///     returned.
        /// </param>
        /// <param name="limit">The maximum number of tracks to return. Default: 100. Minimum: 1. Maximum: 100.</param>
        /// <param name="offset">The index of the first object to return. Default: 0 (i.e., the first object)</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetPlaylistTracks(String userId, String playlistId, String fields = "", int limit = 100, int offset = 0, String market = "")
        {
            limit = Math.Min(limit, 100);
            StringBuilder builder = new StringBuilder(APIBase + "/users/" + userId + "/playlists/" + playlistId + "/tracks");
            builder.Append("?fields=" + fields);
            builder.Append("&limit=" + limit);
            builder.Append("&offset=" + offset);
            if (!String.IsNullOrEmpty(market))
                builder.Append("&market=" + market);
            return builder.ToString();
        }

        /// <summary>
        ///     Create a playlist for a Spotify user. (The playlist will be empty until you add tracks.)
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistName">
        ///     The name for the new playlist, for example "Your Coolest Playlist". This name does not need
        ///     to be unique.
        /// </param>
        /// <param name="isPublic">
        ///     default true. If true the playlist will be public, if false it will be private. To be able to
        ///     create private playlists, the user must have granted the playlist-modify-private scope.
        /// </param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string CreatePlaylist(String userId, String playlistName, Boolean isPublic = true)
        {
            return $"{APIBase}/users/{userId}/playlists";
        }

        /// <summary>
        ///     Change a playlist’s name and public/private state. (The user must, of course, own the playlist.)
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string UpdatePlaylist(String userId, String playlistId)
        {
            return $"{APIBase}/users/{userId}/playlists/{playlistId}";
        }

        /// <summary>
        ///     Replace all the tracks in a playlist, overwriting its existing tracks. This powerful request can be useful for
        ///     replacing tracks, re-ordering existing tracks, or clearing the playlist.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string ReplacePlaylistTracks(String userId, String playlistId)
        {
            return $"{APIBase}/users/{userId}/playlists/{playlistId}/tracks";
        }

        /// <summary>
        ///     Remove one or more tracks from a user’s playlist.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="uris">
        ///     array of objects containing Spotify URI strings (and their position in the playlist). A maximum of
        ///     100 objects can be sent at once.
        /// </param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string RemovePlaylistTracks(String userId, String playlistId, List<DeleteTrackUri> uris)
        {
            return $"{APIBase}/users/{userId}/playlists/{playlistId}/tracks";
        }

        /// <summary>
        ///     Add one or more tracks to a user’s playlist.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <param name="uris">A list of Spotify track URIs to add</param>
        /// <param name="position">The position to insert the tracks, a zero-based index</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string AddPlaylistTracks(String userId, String playlistId, List<String> uris, int? position = null)
        {
            if (position == null)
                return $"{APIBase}/users/{userId}/playlists/{playlistId}/tracks";
            return $"{APIBase}/users/{userId}/playlists/{playlistId}/tracks?position={position}";
        }

        /// <summary>
        ///     Reorder a track or a group of tracks in a playlist.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <param name="playlistId">The Spotify ID for the playlist.</param>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string ReorderPlaylist(String userId, String playlistId)
        {
            return $"{APIBase}/users/{userId}/playlists/{playlistId}/tracks";
        }

        #endregion Playlists

        #region Profiles

        /// <summary>
        ///     Get detailed profile information about the current user (including the current user’s username).
        /// </summary>
        /// <returns></returns>
        /// <remarks>AUTH NEEDED</remarks>
        public string GetPrivateProfile()
        {
            return $"{APIBase}/me";
        }

        /// <summary>
        ///     Get public profile information about a Spotify user.
        /// </summary>
        /// <param name="userId">The user's Spotify user ID.</param>
        /// <returns></returns>
        public string GetPublicProfile(String userId)
        {
            return $"{APIBase}/users/{userId}";
        }

        #endregion Profiles

        #region Tracks

        /// <summary>
        ///     Get Spotify catalog information for multiple tracks based on their Spotify IDs.
        /// </summary>
        /// <param name="ids">A list of the Spotify IDs for the tracks. Maximum: 50 IDs.</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public string GetSeveralTracks(List<String> ids, String market = "")
        {
            if (String.IsNullOrEmpty(market))
                return $"{APIBase}/tracks?ids={string.Join(",", ids.Take(50))}";
            return $"{APIBase}/tracks?market={market}&ids={string.Join(",", ids.Take(50))}";
        }

        /// <summary>
        ///     Get Spotify catalog information for a single track identified by its unique Spotify ID.
        /// </summary>
        /// <param name="id">The Spotify ID for the track.</param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public string GetTrack(String id, String market = "")
        {
            if (String.IsNullOrEmpty(market))
                return $"{APIBase}/tracks/{id}";
            return $"{APIBase}/tracks/{id}?market={market}";
        }

        #endregion Tracks
    }
}
