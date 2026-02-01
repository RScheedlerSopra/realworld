# Real-Time Chat - Technical Implementation Plan

## Overview

This plan details the step-by-step implementation of real-time private messaging between users using SignalR for WebSocket-based real-time communication. The implementation follows the existing CQRS pattern with MediatR on the backend and React Query for state management on the frontend.

---

## Phase 1: Backend - Domain & Database

### 1.1 Create Domain Entities

**Task**: Create `Message` domain entity  
**Location**: `backend/src/Conduit/Domain/Message.cs`  
**Details**:
- Properties: `MessageId` (int, primary key), `ConversationId` (int, FK), `SenderId` (int, FK to Person), `Body` (string, unlimited length), `CreatedAt` (DateTime), `DeliveredAt` (DateTime?, nullable)
- Navigation properties: `Conversation` (Conversation), `Sender` (Person)
- Use `[JsonIgnore]` for foreign keys and navigation properties as appropriate
- Follow the existing pattern from `Comment.cs`

**Task**: Create `Conversation` domain entity  
**Location**: `backend/src/Conduit/Domain/Conversation.cs`  
**Details**:
- Properties: `ConversationId` (int, primary key), `Participant1Id` (int, FK to Person), `Participant2Id` (int, FK to Person), `CreatedAt` (DateTime), `UpdatedAt` (DateTime)
- Navigation properties: `Participant1` (Person), `Participant2` (Person), `Messages` (List<Message>)
- Use `[JsonIgnore]` for foreign keys and navigation properties
- Follow the existing pattern from `Article.cs`

**Task**: Create `ConversationReadStatus` domain entity  
**Location**: `backend/src/Conduit/Domain/ConversationReadStatus.cs`  
**Details**:
- Properties: `ConversationId` (int, FK), `UserId` (int, FK to Person), `LastReadAt` (DateTime?), `UnreadCount` (int, computed property)
- Composite primary key: `ConversationId` + `UserId`
- Navigation properties: `Conversation` (Conversation), `User` (Person)
- Follow the existing pattern from `ArticleFavorite.cs` (many-to-many join table)

### 1.2 Update Database Context

**Task**: Add DbSets to ConduitContext  
**Location**: `backend/src/Conduit/Infrastructure/ConduitContext.cs`  
**Details**:
- Add `DbSet<Message> Messages`
- Add `DbSet<Conversation> Conversations`
- Add `DbSet<ConversationReadStatus> ConversationReadStatuses`

**Task**: Configure entity relationships in OnModelCreating  
**Location**: `backend/src/Conduit/Infrastructure/ConduitContext.cs`  
**Details**:
- Configure `Conversation` relationships:
  - `HasOne(Participant1).WithMany().HasForeignKey(Participant1Id).OnDelete(DeleteBehavior.Restrict)`
  - `HasOne(Participant2).WithMany().HasForeignKey(Participant2Id).OnDelete(DeleteBehavior.Restrict)`
  - Set composite unique index on `(Participant1Id, Participant2Id)` to prevent duplicate conversations
- Configure `Message` relationships:
  - `HasOne(Conversation).WithMany(Messages).HasForeignKey(ConversationId).OnDelete(DeleteBehavior.Cascade)`
  - `HasOne(Sender).WithMany().HasForeignKey(SenderId).OnDelete(DeleteBehavior.Restrict)`
- Configure `ConversationReadStatus` composite key and relationships:
  - `HasKey(t => new { t.ConversationId, t.UserId })`
  - `HasOne(Conversation).WithMany().HasForeignKey(ConversationId).OnDelete(DeleteBehavior.Cascade)`
  - `HasOne(User).WithMany().HasForeignKey(UserId).OnDelete(DeleteBehavior.Cascade)`
- Follow pattern from existing `FollowedPeople` configuration

### 1.3 Create Database Migration

**Task**: Generate and test database migration  
**Location**: N/A (database update via EnsureCreated)  
**Details**:
- Since the project uses `Database.EnsureCreated()`, the schema will auto-update on next run
- Verify schema changes by inspecting `realworld.db` after running the app
- Alternative: If migrations are required, use `dotnet ef migrations add AddChatEntities`

