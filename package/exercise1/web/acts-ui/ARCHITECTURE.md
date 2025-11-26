# ACTS UI - Architecture Documentation

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ACTS UI Application                       │
│                    (Angular 20 + Spartan-ng)                │
└─────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────┴─────────┐
                    │                   │
            ┌───────▼────────┐  ┌──────▼──────────┐
            │  Root Component │  │  App Config    │
            │  (app.ts)       │  │  (app.config)  │
            └───────┬────────┘  └──────┬──────────┘
                    │                  │
        ┌───────────┼──────────────────┼──────────────┐
        │           │                  │              │
        │      ┌────▼─────┐       ┌────▼────┐   ┌───▼────┐
        │      │ Services │       │ Models  │   │ Routes │
        │      └────┬─────┘       └────┬────┘   └───┬────┘
        │           │                  │            │
        │      ┌────▼──────────┐       │            │
        │      │  API Service  │       │            │
        │      │  (HttpClient) │       │            │
        │      └────┬──────────┘       │            │
        │           │                  │            │
        │    ┌──────┴──────┐           │            │
        │    │             │           │            │
    ┌───▼────▼──┐  ┌──────▼────┐  ┌──▼──────────┐
    │ Components│  │ Interfaces│  │ Standalone │
    │ (Signals) │  │ (Models)  │  │ Setup      │
    └───────────┘  └───────────┘  └────────────┘
```

## Component Hierarchy

```
App (Root Component)
│
├── Header
│   └── ACTS Title & Branding
│
├── Navigation (Tab-based)
│   ├── Search Duties Tab
│   ├── People Tab
│   └── Add Duty Tab
│
├── Main Content Area
│   ├── AstronautSearchComponent (Search Tab)
│   │   ├── Search Input
│   │   ├── Duty Cards
│   │   └── Status Badges
│   │
│   ├── PeopleManagementComponent (People Tab)
│   │   ├── Add Person Form
│   │   ├── People List
│   │   └── Status Indicators
│   │
│   └── DutyManagementComponent (Duty Tab)
│       ├── Duty Form
│       │   ├── Name Input
│       │   ├── Rank Input
│       │   ├── Title Input
│       │   └── Date Picker
│       └── Submit/Reset Buttons
│
└── Footer
    └── Version & Credits
```

## Data Flow

```
┌──────────────────────────────────────────────────────────────┐
│                     User Interaction                         │
└──────────────────────┬───────────────────────────────────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │  Component (Signals) │
            │  - activeTab         │
            │  - isLoading         │
            │  - errorMessage      │
            └──────────┬───────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │   API Service        │
            │  (HttpClient)        │
            └──────────┬───────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │   Backend API        │
            │  (http://localhost   │
            │   :5000)             │
            └──────────┬───────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │  Response (JSON)     │
            │  BaseResponse<T>     │
            └──────────┬───────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │  Component Updates   │
            │  Signal Values       │
            └──────────┬───────────┘
                       │
                       ▼
            ┌──────────────────────┐
            │  Template Re-renders │
            │  (Change Detection)  │
            └──────────────────────┘
```

## Service Architecture

```
┌─────────────────────────────────────────┐
│          API Service                    │
│  (Centralized HTTP Communication)       │
└────────────────┬────────────────────────┘
                 │
    ┌────────────┼────────────┐
    │            │            │
    ▼            ▼            ▼
┌─────────┐ ┌──────────┐ ┌──────────┐
│ Person  │ │ Astronaut│ │ Response │
│ Queries │ │ Duty     │ │ Handling │
│         │ │ Queries  │ │          │
└────┬────┘ └────┬─────┘ └────┬─────┘
     │           │            │
     └───────────┼────────────┘
                 │
                 ▼
        ┌────────────────┐
        │  HttpClient    │
        │  (Observables) │
        └────────┬───────┘
                 │
                 ▼
        ┌────────────────┐
        │  Backend API   │
        │  :5000         │
        └────────────────┘
```

## State Management (Signals)

```
Component State (Signals)
│
├── UI State
│   ├── activeTab: signal<'search' | 'people' | 'duties'>
│   ├── isLoading: signal<boolean>
│   └── isCreating: signal<boolean>
│
├── Data State
│   ├── people: signal<Person[]>
│   ├── duties: signal<AstronautDuty[]>
│   └── form: signal<CreateAstronautDutyRequest>
│
└── Message State
    ├── errorMessage: signal<string>
    └── successMessage: signal<string>
