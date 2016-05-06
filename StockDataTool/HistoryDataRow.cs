
namespace StockDataTool
{
    class HistoryDataRow
    {
        public string Stock { get; set; }
        public int Year { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double AnnualReturn { get; set; }
        public double PE { get; set; }
        public double PB { get; set; }

        public HistoryDataRow(string stock, int year, double open, double close, double ar, double pe, double pb)
        {
            this.Stock = stock;
            this.Year = year;
            this.Open = open;
            this.Close = close;
            this.AnnualReturn = ar;
            this.PE = pe;
            this.PB = pb;
        }
        public HistoryDataRow()
        {

        }

    }
}
