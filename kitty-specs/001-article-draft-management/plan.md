# Implementation Plan: Article Draft Management

**Branch**: `001-article-draft-management` | **Date**: 2026-03-01 | **Spec**: [spec.md](spec.md)

## Summary

Enable authors to create and manage article drafts privately before publishing. Authors can save articles as drafts, view them on a dedicated Drafts page, edit them multiple times, and publish when ready. Drafts remain completely private (404 for unauthorized access) and never appear in feeds, profiles, or public article lists.

**Technical Approach**: Add `isDraft` boolean field to existing Articles table, extend existing article endpoints with isDraft parameter, create separate query methods for public vs author-accessible articles, add new `/drafts` frontend route while reusing `/editor/:slug` for editing

## Technical Context

**Language/Version**: Backend: .NET 10 / Frontend: Angular (latest)  
**Primary Dependencies**: Backend: Entity Framework Core, MediatR / Frontend: Angular standalone components, RxJS  
**Storage**: Relational database (SQLite dev, configurable for production) with Entity Framework  
**Testing**: Backend: xUnit with FluentAssertions, Moq, EF InMemory / Frontend: Vitest  
**Target Platform**: Web application (cross-browser)
**Project Type**: Web (backend API + frontend SPA)  
**Performance Goals**: Drafts page load under 2 seconds  
**Constraints**: Privacy enforcement (404 for unauthorized draft access), no draft-to-published reverse transition  
**Scale/Scope**: Standard Conduit scale (multi-user blogging platform)

### Planning Decisions

