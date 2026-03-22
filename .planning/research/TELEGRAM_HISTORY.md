# Telegram Channel History Research

**Project:** LetterboxdToTelegram
**Researched:** 2024-05-24
**Overall confidence:** HIGH

## Overview

The standard **Telegram Bot API** (the HTTP-based API used by most libraries like `Telegram.Bot`) does **not** support fetching the message history of a channel or chat. It is designed for real-time interaction where the bot receives new messages as they occur.

To "seed" a database from an existing channel, you must use either the **Telegram Client API (MTProto)** or manual export methods.

---

## 1. Telegram Bot API (`Telegram.Bot`)

### Capabilities
- **Real-time only:** Can only receive messages via `getUpdates` (long-polling) or Webhooks.
- **Limited Window:** `getUpdates` only returns messages from the last 24 hours (or up to 100 messages). Once an update is confirmed, it is purged from Telegram's queue.
- **No History Method:** There is no `GetHistory` or `GetMessages` method in the standard Bot API.

### Conclusion
**Not suitable for seeding from existing channel history.**

---

## 2. Telegram Client API (MTProto)

The Client API (MTProto) is the "low-level" protocol used by official Telegram apps. It supports fetching full message history.

### Recommended Library: `WTelegramClient` (.NET)
`WTelegramClient` is a modern, high-performance .NET library for MTProto.

#### Using a Bot Token with MTProto
Interestingly, you can authorize a Bot Token using MTProto. This allows a bot to call methods like `Messages_GetHistory`.

**Requirements:**
1.  `api_id` and `api_hash` from [my.telegram.org](https://my.telegram.org).
2.  Your `bot_token`.
3.  The Bot must be an **Administrator** in the channel (if private) or the channel must be **Public**.

**Example (WTelegramClient):**
```csharp
using TL;
using WTelegram;

// 1. Setup Client
using var client = new Client(config => config switch {
    "api_id" => "YOUR_API_ID",
    "api_hash" => "YOUR_API_HASH",
    "bot_token" => "YOUR_BOT_TOKEN",
    _ => null
});

await client.LoginBotIfNeeded();

// 2. Resolve the Channel
var resolved = await client.Contacts_ResolveUsername("channel_username");
if (resolved.Chat is Channel channel)
{
    // 3. Fetch History
    // offset_id: 0 starts from latest. limit: max 100 per call.
    var history = await client.Messages_GetHistory(channel, limit: 100);

    if (history is Messages_ChannelMessages channelMsgs)
    {
        foreach (var msgBase in channelMsgs.messages)
        {
            if (msgBase is Message msg)
            {
                Console.WriteLine($"{msg.date}: {msg.message}");
                // Seed your database here
            }
        }
    }
}
```

#### Using a User Account (UserBot)
If the bot account is too restricted (e.g., it cannot see messages sent by other bots in the channel), you can log in as a standard **User Account**.
- **Pros:** Can see everything a human can see; no "bot-to-bot" visibility issues.
- **Cons:** Requires a phone number and manual verification code for the first login.

---

## 3. Alternative Seeding Strategies

### A. Manual Export (One-time Seed)
If you only need to seed the database once:
1.  Open **Telegram Desktop**.
2.  Go to the Channel -> Click `...` (top right) -> **Export chat history**.
3.  Select **JSON** format.
4.  Write a simple parser in C# to read the `result.json` and insert into your database.
    - *Note: This is often the fastest and safest way for a one-off migration.*

### B. Forward-Logging (Future History)
To ensure you never need to "seed" again:
1.  Add the bot to the channel as an Administrator.
2.  On every `channel_post` update, immediately save the movie info/message ID to your database.
3.  This builds your own local "history" index.

---

## 4. Comparison Table

| Method | Protocol | Account | Effort | Best For |
| :--- | :--- | :--- | :--- | :--- |
| **Bot API** | HTTP | Bot | Low | Real-time updates only |
| **MTProto (Bot Token)** | MTProto | Bot | Medium | Programmatic seeding (Admin) |
| **MTProto (User)** | MTProto | User | Medium | Robust scraping/seeding |
| **Manual Export** | N/A | Human | Very Low | One-time database setup |

---

## Recommendations for LetterboxdToTelegram

1.  **For Seeding:** Use **Manual Export (JSON)** from Telegram Desktop. It avoids the complexity of MTProto authentication and `api_id`/`api_hash` management for a task that is likely only done once.
2.  **For Maintenance:** Ensure the Bot saves every message it posts (or hears in the channel) to a local database to maintain a "seen" list and avoid duplicate posts from RSS.
3.  **If Automation is Required:** Use `WTelegramClient` with the Bot Token. It provides the cleanest programmatic access to `Messages_GetHistory` without needing a user phone number.

## Sources
- [Telegram Bot API Documentation](https://core.telegram.org/bots/api)
- [WTelegramClient GitHub/Docs](https://wiz0u.github.io/WTelegramClient/)
- [Telegram Core API - messages.getHistory](https://core.telegram.org/method/messages.getHistory)
