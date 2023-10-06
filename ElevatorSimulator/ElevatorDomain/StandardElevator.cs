using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorDomain.Interfaces;
using static ElevatorDomain.Enums.Enums;

namespace ElevatorDomain
{
    public class StandardElevator : IElevator
    {

        //text identifier for an elevator
        public string? ElevatorDesignator { get; set; }

        //the current floor an elevator is on
        public int CurrentFloor { get; set; }
        public int? DestinationFloor { get; set; }
        //Capacity as number of people
        public int Capacity { get; set; }

        public int Occupancy { get; set; }

        public ElevatorMovementStatus MovementStatus { get; set; }

        //depending on elevator types this may be faster or slower
        private int SpeedInMS { get; set; }

        public int? SwitchFloor { get; set; }

        public SortedSet<ElevatorStop> AscendingStops { get; set; }
        public SortedSet<ElevatorStop> DescendingStops { get; set; }

        public ElevatorMovementStatus CycleDirection { get; set; }

        private IDisplay Display { get; set; }

        public StandardElevator(string designation, int currentFloor, IDisplay display)
        {
            Capacity = 10;
            Occupancy = 0;
            ElevatorDesignator = designation;
            CurrentFloor = currentFloor;
            MovementStatus = ElevatorMovementStatus.Stationary;
            SpeedInMS = 2000;

            AscendingStops = new SortedSet<ElevatorStop>(new SortedAscendingElevatorStopComparer());
            DescendingStops = new SortedSet<ElevatorStop>(new SortedDescendingElevatorStopComparer());
            CycleDirection = ElevatorMovementStatus.Stationary;
            SwitchFloor = null;
            Display = display;
        }


        public bool HasStops()
        {
            return AscendingStops.Any() || DescendingStops.Any();
        }


        public void DisplayStatus()
        {
            Display.Display($@"{ElevatorDesignator} on level: {CurrentFloor} status: {MovementStatus} occupancy: {Occupancy}", ConsoleColor.White);
        }


        /// <summary>
        /// Handles movement of the Elevator
        /// </summary>
        /// <returns></returns>
        public async Task Move()
        {
            if (DestinationFloor.HasValue)
            {
                if (DestinationFloor.Value == CurrentFloor)
                {
                    MovementStatus = ElevatorMovementStatus.Stationary;
                    NotifyReachedFloor();
                    return;
                }

                MovementStatus = CurrentFloor < DestinationFloor.Value ? ElevatorMovementStatus.Ascending : ElevatorMovementStatus.Descending;
                while (CurrentFloor != DestinationFloor.Value)
                {
                    await Task.Delay(SpeedInMS);

                    if (MovementStatus == ElevatorMovementStatus.Ascending)
                        CurrentFloor++;
                    if (MovementStatus == ElevatorMovementStatus.Descending)
                        CurrentFloor--;
                    NotifyReachedFloor();
                }

                MovementStatus = ElevatorMovementStatus.Stationary;
            }
        }

        public void Work()
        {
            if (CycleDirection == ElevatorMovementStatus.Ascending)
            {
                if (MovementStatus == 0)
                {
                    DestinationFloor = AscendingStops.First().Floor;
                    Task.Run(async () => Move());
                }
            }
            if (CycleDirection == ElevatorMovementStatus.Descending)
            {
                if (MovementStatus == 0)
                {
                    DestinationFloor = DescendingStops.First().Floor;
                    Task.Run(async () => Move());
                }
            }
        }

