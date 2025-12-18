namespace AirportSimulator.UI;

using AirportSimulator.Core;
using AirportSimulator.Models;
using AirportSimulator.Events;

public class ConsoleUI
{
    private readonly AirportController _controller;
    private readonly NotificationSystem _notifications;
    private readonly Logger _logger;
    private bool _running;

    public ConsoleUI()
    {
        _controller = AirportController.Instance;
        _notifications = new NotificationSystem();
        _logger = Logger.Instance;
        _running = false;
    }

    public void Run()
    {
        _running = true;
        _logger.Info("Airport Simulator started");

        Console.Clear();
        DisplayWelcome();

        // Add some sample flights for demo
        SeedSampleFlights();

        while (_running)
        {
            DisplayMenu();
            HandleInput();
        }

        _logger.Info("Airport Simulator closed");
    }

    private void DisplayWelcome()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════════════════╗");
        Console.WriteLine("║                                               ║");
        Console.WriteLine("║          AIRPORT SIMULATOR                    ║");
        Console.WriteLine("║                                               ║");
        Console.WriteLine("║          Pattern Practice Edition             ║");
        Console.WriteLine("║                                               ║");
        Console.WriteLine("╚═══════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    private void DisplayMenu()
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════╗");
        Console.WriteLine("║              MAIN MENU                    ║");
        Console.WriteLine("╠═══════════════════════════════════════════╣");
        Console.WriteLine("║  Flight Management                        ║");
        Console.WriteLine("║   1. Schedule New Flight                  ║");
        Console.WriteLine("║   2. Assign Gate                          ║");
        Console.WriteLine("║   3. Start Boarding                       ║");
        Console.WriteLine("║   4. Depart Flight                        ║");
        Console.WriteLine("║   5. Arrive Flight                        ║");
        Console.WriteLine("║   6. Delay Flight                         ║");
        Console.WriteLine("║   7. Cancel Flight                        ║");
        Console.WriteLine("║                                           ║");
        Console.WriteLine("║  Passenger Operations                     ║");
        Console.WriteLine("║   8. Check In Single Passenger            ║");
        Console.WriteLine("║   9. Process Batch Check-In (Parallel)    ║");
        Console.WriteLine("║                                           ║");
        Console.WriteLine("║  Cargo Operations                         ║");
        Console.WriteLine("║   15. Load Cargo                          ║");
        Console.WriteLine("║   16. Process Batch Cargo Load (Parallel) ║");
        Console.WriteLine("║                                           ║");
        Console.WriteLine("║  Information Displays                     ║");
        Console.WriteLine("║   10. View All Flights                    ║");
        Console.WriteLine("║   11. View Departures Board               ║");
        Console.WriteLine("║   12. View Arrivals Board                 ║");
        Console.WriteLine("║   13. View Available Gates                ║");
        Console.WriteLine("║   14. View Flight Details                 ║");
        Console.WriteLine("║                                           ║");
        Console.WriteLine("║   0. Exit                                 ║");
        Console.WriteLine("╚═══════════════════════════════════════════╝");
        Console.Write("\nSelect option: ");
    }

