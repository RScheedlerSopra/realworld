# Feature Specification: Article Draft Management

**Feature Branch**: `001-article-draft-management`  
**Created**: 2026-03-01  
**Status**: Draft  
**Input**: User description: "I want to be able to draft articles instead of publishing them directly"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Save Draft Article (Priority: P1)

An author wants to start writing an article but isn't ready to publish it publicly. They create a new article, add content (title, description, body, tags), and save it as a draft to work on later.

**Why this priority**: This is the core functionality - without the ability to create and save drafts, the entire feature has no value. This enables authors to iterate on content privately.

**Independent Test**: Can be fully tested by creating a new article, entering content, and clicking "Save as draft". Delivers value by allowing authors to preserve work-in-progress without public exposure.

**Acceptance Scenarios**:

1. **Given** an authenticated author is on the article creation page, **When** they enter article details (title, description, body, tags) and click "Save as draft", **Then** the article is saved as a draft and the author is redirected to their Drafts page
2. **Given** an authenticated author is creating a new article, **When** they try to save as draft without entering a title, **Then** validation errors are shown and the draft is not saved
3. **Given** an author has saved a draft, **When** they navigate away and return to the Drafts page, **Then** the draft appears in their list of drafts with the saved content intact

---

### User Story 2 - View Personal Draft List (Priority: P1)

An author needs to see all their draft articles in one place to manage and continue working on them. They navigate to a dedicated "Drafts" page that shows only their own unpublished articles.

**Why this priority**: Essential for draft discoverability - authors must be able to find their drafts to edit or publish them. Without this, drafts would be orphaned.

**Independent Test**: Can be tested by creating multiple drafts, navigating to the Drafts page, and verifying all drafts appear. Delivers value by providing a central location for draft management.

**Acceptance Scenarios**:

1. **Given** an authenticated author has created draft articles, **When** they navigate to the Drafts page, **Then** they see a list of all their draft articles (title, description, and creation/update timestamp)
2. **Given** an author has no drafts, **When** they navigate to the Drafts page, **Then** they see an empty state message like "You haven't created any drafts yet"
3. **Given** an author is on the Drafts page, **When** they click on a draft article, **Then** they are taken to the edit page for that draft

---

### User Story 3 - Edit Existing Draft (Priority: P2)

An author wants to continue working on a previously saved draft. They open a draft from their Drafts page, make changes to the title, description, body, or tags, and save the updated version as a draft.

**Why this priority**: Critical for iterative content creation - authors need to refine drafts over multiple sessions before publishing.

**Independent Test**: Can be tested by opening an existing draft, modifying content, and saving changes. Delivers value by enabling continuous improvement of draft content.

**Acceptance Scenarios**:

1. **Given** an author is viewing/editing a draft article, **When** they modify the content and click "Save as draft", **Then** the draft is updated with the new content and remains unpublished
2. **Given** an author is editing a draft, **When** they make no changes and click "Save as draft", **Then** the draft is saved successfully (idempotent operation)
3. **Given** an author is editing a draft, **When** they remove required fields (e.g., title) and click "Save as draft", **Then** validation errors are shown and the invalid changes are not saved

---

### User Story 4 - Publish Draft Article (Priority: P2)

An author has finished working on a draft and is ready to make it public. They open the draft for editing and click "Publish" to convert it from a draft to a publicly visible article.

**Why this priority**: This completes the draft workflow - enables authors to transition from private creation to public sharing when ready.

**Independent Test**: Can be tested by creating a draft and clicking "Publish". Delivers value by making the article visible in feeds and on the author's profile.

**Acceptance Scenarios**:

1. **Given** an author is editing a draft article, **When** they click "Publish", **Then** the article becomes publicly visible, appears in global/personal feeds, is removed from the Drafts page, and appears on the author's public profile
2. **Given** an author has just published a draft, **When** other users view the global feed, **Then** the newly published article appears in the feed
3. **Given** an author has published a draft, **When** the author navigates to their Drafts page, **Then** the published article no longer appears in the drafts list
4. **Given** an author is editing a draft with missing required fields, **When** they click "Publish", **Then** validation errors are shown and the article remains a draft

