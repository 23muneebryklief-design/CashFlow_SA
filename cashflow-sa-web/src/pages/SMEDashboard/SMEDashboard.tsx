import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";
import { getWalletBalance, type WalletBalance } from "../../Services/walletService";
import WalletActionModal from "../../components/Dashboard/WalletActionModal/WalletActionModal";
import styles from "./SMEDashboard.module.css";

export default function SMEDashboard() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [wallet, setWallet] = useState<WalletBalance | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);

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

  // See InvestorDashboard.tsx for why this is a separate, non-effect-called
  // function rather than the same one the mount effect above uses.
  async function refreshWallet() {
    if (!user) return;

    try {
      const walletData = await getWalletBalance(user.userId);
      setWallet(walletData);
    } catch {
      // Withdraw already succeeded at this point -- leave the displayed
      // balance as-is rather than surfacing an error for a refresh-only
      // failure.
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
          <p className={styles.eyebrow}>SME Dashboard</p>
          <h1>Welcome back, {user?.email}</h1>
        </div>
      </header>

      <section className={styles.walletCard}>
        <span>Wallet Balance</span>
        <h2>
          {(wallet?.balance ?? 0).toLocaleString("en-ZA", { maximumFractionDigits: 2 })}
          <small> {wallet?.currency}</small>
        </h2>

        {/* SMEs can only withdraw funds already released to their wallet
            (e.g. from a funded invoice) -- unlike investors, they don't
            top up their own balance directly. */}
        <div className={styles.walletActions}>
          <button
            type="button"
            className={styles.walletActionSecondary}
            onClick={() => setIsWithdrawOpen(true)}
          >
            Withdraw
          </button>
        </div>
      </section>

      <section className={styles.invoiceCard}>
        <div>
          <p className={styles.invoiceEyebrow}>Invoice financing</p>
          <h2>Turn an unpaid invoice into working capital.</h2>
          <p>Upload a PDF invoice once your FICA verification is complete. Your invoice starts as a draft while its details are prepared for review.</p>
        </div>
        <button type="button" onClick={() => navigate("/invoices")}>
          Manage invoices
        </button>
      </section>

      <section className={styles.placeholder}>
        <h2>Coming soon</h2>
        <p>Funding requests, funding progress and additional business tools will appear here as they are enabled.</p>
      </section>

      {user && wallet && (
        <WalletActionModal
          isOpen={isWithdrawOpen}
          onClose={() => setIsWithdrawOpen(false)}
          mode="withdraw"
          userId={user.userId}
          currentBalance={wallet.balance}
          currency={wallet.currency}
          onSuccess={refreshWallet}
        />
      )}
    </main>
  );
}
