# Client API reference

Base path: `{BASE}/api/client/v1` (replace `{BASE}` with your API origin, for example `https://api.example.com`).

Unless noted otherwise:

- **Content-Type** for JSON bodies: `application/json`
- **JSON field names** are **camelCase** (ASP.NET Core default)
- **Enums** are serialized as **numbers** (for example `linkTargetType: 2`)

Authenticated requests use:

```http
Authorization: Bearer <accessToken>
```

---

## 1. Home page bundle

Loads the landing page shell: hero slider, categories, **first DB page** of published articles (`articlesSection`), social links, optional about block, and optional **first page** of projects (`projectsSection`) when that hub is visible.

| Item | Value |
|------|--------|
| Method / path | `GET /home` |
| Auth | Not required |

### 200 OK — response body

Shape: `HomePageDto` (fields may include `aboutUs`, `projectsSection`; omitted below when null)

```json
{
  "slides": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "backgroundImageUrl": "/uploads/slider/hero-1.webp",
      "title": "Welcome",
      "subtitle": "Syria Sons Movement",
      "contentHtml": "<p>Optional rich text.</p>",
      "linkTargetType": 2,
      "articleId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "articleSlug": "first-post",
      "externalUrl": null,
      "openInNewTab": false,
      "displayOrder": 0,
      "isActive": true
    }
  ],
  "categories": [
    {
      "id": "b3d4c5d6-e7f8-9012-a345-678901234567",
      "name": "News",
      "slug": "news",
      "backgroundImageUrl": "/uploads/categories/news.webp",
      "description": "Short blurb for the card.",
      "displayOrder": 0,
      "isActive": true
    }
  ],
  "articlesSection": {
    "items": [
      {
        "id": "11111111-2222-3333-4444-555555555555",
        "title": "Article title",
        "slug": "article-title",
        "summary": "Teaser text",
        "coverImageUrl": "/uploads/articles/cover.webp",
        "publishedAt": "2026-04-01T12:00:00+00:00",
        "categoryId": "b3d4c5d6-e7f8-9012-a345-678901234567",
        "categoryName": "News",
        "viewCount": 120,
        "likeCount": 15,
        "commentCount": 4
      }
    ],
    "totalCount": 42,
    "page": 1,
    "pageSize": 10
  },
  "socialLinks": [
    {
      "id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
      "platformKey": "facebook",
      "label": "Facebook",
      "url": "https://facebook.com/yourpage",
      "iconUrl": null,
      "displayOrder": 0,
      "isActive": true
    }
  ]
}
```

### `SliderSlideDto.linkTargetType` (numeric)

| Value | Name | Meaning |
|------:|------|----------|
| `0` | `None` | Decorative slide; do not treat as a link. |
| `1` | `ExternalUrl` | Use `externalUrl`; honor `openInNewTab`. |
| `2` | `InternalArticle` | Link to article route using `articleSlug` (and optionally `articleId`). If the linked article is not published, the API may coerce this to `None` and clear slug/url. |

### Example request

```http
GET /api/client/v1/home HTTP/1.1
Host: api.example.com
Accept: application/json
```

The first page size for `articlesSection` is defined by the server (currently **10**). Use **`GET /home/articles`** (below) or **`GET /articles`** to load additional pages for a horizontal strip or archive.

---

## 1b. Home — more articles (paged, same as list API)

Use this for the home page horizontal strip when the user clicks **next/prev** or arrow controls without refetching the full home payload.

| Item | Value |
|------|--------|
| Method / path | `GET /home/articles` |
| Auth | Not required |

### Query parameters

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | int | `1` | Page index (≥ 1). |
| `pageSize` | int | `10` | Clamped server-side to **1–100**. |
| `categoryId` | guid (optional) | — | Optional category filter. |

### 200 OK

Same shape as **`GET /articles`**: `PagedResult<ArticleSummaryDto>`.

```http
GET /api/client/v1/home/articles?page=2&pageSize=10 HTTP/1.1
Host: api.example.com
```

---

## 1c. Home — projects strip (paged)

When the projects hub is enabled, `GET /home` includes `projectsSection` (metadata + first page). Load more slides with:

| Item | Value |
|------|--------|
| Method / path | `GET /home/projects` |
| Auth | Not required |

### Query parameters

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | int | `1` | Page index. |
| `pageSize` | int | `6` | Capped by server (max **20**). |

### 200 OK — `HomeProjectsSectionDto`

