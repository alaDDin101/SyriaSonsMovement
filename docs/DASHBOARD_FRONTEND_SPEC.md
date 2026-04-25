# Dashboard CMS — implementation prompt (API requirements per page)

Use this document as the **product and integration spec** for building the **Syria Sons Movement** admin dashboard (SPA). Implement each **page** below using only the listed **HTTP APIs**; match **request/response JSON** shapes and **field names** (camelCase in JSON unless your client configures otherwise—ASP.NET Core defaults to camelCase).

---

## Global requirements (all pages except Login)

| Requirement | Detail |
|-------------|--------|
| **API base** | `{BASE}/api/dashboard/v1` — replace `{BASE}` with your environment (e.g. `https://api.example.com`). |
| **Swagger** | Development: `{BASE}/swagger/Dashboard/swagger.json`. |
| **Content type** | `application/json` on requests with a body. |
| **Authenticated calls** | Header: `Authorization: Bearer <accessToken>` on every request **except** `POST /auth/login`. |
| **Login response** | Store `accessToken` from `TokenResponseDto`; send it on all subsequent requests until logout or expiry. |
| **401** | Clear token and redirect to **Login** page. |
| **403** | User lacks permission for that policy; show access denied (do not infinite-retry). |
| **400** | Often `{ "message": "string" }` — surface to the user. |

### JWT permissions (for navigation and guards)

The API enforces **authorization policies**. The JWT includes repeated claims: **`permission`** = one of:

| `permission` value | Area |
|--------------------|------|
| `slider.manage` | Slider |
| `categories.manage` | Categories |
| `articles.manage` | Articles |
| `social.manage` | Social links |
| `comments.moderate` | Comments moderation |
| `media.upload` | Upload images; receive root-relative URLs for CMS fields |
| `users.manage` | Users: list, create, update, delete, set password, assign roles |
| `roles.manage` | Roles: create/update/delete (non-system), set permissions on roles |

**Shared read access:** `GET /permissions` and `GET /roles` (lists) require **`users.manage` OR `roles.manage`** (either permission claim is enough).

**UI rule:** Show a sidebar (or menu) item **only if** the user has the matching `permission` claim. Route guards should align with the same permissions.

---

## Shared DTOs

### `TokenResponseDto` (login response `200`)

| Field | Type | Description |
|-------|------|-------------|
| `accessToken` | `string` | JWT |
| `expiresAt` | `string` (ISO 8601) | Token expiry |
| `userId` | `string` (UUID) | Current user id |
| `email` | `string \| null` | |
| `roles` | `string[]` | Role names |

### `LoginRequestDto` (login body)

| Field | Type | Required |
|-------|------|----------|
| `email` | `string` | yes |
| `password` | `string` | yes |

### `PagedResult<T>`

| Field | Type |
|-------|------|
| `items` | `T[]` |
| `totalCount` | `number` |
| `page` | `number` |
| `pageSize` | `number` |

### `ImageUploadResponseDto` (`POST /media/upload` success body)

| Field | Type | Description |
|-------|------|-------------|
| `url` | `string` | Root-relative path (e.g. `/uploads/2026/04/<guid>.webp`). Store this in slider/article/category/social URL fields. In the browser, display with `new URL(dto.url, API_ORIGIN).href` or concatenate `{BASE}` + `url` when the API serves static files. |

---

## Page: Media — image upload

**Route:** N/A (modal or sub-control on Slider, Articles, Categories, Social forms).

**Purpose:** Upload a binary image; receive a **root-relative** `url` to paste into `coverImageUrl`, `backgroundImageUrl`, `iconUrl`, etc.

**Permission:** `media.upload` (included for **Admin** and **Editor**; re-login after deploy if the claim is missing).

### APIs used

#### `POST /media/upload`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/media/upload` |
| **Auth** | Bearer + policy `media.upload` |
| **Content-Type** | `multipart/form-data` |
| **Form field** | One file part (e.g. field name `file`) — use the same name your client sends; ASP.NET Core binds the first `IFormFile`. |

