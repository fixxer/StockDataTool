using System.Collections.Generic;

namespace StockDataTool
{
    class Portfolio
    {
        public List<string> Stocks;
        public List<HistoryDataRow> HistoryDataRows;

        public Portfolio()
        {
            Stocks = new List<string> { "AAPL", "NVDA", "FB", "MA" };
            HistoryDataRows = new List<HistoryDataRow>();
        }

    }
}
