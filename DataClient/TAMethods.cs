using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataClient
{
    public class TAMethods
    {
        DataWrapper dataWrapper = new DataWrapper();

        static double period24 = 96;
        static double period48 = 192;
        static double period120 = 480;
        static double period240 = 960;
        static double period480 = 1920;

        double multiplier24 = 2 / (period24 + 1);
        double multiplier48 = 2 / (period48 + 1);
        double multiplier120 = 2 / (period120 + 1);
        double multiplier240 = 2 / (period240 + 1);
        double multiplier480 = 2 / (period480 + 1);

        string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Kaare\\documents\\visual studio 2017\\Projects\\PoloapiWrapper\\DataClient\\PoloDB.mdf\";Integrated Security = True";

        public void CalculateEMA(String timestamp)
        {
            if (dataWrapper.CheckDuplicateTimestamp(timestamp, "dbo.EMAData"))
            {
                using (SqlConnection _con = new SqlConnection(connectionString))
                {
                    double SMA24 = 0;
                    double EMA24 = 0;
                    double SMA48 = 0;
                    double EMA48 = 0;
                    double SMA120 = 0;
                    double EMA120 = 0;
                    double SMA240 = 0;
                    double EMA240 = 0;
                    double SMA480 = 0;
                    double EMA480 = 0;

                    List<Double> closeList = new List<double>();

                    String _insertQuery;

                    string _selectQuery = "SELECT TOP " + period480.ToString() + " closeValue FROM dbo.ChartData ORDER BY date DESC";

                    SqlCommand _selectCommand = new SqlCommand(_selectQuery, _con);

                    _con.Open();

                    SqlDataReader reader = _selectCommand.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            closeList.Add(reader.GetDouble(0));
                        }
                    }

                    _con.Close();


                    _insertQuery = "INSERT INTO dbo.EMAData (date, EMA24, EMA48, EMA120, EMA240, EMA480) VALUES (@date, @ema24, @ema48, @ema120, @ema240, @ema480)";
                    SqlCommand _insCommand;

                    _con.Open();

                    _insCommand = new SqlCommand(_insertQuery, _con);
                    _insCommand.Parameters.AddWithValue("@date", timestamp);
                    _insCommand.Parameters.Add("@ema480", SqlDbType.Float);
                    _insCommand.Parameters.Add("@ema240", SqlDbType.Float);
                    _insCommand.Parameters.Add("@ema120", SqlDbType.Float);
                    _insCommand.Parameters.Add("@ema48", SqlDbType.Float);
                    _insCommand.Parameters.Add("@ema24", SqlDbType.Float);

                    var n = closeList.Count;

                    //EMA24
                    if (n >= 95)
                    {
                        SMA24 = closeList[95];

                        EMA24 = ((closeList[94] - SMA24) * multiplier24) + SMA24;

                        for (int i = 93; i > 0; i--)
                        {
                            EMA24 = ((closeList[i] - EMA24) * multiplier24) + EMA24;
                        }

                        _insCommand.Parameters["@ema24"].Value = EMA24;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema24"].Value = DBNull.Value;
                    }

                    //EMA48
                    if (n >= 191)
                    {
                        SMA48 = closeList[191];

                        EMA48 = ((closeList[190] - SMA48) * multiplier48) + SMA48;

                        for (int i = 189; i > 0; i--)
                        {
                            EMA48 = ((closeList[i] - EMA48) * multiplier48) + EMA48;
                        }

                        _insCommand.Parameters["@ema48"].Value = EMA48;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema48"].Value = DBNull.Value;
                    }

                    //EMA120
                    if (n >= 479)
                    {
                        SMA120 = closeList[479];

                        EMA120 = ((closeList[478] - SMA120) * multiplier120) + SMA120;

                        for (int i = 477; i > 0; i--)
                        {
                            EMA120 = ((closeList[i] - EMA120) * multiplier120) + EMA120;
                        }

                        _insCommand.Parameters["@ema120"].Value = EMA120;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema120"].Value = DBNull.Value;
                    }

                    //EMA240
                    if (n >= 959)
                    {
                        SMA240 = closeList[959];

                        EMA240 = ((closeList[958] - SMA240) * multiplier240) + SMA240;

                        for (int i = 957; i > 0; i--)
                        {
                            EMA240 = ((closeList[i] - EMA240) * multiplier240) + EMA240;
                        }

                        _insCommand.Parameters["@ema240"].Value = EMA240;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema240"].Value = DBNull.Value;
                    }              

                    //EMA480
                    if (n >= 1919)
                    {
                        SMA480 = closeList[1919];

                        EMA480 = ((closeList[1918] - SMA480) * multiplier480) + SMA480;

                        for(int i = 1917; i > 0; i--)
                        {
                            EMA480 = ((closeList[i] - EMA480) * multiplier480) + EMA480;
                        }

                        _insCommand.Parameters["@ema480"].Value = EMA480;
                    }
                    else
                    {
                        _insCommand.Parameters["@ema480"].Value = DBNull.Value;
                    }

                    int result = _insCommand.ExecuteNonQuery();

                    _con.Close();
                }
            }
        }

        public void CalculateMACD(String timestamp)
        {
            if(dataWrapper.CheckDuplicateTimestamp(timestamp, "dbo.TAData"))
            {
                using (SqlConnection _con = new SqlConnection(connectionString))
                {
                    double ema48 = 0;
                    double ema120 = 0;
                    double macd;

                    string _selectQuery = "SELECT EMA48, EMA120 FROM dbo.EMAData WHERE date = @timestamp";
                    SqlCommand _selectCommand = new SqlCommand(_selectQuery, _con);

                    string _insertQuery = "INSERT INTO dbo.TAData (date, MACD) VALUES (@date, @macd)";
                    SqlCommand _insertCommand = new SqlCommand(_insertQuery, _con);

                    _con.Open();

                    _selectCommand.Parameters.AddWithValue("@timestamp", timestamp);

                    SqlDataReader reader = _selectCommand.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ema48 = reader.GetDouble(0);
                            ema120 = reader.GetDouble(1);
                        }
                    }

                    _con.Close();
                    _con.Open();

                    macd = ema48 - ema120;

                    _insertCommand.Parameters.AddWithValue("@date", timestamp);
                    _insertCommand.Parameters.AddWithValue("@macd", macd);

                    int result = _insertCommand.ExecuteNonQuery();

                    _con.Close();
                }
            }
        }


    }
}
