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
            SimpleIoc.Default.Register<ArtistsViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<FileInfoViewModel>();
            SimpleIoc.Default.Register<FoldersViewModel>();
            SimpleIoc.Default.Register<GenresViewModel>();
            SimpleIoc.Default.Register<NowPlayingViewModel>();
            SimpleIoc.Default.Register<NowPlayingPlaylistViewModel>();
            SimpleIoc.Default.Register<PlaylistsViewModel>();
            SimpleIoc.Default.Register<PlaylistViewModel>();
            SimpleIoc.Default.Register<RightPanelViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<TagsEditorViewModel>();
            SimpleIoc.Default.Register<TestViewModel>();
        }

        public AddToPlaylistViewModel AddToPlaylistVM => ServiceLocator.Current.GetInstance<AddToPlaylistViewModel>();
        public AlbumViewModel AlbumVM => ServiceLocator.Current.GetInstance<AlbumViewModel>();
        public AlbumsViewModel AlbumsVM => ServiceLocator.Current.GetInstance<AlbumsViewModel>();
        public ArtistsViewModel ArtistsVM => ServiceLocator.Current.GetInstance<ArtistsViewModel>();
        public ArtistViewModel ArtistVM => ServiceLocator.Current.GetInstance<ArtistViewModel>();
        public FileInfoViewModel FileInfoVM => ServiceLocator.Current.GetInstance<FileInfoViewModel>();
        public FoldersViewModel FoldersVM => ServiceLocator.Current.GetInstance<FoldersViewModel>();
        public GenresViewModel GenresVM => ServiceLocator.Current.GetInstance<GenresViewModel>();
        public NowPlayingViewModel NowPlayingVM => ServiceLocator.Current.GetInstance<NowPlayingViewModel>();
        public NowPlayingPlaylistViewModel NowPlayingPlaylistVM => ServiceLocator.Current.GetInstance<NowPlayingPlaylistViewModel>();
        public PlaylistViewModel PlaylistVM => ServiceLocator.Current.GetInstance<PlaylistViewModel>();
        public PlaylistsViewModel PlaylistsVM => ServiceLocator.Current.GetInstance<PlaylistsViewModel>();
        public RightPanelViewModel RightPanelVM => ServiceLocator.Current.GetInstance<RightPanelViewModel>();
        public SettingsViewModel SettingsVM => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public SongsViewModel SongsVM => ServiceLocator.Current.GetInstance<SongsViewModel>();
        public TagsEditorViewModel TagsEditorVM => ServiceLocator.Current.GetInstance<TagsEditorViewModel>();
        public TestViewModel TestVM => ServiceLocator.Current.GetInstance<TestViewModel>();
    }
}