---

## Phase 2: Backend - SignalR Infrastructure

### 2.1 Add SignalR NuGet Package

**Task**: Add SignalR to project dependencies  
**Location**: `backend/src/Conduit/Conduit.csproj`  
**Details**:
- Add package reference: `Microsoft.AspNetCore.SignalR` (should be included in ASP.NET Core by default)
- If needed, add to `Directory.Packages.props` following existing package management pattern

### 2.2 Create ChatHub

**Task**: Create SignalR Hub for real-time messaging  
**Location**: `backend/src/Conduit/Infrastructure/Chat/ChatHub.cs`  
**Details**:
- Create `ChatHub` class inheriting from `Hub`
- Implement method: `Task JoinConversation(string conversationId)` - adds user to SignalR group for that conversation
- Implement method: `Task LeaveConversation(string conversationId)` - removes user from group
- Implement method: `Task SendMessageToConversation(string conversationId, object messageData)` - broadcasts message to all users in conversation group
- Use `Context.UserIdentifier` to get current user (from JWT claims)
- Follow separation of concerns: hub only handles WebSocket routing, business logic stays in MediatR handlers

**Task**: Create user ID provider for SignalR  
**Location**: `backend/src/Conduit/Infrastructure/Chat/UserIdProvider.cs`  
**Details**:
- Implement `IUserIdProvider` interface
- Extract user ID from JWT claims (username or person ID)
- Return unique identifier for SignalR to track user connections
- This allows SignalR to send messages to specific users across multiple connections/devices

### 2.3 Configure SignalR in Program.cs

**Task**: Register SignalR services  
**Location**: `backend/src/Conduit/Program.cs` or `backend/src/Conduit/ServicesExtensions.cs`  
**Details**:
- Add `builder.Services.AddSignalR()` to service registration
- Add `builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>()`
- Add hub endpoint: `app.MapHub<ChatHub>("/hubs/chat")` before `app.Run()`
- Update CORS configuration to allow SignalR:
  ```csharp
  app.UseCors(x => x
      .AllowAnyOrigin()
      .AllowAnyHeader()
      .AllowAnyMethod()
      .AllowCredentials() // Required for SignalR
      .WithOrigins("http://localhost:3000")); // Update as needed
  ```

---

## Phase 3: Backend - Feature Implementation

### 3.1 Messages Feature - Send Message

**Task**: Create Send command and handler  
**Location**: `backend/src/Conduit/Features/Messages/Send.cs`  
**Details**:
- Create `Send.Command` record with properties: `MessageData` (containing `Body`), `RecipientUsername`
- Create `Send.CommandValidator` using FluentValidation:
  - `Body` must not be empty or whitespace
  - `RecipientUsername` must not be empty
- Create `Send.Handler` implementing `IRequestHandler<Command, MessageEnvelope>`:
  - Get or create conversation between current user and recipient (check both directions: user→recipient and recipient→user)
  - If no conversation exists, create new `Conversation` with both participants
  - Create new `Message` with sender as current user, body from command, `CreatedAt = DateTime.UtcNow`
  - Save message to database
  - Update conversation `UpdatedAt` timestamp
  - Inject `IHubContext<ChatHub>` and broadcast message to conversation group via SignalR
  - Return `MessageEnvelope` containing the saved message
- Follow pattern from `Features/Comments/Create.cs`

**Task**: Create MessageEnvelope response model  
**Location**: `backend/src/Conduit/Features/Messages/MessageEnvelope.cs`  
**Details**:
- Record containing single `Message` property
- Follow pattern from `CommentEnvelope.cs`

**Task**: Create MessagesEnvelope response model (for lists)  
**Location**: `backend/src/Conduit/Features/Messages/MessagesEnvelope.cs`  
**Details**:
- Record containing `List<Message>` property
- Follow pattern from `CommentsEnvelope.cs`

### 3.2 Messages Feature - List Messages