        /// <summary>
        /// Processes a request and adds floor data to the current ascendin/descending deestionation lists
        /// </summary>
        /// <param name="request"></param>
        public void Add(Request request)
        {
            Display.Display($"?????????--------Elevator {ElevatorDesignator} has been selected------------????????", ConsoleColor.Green);
            int currentPosition = CurrentFloor;

            // elevator standing still with no orders
            if (AscendingStops.Count == 0 && DescendingStops.Count == 0)
            {
                if (currentPosition <= request.SourceFloor)
                    CycleDirection = ElevatorMovementStatus.Ascending;
                if (currentPosition > request.SourceFloor)
                    CycleDirection = ElevatorMovementStatus.Descending;
            }


            //determine if the request will set a switchfloor
            if (SwitchFloor == null)
            {
                if ((currentPosition <= request.SourceFloor && request.SourceFloor > request.DestinationFloor) ||
                    (currentPosition > request.SourceFloor && request.SourceFloor <= request.DestinationFloor)
                    )
                {
                    SwitchFloor = request.SourceFloor;
                }
            }

            if (currentPosition <= request.SourceFloor)
            {
                if (AscendingStops.FirstOrDefault(p => p.Floor == request.SourceFloor) != null)
                {
                    AscendingStops.FirstOrDefault(p => p.Floor == request.SourceFloor).OccupancyMod += request.NoPersons;
                }
                else
                    AscendingStops.Add(new ElevatorStop(request.SourceFloor, request.NoPersons));
            }
            else
            {
                if (DescendingStops.FirstOrDefault(p => p.Floor == request.SourceFloor) != null)
                {
                    DescendingStops.FirstOrDefault(p => p.Floor == request.SourceFloor).OccupancyMod += request.NoPersons;
                }
                else
                DescendingStops.Add(new ElevatorStop(request.SourceFloor, request.NoPersons));
            }

            if (request.DestinationFloor >= request.SourceFloor)
            {
                if (AscendingStops.FirstOrDefault(p => p.Floor == request.DestinationFloor) != null)
                {
                    AscendingStops.FirstOrDefault(p => p.Floor == request.DestinationFloor).OccupancyMod -= request.NoPersons;
                }
                else
                    AscendingStops.Add(new ElevatorStop(request.DestinationFloor, -1 * request.NoPersons));
            }
            else
            {
                if (DescendingStops.FirstOrDefault(p => p.Floor == request.DestinationFloor) != null)
                {
                    DescendingStops.FirstOrDefault(p => p.Floor == request.DestinationFloor).OccupancyMod -= request.NoPersons;
                }
                else
                    DescendingStops.Add(new ElevatorStop(request.DestinationFloor, -1 * request.NoPersons));
            }
        }

        /// <summary>
        /// Handles logic upon reaching a certain floor
        /// </summary>
        public void NotifyReachedFloor()
        {
            int floor = CurrentFloor;

            var goingUpOrders = AscendingStops.Where(p => p.Floor == floor).ToList();
            var goingDownOrders = DescendingStops.Where(p => p.Floor == floor).ToList();

            int entering = 0;
            int exiting = 0;

            if (CycleDirection == ElevatorMovementStatus.Ascending)
            {
                foreach (var goingup in goingUpOrders)
                {

                    if (goingup.OccupancyMod < 0)
                        exiting += goingup.OccupancyMod;
                    if (goingup.OccupancyMod > 0)
                        entering += goingup.OccupancyMod;

                    Occupancy += goingup.OccupancyMod;

                    AscendingStops.Remove(goingup);
                }


            }

            if (CycleDirection == ElevatorMovementStatus.Descending)
            {
                foreach (var goindown in goingDownOrders)
                {
                    if (goindown.OccupancyMod < 0)
                        exiting += goindown.OccupancyMod;
                    if (goindown.OccupancyMod > 0)
                        entering += goindown.OccupancyMod;

                    Occupancy += goindown.OccupancyMod;
                    DescendingStops.Remove(goindown);
                }

            }


            if (goingUpOrders.Count > 0 && CycleDirection == ElevatorMovementStatus.Ascending)
            {
                Display.Display($"-----------Elevator {ElevatorDesignator} reached floor {floor}, entering persons: {entering}, exiting persons: {Math.Abs(exiting)}, occupancy: {Occupancy}", ConsoleColor.Cyan);
                Display.Display($"-----------Remaining Stops: {String.Join(";", AscendingStops.Select(p => p.Floor))};{String.Join(";", DescendingStops.Select(p => p.Floor))}", ConsoleColor.Yellow);
            }
            if (CycleDirection == ElevatorMovementStatus.Descending && goingDownOrders.Count > 0)
            {
                Display.Display($"-----------Elevator {ElevatorDesignator} reached floor {floor}, entering persons: {entering}, exiting persons: {Math.Abs(exiting)}, occupancy: {Occupancy}", ConsoleColor.Cyan);
                Display.Display($"-----------Remaining Stops: {String.Join(";", DescendingStops.Select(p => p.Floor))};{String.Join(";", AscendingStops.Select(p => p.Floor))}", ConsoleColor.Yellow);
            }


            if (SwitchFloor.HasValue && SwitchFloor.Value == floor)
            {
                SwitchFloor = null;
                CycleDirection = CycleDirection == ElevatorMovementStatus.Descending ? ElevatorMovementStatus.Ascending : ElevatorMovementStatus.Descending;
            }
            //has handled all his stops
            if (AscendingStops.Count == 0 && DescendingStops.Count == 0)
            {
                CycleDirection = ElevatorMovementStatus.Stationary;

                Display.Display($"-----------Elevator {ElevatorDesignator}: finished orders---------", ConsoleColor.Magenta);
            }
        }


