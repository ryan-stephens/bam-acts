# ACTS UI - Setup Guide

## Overview

The Astronaut Career Tracking System (ACTS) UI is a modern Angular 20 application built with Spartan-ng components and Tailwind CSS. It provides a professional interface for managing astronaut career information.

## Prerequisites

- Node.js 18+ and npm
- Angular CLI 20.2.0
- API running on `http://localhost:5000`

## Installation

1. **Install dependencies:**
   ```bash
   npm install
   ```

2. **Verify Spartan-ng is installed:**
   ```bash
   npm list @spartan-ng/brain
   ```

## Running the Application

### Development Server

```bash
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

### Build for Production

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Running Tests

```bash
npm test
```

Tests use Karma and Jasmine. Coverage reports are generated in `coverage/`.

## Project Structure

```
src/
├── app/
│   ├── models/
│   │   └── index.ts                 # TypeScript interfaces
│   ├── services/
│   │   └── api.service.ts           # API communication service
│   ├── shared/
│   │   └── components/
│   │       ├── astronaut-search/    # Search astronaut duties
│   │       ├── people-management/   # Manage people records
│   │       └── duty-management/     # Add new duties
│   ├── app.component.html           # Main layout
│   ├── app.component.scss           # Main styles
│   ├── app.ts                       # Root component
│   ├── app.config.ts                # App configuration
│   └── app.routes.ts                # Route definitions
├── styles.scss                      # Global styles
└── main.ts                          # Entry point
```

## Features

### 1. Search Astronaut Duties
- Search for an astronaut by name
- Display complete duty history
- Show current and previous assignments
- Display rank, title, and dates

### 2. People Management
- View all people in the system
- Add new people
- Display retirement status
- Show career end dates

### 3. Duty Management
- Add new astronaut duties
- Specify rank, title, and start date
- Assign duties to existing people

## Architecture

### Components

All components follow Angular 20 best practices:

- **Standalone Components**: Each component is self-contained
- **Separate Files**: Template (.html), Component (.ts), Styles (.scss), and Tests (.spec.ts)
- **Control Flow Syntax**: Uses `@if` and `@for` for modern control flow
- **Signals API**: Uses Angular Signals for reactive state management

### Services

- **ApiService**: Centralized HTTP communication with the backend API
- Type-safe request/response handling
- Error handling and logging

### Models

- **TypeScript Interfaces**: Full type safety for API responses
- **BaseResponse**: Standard response wrapper
- **Domain Models**: Person, AstronautDuty, etc.

## Styling

- **Tailwind CSS**: Utility-first CSS framework
- **Spartan-ng Components**: Pre-built, accessible UI components
- **Custom Animations**: Smooth transitions and fade-in effects

## API Integration

The UI communicates with the ACTS API at `http://localhost:5000`:

### Endpoints Used

- `GET /person` - Get all people
- `GET /person/{name}` - Get person by name
- `POST /person` - Create new person
- `GET /astronautduty/{name}` - Get duties by astronaut name
- `POST /astronautduty` - Create new duty

## Error Handling

- User-friendly error messages
- Loading states for async operations
- Success notifications for completed actions
- Graceful fallbacks for API failures

## Testing

Each component includes comprehensive unit tests:

- Component creation
- User interactions
- API integration
- Error scenarios
- Data formatting

Run tests with coverage:
```bash
npm test -- --code-coverage
```

## Development Guidelines

### Adding a New Component

1. Create component files:
   ```bash
   ng generate component shared/components/my-component
   ```

2. Ensure standalone component setup
3. Use Spartan-ng components for UI
4. Add comprehensive tests
5. Follow existing patterns for services and models

### Code Quality

- Use strict TypeScript mode
- Follow Angular style guide
- Use Prettier for formatting
- Maintain >50% test coverage

## Troubleshooting

### API Connection Issues

- Verify API is running on `http://localhost:5000`
- Check CORS configuration on the API
- Review browser console for network errors

### Component Not Rendering

- Check component imports in parent
- Verify standalone component setup
- Review template syntax

### Styling Issues

- Clear node_modules and reinstall: `rm -rf node_modules && npm install`
- Rebuild Tailwind: `npm run build`

## Performance

- Lazy loading for routes (when implemented)
- OnPush change detection strategy
- Efficient signal updates
- Minimal re-renders

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## License

Technical Exercise - BAM

## Support

For issues or questions, refer to:
- [Angular Documentation](https://angular.io)
- [Spartan-ng Documentation](https://www.spartan.ng)
- [Tailwind CSS Documentation](https://tailwindcss.com)
