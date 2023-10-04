namespace ElevatorDomain.Interfaces
{
    public interface IElevator
    {
        SortedSet<ElevatorStop> AscendingStops { get; set; }
        int Capacity { get; set; }
        int CurrentFloor { get; set; }
        Enums.Enums.ElevatorMovementStatus CycleDirection { get; set; }
        SortedSet<ElevatorStop> DescendingStops { get; set; }
        int? DestinationFloor { get; set; }
        string? ElevatorDesignator { get; set; }
        Enums.Enums.ElevatorMovementStatus MovementStatus { get; set; }
        int Occupancy { get; set; }
        int? SwitchFloor { get; set; }

        void Add(Request request);
        bool HasStops();
        Task Move();
        void NotifyReachedFloor();
    }
}