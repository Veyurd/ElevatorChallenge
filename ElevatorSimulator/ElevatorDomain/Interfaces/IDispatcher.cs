namespace ElevatorDomain.Interfaces
{
    public interface IDispatcher
    {
        List<IElevator> Elevators { get; set; }
        Queue<Request> UnprocessedCallRequests { get; set; }

        void AddElevator(IElevator elevator);
        void AddUnprocessedCallRequest(Request request);
        Task ProcessRequests();
    }
}