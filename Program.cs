using AirportSimulator.Core;
using AirportSimulator.Events;
using AirportSimulator.UI;

class Program
{
    static void Main(string[] args)
    {
        var ui = new ConsoleUI();
        ui.Run();
    }
}
