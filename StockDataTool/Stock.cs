using System.Collections.Generic;

namespace StockDataTool
{
    public enum Exchange
    {
        NASDAQ, NYSE,
    }
    
    class Stock
    {
		public string Ticker { get; set; }
        public Exchange Exchange { get; set; }

        public string Industry { get; set; }
        public string Sector { get; set; }
        public string Style { get; set; }

        public List<HistoryDataRow> dataRows { get; set; }
        public double AAR { get; set; }
        public double STD { get; set; }
        public double PE { get;set;}
        public double industryPE { get; set; }
        public double PB { get; set; }
        public double industryPB { get; set; }

        public double AvgPE { get; set; }
        public double MaxPE { get; set; }
        public double AvgPB { get; set; }
        public double MaxPB { get; set; }
        public double DividentYield { get; set; }


        public Stock() { dataRows = new List<HistoryDataRow>(); }
        public Stock(string ticker, Exchange ex)
        {
            Ticker = ticker;
            Exchange = ex;
            dataRows = new List<HistoryDataRow>();
        }
    }
}
