# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a technical exercise repository containing the **Stargate Astronaut Career Tracking System (ACTS)**. The system maintains records of people who have served as astronauts, tracking their duties, ranks, titles, and career dates.

**Structure:**
- `package/exercise1/api/` - ASP.NET Core Web API (Stargate API)
- `package/exercise1/web/acts-ui/` - Angular 20 web application

## Business Domain

### Core Entities

1. **Person** - Master list of all people in the system (not all are astronauts)
2. **AstronautDetail** - Current astronaut information for a person (one-to-one)
3. **AstronautDuty** - Historical list of astronaut assignments for a person (one-to-many)

### Business Rules

1. A Person is uniquely identified by their Name
2. A Person who has not had an astronaut assignment will not have Astronaut records
3. A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time
4. A Person's Current Duty will not have a Duty End Date
5. When a Person receives a new Astronaut Duty, the Previous Duty End Date is set to the day before the New Astronaut Duty Start Date
6. A Person is classified as 'Retired' when a Duty Title is 'RETIRED'
7. A Person's Career End Date is one day before the Retired Duty Start Date

## API (.NET 8 / C#)

### Project Location
`package/exercise1/api/`

### Architecture

**CQRS Pattern with MediatR:**
- Commands: `Business/Commands/` - Write operations (CreatePerson, CreateAstronautDuty)
- Queries: `Business/Queries/` - Read operations (GetPeople, GetPersonByName, GetAstronautDutiesByName)
- Controllers use IMediator to dispatch requests to handlers
- Pre-processors handle validation (e.g., CreateAstronautDutyPreProcessor)

**Data Layer:**
- Entity Framework Core with SQLite
- Dapper for raw SQL queries (used alongside EF Core in some handlers)
- Database: `starbase.db` (SQLite)
- Context: `StargateContext` in `Business/Data/`

### Common Commands

**Run the API:**
```bash
cd package/exercise1/api
dotnet run
```

**Build:**
```bash
cd package/exercise1/api
dotnet build
```

**Run Tests:**
```bash
cd package/exercise1/api
dotnet test
```

**Database Migrations:**
```bash
cd package/exercise1/api
dotnet ef migrations add MigrationName
dotnet ef database update
```

**Run single test:**
```bash
cd package/exercise1/api
dotnet test --filter FullyQualifiedName~Namespace.ClassName.MethodName
```

### API Endpoints

- `GET /Person` - Retrieve all people
- `GET /Person/{name}` - Retrieve person by name
- `POST /Person` - Add/update person by name (body: string name)
- `GET /AstronautDuty/{name}` - Retrieve astronaut duties by name
- `POST /AstronautDuty` - Add an astronaut duty (body: CreateAstronautDuty)

### Important Notes

- The CreateAstronautDuty command contains raw SQL queries vulnerable to SQL injection (lines 56, 60, 90 in CreateAstronautDuty.cs)
- AstronautDutyController.GetAstronautDutiesByName incorrectly uses GetPersonByName query instead of GetAstronautDutiesByName
- Mixed use of EF Core and Dapper in the same handlers

## Web (Angular 20)

### Project Location
`package/exercise1/web/acts-ui/`

### Common Commands

**Install dependencies:**
```bash
cd package/exercise1/web/acts-ui
npm install
```

**Start dev server:**
```bash
cd package/exercise1/web/acts-ui
ng serve
# App runs at http://localhost:4200/
```

**Build:**
```bash
cd package/exercise1/web/acts-ui
ng build
```

**Run tests:**
```bash
cd package/exercise1/web/acts-ui
ng test
```

**Generate component:**
```bash
cd package/exercise1/web/acts-ui
ng generate component component-name
```

**Run single test:**
```bash
cd package/exercise1/web/acts-ui
ng test --include='**/component-name.spec.ts'
```

### Configuration

- Angular CLI version: 20.2.0
- TypeScript: 5.9.2
- Testing: Karma + Jasmine
- Prettier configured with printWidth: 100, singleQuote: true

## Exercise Requirements

This codebase is designed as a technical exercise with the following tasks:

1. **Generate the database** - Source and storage location
2. **Enforce business rules** - Implement the 7 rules listed above
3. **Improve defensive coding** - Fix validation, error handling, security issues
4. **Add unit tests** - Target >50% code coverage, focus on most impactful methods
5. **Implement process logging** - Log exceptions and successes to the database

### Known Issues to Address

- SQL injection vulnerabilities in CreateAstronautDuty handler
- Wrong query used in AstronautDutyController.GetAstronautDutiesByName
- Missing error handling in some controllers
- Business rules not fully enforced
- No logging infrastructure
- No unit tests present
