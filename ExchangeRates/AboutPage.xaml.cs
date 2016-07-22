using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ExchangeRates
{
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
            await Singletone.GoToStoreForReview();
        }

        private async void btMail_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri($@"mailto:nowosad@inbox.ru"));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string version = "";
            var uri = new Uri("ms-appx:///AppxManifest.xml");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (var rastream = await file.OpenReadAsync())
            using (var appManifestStream = rastream.AsStreamForRead())
            {
                using (var reader = XmlReader.Create(appManifestStream, new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true }))
                {
                    var doc = XDocument.Load(reader);
                    var app = doc.Descendants(doc.Root.Name.Namespace + "Identity").FirstOrDefault();
                    if (app != null)
                    {
                        var versionAttribute = app.Attribute("Version");
                        if (versionAttribute != null)
                        {
                            version = versionAttribute.Value;
                        }
                    }
                }
            }
            tbVer.Text = $"Версия: {version}";
        }
    }
}