using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoMalfunctionDetection
{
    public class trainevent
    {
        public Guid RideId { get; set; }

        public Guid TrainId { get; set; }

        public Guid CorrelationId { get; set; }

        public string EventType { get; set; }

        public int PassengerCount { get; set; }

        //public double AccelX { get; set; }

        //public double AccelY { get; set; }

        //public double AccelZ { get; set; }

        //public double RotX { get; set; }

        //public double RotY { get; set; }

        //public double RotZ { get; set; }

        public DateTime DeviceTime { get; set; }


    }
}
