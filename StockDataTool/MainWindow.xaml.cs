
using System;
using System.Windows;
using System.ComponentModel;


namespace StockDataTool
{
    public partial class MainWindow : Window
    {
        BackgroundWorker bw;

        public MainWindow()
        {
            bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += Bw_DoWork;
            bw.ProgressChanged += Bw_ProgressChanged;
            InitializeComponent();
            progressBar.Maximum = 150;
            bw.RunWorkerAsync();
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            logTextBox.Text += e.UserState.ToString();
            progressBar.Value = e.ProgressPercentage;
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //0. Downloading available tickers
            bw.ReportProgress(10, "Downloading available tickers");
            StockDataLoader.GetAllTickers();
            bw.ReportProgress(20, "...done!\r\n");

            // 1. Building Portfolio: Tickers. Curerntly the list is pre-defined, may be switched to downloaded at {0}
            bw.ReportProgress(30, "Building portfolio");
            Portfolio portfolio = new Portfolio();
            bw.ReportProgress(40, "...done!\r\n");

            //2. Downloading historical prices data
            bw.ReportProgress(50, "Downloading historical prices");
            foreach (Stock stock in portfolio.Stocks)
            {
                var longPath = StockDataLoader.DownloadPricesCsv(stock.Ticker, 2006, 2015);
                var shortPath = StockDataLoader.ReformatPricesCsv(longPath);
                StockDataLoader.CreateHistoricalData(stock, shortPath, ref portfolio);
            }
            bw.ReportProgress(60, "...done!\r\n");

            bw.ReportProgress(70, "Enriching stocks with AAR data");
            //3. Enriching stocks with AAR data
            StockDataLoader.EnrichStocksWithAAR(portfolio);
            bw.ReportProgress(80, "...done!\r\n");


            //4. Enriching stocks with STDs
            bw.ReportProgress(90, "Enriching stocks with STDs");
            StockDataLoader.EnrichStocksWithSTD(portfolio);
            bw.ReportProgress(100, "...done!\r\n");

            //5. Enriching stocks with P/Es
            bw.ReportProgress(110, "Enriching PEs and PBs\r\n");
            foreach (Stock stock in portfolio.Stocks)
            {
                StockDataLoader.GetCurrentMorningstarData(stock);
                bw.ReportProgress(110, $"\tEnriching:{stock.Ticker}\r\n");
                StockDataLoader.GetHistoricalMorningstarData(stock);
            }
            bw.ReportProgress(120, "...done!\r\n");

            /*n. Generating output. Currently contains:
                - raw data in rows
                - ticker
                - AAR
            */
            bw.ReportProgress(130, "Generating xls");
            StockDataLoader.GenerateMySpreadsheet(ref portfolio);
            bw.ReportProgress(140, "...done!\r\n");

            bw.ReportProgress(150, "All done!\r\n");
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();


        }

        private void Window_Closed(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"C:\CSharp\StockDataTool\StockDataTool\bin\Debug");
        }
    }
}