**Task**: Create List query and handler  
**Location**: `backend/src/Conduit/Features/Messages/List.cs`  
**Details**:
- Create `List.Query` record with property: `ConversationId` (int)
- Create `List.QueryHandler` implementing `IRequestHandler<Query, MessagesEnvelope>`:
  - Verify current user is a participant in the conversation (Participant1Id or Participant2Id matches current user)
  - If not, throw `RestException` with 403 Forbidden
  - Load conversation with all messages, include sender information
  - Order messages by `CreatedAt` ascending (oldest first)
  - Return `MessagesEnvelope` with messages list
- Follow pattern from `Features/Comments/List.cs`

**Task**: Create alternative List by username query  
**Location**: `backend/src/Conduit/Features/Messages/ListByUsername.cs`  
**Details**:
- Create `ListByUsername.Query` record with property: `Username` (string)
- Find conversation between current user and specified username
- If no conversation exists, return empty `MessagesEnvelope`
- Otherwise, delegate to standard List query
- This supports opening chat from profile page

### 3.3 Messages Feature - Mark as Delivered

**Task**: Create MarkDelivered command and handler  
**Location**: `backend/src/Conduit/Features/Messages/MarkDelivered.cs`  
**Details**:
- Create `MarkDelivered.Command` record with property: `MessageId` (int)
- Create handler that:
  - Loads message by ID
  - Verifies current user is the recipient (not the sender)
  - Sets `DeliveredAt = DateTime.UtcNow` if not already set
  - Saves changes
  - Returns success indicator
- This is called by recipient's client when message is received via SignalR

### 3.4 Conversations Feature - List Conversations

**Task**: Create List query and handler  
**Location**: `backend/src/Conduit/Features/Conversations/List.cs`  
**Details**:
- Create `List.Query` record (no parameters, uses current user from context)
- Create `List.QueryHandler` implementing `IRequestHandler<Query, ConversationsEnvelope>`:
  - Load all conversations where current user is Participant1 or Participant2
  - Include both participants' information (username, image)
  - Include last message preview and timestamp
  - Calculate unread count from `ConversationReadStatus`
  - Order by `UpdatedAt` descending (most recent first)
  - Return `ConversationsEnvelope` with conversation list
- Follow pattern from existing list queries

**Task**: Create ConversationEnvelope and ConversationsEnvelope  
**Location**: `backend/src/Conduit/Features/Conversations/ConversationEnvelope.cs`  
**Details**:
- `ConversationEnvelope`: Record with single `Conversation` property
- `ConversationsEnvelope`: Record with `List<Conversation>` property
- Add computed properties to conversation model for frontend:
  - `OtherParticipant` (Person) - the participant who is not current user
  - `LastMessage` (Message) - most recent message
  - `UnreadCount` (int) - count of unread messages for current user

### 3.5 Conversations Feature - Mark as Read

**Task**: Create MarkAsRead command and handler  
**Location**: `backend/src/Conduit/Features/Conversations/MarkAsRead.cs`  
**Details**:
- Create `MarkAsRead.Command` record with property: `ConversationId` (int)
- Create handler that:
  - Verifies current user is a participant
  - Updates or creates `ConversationReadStatus` record for current user
  - Sets `LastReadAt = DateTime.UtcNow`
  - Recalculates `UnreadCount` (count messages after previous LastReadAt)
  - Saves changes
  - Broadcasts unread count update via SignalR to user's other connected devices
- Called when user opens a conversation

### 3.6 Conversations Feature - Get Unread Count

**Task**: Create UnreadCount query and handler  
**Location**: `backend/src/Conduit/Features/Conversations/UnreadCount.cs`  
**Details**:
- Create `UnreadCount.Query` record (no parameters, uses current user)
- Create handler that:
  - Sums up all unread counts across all conversations for current user
  - Returns total unread count
- Used to display badge on Messages nav link

### 3.7 Create API Controllers

**Task**: Create MessagesController  
**Location**: `backend/src/Conduit/Features/Messages/MessagesController.cs`  
**Details**:
- `POST /api/messages` - Send message (requires auth)
  - Body: `{ "message": { "body": "text", "recipientUsername": "john" } }`
  - Returns: MessageEnvelope
