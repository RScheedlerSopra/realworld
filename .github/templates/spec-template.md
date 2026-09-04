# Feature Specification: [FEATURE NAME]

**Feature Branch**: `[###-feature-name]`  
**Created**: [DATE]  
**Status**: Draft  
**Input**: User description: "$ARGUMENTS"

<!-- Conditional phase flags (used by S3 fan-out — ba-functional-design) -->
**has_screens**: false        <!-- triggers Phase 3 (Screen Specs) + Phase 4 (HTML Prototypes) -->  
**has_batches**: false        <!-- triggers Phase 5 (Batch Specs) -->  
**has_notifications**: false  <!-- triggers Phase 6 (Notifications) -->

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently

  ACCESSIBILITY: For any story with UI, content, document, media, or component
  impact, include at least one accessibility acceptance scenario (keyboard,
  focus, labels/errors, status messages, contrast, reflow, media alternatives,
  …) drawn from the catalogue in
  .github/skills/sdlc-ba-functional-design/docs/tpl-test-scenario.md
  (TS-A11Y-001 … TS-A11Y-012). If accessibility is not applicable to a story,
  state it explicitly with a short rationale.
-->

### User Story 1 - [Brief Title] (Priority: P1)

[Describe this user journey in plain language]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently - e.g., "Can be fully tested by [specific action] and delivers [specific value]"]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]
2. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

### User Story 2 - [Brief Title] (Priority: P2)

[Describe this user journey in plain language]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

### User Story 3 - [Brief Title] (Priority: P3)

[Describe this user journey in plain language]

**Why this priority**: [Explain the value and why it has this priority level]

**Independent Test**: [Describe how this can be tested independently]

**Acceptance Scenarios**:

1. **Given** [initial state], **When** [action], **Then** [expected outcome]

---

[Add more user stories as needed, each with an assigned priority]

### Edge Cases

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right edge cases.
-->

- What happens when [boundary condition]?
- How does system handle [error scenario]?

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST [specific capability, e.g., "allow users to create accounts"]
- **FR-002**: System MUST [specific capability, e.g., "validate email addresses"]  
- **FR-003**: Users MUST be able to [key interaction, e.g., "reset their password"]
- **FR-004**: System MUST [data requirement, e.g., "persist user preferences"]
- **FR-005**: System MUST [behavior, e.g., "log all security events"]

*Example of marking unclear requirements:*

- **FR-006**: System MUST authenticate users via [NEEDS CLARIFICATION: auth method not specified - email/password, SSO, OAuth?]
- **FR-007**: System MUST retain user data for [NEEDS CLARIFICATION: retention period not specified]

### Key Entities *(include if feature involves data)*

- **[Entity 1]**: [What it represents, key attributes without implementation]
- **[Entity 2]**: [What it represents, relationships to other entities]

### Non-Functional Requirements *(Constitution-Mandated)*

<!--
  ACTION REQUIRED: Define non-functional requirements per constitution principles.
  All features must address these areas per the SpecKit Constitution.
-->

**Testing Requirements** (NON-NEGOTIABLE):

- **NFR-T1**: Unit test coverage MUST be >= 80% (100% for critical paths)
- **NFR-T2**: Integration tests MUST cover all API contracts and database operations
- **NFR-T3**: Contract tests MUST validate all public interfaces

**Performance Requirements**:

- **NFR-P1**: API endpoints MUST respond with p95 < 200ms, p99 < 500ms
- **NFR-P2**: UI Time to Interactive MUST be < 3 seconds, First Contentful Paint < 1 second
- **NFR-P3**: Database queries MUST execute < 100ms (simple) or < 500ms (complex)
- **NFR-P4**: Memory baseline MUST NOT exceed 512MB (excluding caches)
- **NFR-P5**: Feature MUST handle 10x current load without architectural changes

**User Experience Requirements**:

- **NFR-UX1**: MUST meet WCAG 2.2 Level AA accessibility standards by default; reference EN 301 549 where public-sector, procurement, ICT product, document, or vendor obligations apply
- **NFR-UX2**: User actions MUST provide feedback within 100ms
- **NFR-UX3**: Error messages MUST be user-friendly and actionable
- **NFR-UX4**: MUST use approved design tokens (colors, typography, spacing)
- **NFR-UX5**: MUST function consistently across all supported platforms

**Code Quality Requirements**:

- **NFR-Q1**: Cyclomatic complexity MUST NOT exceed 10 (or require justification)
- **NFR-Q2**: Public APIs and complex logic MUST include documentation comments
- **NFR-Q3**: Code MUST pass automated linting and formatting checks
- **NFR-Q4**: Functions MUST have single, well-defined responsibilities

*Note: Mark any NFR as "N/A" with justification if not applicable to this feature*

### Accessibility Governance *(mandatory for UI, content, document, media, flow, component, and vendor-impacting work)*

<!--
  ACTION REQUIRED: Classify accessibility impact and include the fields below.
  Use .github/templates/accessibility-spec-section-template.md for a fuller version.
  Automated or AI-generated checks are not sufficient for full accessibility compliance.
-->

| Field | Value |
|-------|-------|
| **Accessibility baseline** | WCAG 2.2 AA by default; EN 301 549 where relevant; or Not applicable with rationale |
| **Relevant WCAG/EN criteria** | [List criteria] / Not applicable |
| **Component patterns** | [List catalogue IDs from `.github/knowledge/accessibility/component-criteria/_index.yaml` (e.g. `button`, `dialog-modal`, `datepicker`); each ID inherits its keyboard table, ARIA expectations, applicable WCAG criteria, and Gherkin scenarios. Add free-form notes for patterns that have no catalogue entry yet.] |
| **Keyboard requirements** | [Tab order, arrow keys, Escape, Enter/Space, shortcuts] |
| **Focus requirements** | [Visible focus, focus order, focus trap, focus return] |
| **Semantics / ARIA guidance** | [Native semantics first, ARIA only when needed, restrictions] |
| **Error handling** | [Labels, error association, announcements, prevention, recovery] |
| **Content requirements** | [Headings, link text, alt text, captions, transcripts, plain language] |
| **Responsive / zoom requirements** | [Reflow, 200% zoom, 400% where applicable, orientation] |
| **Test scenarios** | [Manual, automated, Gherkin, Playwright, screen reader smoke] |
| **Evidence expectations** | [Screenshots, DOM/code, keyboard notes, tool output, manual result, retest] |
| **Open risks or manual review needed** | [Items that need human validation] |

Accessibility evidence statuses must use: `Pass`, `Fail`, `Needs human review`, `Not tested`, `Not applicable`, `Accepted risk`, `Fixed`, or `Retest required`.

## Success Criteria *(mandatory)*

<!--
  ACTION REQUIRED: Define measurable success criteria.
  These must be technology-agnostic and measurable.
-->

### Measurable Outcomes

- **SC-001**: [Measurable metric, e.g., "Users can complete account creation in under 2 minutes"]
- **SC-002**: [Measurable metric, e.g., "System handles 1000 concurrent users without degradation"]
- **SC-003**: [User satisfaction metric, e.g., "90% of users successfully complete primary task on first attempt"]
- **SC-004**: [Business metric, e.g., "Reduce support tickets related to [X] by 50%"]
