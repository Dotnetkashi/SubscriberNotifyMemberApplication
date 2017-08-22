using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroMqSubscriber.Models
{
    public class Notification
    {
        public Result result { get; set; }
    }
    public class Result
    {
        public int returncode { get; set; }
        public string errmsg { get; set; }
    }
}
