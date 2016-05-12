using System.Collections.Generic;

namespace StockDataTool
{
    class Portfolio
    {
        public List<string> Stocks;
        public List<HistoryDataRow> HistoryDataRows;

        public List<string> Industries = new List<string>
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
            Stocks = new List<string> { "AAPL", "NVDA", "FB", "MA" };
            HistoryDataRows = new List<HistoryDataRow>();
        }

    }
}
