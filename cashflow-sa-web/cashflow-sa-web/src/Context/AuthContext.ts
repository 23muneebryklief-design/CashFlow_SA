import { createContext, useState, useEffect, createElement, type ReactNode } from "react";
import { jwtDecode } from "jwt-decode";
import { login as loginApi, logout as logoutApi, type LoginRequest } from "../Services/authService";

// Matches the exact claim keys your JwtTokenService writes -- "sub" and "email"
// are short names (JwtRegisteredClaimNames), but Role is NOT "role" -- .NET's
// ClaimTypes.Role serializes as this full URI in the raw JWT payload.
interface DecodedToken {
  sub: string;
  email: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;
  exp: number;
}

interface AuthUser {
  userId: string;
  email: string;
  role: string;
}

interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<AuthUser>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

function decodeUser(token: string): AuthUser {
  const decoded = jwtDecode<DecodedToken>(token);
  return {
    userId: decoded.sub,
    email: decoded.email,
    role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
  };
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);

  // On first load, restore the session from a token already in storage
  // (e.g. the user refreshed the page) rather than forcing a re-login.
  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (token) {
      try {
        setUser(decodeUser(token));
      } catch {
        localStorage.removeItem("accessToken");
      }
    }
  }, []);

  async function login(credentials: LoginRequest) {
    const result = await loginApi(credentials);
    localStorage.setItem("accessToken", result.accessToken);
    localStorage.setItem("refreshToken", result.refreshToken);
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
    { value: { user, isAuthenticated: !!user, login, logout } },
    children
  );
}