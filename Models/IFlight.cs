namespace AirportSimulator.Models;

public interface IFlight
{
    // Identity
    string FlightNumber { get; }
    string Airline { get; }

    // Route
    string Origin { get; }
    string Destination { get; }

    // Timing
    DateTime ScheduledDeparture { get; }
    DateTime? ActualDeparture { get; set; }
    DateTime ScheduledArrival { get; }
    DateTime? ActualArrival { get; set; }

    // Status
    FlightStatus Status { get; set; }
    string? Gate { get; set; }

    // Passengers
    int PassengerCapacity { get; }
    int CheckedInPassengers { get; }

    // Methods
    void CheckInPassenger();
    void StartBoarding();
    void Depart();
    void Delay(int minutes);
    void Arrive();
    string GetStatusDisplay();
}

public enum FlightStatus
{
    Scheduled,
    Boarding,
    Departed,
    InFlight,
    Arrived,
    Delayed,
    Cancelled
}
