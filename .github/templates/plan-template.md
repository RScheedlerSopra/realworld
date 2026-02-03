<!--
Technical implementation plans live at: .github/specs/<kebab-case-name>/plan.md
This plan is written for senior developers to implement a feature.
Focus on architecture, edge cases, pitfalls, and dependencies - not implementation details.
-->

# Technical Implementation Plan: <feature-name>

## 1) Architecture Overview

### System Context
<Brief description of how this feature fits into the overall system and interacts with existing features>

### Architecture Diagram
```mermaid
graph TD
    A[User] --> B[Frontend Component]
    B --> C[API Endpoint]
    C --> D[Handler]
    D --> E[Database]
    
    %% Add more nodes and connections as needed
    %% Use appropriate Mermaid diagram types: graph, sequenceDiagram, classDiagram, etc.
```

### Key Design Decisions
- <Decision 1 with rationale - why this approach vs alternatives>
- <Decision 2 with rationale>

### Data Flow
<Describe the end-to-end data flow, including any transformations, validations, or side effects>

## 2) Backend Implementation

### Domain Layer
**Entities**: `backend/src/Conduit/Domain/<EntityName>.cs`
- New/modified entities and their key properties
- Relationships and navigation properties
- **Database Context**: Update `ConduitContext.cs` with DbSet and relationship configuration (cascade behavior, indexes)
- **Migration**: Generate migration after domain changes

### Features
**Location**: `backend/src/Conduit/Features/<FeatureName>/`

Implement the following operations:
- <Operation 1>: `[HttpMethod] api/<route>` - <brief description>
- <Operation 2>: `[HttpMethod] api/<route>` - <brief description>

Each operation needs:
- Command/Query with MediatR handler
- FluentValidation validator
- Response DTO(s)
- Controller endpoint (authorization: `[Authorize]` or public)

**Complex Business Logic** (if applicable):
<If there's complex logic, provide pseudocode or detailed explanation>

### Edge Cases & Pitfalls
- <Edge case 1 and how to handle>
- <Edge case 2 and how to handle>
- <Potential pitfall and how to avoid>

### Testing Requirements
**Unit Tests**: `backend/tests/Conduit.UnitTests/Features/<FeatureName>/`
- Validator tests for all validation rules
- Handler tests for business logic and database interactions
- Important edge cases: <list critical scenarios>

**Integration Tests**: `backend/tests/Conduit.IntegrationTests/Features/<FeatureName>/`
- Full request/response cycle
- Authentication/authorization
- Database state changes
- Critical edge cases: <list if any>

## 3) Frontend Implementation

### Components
**New Components**: `frontend/src/app/shared/components/` or `frontend/src/app/features/<feature>/components/`
- `<component-name>.component.ts` + `.component.test.ts` - <purpose>
- Standalone components with explicit imports

**Modified Components**:
- `<existing-component>.component.ts` - <what changes and why>

### Pages & Routing
**New Routes**: `frontend/src/app/features/<feature>/pages/`
- `<page-name>.component.ts` + `.component.test.ts`
- Route: `/<route>` (route type: public / authenticated / guest-only)
- Update `app.routes.ts` with new route configuration
- Add route guards if needed (e.g., `authGuard`, `guestGuard`)
- Update navigation components if adding new menu items

### State Management
**Services**: `frontend/src/app/core/services/` or `frontend/src/app/features/<feature>/services/`
- `<feature>.service.ts` + `.service.test.ts` - API integration and state management
- Use Angular signals for reactive state
- Use RxJS observables for async operations
- Provide services at appropriate level (root or component)

**Stores** (if needed): `frontend/src/app/core/stores/`
- `<feature>.store.ts` - State management for complex features
- Use signals and computed for derived state

### API Integration
**API Service**: Update or create services in `frontend/src/app/core/services/`
- Use Angular `HttpClient` for API calls
- Create typed interfaces for request/response models
- Handle authentication headers via interceptors
- Return observables for async operations

### UI/UX Considerations
- Form validation using Angular reactive forms and error handling (422 responses)
- Loading states, empty states, error states using signals
- Update `styles.css` or component-specific styles if needed
- Accessibility considerations (ARIA labels, keyboard navigation)
- Use `@rx-angular/template` for optimized rendering if needed

### Edge Cases & Pitfalls
- <Edge case 1 and how to handle in UI>
- <Edge case 2 and how to handle in UI>

### Testing Requirements
**Unit Tests**: Co-located `.test.ts` files
- Component rendering and behavior
- Service method logic
- User interactions and event handlers
- Important edge cases: <list critical scenarios>

## 4) Validation & Testing

**Backend**:
```bash
cd backend
dotnet build Conduit.sln                                          # Must succeed
dotnet test tests/Conduit.UnitTests/                              # All pass
dotnet test tests/Conduit.IntegrationTests/                       # All pass
```

**Frontend**:
```bash
cd frontend
npm run build                                                     # Must succeed
npm run test                                                      # All unit tests pass
npm run test:e2e                                                  # All E2E tests pass (optional)
```

**Checklist**:
- ✅ Backend builds (0 errors)
- ✅ All backend unit tests pass
- ✅ All backend integration tests pass
- ✅ Frontend builds (0 errors)
- ✅ All frontend unit tests pass
- ✅ Manual testing confirms feature works as specified
- ✅ No regressions in existing functionality

## 5) Implementation Notes

### Dependencies
- <List any dependencies on other features or external systems>
- <List any feature dependencies that depend on this>

### Critical Implementation Order (if applicable)
<Only include if there's a critical order to implementation steps, otherwise omit>

### References
- Feature spec: `.github/specs/<kebab-case-name>/spec.md`
- Related features: <list if applicable>
