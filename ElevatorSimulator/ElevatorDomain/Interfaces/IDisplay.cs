using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain.Interfaces
{
    public interface IDisplay
    {

        void Display(string message);

        void Display(string message, ConsoleColor color);
    }
}
