using ElevatorDomain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorDomain
{
    public class StandardDisplay : IDisplay
    {
  
        void IDisplay.Display(string message)
        {
            Console.WriteLine(message);
        }

        void IDisplay.Display(string message, ConsoleColor color)
        {
            Console.ForegroundColor= color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
