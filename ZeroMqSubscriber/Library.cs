using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMqSubscriber.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Configuration;

namespace ZeroMqSubscriber
{
    class Library
    {
        private const int xLeft = 3;
        private const int xRight = 25;
        private const int yLeft = 10;
        private const int yRight = 3;
        private const int MINCheckConsecutiveShownDiffInSeconds = 5;
        private const int MAXCheckConsecutiveShownDiffInSeconds = 15;
        private const int constSkipNotificationForMinutes = 30;
        SqlConnection Con = null;

        /// <summary>
        /// Default Constructor 
        /// </summary>
        public Library()
        {
            Con = new SqlConnection(Properties.Settings.Default.ConnectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <returns></returns>
        public bool InsertData(LocationData objLocationData)
        {
            try
            {
                SqlCommand insertCommand = new SqlCommand("INSERT INTO dbo.Notifications(mac,LastVistDateTime,NotifiedDateTime) VALUES (@mac,@LastVistDateTime,@NotifiedDateTime)", Con);
                insertCommand.Parameters.Add(new SqlParameter("@mac", objLocationData.mac));
                insertCommand.Parameters.Add(new SqlParameter("@LastVistDateTime", objLocationData.LastSeenDatetime));
                insertCommand.Parameters.Add(new SqlParameter("@NotifiedDateTime", objLocationData.LastSeenDatetime));
                Con.Open();
                Console.WriteLine("Commands executed! Total rows affected are " + insertCommand.ExecuteNonQuery());
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <returns></returns>
        public bool UpdateLastVisiteDate(LocationData objLocationData)
        {
            try
            {
                SqlCommand UpdateCommand = new SqlCommand("UPDATE dbo.Notifications SET LastVistDateTime = @LastVistDateTime where mac=@Mac", Con);
                UpdateCommand.Parameters.Add("@LastVistDateTime", SqlDbType.DateTime).Value = objLocationData.LastSeenDatetime;
                UpdateCommand.Parameters.Add("@Mac", SqlDbType.NVarChar).Value = objLocationData.mac;
                Con.Open();
                Console.WriteLine("Commands executed! Total rows affected are " + UpdateCommand.ExecuteNonQuery());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool UpdateNotificationData(LocationData objLocationData)
        {
            try
            {
                SqlCommand UpdateCommand = new SqlCommand("UPDATE dbo.Notifications SET NotifiedDateTime = @NotifiedDateTime where mac=@Mac", Con);
                UpdateCommand.Parameters.Add("@NotifiedDateTime", SqlDbType.DateTime).Value = objLocationData.LastSeenDatetime;
                UpdateCommand.Parameters.Add("@Mac", SqlDbType.NVarChar).Value = objLocationData.mac;
                Con.Open();
                Console.WriteLine("Commands executed! Total rows affected are " + UpdateCommand.ExecuteNonQuery());
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int IsMacAddressExist(LocationData objLocationData)
        {
            int count = 0;
            try
            {
                SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM dbo.MacAddresses where mac='" + objLocationData.mac + "'", Con);
                Con.Open();
                count = (int)comm.ExecuteScalar();
                Console.WriteLine("Commands executed! Total rows affected are " + count);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return count;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int IsNotificationSentBefore(LocationData objLocationData)
        {
            int CountNotification = 0;
            try
            {
                SqlCommand selectCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.Notifications Where mac='" + objLocationData.mac + "'", Con);
                Con.Open();
                CountNotification = (int)selectCommand.ExecuteScalar();
                Console.Write("Executed Query" + CountNotification);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return CountNotification;
        }


        public DateTime LastSeenDateTimeForMacAddress(string mac)
        {
            DateTime NotifiedDateTime;
            try
            {
                SqlCommand selectCommand = new SqlCommand("SELECT LastVistDateTime FROM dbo.Notifications Where mac='" + mac + "'", Con);
                Con.Open();
                NotifiedDateTime = (DateTime)selectCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return NotifiedDateTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public DateTime LastNotifiedDateTimeForMacAddress(string mac)
        {
            DateTime NotifiedDateTime;
            try
            {
                SqlCommand selectCommand = new SqlCommand("SELECT NotifiedDateTime FROM dbo.Notifications Where mac='" + mac + "'", Con);
                Con.Open();
                NotifiedDateTime = (DateTime)selectCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            return NotifiedDateTime;
        }


        public bool IsAlreadyNotified(string mac)
        {
            int CountNotified = 0;
            bool retVal = false;
            try
            {
                SqlCommand selectCommand = new SqlCommand("SELECT COUNT(*) FROM dbo.TrackMembers Where MacAddress='" + mac + "'", Con);
                Con.Open();
                CountNotified = (int)selectCommand.ExecuteScalar();
                Console.Write("Executed Query ISAlreadyNotified" + CountNotified);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
            if (CountNotified > 0)
            {
                retVal = true;
            }
            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsSeenAfterConstantMinute(LocationData objLocationData)
        {
           
            try
            {
                DateTime LastNotiFiedTime = LastSeenDateTimeForMacAddress(objLocationData.mac);
                Console.WriteLine("Sent time"+ LastNotiFiedTime.ToString());
                Console.WriteLine("minute Diff" + objLocationData.LastSeenDatetime.Subtract(LastNotiFiedTime).Minutes);
                if (objLocationData.LastSeenDatetime.Subtract(LastNotiFiedTime).Minutes >= constSkipNotificationForMinutes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Con.Close();
            }
        }

        /// <summary>
        /// If difference between two datetime is 5s for Particular MacAddress then its a consecutice Data
        /// </summary>
        /// <returns></returns>
        public bool IsConsecutiveStreamDataForMac(LocationData objLocationData)
        {
            DateTime SeenDateTime = LastSeenDateTimeForMacAddress(objLocationData.mac);
            Console.WriteLine("LastVist time" + SeenDateTime.ToString()+""+"For MacAddress"+ objLocationData.mac);
            int secDiff = objLocationData.LastSeenDatetime.Subtract(SeenDateTime).Seconds;
            Console.WriteLine("secDiff"+ secDiff);
            if (secDiff >= MINCheckConsecutiveShownDiffInSeconds && secDiff<=MAXCheckConsecutiveShownDiffInSeconds)
            {
                return true;
            }
            else
            {
                return false;
            }
        }   


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objLocationData"></param>
        /// <returns></returns>
        public bool PostRestCall(LocationData objLocationData)
        {
            Console.WriteLine("Enter into the PostRestCall");
            bool retBoolValue = false;
            objLocationData.PostDateTime = DateTime.Now;
            String resContent = JsonConvert.SerializeObject(objLocationData);
            try
            {
                //PostingTime
                using (HttpClient httpClient = new HttpClient())
                {
                    Console.WriteLine(ConfigurationManager.AppSettings["MermberShipApplication"].ToString());
                    httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["MermberShipApplication"]);
                    var result = httpClient.PostAsync("/RealTimeLocation/TestMemeberApplication", new StringContent(resContent, Encoding.UTF8, "application/json")).Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Successfully sent to the Member Application");
                        var resultContent = result.Content.ReadAsStringAsync();
                        Notification objNotifications = JsonConvert.DeserializeObject<Notification>(resultContent.Result);
                        Console.WriteLine(objNotifications.result.errmsg);
                        if (objNotifications.result.returncode == 0)
                        { 
                            retBoolValue=true;
                        }
                        else
                        {
                            retBoolValue=false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retBoolValue = false;
                throw ex;
            }
            return retBoolValue;
        }
    }
}
