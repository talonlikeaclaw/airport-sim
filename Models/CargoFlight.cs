namespace AirportSimulator.Models;

public class CargoFlight : IFlight
{
    public string FlightNumber { get; }
    public string Airline { get; }
    public string Origin { get; }
    public string Destination { get; }
    public DateTime ScheduledDeparture { get; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime ScheduledArrival { get; }
    public DateTime? ActualArrival { get; set; }
    public FlightStatus Status { get; set; }
    public string? Gate { get; set; }
    public int PassengerCapacity => 0; // No passengers on cargo
    public int CheckedInPassengers => 0; // No passengers
    public double CargoWeightKg { get; private set; }
    public double MaxCargoWeightKg { get; }

    private int _delayMinutes;
    public int DelayMinutes => _delayMinutes;
    public DateTime ExpectedDeparture => ScheduledDeparture.AddMinutes(_delayMinutes);
    public DateTime ExpectedArrival => ScheduledArrival.AddMinutes(_delayMinutes);

    public CargoFlight(string flightNumber, string airline, string origin,
                      string destination, DateTime scheduledDeparture, double maxCargoWeight)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
            throw new ArgumentException("Flight number cannot be empty", nameof(flightNumber));

        FlightNumber = flightNumber;
        Airline = airline;
        Origin = origin;
        Destination = destination;
        ScheduledDeparture = scheduledDeparture;
        ScheduledArrival = scheduledDeparture.AddHours(5); // Cargo ~5 hours
        MaxCargoWeightKg = maxCargoWeight;
        Status = FlightStatus.Scheduled;
        CargoWeightKg = 0;
        _delayMinutes = 0;
    }

    public void LoadCargo(double weightKg)
    {
        if (CargoWeightKg + weightKg > MaxCargoWeightKg)
            throw new InvalidOperationException("Exceeds max cargo weight");

        CargoWeightKg += weightKg;
    }

    public void CheckInPassenger()
    {
        throw new InvalidOperationException("Cargo flights don't carry passengers");
    }

    public void StartBoarding()
    {
        if (Status != FlightStatus.Scheduled && Status != FlightStatus.Delayed)
            throw new InvalidOperationException($"Cannot prepare - status is {Status}");

        Status = FlightStatus.Boarding; // Reusing this status for "ready"
    }

    public void Depart()
    {
        if (Status != FlightStatus.Boarding)
            throw new InvalidOperationException($"Cannot depart - status is {Status}");

        ActualDeparture = DateTime.Now;
        Status = FlightStatus.Departed;
    }

    public void Delay(int minutes)
    {
        if (Status == FlightStatus.Departed || Status == FlightStatus.Arrived)
            throw new InvalidOperationException("Cannot delay - flight already departed/arrived");

        Status = FlightStatus.Delayed;
        _delayMinutes += minutes;
    }

    public void Arrive()
    {
        if (Status != FlightStatus.Departed)
            throw new InvalidOperationException($"Cannot arrive - status is {Status}");

        ActualArrival = DateTime.Now;
        Status = FlightStatus.Arrived;
    }

    public string GetStatusDisplay()
    {
        return $"[{Status}] {FlightNumber} ({Airline}) | {Origin} → {Destination} | " +
               $"Gate: {Gate ?? "TBA"} | Cargo: {CargoWeightKg:F1}/{MaxCargoWeightKg:F1}kg | " +
               $"Dep: {ScheduledDeparture:HH:mm} | CARGO";
    }
}