    private void HandleInput()
    {
        string? input = Console.ReadLine();
        Console.WriteLine();

        try
        {
            switch (input)
            {
                case "1": ScheduleNewFlight(); break;
                case "2": AssignGate(); break;
                case "3": StartBoarding(); break;
                case "4": DepartFlight(); break;
                case "5": ArriveFlight(); break;
                case "6": DelayFlight(); break;
                case "7": CancelFlight(); break;
                case "8": CheckInPassenger(); break;
                case "9": ProcessBatchCheckIn(); break;
                case "10": ViewAllFlights(); break;
                case "15": LoadCargo(); break;
                case "16": ProcessBatchCargoLoading(); break;
                case "11": ViewDeparturesBoard(); break;
                case "12": ViewArrivalsBoard(); break;
                case "13": ViewAvailableGates(); break;
                case "14": ViewFlightDetails(); break;
                case "0": Exit(); break;
                default:
                    Console.WriteLine("❌ Invalid option. Please try again.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.ResetColor();
            _logger.Error("Operation failed", ex);
        }

        if (_running)
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void ScheduleNewFlight()
    {
        Console.WriteLine("=== SCHEDULE NEW FLIGHT ===\n");

        Console.WriteLine("Flight Type:");
        Console.WriteLine("  1. Domestic");
        Console.WriteLine("  2. International");
        Console.WriteLine("  3. Cargo");
        Console.Write("Select: ");
        string? typeChoice = Console.ReadLine();

        string flightType = typeChoice switch
        {
            "1" => "domestic",
            "2" => "international",
            "3" => "cargo",
            _ => throw new ArgumentException("Invalid flight type")
        };

        Console.Write("Flight Number (e.g., AC123): ");
        string? flightNumber = Console.ReadLine();

        Console.Write("Airline: ");
        string? airline = Console.ReadLine();

        Console.Write("Origin (e.g., YYZ): ");
        string? origin = Console.ReadLine();

        Console.Write("Destination (e.g., YVR): ");
        string? destination = Console.ReadLine();

        Console.Write("Departure Time (HH:mm, e.g., 14:30): ");
        string? timeStr = Console.ReadLine();

        if (!TimeOnly.TryParse(timeStr, out TimeOnly departureTime))
        {
            throw new ArgumentException("Invalid time format");
        }

        DateTime scheduledDeparture = DateTime.Today.Add(departureTime.ToTimeSpan());
        if (scheduledDeparture < DateTime.Now)
        {
            scheduledDeparture = scheduledDeparture.AddDays(1);
        }

        IFlight flight;

        if (flightType == "cargo")
        {
            Console.Write("Max Cargo Weight (kg, default 50000): ");
            string? weightStr = Console.ReadLine();
            double maxWeight = string.IsNullOrWhiteSpace(weightStr) ? 50000 : double.Parse(weightStr);

            flight = FlightFactory.CreateFlight(flightType, flightNumber!, airline!,
                                              origin!, destination!, scheduledDeparture, FlightStatus.Scheduled, maxWeight);
        }
        else
        {
            Console.Write($"Passenger Capacity (default {(flightType == "domestic" ? "180" : "350")}): ");
            string? capacityStr = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(capacityStr))
            {
                flight = FlightFactory.CreateFlight(flightType, flightNumber!, airline!,
                                                   origin!, destination!, scheduledDeparture);
            }
            else
            {
                int capacity = int.Parse(capacityStr);
                flight = FlightFactory.CreateFlight(flightType, flightNumber!, airline!,
                                                   origin!, destination!, scheduledDeparture, FlightStatus.Scheduled, capacity);
            }
        }

        _controller.AddFlight(flight);
        Console.WriteLine("\nFlight scheduled successfully!");
    }

    private void AssignGate()
    {
        Console.WriteLine("=== ASSIGN GATE ===\n");

        var flightsWithoutGates = _controller.GetAllFlights().Where(f => f.Gate == null);
        DisplayFlightSelectionList(flightsWithoutGates, "Flights Without Gates");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        Console.WriteLine("\nAvailable Gates:");
        var availableGates = _controller.GetAvailableGates().ToList();

        for (int i = 0; i < Math.Min(15, availableGates.Count); i++)
        {
            Console.Write($"{availableGates[i]}  ");
            if ((i + 1) % 5 == 0) Console.WriteLine();
        }

        Console.Write("\n\nGate (e.g., A5): ");
        string? gate = Console.ReadLine();

        _controller.AssignGate(flightNumber!, gate!);
        Console.WriteLine($"\nGate {gate} assigned to {flightNumber}");
    }

    private void StartBoarding()
    {
        Console.WriteLine("=== START BOARDING ===\n");

        var readyForBoarding = _controller.GetAllFlights()
            .Where(f => f.Status == FlightStatus.Scheduled && f.Gate != null);
        DisplayFlightSelectionList(readyForBoarding, "Flights Ready for Boarding");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        _controller.StartBoarding(flightNumber!);
        Console.WriteLine("\nBoarding started");
    }

    private void DepartFlight()
    {
        Console.WriteLine("=== DEPART FLIGHT ===\n");

        var boardingFlights = _controller.GetAllFlights()
            .Where(f => f.Status == FlightStatus.Boarding);
        DisplayFlightSelectionList(boardingFlights, "Flights Currently Boarding");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        _controller.DepartFlight(flightNumber!);
        Console.WriteLine("\nFlight departed");
    }

    private void ArriveFlight()
    {
        Console.WriteLine("=== ARRIVE FLIGHT ===\n");

        var departedFlights = _controller.GetAllFlights()
            .Where(f => f.Status == FlightStatus.Departed || f.Status == FlightStatus.InFlight);
        DisplayFlightSelectionList(departedFlights, "Flights In-Flight");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        _controller.ArriveFlight(flightNumber!);
        Console.WriteLine("\nFlight arrived");
    }

    private void DelayFlight()
    {
        Console.WriteLine("=== DELAY FLIGHT ===\n");

        var activeFlights = _controller.GetAllFlights()
            .Where(f => f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding);
        DisplayFlightSelectionList(activeFlights, "Active Flights (Not Yet Departed)");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        Console.Write("Delay (minutes): ");
        string? minutesStr = Console.ReadLine();
        int minutes = int.Parse(minutesStr!);

        _controller.DelayFlight(flightNumber!, minutes);
        Console.WriteLine($"\nFlight delayed by {minutes} minutes");
    }

    private void CancelFlight()
    {
        Console.WriteLine("=== CANCEL FLIGHT ===\n");

        var cancellableFlights = _controller.GetAllFlights()
            .Where(f => f.Status != FlightStatus.Cancelled && f.Status != FlightStatus.Arrived);
        DisplayFlightSelectionList(cancellableFlights, "Flights That Can Be Cancelled");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        _controller.RemoveFlight(flightNumber!);
        Console.WriteLine("\nFlight cancelled");
    }

    private void CheckInPassenger()
    {
        Console.WriteLine("=== CHECK IN PASSENGER ===\n");

        var passengerFlights = _controller.GetAllFlights()
            .Where(f => f.PassengerCapacity > 0 &&
                       (f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding));
        DisplayFlightSelectionList(passengerFlights, "Passenger Flights Available for Check-In");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        _controller.CheckInPassenger(flightNumber!);

        var flight = _controller.GetFlightByNumber(flightNumber!);
        Console.WriteLine($"\nPassenger checked in");
        Console.WriteLine($"   Total: {flight.CheckedInPassengers}/{flight.PassengerCapacity}");
    }

    private void ProcessBatchCheckIn()
    {
        Console.WriteLine("=== BATCH CHECK-IN (PARALLEL PROCESSING) ===\n");

        var passengerFlights = _controller.GetAllFlights()
            .Where(f => f.PassengerCapacity > 0 &&
                       (f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding));
        DisplayFlightSelectionList(passengerFlights, "Passenger Flights Available for Check-In");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        Console.Write("Number of Passengers: ");
        string? countStr = Console.ReadLine();
        int count = int.Parse(countStr!);

        Console.WriteLine("\nProcessing check-ins in parallel...");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        _controller.ProcessAllCheckIns(flightNumber!, count);
        sw.Stop();

        var flight = _controller.GetFlightByNumber(flightNumber!);
        Console.WriteLine($"\nBatch check-in complete in {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Checked in: {flight.CheckedInPassengers}/{flight.PassengerCapacity}");
    }

    private void LoadCargo()
    {
        Console.WriteLine("=== LOAD CARGO ===\n");

        var cargoFlights = _controller.GetAllFlights()
            .Where(f => f.PassengerCapacity == 0 &&
                       (f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding));
        DisplayFlightSelectionList(cargoFlights, "Cargo Flights Available for Loading");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        Console.Write("Cargo Weight (kg): ");
        string? weightStr = Console.ReadLine();
        double weight = double.Parse(weightStr!);

        _controller.LoadCargo(flightNumber!, weight);

        var flight = _controller.GetFlightByNumber(flightNumber!);
        if (flight is CargoFlight cargo)
        {
            Console.WriteLine($"\nCargo loaded successfully");
            Console.WriteLine($"   Total: {cargo.CargoWeightKg:F1}/{cargo.MaxCargoWeightKg:F1} kg");
            double remaining = cargo.MaxCargoWeightKg - cargo.CargoWeightKg;
            Console.WriteLine($"   Remaining capacity: {remaining:F1} kg");
        }
    }

    private void ProcessBatchCargoLoading()
    {
        Console.WriteLine("=== BATCH CARGO LOADING (PARALLEL PROCESSING) ===\n");

        var cargoFlights = _controller.GetAllFlights()
            .Where(f => f.PassengerCapacity == 0 &&
                       (f.Status == FlightStatus.Scheduled || f.Status == FlightStatus.Boarding));
        DisplayFlightSelectionList(cargoFlights, "Cargo Flights Available for Loading");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        Console.Write("Total Cargo Weight (kg): ");
        string? totalWeightStr = Console.ReadLine();
        double totalWeight = double.Parse(totalWeightStr!);

        Console.Write("Number of Batches: ");
        string? batchesStr = Console.ReadLine();
        int batches = int.Parse(batchesStr!);

        Console.WriteLine("\nProcessing cargo loading in parallel...");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        _controller.ProcessBatchCargoLoading(flightNumber!, totalWeight, batches);
        sw.Stop();

        var flight = _controller.GetFlightByNumber(flightNumber!);
        if (flight is CargoFlight cargo)
        {
            Console.WriteLine($"\nBatch cargo loading complete in {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"   Loaded: {cargo.CargoWeightKg:F1}/{cargo.MaxCargoWeightKg:F1} kg");
            double remaining = cargo.MaxCargoWeightKg - cargo.CargoWeightKg;
            Console.WriteLine($"   Remaining capacity: {remaining:F1} kg");
        }
    }

    private void ViewAllFlights()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                          ALL FLIGHTS                                  ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════╝\n");

        var flights = _controller.GetAllFlights().ToList();

        if (!flights.Any())
        {
            Console.WriteLine("No flights scheduled.");
            return;
        }

        foreach (var flight in flights.OrderBy(f => f.ScheduledDeparture))
        {
            Console.WriteLine(flight.GetStatusDisplay());
        }

        Console.WriteLine($"\nTotal Flights: {flights.Count}");
    }

    private void ViewDeparturesBoard()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                        DEPARTURES BOARD                               ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════╝\n");

        var departures = _controller.GetDepartingFlights().ToList();

        if (!departures.Any())
        {
            Console.WriteLine("No departing flights.");
            return;
        }

        foreach (var flight in departures)
        {
            Console.WriteLine(flight.GetStatusDisplay());
        }

        Console.WriteLine($"\nTotal Departing: {departures.Count}");
    }

    private void ViewArrivalsBoard()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                         ARRIVALS BOARD                                ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════╝\n");

        var arrivals = _controller.GetArrivingFlights().ToList();

        if (!arrivals.Any())
        {
            Console.WriteLine("No arriving flights.");
            return;
        }

        foreach (var flight in arrivals)
        {
            Console.WriteLine(flight.GetStatusDisplay());
        }

        Console.WriteLine($"\nTotal Arriving: {arrivals.Count}");
    }

