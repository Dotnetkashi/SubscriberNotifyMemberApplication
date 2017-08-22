using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroMqSubscriber
{
   public class GroupDevicesWithVisit
    {
        public string MacAddress { get; set; }

        public int NumberOfTimesShownInParticularArea { get; set; }

        public DateTime AppearedInParticularAreaDateTime { get; set; }

        public DateTime LastNotifiedDateTime { get; set; }

        public bool IsNotifiedOnce{ get; set; }

        public int NoOftimesNotified { get; set; }
    }
}
