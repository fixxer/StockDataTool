﻿using System;
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
        //http://financials.morningstar.com/valuate/current-valuation-list.action?&t=XNAS:AAPL
        //http://financials.morningstar.com/valuate/valuation-history.action?&t=XNAS:AAPL&type=price-earnings
        
        //http://www.nasdaq.com/screening/company-list.aspx
        //http://www.nasdaq.com/screening/companies-by-industry.aspx?industry=Technology&exchange=NYSE
        //http://bsym.bloomberg.com/sym/

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

                var usefulRows = new List<string> {dataRows[1]};

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

        public static void CreateHistoricalData(string stock, string shortPath, ref Portfolio p)
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

            for (int i = 0; i < dataRows.Count-1; i+=2)
            {
                //0 - Date, 1 - Open, 2 - High, 3 - Low, 4 - Close, 5 - Volume, 6 - Adj Close
                string[] parts1 = dataRows[i].Split(new char[] { ',' });
                string[] parts2 = dataRows[i+1].Split(new char[] { ',' });
                int year = int.Parse(parts1[0].Substring(0, 4));
                double open = double.Parse(parts2[6].Replace('.', ','));
                double close = double.Parse(parts1[6].Replace('.', ','));
                var historyDataRow = new HistoryDataRow(stock, year, open, close);
                p.HistoryDataRows.Add(historyDataRow);
            }
        }

        public static void GenerateMySpreadsheet(ref Portfolio p)
        {
            string dateStr = DateTime.Now.Ticks.ToString();
            FileStream fs = new FileStream($"{dateStr}_Stocks.csv", FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("Stock;Year;Open;Close;AAR;");
            int i = 2;
            foreach (HistoryDataRow item in p.HistoryDataRows)
            {
                string aarFromula = $"=((D{i}/C{i})*100)-100";
                sw.WriteLine($"{item.Stock};{item.Year};{item.Open};{item.Close};{aarFromula};");
                i++;
            }
            sw.Close();
            fs.Close();
        }

    }
}
