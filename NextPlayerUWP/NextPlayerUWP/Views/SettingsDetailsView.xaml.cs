using NextPlayerUWP.ViewModels;
using NextPlayerUWP.ViewModels.Settings;
using Template10.Services.SerializationService;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsDetailsView : Page
    {
        ISettingsViewModel ViewModel { get; set; }

        public SettingsDetailsView()
        {
            this.InitializeComponent();
        }

        //~SettingsDetailsView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.OnNavigatedToNewView(true);
            // Parameter is item ID
            ViewModelLocator vml = new ViewModelLocator();
            var service = vml.SettingsVMService;
            string name = SerializationService.Json.Deserialize(e.Parameter?.ToString()) as string;
            ViewModel = service.GetViewModelByName(name);
            DetailContentPresenter.Content = ViewModel;
        }
    }
}
