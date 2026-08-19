import { useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { getWalletBalance, getWalletTransactions, type WalletBalance, type WalletTransaction } from "../../Services/walletService";
import { getInvestorInvestments, type Investment } from "../../Services/investmentService";
import { getCampaignStatus, type CampaignStatus } from "../../Services/fundingService";
import WalletActionModal from "../../components/Dashboard/WalletActionModal/WalletActionModal";
import Modal from "../../components/Shared/Modal/Modal";
import styles from "./InvestorDashboard.module.css";

const money = (n: number) => n.toLocaleString("en-ZA", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

const statusLabel = (status: string) => status.replace(/([a-z])([A-Z])/g, "$1 $2");

export default function InvestorDashboard() {
  const { user } = useAuth();
  const [wallet, setWallet] = useState<WalletBalance | null>(null);
  const [investments, setInvestments] = useState<Investment[]>([]);
  const [transactions, setTransactions] = useState<WalletTransaction[]>([]);
  const [campaignStatuses, setCampaignStatuses] = useState<Record<string, CampaignStatus>>({});
  const [selectedInvestment, setSelectedInvestment] = useState<Investment | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeModal, setActiveModal] = useState<"deposit" | "withdraw" | null>(null);
  const [transactionType, setTransactionType] = useState<"All" | "Credit" | "Debit">("All");
  const [transactionSearch, setTransactionSearch] = useState("");
  const [selectedTransaction, setSelectedTransaction] = useState<WalletTransaction | null>(null);

  async function refresh() {
    if (!user) return;
    const [walletData, investmentData, transactionData] = await Promise.all([
      getWalletBalance(user.userId),
      user.profileId ? getInvestorInvestments(user.profileId) : Promise.resolve([]),
      getWalletTransactions(user.userId),
    ]);
    setWallet(walletData);
    setInvestments(investmentData);
    setTransactions(transactionData);

    const uniqueCampaignIds = [...new Set(investmentData.map((item) => item.campaignId))];
    const statuses = await Promise.allSettled(uniqueCampaignIds.map((id) => getCampaignStatus(id)));
    const next: Record<string, CampaignStatus> = {};
    statuses.forEach((result, index) => {
      if (result.status === "fulfilled") next[uniqueCampaignIds[index]] = result.value;
    });
    setCampaignStatuses(next);
  }

  useEffect(() => {
    if (!user) return;
    setIsLoading(true);
    setError(null);
    refresh().catch(() => setError("Could not load your investor dashboard. Please try again.")).finally(() => setIsLoading(false));
  }, [user]);

  if (isLoading) return <main className={styles.page}><p className={styles.status}>Loading your dashboard...</p></main>;
  if (error) return <main className={styles.page}><p className={styles.status}>{error}</p></main>;

  const committed = investments.reduce((sum, item) => sum + item.amount, 0);
  const returned = investments.reduce((sum, item) => sum + (item.returnAmount ?? 0), 0);
  const activeCount = investments.filter((i) => !["Returned", "Cancelled"].includes(i.status)).length;

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
        <div><span>Active investments</span><strong>{activeCount}</strong></div>
        <div><span>Returned</span><strong>R {money(returned)}</strong></div>
      </section>
      <section className={styles.section}>
        <h2 className={styles.sectionTitle}>Your investments</h2>
        {investments.length === 0 ? <p className={styles.empty}>No investments yet. Browse the marketplace to get started.</p> :
          <div className={styles.list}>{investments.map((i) => {
            const campaign = campaignStatuses[i.campaignId];
            return <button type="button" key={i.investmentId} className={styles.rowButton} onClick={() => setSelectedInvestment(i)}>
              <div><strong>{i.industry}</strong><span>{new Date(i.investedAt).toLocaleDateString("en-ZA")} · {i.tenorDays} days</span></div>
              <div><strong>R {money(i.amount)}</strong><span>{statusLabel(i.status)}{campaign ? ` · Campaign ${statusLabel(campaign.status)}` : ""}</span></div>
            </button>;
          })}</div>}
      </section>
      <section className={styles.section}>
        <div className={styles.sectionHeader}>
          <div>
            <h2 className={styles.sectionTitle}>Wallet activity</h2>
            <p className={styles.sectionHint}>Review deposits, withdrawals and investment-related wallet movements.</p>
          </div>
          <button type="button" className={styles.refreshButton} onClick={() => refresh().catch(() => setError("Could not refresh wallet activity."))}>
            Refresh
          </button>
        </div>
        {transactions.length === 0 ? <p className={styles.empty}>No wallet transactions yet.</p> : (
          <>
            <div className={styles.transactionFilters}>
              <input
                aria-label="Search wallet transactions"
                placeholder="Search transactions..."
                value={transactionSearch}
                onChange={(event) => setTransactionSearch(event.target.value)}
              />
              <select value={transactionType} onChange={(event) => setTransactionType(event.target.value as "All" | "Credit" | "Debit")} aria-label="Filter transaction type">
                <option value="All">All types</option>
                <option value="Credit">Credits</option>
                <option value="Debit">Debits</option>
              </select>
            </div>
            {(() => {
              const filtered = transactions.filter((t) => {
                const matchesType = transactionType === "All" || t.type === transactionType;
                const haystack = `${t.description} ${t.referenceType} ${t.referenceId ?? ""}`.toLowerCase();
                return matchesType && haystack.includes(transactionSearch.toLowerCase().trim());
              });
              return filtered.length === 0
                ? <p className={styles.empty}>No transactions match your filters.</p>
                : <div className={styles.list}>{filtered.map(t =>
                    <button type="button" key={t.transactionId} className={styles.rowButton} onClick={() => setSelectedTransaction(t)}>
                      <div>
                        <strong>{t.description || t.referenceType}</strong>
                        <span>{new Date(t.createdAt).toLocaleString("en-ZA")} · {statusLabel(t.type)}</span>
                      </div>
                      <div>
                        <strong className={t.type === "Debit" ? styles.debit : styles.credit}>{t.type === "Debit" ? "−" : "+"} R {money(t.amount)}</strong>
                        <span>View details</span>
                      </div>
                    </button>
                  )}</div>;
            })()}
          </>
        )}
      </section>
      {selectedInvestment && <InvestmentDetails investment={selectedInvestment} campaign={campaignStatuses[selectedInvestment.campaignId]} onClose={() => setSelectedInvestment(null)} />}
      {selectedTransaction && <TransactionDetails transaction={selectedTransaction} currency={wallet?.currency ?? "ZAR"} onClose={() => setSelectedTransaction(null)} />}
      {user && wallet && <WalletActionModal isOpen={activeModal !== null} onClose={() => setActiveModal(null)} mode={activeModal ?? "deposit"} userId={user.userId} currentBalance={wallet.balance} currency={wallet.currency} onSuccess={() => refresh().catch(() => undefined)} />}
    </main>
  );
}

