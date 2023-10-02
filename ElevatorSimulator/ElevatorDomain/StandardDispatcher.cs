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
        public Dictionary<IElevator, ElevatorRequestData> Elevators { get; set; }



        public StandardDispatcher(int noFloors)
        {
            NoFloors = noFloors;
            UnprocessedCallRequests = new Queue<Request>();
            Elevators = new Dictionary<IElevator, ElevatorRequestData>();
            RequestProcessingRate = 100;
        }



        public void AddElevator(IElevator elevator)
        {
            Elevators.Add(elevator, new ElevatorRequestData());
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
                        var commandList = Elevators[elevator];
                        commandList.Add(callRequest);
                        Elevators[elevator] = commandList;
                    }
                }

                foreach (var key in Elevators.Keys)
                {
                    var elevatorData = Elevators[key];

                    List<int> destinations = elevatorData.DestinationsTree.TraverseInOrder(elevatorData.DestinationsTree.Root);

                    if (destinations.Any() && key.MovementStatus == 0)
                    {
                        Task.Run(async () => key.Move(Math.Abs(destinations[0])));
                    }

                }
            }
        }


        public void ReceiveElevatorNotification(IElevator sourceElevator)
        {

            var elevatorData = Elevators[sourceElevator];

            //remove the stop that has been executed
            elevatorData.DestinationsTree.Remove(sourceElevator.CurrentFloor);

            int entering = 0;
            int exiting = 0;

            for (int i = 0; i < elevatorData.Requests.Count; i++)
            {
                if (elevatorData.Requests[i].SourceFloor == sourceElevator.CurrentFloor)
                {
                    entering += elevatorData.Requests[i].NoPersons;
                }
                if (elevatorData.Requests[i].DestinationFloor == sourceElevator.CurrentFloor)
                {
                    exiting += elevatorData.Requests[i].NoPersons;
                    elevatorData.Requests.Remove(elevatorData.Requests[i]);
                }
            }

            sourceElevator.Occupancy += entering;
            sourceElevator.Occupancy -= exiting;


            Elevators[sourceElevator] = elevatorData;

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"Elevator {sourceElevator.ElevatorDesignator} arrived at floor: {sourceElevator.CurrentFloor}, {entering} persons enter, {exiting} persons exit");
            List<int> remainingStops = elevatorData.DestinationsTree.TraverseInOrder(elevatorData.DestinationsTree.Root);
            if (remainingStops.Any())
            {
                Console.WriteLine("Remaining stops:" + String.Join(";", remainingStops.Select(p=>Math.Abs(p))));
            }
            Console.ResetColor();

        }


        private IElevator? BestCandidate(Request request)
        {

            var elevatorsList = Elevators.Keys.ToList();

            //algorithm to calculate the ideal elevator to come and pick up the people calling for it
            IElevator result = null;

            int resultCurrentFloor = int.MaxValue;

            //because elevators are not always sorted via their current floor, and needing a minimum of O(NlogN) to sort them, a liniar search and determinaton could be better here O(N)
            for (int i = 0; i < elevatorsList.Count; i++)
            {

                IElevator candidate = elevatorsList[i];
                int currentDistance = Math.Abs(request.SourceFloor - resultCurrentFloor);
                int candidateDistance = Math.Abs(request.SourceFloor - candidate.CurrentFloor);

                ElevatorMovementStatus orderDirection = request.SourceFloor <= request.DestinationFloor ? ElevatorMovementStatus.Ascending : ElevatorMovementStatus.Descending;

                //bool directionIsGood = (
                //    (request.SourceFloor > candidate.CurrentFloor) &&
                //    (candidate.MovementStatus == ElevatorMovementStatus.Stationary || candidate.MovementStatus == ElevatorMovementStatus.Ascending)
                //    ) ||
                //   (
                //   (request.SourceFloor <= candidate.CurrentFloor) &&
                //   (candidate.MovementStatus == ElevatorMovementStatus.Stationary || candidate.MovementStatus == ElevatorMovementStatus.Descending)
                //   );

                bool directionIsGood = (candidate.MovementStatus == ElevatorMovementStatus.Stationary && Elevators[candidate].Requests.Count==0) ||
                    (candidate.MovementStatus == ElevatorMovementStatus.Ascending && orderDirection == ElevatorMovementStatus.Ascending && request.SourceFloor >= candidate.CurrentFloor) ||
                    (candidate.MovementStatus == ElevatorMovementStatus.Descending && orderDirection == ElevatorMovementStatus.Descending && request.SourceFloor <= candidate.CurrentFloor);

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
