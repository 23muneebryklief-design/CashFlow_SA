import { useState } from "react";
import type { FormEvent } from "react";
import { getSettlement, triggerSettlement, type Settlement } from "../../Services/settlementService";
import styles from "./Settlements.module.css";

const money = new Intl.NumberFormat("en-ZA", {
  style: "currency",
  currency: "ZAR",
  maximumFractionDigits: 2,
});

function statusLabel(status: Settlement["status"]) {
  if (typeof status === "string") return status;
  return ["Pending", "Processing", "Completed", "Failed"][status] ?? String(status);
}

export default function Settlements() {
  const [campaignId, setCampaignId] = useState("");
  const [settledAmount, setSettledAmount] = useState("");
  const [paymentProvider, setPaymentProvider] = useState("");
  const [referenceNumber, setReferenceNumber] = useState("");
  const [settlementId, setSettlementId] = useState("");
  const [settlement, setSettlement] = useState<Settlement | null>(null);
  const [loading, setLoading] = useState(false);
  const [lookupLoading, setLookupLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  async function handleTrigger(event: FormEvent) {
    event.preventDefault();
    setMessage("");
    setError("");

    if (!campaignId.trim() || !settledAmount || !paymentProvider.trim() || !referenceNumber.trim()) {
      setError("Complete all settlement fields before continuing.");
      return;
    }

    const amount = Number(settledAmount);
    if (!Number.isFinite(amount) || amount <= 0) {
      setError("Settled amount must be greater than zero.");
      return;
    }

    if (!window.confirm(
      "Trigger settlement? This will mark the funded campaign as settled and credit investor wallets."
    )) return;

    setLoading(true);
    try {
      const result = await triggerSettlement(campaignId.trim(), {
        settledAmount: amount,
        paymentProvider: paymentProvider.trim(),
        referenceNumber: referenceNumber.trim(),
      });
      setSettlementId(result.settlementId);
      setMessage(`Settlement completed. Settlement ID: ${result.settlementId}`);
      setCampaignId("");
      setSettledAmount("");
      setPaymentProvider("");
      setReferenceNumber("");
      await loadSettlement(result.settlementId);
    } catch (err: any) {
      setError(err?.response?.data?.message ?? "Settlement could not be triggered. Confirm that the campaign is fully funded.");
    } finally {
      setLoading(false);
    }
  }

  async function loadSettlement(id = settlementId) {
    if (!id.trim()) {
      setError("Enter a settlement ID.");
      return;
    }
    setLookupLoading(true);
    setError("");
    try {
      setSettlement(await getSettlement(id.trim()));
    } catch (err: any) {
      setSettlement(null);
      setError(err?.response?.data?.message ?? "Settlement could not be found.");
    } finally {
      setLookupLoading(false);
    }
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Admin Portal</p>
        <h1>Settlements</h1>
        <p>Settle fully funded campaigns and inspect completed settlement records.</p>
      </header>

      {message && <div className={styles.success}>{message}</div>}
      {error && <div className={styles.error}>{error}</div>}

      <section className={styles.grid}>
        <article className={styles.card}>
          <div className={styles.cardHeader}>
            <p className={styles.eyebrow}>Trigger</p>
            <h2>Settle a funded campaign</h2>
            <p>Only campaigns with a <strong>Funded</strong> status can be settled.</p>
          </div>

          <form onSubmit={handleTrigger} className={styles.form}>
            <label>
              Campaign ID
              <input value={campaignId} onChange={(e) => setCampaignId(e.target.value)} placeholder="GUID" />
            </label>
            <label>
              Settled amount (ZAR)
              <input type="number" min="0.01" step="0.01" value={settledAmount} onChange={(e) => setSettledAmount(e.target.value)} placeholder="0.00" />
            </label>
            <label>
              Payment provider
              <input value={paymentProvider} onChange={(e) => setPaymentProvider(e.target.value)} placeholder="e.g. Bank transfer" />
            </label>
            <label>
              Reference number
              <input value={referenceNumber} onChange={(e) => setReferenceNumber(e.target.value)} placeholder="Payment reference" />
            </label>
            <button disabled={loading} type="submit">
              {loading ? "Processing…" : "Trigger settlement"}
            </button>
          </form>
        </article>

        <article className={styles.card}>
          <div className={styles.cardHeader}>
            <p className={styles.eyebrow}>Lookup</p>
            <h2>Settlement details</h2>
            <p>Use a settlement ID returned after a successful settlement.</p>
          </div>

          <div className={styles.lookup}>
            <input value={settlementId} onChange={(e) => setSettlementId(e.target.value)} placeholder="Settlement ID" />
            <button disabled={lookupLoading} onClick={() => loadSettlement()}>
              {lookupLoading ? "Loading…" : "View settlement"}
            </button>
          </div>

          {settlement && (
            <dl className={styles.details}>
              <div><dt>Status</dt><dd>{statusLabel(settlement.status)}</dd></div>
              <div><dt>Campaign</dt><dd>{settlement.campaignId}</dd></div>
              <div><dt>Settled amount</dt><dd>{money.format(settlement.settledAmount)}</dd></div>
              <div><dt>Payment provider</dt><dd>{settlement.paymentProvider}</dd></div>
              <div><dt>Reference</dt><dd>{settlement.referenceNumber}</dd></div>
              <div><dt>Settlement date</dt><dd>{new Date(settlement.settlementDate).toLocaleString("en-ZA")}</dd></div>
            </dl>
          )}
        </article>
      </section>
    </main>
  );
}
