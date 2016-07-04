using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExchangeRates
{
    class CbrCourse : IEnumerable<Valute>
    {
        readonly string pathCbrAsp = "http://www.cbr.ru/scripts/XML_daily.asp";
        XmlDocument xml = new XmlDocument();
        List<Valute> Items = new List<Valute>();

        public DateTime Date { get; private set; }
        public int Count { get { return Items.Count; } }
        public Valute this[string ValuteCode] { get { return Items.Find(x => x.CharCode == ValuteCode.ToUpper()); } }

        public async Task LoadCourse()
        {
            Date = DateTime.Today.Date;
            await LoadXmlDocument(pathCbrAsp);
        }

        public async Task LoadCourse(DateTime Date)
        {
            this.Date = Date.Date;
            string path = pathCbrAsp + "?date_req=" + this.Date.ToString("dd/MM/yyyy");
            await LoadXmlDocument(path);
        }

        private async Task LoadXmlDocument(string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
                HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251));
                string data = await reader.ReadToEndAsync();
                xml.LoadXml(data);
            }
            catch
            {
                return;
            }

            XmlNode valCurs = xml["ValCurs"];

            foreach (XmlAttribute valCursAttrib in valCurs.Attributes)
                if (valCursAttrib.Name == "Date")
                    Date = DateTime.Parse(valCursAttrib.Value);

            Items.Clear();
            foreach (XmlNode valute in valCurs.ChildNodes)
                Items.Add(Valute.Create(valute));
        }

        IEnumerator<Valute> IEnumerable<Valute>.GetEnumerator()
        {
            return ((IEnumerable<Valute>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Valute>)Items).GetEnumerator();
        }
    }
}
