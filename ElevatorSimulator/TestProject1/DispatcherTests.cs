using ElevatorDomain;

namespace TestProject1
{
    public class Tests
    {

        public StandardDispatcher Dispatcher { get; set; }

        public int NoFloors { get; set; }


        [SetUp]
        public void Setup()
        {
            NoFloors = 30;
             Dispatcher = new StandardDispatcher(NoFloors);
            
        }

        [Test]
        [TestCase("30")]
        public void CheckElevatorsAdded(int numberOfElevators)
        {
        

            for (int i=0;i<numberOfElevators; i++)
            {
                StandardElevator elevator = new StandardElevator("elevator"+i, 0, Dispatcher);
                Dispatcher.AddElevator(elevator);
            }
         

            Assert.AreEqual(numberOfElevators,Dispatcher.Elevators.Count);
        }


        [Test]
        public void CheckErroredCommand()
        {
            Request request = new Request(3, -4, 41);
            Assert.AreEqual(Dispatcher.UnprocessedCallRequests.Count(), 0);
        }

        
    }
}