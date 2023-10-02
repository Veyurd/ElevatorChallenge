using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain
{
    public  class ElevatorRequestData
    {
        public ElevatorRequestData() {
            Requests = new List<Request>();
            DestinationsTree = new BinaryTree();
        }
        public List<Request> Requests { get; set; }

        public BinaryTree DestinationsTree { get; set; }

        public void Add(Request request)
        { 
            Requests.Add(request);

            if (request.SourceFloor <= request.DestinationFloor)
            {
                DestinationsTree.Add(request.SourceFloor);
                DestinationsTree.Add(request.DestinationFloor);
            }
            else
            {
                DestinationsTree.Add(-1 * request.SourceFloor);
                DestinationsTree.Add(-1 * request.SourceFloor);
            }
        }
    }
}
