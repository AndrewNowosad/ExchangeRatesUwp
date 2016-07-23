using System;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace ExchangeRates
{
    static class Singletone
    {
        static public CbrCourse Course = new CbrCourse();
        static public CbrCourse LastCourse = new CbrCourse();
        static public ElementTheme AppTheme = ElementTheme.Default;
        static public int TileLinesCounter = 0;
        static public readonly int TileLinesMax = 4;
        static public string[] TileLines = new string[4];
        static public TimeSpan UpdatePeriodicity = new TimeSpan(6, 0, 0);

        static DateTime LastUpdate;
        static readonly string SettingsFileName = "Settings.xml";
        static readonly string ReviewFileName = "Review.xml";
        static readonly string LastCourseFileName = "LastCourse.xml";
        static readonly string CurrentCourseFileName = "CurrentCourse.xml";

        static async Task<XmlDocument> LoadXmlFileFromLocalStorage(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync(fileName) == null) return null;
            StorageFile file = await folder.GetFileAsync(fileName);
            string data = await FileIO.ReadTextAsync(file, UnicodeEncoding.Utf8);
            XmlDocument xml = new XmlDocument();
            try { xml.LoadXml(data); } catch { return null; }
            return xml;
        }

        static async Task SaveXmlFileToLocalStorage(string fileName, XmlDocument xml)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, xml.OuterXml, UnicodeEncoding.Utf8);
        }

        static public async Task LoadSettings()
        {
            XmlDocument xml = await LoadXmlFileFromLocalStorage(SettingsFileName);
            if (xml == null) return;
            XmlNode node = xml["Settings"];
            AppTheme = (ElementTheme)int.Parse(node["AppTheme"].InnerText);
            TileLinesCounter = int.Parse(node["TileLinesCounter"].InnerText);
            for (int i = 0; i < TileLinesMax; ++i)
                TileLines[i] = node[$"TileLine{i}"].InnerText;
            string s = node["UpdatePeriodicity"].InnerText;
            UpdatePeriodicity = new TimeSpan(0, int.Parse(s), 0);
        }

        static public async Task SaveSettings()
        {
            string data = $@"<?xml version='1.0' encoding='utf-8' ?><Settings><AppTheme>{(int)AppTheme}</AppTheme><TileLinesCounter>{TileLinesCounter}</TileLinesCounter>";
            for (int i = 0; i < TileLinesMax; ++i)
                data += $@"<TileLine{i}>{TileLines[i]}</TileLine{i}>";
            data += $@"<UpdatePeriodicity>{(int)UpdatePeriodicity.TotalMinutes}</UpdatePeriodicity></Settings>";
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);
            await SaveXmlFileToLocalStorage(SettingsFileName, xml);
        }

        static public async Task<bool> GetReviewInfo()
        {
            bool reviewExist = false;
            int startCount = 0;
            XmlDocument xml = await LoadXmlFileFromLocalStorage(ReviewFileName);
            if (xml != null)
            {
                XmlNode node = xml["Settings"];
                reviewExist = bool.Parse(node["ReviewExist"].InnerText);
                startCount = int.Parse(node["StartCount"].InnerText);
            }
            string data = $@"<?xml version='1.0' encoding='utf-8' ?><Settings><ReviewExist>{reviewExist}</ReviewExist><StartCount>{++startCount}</StartCount></Settings>";
            xml = new XmlDocument();
            xml.LoadXml(data);
            await SaveXmlFileToLocalStorage(ReviewFileName, xml);
            return (reviewExist) ? (startCount == 147) : (startCount % 30 == 12);
        }

        static async Task SetReviewInfo()
        {
            string data = $@"<?xml version='1.0' encoding='utf-8' ?><Settings><ReviewExist>{true}</ReviewExist><StartCount>{0}</StartCount></Settings>";
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);
            await SaveXmlFileToLocalStorage(ReviewFileName, xml);
        }

        static public async Task GoToStoreForReview()
        {
            await Launcher.LaunchUriAsync(new Uri(@"ms-windows-store://review/?ProductId=9NBLGGH4T03H"));
            await SetReviewInfo();
        }

        static public async Task LoadCourse()
        {
            if (DateTime.Now < LastUpdate + UpdatePeriodicity) return;

            CbrCourse tempCourse = new CbrCourse();

            bool tileNeedUpdating = false;
            try { tempCourse.Load(await CbrApi.GetDailyQuotation()); }
            catch { tempCourse.Load(await LoadXmlFileFromLocalStorage(LastCourseFileName)); }
            if (tempCourse.Date > LastCourse.Date)
            {
                LastCourse = tempCourse;
                await SaveXmlFileToLocalStorage(LastCourseFileName, LastCourse.GetXml());
                LastUpdate = DateTime.Now;
                tileNeedUpdating = true;
            }

            if (tempCourse.Date > DateTime.Today)
            {
                tempCourse = new CbrCourse();
                try { tempCourse.Load(await CbrApi.GetDailyQuotation(DateTime.Today)); }
                catch { tempCourse.Load(await LoadXmlFileFromLocalStorage(CurrentCourseFileName)); }
            }
            if (tempCourse.Date > Course.Date)
            {
                Course = tempCourse;
                await SaveXmlFileToLocalStorage(CurrentCourseFileName, Course.GetXml());
                LastUpdate = DateTime.Now;
                tileNeedUpdating = true;
            }

            if (tileNeedUpdating)
                UpdateTiles();
        }

        static public void UpdateTiles()
        {
            TileUpdater updater = TileUpdateManager.CreateTileUpdaterForApplication();
            if (TileLinesCounter == 0)
            {
                updater.Clear();
                return;
            }
            updater.EnableNotificationQueue(true);

            var document = new Windows.Data.Xml.Dom.XmlDocument();
            TileNotification notification;

            if (LastCourse.Date == DateTime.Today.AddDays(1))
            {
                document.LoadXml(CreateTileLayout(LastCourse, "Завтра:", false));
                notification = new TileNotification(document);
                notification.Tag = "tomorrow";
                notification.ExpirationTime = DateTime.Today.AddDays(1);
                updater.Update(notification);
            }

            document.LoadXml(CreateTileLayout(Course, "Сегодня:", true));
            notification = new TileNotification(document);
            notification.Tag = "today";
            notification.ExpirationTime = DateTime.Today.AddDays(1);
            updater.Update(notification);
        }

        static string CreateTileLayout(CbrCourse course, string caption, bool needUpdateSmallTile)
        {
            Valute v;

            string tileSmallContent = string.Empty;
            if (needUpdateSmallTile)
            {
                v = course[TileLines[0]];
                tileSmallContent  = $@"<binding template='TileSmall' hint-textStacking='center'>";
                tileSmallContent += $@"<text hint-align='center' hint-style='base'>{v.CharCode}</text>";
                tileSmallContent += $@"<text hint-align='center'>{v.ValueOf1Unit:0.00} &#8381;</text>";
                tileSmallContent += $@"</binding>";
            }

            string tileMediumContent = $@"<binding template='TileMedium' branding='name'><text>{caption}</text>";
            string tileWideContent = $@"<binding template='TileWide' branding='name'><text>{caption}</text>";
            for (int i = 0; i < TileLinesCounter; ++i)
            {
                v = course[TileLines[i]];
                tileMediumContent +=
                    $@"<text hint-style='captionSubtle'>{v.Nominal} {v.CharCode} = {v.Value:0.00} &#8381;</text>";
                tileWideContent +=
                    $@"<text hint-style='captionSubtle'>{v}</text>";
            }
            tileMediumContent += $@"</binding>";
            tileWideContent += $@"</binding>";
            
            return
                $@"<tile><visual>{tileSmallContent}{tileMediumContent}{tileWideContent}</visual></tile>";
        }
    }
}