using System.Threading.Tasks;

namespace NextPlayerUWP.Common
{
    public class PlayerInitializer
    {
        private bool isInitialized = false;
        private bool initializationStarted = false;
        public async Task Init()
        {
            if (isInitialized) return;
            if (!isInitialized && initializationStarted) throw new System.Exception("PlayerInitializer failed");
            System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() Start");
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            initializationStarted = true;

            await NowPlayingPlaylistManager.Current.Init();
            await PlaybackService.Instance.Initialize();

            App.AlbumArtFinder.StartLooking().ConfigureAwait(false);
            s.Stop();
            System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() End {0}ms", s.ElapsedMilliseconds);
            isInitialized = true;
        }
    }
}
