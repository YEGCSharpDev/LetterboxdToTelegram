# Telegram Bot API Research: Rich Movie Cards

**Researched:** May 2024
**API Version:** 7.3+
**Confidence:** HIGH (Official Docs & Recent Feature Updates)

## Overview
Telegram Bot API has evolved significantly in 2023-2024, offering more sophisticated ways to present "cards" (like movie entries) with rich media, better formatting, and interactive elements.

## 1. Core Formatting Options

| Feature | HTML | MarkdownV2 | Recommendation |
|---------|------|------------|----------------|
| **Ease of Use** | High (familiar tags) | Low (requires heavy escaping) | **HTML** |
| **Robustness** | High | Low (single unescaped char fails request) | **HTML** |
| **Nesting** | Supported | Supported | Both |
| **Spoilers** | `<tg-spoiler>text</tg-spoiler>` | `||text||` | Both |

### HTML Advantage for Movie Cards
Movie titles, descriptions, and URLs often contain characters like `.`, `-`, `!`, `(`, `)` which **must** be escaped in MarkdownV2. HTML only requires escaping `<`, `>`, and `&`.

## 2. Card Layout Strategies

### Strategy A: `sendPhoto` with Caption (Recommended)
This is the classic "Movie Card" look.
- **Media:** A high-res poster sent via `photo`.
- **Caption:** Up to **1024 characters**.
- **Pros:** Image is always shown; caption is attached to the image.
- **Cons:** 1024 character limit can be tight for long reviews + plot.

### Strategy B: `sendMessage` with Link Preview (New in v7.0)
- **Feature:** `link_preview_options` allows for:
    - `prefer_large_media`: Shows a large poster-sized image from the link.
    - `show_above_text`: Places the image above the text (looks like a card).
    - `url`: Can use a different URL for the preview than what's in the text.
- **Pros:** Text limit is **4096 characters**.
- **Cons:** If the link doesn't have proper OpenGraph tags, the preview might fail or look poor.

## 3. Advanced Features (v7.0 - v7.3)

### Link Preview Customization (v7.0)
You can now control exactly how links look:
```json
{
  "link_preview_options": {
    "is_disabled": false,
    "url": "https://www.imdb.com/title/tt1375666/",
    "prefer_large_media": true,
    "show_above_text": true
  }
}
```

### Reactions (v7.0)
Bots can now use `setMessageReaction` to add reactions to their own cards, or listen for user reactions. This is great for "Rate this movie" or "Add to Watchlist" functionality using emojis like 🍿 or ⭐.

### Custom Emojis (v7.1)
If the bot/channel has the necessary boosts/premium, custom emojis can be used in captions to brand the movie cards (e.g., specific genre icons or studio logos).

### Inline Keyboards
Always use `InlineKeyboardMarkup` for call-to-action buttons:
- "Watch Trailer" (URL button)
- "Full Review" (URL button)
- "Add to Watchlist" (Callback button)

## 4. Implementation Pitfalls

- **Character Limits:** `sendPhoto` caption is strictly 1024 chars. If the combined Plot + Review exceeds this, the request will fail.
    - *Mitigation:* Truncate the plot or review and add a "... [Read More](link)" link.
- **Media Sizes:** Telegram compresses photos. For best quality movie posters, ensure the source URL is high-res but under 10MB.
- **Escaping:** Even in HTML, you MUST escape user-generated content (like Letterboxd reviews) to prevent malformed tags.

## 5. Comparison: Current Code vs. Modern API

| Feature | Current Implementation | Modern Recommendation |
|---------|------------------------|-----------------------|
| **Parse Mode** | MarkdownV2 | HTML |
| **Escaping** | Manual `EscapeForMarkdown` | Standard HTML Entity Escaping |
| **Link Previews** | N/A (SendPhoto only) | Consider `LinkPreviewOptions` for text-heavy reviews |
| **Interactivity** | Text links only | Inline Buttons + Reactions |

## Sources
- [Telegram Bot API - Official Docs](https://core.telegram.org/bots/api)
- [Bot API 7.0 Announcement](https://core.telegram.org/bots/api#december-31-2023)
- [Bot API 7.3 Announcement](https://core.telegram.org/bots/api#may-6-2024)
