namespace AirportSimulator.Core;

using AirportSimulator.Models;

public static class FlightFactory
{
    public static IFlight CreateFlight(string type, string flightNumber, string airline,
                                      string origin, string destination,
                                      DateTime scheduledDeparture,
                                      FlightStatus initialStatus = FlightStatus.Scheduled,
                                      params object[] additionalArgs)
    {
        return type.ToLower() switch
        {
            "domestic" => CreateDomestic(flightNumber, airline, origin, destination,
                                        scheduledDeparture, initialStatus, additionalArgs),
            "international" => CreateInternational(flightNumber, airline, origin, destination,
                                                  scheduledDeparture, initialStatus, additionalArgs),
            "cargo" => CreateCargo(flightNumber, airline, origin, destination,
                                  scheduledDeparture, initialStatus, additionalArgs),
            _ => throw new ArgumentException($"Unknown flight type: {type}")
        };
    }

    private static IFlight CreateDomestic(string flightNumber, string airline, string origin,
                                         string destination, DateTime scheduledDeparture,
                                         FlightStatus initialStatus, object[] args)
    {
        // Default capacity of 180 for domestic, or use provided value
        int capacity = args.Length > 0 && args[0] is int cap ? cap : 180;

        return new DomesticFlight(flightNumber, airline, origin, destination,
                                 scheduledDeparture, capacity, initialStatus);
    }

    private static IFlight CreateInternational(string flightNumber, string airline, string origin,
                                              string destination, DateTime scheduledDeparture,
                                              FlightStatus initialStatus, object[] args)
    {
        // Default capacity of 350 for international, or use provided value
        int capacity = args.Length > 0 && args[0] is int cap ? cap : 350;

        return new InternationalFlight(flightNumber, airline, origin, destination,
                                      scheduledDeparture, capacity, initialStatus);
    }

    private static IFlight CreateCargo(string flightNumber, string airline, string origin,
                                      string destination, DateTime scheduledDeparture,
                                      FlightStatus initialStatus, object[] args)
    {
        // Default max cargo weight of 50000kg, or use provided value
        double maxWeight = args.Length > 0 && args[0] is double weight ? weight : 50000.0;

        return new CargoFlight(flightNumber, airline, origin, destination,
                              scheduledDeparture, maxWeight, initialStatus);
    }
}
