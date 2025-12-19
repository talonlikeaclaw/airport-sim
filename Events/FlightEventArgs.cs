namespace AirportSimulator.Events;

using AirportSimulator.Models;

public class FlightEventArgs : EventArgs
{
    public IFlight Flight { get; }
    public DateTime Timestamp { get; }

    public FlightEventArgs(IFlight flight)
    {
        Flight = flight;
        Timestamp = DateTime.Now;
    }
}