- `GET /api/messages?conversationId={id}` - List messages in conversation (requires auth)
  - Returns: MessagesEnvelope
- `GET /api/messages?username={username}` - List messages with specific user (requires auth)
  - Returns: MessagesEnvelope
- `PUT /api/messages/{id}/delivered` - Mark message as delivered (requires auth)
- Follow pattern from `CommentsController.cs`

**Task**: Create ConversationsController  
**Location**: `backend/src/Conduit/Features/Conversations/ConversationsController.cs`  
**Details**:
- `GET /api/conversations` - List all conversations for current user (requires auth)
  - Returns: ConversationsEnvelope
- `PUT /api/conversations/{id}/read` - Mark conversation as read (requires auth)
- `GET /api/conversations/unread-count` - Get total unread count (requires auth)
  - Returns: `{ "unreadCount": 5 }`
- Follow pattern from existing controllers

---

## Phase 4: Frontend - SignalR Client Setup

### 4.1 Install SignalR Client Package

**Task**: Add @microsoft/signalr to package.json  
**Location**: `frontend/package.json`  
**Details**:
- Run: `npm install @microsoft/signalr`
- Verify version compatibility with ASP.NET Core SignalR

### 4.2 Create SignalR Connection Service

**Task**: Create SignalR connection manager  
**Location**: `frontend/src/services/signalr.js`  
**Details**:
- Create singleton connection to `/hubs/chat`
- Export functions:
  - `startConnection(token)` - establishes connection with JWT token
  - `stopConnection()` - disconnects
  - `onMessageReceived(callback)` - registers callback for new messages
  - `onMessageDelivered(callback)` - registers callback for delivery confirmations
  - `onUnreadCountChanged(callback)` - registers callback for unread count updates
  - `joinConversation(conversationId)` - joins SignalR group
  - `leaveConversation(conversationId)` - leaves SignalR group
- Handle automatic reconnection on connection loss
- Extract JWT token from localStorage

### 4.3 Create SignalR Hook

**Task**: Create useSignalRConnection custom hook  
**Location**: `frontend/src/hooks/useSignalRConnection.js`  
**Details**:
- Manages SignalR connection lifecycle
- Connects when user is authenticated, disconnects on logout
- Provides connection status (connected, connecting, disconnected, reconnecting)
- Integrates with React Query to invalidate queries when real-time updates arrive
- Returns: `{ status, error, reconnect }`
- Follow pattern from existing hooks

---

## Phase 5: Frontend - Chat Hooks (API Integration)

### 5.1 Create Conversation Hooks

**Task**: Create useConversationsQuery hook  
**Location**: `frontend/src/hooks/useConversationsQuery.js`  
**Details**:
- Uses React Query to fetch `/api/conversations`
- Returns list of conversations with unread counts and last message previews
- Automatically refetches when SignalR broadcasts update
- Placeholder data: `{ conversations: [] }`
- Follow pattern from `useArticlesQuery.js`

**Task**: Create useUnreadCountQuery hook  
**Location**: `frontend/src/hooks/useUnreadCountQuery.js`  
**Details**:
- Uses React Query to fetch `/api/conversations/unread-count`
- Returns total unread count for navigation badge
- Automatically refetches when SignalR broadcasts update
- Enabled only when user is authenticated
- Refetch interval: 30 seconds (backup to SignalR)

**Task**: Create useMarkConversationReadMutation hook  
**Location**: `frontend/src/hooks/useMarkConversationReadMutation.js`  
**Details**:
- Mutation to PUT `/api/conversations/{id}/read`
- Invalidates conversations query and unread count query on success
- Called when user opens a conversation
- Follow pattern from `useFavoriteArticleMutation.js`

### 5.2 Create Message Hooks

**Task**: Create useMessagesQuery hook  
**Location**: `frontend/src/hooks/useMessagesQuery.js`  
**Details**:
- Hook takes parameter: `conversationId` or `username`
- Uses React Query to fetch messages for that conversation
- Automatically refetches when SignalR broadcasts new message
- Placeholder data: `{ messages: [] }`
- Follow pattern from `useArticleCommentsQuery.js`

