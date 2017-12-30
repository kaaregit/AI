using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoloBot
{
    public class TAMethods
    {
        double period24 = 288;
        double period48 = 576;
        double period120 = 1440;
        double period240 = 2880;
        double period480 = 5760;

        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Kaare\\documents\\visual studio 2017\\Projects\\PoloapiWrapper\\DataClient\\PoloDB.mdf\";Integrated Security = True";

        public void ReloadEMA()
        {
            double multiplier24 = 2 / (period24 + 1);
            double multiplier48 = 2 / (period48 + 1);
            double multiplier120 = 2 / (period120 + 1);
            double multiplier240 = 2 / (period240 + 1);
            double multiplier480 = 2 / (period480 + 1);

            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                List<String> dateList = new List<string>();
                List<Double> closeList = new List<double>();

                String _insertQuery;

                string _selectQuery = "SELECT TOP " + period480.ToString() + " date, closeValue FROM dbo.ChartData ORDER BY date DESC";

                SqlCommand _selectCommand = new SqlCommand(_selectQuery, _con);

                _con.Open();

                SqlDataReader reader = _selectCommand.ExecuteReader();

                if(reader.HasRows)
                {
                    while (reader.Read())
                    {
                        dateList.Add(reader.GetString(0));
                        closeList.Add(reader.GetDouble(1));
                    }
                }

                _con.Close();

                double SMA24 = closeList[287];

                double EMA24 = ((closeList[286] - SMA24) * multiplier24) + SMA24;

                double SMA48 = closeList[575];

                double EMA48 = ((closeList[574] - SMA48) * multiplier48) + SMA48;

                double SMA120 = closeList[1439];

                double EMA120 = ((closeList[1438] - SMA120) * multiplier120) + SMA120;

                int n = 1439;

                _insertQuery = "INSERT INTO dbo.EMAData (date, EMA24, EMA48, EMA120) VALUES (@date, @ema24, @ema48, @ema120)";
                SqlCommand _insCommand;

                _con.Open();

                while (n > 0)
                {
                    _insCommand = new SqlCommand(_insertQuery, _con);
                    _insCommand.Parameters.AddWithValue("@date", dateList[n]);
                    _insCommand.Parameters.Add("@ema120", SqlDbType.Float);
                    _insCommand.Parameters.Add("@ema48", SqlDbType.Float);
                    _insCommand.Parameters.Add("@ema24", SqlDbType.Float);

                    ////EMA 480
                    //Console.WriteLine("");
                    //EMA24 = ((closeList[i] - EMA24) * multiplier24) + EMA24; ;

                    //Console.WriteLine("Close:" + closeList[i].ToString());
                    //Console.WriteLine("EMA:" + EMA24.ToString());

                    //_insertQuery = "INSERT INTO dbo.EMAData (TimeStamp, VolumeDiff, TotalVolumeBTC) VALUES (@time, @diff, @total)"

                    ////EMA 240
                    //if (n < 2877)
                    //{
                    //    Console.WriteLine("");
                    //    EMA24 = ((closeList[i] - EMA24) * multiplier24) + EMA24; ;

                    //    Console.WriteLine("Close:" + closeList[i].ToString());
                    //    Console.WriteLine("EMA:" + EMA24.ToString());

                    //    _insertQuery = "INSERT INTO dbo.EMAData (TimeStamp, VolumeDiff, TotalVolumeBTC) VALUES (@time, @diff, @total)"
                    //}

                    //EMA 120
                    if (n < 1437)
                    {
                        EMA120 = ((closeList[n] - EMA120) * multiplier120) + EMA120;
                        _insCommand.Parameters["@ema120"].Value = EMA120;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema120"].Value = DBNull.Value;
                    }


                    //EMA 48
                    if (n < 573)
                    {
                        EMA48 = ((closeList[n] - EMA48) * multiplier48) + EMA48;
                        _insCommand.Parameters["@ema48"].Value = EMA48;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema48"].Value = DBNull.Value;
                    }

                    //EMA 24
                    if (n < 285)
                    {
                        EMA24 = ((closeList[n] - EMA24) * multiplier24) + EMA24;
                        _insCommand.Parameters["@ema24"].Value = EMA24;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema24"].Value = DBNull.Value;
                    }

                    int result = _insCommand.ExecuteNonQuery();

                    n--;
                }

                _con.Close();
            }
        }

        public void CalcEMAOneTime(String timestamp)
        {

        }

        public bool CheckDuplicateTimestamp(String timestamp)
        {
            using (SqlConnection _con = new SqlConnection(connectionString))
            {
                String _query = "SELECT COUNT(*) FROM dbo.ChartData WHERE date = @date";

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
