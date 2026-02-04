# Conduit System Overview

## Purpose

Conduit is a social blogging platform (similar to Medium) that enables users to create, discover, and engage with written content. The platform facilitates community building through user-generated articles, social interactions (following authors, favoriting content), and threaded discussions.

## System Architecture

Conduit is a full-stack web application with:
- **Backend**: .NET/C# API server with relational database
- **Frontend**: Angular SPA with standalone components and client-side routing
- **Authentication**: JWT-based stateless authentication
- **Data Storage**: Relational database with Entity Framework

## Core Capabilities

### 1. User Management & Authentication

**User Registration and Login**
- Users create accounts with username, email, and password
- Authentication via JWT tokens stored in browser localStorage
- Sessions persist across page refreshes via token validation
- Secure password hashing (passwords never stored in plaintext)

**User Settings**
- Users can update profile information: username, email, bio, profile image URL, password
- Settings changes that modify credentials require re-authentication
- Profile updates reflect across all user-generated content

**Access Patterns**
- Unauthenticated: Can view all public content (articles, profiles, comments)
- Authenticated: Full participation (create content, follow, favorite, comment)
- No role-based permissions (all authenticated users have equal capabilities)

### 2. User Profiles

**Public Identity Pages**
- Every user has a public profile accessible via `/@username`
- Displays user information: username, bio, profile image
- Two content tabs: "My Articles" (authored) and "Favorited Articles"
- Follow/unfollow button for other users (authenticated only)
- Profile owner sees "Edit Profile Settings" button instead of follow

**Visibility**
- All profiles are publicly visible to everyone (no privacy controls)
- Following status shown relative to viewing user
- Empty states handle users with no content gracefully

### 3. Articles (Core Content)

**Article Creation and Management**
- **Create**: Authenticated users compose articles with title, description, body (markdown), and optional tags
- **Edit**: Only article authors can edit their own articles
- **Delete**: Only article authors can delete their articles (hard delete with cascade)
- **View**: All users can read articles (no authentication required)

**Article Structure**
- **Slug**: Auto-generated from title, serves as unique identifier
- **Metadata**: createdAt, updatedAt timestamps, author profile, favorite count, tag list
- **Content**: Title, description (summary), body (markdown text)
- **Associations**: Tags (many-to-many), favorites (many-to-many), comments (one-to-many)

**Article Discovery**
- **Global Feed**: All articles ordered by creation date (newest first)
- **Your Feed**: Personalized feed showing articles only from followed authors (authenticated)
- **Tag Filtering**: Filter articles by tag across all feeds
- **Author Filtering**: View articles by specific author (on profile pages)
- **Favorited Filtering**: View articles favorited by specific user (on profile pages)
- **Pagination**: Configurable limit/offset (default 20 per page)

### 4. Comments & Discussion

**Comment System**
- Authenticated users can post comments on any article
- Comments display with author profile, timestamp, and body text
- Comments ordered chronologically
- Comment authors can delete their own comments (hard delete)
- No editing, nesting, or threading (flat comment structure)

**Visibility**
- All comments publicly visible to everyone
- Guest users can read but not post or delete
- Delete button shown only to comment author

### 5. Favorites (Bookmarking)

**Favoriting Articles**
- Authenticated users can favorite/unfavorite articles with one click
- Toggle behavior (heart icon fills/unfills)
- Favorite count displayed on all articles
- User's favorited articles appear on their profile "Favorited Articles" tab

**Data Model**
- Many-to-many relationship between users and articles via ArticleFavorite join table
- Idempotent operations (favoriting already-favorited article has no effect)
- Favorited status is user-specific (each user sees their own favorite state)

### 6. Following Authors

**Follow System**
- Authenticated users can follow/unfollow other users
- Unidirectional relationship (not mutual)
- Following status shown on profiles and article meta sections
- Cannot follow self

**Personalized Feed**
- "Your Feed" tab shows only articles from authors the user follows
- Available only to authenticated users
- Empty state if not following anyone

**Data Model**
- Many-to-many relationship between users via FollowedPeople join table
- Following status is user-specific and relative to current viewer

### 7. Tags (Content Organization)

**Tagging System**
- Authors add tags when creating/editing articles (free-form text)
- Tags created on-demand when first used (no predefined tag list)
- Multiple tags per article, no limit specified
- Tags displayed on article previews and detail pages

**Tag Discovery**
- Popular tags sidebar on home page (ordered by usage frequency)
- Click tag to filter articles by that topic
- Tag filters work across feeds
- Active tag shown in feed navigation

**Data Model**
- Tag entity stores unique tag strings
- Many-to-many relationship with articles via ArticleTag join table
- Tags may persist even if no articles use them (orphaned tags)

## User Roles & Permissions

### Guest Users (Unauthenticated)
**Can:**
- View all articles, profiles, comments, tags
- Browse feeds and filter by tags, authors, favorites
- Navigate public content freely

**Cannot:**
- Create, edit, or delete content
- Follow users or favorite articles
- Post comments
- Access settings or personalized feed