**Task**: Create useSendMessageMutation hook  
**Location**: `frontend/src/hooks/useSendMessageMutation.js`  
**Details**:
- Mutation to POST `/api/messages`
- Optimistic update: immediately add message to query cache with "sending" status
- On success, update message status to "sent"
- On error, rollback optimistic update and show retry option
- Invalidates conversations list query (to update last message preview)
- Follow pattern from `useAddCommentMutation.js`

**Task**: Create useMarkMessageDeliveredMutation hook  
**Location**: `frontend/src/hooks/useMarkMessageDeliveredMutation.js`  
**Details**:
- Mutation to PUT `/api/messages/{id}/delivered`
- Called automatically when recipient receives message via SignalR
- Silently fails if already marked delivered (idempotent)
- Updates message in query cache to show "delivered" status

### 5.3 Create Draft Message Hook

**Task**: Create useDraftMessage hook  
**Location**: `frontend/src/hooks/useDraftMessage.js`  
**Details**:
- Manages draft message state per conversation using localStorage
- Hook takes `conversationId` or `username` parameter
- Returns: `{ draft, setDraft, clearDraft }`
- Persists draft text when modal is closed
- Restores draft when modal is reopened
- Uses `useState` with lazy initialization from localStorage

---

## Phase 6: Frontend - UI Components

### 6.1 Update Navbar Component

**Task**: Add Messages link with unread badge  
**Location**: `frontend/src/components/Navbar.jsx`  
**Details**:
- Add "Messages" link to authenticated user section (next to "Settings")
- Display unread count badge using `useUnreadCountQuery` hook
- Badge shows number if > 0, hidden if 0
- Link points to `/messages` route
- Follow existing Navbar pattern for authenticated links

### 6.2 Update Profile Component

**Task**: Add Message button to profile  
**Location**: `frontend/src/pages/Profile.jsx`  
**Details**:
- Add "Message" button next to "Follow" button
- Show button only when viewing another user's profile (not own profile)
- Button click opens ChatModal with that user
- Follow existing pattern from FollowProfileButton component

### 6.3 Create ChatModal Component

**Task**: Create modal overlay for chat conversation  
**Location**: `frontend/src/components/ChatModal.jsx`  
**Details**:
- Props: `recipientUsername`, `onClose`
- Modal overlay covering page with semi-transparent backdrop
- Header: Recipient username, close button
- Message list area: Scrollable container with messages
- Message input area: Text input + Send button
- Uses `useMessagesQuery(username)` to load message history
- Uses `useSendMessageMutation()` to send messages
- Uses `useMarkConversationReadMutation()` when opened
- Uses `useDraftMessage(username)` to preserve unsent text
- Auto-scrolls to bottom when new messages arrive
- Shows loading state while fetching history
- Shows empty state: "Start a conversation with [username]"
- Shows error state with retry button
- Create separate sub-components:
  - `ChatModalHeader.jsx` - title bar with username and close button
  - `ChatMessageList.jsx` - scrollable message list
  - `ChatMessageItem.jsx` - individual message bubble (left for received, right for sent)
  - `ChatMessageInput.jsx` - text input with send button
  - `ChatModalError.jsx` - error display with retry

**Task**: Create ChatMessage component  
**Location**: `frontend/src/components/ChatMessage.jsx`  
**Details**:
- Props: `message`, `isSender` (boolean)
- Display message bubble with appropriate alignment (left/right)
- Show sender avatar and username (on left if received)
- Show message body text
- Show timestamp below message
- Show delivery status indicator (sending/sent/delivered) for sent messages
- Style differently for sent vs received messages

### 6.4 Create Messages Inbox Page

**Task**: Create Messages/Inbox page component  
**Location**: `frontend/src/pages/Messages.jsx`  
**Details**:
- Full page listing all conversations
- Uses `useConversationsQuery()` hook
- Shows loading state while fetching
- Shows empty state: "No messages yet. Visit a user's profile to start a conversation."
- List of conversation items showing:
  - Other participant's avatar and username
  - Last message preview (truncated to 50 chars)
  - Last message timestamp (formatted: "2 hours ago", "Yesterday", "Jan 15")
  - Unread count badge if > 0