        /// <summary>
        /// Calculates if a request can be accepted by the elevator based on current projected occupancy and incoming request numbers
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanAccomodateNoPersons(Request request)
        {
            var SimulatedAscendingStops = new SortedSet<ElevatorStop>(new SortedAscendingElevatorStopComparer());
            var SimulatedDescendingStops = new SortedSet<ElevatorStop>(new SortedDescendingElevatorStopComparer());
            int? SimulatedSwitchFloor = null;
            int simulatedCurrentPosition = CurrentFloor;
            SimulatedSwitchFloor = SwitchFloor;
            ElevatorMovementStatus simulatedCycleDirection = CycleDirection;


            // Copy stops and ocupancy modifications to the simulated Sorted sets.

            foreach (var stop in AscendingStops)
                SimulatedAscendingStops.Add(stop);
            foreach (var stop in DescendingStops)
                SimulatedDescendingStops.Add(stop);

            //simulate adding this request to the current destination lists and calculate if at any point the elevator would become overpopulated

            // elevator standing still with no orders
            if (SimulatedAscendingStops.Count == 0 && SimulatedDescendingStops.Count == 0)
            {
                if (simulatedCurrentPosition <= request.SourceFloor)
                    simulatedCycleDirection = ElevatorMovementStatus.Ascending;
                if (simulatedCurrentPosition > request.SourceFloor)
                    simulatedCycleDirection = ElevatorMovementStatus.Descending;
            }


            //determine if the request will set a switchfloor
            if (SimulatedSwitchFloor == null)
            {
                if ((simulatedCurrentPosition <= request.SourceFloor && request.SourceFloor > request.DestinationFloor) ||
                    (simulatedCurrentPosition > request.SourceFloor && request.SourceFloor <= request.DestinationFloor)
                    )
                {
                    SimulatedSwitchFloor = request.SourceFloor;
                }
            }

            if (simulatedCurrentPosition <= request.SourceFloor)
            {
                SimulatedAscendingStops.Add(new ElevatorStop(request.SourceFloor, request.NoPersons));
            }
            else
            {
                SimulatedDescendingStops.Add(new ElevatorStop(request.SourceFloor, request.NoPersons));
            }

            if (request.DestinationFloor >= request.SourceFloor)
            {
                SimulatedAscendingStops.Add(new ElevatorStop(request.DestinationFloor, -1 * request.NoPersons));
            }
            else
            {
                SimulatedDescendingStops.Add(new ElevatorStop(request.DestinationFloor, -1 * request.NoPersons));
            }


            bool canBeAccomodated = true;
            int simulatedOccupancy = Occupancy;
            if (simulatedCycleDirection == ElevatorMovementStatus.Ascending)
            {
                // Elevator going up scan occupancy calculator going up then going down
                for (int i = 0; i < SimulatedAscendingStops.Count; i++)
                {
                    simulatedOccupancy += SimulatedAscendingStops.ElementAt(i).OccupancyMod;
                    if (simulatedOccupancy > Capacity)
                    {
                        canBeAccomodated = false;
                        // No reason to continue looping
                        break;
                    }
                }
                for (int i = 0; i < SimulatedDescendingStops.Count; i++)
                {
                    simulatedOccupancy += SimulatedDescendingStops.ElementAt(i).OccupancyMod;
                    if (simulatedOccupancy > Capacity)
                    {
                        canBeAccomodated = false;
                        // No reason to continue looping
                        break;
                    }
                }
            }
            if (simulatedCycleDirection == ElevatorMovementStatus.Descending)
            {
                // Elevator going down scan occupancy calculator going down then going up
                for (int i = 0; i < SimulatedDescendingStops.Count; i++)
                {
                    simulatedOccupancy += SimulatedDescendingStops.ElementAt(i).OccupancyMod;
                    if (simulatedOccupancy > Capacity)
                    {
                        canBeAccomodated = false;
                        // No reason to continue looping
                        break;
                    }
                }
                for (int i = 0; i < SimulatedAscendingStops.Count; i++)
                {
                    simulatedOccupancy += SimulatedAscendingStops.ElementAt(i).OccupancyMod;
                    if (simulatedOccupancy > Capacity)
                    {
                        canBeAccomodated = false;
                        // No reason to continue looping
                        break;
                    }
                }

            }

            return canBeAccomodated;
        }

