namespace AirportSimulator.Events;

using AirportSimulator.Core;

public class NotificationSystem
{
    private readonly Logger _logger;

    public NotificationSystem()
    {
        _logger = Logger.Instance;

        // Subscribe to all AirportController events
        var controller = AirportController.Instance;
        controller.FlightScheduled += OnFlightScheduled;
        controller.BoardingStarted += OnBoardingStarted;
        controller.FlightDeparted += OnFlightDeparted;
        controller.FlightArrived += OnFlightArrived;
        controller.FlightDelayed += OnFlightDelayed;
        controller.GateChanged += OnGateChanged;

        _logger.Info("NotificationSystem initialized and subscribed to events");
    }

    private void OnFlightScheduled(object? sender, FlightEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n NEW FLIGHT SCHEDULED");
        Console.WriteLine($"   Flight: {e.Flight.FlightNumber} ({e.Flight.Airline})");
        Console.WriteLine($"   Route: {e.Flight.Origin} → {e.Flight.Destination}");
        Console.WriteLine($"   Departure: {e.Flight.ScheduledDeparture:MMM dd, HH:mm}");
        Console.ResetColor();
    }

    private void OnBoardingStarted(object? sender, FlightEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\nNOW BOARDING");
        Console.WriteLine($"   Flight {e.Flight.FlightNumber} to {e.Flight.Destination}");
        Console.WriteLine($"   Please proceed to Gate {e.Flight.Gate}");
        Console.WriteLine($"   Passengers: {e.Flight.CheckedInPassengers}/{e.Flight.PassengerCapacity}");
        Console.ResetColor();

        // System beep for attention
        Console.Beep();
    }

    private void OnFlightDeparted(object? sender, FlightEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nFLIGHT DEPARTED");
        Console.WriteLine($"   Flight {e.Flight.FlightNumber} has taken off");
        Console.WriteLine($"   Destination: {e.Flight.Destination}");
        Console.WriteLine($"   Departure Time: {e.Flight.ActualDeparture:HH:mm}");
        Console.WriteLine($"   Expected Arrival: {e.Flight.ScheduledArrival:HH:mm}");
        Console.ResetColor();
    }

    private void OnFlightArrived(object? sender, FlightEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nFLIGHT ARRIVED");
        Console.WriteLine($"   Flight {e.Flight.FlightNumber} from {e.Flight.Origin}");
        Console.WriteLine($"   Arrival Time: {e.Flight.ActualArrival:HH:mm}");

        // Calculate if early/late
        if (e.Flight.ActualArrival.HasValue)
        {
            var diff = e.Flight.ActualArrival.Value - e.Flight.ScheduledArrival;
            if (diff.TotalMinutes > 5)
            {
                Console.WriteLine($"   Status: Delayed by {diff.TotalMinutes:F0} minutes");
            }
            else if (diff.TotalMinutes < -5)
            {
                Console.WriteLine($"   Status: Early by {Math.Abs(diff.TotalMinutes):F0} minutes");
            }
            else
            {
                Console.WriteLine($"   Status: On time");
            }
        }
        Console.ResetColor();
    }

    private void OnFlightDelayed(object? sender, FlightEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n FLIGHT DELAYED");
        Console.WriteLine($"   Flight {e.Flight.FlightNumber} to {e.Flight.Destination}");
        Console.WriteLine($"   Original: {e.Flight.ScheduledDeparture:HH:mm}");
        Console.WriteLine($"   Status: DELAYED");
        Console.WriteLine($"   Please check departure boards for updates");
        Console.ResetColor();

        // Double beep for delays
        Console.Beep();
        Thread.Sleep(200);
        Console.Beep();
    }

    private void OnGateChanged(object? sender, FlightEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"\nGATE CHANGE");
        Console.WriteLine($"   Flight {e.Flight.FlightNumber} to {e.Flight.Destination}");
        Console.WriteLine($"   New Gate: {e.Flight.Gate}");
        Console.WriteLine($"   Please proceed to the new gate");
        Console.ResetColor();

        // Triple beep for gate changes
        for (int i = 0; i < 3; i++)
        {
            Console.Beep();
            Thread.Sleep(100);
        }
    }
}
