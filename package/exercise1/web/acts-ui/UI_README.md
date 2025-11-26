# ACTS UI - Astronaut Career Tracking System

A modern, professional Angular 20 web application for managing astronaut career information. Built with Spartan-ng components and Tailwind CSS.

## 🚀 Quick Start

```bash
# Install dependencies
npm install

# Start development server
npm start

# Navigate to http://localhost:4200
```

## 📋 Features

### 🔍 Search Astronaut Duties
Search for any astronaut by name and view their complete career history including:
- Current and previous duty assignments
- Rank and title information
- Start and end dates for each assignment
- Status indicators (Current/Previous)

### 👥 People Management
Manage the astronaut database:
- View all people in the system
- Add new people to the database
- See retirement status
- Track career end dates

### ⭐ Duty Management
Create new astronaut duty assignments:
- Assign rank and title
- Set start dates
- Link duties to existing people

## 🏗️ Architecture

### Modern Angular 20 Features
- **Standalone Components**: Self-contained, tree-shakeable components
- **Signals API**: Reactive state management without RxJS boilerplate
- **Control Flow Syntax**: `@if` and `@for` for cleaner templates
- **Typed Forms**: Full type safety with reactive forms

### Component Structure
Each component includes:
```
component/
├── component.ts          # Component logic with signals
├── component.html        # Template with control flow syntax
├── component.scss        # Component-scoped styles
└── component.spec.ts     # Unit tests
```

### Services
- **ApiService**: Centralized HTTP communication
  - Type-safe requests/responses
  - Error handling
  - Observable-based async operations

### Models
- **TypeScript Interfaces**: Full type safety
  - BaseResponse wrapper
  - Domain models (Person, AstronautDuty)
  - Request/Response DTOs

## 🎨 UI Components (Spartan-ng)

The application uses Spartan-ng components for a professional, accessible interface:

- **HlmButton**: Primary action buttons with variants
- **HlmInput**: Form inputs with consistent styling
- **HlmLabel**: Accessible form labels
- **HlmCard**: Content containers
- **HlmBadge**: Status indicators

## 🎯 API Integration

### Base URL
```
http://localhost:5000
```

### Endpoints
| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/person` | Get all people |
| GET | `/person/{name}` | Get person by name |
| POST | `/person` | Create new person |
| GET | `/astronautduty/{name}` | Get duties by name |
| POST | `/astronautduty` | Create new duty |

## 📁 Project Structure

```
src/
├── app/
│   ├── models/
│   │   └── index.ts                    # TypeScript interfaces
│   ├── services/
│   │   └── api.service.ts              # API communication
│   ├── shared/
│   │   └── components/
│   │       ├── astronaut-search/       # Search component
│   │       ├── people-management/      # People component
│   │       └── duty-management/        # Duty component
│   ├── app.component.html              # Main layout
│   ├── app.component.scss              # Main styles
│   ├── app.ts                          # Root component
│   ├── app.config.ts                   # App configuration
│   └── app.routes.ts                   # Routes
├── styles.scss                         # Global styles
├── main.ts                             # Entry point
└── index.html                          # HTML template
```

## 🧪 Testing

### Run Tests
```bash
npm test
```

### Generate Coverage Report
```bash
npm test -- --code-coverage
```

### Test Coverage
Each component includes tests for:
- Component creation
- User interactions
- API calls
- Error handling
- Data formatting

## 🎨 Styling

### Tailwind CSS
- Utility-first CSS framework
- Responsive design
- Dark mode support (configured)
- Custom animations

### Color Scheme
- Primary: Blue (#1e3a8a)
- Secondary: Pink/Rose
- Neutral: Slate
- Status: Green (success), Red (error)

## 🔧 Development

### Add a New Component
```bash
ng generate component shared/components/my-component
```

Then update to standalone:
```typescript
@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [CommonModule, /* other imports */],
  templateUrl: './my-component.component.html',
  styleUrl: './my-component.component.scss'
})
export class MyComponentComponent { }
```

### Code Quality
- TypeScript strict mode enabled
- ESLint configured
- Prettier formatting
- Unit tests required

## 📊 State Management

Uses Angular Signals for reactive state:

```typescript
// Component state
activeTab = signal<'search' | 'people' | 'duties'>('search');
isLoading = signal(false);
errorMessage = signal('');

// Update state
this.activeTab.set('people');

// Read state in template
{{ activeTab() }}
```

## ⚡ Performance

- Standalone components (smaller bundle)
- OnPush change detection
- Lazy loading ready
- Efficient signal updates
- Minimal re-renders

## 🐛 Troubleshooting

### API Connection Failed
- Verify API is running on port 5000
- Check CORS headers
- Review browser console

### Styles Not Applied
- Clear node_modules: `rm -rf node_modules`
- Reinstall: `npm install`
- Rebuild: `npm run build`

### Component Not Found
- Check imports in parent component
- Verify standalone: true
- Check file paths

## 📚 Resources

- [Angular Documentation](https://angular.io)
- [Spartan-ng Documentation](https://www.spartan.ng)
- [Tailwind CSS](https://tailwindcss.com)
- [TypeScript Handbook](https://www.typescriptlang.org/docs)

## 📝 Notes

- All components are standalone
- Full type safety with TypeScript
- Modern control flow syntax (@if, @for)
- Comprehensive error handling
- Responsive design
- Accessible UI components

## 🎓 Technical Exercise

This is a technical exercise demonstrating:
- Modern Angular 20 best practices
- Component-based architecture
- Type-safe API integration
- Professional UI/UX
- Comprehensive testing
- Production-ready code quality

---

**Built with Angular 20 • Spartan-ng • Tailwind CSS**