**Request:** multipart body with an image file. Allowed extensions: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp` (configurable server-side). Max size default **5 MB** (`Uploads:MaxBytes`).

**Response `200`:** `ImageUploadResponseDto` — `{ "url": "/uploads/yyyy/MM/<guid>.<ext>" }`.

**Response `400`:** `{ "message": "..." }` (validation / empty file / disallowed type).

**Frontend usage**

1. `POST` with `FormData`: `formData.append('file', fileBlob, fileName)`.
2. Read `url` from JSON; save into article/slider/category/social payloads.
3. **Preview in dashboard:** `img.src = apiBase.replace(/\/$/, '') + response.url` (or `new URL(response.url, apiBase)`).
4. **Public site:** same rule — API host must serve `GET /uploads/...` (static files). If the SPA uses a different origin, always prefix with the API base URL.

---

## Page: Login

**Route:** `/login` (unauthenticated).

**Purpose:** Staff sign-in with email/password. Obtain JWT for all other dashboard pages.

**Permission:** None (public endpoint).

### APIs used

#### `POST /auth/login`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/auth/login` |
| **Auth** | None |
| **Request body** | `LoginRequestDto` |

**Request example:**

```json
{
  "email": "admin@example.com",
  "password": "YourPasswordHere"
}
```

**Response `200` — body:** `TokenResponseDto` (see fields above).

**Response `401`:** Invalid credentials (no body or minimal).

**After success:** Persist `accessToken` (and optionally `expiresAt`), then navigate to the dashboard shell (e.g. `/` or `/articles`).

---

## Page: App shell (layout)

**Route:** `/` or `/dashboard` (authenticated).

**Purpose:** Persistent layout: header (user email optional), **logout** (clear token), **sidebar** filtered by JWT `permission` claims. No aggregate “stats” API exists—optional empty welcome or redirect to the first allowed section.

**Permission:** Any valid JWT (no specific `permission` required to see the shell).

### APIs used

None required for layout alone. Optional: decode JWT client-side for `email`, `roles`, `permission` claims.

---

## Page: Slider — list

**Route:** `/slider` (example).

**Purpose:** View all home slider slides; navigate to create or edit; delete a slide.

**Permission:** `slider.manage`.

### APIs used

#### `GET /slider`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/slider` |
| **Auth** | Bearer + policy `slider.manage` |
| **Request body** | — |

**Response `200` — body:** JSON array of `SliderSlideDto`.

**`SliderSlideDto` (each element):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | |
| `backgroundImageUrl` | `string \| null` | |
| `title` | `string \| null` | |
| `subtitle` | `string \| null` | |
| `contentHtml` | `string \| null` | |
| `linkTargetType` | `number` | `0` = None, `1` = ExternalUrl, `2` = InternalArticle |
| `articleId` | UUID \| null | |
| `articleSlug` | `string \| null` | Read-only hint for internal links |
| `externalUrl` | `string \| null` | |
| `openInNewTab` | `boolean` | |
| `displayOrder` | `number` | |

**UI:** Sort or display by `displayOrder`; actions: Edit → navigate to `/slider/:id`, Delete → call delete API, Create → `/slider/new`.

#### `DELETE /slider/{id}`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/slider/{id}` |
| **Auth** | Bearer + `slider.manage` |
| **Request body** | — |

**Response `204`:** Success. **Response `404`:** Slide not found.

---

## Page: Slider — create / edit

**Route:** `/slider/new`, `/slider/:id`.

**Purpose:** Create or update one slide: images, titles, HTML snippet, link type (none / external URL / internal article), order, active flag.

**Permission:** `slider.manage`.

**Helper:** For **internal article** links, load article options via **Articles list** API (`GET /articles` with `search` / pagination) so the user can pick `articleId`.

### APIs used

#### `GET /slider/{id}` (edit only)

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/slider/{id}` |
| **Auth** | Bearer + `slider.manage` |

**Response `200`:** `SliderSlideDto` (fields as in list). **Response `404`:** Not found.

#### `POST /slider` (create)

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/slider` |
| **Auth** | Bearer + `slider.manage` |
| **Request body** | `SliderSlideUpsertDto` |

**`SliderSlideUpsertDto`:**

| Field | Type | Notes |
|-------|------|--------|
| `backgroundImageUrl` | `string \| null` | |
| `title` | `string \| null` | |
| `subtitle` | `string \| null` | |
| `contentHtml` | `string \| null` | |
| `linkTargetType` | `number` | `0` / `1` / `2` |
| `articleId` | UUID \| null | Set when `linkTargetType === 2` |
| `externalUrl` | `string \| null` | Set when `linkTargetType === 1` |
| `openInNewTab` | `boolean` | |
| `displayOrder` | `number` | |
| `isActive` | `boolean` | default `true` |

**Response `200`:** `SliderSlideDto`. **Response `400`:** `{ "message": "..." }`.

#### `PUT /slider/{id}` (update)

Same **request body** as `POST`. **Response `200`:** `SliderSlideDto`. **Response `404` / `400`:** as above.

---

## Page: Categories — list

**Route:** `/categories`.

**Purpose:** List categories; open create/edit; delete.

