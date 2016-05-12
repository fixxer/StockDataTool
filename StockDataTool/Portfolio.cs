using System.Collections.Generic;

namespace StockDataTool
{
    class Portfolio
    {
        public List<Stock> Stocks;
        //public List<HistoryDataRow> HistoryDataRows;

        public static List<string> Industries = new List<string>
        {
            "Basic+Industries",
            "Capital+Goods",
            "Consumer+Durable",
            "Consumer+Non-Durables",
            "Consumer+Services",
            "Energy",
            "Finance",
            "Health+Care",
            "Miscellaneous",
            "Public+Utilities",
            "Technology",
            "Transportation",
        };

        public Portfolio()
        {
            Stocks = new List<Stock>();
            Stocks.Add(new Stock("AAPL", Exchange.NASDAQ));
            Stocks.Add(new Stock("NVDA", Exchange.NASDAQ));
            Stocks.Add(new Stock("FB", Exchange.NASDAQ));
            Stocks.Add(new Stock("MA", Exchange.NYSE));
            //HistoryDataRows = new List<HistoryDataRow>();
        }

    }
}
