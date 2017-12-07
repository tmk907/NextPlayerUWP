using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWP.Playback;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class TagsEditorViewModel : Template10.Mvvm.ViewModelBase
    {
        int songId;
        SongData songData;
        string genres;
        string artists;
        string album;
        string albumArtist;
        private bool isAlbumArtChanged = false;
        private StorageFile albumArtFile;

        private Tags tagsData = new Tags();
        public Tags TagsData
        {
            get { if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) tagsData = new Tags();
                return tagsData; }
            set { Set(ref tagsData, value); }
        }

        private bool showProgressBar = false;
        public bool ShowProgressBar
        {
            get { return showProgressBar; }
            set { Set(ref showProgressBar, value); }
        }

        private bool buttonsEnabled = true;
        public bool ButtonsEnabled
        {
            get { return buttonsEnabled; }
            set { Set(ref buttonsEnabled, value); }
        }

        private WriteableBitmap albumArt;
        public WriteableBitmap AlbumArt
        {
            get { return albumArt; }
            set { Set(ref albumArt, value); }
        }

        private bool isAlbumArtVisible = false;
        public bool IsAlbumArtVisible
        {
            get { return isAlbumArtVisible; }
            set { Set(ref isAlbumArtVisible, value); }
        }

        private ObservableCollection<SongItem> songs;
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        public async void SaveTags(object sender, RoutedEventArgs e)
        {
            ShowProgressBar = true;
            ButtonsEnabled = false;
            TagsData.FirstArtist = GetFirst(tagsData.Artists);
            TagsData.FirstComposer = GetFirst(tagsData.Composers);
            if (album != tagsData.Album || albumArtist != tagsData.AlbumArtist )
            {
                AlbumItem oldAlbum = await DatabaseManager.Current.GetAlbumItemAsync(album, albumArtist);
                AlbumItem newAlbum = await DatabaseManager.Current.GetAlbumItemAsync(tagsData.Album, tagsData.AlbumArtist);
                //if there is no album with (tagsData.Album, tagsData.AlbumArtist) in db then default AlbumItem is returned and id = -1
                if (oldAlbum.SongsNumber == 1)
                {
                    if (newAlbum.AlbumId > 0)
                    {
                        oldAlbum.LastAdded = (oldAlbum.LastAdded > newAlbum.LastAdded) ? oldAlbum.LastAdded : newAlbum.LastAdded;
                        oldAlbum.ImagePath = newAlbum.ImagePath;
                        oldAlbum.ImageUri = newAlbum.ImageUri;
                        oldAlbum.IsImageSet = newAlbum.IsImageSet;
                        await DatabaseManager.Current.DeleteAlbumAsync(newAlbum.AlbumParam, newAlbum.AlbumArtist);
                    }
                    oldAlbum.AlbumParam = tagsData.Album;
                    oldAlbum.AlbumArtist = tagsData.AlbumArtist;
                    await DatabaseManager.Current.UpdateAlbumItem(oldAlbum);
                }
            }

            if (artists != tagsData.Artists)
            {
                tagsData.Artists = tagsData.Artists.TrimEnd(new char[] { ';', ' ' });
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < tagsData.Artists.Length; i++)
                {
                    sb.Append(tagsData.Artists[i]);
                    if (tagsData.Artists[i]==';' && tagsData.Artists[i+1] != ' ')
                    {
                        sb.Append(' ');
                    }
                }
                tagsData.Artists = sb.ToString();
                var old = artists.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                var edited = tagsData.Artists.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                if (old.Length == 1 && edited.Length == 1)
                {
                    ArtistItem prevArtist = await DatabaseManager.Current.GetArtistItemAsync(old[0]);
                    ArtistItem newArtist = await DatabaseManager.Current.GetArtistItemAsync(edited[0]);
                    if (prevArtist.SongsNumber == 1)
                    {
                        if (newArtist.ArtistId > 0)
                        {
                            prevArtist.LastAdded = (prevArtist.LastAdded > newArtist.LastAdded) ? prevArtist.LastAdded : newArtist.LastAdded;
                            await DatabaseManager.Current.DeleteArtistAsync(edited[0]);
                        }
                        prevArtist.ArtistParam = tagsData.Artists;
                        await DatabaseManager.Current.UpdateArtistItem(prevArtist);
                    }
                }
                //else
                //{
                //    foreach (var o in old)
                //    {
                //        bool find = false;
                //        foreach (var ed in edited)
                //        {
                //            if (o.Equals(ed))
                //            {
                //                find = true;
                //                break;
                //            }
                //        }
                //        if (!find)
                //        {
                //            ArtistItem a = await DatabaseManager.Current.GetArtistItemAsync(o);
                //            if (a.SongsNumber == 1)
                //            {
                //                await DatabaseManager.Current.DeleteArtistAsync(o);
                //            }
                //        }
                //    }
                //}
            }

            if (genres != tagsData.Genres)
            {
                tagsData.Genres = tagsData.Genres.TrimEnd(new char[] { ';', ' ' });
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < tagsData.Genres.Length; i++)
                {
                    sb.Append(tagsData.Genres[i]);
                    if (tagsData.Genres[i] == ';' && tagsData.Genres[i + 1] != ' ')
                    {
                        sb.Append(' ');
                    }
                }
                tagsData.Genres = sb.ToString();
            }
            songData.Tag = TagsData;
            TagsManager tm = new TagsManager();
            Songs = new ObservableCollection<SongItem>();

            if (isAlbumArtChanged)
            {
                StorageFile file;
                try
                {
                    file = await StorageFile.GetFileFromPathAsync(songData.Path);
                }
                catch (System.IO.FileNotFoundException)
                {
                    file = null;
                }
                if (IsAlbumArtSet())
                {
                    AlbumArtsManager aam = new AlbumArtsManager();
                    await aam.SaveAlbumArtAndColor(albumArt, songData);
                    if (file != null) await tm.SaveAlbumArt(albumArtFile, file);
                }
                else
                {
                    try
                    {
                        await ImagesManager.TryDeleteAppLocalFile(songData.AlbumArtPath);
                    }
                    catch (Exception)
                    {

                    }
                    songData.AlbumArtPath = AppConstants.AlbumCover;
                    if (file != null) await tm.DeleteAlbumArt(file);
                }
            }
            
            await DatabaseManager.Current.UpdateSongData(songData);
            await DatabaseManager.Current.UpdateTables();
            await NowPlayingPlaylistManager.Current.UpdateSong(songData);
            
            await tm.SaveTags(songData);
            App.OnSongUpdated(songData.SongId);
            ShowProgressBar = false;
            ButtonsEnabled = true;
            NavigationService.GoBack();
        }

        public void Cancel(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private string GetFirst(string text)
        {
            if (text.IndexOf(';') > 0)
            {
                return text.Substring(0, text.IndexOf(';'));
            }
            return text;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            App.OnNavigatedToNewView(true);
            MessageHub.Instance.Publish<PageNavigated>(new PageNavigated() { NavigatedTo = true, PageType = PageNavigatedType.TagsEditor });
            IsAlbumArtVisible = false;
            songId = -1;
            songData = new SongData();
            if (parameter != null)
            {
                songId = Int32.Parse(MusicItem.SplitParameter(parameter as string)[1]);
                songData = await DatabaseManager.Current.GetSongDataAsync(songId);
            }
            TagsData = songData.Tag;
            album = tagsData.Album;
            artists = tagsData.Artists;
            genres = tagsData.Genres;
            albumArtist = tagsData.AlbumArtist;
            TagsManager tm = new TagsManager();
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(songData.Path);
                AlbumArt = await tm.GetAlbumArt(file);
            }
            catch (Exception ex)
            {
                var file = await FutureAccessHelper.GetFileFromPathAsync(songData.Path);
                if (file != null)
                {
                    AlbumArt = await tm.GetAlbumArt(file);
                }
            }
            IsAlbumArtVisible = IsAlbumArtSet();
            await LoadSongs();
            TelemetryAdapter.TrackPageView(this.GetType().ToString());
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            MessageHub.Instance.Publish<PageNavigated>(new PageNavigated() { NavigatedTo = false, PageType = PageNavigatedType.TagsEditor });
            return base.OnNavigatedFromAsync(pageState, suspending);
        }

        public void ClearAlbumArt()
        {
            AlbumArt = new WriteableBitmap(1, 1);
            IsAlbumArtVisible = false;
            isAlbumArtChanged = true;
        }

        public async void AddFromFile()
        {
            await AddAlbumArtFromFile();
        }

        public async void AddFromSong(SongItem song)
        {
            //var song = (SongItem)e.ClickedItem;
            await AddAlbumArtFromSong(song);
        }

        private async Task AddAlbumArtFromFile()
        {
            ShowProgressBar = true;
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                using (IRandomAccessStream istream = await file.OpenAsync(FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                    AlbumArt = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    istream.Seek(0);
                    await AlbumArt.SetSourceAsync(istream);
                }
                albumArtFile = file;
            }
            else
            {
                //show error
            }
            IsAlbumArtVisible = IsAlbumArtSet();
            isAlbumArtChanged = true;
            ShowProgressBar = false;
        }

        private async Task AddAlbumArtFromSong(SongItem song)
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(song.AlbumArtUri);
                if (file != null)
                {
                    using (IRandomAccessStream istream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(istream);
                        AlbumArt = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                        istream.Seek(0);
                        await AlbumArt.SetSourceAsync(istream);
                    }
                    albumArtFile = file;
                }
                else
                {
                    //show error
                }
                IsAlbumArtVisible = IsAlbumArtSet();
                isAlbumArtChanged = true;
            }
            catch (Exception ex)
            {

            }
        }

        private bool IsAlbumArtSet()
        {
            if (albumArt == null) return false;
            else return albumArt.PixelHeight != 1 || albumArt.PixelWidth != 1;
        }

        public async void SaveToFile()
        {
            if (!IsAlbumArtSet())
            {
                MessageDialogHelper m = new MessageDialogHelper();
                await m.ShowAlbumArtSaveError();
                return;
            }

            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            savePicker.FileTypeChoices.Add("jpg", new List<string>() { ".jpg" });
            savePicker.SuggestedFileName = songData.Filename.Substring(0, songData.Filename.LastIndexOf('.'));

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);
                ShowProgressBar = true;
                await ImagesManager.SaveBitmap(file, albumArt);
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                ShowProgressBar = false;
                if (status == FileUpdateStatus.Complete)
                {
                    
                }
                else
                {
                    MessageDialogHelper m = new MessageDialogHelper();
                    await m.ShowAlbumArtSaveError();
                }
            }
            else
            {
                //"Operation cancelled.";
            }
        }

        public async Task LoadSongs()
        {
            var s = await DatabaseManager.Current.GetAllSongItemsAsync();
            Songs = new ObservableCollection<SongItem>(s.Where(t => t.CoverPath != AppConstants.AlbumCover));
        }

        ListView listView;

        public void SetListView(ListView listView)
        {
            this.listView = listView;
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var matchingSongs = songs.Where(s => s.Title.ToLower().StartsWith(query));
                var m2 = songs.Where(s => s.Title.ToLower().Contains(query));
                var m3 = songs.Where(s => (s.Album.ToLower().Contains(query) || s.Artist.ToLower().Contains(query)));
                var m4 = matchingSongs.Concat(m2).Concat(m3).Distinct();
                sender.ItemsSource = m4.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = songs.IndexOf((SongItem)args.ChosenSuggestion);
            }
            else
            {
                var list = songs.Where(s => s.Title.ToLower().StartsWith(sender.Text)).OrderBy(s => s.Title).ToList();
                if (list.Count == 0) return;
                index = 0;
                bool find = false;
                foreach (var g in songs)
                {
                    if (g.Title.Equals(list.FirstOrDefault().Title))
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                if (!find) return;
            }
            listView.SelectedIndex = index;
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var song = args.SelectedItem as SongItem;
            sender.Text = song.Title;
        }
    }
}
