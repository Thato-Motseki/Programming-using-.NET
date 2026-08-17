using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class Package
    {
        public string TrackingNumber { get; set; }
        public string Status { get; private set; }

        public Package(string trackingNumber)
        {
            TrackingNumber = trackingNumber;
            Status = "Created";
        }

        public void UpdateStatus(string newStatus)
        {
            Status = newStatus;
        }
    }
}
