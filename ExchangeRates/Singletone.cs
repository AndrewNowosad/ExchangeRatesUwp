using Windows.UI.Xaml;

namespace ExchangeRates
{
    static class Singletone
    {
        static public CbrCourse Course = new CbrCourse();
        static public ElementTheme AppTheme = ElementTheme.Default;
        static public int TileLinesCounter = 0;
        static public readonly int TileLinesMax = 4;
        static public string[] TileLines = new string[4];
    }
}
