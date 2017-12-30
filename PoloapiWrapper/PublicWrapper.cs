using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PoloapiWrapper
{
    public class PublicWrapper
    {
        public String PublicQuery(String command)
        {
            HttpWebRequest request = null;

            if (command == "returnTicker" || command == "return24hVolume")
            {
                request = (HttpWebRequest)WebRequest.Create("https://poloniex.com/public?command=" + command);
            }

            else if (command == "returnOrderBook" || command == "returnChartData" || command == "returnTradeHistory")
            {
                request = (HttpWebRequest)WebRequest.Create("https://poloniex.com/public?command=" + command + "&currencyPair=USDT_BTC");
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }
    }
}
