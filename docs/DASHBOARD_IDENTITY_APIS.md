# Dashboard API — Permissions, Roles & Users

Reference for frontend integration. **Swagger (dev):** `{BASE}/swagger/Dashboard/swagger.json`.

All paths are under:

```text
{BASE}/api/dashboard/v1
```

Replace `{BASE}` with your API origin (e.g. `https://api.example.com`).

---

## Authentication

| Item | Value |
|------|--------|
| **Header** | `Authorization: Bearer <accessToken>` |
| **Token source** | `POST /api/dashboard/v1/auth/login` (see main dashboard spec) |

### JWT `permission` claims

The API checks **permission strings** in the JWT (repeated claim `permission`). Typical values:

| Permission | Meaning |
|------------|---------|
| `users.manage` | All **users** endpoints below |
| `roles.manage` | **Roles** detail, create, update, delete, set permissions |
| `users.manage` **or** `roles.manage` | **Permissions** catalog + **roles** list (`GET /permissions`, `GET /roles`) |

**Frontend:** decode JWT payload; show menu items only if the user has the required permission(s).

**After changing roles or permissions in the admin UI**, affected users must **log in again** to receive a new JWT with updated `permission` claims.

---

## JSON naming

Responses use **camelCase** property names (ASP.NET Core default), e.g. `permissionNames`, `emailConfirmed`, `isSystemRole`.

---

## Error responses

| Status | Typical body |
|--------|----------------|
| `400` | `{ "message": "Human-readable reason" }` |
| `401` | Missing/invalid token |
| `403` | Logged in but missing required permission |
| `404` | Resource not found (or empty body) |

---

# Permissions API

## `GET /permissions`

List all assignable permissions (for role editor checklists).

| | |
|---|---|
| **Required permission** | `users.manage` **or** `roles.manage` |
| **Request body** | — |

### Response `200`

Array of `PermissionDto[]`.

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string` (UUID) | Primary key |
| `name` | `string` | Permission key (use in `permissionNames` when saving a role) |
| `description` | `string \| null` | Optional |

### Example response

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "name": "articles.manage",
    "description": null
  },
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "name": "users.manage",
    "description": null
  }
]
```

### Example request (fetch)

```http
GET /api/dashboard/v1/permissions HTTP/1.1
Host: api.example.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

# Roles API

## `GET /roles`

List roles (summary, permission count).

| | |
|---|---|
| **Required permission** | `users.manage` **or** `roles.manage` |
| **Request body** | — |

### Response `200`

Array of `RoleListItemDto[]`.

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string` (UUID) | |
| `name` | `string` | e.g. `Admin`, `Editor`, `Moderator`, or custom |
| `description` | `string \| null` | |
| `createdAt` | `string` (ISO 8601) | |
| `permissionCount` | `number` | Number of permissions assigned |

### Example response

```json
[
  {
    "id": "11111111-1111-1111-1111-111111111111",
    "name": "Admin",
    "description": null,
    "createdAt": "2026-01-15T10:00:00+00:00",
    "permissionCount": 9
  }
]
```

---

## `GET /roles/{id}`

**Required permission:** `roles.manage`

### Response `200` — `RoleDetailDto`

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | |
| `name` | `string` | |
| `description` | `string \| null` | |
| `createdAt` | ISO string | |
| `isSystemRole` | `boolean` | `true` for built-in `Admin`, `Editor`, `Moderator` |
| `permissions` | `PermissionDto[]` | Full permission rows for this role |

### Example response

```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "Editor",
  "description": null,
  "createdAt": "2026-01-15T10:00:00+00:00",
  "isSystemRole": true,
  "permissions": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "name": "articles.manage",
      "description": null
    }
  ]
}
```

### Response `404`

Role id does not exist.

---

## `POST /roles`

Create a **custom** role and optional initial permissions.

| **Required permission** | `roles.manage` |
|-------------------------|----------------|
| **Content-Type** | `application/json` |

### Request body — `CreateRoleDto`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | `string` | yes | Unique role name |
| `description` | `string \| null` | no | |
| `permissionNames` | `string[]` | no | Permission `name` values from `GET /permissions` |

### Example request

```json
{
  "name": "ContentReviewer",
  "description": "Reviews only articles",
  "permissionNames": ["articles.manage", "comments.moderate"]
}
```

### Response `200`

Same shape as `RoleDetailDto` (see `GET /roles/{id}`).

### Response `400`

Duplicate role name, or unknown permission name in `permissionNames`.

---

## `PUT /roles/{id}`

Update role **description** and/or **name** (name only for **non-system** roles).

| **Required permission** | `roles.manage` |

### Request body — `UpdateRoleDto`

| Field | Type | Description |
|-------|------|-------------|
| `name` | `string \| null` | New name (ignored for system roles) |
| `description` | `string \| null` | |

### Example request

```json
{
  "description": "Updated description"
}
```

### Response `200`

`RoleDetailDto`.

### Response `404` / `400`

Not found or business rule violation (e.g. cannot rename system role).

---

## `PUT /roles/{id}/permissions`

**Replace** all permissions for the role with the given list.

| **Required permission** | `roles.manage` |

### Request body — `SetRolePermissionsDto`

| Field | Type | Description |
|-------|------|-------------|
| `permissionNames` | `string[]` | **Complete** new set; use `[]` to remove all permissions |

### Example request

```json
{
  "permissionNames": [
    "slider.manage",
    "categories.manage",
    "articles.manage",
    "social.manage",
    "media.upload"
  ]
}
```

### Response `200`

`RoleDetailDto` with updated `permissions`.

### Response `400`

Unknown `permissionNames` entry.