function InvestmentDetails({ investment, campaign, onClose }: { investment: Investment; campaign?: CampaignStatus; onClose: () => void }) {
  const remaining = campaign ? Math.max(0, campaign.targetAmount - campaign.fundedAmount) : null;
  return <Modal isOpen={true} onClose={onClose} title="Investment details">
    <div className={styles.detailGrid}>
      <div><span>Industry</span><strong>{investment.industry}</strong></div>
      <div><span>Investment amount</span><strong>R {money(investment.amount)}</strong></div>
      <div><span>Investment status</span><strong>{statusLabel(investment.status)}</strong></div>
      <div><span>Invested</span><strong>{new Date(investment.investedAt).toLocaleString("en-ZA")}</strong></div>
      <div><span>Tenor</span><strong>{investment.tenorDays} days</strong></div>
      <div><span>Return amount</span><strong>{investment.returnAmount == null ? "Pending" : `R ${money(investment.returnAmount)}`}</strong></div>
      {campaign && <>
        <div><span>Campaign status</span><strong>{statusLabel(campaign.status)}</strong></div>
        <div><span>Funding model</span><strong>{statusLabel(campaign.fundingModel)}</strong></div>
        <div><span>Campaign target</span><strong>R {money(campaign.targetAmount)}</strong></div>
        <div><span>Funded amount</span><strong>R {money(campaign.fundedAmount)}</strong></div>
        <div><span>Remaining target</span><strong>R {money(remaining ?? 0)}</strong></div>
        <div><span>Funding deadline</span><strong>{campaign.fundingDeadline ? new Date(campaign.fundingDeadline).toLocaleString("en-ZA") : "No deadline"}</strong></div>
      </>}
    </div>
  </Modal>;
}

function TransactionDetails({ transaction, currency, onClose }: { transaction: WalletTransaction; currency: string; onClose: () => void }) {
  const isDebit = transaction.type === "Debit";
  return (
    <Modal isOpen={true} onClose={onClose} title="Transaction details">
      <div className={styles.detailGrid}>
        <div><span>Type</span><strong>{statusLabel(transaction.type)}</strong></div>
        <div><span>Amount</span><strong className={isDebit ? styles.debit : styles.credit}>{isDebit ? "−" : "+"} {currency} {money(transaction.amount)}</strong></div>
        <div><span>Date</span><strong>{new Date(transaction.createdAt).toLocaleString("en-ZA")}</strong></div>
        <div><span>Transaction ID</span><strong>{transaction.transactionId}</strong></div>
        <div><span>Reference type</span><strong>{statusLabel(transaction.referenceType || "N/A")}</strong></div>
        <div><span>Reference ID</span><strong>{transaction.referenceId || "N/A"}</strong></div>
        <div style={{ gridColumn: "1 / -1" }}><span>Description</span><strong>{transaction.description || "No description provided."}</strong></div>
      </div>
    </Modal>
  );
}
