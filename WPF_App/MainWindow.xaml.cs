using System.Net.Http;
using System.Windows;
using WPF_App.Api;
using WPF_App.ViewModels;

namespace WPF_App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:3001/") };
            DataContext = new MainViewModel(new HeroesApiClient(httpClient));
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            await ((MainViewModel)DataContext).LoadAsync();
        }
    }
}