---

## `DELETE /roles/{id}`

| **Required permission** | `roles.manage` |

### Response `204`

Deleted.

### Response `400`

- System role (`Admin`, `Editor`, `Moderator`) cannot be deleted.
- Role is still assigned to one or more users.

### Response `404`

Unknown role id.

---

# Users API

## `GET /users`

Paged list with optional search and role filter.

| **Required permission** | `users.manage` |

### Query parameters — `UserManagementQuery`

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | `number` | `1` | 1-based page |
| `pageSize` | `number` | `20` | Page size |
| `search` | `string` | — | Matches email or display name (contains) |
| `roleId` | UUID | — | Only users that have this role |

### Example request

```http
GET /api/dashboard/v1/users?page=1&pageSize=20&search=admin HTTP/1.1
Authorization: Bearer ...
```

```http
GET /api/dashboard/v1/users?roleId=11111111-1111-1111-1111-111111111111 HTTP/1.1
Authorization: Bearer ...
```

### Response `200` — `PagedResult<UserListItemDto>`

| Field | Type |
|-------|------|
| `items` | `UserListItemDto[]` |
| `totalCount` | `number` |
| `page` | `number` |
| `pageSize` | `number` |

**`UserListItemDto`**

| Field | Type |
|-------|------|
| `id` | UUID |
| `email` | `string` |
| `displayName` | `string \| null` |
| `emailConfirmed` | `boolean` |
| `createdAt` | ISO string |
| `lastLoginAt` | ISO string \| null |
| `roles` | `string[]` |

### Example response

```json
{
  "items": [
    {
      "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "email": "editor@example.com",
      "displayName": "Editor User",
      "emailConfirmed": true,
      "createdAt": "2026-02-01T12:00:00+00:00",
      "lastLoginAt": "2026-04-01T08:30:00+00:00",
      "roles": ["Editor"]
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

---

## `GET /users/{id}`

| **Required permission** | `users.manage` |

### Response `200` — `UserDetailDto`

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

### Example response

```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "email": "editor@example.com",
  "displayName": "Editor User",
  "profileImageUrl": null,
  "emailConfirmed": true,
  "lockoutEnd": null,
  "createdAt": "2026-02-01T12:00:00+00:00",
  "lastLoginAt": "2026-04-01T08:30:00+00:00",
  "roles": ["Editor"]
}
```

---

## `POST /users`

Create a user (email login, **password** set by admin).

| **Required permission** | `users.manage` |

### Request body — `CreateUserDto`

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `email` | `string` | yes | Unique email |
| `password` | `string` | yes | Must satisfy server password policy (length, digits, etc.) |
| `displayName` | `string \| null` | no | |
| `roleNames` | `string[]` | no | Must match existing role **names** exactly (e.g. `Editor`) |

### Example request

```json
{
  "email": "newuser@example.com",
  "password": "Str0ng!Passw0rd",
  "displayName": "New User",
  "roleNames": ["Editor"]
}
```

### Response `200`

`UserDetailDto`. New users are created with `emailConfirmed: true`.

### Response `400`

Identity validation errors (password too weak, duplicate email, etc.).

---

## `PUT /users/{id}`

| **Required permission** | `users.manage` |

### Request body — `UpdateUserDto`

| Field | Type | Description |
|-------|------|-------------|
| `email` | `string \| null` | If set, updates email + username |
| `displayName` | `string \| null` | |
| `emailConfirmed` | `boolean \| null` | |

### Example request

```json
{
  "displayName": "Updated Name",
  "emailConfirmed": true
}
```

### Response `200`

`UserDetailDto`.

---

## `POST /users/{id}/password`

Admin reset of password (no old password required).

| **Required permission** | `users.manage` |

### Request body — `AdminSetPasswordDto`

| Field | Type | Required |
|-------|------|----------|
| `newPassword` | `string` | yes |

### Example request

```json
{
  "newPassword": "NewStr0ng!Pass"
}
```

### Response `200`

`UserDetailDto`.

---

## `POST /users/{id}/roles`

**Replace** the user’s roles with the given list.

| **Required permission** | `users.manage` |

### Request body — `AssignUserRolesDto`

| Field | Type | Description |
|-------|------|-------------|
| `roleNames` | `string[]` | **Full** replacement; use `[]` to remove all roles |

### Example request

```json
{
  "roleNames": ["Moderator"]
}
```

### Response `200`

`UserDetailDto`.

### Response `400`

Unknown role name.

---

## `DELETE /users/{id}`

| **Required permission** | `users.manage` |

### Response `204`

User deleted.

### Response `400`

`{ "message": "You cannot delete your own account." }` when the target id matches the **current** user from the JWT.

### Response `401`

If the token has no `sub` / `NameIdentifier` (delete handler requires current user id).

---

## Business rules (summary)

| Topic | Rule |
|-------|------|
| **System roles** | Names `Admin`, `Editor`, `Moderator`: cannot be **deleted** or **renamed**; **permissions** can still be updated. |
| **Delete role** | Fails if any user still has that role. |
| **Permission names** | Must match `GET /permissions` exactly (e.g. `slider.manage`). |
| **Role names for users** | Must match an existing role name (e.g. `Admin`, `Editor`). |
| **JWT refresh** | Editing roles/permissions in DB does not change existing JWTs until the user logs in again. |

---

## Quick permission name list (reference)

These may appear in `GET /permissions` (subject to server seed):

| `name` |
|--------|
| `slider.manage` |
| `categories.manage` |
| `articles.manage` |
| `social.manage` |
| `comments.moderate` |
| `users.manage` |
| `roles.manage` |
| `media.upload` |
