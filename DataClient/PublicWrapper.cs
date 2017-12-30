using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataClient
{
    class PublicWrapper
    {
        public String PublicQuery(String command, String timeStamp)
        {
            HttpWebRequest request = null;

            if (command == "returnTicker" || command == "return24hVolume")
            {
                request = (HttpWebRequest)WebRequest.Create("https://poloniex.com/public?command=" + command);
            }

            else if (command == "returnOrderBook" || command == "returnTradeHistory")
            {
                request = (HttpWebRequest)WebRequest.Create("https://poloniex.com/public?command=" + command + "&currencyPair=USDT_BTC");
            }

            else if (command == "returnChartData")
            {
                var unixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - 900;
                request = (HttpWebRequest)WebRequest.Create("https://poloniex.com/public?command=" + command + 
                    "&currencyPair=USDT_BTC&start=" + timeStamp + "&end=9999999999&period=900");
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }
    }
}