    private void ViewAvailableGates()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                        AVAILABLE GATES                                ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════╝\n");

        var availableGates = _controller.GetAvailableGates().ToList();

        Console.WriteLine("Available Gates:");
        for (int i = 0; i < availableGates.Count; i++)
        {
            Console.Write($"{availableGates[i]}  ");
            if ((i + 1) % 10 == 0) Console.WriteLine();
        }

        Console.WriteLine($"\n\nTotal Available: {availableGates.Count}/30");
    }

    private void ViewFlightDetails()
    {
        Console.WriteLine("=== FLIGHT DETAILS ===\n");

        var allFlights = _controller.GetAllFlights();
        DisplayFlightSelectionList(allFlights, "All Flights");

        Console.Write("Flight Number: ");
        string? flightNumber = Console.ReadLine();

        var flight = _controller.GetFlightByNumber(flightNumber!);

        Console.WriteLine($"\n╔═══════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  Flight {flight.FlightNumber} - {flight.Airline}");
        Console.WriteLine($"╚═══════════════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"\nRoute: {flight.Origin} → {flight.Destination}");
        Console.WriteLine($"Status: {flight.Status}");
        Console.WriteLine($"Gate: {flight.Gate ?? "Not Assigned"}");
        Console.WriteLine($"\nScheduled Departure: {flight.ScheduledDeparture:MMM dd, yyyy HH:mm}");
        Console.WriteLine($"Actual Departure: {(flight.ActualDeparture.HasValue ? flight.ActualDeparture.Value.ToString("MMM dd, yyyy HH:mm") : "N/A")}");
        Console.WriteLine($"\nScheduled Arrival: {flight.ScheduledArrival:MMM dd, yyyy HH:mm}");
        Console.WriteLine($"Actual Arrival: {(flight.ActualArrival.HasValue ? flight.ActualArrival.Value.ToString("MMM dd, yyyy HH:mm") : "N/A")}");

        if (flight.PassengerCapacity > 0)
        {
            Console.WriteLine($"\nPassengers: {flight.CheckedInPassengers}/{flight.PassengerCapacity}");
            double loadFactor = (double)flight.CheckedInPassengers / flight.PassengerCapacity * 100;
            Console.WriteLine($"Load Factor: {loadFactor:F1}%");
        }
        else if (flight is CargoFlight cargo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[CARGO FLIGHT - No Passengers]");
            Console.ResetColor();

            Console.WriteLine($"\nCargo Weight: {cargo.CargoWeightKg:F1}/{cargo.MaxCargoWeightKg:F1} kg");

            double cargoLoadFactor = (cargo.CargoWeightKg / cargo.MaxCargoWeightKg) * 100;
            Console.WriteLine($"Cargo Load Factor: {cargoLoadFactor:F1}%");

            double remainingCapacity = cargo.MaxCargoWeightKg - cargo.CargoWeightKg;
            Console.WriteLine($"Remaining Capacity: {remainingCapacity:F1} kg");

            if (cargoLoadFactor >= 90)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Status: ALMOST FULL");
                Console.ResetColor();
            }
            else if (cargoLoadFactor >= 50)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Status: PARTIALLY LOADED");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Status: READY FOR LOADING");
                Console.ResetColor();
            }
        }
    }

    private void Exit()
    {
        Console.WriteLine("Shutting down Airport Simulator...");
        _running = false;
    }

    private void DisplayFlightSelectionList(IEnumerable<IFlight> flights, string header = "Available Flights")
    {
        var flightList = flights.ToList();

        if (!flightList.Any())
        {
            Console.WriteLine("No flights available for this operation.\n");
            return;
        }

        Console.WriteLine($"--- {header} ---");
        foreach (var flight in flightList.OrderBy(f => f.ScheduledDeparture))
        {
            string route = $"{flight.Origin}-{flight.Destination}";
            string gate = flight.Gate ?? "No Gate";
            string time = flight.ScheduledDeparture.ToString("HH:mm");

            // Add type indicator and capacity info
            string flightInfo = "";
            if (flight.PassengerCapacity > 0)
            {
                // Passenger flight
                int available = flight.PassengerCapacity - flight.CheckedInPassengers;
                flightInfo = $" | Pax: {flight.CheckedInPassengers}/{flight.PassengerCapacity} ({available} avail)";
            }
            else if (flight is CargoFlight cargo)
            {
                // Cargo flight
                double availableWeight = cargo.MaxCargoWeightKg - cargo.CargoWeightKg;
                flightInfo = $" | [CARGO] {cargo.CargoWeightKg:F0}/{cargo.MaxCargoWeightKg:F0} kg ({availableWeight:F0} avail)";
            }

            Console.WriteLine($"  {flight.FlightNumber,-8} | {flight.Airline,-18} | {route,-8} | {gate,-7} | {time} | {flight.Status,-10}{flightInfo}");
        }
        Console.WriteLine();
    }

    private void SeedSampleFlights()
    {
        try
        {
            // Montreal (YUL) Airport - Sample Flights with Game-Like Timing
            // Times compressed for quick gameplay (seconds/minutes instead of hours)

            // IMMEDIATE ACTION NEEDED (boarding, ready to depart)
            var flight1 = FlightFactory.CreateFlight("domestic", "AC301", "Air Canada",
                "YUL", "YYZ", DateTime.Now.AddSeconds(30), FlightStatus.Boarding, 180);
            _controller.AddFlight(flight1);
            _controller.AssignGate("AC301", "A5");

            var flight6 = FlightFactory.CreateFlight("international", "BA094", "British Airways",
                "YUL", "LHR", DateTime.Now.AddSeconds(45), FlightStatus.Boarding, 275);
            _controller.AddFlight(flight6);
            _controller.AssignGate("BA094", "B5");

            var flight12 = FlightFactory.CreateFlight("cargo", "FX5234", "FedEx",
                "YUL", "MEM", DateTime.Now.AddMinutes(1), FlightStatus.Boarding, 65000.0);
            _controller.AddFlight(flight12);
            _controller.AssignGate("FX5234", "C9");

            // READY FOR BOARDING (scheduled, need to start boarding soon)
            var flight2 = FlightFactory.CreateFlight("domestic", "WS245", "WestJet",
                "YUL", "YVR", DateTime.Now.AddMinutes(2), FlightStatus.Scheduled, 168);
            _controller.AddFlight(flight2);
            _controller.AssignGate("WS245", "A7");

            var flight10 = FlightFactory.CreateFlight("international", "DL2134", "Delta Air Lines",
                "YUL", "ATL", DateTime.Now.AddMinutes(3), FlightStatus.Scheduled, 180);
            _controller.AddFlight(flight10);
            _controller.AssignGate("DL2134", "C6");

            var flight14 = FlightFactory.CreateFlight("cargo", "W8702", "Cargojet",
                "YUL", "YYZ", DateTime.Now.AddMinutes(2.5), FlightStatus.Scheduled, 45000.0);
            _controller.AddFlight(flight14);
            _controller.AssignGate("W8702", "C10");

            // NEED GATE ASSIGNMENT (scheduled soon, no gate!)
            var flight4 = FlightFactory.CreateFlight("domestic", "WS320", "WestJet",
                "YUL", "YYC", DateTime.Now.AddMinutes(4), FlightStatus.Scheduled, 180);
            _controller.AddFlight(flight4);

            var flight8 = FlightFactory.CreateFlight("international", "AM695", "Aeromexico",
                "YUL", "MEX", DateTime.Now.AddMinutes(5), FlightStatus.Scheduled, 189);
            _controller.AddFlight(flight8);

            var flight11 = FlightFactory.CreateFlight("international", "AA1896", "American Airlines",
                "YUL", "MIA", DateTime.Now.AddMinutes(6), FlightStatus.Scheduled, 172);
            _controller.AddFlight(flight11);

            var flight13 = FlightFactory.CreateFlight("cargo", "5X451", "UPS",
                "YUL", "SDF", DateTime.Now.AddMinutes(5.5), FlightStatus.Scheduled, 58000.0);
            _controller.AddFlight(flight13);

            // DELAYED (needs attention!)
            var flight7 = FlightFactory.CreateFlight("international", "LH476", "Lufthansa",
                "YUL", "FRA", DateTime.Now.AddSeconds(-30), FlightStatus.Delayed, 298);
            _controller.AddFlight(flight7);
            _controller.AssignGate("LH476", "B8");

            // FUTURE SCHEDULED (has time)
            var flight5 = FlightFactory.CreateFlight("international", "AF342", "Air France",
                "YUL", "CDG", DateTime.Now.AddMinutes(8), FlightStatus.Scheduled, 350);
            _controller.AddFlight(flight5);
            _controller.AssignGate("AF342", "B3");

            // ALREADY DEPARTED (in the past)
            var flight3 = FlightFactory.CreateFlight("domestic", "AC407", "Air Canada",
                "YUL", "YHZ", DateTime.Now.AddMinutes(-2), FlightStatus.Departed, 120);
            _controller.AddFlight(flight3);
            _controller.AssignGate("AC407", "A2");

            var flight9 = FlightFactory.CreateFlight("international", "UA1247", "United Airlines",
                "YUL", "EWR", DateTime.Now.AddMinutes(-3), FlightStatus.Departed, 150);
            _controller.AddFlight(flight9);
            _controller.AssignGate("UA1247", "C4");

            // ARRIVING SOON (inbound flights ready to land)
            var arrival4 = FlightFactory.CreateFlight("international", "AF343", "Air France",
                "CDG", "YUL", DateTime.Now.AddSeconds(30), FlightStatus.Departed, 350);
            _controller.AddFlight(arrival4);
            _controller.AssignGate("AF343", "B2");

            var arrival2 = FlightFactory.CreateFlight("domestic", "WS246", "WestJet",
                "YVR", "YUL", DateTime.Now.AddSeconds(45), FlightStatus.Departed, 168);
            _controller.AddFlight(arrival2);
            _controller.AssignGate("WS246", "A8");

            var arrival8 = FlightFactory.CreateFlight("international", "DL2135", "Delta Air Lines",
                "ATL", "YUL", DateTime.Now.AddMinutes(1), FlightStatus.Departed, 180);
            _controller.AddFlight(arrival8);
            _controller.AssignGate("DL2135", "C5");

            // ARRIVING LATER
            var arrival6 = FlightFactory.CreateFlight("international", "LH477", "Lufthansa",
                "FRA", "YUL", DateTime.Now.AddMinutes(3), FlightStatus.Departed, 298);
            _controller.AddFlight(arrival6);
            _controller.AssignGate("LH477", "B7");

            var arrival1 = FlightFactory.CreateFlight("domestic", "AC302", "Air Canada",
                "YYZ", "YUL", DateTime.Now.AddMinutes(4), FlightStatus.Scheduled, 180);
            _controller.AddFlight(arrival1);
            _controller.AssignGate("AC302", "A3");

            var arrival3 = FlightFactory.CreateFlight("domestic", "AC408", "Air Canada",
                "YYC", "YUL", DateTime.Now.AddMinutes(5), FlightStatus.Departed, 150);
            _controller.AddFlight(arrival3);
            _controller.AssignGate("AC408", "A4");

            // ALREADY ARRIVED (occupying gates - could remove to free space)
            var arrival5 = FlightFactory.CreateFlight("international", "BA095", "British Airways",
                "LHR", "YUL", DateTime.Now.AddMinutes(-1.5), FlightStatus.Arrived, 275);
            _controller.AddFlight(arrival5);
            _controller.AssignGate("BA095", "B4");

            var arrival7 = FlightFactory.CreateFlight("international", "UA1248", "United Airlines",
                "EWR", "YUL", DateTime.Now.AddMinutes(-2.5), FlightStatus.Arrived, 150);
            _controller.AddFlight(arrival7);
            _controller.AssignGate("UA1248", "C3");

            Console.WriteLine("\nSample flights loaded for Montreal (YUL) - 14 departures, 8 arrivals");
            Console.WriteLine("GAME MODE: Times compressed (seconds/minutes) for fast-paced gameplay!");
            Console.WriteLine("TIP: Manage boarding, departures, arrivals, and gate assignments quickly!\n");
        }
        catch
        {
            // Ignore if flights already exist
        }
    }
}
