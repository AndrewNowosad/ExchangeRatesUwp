using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ExchangeRates
{
    public sealed partial class MainPage : Page
    {
        CbrCourse course = new CbrCourse();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ReloadData();
        }

        private async Task ReloadData()
        {
            pbLoading.Visibility = Visibility.Visible;
            await course.LoadCourse();
            if (course.Count == 0)
                icRates.ItemsSource = new string[] { "Ошибка загрузки!" };
            else
                icRates.ItemsSource = course;
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
    }
}
