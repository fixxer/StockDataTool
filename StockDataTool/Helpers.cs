
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace StockDataTool
{
    public static class Helpers
    {
        public static string HandleWebRequest(string path)
        {
            int ok = 0;
            string result = "";
            while (ok < 5)
            {
                try
                {
                    var request = (HttpWebRequest) WebRequest.Create(path);
                    var response = (HttpWebResponse) request.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    result = readStream.ReadToEnd();
                    receiveStream.Close();
                    response.Close();
                    readStream.Close();
                    ok = 5;
                    return result;
                }
                catch (WebException)
                {
                    Thread.Sleep(50);
                    ok++;
                }
            }
            return result;
        }
    }
}
