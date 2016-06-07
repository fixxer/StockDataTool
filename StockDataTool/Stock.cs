

using System.Collections.Generic;

namespace StockDataTool
{
	
	/*
	http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Basic+Industries&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Capital+Goods&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Consumer+Durable&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Consumer+Non-Durables&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Consumer+Services&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Energy&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Finance&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Health+Care&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Miscellaneous&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Public+Utilities&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Technology&exchange=NASDAQ&render=download
http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Transportation&exchange=NASDAQ&render=download
	*/
	
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
        /*
		stock type
		stock style
		RR
		*/

        public Stock() { dataRows = new List<HistoryDataRow>(); }
        public Stock(string ticker, Exchange ex)
        {
            Ticker = ticker;
            Exchange = ex;
            dataRows = new List<HistoryDataRow>();
        }
    }
}
