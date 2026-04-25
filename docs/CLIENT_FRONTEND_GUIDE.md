# Client website — frontend implementation guide

This guide complements **[CLIENT_API_REFERENCE.md](./CLIENT_API_REFERENCE.md)**. It describes how to assemble a **professional public site** using the client APIs: information architecture, key UI blocks, authentication, and safe rendering of CMS-driven content.

---

## 1. Tech assumptions

- **SPA** (React, Vue, Svelte, etc.) or **multi-page** app: either works if you follow the same data and routing rules.
- **HTTPS** in production for cookies (if used), OAuth, and JWT in memory/storage.
- **API base URL** from configuration (for example `VITE_API_BASE_URL`), never hard-coded per environment.

---

## 2. Site map (recommended pages)

| Route (example) | Purpose | Primary data source |
|-----------------|---------|------------------------|
| `/` | Home | `GET /home` |
| `/articles` | Article listing with filters | `GET /articles` + category `slug`↔`id` from home payload |
| `/articles/{slug}` | Article detail | `GET /articles/{slug}` |
| `/projects` | Projects hub (paged) | `GET /projects?page=&pageSize=&section=` |
| `/projects/{slug}` | Project detail | `GET /projects/{slug}` |
| `/categories/{slug}` | Optional dedicated category hub | Same list API with `categoryId` query |
| Legal / static | Privacy, terms | Static content or CMS later |

You do **not** need a separate “register” page for credentials: **Google sign-in is both login and first-time registration**.

---

## 3. Global layout

### 3.1 Navbar (top app bar)

**Data:** Prefer driving the main nav from **`GET /home` → `categories`** (active categories, ordered by `displayOrder`).

Suggested structure:

- **Logo / site name** → link to `/`
- **Category links** → `/articles?categoryId=<id>` or `/categories/<slug>` if you add a pretty route (resolve slug to id once from home or a small cache).
- **Articles** (optional) → `/articles`
- **Right side:**
  - If **no JWT**: button **“Sign in with Google”**
  - If **JWT valid**: avatar or email from token payload or from `TokenResponseDto.email` stored at login; menu with **Sign out** (delete token client-side).

**Behavior:**

- Highlight active category based on current route or query param.
- Sticky navbar on scroll is optional but common for “professional” feel.

### 3.2 Footer

**Data:** `GET /home` → `socialLinks` (filter client-side with `isActive === true`, sort by `displayOrder`).

Render a row of icon buttons or text links:

- Use `url` for `href`
- Use `label` or `platformKey` for accessible name (`aria-label`)
- If `iconUrl` is set, show custom icon; otherwise map `platformKey` to a built-in icon set (facebook, x, instagram, youtube, etc.)

### 3.3 Loading and errors

- **Skeleton** or shimmer for home and article list while fetching.
- **Toast** or inline alert for API errors; read `message` from `{ "message": "..." }` when present (auth and comments).
- **401** on protected actions: prompt sign-in or refresh token flow (this API does not expose refresh; re-authenticate with Google when expired).

---

## 4. Home page — hero slider

**Data:** `GET /home` → `slides` (already filtered to `isActive` and ordered by `displayOrder`).

### 4.1 Rendering rules

For each slide:

1. **Background** — `backgroundImageUrl` is usually a **root-relative** path (for example `/uploads/...`). Prefix with your API or CDN origin if the SPA is on another host.
2. **Text** — `title`, `subtitle` as headings and tagline.
3. **Rich content** — `contentHtml` may contain HTML from the CMS. Render only after sanitization (see **Section 8**).
4. **Call-to-action / click**
   - `linkTargetType === 0` (`None`) — no navigation; optional “read more” hidden.
   - `linkTargetType === 1` — link to `externalUrl`; if `openInNewTab`, use `target="_blank"` and `rel="noopener noreferrer"`.
   - `linkTargetType === 2` — internal article: route to `/articles/{articleSlug}`. If `articleSlug` is null, treat as no link (API may strip unpublished targets).

