<!--
Feature specs live at: .github/specs/<kebab-case-name>/spec.md
Write in brief sentences. Prefer specific, testable wording.
-->

# Feature: Article Drafts
Kebab-case: `article-drafts`

## 1) Goal (Why)

### Problem statement
Users cannot save incomplete articles for later completion, forcing them to either publish unfinished content or lose their work if they navigate away.

### Who feels it most
Content creators who need multiple writing sessions to complete articles, want to experiment with ideas before publishing, or need time to review and refine their work.

### Desired outcome
Users can save articles in draft status, return to edit them multiple times, and publish only when ready, without fear of losing work or publishing prematurely.

### Success criteria
- Users can save articles as drafts without publishing
- Drafts persist in user's profile until deleted or published
- Users can edit drafts multiple times before publishing
- Drafts remain private (visible only to author)
- Publishing a draft converts it to a regular published article

### Non-goals / out of scope
- Auto-save functionality (user must explicitly click Save Draft)
- Converting published articles back to draft status
- Sharing draft links with others
- Collaborative editing or draft comments
- Draft versioning or revision history
- Scheduled publishing of drafts
- Draft expiration or cleanup

## 2) Users & Access (What)

### Target users/personas
- Authenticated content creators who want to work on articles over time
- Writers who need to review and refine before publishing
- Users experimenting with article ideas

### Eligibility
- **Create/Edit/Delete Drafts**: Authenticated users only
- **View Drafts**: Only the draft author
- **Publish Drafts**: Only the draft author

### Visibility
- Drafts are completely private and visible only to the author
- Guest users cannot see or interact with drafts
- Other authenticated users cannot see someone else's drafts
- Draft URLs are not accessible to anyone except the author

### Where it lives
- Profile page: "My Drafts" tab alongside "My Articles" and "Favorited Articles"
- Editor page: Same editor used for creating and editing articles
- Navigation: "New Article" button opens editor that can save as draft or publish

### How to reach it
- Primary entry point: "My Drafts" tab on user's own profile page
- Secondary entry points:
  - "New Article" button saves as draft or publishes directly
  - Edit button on draft preview in drafts list
  - Browser prompt when navigating away during draft editing

## 3) User Experience & Flow (What)

### Happy path
1. User clicks "New Article" in navigation
2. User enters title and optionally description, body, tags
3. User clicks "Save Draft" button
4. System saves article as draft with author info and timestamp
5. User redirected to their drafts list
6. Draft appears in "My Drafts" tab on profile
7. User later opens draft, completes content, clicks "Publish"
8. Draft becomes published article and appears in global feed

### Key screens/states
- Editor page: Form with title, description, body, tags inputs, two buttons ("Save Draft" and "Publish")
- My Drafts tab: List of draft previews with edit and delete buttons, ordered by last updated
- Draft preview: Shows title, description snippet, last updated timestamp, tags
- Navigation prompt: Browser confirmation dialog when leaving editor without saving

### Empty state
- My Drafts tab with no drafts: "No drafts yet. Start writing to save your work for later."
- Editor is blank when creating new article
- Other users' profiles show only "My Articles" and "Favorited Articles" tabs (no drafts tab)

### Loading state
- Drafts list shows loading indicator while fetching
- Editor shows loading indicator when opening existing draft
- Save Draft and Publish buttons show loading state during save operation

### Error state
- Validation errors appear above form if title is empty when saving draft
- Network errors show generic error message on save failure
- 404 error if draft ID not found when editing
- 403 error if non-author tries to access draft URL

### Cancellation/escape
- Browser prompt appears when user tries to navigate away from editor with unsaved changes
- User can choose to stay and save or leave and discard changes
- Clicking "Leave" discards all unsaved changes
- Clicking "Stay" keeps user in editor

## 4) Functional Requirements (What)

### User actions
- Save new article as draft with title and optional content
- Edit existing draft multiple times
- Delete draft permanently
- Publish draft to convert it to regular article
- View list of own drafts ordered by last updated
- Navigate to editor to create new draft or edit existing one

### System behaviors
- Store draft with author reference, timestamps, and content
- Return drafts only for requesting user (enforce privacy)
- Order drafts by updatedAt timestamp (most recent first)
- Generate slug only when publishing (not when saving draft)
- Replace draft record with published article record on publish
- Show browser confirmation when user has unsaved changes in editor
- Validate title is present when saving draft

### Inputs
- **Title** (required for save): String, identifies the draft
- **Description** (optional): String, brief summary
- **Body** (optional): String, article content in markdown
- **TagList** (optional): Array of strings, article tags
- **Action**: "Save Draft" or "Publish" button determines outcome

### Outputs
- **Draft object**: id, title, description, body, tagList, createdAt, updatedAt, author
- **Drafts list**: Array of draft previews for current user
- **Success confirmation**: User redirected to drafts list after saving
- **Published article**: Standard article object after publishing draft

## 5) Rules, Constraints, and Edge Cases (What)

### Validation rules
- Title must not be null or empty when saving draft
- Description, body, and tags are optional for drafts
- Same validation as published articles applies when publishing draft

### Business rules
- Drafts are always private to the author
- Drafts ordered by updatedAt timestamp descending
- No limit on number of drafts per user
- Slug is generated only when publishing (drafts use numeric ID)
- Publishing draft removes draft record and creates article record
- UpdatedAt timestamp changes on each save

### Permissions rules
- **Authenticated users**: Can create, view, edit, and delete own drafts only
- **Guest users**: Cannot access or see any drafts
- **Draft authors**: Full control over their drafts
- **Other users**: Cannot view, edit, or delete someone else's drafts even with direct URL

### Edge cases
- Saving draft with only title: Valid, creates minimal draft
- Publishing draft with empty description/body: Validation error (same as article creation)
- Concurrent edits of same draft: Last write wins (no conflict resolution)
- Navigating away with unsaved changes: Browser prompt to confirm
- Deleting draft while editing it in another tab: Shows 404 on next save
- Publishing draft: Draft disappears from drafts list, appears in published articles

### Safety checks
- Browser confirmation prompt prevents accidental loss of unsaved changes
- Delete button on draft requires intentional click (no confirmation dialog)
- Publishing validates all required fields before converting draft

## 6) Data & Lifecycle (What)

### Data created/updated
- **Draft record**: Created on first save, updated on each subsequent save
- **Timestamps**: CreatedAt set on creation, UpdatedAt modified on each edit
- **Draft ID**: Numeric identifier used before slug generation
- **Article record**: Created when draft is published (replaces draft record)

### Retention
- Drafts persist indefinitely until author deletes or publishes them
- No automatic archival, expiration, or cleanup of old drafts

### Deletion behavior
- Hard delete: Draft record removed from database permanently
- Publishing: Draft record deleted, article record created with same content
- No cascade effects (drafts have no comments, favorites, or associations)

### History/audit expectations (if any)
- CreatedAt timestamp shows when draft was first created
- UpdatedAt timestamp shows last save time
- No revision history or audit trail of draft edits
- No "modified by" tracking (only author can modify drafts)
