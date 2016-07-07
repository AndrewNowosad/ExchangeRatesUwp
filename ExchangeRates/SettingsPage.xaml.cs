using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExchangeRates
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void tbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
