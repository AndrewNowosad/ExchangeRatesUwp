using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace ExchangeRates
{
    class CbrCourse : IEnumerable<Valute>
    {
        XmlDocument xml = new XmlDocument();
        List<Valute> Items = new List<Valute>();

        public DateTime Date { get; private set; }
        public int Count { get { return Items.Count; } }
        public Valute this[string ValuteCode] { get { return Items.Find(x => x.CharCode == ValuteCode.ToUpper()); } }

        public void Load(XmlDocument xml)
        {
            if (xml == null) return;
            this.xml = xml;

            XmlNode valCurs = xml["ValCurs"];

            foreach (XmlAttribute valCursAttrib in valCurs.Attributes)
                if (valCursAttrib.Name == "Date")
                    Date = DateTime.Parse(valCursAttrib.Value);

            Items.Clear();
            foreach (XmlNode valute in valCurs.ChildNodes)
                Items.Add(Valute.Create(valute));
        }

        internal void Load(Task<XmlDocument> task)
        {
            throw new NotImplementedException();
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
