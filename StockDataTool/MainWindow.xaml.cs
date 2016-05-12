
using System;
using System.Windows;


namespace StockDataTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Portfolio portfolio = new Portfolio();
            StockDataLoader.GetAllTickers(portfolio);
            foreach (string stock in portfolio.Stocks)
            {
                var longPath = StockDataLoader.DownloadPricesCsv(stock, 2006, 2015);
                var shortPath = StockDataLoader.ReformatPricesCsv(longPath);
                StockDataLoader.CreateHistoricalData(stock, shortPath, ref portfolio);
            }
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
