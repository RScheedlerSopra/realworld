<!--
Feature specs live at: .github/specs/<kebab-case-name>/spec.md
Write in brief sentences. Prefer specific, testable wording.
-->

# Feature: Article Ratings
Kebab-case: `article-ratings`

## 1) Goal (Why)

### Problem statement
Readers can only favorite articles today. They cannot express quality with more nuance.

### Who feels it most
Frequent readers deciding what to read next, and authors who want clearer feedback on article quality.

### Desired outcome
Users can rate articles with stars, update or clear their rating, and everyone can use rating averages and counts to judge article quality.

### Success criteria
- Authenticated users can set a 1–5 star rating on any article
- Users can update an existing rating or clear it
- Average rating and total rating count are visible on article list and article detail views
- Rating values remain consistent after page reload and across sessions
- Guests are prompted to sign in when attempting to rate

### Non-goals / out of scope
- Written rating reviews or comments attached to ratings
- Profile-level "Rated Articles" pages or filters
- Sorting or feed ranking changes based on ratings
- Rating notifications to authors
- Rating moderation workflows

## 2) Users & Access (What)

### Target users/personas
- Authenticated readers who want to score article quality
- Guest readers who want to view quality signals before reading
- Authors monitoring audience feedback

### Eligibility
- Set/update/clear rating: Authenticated users only
- View average rating and count: All users

### Visibility
- Star controls are shown to authenticated users
- Guests see aggregate rating info but not editable controls
- If guests use a rating entry point, they are directed to sign in

### Where it lives
- Article preview cards in global and personalized feeds
- Article detail page in article meta area

### How to reach it
- Primary entry point: Star rating control on each article card and article detail page
- Secondary entry points: None in this phase

## 3) User Experience & Flow (What)

### Happy path
1. Authenticated user opens an article card or article detail view
2. User selects a star value from 1 to 5
3. System saves the rating
4. UI reflects selected stars and updated average/count
5. User later changes rating or clears it, and UI updates again

### Key screens/states
- Feed article card with rating control and aggregate display
- Article detail header/meta with rating control and aggregate display

### Empty state
- Article with no ratings shows “No ratings yet” and count of 0

### Loading state
- On save/update/clear, star controls are disabled until the request completes

### Error state
- Unauthenticated attempt: prompt to sign in
- Save failure: keep prior visible state and show “Unable to save rating. Try again.” near the rating control
- Article not found: show the existing article-not-found state

### Cancellation/escape
- User can leave without rating and no data is saved
- User can clear their rating to return to unrated state

## 4) Functional Requirements (What)

### User actions
- Set a first-time rating from 1–5 stars
- Update existing rating to a different star value
- Clear existing rating
- View aggregate rating information on article cards and article detail

### System behaviors
- Store at most one active rating per user per article
- After any rating change, show updated average rating and total rating count
- When authenticated, show the current user’s selected rating
- Show aggregate values to guests

### Inputs
- **Article slug** (required): Article to rate
- **Star value** (required for set/update): Integer from 1 to 5
- **Clear intent** (required for clear): Explicit request to remove current user rating

### Outputs
- **Article rating summary**: Average rating (displayed to 1 decimal place, round half up) and total ratings count
- **User rating state**: Current user’s selected rating or null
- **Operation result**: Success or failure with actionable message

## 5) Rules, Constraints, and Edge Cases (What)

### Validation rules
- User must be authenticated to set/update/clear ratings
- Star value must be an integer from 1 through 5
- Target article must exist

### Business rules
- A user has only one rating per article at a time
- Setting the same rating value again does not change displayed aggregate values
- Updating a rating replaces prior value and recalculates aggregates
- Clearing a rating removes that user’s contribution from aggregates

### Permissions rules
- **Authenticated users**: Can rate, update, and clear their own ratings on any article
- **Guest users**: Can view aggregates only
- **Authors**: Have no special rating privileges on others beyond normal authenticated behavior

### Edge cases
- First rating on an article transitions from “No ratings yet” to populated aggregate
- Last remaining rating is cleared and article returns to no-rating state
- Rapid repeated clicks should apply one final stable value
- Concurrent ratings from many users should result in correct average and count
- User rates their own article: allowed

### Safety checks
- Duplicate submissions blocked while request is in progress
- Clear action is reversible by setting a new rating

## 6) Data & Lifecycle (What)

### Data created/updated
- **User rating state**: Created on first rating, changed on update, removed on clear
- **Article rating summary**: Updated average and count after each rating change

### Retention
- Ratings persist until user clears them, article is deleted, or user is deleted

### Deletion behavior
- Clearing rating removes only the current user’s rating for that article
- Deleting an article removes all associated ratings
- Deleting a user removes all ratings made by that user

### History/audit expectations (if any)
- No end-user audit timeline for rating changes in this phase
- Only current user rating and current aggregates are presented
