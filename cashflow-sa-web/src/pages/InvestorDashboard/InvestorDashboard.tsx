import { useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { getWalletBalance, getWalletTransactions, type WalletBalance, type WalletTransaction } from "../../Services/walletService";
import { getInvestorInvestments, type Investment } from "../../Services/investmentService";
import WalletActionModal from "../../components/Dashboard/WalletActionModal/WalletActionModal";
import styles from "./InvestorDashboard.module.css";

const money = (n: number) => n.toLocaleString("en-ZA", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export default function InvestorDashboard() {
  const { user } = useAuth();
  const [wallet, setWallet] = useState<WalletBalance | null>(null);
  const [investments, setInvestments] = useState<Investment[]>([]);
  const [transactions, setTransactions] = useState<WalletTransaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeModal, setActiveModal] = useState<"deposit" | "withdraw" | null>(null);

  async function refresh() {
    if (!user) return;
    const [walletData, investmentData, transactionData] = await Promise.all([
      getWalletBalance(user.userId),
      user.profileId ? getInvestorInvestments(user.profileId) : Promise.resolve([]),
      getWalletTransactions(user.userId),
    ]);
    setWallet(walletData); setInvestments(investmentData); setTransactions(transactionData);
  }

  useEffect(() => {
    if (!user) return;
    setIsLoading(true);
    refresh().catch(() => setError("Could not load your investor dashboard. Please try again.")).finally(() => setIsLoading(false));
  }, [user]);

  if (isLoading) return <main className={styles.page}><p className={styles.status}>Loading your dashboard...</p></main>;
  if (error) return <main className={styles.page}><p className={styles.status}>{error}</p></main>;

  const committed = investments.reduce((sum, item) => sum + item.amount, 0);
  const returned = investments.reduce((sum, item) => sum + (item.returnAmount ?? 0), 0);

  return (
    <main className={styles.page}>
      <header className={styles.header}><div><p className={styles.eyebrow}>Investor Dashboard</p><h1>Welcome back, {user?.email}</h1></div></header>
      <section className={styles.walletCard}>
        <span>Wallet Balance</span>
        <h2>{money(wallet?.balance ?? 0)}<small> {wallet?.currency}</small></h2>
        <div className={styles.walletActions}>
          <button className={styles.walletActionPrimary} onClick={() => setActiveModal("deposit")}>Add money</button>
          <button className={styles.walletActionSecondary} onClick={() => setActiveModal("withdraw")}>Withdraw</button>
        </div>
      </section>
      <section className={styles.statsGrid}>
        <div><span>Committed</span><strong>R {money(committed)}</strong></div>
        <div><span>Active investments</span><strong>{investments.filter(i => !["Returned", "Cancelled"].includes(i.status)).length}</strong></div>
        <div><span>Returned</span><strong>R {money(returned)}</strong></div>
      </section>
      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Your investments</h2>
        {investments.length === 0 ? <p className={styles.empty}>No investments yet. Browse the marketplace to get started.</p> :
          <div className={styles.list}>{investments.map(i => <article key={i.investmentId} className={styles.row}><div><strong>{i.industry}</strong><span>{new Date(i.investedAt).toLocaleDateString("en-ZA")} · {i.tenorDays} days</span></div><div><strong>R {money(i.amount)}</strong><span>{i.status}</span></div></article>)}</div>}
      </section>
      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Wallet activity</h2>
        {transactions.length === 0 ? <p className={styles.empty}>No wallet transactions yet.</p> : <div className={styles.list}>{transactions.slice(0, 8).map(t => <article key={t.transactionId} className={styles.row}><div><strong>{t.description || t.referenceType}</strong><span>{new Date(t.createdAt).toLocaleString("en-ZA")}</span></div><div><strong className={t.type === "Debit" ? styles.debit : styles.credit}>{t.type === "Debit" ? "−" : "+"} R {money(t.amount)}</strong></div></article>)}</div>}
      </section>
      {user && wallet && <WalletActionModal isOpen={activeModal !== null} onClose={() => setActiveModal(null)} mode={activeModal ?? "deposit"} userId={user.userId} currentBalance={wallet.balance} currency={wallet.currency} onSuccess={() => refresh().catch(() => undefined)} />}
    </main>
  );
}