**Permission:** `categories.manage`.

### APIs used

#### `GET /categories`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/categories` |
| **Auth** | Bearer + `categories.manage` |

**Response `200`:** JSON array of `CategoryCardDto`.

**`CategoryCardDto`:**

| Field | Type |
|-------|------|
| `id` | UUID |
| `name` | `string` |
| `slug` | `string` |
| `backgroundImageUrl` | `string \| null` |
| `description` | `string \| null` |
| `displayOrder` | `number` |

#### `DELETE /categories/{id}`

**Response `204`:** OK. **Response `404`:** Not found.

---

## Page: Categories — create / edit

**Route:** `/categories/new`, `/categories/:id`.

**Permission:** `categories.manage`.

### APIs used

#### `GET /categories/{id}` (edit)

**Response `200`:** `CategoryCardDto`. **Response `404`:** Not found.

#### `POST /categories`

**Request body — `CategoryUpsertDto`:**

| Field | Type |
|-------|------|
| `name` | `string` |
| `slug` | `string` |
| `backgroundImageUrl` | `string \| null` |
| `description` | `string \| null` |
| `displayOrder` | `number` |
| `isActive` | `boolean` |

**Response `200`:** `CategoryCardDto`.

#### `PUT /categories/{id}`

Same request body. **Response `200`:** `CategoryCardDto`. **Response `404`:** Not found.

---

## Page: Articles — list

**Route:** `/articles`.

**Purpose:** Paginated table of articles with filters; open editor; delete.

**Permission:** `articles.manage`.

### APIs used

#### `GET /articles`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/articles` |
| **Auth** | Bearer + `articles.manage` |

**Query parameters (`ArticleDashboardQuery`):**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | `number` | `1` | |
| `pageSize` | `number` | `20` | |
| `categoryId` | UUID \| omitted | | Filter by category |
| `isPublished` | `boolean` \| omitted | | Filter draft vs published |
| `search` | `string` \| omitted | | Search text |

**Response `200`:** `PagedResult<ArticleListItemDto>`.

**`ArticleListItemDto`:**

| Field | Type |
|-------|------|
| `id` | UUID |
| `title` | `string` |
| `slug` | `string` |
| `isPublished` | `boolean` |
| `publishedAt` | `string \| null` (ISO 8601) |
| `categoryId` | UUID |
| `categoryName` | `string` |
| `viewCount` | `number` |
| `createdAt` | `string` (ISO 8601) |

#### `DELETE /articles/{id}`

**Response `204`:** OK. **Response `404`:** Not found.

---

## Page: Articles — create / edit

**Route:** `/articles/new`, `/articles/:id`.

**Purpose:** Full article editor: category, title, slug, summary, HTML body, cover image, publish flags and date.

**Permission:** `articles.manage`.

**Note:** For **create**, `categoryId` must reference an existing category—use **Categories list** (`GET /categories`) to populate a dropdown.

### APIs used

#### `GET /articles/{id}` (edit)

**Response `200`:** `ArticleDetailDto`. **Response `404`:** Not found.

**`ArticleDetailDto`:**

| Field | Type |
|-------|------|
| `id` | UUID |
| `title` | `string` |
| `slug` | `string` |
| `summary` | `string \| null` |
| `bodyHtml` | `string` |
| `coverImageUrl` | `string \| null` |
| `publishedAt` | `string \| null` |
| `viewCount` | `number` |
| `likeCount` | `number` |
| `categoryId` | UUID |
| `categoryName` | `string` |
| `authorDisplayName` | `string \| null` |
| `likedByCurrentUser` | `boolean` |
| `comments` | `ArticleCommentDto[]` |

**`ArticleCommentDto` (nested; dashboard may show read-only thread):**

| Field | Type |
|-------|------|
| `id` | UUID |
| `body` | `string` |
| `createdAt` | `string` |
| `userId` | UUID |
| `userDisplayName` | `string \| null` |
| `parentCommentId` | UUID \| null |
| `isApproved` | `boolean` |
| `replies` | `ArticleCommentDto[]` |

#### `POST /articles` (create)

**Request body — `ArticleUpsertDto`:**

| Field | Type |
|-------|------|
| `categoryId` | UUID |
| `title` | `string` |
| `slug` | `string` |
| `summary` | `string \| null` |
| `bodyHtml` | `string` |
| `coverImageUrl` | `string \| null` |
| `isPublished` | `boolean` |
| `publishedAt` | `string \| null` (ISO 8601) |

**Response `200`:** `ArticleDetailDto`. **Response `400`:** `{ "message": "..." }`.

