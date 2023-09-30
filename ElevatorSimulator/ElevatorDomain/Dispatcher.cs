using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain
{
    public class Dispatcher
    {
        public List<Elevator> Elevators { get; set; }
        public Queue<CallRequest> UnprocessedCallRequests { get; set; }

        public Queue<DestinationRequest> UnprocessedDestinationRequests { get; set; }

        Dictionary<Elevator, List<DestinationRequest>> ElevatorStops { get; set; }


        public Dispatcher()
        {
            Elevators = new List<Elevator>();
            UnprocessedCallRequests = new Queue<CallRequest>();
            UnprocessedDestinationRequests = new Queue<DestinationRequest>();
            ElevatorStops = new Dictionary<Elevator, List<DestinationRequest>>();
        }



        public void AddElevator(Elevator elevator)
        {
            Elevators.Add(elevator);
            ElevatorStops.Add(elevator, new List<DestinationRequest>());
        }

        public void AddUnprocessedCallRequest(CallRequest request)
        {
            UnprocessedCallRequests.Enqueue(request);
        }

        public void AddUnprocessedDestinationRequest(DestinationRequest request)
        {
            UnprocessedDestinationRequests.Enqueue(request);
        }

        public async Task ProcessRequests()
        {
            while (true)
            {
                await Task.Delay(1000);
                // Console.WriteLine("processingRequests");
                CallRequest callRequest;
                UnprocessedCallRequests.TryDequeue(out callRequest);
                if (callRequest != null)
                {
                    var elevator = ClosestCandidate(callRequest);
                    var commandList = ElevatorStops[elevator];
                    commandList.Add(new DestinationRequest { DestinationFloor = callRequest.SourceFloor });
                    commandList=commandList.OrderBy(p=>p.DestinationFloor).ToList();
                    ElevatorStops[elevator] = commandList;

                    //send command to the elevator
                    Task.Run(() => elevator.Move(callRequest.SourceFloor));
                }
            }
        }
     

        private Elevator ClosestCandidate(CallRequest request)
        {
            Elevator result = Elevators[0];
            //refactor to take into account occupancy
            //algorithm to calculate the ideal elevator to come and pick up the people calling for it

            for (int i = 0; i < Elevators.Count; i++)
            {

                Elevator candidate= Elevators[i]; ;
                int currentDistance = Math.Abs(request.SourceFloor - result.CurrentFloor);
                int candidateDistance = Math.Abs(request.SourceFloor - candidate.CurrentFloor);
                bool directionIsGood= ((request.SourceFloor > candidate.CurrentFloor)  && (candidate.MovementStatus == 0 || candidate.MovementStatus == 1)) ||
                   ((request.SourceFloor <= candidate.CurrentFloor) && (candidate.MovementStatus == 0 || candidate.MovementStatus == -1));
                if(candidateDistance<currentDistance && directionIsGood)
                    result = candidate;

            }



            return result;
        }
    }
}
