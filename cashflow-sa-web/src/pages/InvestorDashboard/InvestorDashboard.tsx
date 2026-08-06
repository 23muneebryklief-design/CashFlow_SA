import { useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { getWalletBalance, type WalletBalance } from "../../Services/walletService";
import styles from "./InvestorDashboard.module.css";

export default function InvestorDashboard() {
  const { user } = useAuth();

  const [wallet, setWallet] = useState<WalletBalance | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return;

    async function loadWallet() {
      try {
        const walletData = await getWalletBalance(user!.userId);
        setWallet(walletData);
      } catch {
        setError("Could not load your dashboard. Please try again.");
      } finally {
        setIsLoading(false);
      }
    }

    loadWallet();
  }, [user]);

  if (isLoading) {
    return (
      <main className={styles.page}>
        <p className={styles.status}>Loading your dashboard...</p>
      </main>
    );
  }

  if (error) {
    return (
      <main className={styles.page}>
        <p className={styles.status}>{error}</p>
      </main>
    );
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Investor Dashboard</p>
          <h1>Welcome back, {user?.email }</h1>
        </div>
      </header>

      <section className={styles.walletCard}>
        <span>Wallet Balance</span>
        <h2>
          {wallet?.balance.toLocaleString("en-ZA", { maximumFractionDigits: 2 })}
          <small> {wallet?.currency}</small>
        </h2>
      </section>
    </main>
  );
}
