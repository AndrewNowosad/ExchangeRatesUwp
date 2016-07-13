using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExchangeRates
{
    public sealed partial class SettingsPage : Page
    {
        int[] updatePeriodicities = new int[] { 30, 60, 180, 360, 720, 1440 };

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

            bool isUpdateOn = Singletone.TileLinesCounter != 0;
            tbUpdatePeriodicityTip1.Opacity = tbUpdatePeriodicityTip2.Opacity = isUpdateOn ? 1.0 : 0.5;
            cbUpdatePeriodicity.IsEnabled = isUpdateOn;

            cbUpdatePeriodicity.SelectedIndex = Array.IndexOf(updatePeriodicities, (int)Singletone.UpdatePeriodicity.TotalMinutes);
            cbUpdatePeriodicity.SelectionChanged += cbUpdatePeriodicity_SelectionChanged;
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
            int tempTileLinesCounter = Singletone.TileLinesMax;
            for (int i = Singletone.TileLinesMax - 1; i >= 0; --i)
            {
                cbTileValute[i].IsEnabled = true;
                Singletone.TileLines[i] = string.Empty;
                foreach (Valute valute in Singletone.Course)
                    if ((string)((cbTileValute[i].SelectedItem as ComboBoxItem)?.Content) == valute.Name)
                        Singletone.TileLines[i] = valute.CharCode;
                if (cbTileValute[i].SelectedIndex == 0)
                {
                    tempTileLinesCounter = i;
                    for (int j = i + 1; j < Singletone.TileLinesMax; ++j)
                        cbTileValute[j].IsEnabled = false;
                }
            }
            bool isNeedUpdateBackgroundTask = (tempTileLinesCounter == 0) ^ (Singletone.TileLinesCounter == 0);
            Singletone.TileLinesCounter = tempTileLinesCounter;
            Singletone.UpdateTiles();
            await Singletone.SaveSettings();

            if (!isNeedUpdateBackgroundTask) return;

            bool isUpdateOn = Singletone.TileLinesCounter != 0;
            tbUpdatePeriodicityTip1.Opacity = tbUpdatePeriodicityTip2.Opacity = isUpdateOn ? 1.0 : 0.5;
            cbUpdatePeriodicity.IsEnabled = isUpdateOn;
            if (isUpdateOn)
                await BackgroungTaskManager.RegisterBackgroundTaskAsync();
            else
                await BackgroungTaskManager.RemoveBackgroundTaskAsync();
        }

        private async void cbUpdatePeriodicity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Singletone.UpdatePeriodicity = new TimeSpan(0, updatePeriodicities[cbUpdatePeriodicity.SelectedIndex], 0);
            await BackgroungTaskManager.RegisterBackgroundTaskAsync();
            await Singletone.SaveSettings();
        }
    }
}
