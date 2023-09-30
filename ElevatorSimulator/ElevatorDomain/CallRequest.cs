using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain
{
    public class CallRequest
    {

        public CallRequest(int noPerson, int sourceFloor)
        {
            NoPersons = noPerson;
            SourceFloor = sourceFloor;
        }

        public int NoPersons { get; set; }
        public int SourceFloor { get; set; }
    }
}
