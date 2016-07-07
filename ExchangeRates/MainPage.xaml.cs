﻿using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExchangeRates
{
    public sealed partial class MainPage : Page
    {
        CbrCourse course;

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
            course = new CbrCourse();
            pbLoading.Visibility = Visibility.Visible;
            try {course.Load(await CbrApi.GetDailyQuotation());}
            catch { }
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

        private async void tbRefresh_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            await ReloadData();
        }

        private void UpdateTiles()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            string s =$@"<tile>
                           <visual displayName='Exchange Rates'>
                             <binding template='TileSmall' hint-textStacking='center'>
                               <text hint-align='center' hint-style='body'>USD</text>
                               <text hint-align='center'>{course["USD"].ValueOf1Unit:0.00} &#8381;</text>
                             </binding>
                             <binding template='TileMedium' branding='name'>
                               <text hint-wrap='true' hint-maxLines='2'>Курс {course.Date:d}:</text>
                               <text hint-style='captionSubtle'>{course["USD"].Nominal} USD = {course["USD"].Value:0.00} руб.</text>
                               <text hint-style='captionSubtle'>{course["EUR"].Nominal} EUR = {course["EUR"].Value:0.00} руб.</text>
                               <text hint-style='captionSubtle'>{course["UAH"].Nominal} UAH = {course["UAH"].Value:0.00} руб.</text>
                               <text hint-style='captionSubtle'>{course["CNY"].Nominal} CNY = {course["CNY"].Value:0.00} руб.</text>
                             </binding>
                             <binding template='TileWide' branding='nameAndLogo'>
                               <text>Курсы валют на {course.Date:d}:</text>
                               <text hint-style='captionSubtle'>{course["USD"]}</text>
                               <text hint-style='captionSubtle'>{course["EUR"]}</text>
                               <text hint-style='captionSubtle'>{course["UAH"]}</text>
                               <text hint-style='captionSubtle'>{course["CNY"]}</text>
                             </binding>
                             <binding template='TileLarge' branding='nameAndLogo'>
                               <text hint-wrap='true'>Курсы валют на {course.Date:d}:</text>
                               <text hint-style='captionSubtle'>{course["USD"]}</text>
                               <text hint-style='captionSubtle'>{course["EUR"]}</text>
                               <text hint-style='captionSubtle'>{course["UAH"]}</text>
                               <text hint-style='captionSubtle'>{course["CNY"]}</text>
                             </binding>
                           </visual>
                         </tile>";
            XmlDocument document = new XmlDocument();
            document.LoadXml(s);
            TileNotification notification = new TileNotification(document);
            updater.Update(notification);
        }

        private void tbAbout_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            UpdateTiles();
        }
    }
}