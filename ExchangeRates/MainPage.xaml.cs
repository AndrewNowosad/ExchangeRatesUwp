using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExchangeRates
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadData();
        }

        private async Task ReloadData()
        {
            //icRates.ItemsSource = null;
            pbLoading.Visibility = Visibility.Visible;
            await Singletone.LoadCourse();
            if (Singletone.Course.Count == 0)
                icRates.ItemsSource = new string[] { "Ошибка загрузки!" };
            else
                icRates.ItemsSource = Singletone.Course;
            pbLoading.Visibility = Visibility.Collapsed;
        }

        private void tbPane_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = true;
        }

        private void tbBack_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
        }

        private void tbSettings_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void tbRefresh_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            await ReloadData();
        }

        private void tbAbout_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
        }
    }
}
