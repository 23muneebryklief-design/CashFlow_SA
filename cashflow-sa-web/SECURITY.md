# Frontend security notes

- API URL is configurable through `VITE_API_URL`; the development fallback remains `http://localhost:5081/api`.
- Access tokens are validated for expiry before a session is restored.
- An expired access token automatically clears the local session.
- HTTP 401 responses clear the session and notify the auth context immediately.
- Multiple browser tabs synchronize logout through the `storage` event.
- Route authorization is enforced client-side by `ProtectedRoute`; backend authorization remains the authoritative security boundary.
- The backend currently has no refresh-token endpoint exposed by `AuthController`, so the frontend does not attempt to invent a refresh flow.
- Do not put secrets, API keys, or database credentials in `VITE_*` variables; Vite exposes these values to the browser bundle.
