import { useAuth } from "../../Hooks/useAuth";
import CreateAdminForm from "../../components/Admin/CreateAdminForm/CreateAdminForm";
import styles from "./AdminDashboard.module.css";

export default function AdminDashboard() {
  const { user } = useAuth();

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Admin Portal</p>
        <h1>Welcome back, {user?.email}</h1>
      </header>

      <section className={styles.placeholder}>
        <h2>Coming soon</h2>
        <p>This portal will let admins:</p>
        <ul>
          <li>Review and approve KYC submissions</li>
          <li>Moderate marketplace listings</li>
          <li>View platform-wide audit logs</li>
          <li>Manage users and roles</li>
        </ul>
      </section>

      {user?.role === "SuperAdmin" && <CreateAdminForm />}
    </main>
  );
}
