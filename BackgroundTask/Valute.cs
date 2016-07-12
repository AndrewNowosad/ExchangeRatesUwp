using System.Globalization;
using System.Xml;

namespace ExchangeRates
{
    class Valute
    {
        static CultureInfo cInfo = new CultureInfo("ru-RU");
        public int NumCode { get; }
        public string CharCode { get; }
        public int Nominal { get; }
        public string Name { get; }
        public float Value { get; }
        public float ValueOf1Unit { get { return Value / Nominal; } }

        public static Valute Create(XmlNode nodeValute)
        {
            XmlNode numCode = nodeValute["NumCode"];
            XmlNode charCode = nodeValute["CharCode"];
            XmlNode nominal = nodeValute["Nominal"];
            XmlNode name = nodeValute["Name"];
            XmlNode value = nodeValute["Value"];
            return new Valute(
                numCode.InnerText,
                charCode.InnerText,
                nominal.InnerText,
                name.InnerText,
                value.InnerText);
        }

        public Valute(int NumCode, string CharCode, int Nominal, string Name, float Value)
        {
            this.NumCode = NumCode;
            this.CharCode = CharCode;
            this.Nominal = Nominal;
            this.Name = Name;
            this.Value = Value;
        }

        public Valute(string NumCode, string CharCode, string Nominal, string Name, string Value)
            : this(
                  int.Parse(NumCode),
                  CharCode,
                  int.Parse(Nominal),
                  Name,
                  float.Parse(Value, cInfo)
                  )
        { }

        public override string ToString() => $"{Nominal} {Name} = {Value.ToString("0.00", cInfo)} руб.";
    }
}