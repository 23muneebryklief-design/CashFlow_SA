import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { getNotifications, markNotificationAsRead, type NotificationItem } from "../../Services/notificationService";
import { startNotificationHub, stopNotificationHub } from "../../Services/notificationHubService";
import styles from "./Notifications.module.css";

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-ZA", { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
}

export default function Notifications() {
  const { user } = useAuth();
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [realtime, setRealtime] = useState(false);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!user?.userId) return;
    setLoading(true); setError("");
    try { setItems(await getNotifications(user.userId)); }
    catch { setError("Unable to load notifications. Please try again."); }
    finally { setLoading(false); }
  }, [user?.userId]);

  useEffect(() => {
    if (!user?.userId) return;
    let active = true;
    void load();
    void startNotificationHub((notification) => {
      if (!active) return;
      setItems((current) => [notification, ...current.filter((item) => item.notificationId !== notification.notificationId)]);
    }).then(() => { if (active) setRealtime(true); }).catch(() => { if (active) setRealtime(false); });
    return () => { active = false; void stopNotificationHub(); };
  }, [user?.userId, load]);

  async function markRead(item: NotificationItem) {
    if (item.isRead) return;
    setBusyId(item.notificationId);
    try {
      await markNotificationAsRead(item.notificationId);
      setItems((current) => current.map((entry) => entry.notificationId === item.notificationId ? { ...entry, isRead: true, readAt: new Date().toISOString() } : entry));
    } catch { setError("Could not mark the notification as read."); }
    finally { setBusyId(null); }
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div><p className={styles.eyebrow}>Account</p><h1>Notifications</h1><p className={styles.subtitle}>Updates and activity related to your CashFlow SA account.</p></div>
        <div className={styles.headerMeta}>
          <span className={styles.connectionBadge}>{realtime ? "● Live" : "○ History only"}</span>
          <div className={styles.count}>{items.filter((item) => !item.isRead).length} unread</div>
        </div>
      </header>

      {loading && <div className={styles.state}>Loading notifications…</div>}
      {!loading && error && <div className={styles.error}>{error}</div>}
      {!loading && !error && items.length === 0 && <div className={styles.empty}><div className={styles.emptyIcon}>✓</div><h2>You’re all caught up</h2><p>There are no notifications for your account yet.</p></div>}
      {!loading && !error && items.length > 0 && (
        <section className={styles.list} aria-label="Notification history">
          {items.map((item) => (
            <article key={item.notificationId} className={`${styles.card} ${item.isRead ? "" : styles.unread}`}>
              <div className={styles.dot} aria-hidden="true" />
              <div className={styles.body}>
                <div className={styles.cardTop}><h2>{item.title}</h2><time dateTime={item.createdAt}>{formatDate(item.createdAt)}</time></div>
                <p>{item.message}</p>
                {!item.isRead && <button type="button" className={styles.readButton} onClick={() => void markRead(item)} disabled={busyId === item.notificationId}>{busyId === item.notificationId ? "Saving…" : "Mark as read"}</button>}
              </div>
            </article>
          ))}
        </section>
      )}
    </main>
  );
}
