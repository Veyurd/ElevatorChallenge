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


        private int RequestProcessingRate { get; set; }
        public Queue<Request> UnprocessedCallRequests { get; set; }
        public List<IElevator> Elevators { get; set; }



        public StandardDispatcher()
        {
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
            UnprocessedCallRequests.Enqueue(request);
        }


        public async Task ProcessRequests()
        {
            while (true)
            {
                await Task.Delay(RequestProcessingRate);
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
                    elevator.Work();
                }
            }
        }

        /// <summary>
        /// Algorithm to caluclate the "best" elevator that should service incoming request.
        /// Resides in the dispatcher because only the dispatcher has knowledge about all elevators in the system
        /// </summary>
        /// <param name="request">Incoming request</param>
        /// <returns></returns>
        private IElevator? BestCandidate(Request request)
        {
            var elevatorsList = Elevators;
            IElevator result = null;
            int resultCurrentFloor = int.MaxValue;

            // Because elevators are not always sorted via their current floor, and needing a minimum of O(NlogN) to sort them, a linear search and determination could be better here O(N).
            for (int i = 0; i < elevatorsList.Count; i++)
            {

                // Calculate the distance from the requests source floor
                IElevator candidate = elevatorsList[i];
                int currentDistance = Math.Abs(request.SourceFloor - resultCurrentFloor);
                int candidateDistance = Math.Abs(request.SourceFloor - candidate.CurrentFloor);

                // Calculate if the direction is good based on multiple factors (elevator general direction, pressence of existing stops etc
                bool directionIsGood = candidate.CanAccomodateRequestDirection(request);

                // This should be refactored to take into account the estimated occupancy at the moment of arrival at the source 
                if (candidateDistance < currentDistance && directionIsGood && candidate.CanAccomodateNoPersons(request))
                {
                    result = candidate;
                    resultCurrentFloor = candidateDistance;
                }

            }

            return result;
        }
    }
}
