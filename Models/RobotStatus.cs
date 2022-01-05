using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<RobotStatus>(myJsonResponse); 
    public class RobotStatus
    {
        public string robotId { get; set; }
        public int batteryLevel { get; set; }
        public int y { get; set; }
        public int x { get; set; }
    }

}
