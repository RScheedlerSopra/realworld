# Quick Start: Article Draft Management

**Feature**: 001-article-draft-management  
**Created**: 2026-03-01

## Overview

This guide helps developers quickly understand and implement the article draft management feature. Start here before diving into detailed specs.

## What This Feature Does

Allows authors to:
1. Save articles as **drafts** (private, unpublished)
2. View all their drafts on a dedicated **Drafts page**
3. Edit drafts multiple times before publishing
4. **Publish** drafts to make them publicly visible
5. Keep drafts completely private (404 for unauthorized access)

**Key constraint**: Published articles can NEVER be converted back to drafts (one-way transition only)

## Implementation Checklist

### Backend (Priority Order)

- [ ] **Database Migration**
  - [ ] Add `IsDraft` boolean field to Articles table (default: false)
  - [ ] Add composite index on `(IsDraft, AuthorId)`
  - [ ] Verify migration rollback works

- [ ] **Domain Model**
  - [ ] Add `IsDraft` property to Article entity
  - [ ] Add validation: prevent draft → published → draft transition

- [ ] **Query Methods**
  - [ ] Create `GetPublishedArticlesAsync()` - filters `IsDraft = false`
  - [ ] Create `GetAuthorDraftsAsync(authorId)` - filters `IsDraft = true AND AuthorId = authorId`
  - [ ] Update all existing article list queries to use `GetPublishedArticlesAsync()`

- [ ] **CQRS Handlers**
  - [ ] Modify `CreateArticleHandler`: Accept optional `isDraft` parameter
  - [ ] Modify `UpdateArticleHandler`: Accept optional `isDraft` parameter, validate one-way transition  
  - [ ] Create `ListDraftsHandler`: Returns author's drafts only
  - [ ] Create `GetArticleHandler`: Privacy check (404 if draft and not author)

- [ ] **API Controller**
  - [ ] Update `POST /api/articles` to accept `isDraft` field
  - [ ] Update `PUT /api/articles/:slug` to accept `isDraft` field
  - [ ] Add `GET /api/drafts` endpoint
  - [ ] Update `GET /api/articles/:slug` with draft privacy check

