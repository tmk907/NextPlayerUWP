﻿using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NextPlayerUWP.Common;
using NextPlayerUWP.Extensions;

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
            SimpleIoc.Default.Register<FoldersRootViewModel>();
            SimpleIoc.Default.Register<GenresViewModel>();
            SimpleIoc.Default.Register<LyricsViewModel>();
            SimpleIoc.Default.Register<LyricsPanelViewModel>();
            SimpleIoc.Default.Register<NewSmartPlaylistViewModel>();          
            SimpleIoc.Default.Register<NowPlayingViewModel>();
            SimpleIoc.Default.Register<NowPlayingDesktopViewModel>();
            SimpleIoc.Default.Register<NowPlayingPlaylistViewModel>();
            SimpleIoc.Default.Register<NowPlayingPlaylistPanelViewModel>();
            SimpleIoc.Default.Register<PlaylistsViewModel>();
            SimpleIoc.Default.Register<PlaylistViewModel>();
            SimpleIoc.Default.Register<RadiosViewModel>();
            SimpleIoc.Default.Register<RightPanelViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
            SimpleIoc.Default.Register<TagsEditorViewModel>();
            SimpleIoc.Default.Register<TestViewModel>();
            SimpleIoc.Default.Register<PlayerViewModelBase>();
            SimpleIoc.Default.Register<QueueViewModelBase>();
            SimpleIoc.Default.Register<CuteRadioViewModel>();

            SimpleIoc.Default.Register<SettingsVMService>();
            SimpleIoc.Default.Register<MyLyricsExtensionsClient>();
            SimpleIoc.Default.Register<FoldersVMHelper>();

            
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
        public FoldersRootViewModel FoldersRootVM => ServiceLocator.Current.GetInstance<FoldersRootViewModel>();
        public GenresViewModel GenresVM => ServiceLocator.Current.GetInstance<GenresViewModel>();
        public LyricsViewModel LyricsVM => ServiceLocator.Current.GetInstance<LyricsViewModel>();
        public LyricsPanelViewModel LyricsPanelVM => ServiceLocator.Current.GetInstance<LyricsPanelViewModel>();
        public NewSmartPlaylistViewModel NewSmartPlaylistVM => ServiceLocator.Current.GetInstance<NewSmartPlaylistViewModel>();
        public NowPlayingViewModel NowPlayingVM => ServiceLocator.Current.GetInstance<NowPlayingViewModel>();
        public NowPlayingPlaylistViewModel NowPlayingPlaylistVM => ServiceLocator.Current.GetInstance<NowPlayingPlaylistViewModel>();
        public NowPlayingPlaylistPanelViewModel NowPlayingPlaylistPanelVM => ServiceLocator.Current.GetInstance<NowPlayingPlaylistPanelViewModel>();
        public NowPlayingDesktopViewModel NowPlayingDesktopVM => ServiceLocator.Current.GetInstance<NowPlayingDesktopViewModel>();
        public PlaylistViewModel PlaylistVM => ServiceLocator.Current.GetInstance<PlaylistViewModel>();
        public PlaylistsViewModel PlaylistsVM => ServiceLocator.Current.GetInstance<PlaylistsViewModel>();
        public RadiosViewModel RadiosVM => ServiceLocator.Current.GetInstance<RadiosViewModel>();
        public RightPanelViewModel RightPanelVM => ServiceLocator.Current.GetInstance<RightPanelViewModel>();
        public SongsViewModel SongsVM => ServiceLocator.Current.GetInstance<SongsViewModel>();
        public TagsEditorViewModel TagsEditorVM => ServiceLocator.Current.GetInstance<TagsEditorViewModel>();
        public TestViewModel TestVM => ServiceLocator.Current.GetInstance<TestViewModel>();
        public PlayerViewModelBase PlayerVM => ServiceLocator.Current.GetInstance<PlayerViewModelBase>();
        public QueueViewModelBase QueueVM => ServiceLocator.Current.GetInstance<QueueViewModelBase>();
        public CuteRadioViewModel CuteRadioVM => ServiceLocator.Current.GetInstance<CuteRadioViewModel>();

        public SettingsVMService SettingsVMService => ServiceLocator.Current.GetInstance<SettingsVMService>();
        public MyLyricsExtensionsClient LyricsExtensionsClient => ServiceLocator.Current.GetInstance<MyLyricsExtensionsClient>();
        public FoldersVMHelper FolderVMHelper => ServiceLocator.Current.GetInstance<FoldersVMHelper>();
    }
}
