// See https://aka.ms/new-console-template for more information
using ElevatorDomain;

Console.WriteLine("Hello, World!");

string Input = "0";

int NoElevators;

Console.WriteLine("Number of elevators:");
NoElevators = Convert.ToInt32(Console.ReadLine());



Dispatcher dispatcher = new Dispatcher();

Task.Run(async () => dispatcher.ProcessRequests());

for (int i = 0; i < NoElevators; i++)
{
    Elevator elevator = new Elevator("Elevator_" + i, 0);
    dispatcher.AddElevator(elevator);
}


while (Input != "Exit")
{
 

    if (Input == "Status")
    {
        foreach (var elevator in dispatcher.Elevators)
        {
            Console.WriteLine($@"{elevator.ElevatorDesignator} on level: {elevator.CurrentFloor} status: {elevator.MovementStatus} occupancy: {elevator.Occupancy}");
        }
    }

    if (Input == "Call")
    {
        Console.WriteLine("Which floor are you on?");
        int callingFloor = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("How Many persons?");
        int occupants=Convert.ToInt32(Console.ReadLine());

        Task.Run(async () => dispatcher.AddUnprocessedCallRequest(new CallRequest(occupants,callingFloor)));
    }

    Console.WriteLine("-------------------------");
    Console.WriteLine("Operations:");
    Console.WriteLine("Status :prints status");
    Console.WriteLine("Exit : closes the program");
    Console.WriteLine("Call : closes the program");

    Input = Console.ReadLine();
}

