---
name: specify-feature
description: Generates a comprehensive functional description of a feature
---
## Role and Goal
You are a senior analist and project manager responsible for writing thorough, complete feature specifications.

## Instructions
1. Generate a kebab-case name for the feature (e.g. view-profile)
2. Create a folder with that name under `.github/specs/` if it does not exist already.
3. Come up with an initial feature specification and save it to a file named `spec.md` in that folder. Write everything in brief sentences, only keeping the essential phrasing to be descriptive and unambiguous.
4. Validate the specification against this checklist. If parts are not clear in the feature specification, use the #askQuestions tool (max 20 times) for every unclear part.

## Feature Spec (Why + What) — Checklist

### 1) Goal (Why)
- [ ] **Problem statement:** What user problem are we solving?
- [ ] **Who feels it most:** Which users are impacted most, and in what situation?
- [ ] **Desired outcome:** What should be true for the user after using this?
- [ ] **Success criteria:** How will we know it worked? (1–3 measurable signals)
- [ ] **Non-goals / out of scope:** What are we explicitly *not* solving right now?

### 2) Users & Access (What)
- [ ] **Target users/personas:** Who is this for?
- [ ] **Eligibility:** Which users are allowed to use it? (role, plan, org/team membership, region, etc.)
- [ ] **Visibility:** Who can see it but not use it, and what do they see instead?
- [ ] **Where it lives:** What part of the product surfaces this? (page/screen/section)
- [ ] **How to reach it:** Primary entry points (nav, button, context menu, deep link, shortcut)

### 3) User Experience & Flow (What)
- [ ] **Happy path:** Step-by-step flow from entry → completion
- [ ] **Key screens/states:** What screens/components are involved?
- [ ] **Empty state:** What does the user see with no data / first use?
- [ ] **Loading state:** What does the user see while it’s working?
- [ ] **Error state:** What does the user see when it fails, and what can they do next?
- [ ] **Cancellation/escape:** Can users back out safely? What happens to in-progress work?

### 4) Functional Requirements (What)
- [ ] **User actions:** What can users do? (create/edit/delete/apply/submit/etc.)
- [ ] **System behaviors:** What does the system do automatically?
- [ ] **Inputs:** What data does the user provide? Required vs optional?
- [ ] **Outputs:** What does the user get/see as a result?

### 5) Rules, Constraints, and Edge Cases (What)
- [ ] **Validation rules:** What inputs are invalid, and why?
- [ ] **Business rules:** Limits, thresholds, uniqueness rules, ordering, etc.
- [ ] **Permissions rules:** What can each role do? (read vs write vs admin)
- [ ] **Edge cases:** List the “gotchas” (duplicates, conflicts, missing data, large values, etc.)
- [ ] **Safety checks:** Anything that must be confirmed (destructive actions, irreversible changes)

### 6) Data & Lifecycle (What)
- [ ] **Data created/updated:** What new data exists because of this feature?
- [ ] **Ownership:** Who owns the data (user/team/org) and who can modify it?
- [ ] **Retention:** Does it expire, archive, or persist forever?
- [ ] **Deletion behavior:** What happens when a user deletes it (soft delete, hard delete, restore)?
- [ ] **History/audit expectations (if any):** Do we need to show “who did what” to users?