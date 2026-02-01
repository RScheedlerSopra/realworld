# Real-Time Chat Feature Specification

## 1. Goal (Why)

### Problem Statement
Users cannot communicate privately with other users on the platform. All interaction is limited to public comments on articles.

### Who Feels It Most
Authenticated users who want to have private conversations with authors they follow or users whose content they engage with.

### Desired Outcome
Users can initiate and maintain private, real-time conversations with other users directly on the platform.

### Success Criteria
- Users can successfully send and receive messages in real-time
- Message delivery latency is under 2 seconds
- Chat history persists across sessions

### Non-Goals / Out of Scope
- Group chat or multi-user conversations
- File/image sharing in chat
- Video or voice chat
- Message reactions or rich text formatting (markdown, emojis, bold/italic)
- Typing indicators ("User is typing...")
- Read receipts ("Seen at..." timestamps)
- Online/offline status indicators (green dot showing user is active)
- Message editing after sending
- Message deletion (for sender or recipient)
- Conversation deletion or hiding
- Message search functionality
- Chat notifications (email or push notifications)
- Blocking users from initiating chats
- Chat moderation, reporting, or flagging
- Message character limits or rate limiting
- Message archival or retention policies
- Conversation folders or organization

## 2. Users & Access (What)

### Target Users/Personas
Authenticated users who want to communicate privately with other platform members.

### Eligibility
Only logged-in (authenticated) users can access chat functionality.

### Visibility
- Unauthenticated users: Cannot see chat interface or message other users
- Authenticated users: Can see chat entry point on all user profiles except their own

### Where It Lives
- Chat entry point: User profile pages (/@username)
- Chat interface: Modal overlay that appears on top of current page
- Main navigation: "Messages" link in nav bar leading to inbox/messages page

### How to Reach It
- Primary entry: "Message" button on another user's profile page
- Secondary entry: Navigation bar link to Messages/Inbox page listing all conversations
- Unread badge: Notification badge on Messages nav link showing total unread count

## 3. User Experience & Flow (What)

### Happy Path
1. User navigates to another user's profile page (/@username)
2. User clicks "Message" button on profile
3. Chat modal overlay opens showing conversation with that user
4. If first conversation, user sees welcome message "Start a conversation with [username]"
5. User types message in input field at bottom of modal
6. User presses Enter or clicks Send button
7. Message appears in conversation immediately with "sending" status
8. Message status updates to "sent" then "delivered" when confirmed
9. Recipient sees message appear in real-time (if online) or when they next open the chat
10. Users can exchange messages back and forth
11. User closes chat modal by clicking X or clicking outside modal
12. User can access all conversations via Messages link in navigation bar
13. Messages page shows list of all conversations with unread badges
14. Clicking a conversation opens the chat modal for that user

### Key Screens/States
- **User profile page**: Shows "Message" button (not shown on own profile)
- **Chat modal**: Overlay showing conversation history, message input, close button
- **Messages inbox page**: List of all conversations with preview, timestamps, unread counts
- **Navigation bar**: Messages link with unread badge showing total unread count
- **Message states**: Sending indicator, sent confirmation, delivered confirmation

### Empty State
- **New conversation**: Show welcome message "Start a conversation with [username]" above empty message area with active input field ready
- **Inbox with no conversations**: Show "No messages yet. Visit a user's profile to start a conversation."

### Loading State
- **Opening chat modal**: Show skeleton/spinner while loading message history from server
- **Sending message**: Show message in chat with "sending..." indicator
- **Message sent**: Update to "sent" indicator once server confirms
- **Message delivered**: Update to "delivered" when recipient's client acknowledges (if online)

### Error State
- **Message send failure**: Show error indicator on message with "Failed to send - Click to retry" option
- **Connection lost**: Show persistent banner at top of chat "Connection lost - Reconnecting..." with retry button
- **Chat load failure**: Show error message "Failed to load conversation" with retry button
- **Inbox load failure**: Show error message "Failed to load messages" with retry button

### Cancellation/Escape
- User can close chat modal at any time by clicking X button or clicking outside modal
- Unsent text in input field is preserved when modal is closed and reopened for same conversation
- No confirmation needed to close chat (all sent messages are already persisted)
- User can navigate away from any chat screen without losing data

## 4. Functional Requirements (What)

### User Actions
- Open chat with another user from their profile (opens modal)
- Send text messages in a conversation
- View message history in a conversation
- View list of all conversations in Messages inbox
- View unread count for each conversation and total unread count in nav badge
- Close/dismiss chat modal
- Retry sending failed messages
- Mark conversation as read by opening it
- Preserve draft message in input field when closing and reopening same conversation

### System Behaviors
- Deliver messages in real-time via SignalR/WebSockets to connected recipients
- Store all messages persistently in database
- Queue messages for offline users and deliver when they reconnect
- Update message delivery status (sending → sent → delivered)
- Track unread messages per conversation per user
- Update unread counts in real-time when new messages arrive
- Mark messages as read when user opens conversation
- Order messages chronologically by send time (oldest first)
- Order conversations in inbox by most recent activity (newest first)
- Open existing conversation when user clicks Message button on same user's profile again
- Show current username of sender on all messages (updates retroactively if user changes username)

### Inputs
**Required:**
- Message body (text content, no maximum length)
- Recipient user (selected via profile page or conversation list)

**Optional:**
- None

### Outputs
- Real-time message delivery to recipient via SignalR/WebSockets
- Visual delivery status indicators (sending, sent, delivered)
- Persistent message history accessible to both participants
- Messages inbox page showing all conversations with unread badges
- Unread count badge in navigation bar
- Draft message preservation in input field per conversation

