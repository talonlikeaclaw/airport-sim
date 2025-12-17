namespace AirportSimulator.Core;

using AirportSimulator.Models;

public static class FlightFactory
{
    public static IFlight CreateFlight(string type, string flightNumber, string airline,
                                      string origin, string destination,
                                      DateTime scheduledDeparture, params object[] additionalArgs)
    {
        return type.ToLower() switch
        {
            "domestic" => CreateDomestic(flightNumber, airline, origin, destination,
                                        scheduledDeparture, additionalArgs),
            "international" => CreateInternational(flightNumber, airline, origin, destination,
                                                  scheduledDeparture, additionalArgs),
            "cargo" => CreateCargo(flightNumber, airline, origin, destination,
                                  scheduledDeparture, additionalArgs),
            _ => throw new ArgumentException($"Unknown flight type: {type}")
        };
    }

    private static IFlight CreateDomestic(string flightNumber, string airline, string origin,
                                         string destination, DateTime scheduledDeparture,
                                         object[] args)
    {
        // Default capacity of 180 for domestic, or use provided value
        int capacity = args.Length > 0 && args[0] is int cap ? cap : 180;

        return new DomesticFlight(flightNumber, airline, origin, destination,
                                 scheduledDeparture, capacity);
    }

    private static IFlight CreateInternational(string flightNumber, string airline, string origin,
                                              string destination, DateTime scheduledDeparture,
                                              object[] args)
    {
        // Default capacity of 350 for international, or use provided value
        int capacity = args.Length > 0 && args[0] is int cap ? cap : 350;

        return new InternationalFlight(flightNumber, airline, origin, destination,
                                      scheduledDeparture, capacity);
    }

    private static IFlight CreateCargo(string flightNumber, string airline, string origin,
                                      string destination, DateTime scheduledDeparture,
                                      object[] args)
    {
        // Default max cargo weight of 50000kg, or use provided value
        double maxWeight = args.Length > 0 && args[0] is double weight ? weight : 50000.0;

        return new CargoFlight(flightNumber, airline, origin, destination,
                              scheduledDeparture, maxWeight);
    }
}
