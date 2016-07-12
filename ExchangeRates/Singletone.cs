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
        static public TimeSpan UpdatePeriodicity = new TimeSpan(1, 0, 0);

        static DateTime LastUpdate;
        static readonly string SettingsFileName = "Settings.xml";
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
        }

        static public async Task SaveSettings()
        {
            string data = $@"<?xml version='1.0' encoding='utf-8' ?><Settings><AppTheme>{(int)AppTheme}</AppTheme><TileLinesCounter>{TileLinesCounter}</TileLinesCounter>";
            for (int i = 0; i < TileLinesMax; ++i)
                data += $@"<TileLine{i}>{TileLines[i]}</TileLine{i}>";
            data += $@"</Settings>";
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);
            await SaveXmlFileToLocalStorage(SettingsFileName, xml);
        }

        static public async Task LoadCourse()
        {
            if (DateTime.Now < LastUpdate + UpdatePeriodicity) return;

            bool tileNeedUpdating = false;

            CbrCourse tempLastCourse = new CbrCourse();
            try { tempLastCourse.Load(await CbrApi.GetDailyQuotation()); }
            catch { tempLastCourse.Load(await LoadXmlFileFromLocalStorage(LastCourseFileName)); }
            if (tempLastCourse.Date != LastCourse.Date)
            {
                LastCourse = tempLastCourse;
                await SaveXmlFileToLocalStorage(LastCourseFileName, LastCourse.GetXml());
                LastUpdate = DateTime.Now;
                tileNeedUpdating = true;
            }

            if (Course.Date < DateTime.Today)
            {
                tileNeedUpdating = true;
                if (LastCourse.Date == DateTime.Today)
                {
                    Course = LastCourse;
                    await SaveXmlFileToLocalStorage(CurrentCourseFileName, Course.GetXml());
                    LastUpdate = DateTime.Now;
                }
                else
                {
                    CbrCourse tempCurrentCourse = new CbrCourse();
                    try { tempCurrentCourse.Load(await CbrApi.GetDailyQuotation(DateTime.Today)); }
                    catch { tempCurrentCourse.Load(await LoadXmlFileFromLocalStorage(CurrentCourseFileName)); }
                    if (tempCurrentCourse.Date == DateTime.Today)
                    {
                        Course = tempCurrentCourse;
                        await SaveXmlFileToLocalStorage(CurrentCourseFileName, Course.GetXml());
                        LastUpdate = DateTime.Now;
                    }
                }
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
            document.LoadXml(CreateTileLayout(Course, "Сегодня:"));
            TileNotification notification = new TileNotification(document);
            notification.Tag = "today";
            notification.ExpirationTime = DateTime.Today.AddDays(1);
            updater.Update(notification);

            if (LastCourse.Date != DateTime.Today.AddDays(1)) return;
            document.LoadXml(CreateTileLayout(LastCourse, "Завтра:"));
            notification = new TileNotification(document);
            notification.Tag = "tomorrow";
            notification.ExpirationTime = DateTime.Today.AddDays(1);
            updater.Update(notification);
        }

        static string CreateTileLayout(CbrCourse course, string caption)
        {
            Valute v = course[TileLines[0]];
            string tileSmallContent =$@"<text hint-align='center' hint-style='base'>{v.CharCode}</text>
                                        <text hint-align='center'>{v.ValueOf1Unit:0.00} &#8381;</text>";

            string tileMediumContent = $@"<text>{caption}</text>";
            string tileWideContent = $@"<text>{caption}</text>";
            for (int i = 0; i < TileLinesCounter; ++i)
            {
                v = course[TileLines[i]];
                tileMediumContent +=
                    $@"<text hint-style='captionSubtle'>{v.Nominal} {v.CharCode} = {v.Value:0.00} &#8381;</text>";
                tileWideContent +=
                    $@"<text hint-style='captionSubtle'>{v}</text>";
            }

            string layout = $@"<tile>
                                 <visual displayName='Обновлено: {DateTime.Now:t}'>
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

            return layout;
        }
    }
}