#### `PUT /articles/{id}` (update)

Same **request body** as create. **Response `200`:** `ArticleDetailDto`. **Response `404` / `400`:** as above.

---

## Page: Social links — list

**Route:** `/social`.

**Purpose:** List social links; add, edit, delete.

**Permission:** `social.manage`.

### APIs used

#### `GET /social`

**Response `200`:** JSON array of `SocialLinkDto`.

**`SocialLinkDto`:**

| Field | Type |
|-------|------|
| `id` | UUID |
| `platformKey` | `string` |
| `label` | `string \| null` |
| `url` | `string` |
| `iconUrl` | `string \| null` |
| `displayOrder` | `number` |

#### `DELETE /social/{id}`

**Response `204`:** OK. **Response `404`:** Not found.

---

## Page: Social links — create / edit

**Route:** `/social/new`, `/social/:id`.

**Permission:** `social.manage`.

### APIs used

#### `GET /social/{id}` (edit)

**Response `200`:** `SocialLinkDto`. **Response `404`:** Not found.

#### `POST /social`

**Request body — `SocialLinkUpsertDto`:**

| Field | Type |
|-------|------|
| `platformKey` | `string` |
| `label` | `string \| null` |
| `url` | `string` |
| `iconUrl` | `string \| null` |
| `displayOrder` | `number` |
| `isActive` | `boolean` |

**Response `200`:** `SocialLinkDto`.

#### `PUT /social/{id}`

Same body. **Response `200`:** `SocialLinkDto`. **Response `404`:** Not found.

---

## Page: Comments — moderation queue

**Route:** `/comments`.

**Purpose:** Review comments: filter by article/approval; approve, reject, or soft-delete.

**Permission:** `comments.moderate`.

### APIs used

#### `GET /comments`

**Query parameters (`CommentDashboardQuery`):**

| Parameter | Type | Default |
|-----------|------|---------|
| `page` | `number` | `1` |
| `pageSize` | `number` | `20` |
| `articleId` | UUID \| omitted | |
| `approvedOnly` | `boolean` \| omitted | |

**Response `200`:** `PagedResult<CommentModerationItemDto>`.

**`CommentModerationItemDto`:**

| Field | Type |
|-------|------|
| `id` | UUID |
| `articleId` | UUID |
| `articleTitle` | `string` |
| `body` | `string` |
| `isApproved` | `boolean` |
| `isDeleted` | `boolean` |
| `createdAt` | `string` |
| `userId` | UUID |
| `userEmail` | `string \| null` |

#### `POST /comments/{id}/approve`

**Request body:** none. **Response `204`:** Success. **Response `404`:** Not found.

#### `POST /comments/{id}/reject`

**Request body:** none. **Response `204`:** Success. **Response `404`:** Not found.

#### `DELETE /comments/{id}`

Soft delete. **Response `204`:** Success. **Response `404`:** Not found.

---

## Page: Permissions catalog (read-only)

**Route:** `/permissions` (or embedded in role editor).

**Purpose:** List all permission strings that can be assigned to roles (from `Permissions` table).

**Permission:** `users.manage` **or** `roles.manage`.

### APIs used

#### `GET /permissions`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/permissions` |
| **Auth** | Bearer + `users.manage` **or** `roles.manage` |

**Response `200`:** JSON array of `PermissionDto`.

| Field | Type |
|-------|------|
| `id` | UUID |
| `name` | `string` (e.g. `articles.manage`) |
| `description` | `string \| null` |

**UI:** Multi-select or checklist when editing a role’s permissions; use `name` values in `SetRolePermissionsDto`.

---

## Page: Roles — list

**Route:** `/roles`.

**Purpose:** Table of roles with permission counts; links to detail/edit; create custom role.

**Permission:** `users.manage` **or** `roles.manage` (list only).

### APIs used

#### `GET /roles`

| | |
|---|---|
| **Full path** | `{BASE}/api/dashboard/v1/roles` |
| **Auth** | Bearer + `users.manage` **or** `roles.manage` |

**Response `200`:** `RoleListItemDto[]`.

| Field | Type |
|-------|------|
| `id` | UUID |
| `name` | `string` |
| `description` | `string \| null` |
| `createdAt` | ISO string |
| `permissionCount` | `number` |

**System roles:** `Admin`, `Editor`, `Moderator` — cannot be deleted or renamed; permissions can still be updated.

---

## Page: Roles — detail / edit permissions

**Route:** `/roles/:id`.

**Permission:** `roles.manage` (stricter than list).

### APIs used

#### `GET /roles/{id}`

**Response `200`:** `RoleDetailDto`.

