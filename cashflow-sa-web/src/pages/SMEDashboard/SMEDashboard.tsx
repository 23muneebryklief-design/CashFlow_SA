import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";
import { useKycStatus } from "../../Hooks/useKycStatus";
import { getWalletBalance, type WalletBalance } from "../../Services/walletService";
import { getInvoicesBySme, type InvoiceSummary } from "../../Services/invoiceService";
import WalletActionModal from "../../components/Dashboard/WalletActionModal/WalletActionModal";
import styles from "./SMEDashboard.module.css";

const statusLabel: Record<string, string> = {
  Draft: "Draft",
  Submitted: "Submitted",
  UnderReview: "Under review",
  Approved: "Approved",
  Rejected: "Rejected",
  Listed: "Listed",
};

export default function SMEDashboard() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { status: kycStatus, isLoading: isKycLoading } = useKycStatus();
  const [wallet, setWallet] = useState<WalletBalance | null>(null);
  const [invoices, setInvoices] = useState<InvoiceSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isWithdrawOpen, setIsWithdrawOpen] = useState(false);

  const loadDashboard = useCallback(async () => {
    if (!user) return;
    setIsLoading(true);
    setError(null);
    try {
      const walletData = await getWalletBalance(user.userId);
      setWallet(walletData);
      if (user.profileId && kycStatus === "Verified") {
        setInvoices(await getInvoicesBySme(user.profileId));
      } else {
        setInvoices([]);
      }
    } catch {
      setError("Could not load your dashboard. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [user, kycStatus]);

  useEffect(() => {
    if (!isKycLoading) void loadDashboard();
  }, [isKycLoading, loadDashboard]);

  async function refreshWallet() {
    if (!user) return;
    try {
      setWallet(await getWalletBalance(user.userId));
    } catch {
      // Keep the last known balance after a successful wallet action.
    }
  }

  const counts = invoices.reduce(
    (acc, invoice) => {
      acc.total += 1;
      acc[invoice.status] = (acc[invoice.status] ?? 0) + 1;
      return acc;
    },
    { total: 0 } as Record<string, number>
  );

  if (isLoading || isKycLoading) {
    return <main className={styles.page}><p className={styles.status}>Loading your business dashboard...</p></main>;
  }

  if (error) {
    return <main className={styles.page}><p className={styles.status}>{error}</p></main>;
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>SME Dashboard</p>
          <h1>Welcome back, {user?.email}</h1>
          <p className={styles.subhead}>Manage verification, invoices and your path to working capital.</p>
        </div>
        <button type="button" className={styles.primaryButton} onClick={() => navigate(kycStatus === "Verified" ? "/invoices" : "/fica-verification")}>
          {kycStatus === "Verified" ? "Upload invoice" : "Complete FICA"}
        </button>
      </header>

      <section className={styles.metrics}>
        <article className={styles.metricCard}>
          <span>FICA status</span>
          <strong>{kycStatus === "Verified" ? "Verified" : kycStatus === "Pending" ? "Under review" : "Action required"}</strong>
          <button type="button" onClick={() => navigate("/fica-verification")}>View verification</button>
        </article>
        <article className={styles.metricCard}>
          <span>Total invoices</span>
          <strong>{counts.total ?? 0}</strong>
          <button type="button" onClick={() => navigate("/invoices")}>Manage invoices</button>
        </article>
        <article className={styles.metricCard}>
          <span>Approved invoices</span>
          <strong>{counts.Approved ?? 0}</strong>
          <span className={styles.metricHint}>Eligible for financing requests</span>
        </article>
      </section>

      <section className={styles.walletCard}>
        <div>
          <span>Available wallet balance</span>
          <h2>{(wallet?.balance ?? 0).toLocaleString("en-ZA", { maximumFractionDigits: 2 })}<small> {wallet?.currency}</small></h2>
          <p>Funds released to your business wallet can be withdrawn to your bank account.</p>
        </div>
        <button type="button" className={styles.walletActionSecondary} onClick={() => setIsWithdrawOpen(true)}>Withdraw</button>
      </section>

      <section className={styles.pipelineCard}>
        <div className={styles.sectionHeader}>
          <div>
            <p className={styles.eyebrow}>Invoice pipeline</p>
            <h2>What happens next</h2>
          </div>
          <span className={styles.pipelineCount}>{counts.total ?? 0} invoices</span>
        </div>
        <div className={styles.pipeline}>
          {[
            ["Draft", "Complete invoice details and submit for review."],
            ["Under review", "Our credit team checks the invoice."],
            ["Approved", "Request financing against the approved invoice."],
            ["Listed", "The opportunity is made available to investors."],
          ].map(([title, description]) => (
            <div key={title} className={styles.pipelineStep}>
              <span className={styles.pipelineDot} />
              <div><strong>{title}</strong><p>{description}</p></div>
            </div>
          ))}
        </div>
      </section>

      {invoices.length > 0 && (
        <section className={styles.recentCard}>
          <div className={styles.sectionHeader}>
            <div><p className={styles.eyebrow}>Recent activity</p><h2>Latest invoices</h2></div>
            <button type="button" className={styles.textButton} onClick={() => navigate("/invoices")}>View all</button>
          </div>
          <div className={styles.recentList}>
            {invoices.slice(0, 5).map((invoice) => (
              <div className={styles.recentRow} key={invoice.invoiceId}>
                <div><strong>{invoice.invoiceNumber.startsWith("DRAFT-") ? "New invoice" : invoice.invoiceNumber}</strong><span>Due {new Date(invoice.dueDate).toLocaleDateString("en-ZA")}</span></div>
                <span className={styles.statusPill}>{statusLabel[invoice.status] ?? invoice.status}</span>
                <strong>R {invoice.amount.toLocaleString("en-ZA", { minimumFractionDigits: 2 })}</strong>
              </div>
            ))}
          </div>
        </section>
      )}

      {user && wallet && <WalletActionModal isOpen={isWithdrawOpen} onClose={() => setIsWithdrawOpen(false)} mode="withdraw" userId={user.userId} currentBalance={wallet.balance} currency={wallet.currency} onSuccess={refreshWallet} />}
    </main>
  );
}
