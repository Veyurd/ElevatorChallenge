// See https://aka.ms/new-console-template for more information
using ElevatorDomain;

Console.WriteLine("Wellcome to Noni's Elevator Challenge!");



string Input = "";

//read elevator count
int NoElevators;
Console.WriteLine("Number of elevators:");
NoElevators = Convert.ToInt32(Console.ReadLine());

//read maximum number of floors
int NoFloors;
Console.WriteLine("Number of floors:");
NoFloors = Convert.ToInt32(Console.ReadLine());



//initiate dispatcher and start processing requests
StandardDispatcher dispatcher = new StandardDispatcher(NoFloors);
Task.Run(async () => dispatcher.ProcessRequests());



for (int i = 0; i < NoElevators; i++)
{
    StandardElevator elevator = new StandardElevator("Elevator_" + i, 0,dispatcher);
    dispatcher.AddElevator(elevator);
}


//Main loop opf program execution

while (Input != "Exit")
{

    if (Input.ToUpper() == "STATUS" || Input == "1")
    {
        foreach (var elevator in dispatcher.Elevators)
        {
            Console.WriteLine($@"{elevator.Key.ElevatorDesignator} on level: {elevator.Key.CurrentFloor} status: {elevator.Key.MovementStatus} occupancy: {elevator.Key.Occupancy}");
        }
    }

    if (Input.ToUpper() == "CALL" || Input=="2")
    {
        Console.WriteLine("Which floor are you on?");
        int callingFloor = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Which floor are going to?");
        int destinationFloor = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("How many persons?");
        int occupants=Convert.ToInt32(Console.ReadLine());

        Task.Run(async () => dispatcher.AddUnprocessedCallRequest(new Request(occupants,callingFloor, destinationFloor)));
    }

    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("-------------------------");
    Console.WriteLine("Operations:");
    Console.WriteLine("(1)Status :prints status");
    Console.WriteLine("(2)Call : closes the program");
    Console.WriteLine("(3)Exit : closes the program");

    Console.ForegroundColor= ConsoleColor.Green;
    Console.WriteLine("Next command: ");
    Input = Console.ReadLine();
    Console.ResetColor();
}

