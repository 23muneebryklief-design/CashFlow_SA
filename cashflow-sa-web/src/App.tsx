import { BrowserRouter, Routes, Route } from "react-router-dom";

import Home from "./pages/Home/Home";
import Login from "./pages/Login/Login";
import Register from "./pages/Register/Register";

import InvestorMarketplace from "./pages/InvestorMarketplace/InvestorMarketplace";
import InvestorDashboard from "./pages/InvestorDashboard/InvestorDashboard";
import SMEDashboard from "./pages/SMEDashboard/SMEDashboard";
import FicaVerification from "./pages/FicaVerification/FicaVerification";
import Profile from "./pages/Profile/Profile";
import Invoices from "./pages/Invoices/Invoices";

import AdminLogin from "./pages/AdminLogin/AdminLogin";
import AdminDashboard from "./pages/AdminDashboard/AdminDashboard";
import AuditorDashboard from "./pages/AuditorDashboard/AuditorDashboard";
import CreditAnalystDashboard from "./pages/CreditAnalystDashboard/CreditAnalystDashboard";

import ProtectedRoute from "./components/ProtectedRoute/ProtectedRoute";
import AppShell from "./components/layout/AppShell/AppShell";

function App() {
  return (
    <BrowserRouter>
      <Routes>

        {/* Public Routes */}
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        {/* Admin login is intentionally not linked anywhere in the UI --
            reachable only by typing /admin directly. */}
        <Route path="/admin" element={<AdminLogin />} />

        {/* Investor Routes */}
        <Route
          path="/investor-marketplace"
          element={
            <ProtectedRoute requiredRole="Investor">
              <AppShell>
                <InvestorMarketplace />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/investor-dashboard"
          element={
            <ProtectedRoute requiredRole="Investor">
              <AppShell>
                <InvestorDashboard />
              </AppShell>
            </ProtectedRoute>
          }
        />

        {/* SME Routes */}
        <Route
          path="/sme-dashboard"
          element={
            <ProtectedRoute requiredRole="SME">
              <AppShell>
                <SMEDashboard />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/invoices"
          element={
            <ProtectedRoute requiredRole="SME">
              <AppShell>
                <Invoices />
              </AppShell>
            </ProtectedRoute>
          }
        />
        <Route
          path="/fica-verification"
          element={
            <ProtectedRoute requiredRole="SME">
              <AppShell>
                <FicaVerification />
              </AppShell>
            </ProtectedRoute>
          }
        />

        <Route
          path="/profile"
          element={
            <ProtectedRoute requiredRole={["SME", "Investor", "Admin", "SuperAdmin", "CreditAnalyst", "Auditor"]}>
              <AppShell>
                <Profile />
              </AppShell>
            </ProtectedRoute>
          }
        />


        {/* Admin Routes */}
        <Route
          path="/admin-dashboard"
          element={
            <ProtectedRoute requiredRole={["Admin", "SuperAdmin", "CreditAnalyst"]}>
              <AppShell>
                <AdminDashboard />
              </AppShell>
            </ProtectedRoute>
          }
        />

        {/* Credit Analyst Routes */}
        <Route
          path="/credit-review"
          element={
            <ProtectedRoute requiredRole={["CreditAnalyst", "Admin"]}>
              <AppShell>
                <CreditAnalystDashboard />
              </AppShell>
            </ProtectedRoute>
          }
        />

        {/* Auditor Routes */}
        <Route
          path="/auditor-kyc"
          element={
            <ProtectedRoute requiredRole={["Auditor", "Admin", "SuperAdmin"]}>
              <AppShell>
                <AuditorDashboard />
              </AppShell>
            </ProtectedRoute>
          }
        />

        {/* Catch-all Route (Optional) */}
        <Route
          path="*"
          element={<h1>404 - Page Not Found</h1>}
        />

      </Routes>
    </BrowserRouter>
  );
}

export default App;
