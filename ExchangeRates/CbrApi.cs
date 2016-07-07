using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExchangeRates
{
    static class CbrApi
    {
        static readonly string pathCbrApi = "http://www.cbr.ru/scripts/";
        static readonly string dailyQuotation = "XML_daily.asp";

        static private async Task<XmlDocument> QueryApiCbr(string query)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pathCbrApi + query);
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251));
            string data = await reader.ReadToEndAsync();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);
            return xml;
        }

        static public async Task<XmlDocument> GetDailyQuotation()
        {
            return await QueryApiCbr(dailyQuotation);
        }

        static public async Task<XmlDocument> GetDailyQuotation(DateTime Date)
        {
            return await QueryApiCbr($"{dailyQuotation}?date_req={Date:dd/MM/yyyy}");
        }
    }
}
