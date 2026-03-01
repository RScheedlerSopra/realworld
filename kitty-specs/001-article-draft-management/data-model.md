# Data Model: Article Draft Management

**Feature**: 001-article-draft-management  
**Created**: 2026-03-01

## Overview

This feature adds draft functionality to the existing Article entity. The data model changes are minimal - a single boolean field to distinguish between draft and published articles.

## Entities

### Article (Modified)

**Purpose**: Represents a blog article that can be either a draft (private, unpublished) or published (public)

**Fields** (new/modified only):

| Field | Type | Nullable | Default | Description |
|-------|------|----------|---------|-------------|
| IsDraft | bool | No | false | Indicates if article is a draft (true) or published (false) |

**Existing Fields** (unchanged):
- Id (int, primary key)
- Slug (string, unique)
- Title (string, required)
- Description (string, required)
- Body (string, required)
- CreatedAt (DateTime)
- UpdatedAt (DateTime)
- AuthorId (int, foreign key to User)
- Tags (many-to-many with Tag)
- Favorites (many-to-many with User)
- Comments (one-to-many with Comment)

**Validation Rules**:

1. **Slug uniqueness**: Slugs must be unique across ALL articles (both drafts and published)
   - Conflict resolution: Append unique identifier if slug collision detected
   - Applied at: Article creation (both draft and published)

2. **Required fields**: Title, Description, Body must be non-empty
   - Applied to: Both draft and published articles
   - Validation timing: On save (create/update) and on publish

3. **Author ownership**: Only the article's author can access/modify drafts
   - Enforced at: Query level (filter by AuthorId for drafts)
   - Authorization check: Controller/handler level

4. **One-way transition**: IsDraft can only transition from true → false (never false → true)
   - Enforced at: Application logic in Publish handler
   - Validation: Reject attempts to set IsDraft=true on published articles

**State Transitions**:

```
[New Article]
     │
     ├──> Save as Draft ──> IsDraft = true ──┐
     │                                        │
     └──> Publish ────────> IsDraft = false  │
                                              │
                                              ▼
                                    [Edit Draft] ──> Save as Draft (stays IsDraft = true)
                                              │
                                              ▼
                                         [Publish] ──> IsDraft = false (final state)
```

**Important**: No transition from published (IsDraft=false) back to draft (IsDraft=true) allowed.

## Relationships

**No changes to existing relationships**:
- Author (many-to-one: Article → User)
- Tags (many-to-many: Article ↔ Tag)
- Favorites (many-to-many: Article ↔ User)
- Comments (one-to-many: Article → Comment)

**Draft-specific constraints**:
1. **Favorites**: Drafts cannot be favorited (enforced in application logic)
2. **Comments**: Drafts cannot have comments (enforced in application logic)
3. **Tags**: Drafts CAN have tags (tags saved with draft, become public on publish)

## Indexes

**New Index**:
```sql
CREATE INDEX IX_Articles_IsDraft_AuthorId ON Articles (IsDraft, AuthorId)
```

**Rationale**: Optimizes the "list drafts for author" query (`WHERE IsDraft = true AND AuthorId = @authorId`)

**Existing Indexes** (unchanged):
- Primary key on Id
- Unique index on Slug
- Foreign key index on AuthorId

## Migration Strategy

### Migration: AddIsDraftToArticles

**Up**:
```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsDraft",
    table: "Articles",
    type: "boolean",
    nullable: false,
    defaultValue: false);

migrationBuilder.CreateIndex(
    name: "IX_Articles_IsDraft_AuthorId",
    table: "Articles",
    columns: new[] { "IsDraft", "AuthorId" });
```

**Down**:
```csharp
migrationBuilder.DropIndex(
    name: "IX_Articles_IsDraft_AuthorId",
    table: "Articles");

migrationBuilder.DropColumn(
    name: "IsDraft",
    table: "Articles");
```

**Data Impact**:
- All existing articles automatically get `IsDraft = false` (treated as published)
- No data loss or transformation needed
- Rollback safe: Dropping column only loses draft flag (all content preserved)

## Query Patterns

### Public Article Queries (list, feed, profile)

**Before**:
```csharp
var articles = await context.Articles
    .Include(a => a.Author)
    .ToListAsync();
```

**After**:
```csharp
var articles = await context.Articles
    .Where(a => a.IsDraft == false)  // Filter published only
    .Include(a => a.Author)
    .ToListAsync();
```

**Applied to**:
- Global feed
- Personal feed (followed authors)
- Profile article list
- Tag-filtered lists
- Article detail (guest/unauthorized users)

### Author Draft Queries

**New query pattern**:
```csharp
var drafts = await context.Articles
    .Where(a => a.IsDraft == true && a.AuthorId == currentUserId)
    .OrderByDescending(a => a.UpdatedAt)
    .ToListAsync();
```

**Applied to**:
- Drafts list page
- Draft detail (author only)

### Draft Privacy Enforcement

**Unauthorized draft access**:
```csharp
var article = await context.Articles
    .FirstOrDefaultAsync(a => a.Slug == slug);

if (article == null || (article.IsDraft && article.AuthorId != currentUserId))
{
    return NotFound(); // 404 for both non-existent and unauthorized drafts
}
```

## Data Volume Considerations

**Assumption**: Typical author has 5-10 drafts at any time  
**No pagination needed**: Drafts list displays all drafts (acceptable for expected volume)

**If volume increases in future**:
- Consider adding pagination when average drafts > 50 per author
- Index already supports efficient pagination (IsDraft + AuthorId + UpdatedAt)

## Edge Cases

1. **Slug collision**: Draft slug matches existing published article
   - Resolution: Append unique identifier to draft slug (e.g., `-draft-12345`)

2. **Concurrent publish**: Two users publish drafts with similar titles simultaneously
   - Resolution: Existing slug uniqueness constraint prevents collision (second publish fails with unique constraint error)

3. **Orphaned drafts**: Author deletes account
   - Resolution: Cascade delete (existing foreign key behavior)

4. **Large draft count**: Author with 100+ drafts
   - Current: All loaded (no pagination)
   - Future consideration: Add pagination if this becomes common
