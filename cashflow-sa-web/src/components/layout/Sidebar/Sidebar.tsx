import { NavLink } from "react-router-dom";
import { useAuth } from "../../../Hooks/useAuth";
import styles from "./Sidebar.module.css";
import { useEffect, useState } from "react";
import { getNotifications } from "../../../Services/notificationService";

interface SidebarLink {
  to: string;
  label: string;
  badge?: string;
}

const investorLinks: SidebarLink[] = [
  { to: "/investor-marketplace", label: "Marketplace" },
  { to: "/investor-dashboard", label: "Dashboard" },
  { to: "/profile", label: "Profile" },
];

const smeLinks: SidebarLink[] = [
  { to: "/sme-dashboard", label: "Dashboard" },
  { to: "/invoices", label: "Invoices" },
  { to: "/fica-verification", label: "FICA Verification" },
  { to: "/profile", label: "Profile" },
];

const adminLinks: SidebarLink[] = [
  { to: "/admin-dashboard", label: "Overview" },
  { to: "/credit-review", label: "Review Queues" },
  { to: "/invoice-review", label: "Invoice Review" },
  { to: "/settlements", label: "Settlements" },
  { to: "/profile", label: "Profile" },
];

const creditAnalystLinks: SidebarLink[] = [
  { to: "/credit-review", label: "Review Queues" },
  { to: "/settlements", label: "Settlements" },
  { to: "/profile", label: "Profile" },
];

const auditorLinks: SidebarLink[] = [
  { to: "/auditor-kyc", label: "KYC Review" },
  { to: "/profile", label: "Profile" },
];

function getLinksForRole(role: string | undefined): SidebarLink[] {
  if (role === "Investor") return investorLinks;
  if (role === "SME") return smeLinks;
  if (role === "Auditor") return auditorLinks;
  if (role === "CreditAnalyst") return creditAnalystLinks;
  if (role === "Admin" || role === "SuperAdmin") return adminLinks;
  return [];
}

export default function Sidebar() {
  const { user, logout } = useAuth();
  const links = getLinksForRole(user?.role);
  const [unreadCount, setUnreadCount] = useState(0);

  useEffect(() => {
    let active = true;
    async function loadUnreadCount() {
      if (!user?.userId) return;
      try {
        const notifications = await getNotifications(user.userId);
        if (active) setUnreadCount(notifications.filter((item) => !item.isRead).length);
      } catch {
        if (active) setUnreadCount(0);
      }
    }
    loadUnreadCount();
    return () => { active = false; };
  }, [user?.userId]);

  return (
    <aside className={styles.sidebar}>
      <div className={styles.logo}>
        <div className={styles.logoMark}>
          <span className={styles.logoLineEmerald} />
          <span className={styles.logoLineGold} />
        </div>
        <span>CashFlow SA</span>
      </div>

      <nav className={styles.nav}>
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            className={({ isActive }) =>
              isActive ? `${styles.link} ${styles.active}` : styles.link
            }
          >
            {link.label}
          </NavLink>
        ))}

        <NavLink
          to="/notifications"
          className={({ isActive }) =>
            isActive ? `${styles.link} ${styles.active}` : styles.link
          }
        >
          <span>Notifications</span>
          {unreadCount > 0 && <span className={styles.notificationBadge}>{unreadCount > 99 ? "99+" : unreadCount}</span>}
        </NavLink>
      </nav>

      <div className={styles.footer}>
        <span className={styles.email} title={user?.email}>
          {user?.email}
        </span>
        <button className={styles.logoutBtn} onClick={logout}>
          Log out
        </button>
      </div>
    </aside>
  );
}
