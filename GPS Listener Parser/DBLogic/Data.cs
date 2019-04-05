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
            SqlCommand sp = db.InitQuery(@" INSERT INTO GPS_Real (
DeviceId, 
[ServerTimestamp],    
    Long,
    Lat, 
    Altitude, 
    Direction, 
    Satellites, 
    Speed)
    VALUES (@ModemId, @ServerTimestamp,    
    @Long,
   @Lat, 
   @Altitude, 
   @Direction, 
   @Satellites, 
   @Speed)");
            sp.Parameters.AddWithValue("@DeviceId", gpsPos.IMEI);
            sp.Parameters.AddWithValue("@ServerTimestamp", DateTime.Now);
            sp.Parameters.AddWithValue("@DeviceTimeStamp", gpsPos.Timestamp);
            sp.Parameters.AddWithValue("@Long", gpsPos.Long);
            sp.Parameters.AddWithValue("@Lat", gpsPos.Lat);
            sp.Parameters.AddWithValue("@Altitude", gpsPos.Altitude);
            sp.Parameters.AddWithValue("@Direction", gpsPos.Direction);
            sp.Parameters.AddWithValue("@Satellites", gpsPos.Satellites);
            sp.Parameters.AddWithValue("@Speed", gpsPos.Speed);
            db.ExecQuery(sp);
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
            SqlCommand sp = db.InitQuery(@" INSERT INTO GPS_Real (
DeviceId, 
[ServerTimestamp],    
    Long,
    Lat, 
    Altitude, 
    Direction, 
    Satellites, 
    Speed)
    VALUES (@ModemId, @ServerTimestamp,    
    @Long,
   @Lat, 
   @Altitude, 
   @Direction, 
   @Satellites, 
   @Speed)");
            sp.Parameters.AddWithValue("@DeviceId", gpsPos.IMEI);
            sp.Parameters.AddWithValue("@ServerTimestamp", DateTime.Now);
            sp.Parameters.AddWithValue("@DeviceTimeStamp", gpsPos.Timestamp);
            sp.Parameters.AddWithValue("@Long", gpsPos.Long);
            sp.Parameters.AddWithValue("@Lat", gpsPos.Lat);
            sp.Parameters.AddWithValue("@Altitude", gpsPos.Altitude);
            sp.Parameters.AddWithValue("@Direction", gpsPos.Direction);
            sp.Parameters.AddWithValue("@Satellites", gpsPos.Satellites);
            sp.Parameters.AddWithValue("@Speed", gpsPos.Speed);
            db.ExecQuery(sp);
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
