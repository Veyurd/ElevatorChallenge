using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ElevatorDomain.Enums;
using ElevatorDomain.Interfaces;
using static ElevatorDomain.Enums.Enums;

namespace ElevatorDomain
{
    public class StandardDispatcher : IDispatcher
    {


        private int NoFloors { get; set; }
        private int RequestProcessingRate { get; set; }
        public Queue<Request> UnprocessedCallRequests { get; set; }
        public List<IElevator> Elevators { get; set; }



        public StandardDispatcher(int noFloors)
        {
            NoFloors = noFloors;
            UnprocessedCallRequests = new Queue<Request>();
            Elevators = new List<IElevator>();
            RequestProcessingRate = 100;
        }



        public void AddElevator(IElevator elevator)
        {
            Elevators.Add(elevator);
        }

        public void AddUnprocessedCallRequest(Request request)
        {
            bool isValidRequest = true;
            if (request.DestinationFloor > NoFloors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Destination Floor is grater than the top floor of the building");
                Console.ResetColor();
                isValidRequest = false;
            }
            if (request.SourceFloor < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Elevator cannot start from beneath the ground floor (0)");
                Console.ResetColor();
                isValidRequest = false;
            }
            if (isValidRequest)
                UnprocessedCallRequests.Enqueue(request);
        }


        public async Task ProcessRequests()
        {
            while (true)
            {
                await Task.Delay(RequestProcessingRate);
                // Console.WriteLine("processingRequests");
                Request callRequest;
                UnprocessedCallRequests.TryPeek(out callRequest);
                if (callRequest != null)
                {
                    var elevator = BestCandidate(callRequest);
                    if (elevator != null)
                    {
                        UnprocessedCallRequests.Dequeue();
                        elevator.Add(callRequest);
                    }
                }

                foreach (var elevator in Elevators)
                {

                    if (elevator.CycleDirection== ElevatorMovementStatus.Ascending)
                    {
                        if (elevator.MovementStatus == 0)
                        {
                            elevator.DestinationFloor = elevator.AscendingStops.First().Floor;
                            Task.Run(async () => elevator.Move());
                        }
                    }
                    if(elevator.CycleDirection== ElevatorMovementStatus.Descending)
                    {
                        if (elevator.MovementStatus == 0)
                        {
                            elevator.DestinationFloor = elevator.DescendingStops.First().Floor;
                            Task.Run(async () => elevator.Move());
                        }
                    }
                }
            }
        }

        private IElevator? BestCandidate(Request request)
        {

            var elevatorsList = Elevators;

            //algorithm to calculate the ideal elevator to come and pick up the people calling for it
            IElevator result = null;

            int resultCurrentFloor = int.MaxValue;

            //because elevators are not always sorted via their current floor, and needing a minimum of O(NlogN) to sort them, a liniar search and determinaton could be better here O(N)
            for (int i = 0; i < elevatorsList.Count; i++)
            {

                IElevator candidate = elevatorsList[i];
                int currentDistance = Math.Abs(request.SourceFloor - resultCurrentFloor);
                int candidateDistance = Math.Abs(request.SourceFloor - candidate.CurrentFloor);
           



                bool directionIsGood = (candidate.MovementStatus == ElevatorMovementStatus.Stationary && !candidate.HasStops()) ||
                    (
                     candidate.MovementStatus == ElevatorMovementStatus.Ascending &&
                     request.SourceFloor >= candidate.CurrentFloor &&
                        (
                            candidate.SwitchFloor == null ||
                            (candidate.SwitchFloor.HasValue && request.SourceFloor <= candidate.SwitchFloor.Value && request.DestinationFloor <= candidate.SwitchFloor.Value)
                        )
                     ) ||
                    (
                    candidate.MovementStatus == ElevatorMovementStatus.Descending &&
                    request.SourceFloor <= candidate.CurrentFloor &&
                        (
                            candidate.SwitchFloor == null ||
                            (candidate.SwitchFloor.HasValue && request.SourceFloor >= candidate.SwitchFloor.Value && request.DestinationFloor>= candidate.SwitchFloor.Value )
                        )

                    );

                //taklle into account capacity as number of people
                if (candidateDistance < currentDistance && directionIsGood && !(candidate.Occupancy + request.NoPersons > candidate.Capacity))
                {
                    result = candidate;
                    resultCurrentFloor = candidateDistance;
                }

            }



            Console.WriteLine($"?????????--------Elevator {result.ElevatorDesignator} has been selected------------????????");

            return result;
        }
    }
}
