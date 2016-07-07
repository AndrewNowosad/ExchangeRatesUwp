using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
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
            pbLoading.Visibility = Visibility.Visible;
            try {Singletone.Course.Load(await CbrApi.GetDailyQuotation());}
            catch { }
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
            UpdateTiles();
        }

        private void UpdateTiles()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            string s = $@"<tile>
                           <visual displayName='Exchange Rates'>
                             <binding template='TileSmall' hint-textStacking='center'>
                               <text hint-align='center' hint-style='body'>USD</text>
                               <text hint-align='center'>{Singletone.Course["USD"].ValueOf1Unit:0.00} &#8381;</text>
                             </binding>
                             <binding template='TileMedium' branding='name'>
                               <text hint-wrap='true' hint-maxLines='2'>Курс {Singletone.Course.Date:d}:</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["USD"].Nominal} USD = {Singletone.Course["USD"].Value:0.00} руб.</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["EUR"].Nominal} EUR = {Singletone.Course["EUR"].Value:0.00} руб.</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["UAH"].Nominal} UAH = {Singletone.Course["UAH"].Value:0.00} руб.</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["CNY"].Nominal} CNY = {Singletone.Course["CNY"].Value:0.00} руб.</text>
                             </binding>
                             <binding template='TileWide' branding='nameAndLogo'>
                               <text>Курсы валют на {Singletone.Course.Date:d}:</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["USD"]}</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["EUR"]}</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["UAH"]}</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["CNY"]}</text>
                             </binding>
                             <binding template='TileLarge' branding='nameAndLogo'>
                               <text hint-wrap='true'>Курсы валют на {Singletone.Course.Date:d}:</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["USD"]}</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["EUR"]}</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["UAH"]}</text>
                               <text hint-style='captionSubtle'>{Singletone.Course["CNY"]}</text>
                             </binding>
                           </visual>
                         </tile>";
            XmlDocument document = new XmlDocument();
            document.LoadXml(s);
            TileNotification notification = new TileNotification(document);
            updater.Update(notification);
        }
    }
}
