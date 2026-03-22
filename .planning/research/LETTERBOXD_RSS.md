# Letterboxd RSS Feed Research

**Domain:** Social Film Tracking / RSS Integration
**Researched:** January 2025
**Overall Confidence:** HIGH

## Overview
Letterboxd provides RSS feeds for user activity, diary entries, and lists. For a "Letterboxd to Telegram" integration, the primary feed of interest is the user diary or activity feed.

**Feed URL:** `https://letterboxd.com/[username]/rss/`
**Namespace:** `xmlns:letterboxd="http://letterboxd.com/"`

---

## RSS Item Structure

Each `<item>` in the feed contains standard RSS fields and custom Letterboxd metadata.

### Standard Fields
| Element | Content | Example |
| :--- | :--- | :--- |
| `<title>` | Film Title, Year - Rating | `The Substance, 2024 - ★★★★½` |
| `<link>` | URL to the Letterboxd entry | `https://letterboxd.com/user/film/the-substance/` |
| `<pubDate>` | Date/time published | `Wed, 22 Jan 2025 10:00:00 +0000` |
| `<guid>` | Unique identifier | (Usually same as link) |
| `<dc:creator>` | Letterboxd username | `johndoe` |
| `<description>` | HTML snippet (Poster + Review) | See "HTML Parsing" section |

### Custom `letterboxd` Namespace Fields
These fields are highly reliable for programmatic use.

| Tag | Purpose | Possible Values |
| :--- | :--- | :--- |
| `<letterboxd:filmTitle>` | Clean movie title | `The Substance` |
| `<letterboxd:filmYear>` | Release year | `2024` |
| `<letterboxd:memberRating>` | Numerical rating | `0.5` to `5.0` |
| `<letterboxd:liked>` | "Heart" status | `Yes` or `No` |
| `<letterboxd:rewatch>` | Rewatch status | `Yes` or `No` |
| `<letterboxd:watchedDate>` | Date watched | `2025-01-21` |

---

## HTML Parsing (Film Metadata)

The `<description>` tag contains escaped HTML. To extract the poster and review text, this content must be parsed as HTML.

### Structure of `<description>`
```html
<p><img src="https://a.ltrbxd.com/resized/sm/upload/.../poster-0-150-0-225-crop.jpg"/></p> 
<p>Watched on Monday Jan 20, 2025.</p>
<p>Review text goes here if the user wrote one...</p>
```

### Poster Extraction
1. **Target:** The first `<img>` tag in the description.
2. **Attribute:** The `src` attribute.
3. **Resolution:** The default URL is often small (`150x225`). 
   - **Medium:** Replace `0-150-0-225-crop.jpg` with `0-230-0-345-crop.jpg`.
   - **Large:** Replace `0-150-0-225-crop.jpg` with `0-1000-0-1500-crop.jpg`.
   - *Note: These are unofficial URL patterns and may change.*

### Rating Extraction
While `<letterboxd:memberRating>` is preferred, the `<title>` contains Unicode stars:
- `★` (U+2605) = 1 star
- `½` (U+00BD) = 0.5 stars

---

## Common Technical Challenges (2024-2025)

### 1. Leading Blank Line (XML Validation)
In early 2025, Letterboxd feeds frequently included a leading blank line before the `<?xml` declaration.
- **Symptom:** "XML or text declaration not at start of entity".
- **Fix:** Always `.Trim()` or strip leading whitespace from the response string before parsing with an XML/RSS library.

### 2. Item Limit
Feeds are capped at the **50 most recent items**. 
- **Implication:** If a user logs 60 films in one go (e.g., importing a history), only the latest 50 will trigger in the RSS poller.

### 3. Rate Limiting
Letterboxd does not have a public API rate limit for RSS, but aggressive polling (e.g., every minute) from the same IP may result in temporary blocks. A poll interval of 5-15 minutes is recommended.

### 4. HTML Escaping
The content in `<description>` is often double-escaped or CDATA wrapped. Parsers must handle `&lt;` and `&gt;` correctly to reconstruct the HTML tree.

---

## Recommendations for Implementation
1. **Prefer Custom Tags:** Use the `letterboxd` namespace for Title, Year, Rating, and Liked status. Do not rely on regex parsing of the `<title>` string.
2. **HTML Parser:** Use `HtmlAgilityPack` (C#) or `BeautifulSoup` (Python) for the description to extract posters.
3. **Sanitization:** Strip HTML tags from the review text if sending to platforms that don't support full HTML (like some Telegram message modes).

## Sources
- Community discussions (Reddit r/letterboxd)
- Unofficial Letterboxd API documentation (GitHub)
- RSS validation reports (February 2025)
