# Feature Landscape

**Domain:** Letterboxd Activity Broadcasting
**Researched:** January 2025

## Table Stakes

Features users expect for this integration.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Film Title & Year | Identifies the movie. | Low | Available in RSS. |
| Rating (Stars) | Shows the user's opinion. | Low | 0.5 to 5.0 range. |
| Poster Image | High visual engagement. | Med | Requires HTML parsing. |
| Review Text | Shares the user's thoughts. | Med | Needs HTML cleaning. |
| Multi-user monitoring | Supports small communities. | Med | Currently limited to split env var. |

## Differentiators

Features that set this tool apart.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| "Liked" Heart Icon | Adds "Like" status to post. | Low | Custom XML tag available. |
| "Rewatch" Status | Adds "↺" or rewatch text. | Low | Custom XML tag available. |
| High-Res Posters | Better visuals on Telegram. | Low | URL string manipulation. |
| Markdown Formatting | Better Telegram message styling. | Low | Bold titles, italic reviews. |
| TMDB Links/Rating | Supplemental data from TMDB. | High | Requires separate API call. |

## Anti-Features

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| Real-time push | Letterboxd has no webhooks. | Use polling with reasonable intervals. |
| Full Diary Import | 50-item limit in RSS. | Direct users to CSV export for one-off imports. |

## MVP Recommendation

Prioritize:
1. Robust multi-item RSS polling (don't miss movies).
2. "Liked" and "Rewatch" status icons.
3. Clean, high-resolution poster extraction.
4. Basic Markdown formatting for Telegram messages.

Defer: TMDB integration, complex multi-user management dashboards.

## Sources
- Letterboxd RSS spec
- Telegram Bot API (Photo captions, MarkdownV2)
