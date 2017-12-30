using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataClient
{
    static class Program
    {
        static void Main()
        {
            String ret;
            String dbStr;

            int first;
            int last;

            var publicWrapper = new PublicWrapper();
            var dataWrapper = new DataWrapper();
            var TAMethods = new TAMethods();
            string latestDate;

            while (true)
            {
                latestDate = dataWrapper.GetLastTimestamp();
                dataWrapper.ChartData(latestDate);

                latestDate = dataWrapper.GetLastTimestamp();
                TAMethods.CalculateEMA(latestDate);

                TAMethods.CalculateMACD(latestDate);
                Thread.Sleep(300000);         
                

            }
        }
    }
}
