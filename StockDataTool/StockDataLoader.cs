﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace StockDataTool
{
    class StockDataLoader
    {

        public static List<string> GetAllTickers()
        {
            /*
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Basic+Industries&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Capital+Goods&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Consumer+Durable&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Consumer+Non-Durables&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Consumer+Services&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Energy&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Finance&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Health+Care&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Miscellaneous&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Public+Utilities&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Technology&exchange=NASDAQ&render=download
                http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Transportation&exchange=NASDAQ&render=download
            */

            //http://bsym.bloomberg.com/sym/ - not used

            List<string> demTickers = new List<string>();

            foreach (string industry in Portfolio.Industries)
            {
                string nasdaqPath = $"http://nasdaq.com/screening/companies-by-industry.aspx?industry={industry}&exchange=NASDAQ&render=download";
                string localPath = $"NASDAQ_{industry}.csv";
                string tickersPath = $"NASDAQ_{industry}_tickers.txt";
                if (!File.Exists(localPath))
                {
                    using (var receiveStream = WebRequest.Create(nasdaqPath).GetResponse().GetResponseStream())
                    {
                        using (FileStream file = new FileStream(localPath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            receiveStream.CopyTo(file);
                        }
                    }
                }
                var localFile = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                var sr = new StreamReader(localFile);

                var tickerFile = new FileStream(tickersPath, FileMode.Create, FileAccess.Write);
                var sw = new StreamWriter(tickerFile);

                sr.ReadLine();//skipping one line with headers
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var ticker = line.Split(new char[] { ',' })[0].Replace("\"", "");
                    sw.WriteLine(ticker);
                    demTickers.Add(ticker);
                }
                sr.Close();
                sw.Close();
                localFile.Close();
                tickerFile.Close();
            }

            return demTickers;
        }

        public static void GetHistoricalMorningstarData(Stock s)
        {
            string exchange = s.Exchange == Exchange.NASDAQ ? "XNAS" : "XNYS";
            string path = $"http://financials.morningstar.com/valuate/valuation-history.action?&t={exchange}:{s.Ticker}&type=price-earnings";

            //int ok = 0;
            //string result = "";
            //while (ok < 5)
            //{
            //    try
            //    {
            //        var request = (HttpWebRequest)WebRequest.Create(path);
            //        var response = (HttpWebResponse)request.GetResponse();
            //        Stream receiveStream = response.GetResponseStream();
            //        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            //        result = readStream.ReadToEnd();
            //        receiveStream.Close();
            //        response.Close();
            //        readStream.Close();
            //        ok = 5;
            //    }
            //    catch (WebException)
            //    {
            //        Thread.Sleep(50);
            //        ok++;
            //    }
            //}

            string result = Helpers.HandleWebRequest(path);

            result =
                        result.Trim()
                            .Replace("S&P", "SnP")
                            .Replace("&nbsp;", " ")
                            .Replace("&ndash;", "-")
                            .Replace("&mdash;", "-");
            result = "<myRoot>" + result + "</myRoot>";

            XElement root = new XElement("dummy");
            try
            {
                root = XElement.Parse(result);
                var tbody = root.Element("div").Element("table").Element("tbody");
                var trs = tbody.Elements().ToArray();
                //PEs
                var stockPEs = trs[1].Elements("td").ToList();
                stockPEs.Reverse();
                for (int i = 1; i < stockPEs.Count; i++) //last one is the current one - we need to skip it
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
                for (int i = 1; i < stockPBs.Count; i++) //last one is the current one - we need to skip it
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
            catch
            {
                foreach (HistoryDataRow row in s.dataRows)
                {
                    row.PE = 0;
                    row.PB = 0;
                }
            }

        }

        public static void GetBasicMorningstarData(Stock s)
        {
            //http://financials.morningstar.com/cmpind/company-profile/component.action?component=BasicData&t=XNAS:FB

            string exchange = s.Exchange == Exchange.NASDAQ ? "XNAS" : "XNYS";
            string path = $"http://financials.morningstar.com/cmpind/company-profile/component.action?component=BasicData&t={exchange}:{s.Ticker}";

            var request = (HttpWebRequest)WebRequest.Create(path);
            var response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string result = readStream.ReadToEnd();
            receiveStream.Close();
            response.Close();
            readStream.Close();
            result = result.Trim().Replace("S&P", "SnP").Replace("&nbsp;", " ").Replace("&ndash;", "-").Replace("&mdash;", "-");
            result = result.Replace("&", "n");

            XElement root = new XElement("dummy");
            try
            {
                root = XElement.Parse(result);
                s.Sector = root.Element("tbody").Elements("tr").ElementAt(5).Elements("td").ElementAt(2).Value;
                s.Industry = root.Element("tbody").Elements("tr").ElementAt(5).Elements("td").ElementAt(4).Value;
                s.Style = root.Element("tbody").Elements("tr").ElementAt(8).Elements("td").ElementAt(0).Value.Trim();
            }
            catch
            {
                s.Sector = "";
                s.Industry = "";
                s.Style = "";
            }
        }

        public static void GetCurrentMorningstarData(Stock s)
        {
            //http://financials.morningstar.com/valuation/price-ratio.html?t=AAPL
            //http://financials.morningstar.com/valuate/current-valuation-list.action?&t=XNAS:AAPL


            string exchange = s.Exchange == Exchange.NASDAQ ? "XNAS" : "XNYS";
            string path =
                $"http://financials.morningstar.com/valuate/current-valuation-list.action?&t={exchange}:{s.Ticker}";

            int ok = 0;
            string result = "";
            while (ok < 5)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(path);
                    var response = (HttpWebResponse)request.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    result = readStream.ReadToEnd();
                    receiveStream.Close();
                    response.Close();
                    readStream.Close();
                    ok = 5;
                }
                catch (WebException)
                {
                    Thread.Sleep(50);
                    ok++;
                }
            }

            result = result.Trim().Replace("S&P", "SnP").Replace("&nbsp;", " ").Replace("&ndash;", "-").Replace("&mdash;", "-");
            result = result.Replace("&", "n");

            XElement root = new XElement("dummy");

            try
            {
                root = XElement.Parse(result);
                var tbody = root.Element("tbody");
                //PE
                var tr = from el in tbody.Elements("tr") where (string)el.Element("th") == @"Price/Earnings" select el;
                var tds = tr.Elements("td");
                var stockPE = tds.ElementAt(0).Value;
                var industryPE = tds.ElementAt(1).Value;

                double myPE = 0;
                if (double.TryParse(stockPE.Replace('.', ','), out myPE))
                    s.PE = myPE;
                else s.PE = 0;

                double indPE = 0;
                if (double.TryParse(industryPE.Replace('.', ','), out indPE))
                    s.industryPE = indPE;
                else s.industryPE = 0;

                //PB
                tr = from el in tbody.Elements("tr") where (string)el.Element("th") == @"Price/Book" select el;
                tds = tr.Elements("td");
                var stockPB = tds.ElementAt(0).Value;
                var industryPB = tds.ElementAt(1).Value;

                double pb = 0;
                if (double.TryParse(stockPB.Replace('.', ','), out pb))
                    s.PB = pb;
                else s.PB = 0;

                double indPb = 0;
                if (double.TryParse(industryPB.Replace('.', ','), out indPb))
                    s.industryPB = indPb;
                else s.industryPB = 0;

                //Divident Yield
                tr = from el in tbody.Elements("tr") where (string)el.Element("th") == @"Dividend Yield %" select el;
                tds = tr.Elements("td");
                var dividents = tds.ElementAt(0).Value;
                //var industryDividents = tds.ElementAt(1).Value; // I don't use it

                double myDivident = 0;
                if (double.TryParse(dividents.Replace('.', ','), out myDivident))
                {
                }
                s.DividentYield = myDivident;
                //else s.DividentYield = 0;
            }
            catch
            {
                s.PE = 0;
                s.industryPE = 0;
                s.PB = 0;
                s.industryPB = 0;
                s.DividentYield = 0;
            }
        }

        public static string DownloadPricesCsv(string ticker, int startYear, int endYear)
        {
            string fileName = $"{ticker}_{startYear}_{endYear}_long.csv";
            if (!File.Exists(fileName))
            {
                try
                {
                    string path = @"http://real-chart.finance.yahoo.com/table.csv";
                    string param = "?s=" + ticker + "&a=00&b=1&c=" + startYear + "&d=11&e=31&f=" + endYear +
                                   "&g=d&ignore=.csv";
                    path += param;

                    var request = (HttpWebRequest)WebRequest.Create(path);
                    var response = (HttpWebResponse)request.GetResponse();
                    var receiveStream = response.GetResponseStream();
                    var readStream = new StreamReader(receiveStream, Encoding.UTF8);

                    FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
                    receiveStream.CopyTo(file);
                    receiveStream.Close();
                    response.Close();
                    readStream.Close();
                    file.Close();
                }
                catch
                {
                    return "error";
                }
            }
            return fileName;
        }

        public static string ReformatPricesCsv(string fileName)
        {
            if (!File.Exists(fileName.Replace("long", "short")))
            {
                try
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
                        if ((dataRows[i].Substring(0, 4) != dataRows[i - 1].Substring(0, 4)) ||
                            (dataRows[i].Substring(0, 4) != dataRows[i + 1].Substring(0, 4)))
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
                }
                catch
                {
                    // ignored
                }
            }
            return fileName.Replace("long", "short");
        }

        public static void CreateHistoricalData(Stock stock, string shortPath, ref Portfolio p)
        {
            try
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
            catch
            {
                //just skip this stock
            }

        }

        [Obsolete("EnrichStocksWithAAR is deprecated, please use EnrichStockWithAAR instead.")]
        public static void EnrichStocksWithAAR(Portfolio p)
        {
            foreach (Stock stock in p.Stocks)
            {
                double aar = 0;
                int years = stock.dataRows.Count;
                if (years > 0)
                {
                    aar += stock.dataRows.Sum(row => row.AnnualReturn);
                    aar = aar / years;
                    stock.AAR = aar;
                }
                else stock.AAR = 0;
            }
        }

        public static void EnrichStockWithAAR(Stock stock)
        {
            double aar = 0;
            int years = stock.dataRows.Count;
            if (years > 0)
            {
                aar += stock.dataRows.Sum(row => row.AnnualReturn);
                aar = aar / years;
                stock.AAR = aar;
            }
            else stock.AAR = 0;
        }

        [Obsolete("EnrichStocksWithSTD is deprecated, please use EnrichStockWithSTD instead.")]
        public static void EnrichStocksWithSTD(Portfolio p)
        {
            foreach (Stock stock in p.Stocks)
            {
                var returns = new List<double>();
                if (stock.dataRows.Count > 0)
                {
                    returns.AddRange(stock.dataRows.Select(row => row.AnnualReturn));
                    double average = returns.Average();
                    double sumOfSquaresOfDifferences = returns.Sum(val => (val - average) * (val - average));
                    stock.STD = Math.Sqrt(sumOfSquaresOfDifferences / returns.Count - 1);
                }
                else stock.STD = 0;
            }
        }

        public static void EnrichStockWithSTD(Stock stock)
        {
            var returns = new List<double>();
            if (stock.dataRows.Count > 0)
            {
                returns.AddRange(stock.dataRows.Select(row => row.AnnualReturn));
                double average = returns.Average();
                double sumOfSquaresOfDifferences = returns.Sum(val => (val - average) * (val - average));
                stock.STD = Math.Sqrt(sumOfSquaresOfDifferences / returns.Count - 1);
            }
            else stock.STD = 0;
        }

        public static void EnrichStocksWithAvgAndMaxPEPBs(Portfolio p)
        {
            foreach (Stock stock in p.Stocks.Where(stock => stock.dataRows.Count > 0))
            {
                stock.MaxPE = stock.dataRows.Max(i => i.PE);
                stock.MaxPB = stock.dataRows.Max(i => i.PB);

                stock.AvgPE = stock.dataRows.Average(i => i.PE);
                stock.AvgPB = stock.dataRows.Average(i => i.PB);
            }
        }

        public static void GenerateMySpreadsheet(ref Portfolio p)
        {
            var dateStr = DateTime.Now.Ticks.ToString();
            var fs = new FileStream($"{dateStr}_Stocks.csv", FileMode.CreateNew, FileAccess.Write);
            var sw = new StreamWriter(fs);

            int i = 2; //counter of used rows for correct file generation

            //write stocks part
            sw.WriteLine("Stock;Sector;Industry;Stock Style;AAR, %;STD;Retrun-Risk ratio;P/E as of 2016;Avg. P/E (10 years);Max P/E (10 years);Avg. P/B (10 years);Max P/B (10 years);Industry avg. P/E;P/B as of 2016;Industry avg. P/B;Divident Yield;;;P/E < avg;P/E < industry avg;P/B < avg;P/B < industry avg;Dividends;R/R > avg.R/R;final decision");
            foreach (Stock stock in p.Stocks)
            {
                var aarString = stock.AAR.ToString().Replace(',', '.');
                var stdString = stock.STD.ToString().Replace(',', '.');
                var peString = stock.PE.ToString().Replace(',', '.');
                var indPeString = stock.industryPE.ToString().Replace(',', '.');
                var pbString = stock.PB.ToString().Replace(',', '.');
                var indPbString = stock.industryPB.ToString().Replace(',', '.');
                var avgPeString = stock.AvgPE.ToString().Replace(',', '.');
                var avgPbString = stock.AvgPB.ToString().Replace(',', '.');
                var maxPeString = stock.MaxPE.ToString().Replace(',', '.');
                var maxPbString = stock.MaxPB.ToString().Replace(',', '.');
                var dividentString = stock.DividentYield.ToString().Replace(',', '.');

                string stockInfo = $"{stock.Ticker};{stock.Sector};{stock.Industry};{stock.Style};{aarString};{stdString};=E{i}/F{i};{peString};{avgPeString};{maxPeString};{avgPbString};{maxPbString};{indPeString};{pbString};{indPbString};{dividentString};";
                stockInfo += $";;=H{i}<I{i};=H{i}<M{i};=N{i}<K{i};=N{i}<O{i};=P{i}>0;R/R > avg.R/R;";
                sw.WriteLine(stockInfo);
                i++;
            }

            //write history rows part
            sw.WriteLine();

            i++;
            foreach (Stock stock in p.Stocks)
            {
                sw.WriteLine("Stock;Year;Open;Close;Annual Return, %;P/E;P/B;");
                i++;
                foreach (HistoryDataRow item in stock.dataRows)
                {
                    string arFromula = $"=((D{i}/C{i})*100)-100";
                    string openPrice = item.Open.ToString().Replace(',', '.');
                    string closePrice = item.Close.ToString().Replace(',', '.');
                    string peString = item.PE != 0 ? item.PE.ToString().Replace(',', '.') : "-";
                    string pbString = item.PB != 0 ? item.PB.ToString().Replace(',', '.') : "-";
                    sw.WriteLine($"{item.Stock};{item.Year};{openPrice};{closePrice};{arFromula};{peString};{pbString};");
                    i++;
                }
                sw.WriteLine();
                i++;
            }
            sw.Close();
            fs.Close();
        }
    }
}