---

### User Story 5 - Draft Privacy (Priority: P1)

Drafts must remain completely private to the author. Other users (including followers) cannot see, access, or interact with draft articles in any way.

**Why this priority**: Privacy is essential - exposing incomplete work would undermine the entire purpose of drafts and create a poor user experience.

**Independent Test**: Can be tested by creating a draft as one user, then attempting to access it as another user (via direct URL, profile, feeds). Delivers value by ensuring safe private workspace.

**Acceptance Scenarios**:

1. **Given** an author has draft articles, **When** another user visits the author's profile page, **Then** the drafts are not visible (only published articles appear)
2. **Given** an author has draft articles, **When** a guest or other user views the global feed, **Then** draft articles never appear
3. **Given** an author has draft articles, **When** followers view their personal feed, **Then** draft articles are not included
4. **Given** an author has created a draft, **When** another user attempts to access the draft via direct URL, **Then** they receive an error (not found or unauthorized)

---

### Edge Cases

- What happens when an author attempts to create a draft with a title identical to one of their existing drafts? (System should allow duplicates since drafts are private and work-in-progress)
- What happens when an author deletes a draft? (Draft is permanently removed with cascade deletion of associated data like tags)
- What happens if an author creates a draft with a slug that matches a published article? (Slug generation should handle conflicts - perhaps by appending unique identifier or timestamp)
- What happens when an author has many drafts (e.g., 100+)? (Drafts page should support pagination similar to article lists)
- What happens when an author tries to favorite or comment on their own draft? (Not possible - drafts don't have social features until published)
- What happens when an author is editing a draft and their session expires? (Standard authentication behavior - redirect to login, preserve draft content)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authenticated authors to create new articles and save them as drafts without publishing
- **FR-002**: System MUST provide a "Save as draft" button on the article creation and edit pages
- **FR-003**: System MUST provide a dedicated "Drafts" page accessible only to authenticated authors showing their own drafts
- **FR-004**: Drafts MUST be completely private - visible only to the author who created them
- **FR-005**: System MUST allow authors to edit their draft articles and save changes without publishing
- **FR-006**: System MUST provide a "Publish" button when editing drafts that converts them to normal published articles
- **FR-007**: Published articles MUST NOT be convertible back to draft status (one-way transition)
- **FR-008**: Draft articles MUST NOT appear in global feeds, personal feeds, profile pages, tag lists, or any public article discovery mechanism
- **FR-009**: System MUST validate required article fields (title, description, body) before saving drafts (same validation as published articles)
- **FR-010**: System MUST support tags for draft articles (tags are saved with the draft)
- **FR-011**: System MUST generate unique slugs for draft articles (or defer slug generation until publish time)
- **FR-012**: Draft articles MUST NOT support comments, favorites, or any social interactions until published
- **FR-013**: System MUST display drafts with metadata including creation and last-updated timestamps
- **FR-014**: When a draft is published, it MUST be removed from the Drafts page and appear in all standard public locations (feeds, profile, tag lists)
- **FR-015**: System MUST handle deletion of draft articles (author can delete drafts)
- **FR-016**: Empty state on Drafts page MUST guide authors when no drafts exist

### Key Entities

- **Draft Article**: An unpublished article entity with the same attributes as published articles (title, description, body, tags, author, timestamps) but with an additional publication status indicator (draft vs. published). Only accessible by the creating author.
- **Draft List**: A collection view showing all draft articles for a specific author, ordered by most recently updated first.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Authors can create and save draft articles without making them publicly visible
- **SC-002**: Authors can view all their drafts on a dedicated Drafts page with immediate load time (under 2 seconds)
- **SC-003**: Authors can edit and update drafts unlimited times before publishing
- **SC-004**: Draft articles remain completely private - zero access by other users (verified by 100% of unauthorized access attempts being rejected)
- **SC-005**: Publishing a draft converts it to a normal article visible in all standard locations (feeds, profiles, tags)
- **SC-006**: Published articles cannot be converted back to drafts (100% prevention of reverse transition)
- **SC-007**: Draft workflow reduces accidental publications - authors can iterate on content safely before public release
