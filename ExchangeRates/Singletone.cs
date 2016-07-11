using System;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace ExchangeRates
{
    static class Singletone
    {
        static public CbrCourse Course = new CbrCourse();     // This is the current course for today
        static public CbrCourse LastCourse = new CbrCourse(); // This is the last available course
        static public ElementTheme AppTheme = ElementTheme.Default;
        static public int TileLinesCounter = 0;
        static public readonly int TileLinesMax = 4;
        static public string[] TileLines = new string[4];

        static readonly string SettingsFileName = "Settings.xml";

        static public async Task LoadSettings()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync(SettingsFileName) == null) return;
            StorageFile file = await folder.GetFileAsync(SettingsFileName);
            string data = await FileIO.ReadTextAsync(file, UnicodeEncoding.Utf8);
            XmlDocument xml = new XmlDocument();
            try { xml.LoadXml(data); } catch { return; }
            XmlNode node = xml["Settings"];
            AppTheme = (ElementTheme)int.Parse(node["AppTheme"].InnerText);
            TileLinesCounter = int.Parse(node["TileLinesCounter"].InnerText);
            for (int i = 0; i < TileLinesMax; ++i)
                TileLines[i] = node[$"TileLine{i}"].InnerText;
        }

        static public async Task SaveSettings()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(SettingsFileName, CreationCollisionOption.ReplaceExisting);
            string data = $@"<?xml version='1.0' encoding='utf-8' ?><Settings><AppTheme>{(int)AppTheme}</AppTheme><TileLinesCounter>{TileLinesCounter}</TileLinesCounter>";
            for (int i = 0; i < TileLinesMax; ++i)
                data += $@"<TileLine{i}>{TileLines[i]}</TileLine{i}>";
            data += $@"</Settings>";
            await FileIO.WriteTextAsync(file, data, UnicodeEncoding.Utf8);
        }

        static public void UpdateTiles()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            if (TileLinesCounter == 0)
            {
                updater.Clear();
                return;
            }

            string tileSmallContent = 
                $@"<text hint-align='center' hint-style='base'>{TileLines[0]}</text>
                   <text hint-align='center'>{Course[TileLines[0]].ValueOf1Unit:0.00} &#8381;</text>";

            string tileMediumContent = $@"<text>Сегодня:</text>";
            for (int i = 0; i < TileLinesCounter; ++i)
                tileMediumContent +=
                    $@"<text hint-style='captionSubtle'>{Course[TileLines[i]].Nominal} {TileLines[i]} = {Course[TileLines[i]].Value:0.00} &#8381;</text>";

            string tileWideContent = $@"<text>Сегодня:</text>";
            for (int i = 0; i < TileLinesCounter; ++i)
                tileWideContent +=
                    $@"<text hint-style='captionSubtle'>{Course[TileLines[i]]}</text>";

            string s = $@"<tile>
                           <visual displayName='Курсы валют ЦБ РФ'>
                             <binding template='TileSmall' hint-textStacking='center'>
                               {tileSmallContent}
                             </binding>
                             <binding template='TileMedium' branding='name'>
                               {tileMediumContent}
                             </binding>
                             <binding template='TileWide' branding='name'>
                               {tileWideContent}
                             </binding>
                           </visual>
                         </tile>";
            var document = new Windows.Data.Xml.Dom.XmlDocument();
            document.LoadXml(s);
            TileNotification notification = new TileNotification(document);
            notification.Tag = "current";
            notification.ExpirationTime = DateTime.Today.AddDays(1);
            updater.EnableNotificationQueue(true);
            updater.Update(notification);
        }
    }
}
