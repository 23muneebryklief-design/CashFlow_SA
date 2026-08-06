import { NavLink } from "react-router-dom";
import { useAuth } from "../../../Hooks/useAuth";
import styles from "./Sidebar.module.css";

interface SidebarLink {
  to: string;
  label: string;
}

const investorLinks: SidebarLink[] = [
  { to: "/investor-marketplace", label: "Marketplace" },
  { to: "/investor-dashboard", label: "Dashboard" },
];

const smeLinks: SidebarLink[] = [
  { to: "/sme-dashboard", label: "Dashboard" },
];

const adminLinks: SidebarLink[] = [
  { to: "/admin-dashboard", label: "Overview" },
];

function getLinksForRole(role: string | undefined): SidebarLink[] {
  if (role === "Investor") return investorLinks;
  if (role === "SME") return smeLinks;
  if (role === "Admin" || role === "SuperAdmin") return adminLinks;
  return [];
}

export default function Sidebar() {
  const { user, logout } = useAuth();
  const links = getLinksForRole(user?.role);

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