- Click conversation item opens ChatModal for that user
- Order by most recent activity (UpdatedAt descending)
- Create separate sub-component:
  - `ConversationListItem.jsx` - single conversation in list

**Task**: Create ConversationListItem component  
**Location**: `frontend/src/components/ConversationListItem.jsx`  
**Details**:
- Props: `conversation`, `onClick`
- Displays conversation preview with hover effect
- Bold text if unread messages exist
- Shows unread badge with count
- Truncates long message previews with ellipsis
- Formats timestamp using date-fns or similar

### 6.5 Create Shared UI Components

**Task**: Create UnreadBadge component  
**Location**: `frontend/src/components/UnreadBadge.jsx`  
**Details**:
- Props: `count` (number)
- Displays circular badge with count
- Hidden if count is 0
- Red background, white text
- Auto-adjusts size for 1-2 digits vs 3+ digits (shows "99+" for 100+)

**Task**: Create MessageStatus component  
**Location**: `frontend/src/components/MessageStatus.jsx`  
**Details**:
- Props: `status` (enum: "sending", "sent", "delivered", "failed")
- Shows icon/text indicator for message delivery status
- Sending: spinner icon
- Sent: single checkmark
- Delivered: double checkmark
- Failed: error icon with "Click to retry" text

**Task**: Create ConnectionBanner component  
**Location**: `frontend/src/components/ConnectionBanner.jsx`  
**Details**:
- Props: `connectionStatus`
- Displays banner at top of chat modal when connection is lost
- Shows: "Connection lost - Reconnecting..." with retry button
- Auto-dismisses when connected
- Uses `useSignalRConnection` hook for status

---

## Phase 7: Frontend - Routing and Integration

### 7.1 Add Routes

**Task**: Update App.jsx with new routes  
**Location**: `frontend/src/app/App.jsx`  
**Details**:
- Add route: `<AuthRoute path="/messages" element={<Messages />} />`
- Import Messages page component

### 7.2 Create Chat Context Provider

**Task**: Create ChatContext for global chat state  
**Location**: `frontend/src/contexts/ChatContext.jsx`  
**Details**:
- Manages global chat state:
  - Currently open chat modal (if any)
  - SignalR connection status
  - Pending message sends
- Provides functions:
  - `openChat(username)` - opens chat modal with user
  - `closeChat()` - closes current chat modal
- Wraps App in `ChatProvider`
- Uses `useSignalRConnection` hook internally

### 7.3 Integrate SignalR Event Handlers

**Task**: Wire up SignalR real-time updates in ChatContext  
**Location**: `frontend/src/contexts/ChatContext.jsx`  
**Details**:
- Register SignalR event handlers:
  - `onMessageReceived`: Call `useMarkMessageDeliveredMutation`, invalidate messages query, play notification sound (optional)
  - `onMessageDelivered`: Update message status in query cache
  - `onUnreadCountChanged`: Invalidate unread count query
- Use `useQueryClient` from React Query to manipulate query cache
- Handle edge cases: message received for currently open conversation vs other conversations

---

## Phase 8: Frontend - Styling and Polish

### 8.1 Add CSS for Chat Components

**Task**: Create chat-specific styles  
**Location**: `frontend/src/components/Chat.css`  
**Details**:
- Modal overlay: semi-transparent black backdrop
- Modal container: white box, centered, max-width 600px, max-height 80vh
- Message bubbles: rounded corners, different colors for sent (blue) vs received (gray)
- Message alignment: sent messages on right, received on left
- Scrollable message area with smooth scroll behavior
- Input area: sticky at bottom, flex layout
- Unread badge: circular, red, white text
- Connection banner: yellow background, at top of modal
- Follow existing Conduit CSS patterns and conventions

### 8.2 Add Responsive Design

**Task**: Make chat components mobile-friendly  
**Location**: `frontend/src/components/Chat.css`  
**Details**:
- Modal takes full screen on mobile (< 768px)
- Conversation list: stack items vertically, full width
- Message bubbles: adjust max-width for narrow screens
- Input area: adjust button size for touch targets
- Use existing breakpoints from Conduit CSS

