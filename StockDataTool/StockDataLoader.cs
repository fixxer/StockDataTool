using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
//using System.Web;
using System.Windows;

namespace StockDataTool
{
    class StockDataLoader
    {
        //http://financials.morningstar.com/valuation/price-ratio.html?t=AAPL

        public static string DownloadPricesCsv(string ticker, int startYear, int endYear)
        {
            string path = @"http://real-chart.finance.yahoo.com/table.csv";
            string param = "?s=" + ticker + "&a=00&b=1&c=" + startYear + "&d=11&e=31&f=" + endYear + "&g=d&ignore=.csv";
            path += param;

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            //string result = readStream.ReadToEnd();

            string fileName = String.Format("{0}_{1}_{2}_long.csv", ticker, startYear, endYear);
            FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            receiveStream.CopyTo(file);
            receiveStream.Close();
            response.Close();
            readStream.Close();
            file.Close();
            //System.Diagnostics.Process.Start(fileName); //for debug purpose only

            return fileName;
        }

        public static void ReformatPricesCsv(string fileName)
        {
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);
            var dataRows = new List<string>();
            while (!sr.EndOfStream)
            {
                dataRows.Add(sr.ReadLine());
            }
            sr.Close();
            fs.Close();

            var usefulRows = new List<string>();
            usefulRows.Add(dataRows[1]);

            for (int i = 2; i < dataRows.Count - 1; i++)
            {
                //if (i == 1 || i == dataRows.Count - 1) usefulRows.Add(dataRows[i]);
                if ((dataRows[i].Substring(0, 4) != dataRows[i - 1].Substring(0, 4)) || (dataRows[i].Substring(0, 4) != dataRows[i + 1].Substring(0, 4)))
                {
                    usefulRows.Add(dataRows[i]);
                }
            }
            usefulRows.Add(dataRows[dataRows.Count - 1]);

            fileName = fileName.Replace("long", "short");
            FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(file);
            foreach (string row in usefulRows)
            {
                sw.WriteLine(row);
            }
            sw.Close();
            file.Close();
            System.Diagnostics.Process.Start(fileName); //for debug purpose only
        }

        public static void CreateHistoricalData()
        {
            throw new NotImplementedException();
            //foreach (string row in usefulRows)
            //{
            //    MessageBox.Show(row);
            //}

            //string[] parts = currentRow.Split(new char[] { ',' });
            //0 - Date, 1 - Open, 2 - High, 3 - Low, 4 - Close, 5 - Volume, 6 - Adj Close
            //MessageBox.Show(parts[0]);
        }

    }
}