        /// <summary>
        /// Check if a given request can be handled by the elevator based on current cycle movement and inflexion point
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanAccomodateRequestDirection(Request request)
        {
            bool canAccomodate = (MovementStatus == ElevatorMovementStatus.Stationary && !HasStops()) ||
                    (
                     MovementStatus == ElevatorMovementStatus.Ascending &&
                     request.SourceFloor >= CurrentFloor &&
                        (
                            (SwitchFloor == null && request.SourceFloor >= CurrentFloor && request.DestinationFloor > request.SourceFloor) ||
                            (SwitchFloor.HasValue && request.SourceFloor <= SwitchFloor.Value && request.DestinationFloor <= SwitchFloor.Value)
                        )
                     ) ||
                    (
                    MovementStatus == ElevatorMovementStatus.Descending &&
                    request.SourceFloor <= CurrentFloor &&
                        (
                            (SwitchFloor == null && request.SourceFloor <= Occupancy && request.DestinationFloor < request.SourceFloor) ||
                            (SwitchFloor.HasValue && request.SourceFloor >= SwitchFloor.Value && request.DestinationFloor >= SwitchFloor.Value)
                        )

                    );

            return canAccomodate;
        }
    }

    // Small helper classes, i'm not very sure how worthwile it would be to externalize them in separate files. Truthfully this could be replaced with Tuple<int,int>.
    // It's just a pairing between a floor number and a number of incoming/outgoing persons
    public class ElevatorStop
    {

        public ElevatorStop(int floor, int occupancyMod)
        {
            Floor = floor;
            OccupancyMod = occupancyMod;
        }
        public int Floor { get; set; }
        public int OccupancyMod { get; set; }
    }

    internal class SortedAscendingElevatorStopComparer : IComparer<ElevatorStop>
    {
        public int Compare(ElevatorStop x, ElevatorStop y)
        {
            return x.Floor.CompareTo(y.Floor);
        }
    }

    internal class SortedDescendingElevatorStopComparer : IComparer<ElevatorStop>
    {
        public int Compare(ElevatorStop x, ElevatorStop y)
        {
            return y.Floor.CompareTo(x.Floor);
        }
    }

}
