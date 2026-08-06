import styles from "./SMEDashboard.module.css";
import { useAuth } from "../../Hooks/useAuth";

export default function SMEDashboard() {
  const { user } = useAuth();

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <h1>SME Dashboard</h1>

        <p>Welcome back!</p>

        <div className={styles.info}>
          <p>
            <strong>Email:</strong> {user?.email}
          </p>

          <p>
            <strong>Role:</strong> {user?.role}
          </p>

          <p>
            <strong>User ID:</strong> {user?.userId}
          </p>
        </div>

        <div className={styles.placeholder}>
          <h2>🚧 Coming Soon</h2>

          <p>
            This dashboard will allow SMEs to:
          </p>

          <ul>
            <li>Create funding requests</li>
            <li>Manage invoices</li>
            <li>Track investments</li>
            <li>View funding progress</li>
            <li>Manage company profile</li>
          </ul>
        </div>
      </div>
    </div>
  );
}