using System.Net;
using System.IO;
using System.Text;
//using System.Web;

namespace StockDataTool
{
    class StockDataLoader
    {
        //http://financials.morningstar.com/valuation/price-ratio.html?t=AAPL

        public static void DownloadPricesCsv(/*string ticker*/)
        {
            string path = @"http://real-chart.finance.yahoo.com/table.csv?s=AAPL&a=00&b=1&c=2006&d=11&e=31&f=2015&g=d&ignore=.csv"; 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            //string result = readStream.ReadToEnd();
            receiveStream.CopyTo(new FileStream("result.csv", FileMode.Create, FileAccess.ReadWrite));
            receiveStream.Close();
            response.Close();
            readStream.Close();
        }

    }
}