1. **Database Schema**: Add `isDraft` boolean field to existing Articles table (single table approach)
2. **Migration Strategy**: Default all existing articles to `isDraft = false` (published)
3. **API Design**: Extend existing article endpoints with `isDraft` parameter for create/update operations
4. **Query Filtering**: Create separate query methods - one for public articles (isDraft = false), one for all articles (author's view)
5. **Frontend Routing**: New `/drafts` route for draft list, reuse existing `/editor/:slug` for editing both drafts and published articles
6. **URL Pattern**: Use `/editor/:slug` for all articles (draft status handled internally)
7. **Slug Generation**: Generate immediately when draft is first saved (enables stable URLs)
8. **Slug Stability**: Keep original slug unchanged when title is edited (maintains URL consistency)
9. **Pagination**: No pagination on drafts page (display all in single list, ordered by most recently updated)
10. **Unauthorized Access**: Return 404 Not Found (hides draft existence for privacy)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Languages and Frameworks**: ✅ PASS
- Backend: .NET 10 (compliant)
- Frontend: Angular (compliant)

**Testing Requirements**: ✅ PASS  
- Backend: xUnit with 80% line coverage target (100% for critical draft privacy logic)
- Frontend: Vitest for draft list component and editor modifications

**Performance and Scale**: ✅ PASS
- No specific performance constraints in constitution
- Feature targets: <2 seconds for drafts page load

**Deployment and Constraints**: ✅ PASS
- No specific deployment constraints
- Standard Conduit deployment process

**Conclusion**: All constitution checks pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```
kitty-specs/[###-feature]/
├── plan.md              # This file (/spec-kitty.plan command output)
├── research.md          # Phase 0 output (/spec-kitty.plan command)
├── data-model.md        # Phase 1 output (/spec-kitty.plan command)
├── quickstart.md        # Phase 1 output (/spec-kitty.plan command)
├── contracts/           # Phase 1 output (/spec-kitty.plan command)
└── tasks.md             # Phase 2 output (/spec-kitty.tasks command - NOT created by /spec-kitty.plan)
```

### Source Code (repository root)

```
backend/
├── src/
│   └── Conduit/
│       ├── Domain/
│       │   └── Article.cs              # Add IsDraft property
│       ├── Features/
│       │   └── Articles/
│       │       ├── Create.cs           # Modify to support isDraft parameter
│       │       ├── Update.cs           # Modify to support isDraft parameter
│       │       ├── List.cs             # Modify query to filter published only
│       │       ├── ListDrafts.cs       # NEW: List author's drafts
│       │       ├── Publish.cs          # NEW: Publish a draft
│       │       └── ArticlesController.cs # Add drafts endpoints
│       ├── Infrastructure/
│       │   └── ConduitContext.cs       # No changes (EF handles IsDraft)
│       └── Migrations/
│           └── AddIsDraftToArticles.cs # NEW: Migration for IsDraft field
└── tests/
    ├── Conduit.UnitTests/
    │   ├── Domain/
    │   │   └── ArticleTests.cs         # Test IsDraft behavior
    │   └── Features/
    │       └── Articles/
    │           ├── CreateHandlerTests.cs    # Test draft creation
    │           ├── ListDraftsHandlerTests.cs # NEW: Test draft listing
    │           └── PublishHandlerTests.cs    # NEW: Test publishing
    └── Conduit.IntegrationTests/
        └── Features/
            └── Articles/
                └── DraftWorkflowTests.cs # NEW: End-to-end draft workflow

frontend/
├── src/
│   └── app/
│       ├── features/
│       │   ├── article/
│       │   │   ├── article-edit/
│       │   │   │   └── article-edit.component.ts # Modify for isDraft support
│       │   │   └── services/
│       │   │       └── articles.service.ts       # Add draft methods
│       │   └── drafts/                          # NEW: Draft management feature
│       │       ├── drafts-list/
│       │       │   ├── drafts-list.component.ts
│       │       │   ├── drafts-list.component.html
│       │       │   └── drafts-list.component.spec.ts
│       │       └── drafts.routes.ts
│       └── core/
│           └── http/
│               └── api.service.ts               # Add draft endpoints
└── tests/
    └── [component test files co-located]
```

**Structure Decision**: Web application structure (Option 2). Backend uses vertical slice architecture (Features/), frontend uses Angular feature modules. Draft functionality integrated into existing Articles feature on backend, new Drafts feature on frontend.

## Phase 0: Research & Technical Decisions

All technical decisions resolved during planning interrogation. No research agents needed.

**Key Decisions**:
1. Database approach: Single table with `isDraft` boolean (simpler than separate tables, adequate privacy enforcement at query level)
2. API strategy: Extend existing endpoints (minimizes API surface area, backward compatible)
3. Query isolation: Separate methods for public vs author queries (clear separation of concerns, prevents accidental draft leaks)
4. Frontend routing: Dedicated `/drafts` list + unified `/editor/:slug` editing (consistent UX, reuses existing editor logic)
5. Privacy mechanism: 404 responses for unauthorized access (stronger privacy than 403, doesn't reveal draft existence)

## Phase 1: Design & Contracts

**Status**: ✅ Complete

### Data Model

**File**: [data-model.md](data-model.md)

**Key Changes**:
- Add `IsDraft` boolean field to Articles table (default: false)
- Add composite index on `(IsDraft, AuthorId)` for efficient draft queries
- Migration strategy: All existing articles default to published (`IsDraft = false`)
- Validation: Enforce one-way transition (draft → published only)
- Query patterns: Separate methods for public articles vs author's all articles

**Relationships**: No changes to existing relationships (Author, Tags, Favorites, Comments)

**Constraints**:
- Drafts cannot be favorited or commented (enforced in application logic)
- Drafts CAN have tags (tags become public when draft is published)
- Slug uniqueness enforced across ALL articles (both drafts and published)
- Slug remains stable when title is edited (no regeneration)

### API Contracts

**File**: [contracts/api-endpoints.md](contracts/api-endpoints.md)

**Modified Endpoints**:
1. `POST /api/articles` - Add optional `isDraft` field to request body
2. `PUT /api/articles/:slug` - Add optional `isDraft` field, validate one-way transition
3. `GET /api/articles/:slug` - Add privacy enforcement (404 for unauthorized draft access)
4. `GET /api/articles` - Auto-filter to published only (`isDraft = false`)

**New Endpoints**:
1. `GET /api/drafts` - List all drafts for authenticated user (ordered by updatedAt DESC)

**Response Schema**: No changes - `isDraft` field NOT exposed in API responses (internal only)

**Error Handling**:
- 404 for unauthorized draft access (hides existence)
- 422 for attempting to convert published article back to draft

**Backward Compatibility**: Fully backward compatible - existing clients continue to work unchanged

### Quick Start Guide

**File**: [quickstart.md](quickstart.md)

**Provides**:
- Feature overview and implementation checklist
- Step-by-step implementation guide with code samples
- Backend: Migration, domain model, handlers, controllers
- Frontend: API service, components, routing
- Testing strategy and time estimates
- Common pitfalls and best practices

### Agent Context Update

**No technology additions**: Feature uses existing stack (.NET, Entity Framework, Angular)  
**No agent context updates needed**: All technology already documented in copilot-instructions.md

---

## Planning Complete

**Status**: ✅ All planning artifacts generated

**Artifacts Created**:
- ✅ plan.md (this file)
- ✅ data-model.md
- ✅ contracts/api-endpoints.md
- ✅ quickstart.md

**Constitution Re-Check**: ✅ PASS - No violations, all requirements met

**Ready for**: Task generation (`/spec-kitty.tasks`)

**Next Steps**:
1. Review planning artifacts with team
2. Run `/spec-kitty.tasks` to generate work packages
3. Begin implementation following quickstart guide