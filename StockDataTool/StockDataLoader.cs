using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
//using System.Web;
using System.Windows;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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
                string tickersPath = $"NASDAQ_{industry}_tickers.txt";
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

                    FileStream localFile = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(localFile);
                    FileStream tickerFile = new FileStream(tickersPath, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(tickerFile);
                    sr.ReadLine();//skipping one line with headers
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        var ticker = line.Split(new char[] { ',' })[0].Replace("\"", "");
                        sw.WriteLine(ticker);
                    }
                    sr.Close();
                    sw.Close();
                    localFile.Close();
                    tickerFile.Close();
                }
            }
        }


        public static void GetHistoricalMorningstarData(Stock s)
        {
            string exchange = s.Exchange == Exchange.NASDAQ ? "XNAS" : "XNYS";
            string path = $"http://financials.morningstar.com/valuate/valuation-history.action?&t={exchange}:{s.Ticker}&type=price-earnings";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string result = readStream.ReadToEnd();
            receiveStream.Close();
            response.Close();
            readStream.Close();
            result = result.Trim().Replace("S&P", "SnP").Replace("&nbsp;", " ").Replace("&ndash;", "-").Replace("&mdash;", "-");
            result = "<myRoot>" + result + "</myRoot>";
            XElement root = XElement.Parse(result);
            var tbody = root.Element("div").Element("table").Element("tbody");
            var trs = tbody.Elements().ToArray();
            //PEs
            var stockPEs = trs[1].Elements("td").ToList();
            stockPEs.Reverse();
            for (int i = 1; i < stockPEs.Count; i++)//last one is the current one - we need to skip it
            {
                if (i > s.dataRows.Count) break;
                double nextPE = 0;
                if (double.TryParse((stockPEs[i].Value).Replace('.', ','), out nextPE))
                {
                    s.dataRows[i - 1].PE = nextPE;
                }
                else
                    s.dataRows[i - 1].PE = 0;

            }
            //PBs
            var stockPBs = trs[4].Elements("td").ToList();
            stockPBs.Reverse();
            for (int i = 1; i < stockPBs.Count; i++)//last one is the current one - we need to skip it
            {
                if (i > s.dataRows.Count) break;
                double nextPB = 0;
                if (double.TryParse((stockPBs[i].Value).Replace('.', ','), out nextPB))
                {
                    s.dataRows[i - 1].PB = nextPB;
                }
                else
                    s.dataRows[i - 1].PB = 0;
            }

            //var avgPEs = trs[2];
            //var avgPBs = trs[5];

        }

        public static void GetCurrentMorningstarData(Stock s)
        {
            //http://financials.morningstar.com/valuation/price-ratio.html?t=AAPL
            //http://financials.morningstar.com/valuate/current-valuation-list.action?&t=XNAS:AAPL


            string exchange = s.Exchange == Exchange.NASDAQ ? "XNAS" : "XNYS";
            string path = $"http://financials.morningstar.com/valuate/current-valuation-list.action?&t={exchange}:{s.Ticker}";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string result = readStream.ReadToEnd();
            receiveStream.Close();
            response.Close();
            readStream.Close();
            result = result.Trim().Replace("S&P", "SnP").Replace("&nbsp;", " ").Replace("&ndash;", "-").Replace("&mdash;", "-");

            XElement root = XElement.Parse(result);
            var tbody = root.Element("tbody");
            //PE
            var tr = from el in tbody.Elements("tr") where (string)el.Element("th") == @"Price/Earnings" select el;
            var tds = tr.Elements("td");
            var stockPE = tds.ElementAt(0).Value;
            var industryPE = tds.ElementAt(1).Value;

            s.PE = double.Parse(stockPE.Replace('.', ','));
            s.industryPE = double.Parse(industryPE.Replace('.', ','));

            //PB
            tr = from el in tbody.Elements("tr") where (string)el.Element("th") == @"Price/Book" select el;
            tds = tr.Elements("td");
            var stockPB = tds.ElementAt(0).Value;
            var industryPB = tds.ElementAt(1).Value;

            s.PB = double.Parse(stockPB.Replace('.', ','));
            s.industryPB = double.Parse(industryPB.Replace('.', ','));

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

        public static void EnrichStocksWithSTD(Portfolio p)
        {
            foreach (Stock stock in p.Stocks)
            {
                var returns = new List<double>();
                foreach (var row in stock.dataRows)
                {
                    returns.Add(row.AnnualReturn);
                }
                double average = returns.Average();
                double sumOfSquaresOfDifferences = returns.Sum(val => (val - average) * (val - average));
                stock.STD = Math.Sqrt(sumOfSquaresOfDifferences / returns.Count - 1);
            }
        }

        public static void EnrichStocksWithAvgPEPBs(Portfolio p)
        {
            foreach (Stock stock in p.Stocks)
            {
                var pes = new List<double>();
                var pbs = new List<double>();
                foreach (var row in stock.dataRows)
                {
                    pes.Add(row.PE);
                    pbs.Add(row.PB);
                }
            }
        }

        public static void GenerateMySpreadsheet(ref Portfolio p)
        {
            string dateStr = DateTime.Now.Ticks.ToString();
            FileStream fs = new FileStream($"{dateStr}_Stocks.csv", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            int i = 2; //counter of used rows for correct file generation
            //write stocks part
            sw.WriteLine("Stock;Sector;Industry;Stock Type;Stock Style;AAR, %;STD;Retrun-Risk ratio;P/E as of 2016;Avg. P/E (10 years);max P/E (10 years);Industry avg. P/E;P/B as of 2016;Industry avg. P/B;Divident Yield;");
            foreach (Stock stock in p.Stocks)
            {
                string aarString = stock.AAR.ToString().Replace(',', '.');
                string stdString = stock.STD.ToString().Replace(',', '.');
                string peString = stock.PE.ToString().Replace(',', '.');
                string indPeString = stock.industryPE.ToString().Replace(',', '.');
                string pbString = stock.PB.ToString().Replace(',', '.');
                string indPbString = stock.industryPB.ToString().Replace(',', '.');
                string stockInfo = $"{stock.Ticker};sector;industry;type;style;{aarString};{stdString};=F{i}/G{i};{peString};avgPE;maxPE;{indPeString};{pbString};{indPbString};yield;";
                sw.WriteLine(stockInfo);
                i++;
            }

            //write history rows part
            sw.WriteLine();
            sw.WriteLine("Stock;Year;Open;Close;Return;P/E;P/B;");
            i += 2;
            foreach (Stock stock in p.Stocks)
            {
                foreach (HistoryDataRow item in stock.dataRows)
                {
                    string arFromula = $"=((D{i}/C{i})*100)-100";
                    string openPrice = item.Open.ToString().Replace(',', '.');
                    string closePrice = item.Close.ToString().Replace(',', '.');
                    string peString = item.PE.ToString().Replace(',', '.');
                    string pbString = item.PB.ToString().Replace(',', '.');
                    sw.WriteLine($"{item.Stock};{item.Year};{openPrice};{closePrice};{arFromula};{peString};{pbString};");
                    i++;
                }
            }
            sw.Close();
            fs.Close();
        }

    }
}
