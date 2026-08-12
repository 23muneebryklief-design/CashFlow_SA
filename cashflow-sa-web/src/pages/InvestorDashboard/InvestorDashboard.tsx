import { useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { getWalletBalance, type WalletBalance } from "../../Services/walletService";
import WalletActionModal from "../../components/Dashboard/WalletActionModal/WalletActionModal";
import styles from "./InvestorDashboard.module.css";

export default function InvestorDashboard() {
  const { user } = useAuth();

  const [wallet, setWallet] = useState<WalletBalance | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeModal, setActiveModal] = useState<"deposit" | "withdraw" | null>(null);

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

  // Separate from the mount-time fetch above (loadWallet is defined inline
  // inside that effect on purpose -- react-hooks/set-state-in-effect flags
  // calling an externally-defined setState-calling function from inside an
  // effect). This one is only ever invoked from an event handler (the
  // modal's onSuccess), never from an effect, so it's fine as a normal
  // function.
  async function refreshWallet() {
    if (!user) return;

    try {
      const walletData = await getWalletBalance(user.userId);
      setWallet(walletData);
    } catch {
      // Deposit/withdraw already succeeded at this point -- just leave the
      // displayed balance as-is rather than surfacing an error for a
      // refresh-only failure.
    }
  }

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
          <h1>Welcome back, {user?.email}</h1>
        </div>
      </header>

      <section className={styles.walletCard}>
        <span>Wallet Balance</span>
        <h2>
          {(wallet?.balance ?? 0).toLocaleString("en-ZA", { maximumFractionDigits: 2 })}
          <small> {wallet?.currency}</small>
        </h2>

        <div className={styles.walletActions}>
          <button
            type="button"
            className={styles.walletActionPrimary}
            onClick={() => setActiveModal("deposit")}
          >
            Add money
          </button>

          <button
            type="button"
            className={styles.walletActionSecondary}
            onClick={() => setActiveModal("withdraw")}
          >
            Withdraw
          </button>
        </div>
      </section>

      {user && wallet && (
        <WalletActionModal
          isOpen={activeModal !== null}
          onClose={() => setActiveModal(null)}
          mode={activeModal ?? "deposit"}
          userId={user.userId}
          currentBalance={wallet.balance}
          currency={wallet.currency}
          // Re-fetch the confirmed-good balance endpoint rather than trust
          // the deposit/withdraw response body (unconfirmed shape -- see
          // walletService.ts notes; this is what was crashing to a blank
          // page after a successful deposit).
          onSuccess={refreshWallet}
        />
      )}
    </main>
  );
}
