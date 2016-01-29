using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainPageViewModel>();
            SimpleIoc.Default.Register<AlbumsViewModel>();
            SimpleIoc.Default.Register<AlbumViewModel>();
            SimpleIoc.Default.Register<ArtistsViewModel>();
            SimpleIoc.Default.Register<ArtistViewModel>();
            SimpleIoc.Default.Register<FoldersViewModel>();
            SimpleIoc.Default.Register<GenresViewModel>();
            SimpleIoc.Default.Register<NowPlayingViewModel>();
            SimpleIoc.Default.Register<PlaylistsViewModel>();
            SimpleIoc.Default.Register<PlaylistViewModel>();
            SimpleIoc.Default.Register<SongsViewModel>();
        }

        public MainPageViewModel MainPageVM => ServiceLocator.Current.GetInstance<MainPageViewModel>();
        public AlbumViewModel AlbumVM => ServiceLocator.Current.GetInstance<AlbumViewModel>();
        public AlbumsViewModel AlbumsVM => ServiceLocator.Current.GetInstance<AlbumsViewModel>();
        public ArtistsViewModel ArtistsVM => ServiceLocator.Current.GetInstance<ArtistsViewModel>();
        public ArtistViewModel ArtistVM => ServiceLocator.Current.GetInstance<ArtistViewModel>();
        public FoldersViewModel FoldersVM => ServiceLocator.Current.GetInstance<FoldersViewModel>();
        public GenresViewModel GenresVM => ServiceLocator.Current.GetInstance<GenresViewModel>();
        public NowPlayingViewModel NowPlayingVM => ServiceLocator.Current.GetInstance<NowPlayingViewModel>();
        public PlaylistViewModel PlaylistVM => ServiceLocator.Current.GetInstance<PlaylistViewModel>();
        public PlaylistsViewModel PlaylistsVM => ServiceLocator.Current.GetInstance<PlaylistsViewModel>();
        public SongsViewModel SongsVM => ServiceLocator.Current.GetInstance<SongsViewModel>();

        //public ViewModel VM => ServiceLocator.Current.GetInstance<MainPageViewModel>();
    }
}