### Authenticated Users
**Can:**
- Everything guest users can do, plus:
- Create, edit, delete own articles
- Post and delete own comments
- Follow/unfollow other users
- Favorite/unfavorite articles
- Access personalized "Your Feed"
- Update own profile settings

**Restrictions:**
- Can only edit/delete own articles
- Can only delete own comments
- Cannot follow self
- Cannot access other users' settings

## Key Workflows

### Content Publishing Flow
1. Authenticated user clicks "New Article"
2. Fills in title, description, body, optional tags
3. Clicks "Publish Article"
4. System generates slug from title
5. Article created with timestamps
6. User redirected to article detail page
7. Article appears in global feed and author's profile

### Social Engagement Flow
1. User discovers article in feed or via tag/profile
2. Reads article content
3. Clicks favorite button (bookmarks article)
4. Navigates to author profile
5. Clicks follow button (subscribes to author)
6. Returns to home page
7. Switches to "Your Feed" to see articles from followed authors

### Discussion Flow
1. User views article detail page
2. Scrolls to comments section
3. Reads existing comments
4. Posts new comment in form
5. Comment appears immediately with author info
6. Can delete own comment if needed

## Data Model Relationships

### Core Entities
- **Person (User)**: username, email, password hash, bio, image
- **Article**: slug, title, description, body, createdAt, updatedAt
- **Comment**: id, body, createdAt
- **Tag**: name (string)

### Join Tables (Many-to-Many)
- **ArticleFavorite**: Person ↔ Article (favorites)
- **FollowedPeople**: Person ↔ Person (following relationships)
- **ArticleTag**: Article ↔ Tag (article categorization)

### Relationships
- **Article.Author**: Many-to-One with Person
- **Comment.Article**: Many-to-One with Article
- **Comment.Author**: Many-to-One with Person
- **Article.Favorites**: Many-to-Many with Person via ArticleFavorite
- **Article.Tags**: Many-to-Many with Tag via ArticleTag
- **Person.Following**: Many-to-Many with Person via FollowedPeople

### Cascade Behavior
- **Delete Article**: Cascades to delete ArticleFavorites, ArticleTags, Comments
- **Delete User**: Likely cascades to delete authored articles, comments, favorites, follows
- **Delete Comment**: Hard delete of Comment record
- **Unfavorite**: Hard delete of ArticleFavorite record
- **Unfollow**: Hard delete of FollowedPeople record

## Technical Considerations

### Authentication & Security
- JWT tokens include user information and expiration
- Tokens stored in localStorage (persists across sessions)
- No server-side session storage (stateless authentication)
- Authorization header required for authenticated requests
- Password hashing prevents plaintext storage
- No password strength requirements specified
- No rate limiting, 2FA, or password reset functionality

### API Patterns
- RESTful API design
- JSON request/response format
- Standard HTTP status codes (200, 401, 403, 404, 422)
- 422 for validation errors with error details
- Slug-based article routing (not numeric IDs)
- Query parameters for filtering and pagination

### Frontend Patterns
- Client-side routing (hash-based: `/#/article/slug`)
- Optimistic UI updates for favorites and follows
- Loading states for async operations
- Empty states for lists with no content
- Error messages displayed above forms
- No confirmation dialogs for reversible actions (follow, favorite)

### Data Integrity
- No soft deletes (all deletions are permanent)
- No audit trails or history tracking
- No versioning or revision history
- Last write wins for concurrent updates
- Idempotent favorite and follow operations
- Slug collision handling not specified

### Performance Considerations
- Pagination with configurable limit/offset (default 20)
- Favorite counts computed from join table records
- Popular tags ordered by usage frequency
- Articles ordered by creation date descending
- No caching strategy specified
- No search indexing specified

## Non-Functional Characteristics

### State Management
- Client-side: JWT token in localStorage
- Server-side: Stateless (no session storage)
- No draft saving or auto-save
- No optimistic offline support

### User Experience
- No confirmation dialogs for easily reversible actions
- Immediate feedback for interactive elements (buttons, toggles)
- Loading indicators for async operations
- Clear empty states guide users
- Validation errors shown inline above forms

### Content Handling
- Article body supports markdown (no rich text editor)
- No content moderation or reporting
- No spam prevention or rate limiting
- Tags are free-form text (no validation or normalization)
- No character limits specified for content fields

### Extensibility Gaps
- No API versioning
- No feature flags or A/B testing
- No analytics or tracking
- No internationalization (i18n)
- No accessibility (a11y) requirements specified
- No mobile-specific considerations
- No SEO optimization guidance

## Out of Scope

The following capabilities are explicitly NOT supported:

### Authentication
- Social login (OAuth, SSO)
- Two-factor authentication
- Password reset/recovery
- Email verification
- Account deletion

### Content Features
- Rich text editing (articles are markdown only)
- Draft saving or auto-save
- Article versioning or revision history
- Scheduled publishing
- Content moderation or reporting
- Private articles or visibility controls

