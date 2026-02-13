# Feature: Article Drafts
Kebab-case: `article-drafts`

## 1) Goal (Why)

### Problem statement
Authors must complete and publish articles in a single session. If they close the editor or navigate away, all progress is lost. Authors cannot save incomplete work and return to it later.

### Who feels it most
Authors creating long-form content who need multiple sessions to complete an article. Authors who want to prepare content in advance before publishing. Authors who want to iterate on ideas without immediately making them public.

### Desired outcome
Authors can save incomplete articles as private drafts, return to edit them at any time, and publish them when ready. No work is lost when navigating away from the editor. Authors have a dedicated space to manage unpublished content.

### Success criteria
- Authors can save articles as drafts before publishing
- Drafts persist and can be edited across multiple sessions
- Authors can view a list of their drafts separate from published articles
- Drafts can be published with one click when ready
- No content loss occurs during draft creation or editing

### Non-goals / out of scope
- Auto-saving drafts while typing
- Unpublishing already-published articles back to draft status
- Sharing draft previews with other users
- Version history or revision tracking for drafts
- Scheduling publication for future dates
- Draft expiration or automatic cleanup

## 2) Users & Access (What)

### Target users/personas
- Authenticated authors creating articles
- Authors working on multiple pieces simultaneously
- Authors who prefer to refine content before publishing

### Eligibility
Only authenticated users can create and manage drafts. All authenticated users have equal draft capabilities (no role-based permissions).

### Visibility
Drafts are completely private to the author. Other users cannot see, search for, or access drafts in any way. Drafts do not appear in feeds, profiles, search results, or tag filters.

### Where it lives
Primary location: Dedicated "Drafts" page accessible from main navigation. Secondary locations: Article editor (for saving drafts), standard editor (for editing drafts).

### How to reach it
- Primary entry point: "Drafts" link in main navigation bar (visible only when logged in)
- Secondary entry points: After saving a draft, direct link to edit draft from drafts list

## 3) User Experience & Flow (What)

### Happy path
1. Author navigates to "New Article" page
2. Author fills in title, description, body, and optionally tags
3. Author clicks "Save as Draft" button
4. Success message appears: "Draft saved successfully"
5. Author remains on editor page to continue editing or navigate away
6. Author later visits "Drafts" page from main navigation
7. Author sees list of drafts sorted by most recently updated
8. Author clicks "Edit" on a draft to continue working
9. Editor opens with draft content pre-filled
10. When ready, author clicks "Publish" button
11. Draft is published as live article with slug generated from title
12. Author is redirected to published article detail page
13. Draft is removed from drafts list

### Key screens/states
- Drafts list page: Shows all author's drafts with title, description, tags, updatedAt timestamp, edit button, delete button
- Article editor: Contains both "Save as Draft" and "Publish" buttons when creating or editing content
- Success message: Brief notification after saving draft or publishing

### Empty state
When author has no drafts, Drafts page displays: "No drafts yet. Start writing!" with optional button/link to create new article.

### Loading state
While fetching drafts list: Show loading spinner or skeleton screen. While saving draft: Disable save button and show "Saving..." text. While publishing: Disable publish button and show "Publishing..." text.

### Error state
If save fails: Display error message above editor form: "Failed to save draft. Please try again." If publish fails: Display error message above editor form: "Failed to publish article. Please try again." If draft load fails: Display error on drafts page: "Unable to load drafts. Please refresh the page."

### Cancellation/escape
Authors can navigate away from editor at any time after saving. Unsaved changes are lost if author navigates away without clicking "Save as Draft". No warning is shown for unsaved changes and no confirmation dialog appears on navigation (consistent with current article editor behavior).

## 4) Functional Requirements (What)

### User actions
- Save new article as draft from editor
- View list of all own drafts
- Edit existing draft
- Save changes to existing draft
- Delete draft permanently
- Publish draft as live article
- Continue editing draft after saving

### System behaviors
- Track creation date, last update date, and author for each draft
- Update last modified timestamp on each save
- Generate slug from title when draft is published
- Remove draft from drafts list after publishing
- Sort drafts by updatedAt descending (most recent first) on drafts list page
- Exclude drafts from all public feeds and searches
- Exclude drafts from tag popularity calculations

### Inputs
- Title (required): Same validation as published articles
- Description (required): Same validation as published articles
- Body (required): Same validation as published articles
- Tags (optional): Free-form text, multiple tags allowed
- Author (implicit): Current authenticated user

### Outputs
- Draft article saved to user's private collection
- Success message after saving
- Updated drafts list reflecting changes
- Published article with generated slug (when publishing)

## 5) Rules, Constraints, and Edge Cases (What)

### Validation rules
- Title, description, and body validation requirements match published articles (see Article Creation spec for specific constraints including minimum/maximum lengths and allowed characters)
- Tags are optional with no format restrictions

### Business rules
- Drafts are private to author only
- Drafts do not generate slugs until published
- Drafts do not appear in any feeds or search results
- Drafts do not affect tag popularity or tag lists
- Publishing is one-way operation (draft → published article)
- Published articles cannot be converted back to drafts
- No limit on number of drafts per user

### Permissions rules
- Authenticated users: Can create, view, edit, delete, and publish only own drafts
- Guest users: Cannot access drafts feature at all
- Authors: Can only see and modify their own drafts, not others' drafts

### Edge cases
- Saving draft with same title as existing published article: Allowed, slug generated on publish may differ
- Saving draft with same title as another draft: Allowed, treated as separate drafts
- Publishing draft with title matching existing published article slug: System must ensure each published article has a unique slug
- Editing draft then navigating away without saving: Changes are lost with no warning (consistent with current article editor behavior)
- Deleting draft while editing it in another tab: Edit tab shows error on next save attempt
- Publishing draft in one tab while editing in another tab: Similar error handling as deletion case
- No drafts exist: Empty state message shown
- Network error during save: Error message displayed, author can retry

### Safety checks
- No confirmation required before deleting draft (hard delete is immediate)
- No confirmation required before publishing draft (immediate transition to live article)

## 6) Data & Lifecycle (What)

### Data created/updated
- Draft article: Created when "Save as Draft" clicked first time
- Draft article: Updated when "Save as Draft" clicked on existing draft
- Draft createdAt: Set when draft first created, never changes
- Draft updatedAt: Set when draft created, updated on every save
- Published article: Created when "Publish" clicked on draft, draft is removed

### Retention
Drafts persist indefinitely until author publishes or deletes them. No automatic expiration or cleanup.

### Deletion behavior
Hard delete: Clicking delete on draft permanently deletes it. No soft delete or recovery mechanism. No cascading impact since drafts have no comments, favorites, or other associations.

### History/audit expectations (if any)
No audit trail or history tracking. No visibility into who created or modified drafts (only author has access). Timestamps show when draft was created and last updated, but no change history.
