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

        //todo refactor in enum (descending, ascending, stationary)
        public ElevatorMovementStatus MovementStatus { get; set; }

        //depending on elevator types this may be faster or slower
        private int SpeedInMS { get; set; }

        public int? SwitchFloor { get; set; }
        public SortedSet<ElevatorStop> AscendingStops { get; set; }
        public SortedSet<ElevatorStop> DescendingStops { get; set; }

        public ElevatorMovementStatus CycleDirection { get; set; }




        public StandardElevator(string designation, int currentFloor)
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
        }


        public bool HasStops()
        {
            return AscendingStops.Any() || DescendingStops.Any();
        }

        public async Task Move()
        {

            if (DestinationFloor.HasValue)
            {



                if (DestinationFloor.Value == CurrentFloor)
                {
                    MovementStatus = ElevatorMovementStatus.Stationary;
                    //notify dispatcher
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



        public void Add(Request request)
        {

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
                AscendingStops.Add(new ElevatorStop(request.SourceFloor, request.NoPersons));
            }
            else
            {
                DescendingStops.Add(new ElevatorStop(request.SourceFloor, request.NoPersons));
            }

            if (request.DestinationFloor >= request.SourceFloor)
            {
                AscendingStops.Add(new ElevatorStop(request.DestinationFloor, -1 * request.NoPersons));
            }
            else
            {
                DescendingStops.Add(new ElevatorStop(request.DestinationFloor, -1 * request.NoPersons));
            }
        }


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
                Console.WriteLine($"-----------Elevator {ElevatorDesignator} reached floor {floor}, entering persons: {entering}, exiting persons: {Math.Abs(exiting)}, occupancy: {Occupancy}");
                Console.WriteLine($"-----------Remaining Stops: {String.Join(";", AscendingStops.Select(p => p.Floor))};{String.Join(";", DescendingStops.Select(p => p.Floor))}");
            }
            if (CycleDirection == ElevatorMovementStatus.Descending && goingDownOrders.Count > 0)
            {
                Console.WriteLine($"-----------Elevator {ElevatorDesignator} reached floor {floor}, entering persons: {entering}, exiting persons: {Math.Abs(exiting)}, occupancy: {Occupancy}");
                Console.WriteLine($"-----------Remaining Stops: {String.Join(";", DescendingStops.Select(p => p.Floor))};{String.Join(";", AscendingStops.Select(p => p.Floor))}");
            }


            if (SwitchFloor.HasValue && SwitchFloor.Value == floor)
            {
                SwitchFloor = null;
                CycleDirection = CycleDirection == ElevatorMovementStatus.Descending ? ElevatorMovementStatus.Ascending : ElevatorMovementStatus.Descending;
            }
            //has handled all his stops
            if (AscendingStops.Count == 0 && DescendingStops.Count == 0)
            {
                CycleDirection = 0;
                Console.WriteLine($"-----------Elevator {ElevatorDesignator}: finished orders---------");
            }
        }
    }


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
