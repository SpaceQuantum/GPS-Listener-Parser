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
            SqlCommand sp = db.InitQuery(@" INSERT INTO GPS_Real (ModemId, [ServerTimestamp],    
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
            sp.Parameters.AddWithValue("@ModemId", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@ServerTimestamp", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Long", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Lat", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Altitude", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Direction", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Satellites", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Speed", gpsPos.Direction.ToString());
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
            SqlCommand sp = db.InitQuery(@" INSERT INTO GPS_Real (ModemId, [ServerTimestamp],    
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
            sp.Parameters.AddWithValue("@ModemId", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@ServerTimestamp", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Long", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Lat", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Altitude", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Direction", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Satellites", gpsPos.Direction.ToString());
            sp.Parameters.AddWithValue("@Speed", gpsPos.Direction.ToString());
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
