import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";
import CreateAdminForm from "../../components/Admin/CreateAdminForm/CreateAdminForm";
import { getFundingVolume, getRiskDistribution, type FundingVolume, type RiskDistributionItem } from "../../Services/analyticsService";
import styles from "./AdminDashboard.module.css";

const money = new Intl.NumberFormat("en-ZA", {
  style: "currency",
  currency: "ZAR",
  maximumFractionDigits: 0,
});

export default function AdminDashboard() {
  const { user } = useAuth();
  const [funding, setFunding] = useState<FundingVolume | null>(null);
  const [risk, setRisk] = useState<RiskDistributionItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    Promise.all([getFundingVolume(), getRiskDistribution()])
      .then(([fundingData, riskData]) => {
        if (!active) return;
        setFunding(fundingData);
        setRisk(riskData);
      })
      .catch(() => {
        if (active) setError("We could not load the platform analytics. Check that the API is running and try again.");
      })
      .finally(() => active && setLoading(false));

    return () => {
      active = false;
    };
  }, []);

  const maxRiskCount = useMemo(
    () => Math.max(1, ...risk.map((item) => item.count)),
    [risk]
  );

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Admin Portal</p>
          <h1>Platform overview</h1>
          <p className={styles.intro}>A live snapshot of funding activity and portfolio risk.</p>
        </div>
        <span className={styles.role}>{user?.role}</span>
      </header>

      {error && <div className={styles.error}>{error}</div>}

      <section className={styles.stats} aria-label="Platform funding statistics">
        <Stat label="Campaigns" value={loading ? "—" : String(funding?.totalCampaigns ?? 0)} />
        <Stat label="Target volume" value={loading ? "—" : money.format(funding?.totalTargetAmount ?? 0)} />
        <Stat label="Funded volume" value={loading ? "—" : money.format(funding?.totalFundedAmount ?? 0)} />
        <Stat label="Settled volume" value={loading ? "—" : money.format(funding?.totalSettledAmount ?? 0)} />
        <Stat label="Average funded" value={loading ? "—" : `${(funding?.averageFundingPercentage ?? 0).toFixed(1)}%`} />
      </section>

      <section className={styles.grid}>
        <article className={styles.card}>
          <div className={styles.cardHeader}>
            <div>
              <p className={styles.cardEyebrow}>Risk</p>
              <h2>Risk distribution</h2>
            </div>
          </div>
          {loading ? (
            <p className={styles.muted}>Loading risk distribution…</p>
          ) : risk.length === 0 ? (
            <p className={styles.muted}>No risk assessments are available yet.</p>
          ) : (
            <div className={styles.riskList}>
              {risk.map((item) => (
                <div className={styles.riskRow} key={String(item.riskGrade)}>
                  <span className={styles.riskLabel}>Grade {String(item.riskGrade)}</span>
                  <div className={styles.barTrack}>
                    <div className={styles.bar} style={{ width: `${(item.count / maxRiskCount) * 100}%` }} />
                  </div>
                  <strong>{item.count}</strong>
                </div>
              ))}
            </div>
          )}
        </article>

        <article className={styles.card}>
          <div className={styles.cardHeader}>
            <div>
              <p className={styles.cardEyebrow}>Operations</p>
              <h2>Review queues</h2>
            </div>
          </div>
          <div className={styles.actions}>
            <Link to="/credit-review" className={styles.action}>
              <span>Credit review</span>
              <small>Funding and KYC applications</small>
            </Link>
            <Link to="/invoice-review" className={styles.action}>
              <span>Invoice review</span>
              <small>Review submitted invoices</small>
            </Link>
            <Link to="/auditor-kyc" className={styles.action}>
              <span>KYC review</span>
              <small>Document verification</small>
            </Link>
          </div>
        </article>
      </section>

      <section className={styles.card}>
        <div className={styles.cardHeader}>
          <div>
            <p className={styles.cardEyebrow}>Administration</p>
            <h2>Available admin tools</h2>
          </div>
        </div>
        <div className={styles.toolGrid}>
          <Tool title="Platform analytics" description="Funding volume and risk distribution are now live above." />
          <Tool title="Admin accounts" description="SuperAdmins can create additional admin, analyst and auditor accounts." />
          <Tool title="Audit logs" description="The backend currently restricts audit log access to the Auditor role." />
          <Tool title="Marketplace moderation" description="The backend currently exposes marketplace listings to Investors only; moderation endpoints are still required." />
        </div>
      </section>

      {user?.role === "SuperAdmin" && <CreateAdminForm />}
    </main>
  );
}

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <article className={styles.stat}>
      <span>{label}</span>
      <strong>{value}</strong>
    </article>
  );
}

function Tool({ title, description }: { title: string; description: string }) {
  return (
    <div className={styles.tool}>
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  );
}
