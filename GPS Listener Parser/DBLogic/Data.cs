using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Data.SqlClient;

namespace GPSParser.DBLogic
{
    public class Data
    {
        public void SaveGPSPositionFMXXXX(GPSdata gpsPos)
        {
            DBUtils db = new DBUtils();
            SqlCommand sp = db.InitSP("SaveGPSpointFMXXXX");
            sp.Parameters.AddWithValue("@priority", gpsPos.Priority.Value);
   
            sp.Parameters.AddWithValue("@device_id", gpsPos.IMEI);
            sp.Parameters.AddWithValue("@latitude", gpsPos.Lat.Value);
            sp.Parameters.AddWithValue("@longitude", gpsPos.Long.Value);
            sp.Parameters.AddWithValue("@altitude", gpsPos.Altitude);
            sp.Parameters.AddWithValue("@speed", gpsPos.Speed);
            sp.Parameters.AddWithValue("@direction", gpsPos.Direction);
            sp.Parameters.AddWithValue("@satellites", gpsPos.Satellites);
            sp.Parameters.AddWithValue("@rtc_time", gpsPos.Timestamp.Value);
            try
            {
                db.ExecSP(sp);
            }
            catch (Exception ex)
            {
               
            }
            finally
            {
                db.FreeSP(sp);
            }
        }
              
        public void SaveGPSPositionGH3000(GPSdata gpsPos)
        {
            DBUtils db = new DBUtils();
            
            SqlCommand sp = db.InitSP("SaveGPSpointFMXXXX");
            sp.Parameters.AddWithValue("@priority", gpsPos.Priority.Value);

            sp.Parameters.AddWithValue("@device_id", gpsPos.IMEI);
            sp.Parameters.AddWithValue("@latitude", gpsPos.Lat.Value);
            sp.Parameters.AddWithValue("@longitude", gpsPos.Long.Value);
            sp.Parameters.AddWithValue("@altitude", gpsPos.Altitude);
            sp.Parameters.AddWithValue("@speed", gpsPos.Speed);
            sp.Parameters.AddWithValue("@direction", gpsPos.Direction);
            sp.Parameters.AddWithValue("@satellites", gpsPos.Satellites);
            sp.Parameters.AddWithValue("@rtc_time", gpsPos.Timestamp.Value);
            try
            {
                db.ExecSP(sp);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                db.FreeSP(sp);
            }
        }
       
    }
}
