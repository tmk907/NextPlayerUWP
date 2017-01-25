using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace NextPlayerUWP.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<AddToPlaylistViewModel>();
            SimpleIoc.Default.Register<AlbumsViewModel>();
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<AlbumArtistsViewModel>();
            SimpleIoc.Default.Register<AlbumArtistViewModel>();
            SimpleIoc.Default.Register<ArtistsViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<AudioSettingsViewModel>();
            SimpleIoc.Default.Register<BottomPlayerViewModel>();
            SimpleIoc.Default.Register<CloudStorageFoldersViewModel>();
            SimpleIoc.Default.Register<FileInfoViewModel>();
            SimpleIoc.Default.Register<FoldersViewModel>();
            SimpleIoc.Default.Register<GenresViewModel>();
            SimpleIoc.Default.Register<LyricsViewModel>();
            SimpleIoc.Default.Register<NewSmartPlaylistViewModel>();          
            SimpleIoc.Default.Register<NowPlayingViewModel>();
            SimpleIoc.Default.Register<NowPlayingDesktopViewModel>();
            SimpleIoc.Default.Register<NowPlayingPlaylistViewModel>();
            SimpleIoc.Default.Register<PlaylistsViewModel>();
            SimpleIoc.Default.Register<PlaylistViewModel>();
            SimpleIoc.Default.Register<RadiosViewModel>();
            SimpleIoc.Default.Register<RightPanelViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<TagsEditorViewModel>();
            SimpleIoc.Default.Register<TestViewModel>();
            SimpleIoc.Default.Register<PlayerViewModelBase>();
            SimpleIoc.Default.Register<QueueViewModelBase>();
        }

        public AddToPlaylistViewModel AddToPlaylistVM => ServiceLocator.Current.GetInstance<AddToPlaylistViewModel>();
        public AlbumViewModel AlbumVM => ServiceLocator.Current.GetInstance<AlbumViewModel>();
        public AlbumsViewModel AlbumsVM => ServiceLocator.Current.GetInstance<AlbumsViewModel>();
        public AlbumArtistsViewModel AlbumArtistsVM => ServiceLocator.Current.GetInstance<AlbumArtistsViewModel>();
        public AlbumArtistViewModel AlbumArtistVM => ServiceLocator.Current.GetInstance<AlbumArtistViewModel>();
        public ArtistsViewModel ArtistsVM => ServiceLocator.Current.GetInstance<ArtistsViewModel>();
        public ArtistViewModel ArtistVM => ServiceLocator.Current.GetInstance<ArtistViewModel>();
        public AudioSettingsViewModel AudioSettingsVM => ServiceLocator.Current.GetInstance<AudioSettingsViewModel>();
        public BottomPlayerViewModel BottomPlayerVM => ServiceLocator.Current.GetInstance<BottomPlayerViewModel>();
        public CloudStorageFoldersViewModel CloudStorageFoldersVM => ServiceLocator.Current.GetInstance<CloudStorageFoldersViewModel>();
        public FileInfoViewModel FileInfoVM => ServiceLocator.Current.GetInstance<FileInfoViewModel>();
        public FoldersViewModel FoldersVM => ServiceLocator.Current.GetInstance<FoldersViewModel>();
        public GenresViewModel GenresVM => ServiceLocator.Current.GetInstance<GenresViewModel>();
        public LyricsViewModel LyricsVM => ServiceLocator.Current.GetInstance<LyricsViewModel>();
        public NewSmartPlaylistViewModel NewSmartPlaylistVM => ServiceLocator.Current.GetInstance<NewSmartPlaylistViewModel>();
        public NowPlayingViewModel NowPlayingVM => ServiceLocator.Current.GetInstance<NowPlayingViewModel>();
        public NowPlayingPlaylistViewModel NowPlayingPlaylistVM => ServiceLocator.Current.GetInstance<NowPlayingPlaylistViewModel>();
        public NowPlayingDesktopViewModel NowPlayingDesktopVM => ServiceLocator.Current.GetInstance<NowPlayingDesktopViewModel>();
        public PlaylistViewModel PlaylistVM => ServiceLocator.Current.GetInstance<PlaylistViewModel>();
        public PlaylistsViewModel PlaylistsVM => ServiceLocator.Current.GetInstance<PlaylistsViewModel>();
        public RadiosViewModel RadiosVM => ServiceLocator.Current.GetInstance<RadiosViewModel>();
        public RightPanelViewModel RightPanelVM => ServiceLocator.Current.GetInstance<RightPanelViewModel>();
        public SettingsViewModel SettingsVM => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public SongsViewModel SongsVM => ServiceLocator.Current.GetInstance<SongsViewModel>();
        public TagsEditorViewModel TagsEditorVM => ServiceLocator.Current.GetInstance<TagsEditorViewModel>();
        public TestViewModel TestVM => ServiceLocator.Current.GetInstance<TestViewModel>();
        public PlayerViewModelBase PlayerVM => ServiceLocator.Current.GetInstance<PlayerViewModelBase>();
        public QueueViewModelBase QueueVM => ServiceLocator.Current.GetInstance<QueueViewModelBase>();
    }
}
