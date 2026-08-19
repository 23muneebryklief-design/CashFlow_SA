import { Navigate } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";

interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredRole?: string | string[];
}

// Blocks access to a route unless the user is logged in -- and optionally,
// unless they hold a specific role (or one of several). Without this,
// anyone could type /dashboard directly into the address bar and see it,
// logged in or not.
export default function ProtectedRoute({ children, requiredRole }: ProtectedRouteProps) {
  const { isAuthenticated, user, isInitializing } = useAuth();

  if (isInitializing) return null;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const normalizeRole = (role: string | undefined) =>
    role?.trim().toLowerCase() ?? "";

  const allowedRoles = Array.isArray(requiredRole)
    ? requiredRole
    : requiredRole
    ? [requiredRole]
    : null;

  if (allowedRoles && (!user || !allowedRoles.some((role) => normalizeRole(role) === normalizeRole(user.role)))) {
    // Keep authenticated users inside the application. If a role mismatch
    // occurs, send them to the dashboard that matches their actual role
    // instead of the public homepage.
    const role = normalizeRole(user?.role);
    const fallback =
      role === "investor" ? "/investor-dashboard" :
      role === "sme" ? "/sme-dashboard" :
      role === "auditor" ? "/auditor-kyc" :
      role === "creditanalyst" ? "/credit-review" :
      role === "admin" || role === "superadmin" ? "/admin-dashboard" :
      "/login";
    return <Navigate to={fallback} replace />;
  }

  return <>{children}</>;
}