### 4.2 Carousel behavior

- Auto-advance every 5–8s with pause on hover and on focus (keyboard accessibility).
- Dots or thumbnails per slide; swipe on mobile.
- Prefer `prefers-reduced-motion` to reduce or disable auto-advance.

---

## 5. Home page — category section

**Data:** `categories` from `GET /home`.

Each **Category card** can show:

- `name`, `description`
- `backgroundImageUrl` as card background or thumbnail
- Link to `/articles?categoryId=<id>` or your slug-based route

Sort is already server-ordered; keep `displayOrder` if you re-sort client-side after edits.

---

## 6. Home page — articles strip (horizontal + DB pagination)

**Initial data:** `GET /home` → **`articlesSection`** (`PagedResult<ArticleSummaryDto>`). The server returns **page 1** with **`totalCount`**, **`page`**, and **`pageSize`** so you can paginate without guessing.

**Load more / arrows:** call **`GET /api/client/v1/home/articles?page=&pageSize=&categoryId=`** (same JSON shape as **`GET /articles`**). Increment or decrement `page` when the user clicks **right / left** arrows or “next page” on the strip.

**UI recommendations**

- Render a **horizontally scrollable** row of cards (CSS `overflow-x: auto` with scroll-snap, or a small carousel).
- Add **left/right arrow** buttons that either:
  - **Scroll the strip** (`scrollBy({ left: ±containerWidth, behavior: 'smooth' })`) when one page of items is already loaded, or
  - **Fetch the next/previous page** from the API when the strip reaches the end or when arrows mean “next page” (use `totalCount` to disable next on the last page).
- Show **page indicator** optional: `page` / `ceil(totalCount / pageSize)`.

Card content:

- Cover: `coverImageUrl`
- Title → link to `/articles/{slug}`
- Meta: `categoryName`, `publishedAt`, optional counts

For the **main articles archive** (full page), use **`GET /articles`** (section 7).

---

## 6b. Home page — projects strip (horizontal + DB pagination)

When `GET /home` returns **`projectsSection`**, it includes **`page`**, **`pageSize`**, **`totalCount`**, and **`items`** (first page only).

- **More slides:** `GET /api/client/v1/home/projects?page=&pageSize=` (same response shape as `projectsSection`).
- **Horizontal UI:** same pattern as articles: scroll container + optional **left/right** arrows; fetch next/prev **page** when arrows advance beyond loaded items.
- **Detail page:** `GET /api/client/v1/projects/{slug}` (unchanged).

---

## 6c. Projects hub listing page

**Data:** `GET /projects?page=&pageSize=&section=` where **`section`** is optional **1**, **2**, or **3** to filter by initiative type.

Use **`totalCount`**, **`page`**, and **`pageSize`** for numbered pagination or infinite scroll. Each **`items[]`** entry includes **`section`** and **`sectionTitle`** so you can group visually in the UI even when `section` filter is omitted.

---

## 7. Article listing page

**Data:** `GET /articles?page=&pageSize=&categoryId=`

### UX

- **Pagination:** `totalCount`, `page`, `pageSize` drive page controls. Disable “next” when `page * pageSize >= totalCount`.
- **Category filter:** Build a selector from home `categories` (id + name). Changing filter resets `page` to 1.
- **Card layout:** image, title, summary (truncate), metadata row.

---

## 8. Article detail page

**Data:** `GET /articles/{slug}` (optional `Authorization` for correct `likedByCurrentUser`).

### 8.1 HTML safety (`bodyHtml`, slider `contentHtml`)

Treat all CMS HTML as **untrusted** for XSS unless you fully trust every editor.

- **Sanitize** before `dangerouslySetInnerHTML` / `v-html` / equivalent (for example DOMPurify with a strict allowlist).
- **Do not** execute inline scripts from content.
- Consider **CSP** (Content-Security-Policy) headers on your **frontend** host to block inline script.

### 8.2 Likes