---

## Phase 9: Testing and Validation

### 9.1 Backend Unit Tests (Optional - if tests exist)

**Task**: Create unit tests for message handlers  
**Location**: `backend/tests/Conduit.IntegrationTests/Features/Messages/`  
**Details**:
- Test Send.Handler: creates conversation, saves message, broadcasts via SignalR
- Test List.Handler: returns messages for authorized user, denies access to non-participants
- Test MarkAsRead.Handler: updates read status correctly
- Follow existing test patterns from Comments tests

### 9.2 Frontend Component Tests (Optional - if tests exist)

**Task**: Create tests for chat components  
**Location**: `frontend/src/components/__tests__/`  
**Details**:
- Test ChatModal: renders messages, sends message, handles errors
- Test ConversationListItem: displays correctly, handles click
- Use existing test setup (Jest/React Testing Library if available)

### 9.3 Manual Testing Checklist

**Task**: Comprehensive manual testing  
**Details**:
- **Authentication**:
  - [ ] Unauthenticated users cannot access Messages page
  - [ ] Unauthenticated users do not see Message button on profiles
  - [ ] Unauthenticated users do not see Messages link in navbar
- **Message Sending**:
  - [ ] User can send message from profile page
  - [ ] Message appears immediately in chat modal (optimistic update)
  - [ ] Message status changes from "sending" to "sent" to "delivered"
  - [ ] Empty messages cannot be sent (validation error)
  - [ ] Very long messages send and display correctly
  - [ ] Cannot send message to self
- **Real-time Delivery**:
  - [ ] Open two browsers (User A and User B)
  - [ ] User A sends message to User B
  - [ ] User B sees message appear in real-time without refresh
  - [ ] Delivery status updates in real-time for User A
  - [ ] Works across multiple tabs for same user
- **Conversations List**:
  - [ ] All conversations appear in Messages inbox
  - [ ] Conversations ordered by most recent activity
  - [ ] Last message preview displays correctly
  - [ ] Unread counts display correctly
  - [ ] Clicking conversation opens chat modal with that user
- **Unread Tracking**:
  - [ ] Unread badge appears on Messages nav link with correct count
  - [ ] Opening conversation marks messages as read
  - [ ] Unread count updates in real-time when new message arrives
  - [ ] Unread count updates across multiple tabs/devices
- **Draft Persistence**:
  - [ ] Type unsent message in chat modal
  - [ ] Close modal
  - [ ] Reopen chat with same user
  - [ ] Draft text is restored
  - [ ] Sending message clears draft
- **Empty States**:
  - [ ] New conversation shows welcome message
  - [ ] Messages inbox with no conversations shows empty state message
- **Error Handling**:
  - [ ] Failed message send shows retry option
  - [ ] Lost SignalR connection shows reconnecting banner
  - [ ] Clicking retry on failed message resends successfully
  - [ ] Cannot access messages for conversation user is not participant in (403 error)
- **Edge Cases**:
  - [ ] User deletes account → messages show "[Deleted User]" (if implemented)
  - [ ] User changes username → messages update to show new username
  - [ ] Rapid message sending (10+ messages quickly) works without errors
  - [ ] Conversation with 100+ messages loads and scrolls correctly
  - [ ] Offline user receives queued messages when they log back in

---

## Phase 10: Documentation

### 10.1 Update API Documentation

**Task**: Document new endpoints in Swagger  
**Location**: `backend/src/Conduit/` (Swagger auto-generates from controllers)  
**Details**:
- Ensure all new endpoints have XML comments for Swagger
- Document request/response schemas
- Document authorization requirements
- Test Swagger UI to verify endpoints appear correctly

### 10.2 Update README Files

**Task**: Document chat feature usage  
**Location**: `frontend/readme.md` and `backend/readme.md`  
**Details**:
- Add section describing real-time chat feature
- Explain SignalR setup and configuration
- Document environment variables if any are added
- List new npm packages and NuGet packages

