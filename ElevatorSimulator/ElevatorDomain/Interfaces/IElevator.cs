namespace ElevatorDomain.Interfaces
{
    public interface IElevator
    {
        int Capacity { get; set; }
        int CurrentFloor { get; set; }
        string? ElevatorDesignator { get; set; }
        Enums.Enums.ElevatorMovementStatus MovementStatus { get; set; }
        int Occupancy { get; set; }
        Task Move(int destinationFloor);
    }
}