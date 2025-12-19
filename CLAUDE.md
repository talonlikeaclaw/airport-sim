# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AirportSim is a C# .NET 10.0 console application demonstrating design patterns through an airport flight management simulator. The project simulates Montreal-Trudeau International Airport (YUL) and showcases Singleton, Factory, and Observer patterns.

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Clean build artifacts
dotnet clean

# Restore dependencies
dotnet restore
```

## Architecture and Design Patterns

### Namespace Structure

- `AirportSimulator.Models` - Flight domain models and interfaces
- `AirportSimulator.Core` - Core business logic (AirportController, FlightFactory, Logger)
- `AirportSimulator.Events` - Event system for notifications
- `AirportSimulator.UI` - Console user interface

### Design Patterns

**Singleton Pattern** (Thread-safe implementation):
- `AirportController` - Central flight management coordinator (Core/AirportController.cs:6-54)
- `Logger` - Application-wide logging (Core/Logger.cs:3-35)

Both use double-check locking pattern with null-coalescing assignment for thread safety.

**Factory Pattern**:
- `FlightFactory` - Creates different flight types (domestic, international, cargo) based on string type parameter (Core/FlightFactory.cs:5-55)
- Centralizes flight instantiation with default values for each type
- Supports optional capacity/weight parameters via `params object[]`

**Observer Pattern**:
- `AirportController` exposes 6 events using `EventHandler<FlightEventArgs>` delegate: FlightScheduled, BoardingStarted, FlightDeparted, FlightArrived, FlightDelayed, GateChanged (Core/AirportController.cs:16-21)
- Events raised using null-conditional invoke pattern: `FlightScheduled?.Invoke(this, new FlightEventArgs(flight))`
- `NotificationSystem` subscribes to all events in its constructor (Events/NotificationSystem.cs:14-20) and provides formatted console notifications with colors and beeps
- Uses `FlightEventArgs` to pass flight data with timestamp

### Core Components

**IFlight Interface** (Models/IFlight.cs):
- Base interface for all flight types
- Defines flight identity, route, timing, status, passenger capacity
- Common methods: CheckInPassenger, StartBoarding, Depart, Delay, Arrive

**Flight Implementations**:
- `DomesticFlight` - 2.5 hour default duration, typical capacity 180
- `InternationalFlight` - Similar structure, typical capacity 350
- `CargoFlight` - 5 hour default duration, uses cargo weight (kg) instead of passengers
  - Exposes `LoadCargo(double weightKg)` method for adding cargo
  - Properties: `CargoWeightKg` (current), `MaxCargoWeightKg` (capacity)
  - PassengerCapacity returns 0, CheckInPassenger throws InvalidOperationException

**AirportController** (Core/AirportController.cs):
- Manages 30 gates (A1-A10, B1-B10, C1-C10)
- Maintains flight collection
- Enforces gate assignment rules (no double-booking)
- Provides filtering methods: GetDepartingFlights (filters by Origin == "YUL"), GetArrivingFlights (filters by Destination == "YUL"), GetAvailableGates
- Passenger operations:
  - CheckInPassenger (line 137) - check in single passenger
  - ProcessAllCheckIns (line 145) - parallel batch check-in using Parallel.For
- Cargo operations:
  - LoadCargo (line 167) - load cargo onto a cargo flight
  - ProcessBatchCargoLoading (line 178) - parallel batch cargo loading using Parallel.For

**ConsoleUI** (UI/ConsoleUI.cs):
- Interactive menu-driven interface with 16 operations:
  - Flight Management (1-7): Schedule, Assign Gate, Board, Depart, Arrive, Delay, Cancel
  - Passenger Operations (8-9): Single check-in, Batch check-in (parallel)
  - Cargo Operations (15-16): Load cargo, Batch cargo loading (parallel)
  - Information Displays (10-14): All flights, Departures, Arrivals, Gates, Flight details
- Orchestrates all three design patterns: obtains Singleton instances (AirportController, Logger), creates NotificationSystem observer (line 17), and uses FlightFactory for flight creation
- NotificationSystem subscription happens automatically when ConsoleUI instantiates it (line 17), wiring up the entire Observer pattern
- Seeds 22 sample flights on startup (SeedSampleFlights at line 526):
  - 14 departures from Montreal (YUL): 4 domestic, 7 international, 3 cargo
  - 8 arrivals to Montreal (YUL): 3 domestic, 5 international
- Handles all user input and delegates to AirportController, remaining loosely coupled to business logic
- Cargo flight display enhancements:
  - DisplayFlightSelectionList (line 489) shows [CARGO] prefix and weight capacity
  - ViewFlightDetails (line 543-575) displays cargo load factor, remaining capacity, and color-coded status
  - Filters cargo flights (PassengerCapacity == 0) for cargo-specific operations

### State Management

**FlightStatus Enum**: Scheduled → Boarding → Departed/InFlight → Arrived (or Delayed/Cancelled)

State transitions are enforced in flight implementations:
- Cannot board without gate assignment
- Cannot depart unless boarding
- Cannot arrive unless departed
- Cannot delay after departure/arrival

### Threading

- Logger uses thread-safe Singleton initialization
- AirportController uses thread-safe Singleton initialization
- Parallel operations using Parallel.For:
  - ProcessAllCheckIns (AirportController.cs:152) - concurrent passenger check-in
  - ProcessBatchCargoLoading (AirportController.cs:190) - concurrent cargo loading
- Flight check-in counters (_checkedInPassengers) and cargo weight may need locking for true thread safety in production

## Key Implementation Details

- All flights auto-calculate arrival times based on flight type
- Gates are released when flights depart or arrive (Status check in GetAvailableGates)
- Logger writes to platform-specific AppData folder:
  - Windows: `%AppData%/AirportSimulator/airport_YYYY-MM-DD.log`
  - Linux: `~/.config/AirportSimulator/airport_YYYY-MM-DD.log`
  - macOS: `~/Library/Application Support/AirportSimulator/airport_YYYY-MM-DD.log`
- Time input uses TimeOnly parsing, schedules for next day if time has passed today
- FlightFactory returns IFlight interface, not concrete types
