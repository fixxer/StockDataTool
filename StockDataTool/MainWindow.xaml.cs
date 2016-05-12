
using System;
using System.Windows;


namespace StockDataTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //0. Downloading available tickers
            StockDataLoader.GetAllTickers();

            // 1. Building Portfolio: Tickers. Curerntly the list is pre-defined, may be switched to downloaded at {0}
            Portfolio portfolio = new Portfolio();

            //2. Downloading historical prices data
            foreach (Stock stock in portfolio.Stocks)
            {
                var longPath = StockDataLoader.DownloadPricesCsv(stock.Ticker, 2006, 2015);
                var shortPath = StockDataLoader.ReformatPricesCsv(longPath);
                StockDataLoader.CreateHistoricalData(stock, shortPath, ref portfolio);
            }

            //3. Enriching stocks with AAR data
            StockDataLoader.EnrichStocksWithAAR(portfolio);

            /*n. Generating output. Currently contains:
                - raw data in rows
                - ticker
                - AAR
            */
            StockDataLoader.GenerateMySpreadsheet(ref portfolio);
            this.Close();
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {



        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\CSharp\StockDataTool\StockDataTool\bin\Debug");
        }
    }
}
