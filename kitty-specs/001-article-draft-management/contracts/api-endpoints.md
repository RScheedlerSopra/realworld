# API Contracts: Article Draft Management

**Feature**: 001-article-draft-management  
**Created**: 2026-03-01

## Overview

This feature extends existing article endpoints with draft support and adds new endpoints for draft-specific operations.

## Modified Endpoints

### POST /api/articles

**Purpose**: Create a new article (draft or published)

**Authentication**: Required (JWT)

**Change**: Add optional `isDraft` field to request body

**Request Body** (modified):
```json
{
  "article": {
    "title": "Article Title",
    "description": "Brief description",
    "body": "Article content in markdown",
    "tagList": ["tag1", "tag2"],
    "isDraft": false  // NEW: optional, defaults to false (published)
  }
}
```

**Response** (unchanged):
```json
{
  "article": {
    "slug": "article-title-abc123",
    "title": "Article Title",
    "description": "Brief description",
    "body": "Article content in markdown",
    "tagList": ["tag1", "tag2"],
    "createdAt": "2026-03-01T12:00:00.000Z",
    "updatedAt": "2026-03-01T12:00:00.000Z",
    "favorited": false,
    "favoritesCount": 0,
    "author": {
      "username": "johndoe",
      "bio": "...",
      "image": "...",
      "following": false
    }
  }
}
```

**Status Codes**:
- `200 OK`: Article created successfully (draft or published)
- `401 Unauthorized`: Not authenticated
- `422 Unprocessable Entity`: Validation errors (missing required fields)

**Behavior**:
- If `isDraft` not provided or `false`: Creates published article (existing behavior)
- If `isDraft` is `true`: Creates draft article (private to author)
- Slug generated immediately for both drafts and published articles

---

### PUT /api/articles/:slug

**Purpose**: Update an existing article (draft or published)

**Authentication**: Required (JWT, must be article author)

**Change**: Add optional `isDraft` field to request body

**Request Body** (modified):
```json
{
  "article": {
    "title": "Updated Title",        // optional
    "description": "Updated desc",   // optional
    "body": "Updated content",       // optional
    "tagList": ["tag1", "tag3"],     // optional
    "isDraft": true                  // NEW: optional, allows setting to true (save as draft) or false (publish)
  }
}
```

**Response** (unchanged):
```json
{
  "article": {
    "slug": "article-title-abc123",  // Slug never changes
    "title": "Updated Title",
    "description": "Updated desc",
    "body": "Updated content",
    "tagList": ["tag1", "tag3"],
    "createdAt": "2026-03-01T12:00:00.000Z",
    "updatedAt": "2026-03-01T12:30:00.000Z",
    "favorited": false,
    "favoritesCount": 0,
    "author": {
      "username": "johndoe",
      "bio": "...",
      "image": "...",
      "following": false
    }
  }
}
```

**Status Codes**:
- `200 OK`: Article updated successfully
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Not the article author
- `404 Not Found`: Article doesn't exist
- `422 Unprocessable Entity`: Validation errors

**Behavior**:
- If `isDraft` not provided: Keeps current draft status (no change)
- If `isDraft` is `true` on a published article: **ERROR** - Cannot convert published article back to draft
- If `isDraft` is `false` on a draft: Publishes the draft (one-way transition)
- Slug remains unchanged regardless of title changes

---

### GET /api/articles/:slug

**Purpose**: Get a single article

**Authentication**: Optional (different behavior for authenticated vs guest)

**Change**: Privacy enforcement for drafts

**Request**: No changes

**Response**: No changes to schema

**Status Codes**:
- `200 OK`: Article found and accessible
- `404 Not Found`: Article doesn't exist OR is a draft and requester is not the author

**Behavior** (CHANGED):
- Published article: Accessible to everyone (existing behavior)
- Draft article + requester is author: Returns draft (NEW)
- Draft article + requester is not author (or guest): Returns `404 Not Found` (NEW)

---

### GET /api/articles

**Purpose**: List all published articles (global feed)

**Authentication**: Optional

**Change**: Automatically filters out drafts

**Query Parameters** (unchanged):
```
?tag=tag1          // Filter by tag
&author=username   // Filter by author
&favorited=username // Filter by favorited by user
&limit=20          // Pagination limit
&offset=0          // Pagination offset
```

**Response**: No changes to schema

**Behavior** (CHANGED):
- Now filters to `isDraft = false` automatically
- Drafts never appear in this list (even if authored by requester)

---

## New Endpoints

### GET /api/drafts

**Purpose**: List all drafts for the authenticated user

**Authentication**: Required (JWT)

**Method**: `GET`

**Query Parameters**:
```
?limit=20   // Optional: pagination limit (though not used currently)
?offset=0   // Optional: pagination offset (though not used currently)
```

**Response**:
```json
{
  "articles": [
    {
      "slug": "draft-article-xyz789",
      "title": "Draft Article Title",
      "description": "Brief description",
      "body": "Article content in markdown",
      "tagList": ["tag1", "tag2"],
      "createdAt": "2026-03-01T10:00:00.000Z",
      "updatedAt": "2026-03-01T11:30:00.000Z",
      "favorited": false,
      "favoritesCount": 0,
      "author": {
        "username": "johndoe",
        "bio": "...",
        "image": "...",
        "following": false
      }
    }
  ],
  "articlesCount": 5
}
```

**Status Codes**:
- `200 OK`: Drafts list returned (may be empty)
- `401 Unauthorized`: Not authenticated

**Behavior**:
- Returns all drafts where `isDraft = true` AND `authorId = currentUser.Id`
- Ordered by `updatedAt` descending (most recently updated first)
- No pagination applied (all drafts returned)
- Returns empty list if author has no drafts

---

## Response Schema Changes

**No changes to response schemas**. The `article` object remains the same - the `isDraft` field is NOT exposed in API responses (it's an internal database field only).

**Rationale**: 
- Frontend doesn't need to track draft status from responses
- Draft status is implicit from the endpoint used:
  - `/api/drafts` → all articles are drafts
  - `/api/articles` → all articles are published
- Reduces API surface area and coupling

**Alternative considered**: Add `isDraft` to response schema but rejected because:
- Adds unnecessary field to all article responses
- Frontend already knows context (draft list vs public list)
- Creates coupling between frontend and database schema

## Error Responses

### Standard Error Format
```json
{
  "errors": {
    "body": ["error message 1", "error message 2"]
  }
}
```

### Draft-Specific Errors

**Attempting to convert published article to draft**:
```json
Status: 422 Unprocessable Entity
{
  "errors": {
    "body": ["Cannot convert published article back to draft"]
  }
}
```

**Accessing another user's draft**:
```json
Status: 404 Not Found
{
  "errors": {
    "body": ["Article not found"]
  }
}
```
Note: Returns 404 (not 403) to hide draft existence from unauthorized users

## Backward Compatibility

**Fully backward compatible**:
- Existing clients (not aware of drafts) continue to work unchanged
- `isDraft` field is optional on create/update (defaults to false)
- Existing published articles have `isDraft = false` after migration
- New `/api/drafts` endpoint is additive (doesn't break existing endpoints)

**Migration path for clients**:
1. Phase 1: Backend deployed with draft support, all clients still work (articles default to published)
2. Phase 2: Frontend updated to use draft endpoints when ready
3. No breaking changes, no client coordination needed
