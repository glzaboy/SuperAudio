using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SuperAudio.ViewModels;
using Windows.Devices.Enumeration;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SuperAudio.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private HomePageViewModel ViewModel { get; }
        public HomePage()
        {
            InitializeComponent();
            ViewModel = App.Host.Services.GetRequiredService<HomePageViewModel>();

            this.DataContext = ViewModel;
        }
        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.InitCommand.Execute(this);
        }
        
    }
}
