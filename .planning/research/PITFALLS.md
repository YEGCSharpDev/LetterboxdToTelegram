# Domain Pitfalls

**Domain:** RSS & Telegram Automation
**Researched:** January 2025

## Critical Pitfalls

Mistakes that cause rewrites or major issues.

### Pitfall 1: Leading Blank Line (XML)
**What goes wrong:** Some Letterboxd feeds include a blank line before the `<?xml` declaration.
**Why it happens:** Technical glitch on Letterboxd side (reported early 2025).
**Consequences:** Standard XML parsers (`XmlDocument.LoadXml`, `SyndicationFeed.Load`) will crash.
**Prevention:** Use `.Trim()` on the raw string before parsing.
**Detection:** "XML or text declaration not at start of entity" exception.

### Pitfall 2: RSS 50-Item Limit
**What goes wrong:** RSS feeds only hold the last 50 items.
**Why it happens:** Hard limit set by Letterboxd to keep feeds lightweight.
**Consequences:** If a user logs 51 films between polls (or during a bulk import), some entries will be lost forever.
**Prevention:** Poll more frequently (e.g., every 5-15m). If an import is needed, use CSV export instead.

### Pitfall 3: Rating Unicode vs. Decimal
**What goes wrong:** Relying on the `<title>` for ratings (e.g., "★★★★½").
**Why it happens:** Title formatting is for humans, not for software.
**Consequences:** Parsing Unicode stars is fragile.
**Prevention:** Always use the `<letterboxd:memberRating>` tag for numerical values.

## Moderate Pitfalls

### Pitfall 4: Poster Resolution
**What goes wrong:** Using the low-resolution poster URL from the `<img>` tag.
**Prevention:** Use string replacement to get the higher resolution variant (e.g., replace `0-150-0-225-crop.jpg` with `0-1000-0-1500-crop.jpg`).

### Pitfall 5: Telegram Caption Limits
**What goes wrong:** Letterboxd reviews can be long, exceeding Telegram's 1024-character caption limit for photos.
**Prevention:** Truncate reviews with an ellipsis if they exceed ~1000 characters.

## Minor Pitfalls

### Pitfall 6: Namespace URI Mismatch
**What goes wrong:** Confusing `https://letterboxd.com/` with `http://letterboxd.com/` or `https://letterboxd.com/namespace/`.
**Prevention:** Verify the exact `xmlns:letterboxd` attribute in the live feed.

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| RSS Parsing | Leading blank line | String trim before parse. |
| Telegram Posting | Caption limit | Review text truncation. |
| Metadata Extraction | Poster resolution | URL string replacement logic. |

## Sources
- Community Reddit posts regarding Letterboxd RSS bugs
- Telegram API documentation (Caption limits)
- Project codebase (XML parsing logic)