- [ ] **Unit Tests**
  - [ ] Test draft creation (isDraft = true)
  - [ ] Test published article creation (isDraft = false or omitted)
  - [ ] Test draft listing (only shows author's drafts)
  - [ ] Test publishing draft (isDraft: false on update)
  - [ ] Test attempted draft → published → draft transition (should fail)
  - [ ] Test unauthorized draft access (returns 404)
  - [ ] Test slug generation for drafts
  - [ ] Test slug stability when title changes

- [ ] **Integration Tests**
  - [ ] End-to-end: Create draft → edit → publish workflow
  - [ ] Privacy: Verify drafts not in feeds, profiles, tag lists
  - [ ] Privacy: Verify 404 on unauthorized draft access

### Frontend (Priority Order)

- [ ] **API Service**
  - [ ] Add `createDraft()` method
  - [ ] Add `updateDraft()` method
  - [ ] Add `publishDraft()` method
  - [ ] Add `listDrafts()` method
  - [ ] Update `createArticle()` to accept `isDraft` parameter
  - [ ] Update `updateArticle()` to accept `isDraft` parameter

- [ ] **Drafts Feature Module**
  - [ ] Create `drafts.routes.ts` with `/drafts` route
  - [ ] Create `DraftsListComponent` (displays author's drafts)
  - [ ] Create drafts list template (title, description, timestamps)
  - [ ] Add empty state ("No drafts yet" message)
  - [ ] Wire up click handlers to navigate to `/editor/:slug`

- [ ] **Article Editor**
  - [ ] Add "Save as draft" button
  - [ ] Add "Publish" button (when editing draft)
  - [ ] Update save logic to pass `isDraft` parameter to API
  - [ ] Show correct buttons based on article status
  - [ ] Handle validation errors

- [ ] **Navigation**
  - [ ] Add "Drafts" link to main navigation (authenticated users only)
  - [ ] Add "Drafts" link to user dropdown/profile menu

- [ ] **Unit Tests**
  - [ ] Test `DraftsListComponent` renders draft list
  - [ ] Test `DraftsListComponent` shows empty state
  - [ ] Test API service draft methods
  - [ ] Test editor save/publish button logic

## Quick Implementation Guide

### Step 1: Database (15 minutes)

```csharp
// In Migrations folder: AddIsDraftToArticles.cs
public partial class AddIsDraftToArticles : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Articles_IsDraft_AuthorId", table: "Articles");
        migrationBuilder.DropColumn(name: "IsDraft", table: "Articles");
    }
}
```

```csharp
// In Domain/Article.cs
public class Article
{
    // Existing properties...
    public bool IsDraft { get; set; }
}
```

### Step 2: Backend Query Methods (20 minutes)

```csharp
// New method in ArticlesController or repository
public async Task<List<Article>> GetPublishedArticlesAsync(...)
{
    return await _context.Articles
        .Where(a => a.IsDraft == false)
        // ... rest of query
        .ToListAsync();
}

public async Task<List<Article>> GetAuthorDraftsAsync(int authorId)
{
    return await _context.Articles
        .Where(a => a.IsDraft == true && a.AuthorId == authorId)
        .OrderByDescending(a => a.UpdatedAt)
        .ToListAsync();
}
```

**CRITICAL**: Update ALL existing list/feed queries to filter `IsDraft = false`:
- Global feed
- Personal feed
- Profile articles
- Tag-filtered lists

### Step 3: Backend Handlers (30 minutes)

```csharp
// Modify Features/Articles/Create.cs
public class Command : IRequest<ArticleEnvelope>
{
    // Existing properties...
    public bool IsDraft { get; set; } = false; // NEW
}

// In CreateHandler
var article = new Article
{
    // Existing mappings...
    IsDraft = request.IsDraft // NEW
};
```

```csharp
// New Features/Articles/ListDrafts.cs
public class Query : IRequest<ArticlesEnvelope>
{
    // No parameters needed - uses current user from context
}

public class Handler : IRequestHandler<Query, ArticlesEnvelope>
{
    public async Task<ArticlesEnvelope> Handle(Query request, ...)
    {
        var drafts = await GetAuthorDraftsAsync(_currentUser.Id);
        return new ArticlesEnvelope(drafts);
    }
}
```

### Step 4: Backend API Endpoints (15 minutes)

```csharp
// In Features/Articles/ArticlesController.cs

[HttpGet("drafts")]
[Authorize]
public async Task<ArticlesEnvelope> ListDrafts([FromQuery] Query query)
{
    return await _mediator.Send(query);
}

// Modify existing GET article to add privacy check
[HttpGet("{slug}")]
public async Task<ArticleEnvelope> Get(string slug)
{
    var article = await _mediator.Send(new Query { Slug = slug });
    
    // NEW: Privacy check for drafts
    if (article.IsDraft && article.Author.Id != _currentUser?.Id)
    {
        throw new NotFoundException(); // Returns 404
    }
    
    return article;
}
```

### Step 5: Frontend Drafts Page (30 minutes)

```typescript
// src/app/features/drafts/drafts-list/drafts-list.component.ts
@Component({
  selector: 'app-drafts-list',
  standalone: true,
  templateUrl: './drafts-list.component.html'
})
export class DraftsListComponent implements OnInit {
  drafts: Article[] = [];
  
  constructor(private articlesService: ArticlesService) {}
  
  ngOnInit() {
    this.articlesService.listDrafts().subscribe(
      response => this.drafts = response.articles
    );
  }
  
  editDraft(slug: string) {
    this.router.navigate(['/editor', slug]);
  }
}
```

```html
<!-- drafts-list.component.html -->
<div class="drafts-page">
  <h1>My Drafts</h1>
  
  @if (drafts.length === 0) {
    <p class="empty-state">You haven't created any drafts yet.</p>
  }
  
  @for (draft of drafts; track draft.slug) {
    <div class="draft-preview" (click)="editDraft(draft.slug)">
      <h2>{{ draft.title }}</h2>
      <p>{{ draft.description }}</p>
      <span class="date">Last updated: {{ draft.updatedAt | date }}</span>
    </div>
  }
</div>
```

### Step 6: Frontend Editor Updates (20 minutes)

```typescript
// article-edit.component.ts
saveAsDraft() {
  const article = { ...this.form.value, isDraft: true };
  
  if (this.isEditing) {
    this.articlesService.updateArticle(this.slug, article).subscribe(...);
  } else {
    this.articlesService.createArticle(article).subscribe(...);
  }
}

publish() {
  const article = { ...this.form.value, isDraft: false };
  this.articlesService.updateArticle(this.slug, article).subscribe(...);
}
```

```html
<!-- article-edit.component.html -->
<div class="editor-buttons">
  <button (click)="saveAsDraft()">Save as Draft</button>
  <button (click)="publish()">Publish</button>
</div>
```

## Testing Strategy

### Backend Tests (Priority)

1. **Unit tests for handlers** (MUST HAVE):
   - Create draft
   - List drafts (filtered by author)
   - Publish draft
   - Privacy enforcement (404 on unauthorized access)

2. **Integration tests** (RECOMMENDED):
   - Full workflow: create draft → edit → publish
   - Verify drafts don't appear in feeds/profiles

### Frontend Tests (Priority)

1. **Component tests** (MUST HAVE):
   - Drafts list renders correctly
   - Empty state shows when no drafts
   - Save/publish buttons work

## Common Pitfalls

### ❌ Don't Forget:

1. **Filter ALL existing queries**: Every query that lists articles must filter `IsDraft = false`
   - Global feed
   - Personal feed
   - Profile articles
   - Tag lists
   
2. **Privacy on GET single article**: Return 404 (not 403) for unauthorized draft access

3. **Validate one-way transition**: Prevent published articles from becoming drafts

4. **Slug stability**: Don't regenerate slug when title changes

5. **Index for performance**: Add composite index on `(IsDraft, AuthorId)`

### ✅ Remember:

- Drafts CAN have tags (they become public when draft is published)
- Drafts CANNOT have favorites or comments (enforce in logic)
- Empty drafts list is valid (show friendly empty state)
- isDraft field is NOT exposed in API responses (internal only)

## Time Estimates

- **Backend**: 2-3 hours (migration, handlers, tests)
- **Frontend**: 2-3 hours (UI components, routing, tests)
- **Testing**: 1-2 hours (unit + integration)
- **Total**: 5-8 hours for complete implementation

## Next Steps

After completing implementation:
1. Run all tests (backend + frontend)
2. Manual testing: Create draft → edit → publish workflow
3. Verify privacy: Try accessing draft as another user (should 404)
4. Verify feeds: Ensure drafts don't appear in public feeds
5. Code review focusing on privacy enforcement

## Reference

- **Detailed spec**: [spec.md](../spec.md)
- **Data model**: [data-model.md](../data-model.md)
- **API contracts**: [contracts/api-endpoints.md](../contracts/api-endpoints.md)
- **Implementation plan**: [plan.md](../plan.md)
