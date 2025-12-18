namespace AirportSimulator.Core;

using AirportSimulator.Models;
using AirportSimulator.Events;

public sealed class AirportController
{
    private static AirportController? _instance;
    private static readonly object _lock = new object();

    private readonly List<IFlight> _flights;
    private readonly List<string> _gates;
    private readonly Logger _logger;

    // Events for Observer pattern
    public event EventHandler<FlightEventArgs>? FlightScheduled;
    public event EventHandler<FlightEventArgs>? BoardingStarted;
    public event EventHandler<FlightEventArgs>? FlightDeparted;
    public event EventHandler<FlightEventArgs>? FlightArrived;
    public event EventHandler<FlightEventArgs>? FlightDelayed;
    public event EventHandler<FlightEventArgs>? GateChanged;

    private AirportController()
    {
        _flights = new List<IFlight>();
        _logger = Logger.Instance;

        // Initialize gates (A1-A10, B1-B10, C1-C10)
        _gates = new List<string>();
        foreach (char terminal in new[] { 'A', 'B', 'C' })
        {
            for (int i = 1; i <= 10; i++)
            {
                _gates.Add($"{terminal}{i}");
            }
        }

        _logger.Info("AirportController initialized with 30 gates");
    }

    public static AirportController Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new AirportController();
                }
            }
            return _instance;
        }
    }

    public void AddFlight(IFlight flight)
    {
        if (flight == null)
            throw new ArgumentNullException(nameof(flight));

        if (_flights.Any(f => f.FlightNumber == flight.FlightNumber))
            throw new InvalidOperationException($"Flight {flight.FlightNumber} already exists");

        _flights.Add(flight);
        _logger.Info($"Flight scheduled: {flight.FlightNumber} to {flight.Destination}");

        FlightScheduled?.Invoke(this, new FlightEventArgs(flight));
    }

    public void RemoveFlight(string flightNumber)
    {
        var flight = GetFlightByNumber(flightNumber);
        _flights.Remove(flight);
        _logger.Info($"Flight removed: {flightNumber}");
    }

    public void AssignGate(string flightNumber, string gate)
    {
        if (!_gates.Contains(gate))
            throw new ArgumentException($"Invalid gate: {gate}");

        var flight = GetFlightByNumber(flightNumber);

        // Check if gate is already in use
        if (_flights.Any(f => f.Gate == gate && f.Status != FlightStatus.Departed &&
                         f.Status != FlightStatus.Arrived && f.FlightNumber != flightNumber))
        {
            throw new InvalidOperationException($"Gate {gate} is already in use");
        }

        string? oldGate = flight.Gate;
        flight.Gate = gate;
        _logger.Info($"Gate assigned: {flightNumber} -> {gate}");

        if (oldGate != null)
        {
            GateChanged?.Invoke(this, new FlightEventArgs(flight));
        }
    }

    public void StartBoarding(string flightNumber)
    {
        var flight = GetFlightByNumber(flightNumber);
        flight.StartBoarding();
        _logger.Success($"Boarding started: {flightNumber} at Gate {flight.Gate}");

        BoardingStarted?.Invoke(this, new FlightEventArgs(flight));
    }

    public void DepartFlight(string flightNumber)
    {
        var flight = GetFlightByNumber(flightNumber);
        flight.Depart();
        _logger.Success($"Flight departed: {flightNumber}");

        FlightDeparted?.Invoke(this, new FlightEventArgs(flight));
    }

    public void ArriveFlight(string flightNumber)
    {
        var flight = GetFlightByNumber(flightNumber);
        flight.Arrive();
        _logger.Success($"Flight arrived: {flightNumber}");

        FlightArrived?.Invoke(this, new FlightEventArgs(flight));
    }

    public void DelayFlight(string flightNumber, int minutes)
    {
        var flight = GetFlightByNumber(flightNumber);
        flight.Delay(minutes);
        _logger.Warning($"Flight delayed: {flightNumber} by {minutes} minutes");

        FlightDelayed?.Invoke(this, new FlightEventArgs(flight));
    }

    public void CheckInPassenger(string flightNumber)
    {
        var flight = GetFlightByNumber(flightNumber);
        flight.CheckInPassenger();
        _logger.Info($"Passenger checked in: {flightNumber} " +
                    $"({flight.CheckedInPassengers}/{flight.PassengerCapacity})");
    }

    public void ProcessAllCheckIns(string flightNumber, int passengerCount)
    {
        var flight = GetFlightByNumber(flightNumber);

        _logger.Info($"Processing {passengerCount} check-ins for {flightNumber}...");

        // Use Parallel.For to simulate checking in multiple passengers
        Parallel.For(0, passengerCount, i =>
        {
            try
            {
                flight.CheckInPassenger();
            }
            catch (InvalidOperationException)
            {
                // Flight full, stop trying
            }
        });

        _logger.Success($"Check-in complete: {flight.CheckedInPassengers} passengers on {flightNumber}");
    }

    public IFlight GetFlightByNumber(string flightNumber)
    {
        var flight = _flights.FirstOrDefault(f => f.FlightNumber == flightNumber);

        if (flight == null)
            throw new ArgumentException($"Flight not found: {flightNumber}");

        return flight;
    }

    public IEnumerable<IFlight> GetAllFlights()
    {
        return _flights.AsReadOnly();
    }

    public IEnumerable<IFlight> GetDepartingFlights()
    {
        return _flights.Where(f => f.Status == FlightStatus.Scheduled ||
                                  f.Status == FlightStatus.Boarding ||
                                  f.Status == FlightStatus.Delayed)
                      .OrderBy(f => f.ScheduledDeparture);
    }

    public IEnumerable<IFlight> GetArrivingFlights()
    {
        return _flights.Where(f => f.Status == FlightStatus.InFlight ||
                                  f.Status == FlightStatus.Arrived)
                      .OrderBy(f => f.ScheduledArrival);
    }

    public IEnumerable<string> GetAvailableGates()
    {
        var usedGates = _flights.Where(f => f.Gate != null &&
                                           f.Status != FlightStatus.Departed &&
                                           f.Status != FlightStatus.Arrived)
                               .Select(f => f.Gate);

        return _gates.Except(usedGates);
    }
}