## 5. Rules, Constraints, and Edge Cases (What)

### Validation Rules
- Message body cannot be empty or only whitespace
- Message body has no maximum length limit
- Cannot send messages to self
- Must be authenticated to send/receive messages
- No rate limiting on message sending

### Business Rules
- Conversations are bidirectional (both users can send messages)
- Anyone can initiate a conversation with any other user (no following required, no blocking)
- Message history visible to both participants indefinitely
- Messages cannot be deleted or edited after sending
- Conversations cannot be deleted (permanent record)
- Messages ordered chronologically by creation timestamp (oldest first in chat)
- Conversations ordered by most recent activity (newest first in inbox)
- No limit on message history length (persists indefinitely)
- No limit on number of active conversations per user
- Unread messages tracked per conversation per user
- Opening conversation marks all messages as read for that user
- Draft message text preserved per conversation when modal closed
- Message delivery status shown: sending → sent → delivered
- Messages queued for offline users and delivered when they reconnect
- Username changes reflected retroactively in all messages

### Permissions Rules
- Both participants have equal read/write access to conversation
- Users can only view conversations they are participants in
- Users cannot delete their own messages or conversations
- No admin or moderator access to private chats (fully private between participants)
- Only authenticated users can access any chat functionality

### Edge Cases
- **User initiates chat with inactive user**: Conversation created normally, messages queued until recipient logs in
- **User receives message while offline**: Message stored and delivered when they next connect
- **Concurrent messages**: Both users can send messages simultaneously without conflict
- **User deletes account**: Messages remain visible with sender shown as "[Deleted User]"
- **Very long messages**: No character limit enforced, UI should handle text wrapping gracefully
- **Rapid message sending**: No rate limiting, system should handle rapid-fire messages
- **User changes username**: All existing messages updated to show new username
- **Connection loss during send**: Message marked as failed with retry option
- **Multiple tabs/devices**: User can have chat open in multiple browser tabs, should stay in sync
- **First-time conversation**: Show welcome message before any messages exchanged

### Safety Checks
- No deletion functionality, so no confirmation dialogs needed
- No safety checks for sending individual messages (messages cannot be unsent)
- Retry confirmation not needed for failed sends (user can choose to retry or not)

## 6. Data & Lifecycle (What)

### Data Created/Updated

**New Entities:**

**Message**
- **id**: Unique identifier (GUID or auto-increment)
- **conversationId**: Reference to parent conversation
- **senderId**: Reference to user who sent the message
- **body**: Text content of message (unlimited length)
- **createdAt**: Timestamp when message was created
- **deliveredAt**: Timestamp when message was delivered to recipient (null if not yet delivered)

**Conversation**
- **id**: Unique identifier (GUID or auto-increment)
- **participant1Id**: Reference to first user
- **participant2Id**: Reference to second user
- **createdAt**: Timestamp when conversation was created
- **updatedAt**: Timestamp of last message activity

**ConversationReadStatus** (tracking unread per user)
- **conversationId**: Reference to conversation
- **userId**: Reference to user
- **lastReadAt**: Timestamp when user last opened this conversation
- **unreadCount**: Computed count of messages received after lastReadAt

### Ownership
- **Message**: Owned by sender user, readable by both conversation participants
- **Conversation**: Co-owned by both participants, equal access rights
- **ReadStatus**: Owned by individual user for their own read tracking

### Retention
- Messages persist indefinitely (no expiration or archival)
- Conversations persist indefinitely as long as database exists
- Read status tracked indefinitely
- No automatic cleanup or purging policies

### Deletion Behavior

**Message Deletion:**
- Messages cannot be deleted by users (permanent once sent)
- No soft delete or hide functionality
- Messages remain visible to both participants forever

**Conversation Deletion:**
- Conversations cannot be deleted by users
- No hide or archive functionality
- All conversations permanently visible to both participants

**User/Account Deletion:**
- When user account is deleted, their messages remain in database
- Sender shown as "[Deleted User]" for all their messages
- Conversations remain accessible to the other participant
- Soft delete approach: User record marked as deleted but messages preserved
- Read status records for deleted user can be removed

### History/Audit Expectations
- Full message history visible to both participants indefinitely
- Messages display current username of sender (updated automatically if user changes username)
- Timestamps show when each message was sent (createdAt)
- Delivery status shown per message (sending, sent, delivered)
- No editing history needed (messages cannot be edited)
- No deletion history needed (messages cannot be deleted)
- Conversation list ordered by most recent activity (updatedAt)
- No requirement to audit or log who viewed conversations

### Real-Time Synchronization (Technical)
- **Technology**: SignalR (ASP.NET Core) for WebSocket-based real-time communication
- **Connection**: Establish SignalR connection when user authenticates
- **Message flow**: Sender → SignalR Hub → Recipient's connected client(s)
- **Offline handling**: Messages stored in database and delivered when recipient reconnects
- **Status updates**: Sent status when saved to DB, delivered status when recipient client acknowledges
- **Multi-device**: User can be connected from multiple tabs/devices, all receive messages
- **Reconnection**: Client automatically reconnects if connection drops, fetches missed messages

---

## Summary

This feature enables authenticated users to have private, real-time conversations with other platform members. The chat system uses SignalR for WebSocket-based real-time messaging, with a modal overlay interface accessible from user profiles and a dedicated Messages inbox page. All messages persist indefinitely with no deletion capabilities, ensuring a complete conversation history. The system tracks unread messages and provides delivery status indicators (sending/sent/delivered) for optimal user experience.

