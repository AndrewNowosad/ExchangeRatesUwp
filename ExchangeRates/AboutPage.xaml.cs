using System;
using Windows.ApplicationModel.Store;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ExchangeRates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        DispatcherTimer timer;
        int rot = 0;

        public AboutPage()
        {
            InitializeComponent();

            if (Singletone.AppTheme == ElementTheme.Dark ||
                Singletone.AppTheme == ElementTheme.Default &&
                Application.Current.RequestedTheme == ApplicationTheme.Dark)
                iLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/LogoAbout.png"));
            else
                iLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/LogoAboutBlack.png"));
            
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, object e)
        {
            rot += 1;
            iLogo.RenderTransform = new CompositeTransform() { CenterX = 75, CenterY = 75, Rotation = rot };
            if (rot < 360) return;
            rot = 0;
            timer.Stop();
        }

        private void tbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void iLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!timer.IsEnabled) timer.Start();
        }

        private async void btReview_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($@"ms-windows-store://review/?ProductId={CurrentApp.AppId}"));
        }

        private async void btMail_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($@"mailto://nowosad@inbox.ru"));
        }
    }
}