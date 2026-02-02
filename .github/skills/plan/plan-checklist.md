# Frontend Implementation Checklist

## Component Layer
- [ ] Create new components in `frontend/src/components/`
- [ ] Export new components in `frontend/src/components/index.js`
- [ ] Modify existing components as needed

## Pages & Routing
- [ ] Create new pages in `frontend/src/pages/`
- [ ] Add routes to `frontend/src/App.jsx` with appropriate wrapper (`Route` / `AuthRoute` / `GuestRoute`)
- [ ] Export new pages in `frontend/src/pages/index.jsx`
- [ ] Update the `Navbar` component if adding navigation items

## State Management
- [ ] Create query hooks for data fetching in `frontend/src/hooks/`
- [ ] Create mutation hooks for data modifications with optimistic updates and query invalidations
- [ ] Add or modify auth state in `frontend/src/hooks/useAuth.js` if needed
- [ ] Export new hooks in `frontend/src/hooks/index.js`

## Data Models
- [ ] Create or modify data transformation models in `frontend/src/models/`
- [ ] Export new models in `frontend/src/models/index.js`

## API Integration
- [ ] Update the MirageJS mock server (`frontend/src/server.js`) with newly consumed endpoints
- [ ] Add mock route handlers, models, factories, and seed data as needed

## Forms & Validation
- [ ] Implement forms with Formik and client-side validation
- [ ] Handle server validation errors (422 responses)

## UI/UX
- [ ] Add styles to `frontend/src/App.css`
- [ ] Implement loading, empty, and error states
- [ ] Add accessibility attributes (aria-labels, keyboard navigation)

## Configuration
- [ ] Update `package.json` and run `npm install` if adding dependencies

---

# Backend Implementation Checklist

## Domain Layer
- [ ] Create or modify entities in `backend/src/Conduit/Domain/`
- [ ] Update `ConduitContext.cs` with new `DbSet`s and entity configuration
- [ ] Define relationships and cascade delete behaviors in `OnModelCreating`

## Feature Implementation
- [ ] Create a feature folder and implement handlers in `backend/src/Conduit/Features/<FeatureName>/`
- [ ] Implement FluentValidation validators for all request models
- [ ] Create DTOs and response models
- [ ] Create controller endpoints and apply `[Authorize]` to protected routes

## Infrastructure
- [ ] Create new services in `backend/src/Conduit/Infrastructure/` if needed
- [ ] Register services in `ServicesExtensions.cs`
- [ ] Update `Program.cs` if middleware or auth changes are required

## Configuration
- [ ] Update `Conduit.csproj` and `Directory.Packages.props` if adding NuGet packages

## Validation
- [ ] Build solution: `dotnet build backend/Conduit.sln`
- [ ] Run all tests: `dotnet test backend/Conduit.sln`