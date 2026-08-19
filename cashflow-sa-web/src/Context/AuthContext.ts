import { createContext, useState, useEffect, useCallback, createElement, type ReactNode } from "react";
import { jwtDecode } from "jwt-decode";
import LoadingScreen from "../components/Shared/LoadingScreen/LoadingScreen";
import { login as loginApi, logout as logoutApi, type LoginRequest } from "../Services/authService";
import { clearAuthTokens, getAccessToken, setAuthTokens } from "../utils/storage";

interface DecodedToken {
  sub?: string;
  email?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: string;
  role?: string;
  profileId?: string;
  exp?: number;
}

interface AuthUser {
  userId: string;
  email: string;
  role: string;
  profileId?: string;
}

interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<AuthUser>;
  logout: () => void;
  isInitializing: boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

function decodeUser(token: string): AuthUser {
  const decoded = jwtDecode<DecodedToken>(token);
  const role = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? decoded.role;

  if (!decoded.sub || !decoded.email || !role || !decoded.exp) {
    throw new Error("The authentication token is missing required claims.");
  }

  return { userId: decoded.sub, email: decoded.email, role, profileId: decoded.profileId };
}

function isTokenValid(token: string): boolean {
  try {
    const { exp } = jwtDecode<DecodedToken>(token);
    return typeof exp === "number" && exp * 1000 > Date.now();
  } catch {
    return false;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);

  const clearSession = useCallback(() => {
    clearAuthTokens();
    setUser(null);
  }, []);

  useEffect(() => {
    const restoreSession = () => {
      const token = getAccessToken();
      if (!token || !isTokenValid(token)) {
        clearSession();
        setIsInitializing(false);
        return;
      }

      try {
        setUser(decodeUser(token));
      } catch {
        clearSession();
      } finally {
        setIsInitializing(false);
      }
    };

    restoreSession();

    // Keep multiple tabs in sync when one tab signs out.
    const onStorage = (event: StorageEvent) => {
      if (event.key === "accessToken" && !event.newValue) clearSession();
    };
    const onAuthExpired = () => clearSession();
    window.addEventListener("storage", onStorage);
    window.addEventListener("cashflow:auth-expired", onAuthExpired);
    return () => {
      window.removeEventListener("storage", onStorage);
      window.removeEventListener("cashflow:auth-expired", onAuthExpired);
    };
  }, [clearSession]);

  useEffect(() => {
    if (!user) return;
    const token = getAccessToken();
    if (!token) return;

    try {
      const { exp } = jwtDecode<DecodedToken>(token);
      if (!exp) return;
      const delay = Math.max(0, exp * 1000 - Date.now());
      const timer = window.setTimeout(clearSession, delay);
      return () => window.clearTimeout(timer);
    } catch {
      clearSession();
    }
  }, [user, clearSession]);

  async function login(credentials: LoginRequest) {
    const result = await loginApi(credentials);
    if (!result.accessToken || !result.refreshToken) throw new Error("Login response did not contain authentication tokens.");
    setAuthTokens(result.accessToken, result.refreshToken);
    const decodedUser = decodeUser(result.accessToken);
    setUser(decodedUser);
    return decodedUser;
  }

  function logout() {
    logoutApi();
    setUser(null);
  }

  return createElement(
    AuthContext.Provider,
    { value: { user, isAuthenticated: !!user, login, logout, isInitializing } },
    isInitializing ? createElement(LoadingScreen, { message: "Restoring your session…" }) : children
  );
}