- If user **not logged in**: show like count; button opens sign-in or shows tooltip.
- If **logged in**: toggle between **Like** (`POST .../like`) and **Unlike** (`DELETE .../like`). Use `likedByCurrentUser` from the detail response after load, and optimistically update count with rollback on error.

### 8.3 Comments

**Read:** `comments` is a **tree**: each node may have `replies` (same shape recursively).

- Render nested thread with indentation or thread lines.
- Show `userDisplayName` or “Member” fallback, `createdAt` localized.

**Write:** `POST /articles/{id}/comments` with JWT.

- After submit, show a clear state: **“Your comment was submitted and is awaiting moderation.”** because `isApproved` is `false` until staff approves in the dashboard.
- Optionally append the returned comment to a **“Pending”** local list so the author sees feedback (it will not appear in the public tree until approved).

**Replies:** set `parentCommentId` to the parent comment’s `id`.

---

## 9. Authentication and session (Google only)

### 9.1 Configuration checklist

1. **Google Cloud Console** — OAuth 2.0 Web client ID, authorized JavaScript origins = your **site** origin; authorized redirect URIs if your flow uses redirects.
2. **API appsettings** — `Authentication:Google:ClientId` must equal that **Web client** ID.
3. **Frontend** — use the same client ID when initializing Google Identity Services.

### 9.2 Typical SPA flow

1. User clicks **Sign in with Google**.
2. Browser receives **ID token** (JWT) from Google.
3. SPA sends `POST /auth/google` with `{ "idToken": "<token>" }`.
4. On **200**, store `accessToken` (memory + `sessionStorage` is a reasonable default; avoid `localStorage` if you want reduced XSS blast radius).
5. Attach `Authorization: Bearer <accessToken>` to like/comment requests.
6. On **401** or expiry (`expiresAt`), clear storage and treat user as signed out.

### 9.3 “Registration”

There is no separate registration form. The **first** successful `auth/google` for an email **creates** the user account server-side. Your UI copy can say **“Sign in or create an account with Google”**.

### 9.4 Roles and permissions (FYI)

The JWT includes **roles** and **permission** claims for dashboard use. The public site usually only needs to know **signed in vs anonymous**; you may read `roles` from `TokenResponseDto` if you want to show a “Staff” link (only if you intentionally expose a dashboard URL).

---

## 10. Routing and SEO

- **Canonical URLs** by `slug` for articles and categories.
- After loading article detail, set **document title** and **meta description** from `title` / `summary`.
- **Open Graph** tags using `coverImageUrl` (absolute URLs for social crawlers).
- **Structured data** (`Article` schema.org) optional but improves professionalism.

---

## 11. Internationalization and accessibility

- Format dates with the user’s locale (`Intl.DateTimeFormat`).
- **Keyboard** support for slider arrows, modals, and menus.
- **Focus management** when opening sign-in or mobile nav.
- **Alt text** for images: use `title` or article `title` when CMS does not provide alt.

---

## 12. Performance

- **Cache** `GET /home` briefly in memory (1–5 minutes) to avoid hammering the API on every navigation; provide manual refresh if needed.
- **Image optimization:** serve modern formats from CDN when possible; use `loading="lazy"` below the fold.
- **Code-split** article list vs detail routes.

---

## 13. CORS

If the SPA is on a **different origin** than the API, the API must allow your frontend origin in CORS policy. Coordinate with backend deployment (this is environment-specific).

---

## Summary checklist for a polished public site

- [ ] Navbar from `categories` + auth state
- [ ] Footer from `socialLinks`
- [ ] Hero slider from `slides` with correct `linkTargetType` handling
- [ ] Home latest + full `/articles` listing with paging
- [ ] Article detail with sanitized HTML, likes, threaded comments
- [ ] Google sign-in wired to `POST /auth/google` and JWT on protected routes
- [ ] Clear UX for **moderated** comments (pending until approved)
- [ ] SEO meta, accessible carousel, error/empty states

For raw request/response JSON for every endpoint, use **[CLIENT_API_REFERENCE.md](./CLIENT_API_REFERENCE.md)**.
