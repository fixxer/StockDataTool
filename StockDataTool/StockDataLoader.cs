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

        public static void GetAllTickers()
        {
            //http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Transportation&exchange=NASDAQ&render=download
            //http://bsym.bloomberg.com/sym/ - not used
            foreach (string industry in Portfolio.Industries)
            {
                string nasdaqPath = $"http://nasdaq.com/screening/companies-by-industry.aspx?industry={industry}&exchange=NASDAQ&render=download";
                string localPath = $"NASDAQ_{industry}.csv";
                if (!File.Exists(localPath))
                {
                    var request = (HttpWebRequest)WebRequest.Create(nasdaqPath);
                    var response = (HttpWebResponse)request.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                    FileStream file = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite);
                    receiveStream.CopyTo(file);
                    receiveStream.Close();
                    response.Close();
                    readStream.Close();
                    file.Close();
                }
            }
        }


        public static void GetMorningstarData(Stock s)
        {
            //http://financials.morningstar.com/valuation/price-ratio.html?t=AAPL
            //http://financials.morningstar.com/valuate/current-valuation-list.action?&t=XNAS:AAPL
            //http://financials.morningstar.com/valuate/valuation-history.action?&t=XNAS:AAPL&type=price-earnings
            throw new NotImplementedException();
        }


        public static string DownloadPricesCsv(string ticker, int startYear, int endYear)
        {
            string fileName = $"{ticker}_{startYear}_{endYear}_long.csv";
            if (!File.Exists(fileName))
            {

                string path = @"http://real-chart.finance.yahoo.com/table.csv";
                string param = "?s=" + ticker + "&a=00&b=1&c=" + startYear + "&d=11&e=31&f=" + endYear + "&g=d&ignore=.csv";
                path += param;

                var request = (HttpWebRequest)WebRequest.Create(path);
                var response = (HttpWebResponse)request.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                //string result = readStream.ReadToEnd();


                FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                receiveStream.CopyTo(file);
                receiveStream.Close();
                response.Close();
                readStream.Close();
                file.Close();
                //System.Diagnostics.Process.Start(fileName); //for debug purpose only
            }
            return fileName;
        }

        public static string ReformatPricesCsv(string fileName)
        {
            if (!File.Exists(fileName.Replace("long", "short")))
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

                var usefulRows = new List<string> { dataRows[1] };

                for (int i = 2; i < dataRows.Count - 1; i++)
                {
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
                //System.Diagnostics.Process.Start(fileName); //for debug purpose only
            }
            return fileName.Replace("long", "short");
        }

        public static void CreateHistoricalData(Stock stock, string shortPath, ref Portfolio p)
        {
            var fs = new FileStream(shortPath, FileMode.Open, FileAccess.Read);
            var sr = new StreamReader(fs);
            var dataRows = new List<string>();
            while (!sr.EndOfStream)
            {
                dataRows.Add(sr.ReadLine());
            }
            sr.Close();
            fs.Close();

            for (int i = 0; i < dataRows.Count - 1; i += 2)
            {
                //0 - Date, 1 - Open, 2 - High, 3 - Low, 4 - Close, 5 - Volume, 6 - Adj Close
                string[] parts1 = dataRows[i].Split(new char[] { ',' });
                string[] parts2 = dataRows[i + 1].Split(new char[] { ',' });
                int year = int.Parse(parts1[0].Substring(0, 4));
                double open = double.Parse(parts2[6].Replace('.', ','));
                double close = double.Parse(parts1[6].Replace('.', ','));
                var historyDataRow = new HistoryDataRow(stock.Ticker, year, open, close);
                stock.dataRows.Add(historyDataRow);
                //p.HistoryDataRows.Add();
            }
        }

        public static void EnrichStocksWithAAR(Portfolio p)
        {
            foreach (Stock stock in p.Stocks)
            {
                double aar = 0;
                int years = stock.dataRows.Count;
                foreach (var row in stock.dataRows)
                {
                    aar += row.AnnualReturn;
                }
                aar = aar / years;
                stock.AAR = aar;
            }
        }

        public static void GenerateMySpreadsheet(ref Portfolio p)
        {
            string dateStr = DateTime.Now.Ticks.ToString();
            FileStream fs = new FileStream($"{dateStr}_Stocks.csv", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            int i = 1; //counter of used rows for correct file generation
            //write stocks part
            foreach (Stock stock in p.Stocks)
            {
                string aarString = stock.AAR.ToString().Replace(',', '.');
                string stockInfo = $"{stock.Ticker};{stock.Industry};{aarString};";
                sw.WriteLine(stockInfo);
                i++;
            }

            //write history rows part
            sw.WriteLine();
            sw.WriteLine("Stock;Year;Open;Close;Return;");
            i += 2;
            foreach (Stock stock in p.Stocks)
            {
                foreach (HistoryDataRow item in stock.dataRows)
                {
                    string arFromula = $"=((D{i}/C{i})*100)-100";
                    string openPrice = item.Open.ToString().Replace(',','.');
                    string closePrice = item.Close.ToString().Replace(',', '.');
                    sw.WriteLine($"{item.Stock};{item.Year};{openPrice};{closePrice};{arFromula};");
                    i++;
                }
            }
            sw.Close();
            fs.Close();
        }

    }
}
