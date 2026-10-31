using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NextPlayerWinUI.Common;
//using NextPlayerWinUI.Extensions;

namespace NextPlayerWinUI.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {

            var services = new ServiceCollection();

            services.AddSingleton<AddToPlaylistViewModel>();
            services.AddSingleton<AlbumsViewModel>();
            services.AddSingleton<AlbumViewModel>();
            services.AddSingleton<AlbumArtistsViewModel>();
            services.AddSingleton<AlbumArtistViewModel>();
            services.AddSingleton<ArtistsViewModel>();
            services.AddSingleton<ArtistViewModel>();
            services.AddSingleton<AudioSettingsViewModel>();
            services.AddSingleton<BottomPlayerViewModel>();
            services.AddSingleton<CloudStorageFoldersViewModel>();
            services.AddSingleton<FileInfoViewModel>();
            services.AddSingleton<FoldersViewModel>();
            services.AddSingleton<FoldersRootViewModel>();
            services.AddSingleton<GenresViewModel>();
            services.AddSingleton<LyricsViewModel>();
            services.AddSingleton<LyricsPanelViewModel>();
            services.AddSingleton<NewSmartPlaylistViewModel>();          
            services.AddSingleton<NowPlayingViewModel>();
            services.AddSingleton<NowPlayingDesktopViewModel>();
            services.AddSingleton<NowPlayingPlaylistViewModel>();
            services.AddSingleton<NowPlayingPlaylistPanelViewModel>();
            services.AddSingleton<PlaylistsViewModel>();
            services.AddSingleton<PlaylistViewModel>();
            services.AddSingleton<RadiosViewModel>();
            services.AddSingleton<RightPanelViewModel>();
            services.AddSingleton<SongsViewModel>();
            services.AddSingleton<TestViewModel>();
            services.AddSingleton<PlayerViewModelBase>();
            services.AddSingleton<QueueViewModelBase>();
            services.AddSingleton<CuteRadioViewModel>();

            services.AddSingleton<SettingsVMService>();
            //services.AddSingleton<MyLyricsExtensionsClient>();
            services.AddSingleton<FoldersVMHelper>();


            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        public AddToPlaylistViewModel AddToPlaylistVM => Ioc.Default.GetRequiredService<AddToPlaylistViewModel>();
        public AlbumViewModel AlbumVM => Ioc.Default.GetRequiredService<AlbumViewModel>();
        public AlbumsViewModel AlbumsVM => Ioc.Default.GetRequiredService<AlbumsViewModel>();
        public AlbumArtistsViewModel AlbumArtistsVM => Ioc.Default.GetRequiredService<AlbumArtistsViewModel>();
        public AlbumArtistViewModel AlbumArtistVM => Ioc.Default.GetRequiredService<AlbumArtistViewModel>();
        public ArtistsViewModel ArtistsVM => Ioc.Default.GetRequiredService<ArtistsViewModel>();
        public ArtistViewModel ArtistVM => Ioc.Default.GetRequiredService<ArtistViewModel>();
        public AudioSettingsViewModel AudioSettingsVM => Ioc.Default.GetRequiredService<AudioSettingsViewModel>();
        public BottomPlayerViewModel BottomPlayerVM => Ioc.Default.GetRequiredService<BottomPlayerViewModel>();
        public CloudStorageFoldersViewModel CloudStorageFoldersVM => Ioc.Default.GetRequiredService<CloudStorageFoldersViewModel>();
        public FileInfoViewModel FileInfoVM => Ioc.Default.GetRequiredService<FileInfoViewModel>();
        public FoldersViewModel FoldersVM => Ioc.Default.GetRequiredService<FoldersViewModel>();
        public FoldersRootViewModel FoldersRootVM => Ioc.Default.GetRequiredService<FoldersRootViewModel>();
        public GenresViewModel GenresVM => Ioc.Default.GetRequiredService<GenresViewModel>();
        public LyricsViewModel LyricsVM => Ioc.Default.GetRequiredService<LyricsViewModel>();
        public LyricsPanelViewModel LyricsPanelVM => Ioc.Default.GetRequiredService<LyricsPanelViewModel>();
        public NewSmartPlaylistViewModel NewSmartPlaylistVM => Ioc.Default.GetRequiredService<NewSmartPlaylistViewModel>();
        public NowPlayingViewModel NowPlayingVM => Ioc.Default.GetRequiredService<NowPlayingViewModel>();
        public NowPlayingPlaylistViewModel NowPlayingPlaylistVM => Ioc.Default.GetRequiredService<NowPlayingPlaylistViewModel>();
        public NowPlayingPlaylistPanelViewModel NowPlayingPlaylistPanelVM => Ioc.Default.GetRequiredService<NowPlayingPlaylistPanelViewModel>();
        public NowPlayingDesktopViewModel NowPlayingDesktopVM => Ioc.Default.GetRequiredService<NowPlayingDesktopViewModel>();
        public PlaylistViewModel PlaylistVM => Ioc.Default.GetRequiredService<PlaylistViewModel>();
        public PlaylistsViewModel PlaylistsVM => Ioc.Default.GetRequiredService<PlaylistsViewModel>();
        public RadiosViewModel RadiosVM => Ioc.Default.GetRequiredService<RadiosViewModel>();
        public RightPanelViewModel RightPanelVM => Ioc.Default.GetRequiredService<RightPanelViewModel>();
        public SongsViewModel SongsVM => Ioc.Default.GetRequiredService<SongsViewModel>();
        public TestViewModel TestVM => Ioc.Default.GetRequiredService<TestViewModel>();
        public PlayerViewModelBase PlayerVM => Ioc.Default.GetRequiredService<PlayerViewModelBase>();
        public QueueViewModelBase QueueVM => Ioc.Default.GetRequiredService<QueueViewModelBase>();
        public CuteRadioViewModel CuteRadioVM => Ioc.Default.GetRequiredService<CuteRadioViewModel>();

        public SettingsVMService SettingsVMService => Ioc.Default.GetRequiredService<SettingsVMService>();
        //public MyLyricsExtensionsClient LyricsExtensionsClient => Ioc.Default.GetRequiredService<MyLyricsExtensionsClient>();
        public FoldersVMHelper FolderVMHelper => Ioc.Default.GetRequiredService<FoldersVMHelper>();
    }
}
