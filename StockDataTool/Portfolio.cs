using System.Collections.Generic;

namespace StockDataTool
{
    class Portfolio
    {
        public List<Stock> Stocks;

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
            Stocks.Add(new Stock("MSFT", Exchange.NASDAQ));
            Stocks.Add(new Stock("FB", Exchange.NASDAQ));
            Stocks.Add(new Stock("AMZN", Exchange.NASDAQ));
            Stocks.Add(new Stock("DIS", Exchange.NYSE));
            Stocks.Add(new Stock("NVDA", Exchange.NASDAQ));
            Stocks.Add(new Stock("ATVI", Exchange.NASDAQ));
            Stocks.Add(new Stock("CRM", Exchange.NYSE));
        }

        public Portfolio(List<string> tickers)
        {
            Stocks = new List<Stock>();
            foreach (string ticker in tickers)
            {
                Stocks.Add(new Stock(ticker, Exchange.NASDAQ));
            }
        }

    }
}
