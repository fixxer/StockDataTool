
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
            StockDataLoader.DownloadPricesCsv();
        }
    }
}
