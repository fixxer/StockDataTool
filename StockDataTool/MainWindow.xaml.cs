
using System;
using System.Windows;


namespace StockDataTool
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            var resultPath = StockDataLoader.DownloadPricesCsv("AAPL", 2006, 2015);
            var data = StockDataLoader.ReformatPricesCsv(resultPath);

            foreach (string row in data)
            {
                logTextBox.Text += row + Environment.NewLine;
            }
        }
    }
}
