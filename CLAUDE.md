# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

AirportSim is a C# .NET 10.0 console application demonstrating design patterns through an airport flight management simulator. The project showcases Singleton, Factory, and Observer patterns.

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
- `AirportController` exposes 6 events: FlightScheduled, BoardingStarted, FlightDeparted, FlightArrived, FlightDelayed, GateChanged
- `NotificationSystem` subscribes to all events and provides formatted console notifications with colors and beeps (Events/NotificationSystem.cs:5-118)
- Uses `FlightEventArgs` to pass flight data with timestamp

### Core Components

**IFlight Interface** (Models/IFlight.cs):
- Base interface for all flight types
- Defines flight identity, route, timing, status, passenger capacity
- Common methods: CheckInPassenger, StartBoarding, Depart, Delay, Arrive

**Flight Implementations**:
- `DomesticFlight` - 2.5 hour default duration, typical capacity 180
- `InternationalFlight` - Similar structure, typical capacity 350
- `CargoFlight` - Uses cargo weight instead of passengers

**AirportController** (Core/AirportController.cs):
- Manages 30 gates (A1-A10, B1-B10, C1-C10)
- Maintains flight collection
- Enforces gate assignment rules (no double-booking)
- Provides filtering methods: GetDepartingFlights, GetArrivingFlights, GetAvailableGates
- Uses Parallel.For for batch check-in operations (line 152)

**ConsoleUI** (UI/ConsoleUI.cs):
- Interactive menu-driven interface with 14 operations
- Seeds 3 sample flights on startup (SeedSampleFlights at line 453)
- Handles all user input and delegates to AirportController

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
- ProcessAllCheckIns uses Parallel.For for concurrent passenger check-in (AirportController.cs:152)
- Flight check-in counters (_checkedInPassengers) may need locking for true thread safety in production

## Key Implementation Details

- All flights auto-calculate arrival times based on flight type
- Gates are released when flights depart or arrive (Status check in GetAvailableGates)
- Logger writes to AppData folder: `%AppData%/AirportSimulator/airport_YYYY-MM-DD.log`
- Time input uses TimeOnly parsing, schedules for next day if time has passed today
- FlightFactory returns IFlight interface, not concrete types
