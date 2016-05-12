

namespace StockDataTool
{
    public enum Exchange
    {
        NASDAQ, NYSE,
    }
    public enum Sector
    {
        a,b,c
    }

    class Stock
    {
		public string Ticker { get; set; }
        public Exchange Exchange { get; set; }
        public Sector Sector { get; set; }
        /*
		sector
		industry
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
