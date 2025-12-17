namespace AirportSimulator.Models;

public class DomesticFlight : IFlight
{
    // Identity
    public string FlightNumber { get; }
    public string Airline { get; }

    // Route
    public string Origin { get; }
    public string Destination { get; }

    // Timing
    public DateTime ScheduledDeparture { get; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime ScheduledArrival { get; }
    public DateTime? ActualArrival { get; set; }

    // Status
    public FlightStatus Status { get; set; }
    public string? Gate { get; set; }

    // Passengers
    public int PassengerCapacity { get; }

    private int _checkedInPassengers;
    public int CheckedInPassengers => _checkedInPassengers;

    public DomesticFlight(string flightNumber, string airline, string origin, string destination, DateTime scheduledDeparture, int capacity)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
        {
            throw new ArgumentException("Flight number cannot be empty", nameof(flightNumber));
        }
        FlightNumber = flightNumber;
        Airline = airline;
        Origin = origin;
        Destination = destination;
        ScheduledDeparture = scheduledDeparture;
        ScheduledArrival = scheduledDeparture.AddHours(2.5);
        PassengerCapacity = capacity;
        Status = FlightStatus.Scheduled;
        _checkedInPassengers = 0;
    }

    // Methods
    public void CheckInPassenger()
    {
        if (_checkedInPassengers >= PassengerCapacity)
        {
            throw new InvalidOperationException("Flight is full");
        }
        if (Status == FlightStatus.Departed || Status == FlightStatus.InFlight)
        {
            throw new InvalidOperationException("Cannot check in, flight already departed");
        }

        _checkedInPassengers++;
    }

    public void StartBoarding()
    {
        if (Status != FlightStatus.Scheduled && Status != FlightStatus.Delayed)
        {
            throw new InvalidOperationException($"Cannot start boarding, status is {Status}");
        }
        if (string.IsNullOrWhiteSpace(Gate))
        {
            throw new InvalidOperationException("Cannot board, no gate assigned");
        }

        Status = FlightStatus.Boarding;
    }

    public void Depart()
    {
        if (Status != FlightStatus.Boarding)
        {
            throw new InvalidOperationException($"Cannot depart, status is {Status}");
        }

        ActualDeparture = DateTime.Now;
        Status = FlightStatus.Departed;
    }

    public void Delay(int minutes)
    {
        if (Status == FlightStatus.Departed || Status == FlightStatus.Arrived)
        {
            throw new InvalidOperationException("Cannot delay, flight already departed/arrived");
        }

        Status = FlightStatus.Delayed;
        // Shift both departure and arrival
        var newDeparture = ScheduledDeparture.AddMinutes(minutes);
        var newArrival = ScheduledArrival.AddMinutes(minutes);
    }

    public void Arrive()
    {
        if (Status != FlightStatus.Departed)
        {
            throw new InvalidOperationException($"Cannot arrive, status is {Status}");
        }

        ActualArrival = DateTime.Now;
        Status = FlightStatus.Arrived;
    }

    public string GetStatusDisplay()
    {
        return $"[{Status}] {FlightNumber} ({Airline}) | {Origin} → {Destination} | " +
               $"Gate: {Gate ?? "TBA"} | Pax: {CheckedInPassengers}/{PassengerCapacity} | " +
               $"Dep: {ScheduledDeparture:HH:mm}";
    }
}
