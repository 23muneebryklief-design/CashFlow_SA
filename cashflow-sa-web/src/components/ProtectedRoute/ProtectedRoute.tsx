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
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const allowedRoles = Array.isArray(requiredRole)
    ? requiredRole
    : requiredRole
    ? [requiredRole]
    : null;

  if (allowedRoles && (!user || !allowedRoles.includes(user.role))) {
    // Logged in, but the wrong role for this specific page (e.g. an SME
    // trying to view the Investor dashboard) -- send them home rather than
    // showing a page meant for someone else.
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
}