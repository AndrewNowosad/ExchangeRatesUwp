using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

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
            if (await Singletone.GetReviewInfo())
            {
                MessageDialog dialog = new MessageDialog("Ваш конструктивный отзыв и оценка помогут разработчику задать приоритетное направление в развитии приложения, а также устранить ошибки и некорректное поведение", "Оставьте отзыв");
                UICommand review = new UICommand("Оценить");
                dialog.Commands.Add(review);
                dialog.Commands.Add(new UICommand("Позже"));
                if (await dialog.ShowAsync() == review)
                {
                    await Singletone.GoToStoreForReview();
                    await Singletone.SetReviewInfo();
                }
            }
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

        private void spBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
        }

        private void spSettings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void spRefresh_Tapped(object sender, TappedRoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            await ReloadData();
        }

        private void spAbout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            Frame.Navigate(typeof(AboutPage));
        }

        private void tbValuteInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Singletone.Course.Count == 0) return;
            TextBlock textBlock = sender as TextBlock;
            Valute valute = textBlock.DataContext as Valute;
            spPop.Width = Window.Current.Bounds.Width / 3 * 2 + 5;
            spPopLeft.Width = spPopRight.Width = Window.Current.Bounds.Width / 3;
            tbDName.Text = $"{valute.Name}";
            tbDStandart.Text = $"{valute.Nominal} {valute.CharCode} = {valute.Value} RUB";
            tbDUnit.Text = $"1 {valute.CharCode} = {valute.ValueOf1Unit:0.0000} RUB";
            tbDReciprocal.Text = $"1 RUB = {(1.0 / valute.ValueOf1Unit):0.0000} {valute.CharCode}";
            FlyoutBase.ShowAttachedFlyout(textBlock);
        }

        private void spPop_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (Resources["flyout"] as Flyout).Hide();
        }
    }
}