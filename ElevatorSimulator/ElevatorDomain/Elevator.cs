using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain
{
    public class Elevator
    {

        //text identifier for an elevator
        public string? ElevatorDesignator { get; set; }

        //the current floor an elevator is on
        public int CurrentFloor { get; set; }

        //Capacity as number of people
        public int Capacity { get; set; }

        public int Occupancy { get; set; }

        //todo refactor in enum (descending, ascending, stationary)
        public int MovementStatus { get; set; }

        public Elevator(string designation, int currentFloor)
        {
            Capacity = 10;
            Occupancy = 0;
            ElevatorDesignator = designation;
            CurrentFloor = currentFloor;
            MovementStatus = 0;
        }

        public async Task Move(int destinationFloor)
        {
            if (destinationFloor == CurrentFloor)
            {
                MovementStatus = 0;
                Console.WriteLine(ElevatorDesignator + " is already on floor " + destinationFloor);
                return;
            }

            MovementStatus = CurrentFloor < destinationFloor ? 1 : -1;
            while (CurrentFloor != destinationFloor)
            {
                await Task.Delay(1000);

                if (MovementStatus == 1)
                    CurrentFloor++;
                if (MovementStatus == -1)
                    CurrentFloor--;
               
            }
            MovementStatus = 0;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ElevatorDesignator + " has arrived at floor " + CurrentFloor);
            Console.ResetColor();
        }
    }
}
