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

        //Capacity as number of people
        public int Capacity { get; set; }

        public int Occupancy { get; set; }

        //todo refactor in enum (descending, ascending, stationary)
        public ElevatorMovementStatus MovementStatus { get; set; }

        private IDispatcher Dispatcher { get; set; }




        //depending on elevator types this may be faster or slower
        private int SpeedInMS { get; set; }


        public StandardElevator(string designation, int currentFloor, IDispatcher dispatcher)
        {
            Capacity = 10;
            Occupancy = 0;
            ElevatorDesignator = designation;
            CurrentFloor = currentFloor;
            MovementStatus = ElevatorMovementStatus.Stationary;
            SpeedInMS = 2000;
            Dispatcher = dispatcher;
        }

        public async Task Move(int destinationFloor)
        {

            if (destinationFloor == CurrentFloor)
            {
                MovementStatus = ElevatorMovementStatus.Stationary;
                //notify dispatcher
                Dispatcher.ReceiveElevatorNotification(this);
                return;
            }

            MovementStatus = CurrentFloor < destinationFloor ? ElevatorMovementStatus.Ascending : ElevatorMovementStatus.Descending;
            while (CurrentFloor != destinationFloor)
            {
                await Task.Delay(SpeedInMS);

                if (MovementStatus == ElevatorMovementStatus.Ascending)
                    CurrentFloor++;
                if (MovementStatus == ElevatorMovementStatus.Descending)
                    CurrentFloor--;
            }
            MovementStatus = ElevatorMovementStatus.Stationary;
            Dispatcher.ReceiveElevatorNotification(this);
        }
    }
}