Includes `title`, `leadText`, `page`, `pageSize`, `totalCount`, and `items` (`HomeProjectSlideDto[]` with `sectionTitle`, `slug`, etc.).

### 404 Not Found

Projects hub hidden in CMS / settings.

```http
GET /api/client/v1/home/projects?page=2&pageSize=6 HTTP/1.1
Host: api.example.com
```

---

## 1d. Projects hub page (full list, paged)

| Item | Value |
|------|--------|
| Method / path | `GET /projects` |
| Auth | Not required |

### Query parameters

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | int | `1` | Page index. |
| `pageSize` | int | `12` | Clamped to **1–100**. |
| `section` | byte (optional) | — | Filter: **1** = What we offer, **2** = Service/awareness, **3** = Future plans. Omit for all sections (ordered by section, then display order, then publish date). |

### 200 OK — `ProjectsHubPageDto`

```json
{
  "title": "المشاريع والمبادرات",
  "leadText": "…",
  "introHtml": "<p>…</p>",
  "page": 1,
  "pageSize": 12,
  "totalCount": 35,
  "sectionFilter": null,
  "items": [
    {
      "id": "…",
      "title": "…",
      "slug": "…",
      "summary": "…",
      "coverImageUrl": "/uploads/…",
      "publishedAt": "2026-04-01T12:00:00+00:00",
      "section": 1,
      "sectionTitle": "…"
    }
  ]
}
```

### 404 Not Found

Projects hub not visible.

```http
GET /api/client/v1/projects?page=2&pageSize=12&section=2 HTTP/1.1
Host: api.example.com
```

---

## 2. Authentication (Google)

There is **no** separate email/password sign-up endpoint on the client API. **First successful Google sign-in creates the user** (implicit registration). Returning users refresh profile fields when Google provides them.

| Item | Value |
|------|--------|
| Method / path | `POST /auth/google` |
| Auth | Not required |

### Request body — `GoogleLoginRequestDto`

```json
{
  "idToken": "<Google ID token JWT string from the browser Google Identity Services flow>"
}
```

### 200 OK — `TokenResponseDto`

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-04-11T14:00:00+00:00",
  "userId": "6f1e7b3a-4c2d-4e5f-8a9b-0c1d2e3f4a5b",
  "email": "user@gmail.com",
  "roles": ["Member"]
}
```

Store `accessToken` and send it on protected routes until logout or `expiresAt` (refresh is not implemented in this API surface; re-run Google sign-in when expired).

### 401 Unauthorized

Invalid or untrusted `idToken` (or email missing from token). Body is typically empty.

### 400 Bad Request

Configuration or validation error, for example when Google client ID is not configured on the server:

```json
{
  "message": "Authentication:Google:ClientId is not configured."
}
```

### Example request

```http
POST /api/client/v1/auth/google HTTP/1.1
Host: api.example.com
Content-Type: application/json

