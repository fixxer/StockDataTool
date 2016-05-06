
using System;
using System.Windows;


namespace StockDataTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            foreach (string stock in Settings.Portfolio)
            {
                var resultPath = StockDataLoader.DownloadPricesCsv(stock, 2006, 2015);
                StockDataLoader.ReformatPricesCsv(resultPath); 
            }
            this.Close();
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            

            
        }
    }
}
