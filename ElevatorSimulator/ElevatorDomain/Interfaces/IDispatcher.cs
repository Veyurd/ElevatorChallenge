namespace ElevatorDomain.Interfaces
{
    public interface IDispatcher
    {
        Dictionary<IElevator, ElevatorRequestData> Elevators { get; set; }
        Queue<Request> UnprocessedCallRequests { get; set; }

        void AddElevator(IElevator elevator);
        void AddUnprocessedCallRequest(Request request);
        Task ProcessRequests();
        void ReceiveElevatorNotification(IElevator sourceElevator);
    }
}