{"idToken":"..."}
```

**Server requirement:** `Authentication:Google:ClientId` in API configuration must match the OAuth client used in the browser to obtain `idToken`.

---

## 3. Articles — list (published)

| Item | Value |
|------|--------|
| Method / path | `GET /articles` |
| Auth | Not required |

### Query parameters

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `page` | int | `1` | Page index (≥ 1). |
| `pageSize` | int | `10` | Clamped server-side to **1–100**. |
| `categoryId` | guid (optional) | — | Filter by category. |

### 200 OK — `PagedResult<ArticleSummaryDto>`

```json
{
  "items": [
    {
      "id": "11111111-2222-3333-4444-555555555555",
      "title": "Article title",
      "slug": "article-title",
      "summary": "Teaser",
      "coverImageUrl": "/uploads/articles/cover.webp",
      "publishedAt": "2026-04-01T12:00:00+00:00",
      "categoryId": "b3d4c5d6-e7f8-9012-a345-678901234567",
      "categoryName": "News",
      "viewCount": 120,
      "likeCount": 15,
      "commentCount": 4
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10
}
```

### Example

```http
GET /api/client/v1/articles?page=1&pageSize=12&categoryId=b3d4c5d6-e7f8-9012-a345-678901234567 HTTP/1.1
Host: api.example.com
```

---

## 4. Articles — detail by slug

| Item | Value |
|------|--------|
| Method / path | `GET /articles/{slug}` |
| Auth | Optional |

When **Authorization** is present with a valid JWT, `likedByCurrentUser` reflects whether that user liked the article.

### 200 OK — `ArticleDetailDto`

```json
{
  "id": "11111111-2222-3333-4444-555555555555",
  "title": "Full article",
  "slug": "full-article",
  "summary": "Short intro",
  "bodyHtml": "<p>Rich HTML from CMS.</p>",
  "coverImageUrl": "/uploads/articles/cover.webp",
  "isPublished": true,
  "publishedAt": "2026-04-01T12:00:00+00:00",
  "viewCount": 121,
  "likeCount": 15,
  "categoryId": "b3d4c5d6-e7f8-9012-a345-678901234567",
  "categoryName": "News",
  "authorDisplayName": "Editor Name",
  "likedByCurrentUser": false,
  "comments": [
    {
      "id": "cccccccc-dddd-eeee-ffff-000000000001",
      "body": "Great article",
      "createdAt": "2026-04-02T09:00:00+00:00",
      "userId": "6f1e7b3a-4c2d-4e5f-8a9b-0c1d2e3f4a5b",
      "userDisplayName": "Reader",
      "parentCommentId": null,
      "isApproved": true,
      "replies": []
    }
  ]
}
```

**Comment tree:** each `ArticleCommentDto` may include nested `replies` of the same shape (threaded discussion). Only **approved** comments appear on the public article response.

### 404 Not Found

Unknown slug or unpublished article.

### Example (anonymous)

```http
GET /api/client/v1/articles/full-article HTTP/1.1
Host: api.example.com
```

### Example (authenticated, for accurate like flag)

```http
GET /api/client/v1/articles/full-article HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 5. Articles — like

| Item | Value |
|------|--------|
| Method / path | `POST /articles/{id}/like` |
| Auth | **Required** |

### 204 No Content

Like recorded (or already liked — idempotent success).

### 400 Bad Request

```json
{
  "message": "Article not found."
}
```

### 401 Unauthorized

Missing or invalid JWT.

### Example

```http
POST /api/client/v1/articles/11111111-2222-3333-4444-555555555555/like HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 6. Articles — unlike

| Item | Value |
|------|--------|
| Method / path | `DELETE /articles/{id}/like` |
| Auth | **Required** |

### 204 No Content

### 401 Unauthorized

Missing or invalid JWT.

### Example

```http
DELETE /api/client/v1/articles/11111111-2222-3333-4444-555555555555/like HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 7. Articles — add comment

| Item | Value |
|------|--------|
| Method / path | `POST /articles/{id}/comments` |
| Auth | **Required** |

### Request body — `ArticleCommentCreateDto`

```json
{
  "body": "My comment text",
  "parentCommentId": null
}
```

Use `parentCommentId` set to another comment’s `id` to post a **reply** (thread).

### 201 Created — `ArticleCommentDto`

```json
{
  "id": "dddddddd-eeee-ffff-0000-111111111111",
  "body": "My comment text",
  "createdAt": "2026-04-11T10:30:00+00:00",
  "userId": "6f1e7b3a-4c2d-4e5f-8a9b-0c1d2e3f4a5b",
  "userDisplayName": "Reader",
  "parentCommentId": null,
  "isApproved": false,
  "replies": []
}
```

New comments are created with **`isApproved`: `false`**. Moderation happens in the dashboard; until approved, they **do not** appear in `GET /articles/{slug}`.

### 400 Bad Request

```json
{
  "message": "Article not found."
}
```

or parent comment validation errors with a similar `{ "message": "..." }` shape.

### 401 Unauthorized

### Example

```http
POST /api/client/v1/articles/11111111-2222-3333-4444-555555555555/comments HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{"body":"Thanks for writing this.","parentCommentId":null}
```

---

## Quick reference table

| Method | Path | Auth |
|--------|------|------|
| `GET` | `/api/client/v1/home` | No |
| `GET` | `/api/client/v1/home/articles` | No |
| `GET` | `/api/client/v1/home/projects` | No |
| `GET` | `/api/client/v1/projects` | No |
| `POST` | `/api/client/v1/auth/google` | No |
| `GET` | `/api/client/v1/articles` | No |
| `GET` | `/api/client/v1/articles/{slug}` | Optional |
| `POST` | `/api/client/v1/articles/{id}/like` | Yes |
| `DELETE` | `/api/client/v1/articles/{id}/like` | Yes |
| `POST` | `/api/client/v1/articles/{id}/comments` | Yes |

---

## Related documentation

- **[CLIENT_FRONTEND_GUIDE.md](./CLIENT_FRONTEND_GUIDE.md)** — how to structure the public website (layout, slider, navbar, auth UX, HTML safety, paging).
