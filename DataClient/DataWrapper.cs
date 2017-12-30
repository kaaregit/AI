using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataClient
{
    class DataWrapper
    {
        private PublicWrapper publicWrapper;
        private NumberFormatInfo numberFormat;
        

        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Kaare\\documents\\visual studio 2017\\Projects\\PoloapiWrapper\\DataClient\\PoloDB.mdf\";Integrated Security = True";

        public DataWrapper()
        {
            publicWrapper = new PublicWrapper();

            CultureInfo usCulture = new CultureInfo("en-US");

            numberFormat = usCulture.NumberFormat;
        }

        //Writes Volume Data to table VolumeData
        public void VolumeData()
        {
            String ret;
            String dbStr;

            int first;
            int last;
         
            ret = publicWrapper.PublicQuery("return24hVolume", GetLastTimestamp());

            first = ret.IndexOf("\"totalBTC\":\"", 12);

            last = ret.LastIndexOf("\",\"totalETH");

            dbStr = ret.Substring(first + 12, last - first - 12);

            using (var db = new PoloDBEntities())
            {
                /**String _selectQuery = "SELECT TOP 1 TotalVolumeBTC FROM dbo.VolumeData ORDER BY TimeStamp DESC";
                String _insertQuery = "INSERT INTO dbo.VolumeData (TimeStamp, VolumeDiff, TotalVolumeBTC) VALUES (@time, @diff, @total)";

                SqlCommand _selectCommand = new SqlCommand(_selectQuery, _con);

                SqlCommand _insertCommand = new SqlCommand(_insertQuery, _con);

                _con.Open();

                SqlDataReader _reader = _selectCommand.ExecuteReader();

                _reader.Read();

                var oldVol = _reader["TotalVolumeBTC"].ToString();
                oldVol = oldVol.Replace(",", ".");

                _reader.Close();

                var newVol = float.Parse(dbStr, numberFormat);

                float volDiff = newVol - float.Parse(oldVol, numberFormat);

                Console.WriteLine(volDiff.ToString());

                Int32 unixTimeStamp;

                _insertCommand.Parameters.AddWithValue("@time", unixTimeStamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                _insertCommand.Parameters.AddWithValue("@diff", volDiff);
                _insertCommand.Parameters.AddWithValue("@total", newVol);

                int result = _insertCommand.ExecuteNonQuery();

                _con.Close();**/

                var latest = db.VolumeDatas.First();
            }
        }

        public void ChartData(String timeStamp)
        {
            String oRet;
            String ret;
            String dbStr;
            Dictionary<String, String> dictionary = new Dictionary<String, String>();

            int first;
            int last;
            int n = 0;
            int l;

            oRet = publicWrapper.PublicQuery("returnChartData", timeStamp);

            int count = (oRet.Length - oRet.Replace("date", "").Length) / 4;

            String[] dataNames = new string[7] { "date", "high", "low", "open", "close", "volume", "weightedAverage" };

            var subStrings = new List<String>();

            if(count > 1)
            {
                for (int i = 0; i < count; i++)
                {
                    last = oRet.IndexOf("{\"date", 50);

                    l = oRet.Length;

                    if(last == -1)
                    {
                        subStrings.Add(oRet);
                    }

                    else
                    {
                        subStrings.Add(oRet.Substring(0, last));

                        oRet = oRet.Replace(subStrings.ElementAt(i), "");
                    }

                    Console.WriteLine(subStrings[i]);
                }
            }

            foreach(String s in subStrings)
            {

                first = s.IndexOf(",\"quoteVolume\":");

                last = s.IndexOf(",\"weightedAverage\":");

                ret = s.Remove(first, last - first);

                foreach (String dataName in dataNames)
                {
                    if (n == 6)
                    {
                        first = ret.IndexOf("\"" + dataName + "\":") + ("\"" + dataName + "\":").Length;

                        last = ret.Length - 2;

                        dbStr = ret.Substring(first, last - first);

                        dictionary.Add(dataName, dbStr);

                        Console.WriteLine(dbStr);
                    }

                    else
                    {
                        first = ret.IndexOf("\"" + dataName + "\":") + ("\"" + dataName + "\":").Length;

                        last = ret.IndexOf(",\"" + dataNames[n + 1] + "\"");

                        dbStr = ret.Substring(first, last - first);

                        dictionary.Add(dataName, dbStr);

                        n++;

                        Console.WriteLine(dbStr);
                    }
                }

                ChartAdd(dictionary);

                n = 0;
                dictionary.Clear();
            }            
        }

        public void ChartAdd(Dictionary<String, String> d)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                if (CheckDuplicateTimestamp(d["date"], "dbo.ChartData"))
                {
                    String _insertQuery = "INSERT INTO dbo.ChartData (date, high, low, openValue, closeValue, volume, weightedAverage)" +
                    " VALUES (@date, @high, @low, @openValue, @closeValue, @volume, @weightedAverage)";

                    SqlCommand _insertCommand = new SqlCommand(_insertQuery, _con);

                    _con.Open();

                    var high = float.Parse(d["high"], numberFormat);
                    var low = float.Parse(d["low"], numberFormat);
                    var open = float.Parse(d["open"], numberFormat);
                    var close = float.Parse(d["close"], numberFormat);
                    var volume = float.Parse(d["volume"], numberFormat);
                    var weightedAverage = float.Parse(d["weightedAverage"], numberFormat);

                    _insertCommand.Parameters.AddWithValue("@date", d["date"]);
                    _insertCommand.Parameters.AddWithValue("@high", high);
                    _insertCommand.Parameters.AddWithValue("@low", low);
                    _insertCommand.Parameters.AddWithValue("@openValue", open);
                    _insertCommand.Parameters.AddWithValue("@closeValue", close);
                    _insertCommand.Parameters.AddWithValue("@volume", volume);
                    _insertCommand.Parameters.AddWithValue("@weightedAverage", weightedAverage);

                    int result = _insertCommand.ExecuteNonQuery();

                    _con.Close();

                    TAMethods ta = new TAMethods();

                    ta.CalculateMACD(d["date"]);
                }
            }
        }

        public bool CheckDuplicateTimestamp(String timestamp, String table)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                String _query = "SELECT COUNT(*) FROM " + table + " WHERE date = @date";

                SqlCommand _comm = new SqlCommand(_query, _con);

                _con.Open();

                _comm.Parameters.AddWithValue("@date", timestamp);

                int Count = Convert.ToInt32(_comm.ExecuteScalar());

                _con.Close();

                if (Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public String GetLastTimestamp()
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                String ret = "";

                String _query = "SELECT TOP 1 date FROM dbo.ChartData ORDER BY date DESC";

                SqlDataReader reader;

                SqlCommand _comm = new SqlCommand(_query, _con);

                _con.Open();

                reader = _comm.ExecuteReader();

                while (reader.Read())
                {
                    ret = reader.GetString(0);
                }

                _con.Close();

                return ret;
            }
        }
    }
}
