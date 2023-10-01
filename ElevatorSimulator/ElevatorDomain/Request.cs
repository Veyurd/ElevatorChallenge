using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain
{
    public class Request
    {

        public Request(int noPerson, int sourceFloor, int destinationFloor)
        {
            NoPersons = noPerson;
            SourceFloor = sourceFloor;
            DestinationFloor = destinationFloor;
        }

        public int NoPersons { get; set; }
        public int SourceFloor { get; set; }
        public int DestinationFloor { get; set; }
    }
}
