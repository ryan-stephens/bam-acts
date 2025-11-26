# ACTS UI - Quick Start Guide

## 🚀 Get Running in 3 Steps

### 1. Install Dependencies
```bash
npm install
```

### 2. Start Development Server
```bash
npm start
```

### 3. Open in Browser
Navigate to: `http://localhost:4200`

---

## ✨ What You'll See

### 🔍 Search Tab
Search for any astronaut by name and view their complete career history with all duty assignments.

### 👥 People Tab
View all people in the system and add new astronauts to the database.

### ⭐ Add Duty Tab
Create new duty assignments for astronauts with rank, title, and start date.

---

## 📋 Available Commands

```bash
# Development
npm start              # Start dev server (http://localhost:4200)
npm test              # Run unit tests
npm run build         # Build for production
npm run watch         # Watch mode build

# Code Quality
npm test -- --code-coverage    # Generate coverage report
```

---

## 🏗️ Project Structure

```
src/app/
├── models/                          # TypeScript interfaces
├── services/
│   └── api.service.ts              # API communication
├── shared/components/
│   ├── astronaut-search/           # Search component
│   ├── people-management/          # People component
│   └── duty-management/            # Duty component
├── app.component.html              # Main layout
└── app.ts                          # Root component
```

---

## 🔌 API Configuration

The app connects to: `http://localhost:5000`

**Ensure the API is running before starting the UI!**

---

## 🎨 Key Technologies

- **Angular 20** - Latest framework
- **Spartan-ng** - UI components
- **Tailwind CSS** - Styling
- **TypeScript** - Type safety
- **Signals** - State management

---

## 🧪 Running Tests

```bash
# Run all tests
npm test

# Run with coverage
npm test -- --code-coverage

# Run specific component
npm test -- --include='**/astronaut-search.component.spec.ts'
```

---

## 🐛 Troubleshooting

### Port 4200 Already in Use
```bash
ng serve --port 4201
```

### API Connection Failed
- Check API is running on port 5000
- Check CORS headers
- Review browser console for errors

### Styles Not Loading
```bash
rm -rf node_modules
npm install
npm start
```

---

## 📚 Documentation

- **SETUP.md** - Detailed setup instructions
- **UI_README.md** - Feature documentation
- **IMPLEMENTATION_SUMMARY.md** - Architecture overview

---

## 💡 Quick Tips

### Add a New Component
```bash
ng generate component shared/components/my-component
```

### Update Component State
```typescript
// In component
mySignal.set(newValue);

// In template
{{ mySignal() }}
```

### Add Spartan Component
```typescript
import { HlmButtonDirective } from '@spartan-ng/ui-button-helm';

// In imports array
imports: [HlmButtonDirective]

// In template
<button hlmButton>Click me</button>
```

---

## 🎯 Next Steps

1. ✅ Install dependencies
2. ✅ Start dev server
3. ✅ Explore the UI
4. ✅ Run tests
5. ✅ Review code structure
6. ✅ Start developing!

---

**Happy coding!** 🚀
