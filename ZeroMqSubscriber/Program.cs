using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroMQ;
using ZeroMqSubscriber.Models;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;


namespace ZeroMqSubscriber
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
                string topic = "device";
                string url = "tcp://10.38.129.170:5560";
                using (var context = new ZContext())
                using (var subscriber = new ZSocket(context, ZSocketType.SUB))
                {
                    //Create the Subscriber Connection
                    subscriber.Connect("tcp://10.38.129.170:5560");
                    Console.WriteLine("Subscriber started for Topic with URL : {0} {1}", topic, url);
                    subscriber.Subscribe(topic);
                    int subscribed = 0;
                    
                    while (true)
                    {
                        using (ZMessage message = subscriber.ReceiveMessage())
                        {
                            subscribed++;

                            // Read message contents
                            string contents = message[1].ReadString();

                            Console.WriteLine(contents);

                            LocationData objLocationData = JsonConvert.DeserializeObject<ListOfArea>(contents).device_notification.records.FirstOrDefault();

                            Library objLibray = new Library();
                            bool CheckSeenAfterConstantMinute = false;
                            bool CheckConsecutiveVisit = false;
                            bool CheckAlreadyNotifiedOnce = false;
                            Console.WriteLine("Check the MacAddress exist or Not in particular server");
                            //if the MacAddress is Registered through our RTLS Service Layer
                            if (objLibray.IsMacAddressExist(objLocationData) > 0)
                            {

                                Console.WriteLine(objLocationData.mac + "Mac Exist");

                                //Check the Particular X and Y coordinates for the particular BGEO-15 Area
                                //if ((objLocationData.x >= xLeft && objLocationData.x <= xRight) || (objLocationData.y >= yRight && objLocationData.y <= yLeft))
                                //{

                                //    Console.WriteLine("Find the MacAddress inside the Area" + objLocationData.mac);
                                //    //Convert the Linux Echo TimeStamp to DateTime format

                                DateTime MacFoundDatetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).AddSeconds(objLocationData.last_seen_ts);
                                objLocationData.LastSeenDatetime = MacFoundDatetime;

                                //Console.WriteLine("MacAddress before" + PreviousMacAddress + "MacAddress after" + objLocationData.mac);

                                //Alrearady Exist MacAddress to update the Notification LastSeenDateTime
                                if (objLibray.IsNotificationSentBefore(objLocationData) >= 1)
                                {
                                    objLibray.UpdateLastVisiteDate(objLocationData);
                                    CheckSeenAfterConstantMinute = objLibray.IsSeenAfterConstantMinute(objLocationData);
                                    CheckConsecutiveVisit = objLibray.IsConsecutiveStreamDataForMac(objLocationData);
                                    CheckAlreadyNotifiedOnce = objLibray.IsAlreadyNotified(objLocationData.mac);

                                    Console.WriteLine("CheckSeenAfterConstantMinute" + CheckSeenAfterConstantMinute);
                                    Console.WriteLine("CheckConsecutiveVisit"+CheckConsecutiveVisit);
                                    Console.WriteLine("CheckAlreadyNotifiedOnce" + CheckAlreadyNotifiedOnce);


                                    if ( CheckConsecutiveVisit == true && CheckAlreadyNotifiedOnce == false)
                                    {
                                        Console.WriteLine("enter into if");
                                        objLibray.PostRestCall(objLocationData);
                                        objLibray.UpdateNotificationData(objLocationData);
                                    }
                                    else if(CheckSeenAfterConstantMinute == true  && CheckAlreadyNotifiedOnce == true)
                                    {
                                        Console.WriteLine("enter into else if");
                                        objLibray.PostRestCall(objLocationData);
                                        objLibray.UpdateNotificationData(objLocationData);
                                    }
                                }
                                //New MacAddress For Storing in Notification table
                                else
                                {
                                    objLibray.InsertData(objLocationData);
                                }
                                //If a device seen continiously with in 5s straming data
                                //PreviousMacAddress = objLocationData.mac;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                throw ex;
            }
        }
    }
}