---

## Implementation Order Summary

**Recommended implementation sequence** (each step builds on previous):

1. **Backend Domain & Database** (Phase 1) - Foundation
2. **Backend SignalR Infrastructure** (Phase 2) - Real-time capability
3. **Backend Features - Core CRUD** (Phase 3) - Business logic
4. **Frontend SignalR Client** (Phase 4) - Real-time client
5. **Frontend Hooks** (Phase 5) - Data layer
6. **Frontend Components** (Phase 6) - UI layer
7. **Frontend Routing** (Phase 7) - Navigation integration
8. **Styling** (Phase 8) - Visual polish
9. **Testing** (Phase 9) - Validation
10. **Documentation** (Phase 10) - Final touch

---

## Technical Considerations

### Performance
- **Message pagination**: For conversations with 100+ messages, implement pagination (load most recent 50, load more on scroll)
- **SignalR scaling**: For production, consider Redis backplane for scaling across multiple servers
- **Connection pooling**: SignalR manages connection pooling automatically
- **Query optimization**: Use EF Core `.Include()` efficiently to avoid N+1 queries

### Security
- **Authorization**: All message and conversation endpoints verify user is participant
- **XSS prevention**: Frontend sanitizes message body before rendering (React handles this by default)
- **Rate limiting**: Consider adding rate limiting for message sending (e.g., max 100 messages per minute per user)
- **JWT expiration**: Handle token refresh for long-lived SignalR connections

### Error Handling
- **SignalR disconnection**: Auto-reconnect with exponential backoff
- **Message send failures**: Store failed messages locally and retry on reconnect
- **Database deadlocks**: Implement retry logic for high concurrency scenarios
- **Validation errors**: Display user-friendly error messages

### Known Limitations (from spec)
- No message editing or deletion
- No conversation deletion or archiving
- No group chats (only 1-on-1)
- No file/image sharing
- No typing indicators or read receipts
- No message search

---

## Dependencies

### Backend NuGet Packages (may already be included)
- `Microsoft.AspNetCore.SignalR` - Real-time communication
- `Microsoft.EntityFrameworkCore` - Already installed
- `MediatR` - Already installed
- `FluentValidation` - Already installed

### Frontend npm Packages (new)
- `@microsoft/signalr` - SignalR client library
- `date-fns` (optional) - Date formatting for timestamps

### Frontend npm Packages (already installed)
- `react-query` - Data fetching and caching
- `react-router-dom` - Routing
- `axios` - HTTP client

---

## Risk Mitigation

**Risk**: SignalR connection issues in production  
**Mitigation**: Implement robust reconnection logic, fallback to polling if WebSockets fail, display connection status to user

**Risk**: Database schema changes breaking existing data  
**Mitigation**: Test with existing database file, implement migration strategy if needed

**Risk**: High message volume overwhelming database  
**Mitigation**: Add database indexing on frequently queried columns (ConversationId, SenderId, CreatedAt), consider message archival strategy

**Risk**: Real-time updates not reaching all connected clients  
**Mitigation**: Test with multiple browser tabs/devices, implement heartbeat/ping mechanism to detect dead connections

**Risk**: Complexity of managing both HTTP and SignalR  
**Mitigation**: Keep separation of concerns: SignalR only for notifications, all data operations through REST API and MediatR

---

## Success Criteria

The implementation will be considered complete when:

✅ User can send and receive messages in real-time  
✅ Messages persist across sessions  
✅ Conversations appear in Messages inbox with unread counts  
✅ Unread badge appears on navigation bar  
✅ Draft messages persist when modal is closed  
✅ All validation rules are enforced  
✅ Error states are handled gracefully  
✅ UI is responsive and follows Conduit design patterns  
✅ All manual testing checklist items pass  
✅ Backend builds without errors  
✅ Frontend builds without errors  

---

## Next Steps

1. Review this plan with stakeholders
2. Address any questions or concerns
3. Begin implementation starting with Phase 1
4. Implement incrementally, testing each phase before proceeding
5. Create pull requests for review at major milestones (e.g., after Phase 3, after Phase 6)

