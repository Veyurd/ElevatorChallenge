using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ElevatorDomain.Enums;
using static ElevatorDomain.Enums.Enums;

namespace ElevatorDomain
{
    public class Dispatcher
    {


        private int NoFloors { get; set; }
        public Queue<Request> UnprocessedCallRequests { get; set; }
        public Dictionary<Elevator, ElevatorRequestData> Elevators { get; set; }
        private int RequestProcessingRate { get; set; }


        public Dispatcher(int noFloors)
        {
            NoFloors = noFloors;
            UnprocessedCallRequests = new Queue<Request>();
            Elevators = new Dictionary<Elevator, ElevatorRequestData>();
            RequestProcessingRate = 100;
        }



        public void AddElevator(Elevator elevator)
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

                        Task.Run(async () => key.Move(destinations[0]));
                    }

                }
            }
        }


        //for the moment receive a string, we will modify this to a proper info encapsulation
        public void ReceiveElevatorNotification(Elevator sourceElevator)
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
                Console.WriteLine("Remaining stops:" + String.Join(";", remainingStops));
            }
            Console.ResetColor();

        }


        private Elevator? BestCandidate(Request request)
        {

            var elevatorsList = Elevators.Keys.ToList();
            Elevator result = elevatorsList[0];

            //algorithm to calculate the ideal elevator to come and pick up the people calling for it


            //because elevators are not always sorted via their current floor, and needing a minimum of O(NlogN) to sort them, a liniar search and determinaton could be better here O(N)
            for (int i = 0; i < elevatorsList.Count; i++)
            {

                Elevator candidate = elevatorsList[i];
                int currentDistance = Math.Abs(request.SourceFloor - result.CurrentFloor);
                int candidateDistance = Math.Abs(request.SourceFloor - candidate.CurrentFloor);
                bool directionIsGood = (
                    (request.SourceFloor > candidate.CurrentFloor) &&
                    (candidate.MovementStatus == ElevatorMovementStatus.Stationary || candidate.MovementStatus == ElevatorMovementStatus.Ascending)
                    ) ||
                   (
                   (request.SourceFloor <= candidate.CurrentFloor) &&
                   (candidate.MovementStatus == ElevatorMovementStatus.Stationary || candidate.MovementStatus == ElevatorMovementStatus.Descending)
                   );

                //taklle into account capacity as number of people
                if (candidateDistance < currentDistance && directionIsGood && !(candidate.Occupancy + request.NoPersons > candidate.Capacity))
                    result = candidate;

            }
            //final check to account for result being initiated with the first elevator available
            if (result.Occupancy + request.NoPersons > result.Capacity)
                return null;


            return result;
        }
    }
}
