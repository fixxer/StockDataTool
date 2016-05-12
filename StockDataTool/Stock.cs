

namespace StockDataTool
{
    public enum ExchangeEnum
    {
        NASDAQ, NYSE,
    }

    class Stock
    {
		public string Ticker { get; set; }
        public ExchangeEnum Exchange { get; set; }
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
