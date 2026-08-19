import { useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { getNotifications, type NotificationItem } from "../../Services/notificationService";
import styles from "./Notifications.module.css";

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-ZA", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

export default function Notifications() {
  const { user } = useAuth();
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    async function load() {
      if (!user?.userId) return;
      setLoading(true);
      setError("");
      try {
        const result = await getNotifications(user.userId);
        if (active) setItems(result);
      } catch {
        if (active) setError("Unable to load notifications. Please try again.");
      } finally {
        if (active) setLoading(false);
      }
    }
    load();
    return () => { active = false; };
  }, [user?.userId]);

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Account</p>
          <h1>Notifications</h1>
          <p className={styles.subtitle}>Updates and activity related to your CashFlow SA account.</p>
        </div>
        <div className={styles.count}>{items.filter((item) => !item.isRead).length} unread</div>
      </header>

      {loading && <div className={styles.state}>Loading notifications…</div>}
      {!loading && error && <div className={styles.error}>{error}</div>}
      {!loading && !error && items.length === 0 && (
        <div className={styles.empty}>
          <div className={styles.emptyIcon}>✓</div>
          <h2>You’re all caught up</h2>
          <p>There are no notifications for your account yet.</p>
        </div>
      )}

      {!loading && !error && items.length > 0 && (
        <section className={styles.list} aria-label="Notification history">
          {items.map((item) => (
            <article key={item.notificationId} className={`${styles.card} ${item.isRead ? "" : styles.unread}`}>
              <div className={styles.dot} aria-hidden="true" />
              <div className={styles.body}>
                <div className={styles.cardTop}>
                  <h2>{item.title}</h2>
                  <time dateTime={item.createdAt}>{formatDate(item.createdAt)}</time>
                </div>
                <p>{item.message}</p>
              </div>
            </article>
          ))}
        </section>
      )}
    </main>
  );
}
