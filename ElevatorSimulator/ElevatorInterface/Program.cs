// See https://aka.ms/new-console-template for more information
using ElevatorDomain;
using ElevatorDomain.Interfaces;
using ElevatorDomain.Validators;

Console.WriteLine("Wellcome to Noni's Elevator Challenge!");


IDisplay Display = new StandardDisplay();
string Input = "";

//read elevator count
int NoElevators;
Console.WriteLine("Number of elevators:");
NoElevators = Convert.ToInt32(Console.ReadLine());
if (NoElevators <= 0)
{
    Display.Display("There must be at least 1 elevator", ConsoleColor.Red);
    return;
}

//read maximum number of floors
int MaxFloor;
Console.WriteLine("Max Floor:");
MaxFloor = Convert.ToInt32(Console.ReadLine());
if (MaxFloor <= 0)
{
    Display.Display("The building must have at least 1 floor above ground level (ground level is considered floor 0)", ConsoleColor.Red);
    return;
}


//initiate dispatcher and start processing requests
IDispatcher dispatcher = new StandardDispatcher();
Task.Run(async () => dispatcher.ProcessRequests());


IValidator requestValidator = new RequestValidator(0, MaxFloor);



for (int i = 0; i < NoElevators; i++)
{
    StandardElevator elevator = new StandardElevator("Elevator_" + i, 0,Display);
    dispatcher.AddElevator(elevator);
}


//Main loop of program execution

while (Input.ToUpper() != "Exit" || Input!="3")
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("-------------------------");
    Console.WriteLine("Operations:");
    Console.WriteLine("(1)Status: prints status");
    Console.WriteLine("(2)Request: requests an elevator ride");
    Console.WriteLine("(3)Exit: closes the program");
    Console.ResetColor();
    Display.Display("Next command: ", ConsoleColor.Green);
    Input = Console.ReadLine();

    if (Input.ToUpper() == "STATUS" || Input == "1")
    {
        foreach (var elevator in dispatcher.Elevators)
        {
            elevator.DisplayStatus();
        }
    }

    if (Input.ToUpper() == "REQUEST" || Input=="2")
    {
        Console.WriteLine("Which floor are you on?");
        int callingFloor = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Which floor are going to?");
        int destinationFloor = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("How many persons?");
        int occupants=Convert.ToInt32(Console.ReadLine());


        Request request = new Request(occupants, callingFloor, destinationFloor);
        if (requestValidator.IsValid(request))
        {
            Task.Run(async () => dispatcher.AddUnprocessedCallRequest(request));
        }
        else {
            Display.Display("Invalid Request!", ConsoleColor.Red);
        }
    }



 
}