```

## Type Safety Flow

```
┌──────────────────────────────────────┐
│  API Response (JSON)                 │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│  BaseResponse<T> Interface           │
│  {                                   │
│    success: boolean                  │
│    message: string                   │
│    responseCode: number              │
│    data?: T                          │
│  }                                   │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│  Typed Data Models                   │
│  - Person                            │
│  - AstronautDuty                     │
│  - CreatePersonRequest               │
│  - CreateAstronautDutyRequest        │
└──────────────┬───────────────────────┘
               │
               ▼
┌──────────────────────────────────────┐
│  Component (Fully Typed)             │
│  - No 'any' types                    │
│  - Full IntelliSense                 │
│  - Compile-time safety               │
└──────────────────────────────────────┘
```

## Component Lifecycle

```
Component Initialization
│
├── 1. Constructor
│   └── Inject dependencies (ApiService)
│
├── 2. ngOnInit()
│   └── Load initial data (if needed)
│
├── 3. Signal Creation
│   ├── Initialize state
│   ├── Set default values
│   └── Ready for user interaction
│
└── 4. Template Rendering
    ├── Bind signals to template
    ├── Display UI
    └── Listen for user events

User Interaction
│
├── 1. User Action (click, input, etc.)
│   └── Event handler triggered
│
├── 2. Component Method Called
│   ├── Update signals
│   ├── Call API service
│   └── Handle response
│
├── 3. Signal Update
│   ├── .set() new value
│   └── Trigger change detection
│
└── 4. Template Re-render
    └── Display updated data
```

## Testing Architecture

```
┌─────────────────────────────────────┐
│        Unit Tests (Jasmine)         │
└────────────────┬────────────────────┘
                 │
    ┌────────────┼────────────┐
    │            │            │
    ▼            ▼            ▼
┌────────┐ ┌──────────┐ ┌──────────┐
│Component│ │ Service  │ │ Mocking  │
│Tests   │ │ Tests    │ │ (Spies)  │
└────┬───┘ └────┬─────┘ └────┬─────┘
     │          │            │
     └──────────┼────────────┘
                │
                ▼
        ┌──────────────────┐
        │  Test Execution  │
        │  (Karma)         │
        └────────┬─────────┘
                 │
                 ▼
        ┌──────────────────┐
        │  Coverage Report │
        │  (Istanbul)      │
        └──────────────────┘
```

## API Integration Points

```
┌──────────────────────────────────────────────────────────┐
│                    API Service                           │
└──────────────────────────────────────────────────────────┘
│
├── Person Endpoints
│   ├── GET /person
│   │   └── getAllPeople()
│   │
│   ├── GET /person/{name}
│   │   └── getPersonByName(name)
│   │
│   └── POST /person
│       └── createPerson(name)
│
└── Astronaut Duty Endpoints
    ├── GET /astronautduty/{name}
    │   └── getAstronautDutiesByName(name)
    │
    └── POST /astronautduty
        └── createAstronautDuty(request)
```

## Error Handling Flow

```
API Call
│
├── Success Response
│   ├── Check response.success
│   ├── Extract response.data
│   ├── Update component signals
│   └── Display success message
│
└── Error Response
    ├── Catch error
    ├── Extract error message
    ├── Update errorMessage signal
    ├── Display error UI
    └── Log to console
```

## Build & Deployment

```
Source Code
│
├── TypeScript Compilation
│   └── .ts → .js
│
├── Template Processing
│   └── .html → compiled templates
│
├── Style Processing
│   ├── .scss → .css
│   └── Tailwind CSS generation
│
├── Bundling
│   └── Webpack bundling
│
├── Optimization
│   ├── Tree-shaking
│   ├── Minification
│   └── Code splitting
│
└── Output
    └── dist/ folder
        ├── main.js
        ├── styles.css
        └── assets/
```

## Performance Considerations

```
Optimization Strategies
│
├── Standalone Components
│   └── Smaller bundle size
│
├── Signals API
│   └── Efficient change detection
│
├── OnPush Strategy (Ready)
│   └── Minimal re-renders
│
├── Lazy Loading (Ready)
│   └── Route-based code splitting
│
└── Tree-shaking
    └── Unused code removal
```

---

**Architecture designed for scalability, maintainability, and performance.**
