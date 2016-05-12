

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
        /*
		stock type
		stock style
		aar
		std
		RR
		PE
		avg.PE
		max.PE
		ind.avg.PE
		PB
		Divident Yield
		*/
    }
}