| Field | Type |
|-------|------|
| `id` | UUID |
| `name` | `string` |
| `description` | `string \| null` |
| `createdAt` | ISO string |
| `isSystemRole` | `boolean` |
| `permissions` | `PermissionDto[]` |

#### `PUT /roles/{id}/permissions`

**Request body — `SetRolePermissionsDto`:**

| Field | Type |
|-------|------|
| `permissionNames` | `string[]` (exact names from `GET /permissions`; **replaces** all role permissions) |

**Response `200`:** `RoleDetailDto`. **Response `400`:** unknown permission name.

#### `PUT /roles/{id}`

**Request body — `UpdateRoleDto`:**

| Field | Type |
|-------|------|
| `name` | `string \| null` — **ignored** for system roles; for custom roles, new unique name |
| `description` | `string \| null` |

**Response `200`:** `RoleDetailDto`.

#### `DELETE /roles/{id}`

**Response `204`:** Deleted. **Response `400`:** system role or role still assigned to users.

---

## Page: Roles — create

**Route:** `/roles/new`.

**Permission:** `roles.manage`.

### APIs used

#### `POST /roles`

**Request body — `CreateRoleDto`:**

| Field | Type |
|-------|------|
| `name` | `string` |
| `description` | `string \| null` |
| `permissionNames` | `string[]` (initial permissions) |

**Response `200`:** `RoleDetailDto`. **Response `400`:** duplicate name or invalid permission.

---

## Page: Users — list

**Route:** `/users`.

**Permission:** `users.manage` only.

### APIs used

#### `GET /users`

**Query — `UserManagementQuery`:**

| Parameter | Type | Default |
|-----------|------|---------|
| `page` | `number` | `1` |
| `pageSize` | `number` | `20` |
| `search` | `string \| omitted` | email / display name contains |
| `roleId` | UUID \| omitted | filter users having this role |

**Response `200`:** `PagedResult<UserListItemDto>`.

**`UserListItemDto`:**

| Field | Type |
|-------|------|
| `id` | UUID |
| `email` | `string` |
| `displayName` | `string \| null` |
| `emailConfirmed` | `boolean` |
| `createdAt` | ISO string |
| `lastLoginAt` | ISO string \| null |
| `roles` | `string[]` |

---

## Page: Users — detail / edit

**Route:** `/users/:id`.

**Permission:** `users.manage`.

### APIs used

#### `GET /users/{id}`

**Response `200`:** `UserDetailDto`.

| Field | Type |
|-------|------|
| `id` | UUID |
| `email` | `string` |
| `displayName` | `string \| null` |
| `profileImageUrl` | `string \| null` |
| `emailConfirmed` | `boolean` |
| `lockoutEnd` | ISO string \| null |
| `createdAt` | ISO string |
| `lastLoginAt` | ISO string \| null |
| `roles` | `string[]` |

#### `PUT /users/{id}`

**Request body — `UpdateUserDto`:**

| Field | Type |
|-------|------|
| `email` | `string \| null` — updates username + email when changed |
| `displayName` | `string \| null` |
| `emailConfirmed` | `boolean \| null` |

**Response `200`:** `UserDetailDto`.

#### `POST /users/{id}/password`

**Request body — `AdminSetPasswordDto`:**

| Field | Type |
|-------|------|
| `newPassword` | `string` |

**Response `200`:** `UserDetailDto`. Uses Identity password reset flow server-side.

#### `POST /users/{id}/roles`

**Request body — `AssignUserRolesDto`:**

| Field | Type |
|-------|------|
| `roleNames` | `string[]` — **replaces** all roles (use `[]` to remove all roles) |

**Response `200`:** `UserDetailDto`. Role names must match existing roles (e.g. `Admin`, `Editor`, `Moderator`).

#### `DELETE /users/{id}`

**Response `204`:** **Response `400`:** cannot delete yourself.

---

## Page: Users — create

**Route:** `/users/new`.

**Permission:** `users.manage`.

### APIs used

#### `POST /users`

**Request body — `CreateUserDto`:**

| Field | Type |
|-------|------|
| `email` | `string` |
| `password` | `string` (must meet Identity password policy) |
| `displayName` | `string \| null` |
| `roleNames` | `string[]` |

**Response `200`:** `UserDetailDto`. New users are created with `emailConfirmed: true`.

---

## Out of scope (current backend)

- No REST API for **CMS static Pages** (removed from product).
- Refresh tokens are not specified; handle expiry via **401** and re-login.
- **Permission / role changes** apply to **new JWTs** — users must **re-login** (or call login again) to see updated claims in the token.
