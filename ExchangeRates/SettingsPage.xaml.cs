using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExchangeRates
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            cbTheme.SelectedIndex = (int)Singletone.AppTheme;
            foreach (Valute valute in Singletone.Course)
                for (int i = 0; i < Singletone.TileLinesMax; ++i)
                {
                    cbTileValute[i].Items.Add(new ComboBoxItem { Content = valute.Name });
                    if (valute.CharCode == Singletone.TileLines[i])
                        cbTileValute[i].SelectedIndex = cbTileValute[i].Items.Count - 1;
                }
            for (int i = 0; i < Singletone.TileLinesMax; ++i)
            {
                cbTileValute[i].IsEnabled = i <= Singletone.TileLinesCounter;
                cbTileValute[i].SelectionChanged += cbTileValute_SelectionChanged;
            }
        }

        private void tbBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void cbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Singletone.AppTheme = (ElementTheme)cbTheme.SelectedIndex;
            RequestedTheme = Singletone.AppTheme;
            await Singletone.SaveSettings();
        }

        private async void cbTileValute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Singletone.TileLinesCounter = Singletone.TileLinesMax;
            for (int i = Singletone.TileLinesMax - 1; i >= 0; --i)
            {
                cbTileValute[i].IsEnabled = true;
                Singletone.TileLines[i] = string.Empty;
                foreach (Valute valute in Singletone.Course)
                    if ((string)((cbTileValute[i].SelectedItem as ComboBoxItem)?.Content) == valute.Name)
                        Singletone.TileLines[i] = valute.CharCode;
                if (cbTileValute[i].SelectedIndex == 0)
                {
                    Singletone.TileLinesCounter = i;
                    for (int j = i + 1; j < Singletone.TileLinesMax; ++j)
                        cbTileValute[j].IsEnabled = false;
                }
            }
            Singletone.UpdateTiles();

            if (Singletone.TileLinesCounter == 0)
                await BackgroungTaskManager.RemoveBackgroundTaskAsync();
            else
                await BackgroungTaskManager.RegisterBackgroundTaskAsync();

            await Singletone.SaveSettings();
        }
    }
}