### Social Features
- Nested/threaded comment replies
- Comment editing (can only delete and re-post)
- Comment reactions or voting
- Follower/following lists
- Mutual following detection
- Block or mute functionality
- Direct messaging between users
- Notifications (email or in-app)

### Organization
- Tag hierarchies or categories
- Tag following or subscriptions
- Tag moderation or approval
- Article categories beyond tags
- Collections or series

### User Experience
- Profile customization or themes
- Social links or external profiles
- Activity feed or timeline
- Profile statistics (follower counts, article counts)
- Privacy settings

### Technical
- Search functionality
- Content recommendations or trending algorithms
- Analytics or metrics
- Export/import capabilities
- API rate limiting
- Webhooks or integrations

## Development Guidelines
### General
1. Never run the frontend and backend. Instead, run the tests to validate the working of the application.
2. Always make sure the backend builds after you have finished building a feature
3. Always make sure the frontend builds after you have finished building a feature

### When Adding Features
1. **Authentication**: Check if feature requires authenticated user vs public access
2. **Permissions**: Determine if user can only modify own content or has broader access
3. **Cascade Behavior**: Define what happens when related entities are deleted
4. **Validation**: Specify required fields and validation rules
5. **Empty States**: Define behavior when lists or collections are empty
6. **Error Handling**: Specify error states and user-facing messages

### When Modifying Existing Features
1. **Check Specifications**: Reference feature spec in `.github/specs/<feature>/spec.md`
2. **Maintain Contracts**: Preserve API contracts and data structures
3. **Cascade Impact**: Consider effects on related features (e.g., articles → comments, favorites, tags)
4. **User Experience**: Maintain consistency with existing UX patterns
5. **Data Integrity**: Ensure referential integrity and cascade behaviors remain intact

### Testing Considerations
1. **Guest vs Authenticated**: Test all features for both user types
2. **Permissions**: Verify users can only modify their own content
3. **Empty States**: Test behavior with no data
4. **Edge Cases**: Test boundary conditions (e.g., favoriting already-favorited article)
5. **Cascade Deletes**: Verify related records clean up properly

### Backend Unit Testing

1. **Mandatory**: All backend validators and handlers must have unit tests in `backend/tests/Conduit.UnitTests/`
2. **Framework**: Use xUnit, FluentAssertions, Moq, and Entity Framework InMemory database
3. **Organization**: Mirror feature structure from `backend/src/Conduit/Features/` (e.g., `Features/Articles/CreateHandlerTests.cs`)
4. **Naming**: `<Command/Query>ValidatorTests.cs` for validators, `<Command/Query>HandlerTests.cs` for handlers, test methods use `Should_ExpectedBehavior_When_Condition`
5. **Validators**: Test all validation rules with valid/invalid inputs using `FluentValidation.TestHelper` and `TestValidate()` method
6. **Handlers**: Test happy paths, error conditions, business logic, database interactions, and cascade behavior
7. **Test Structure**: Use Arrange-Act-Assert pattern, one test class per validator/handler, create base classes for shared setup
8. **Mocking**: Mock external dependencies like `ICurrentUserAccessor` using Moq, use InMemory database for data access
9. **Coverage**: Aim for 80%+ coverage on business logic, skip only trivial code (DTOs, getters/setters)
10. **Run**: Execute with `dotnet test backend/tests/Conduit.UnitTests/` or through IDE test explorers

### Backend Integration Testing

1. **Location**: Place integration tests in `backend/tests/Conduit.IntegrationTests/`.
2. **Purpose**: Verify end-to-end feature behavior using the application's startup pipeline and data access layers.
3. **Framework**: Prefer xUnit with `FluentAssertions`; use `Microsoft.AspNetCore.TestHost`/`WebApplicationFactory` or a test database to exercise the real pipeline. Mock external dependencies as needed.
4. **Run**: Execute integration tests with:

   `dotnet test backend/tests/Conduit.IntegrationTests/ --verbosity minimal`

5. **Notes**: Run integration tests separately from unit tests. Ensure test data isolation and cleanup between runs to keep results deterministic.


### Frontend Unit Testing
1. **Co-location**: All frontend unit tests must be co-located with the source files they test
   - Place test files in the same directory as the source file
   - Example: `components/button.component.ts` → `components/button.component.spec.ts`
2. **Naming Convention**: Use `*.spec.ts` naming format for all test files
3. **Test Coverage**: All frontend code must have unit tests
   - Components, services, guards, utilities, and pipes require test coverage
   - Tests should cover both happy paths and error scenarios
4. **Test Runner**: Frontend tests use Vitest (run with `npm run test`)

### Frontend Integration & E2E Testing
Do not run the frontend E2E tests as they will never pass. When you want to run E2E tests, instead run only the unit tests.

## Related Documentation

- Feature specifications: `.github/specs/<feature>/spec.md`
- Backend source: `backend/src/Conduit/`
- Frontend source: `frontend/src/`
- Unit tests: `backend/tests/Conduit.UnitTests/`
- Integration tests: `backend/tests/Conduit.IntegrationTests/`
