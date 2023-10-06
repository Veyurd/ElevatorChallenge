using ElevatorDomain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain.Validators
{
    public class RequestValidator : IValidator
    {

        private int MaxFloor { get; set; }

        private int BottomFloor { get; set; }

        private int MaxPersonsCountAcrossElevators { get; set; }

        public RequestValidator(int bottomFloor, int maxFloor, int maxPersonsCountAcrossElevators)
        {
            BottomFloor = bottomFloor;
            MaxFloor = maxFloor;
            MaxPersonsCountAcrossElevators = maxPersonsCountAcrossElevators;
        }

        public RequestValidator() { }


        public bool IsValid(object requestObject)
        {
            Request request=(Request)requestObject;
            bool isValid = true;

            // Request floors must be bounded by bottom and top floor.
            if (request.SourceFloor < BottomFloor || request.DestinationFloor < BottomFloor || request.SourceFloor > MaxFloor || request.DestinationFloor > MaxFloor)
                isValid = false;

            // Request woth the same source and destination floor makes no sense, won't be taken into consideration by the software.
            if (request.SourceFloor == request.DestinationFloor)
                isValid = false;


            if(request.NoPersons> MaxPersonsCountAcrossElevators)
                isValid = false;


            return isValid;

        }

    }
}
