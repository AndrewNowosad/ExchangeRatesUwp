using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
                    await Singletone.GoToStoreForReview();
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

        private void btPane_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = true;
        }

        private void tbValuteInfo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Singletone.Course.Count == 0) return;
            if (puDetail.IsOpen) return;
            TextBlock textBlock = sender as TextBlock;
            Valute valute = textBlock.DataContext as Valute;
            spPop.Width = Window.Current.Bounds.Width * 0.8 + 5;
            spPopLeft.Width = spPopRight.Width = Window.Current.Bounds.Width * 0.4;
            tbDName.Text = $"{valute.Name}";
            if (valute.Nominal != 1)
                tbDStandart.Visibility = Visibility.Visible;
            else
                tbDStandart.Visibility = Visibility.Collapsed;
            tbDStandart.Text = $"{valute.Nominal} {valute.CharCode} = {valute.Value} RUB";
            tbDUnit.Text = $"1 {valute.CharCode} = {valute.ValueOf1Unit:0.0000} RUB";
            tbDReciprocal.Text = $"1 RUB = {(1.0 / valute.ValueOf1Unit):0.0000} {valute.CharCode}";
            puDetail.VerticalOffset = Math.Min(e.GetPosition(svMenu).Y, Window.Current.Bounds.Height - 150);
            puDetail.HorizontalOffset = -spPop.Width / 2;
            ShowPopDetail = true;
        }

        bool ShowPopDetail = false;

        private void Page_Tapped(object sender, TappedRoutedEventArgs e)
        {
            puDetail.IsOpen = ShowPopDetail;
            ShowPopDetail = false;
        }

        private void btBack_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
        }

        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            Frame.Navigate(typeof(SettingsPage));
        }

        private async void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            await ReloadData();
        }

        private void btAbout_Click(object sender, RoutedEventArgs e)
        {
            svMenu.IsPaneOpen = false;
            Frame.Navigate(typeof(AboutPage));
        }

        struct Swipe
        {
            public double x1, x2, y1, y2;
            public bool f;
        }
        Swipe swipe;

        private void Page_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            swipe.x1 = e.Position.X;
            swipe.y1 = e.Position.Y;
            swipe.f = true;
        }

        private void Page_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (!swipe.f) return;
            swipe.x2 = e.Position.X;
            swipe.y2 = e.Position.Y;
            if (Math.Abs(swipe.y1 - swipe.y2) > 100) swipe.f = false;
        }

        private void Page_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (!swipe.f) return;
            swipe.f = false;
            if (Math.Abs(swipe.x1 - swipe.x2) < 100) return;
            svMenu.IsPaneOpen = swipe.x1 < swipe.x2;
        }
    }
}