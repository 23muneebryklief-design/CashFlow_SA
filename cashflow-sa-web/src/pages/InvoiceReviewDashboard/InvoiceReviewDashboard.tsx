import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import {
  approveInvoiceReview,
  getInvoicesForReview,
  rejectInvoiceReview,
  type InvoiceForReview,
  type InvoiceReviewStatus,
} from "../../Services/invoiceService";
import Modal from "../../components/Shared/Modal/Modal";
import styles from "./InvoiceReviewDashboard.module.css";

const TABS: InvoiceReviewStatus[] = ["Submitted", "Approved", "Rejected", "Draft"];

type RowAction = "approve" | "reject" | null;

export default function InvoiceReviewDashboard() {
  const { user } = useAuth();

  const [invoices, setInvoices] = useState<InvoiceForReview[]>([]);
  const [tab, setTab] = useState<InvoiceReviewStatus>("Submitted");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [rowBusy, setRowBusy] = useState<Record<string, RowAction>>({});
  const [rejectTarget, setRejectTarget] = useState<InvoiceForReview | null>(null);
  const [rejectNotes, setRejectNotes] = useState("");
  const [rejectError, setRejectError] = useState<string | null>(null);

  const fetchInvoices = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await getInvoicesForReview(tab);
      setInvoices(result);
    } catch {
      setError("Could not load invoices. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [tab]);

  useEffect(() => {
    fetchInvoices();
  }, [fetchInvoices]);

  async function handleApprove(invoice: InvoiceForReview) {
    if (!user?.userId) return;

    setRowBusy((prev) => ({ ...prev, [invoice.invoiceId]: "approve" }));
    try {
      await approveInvoiceReview(invoice.invoiceId, user.userId);
      await fetchInvoices();
    } catch {
      setError("Could not approve that invoice. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [invoice.invoiceId]: null }));
    }
  }

  function openRejectModal(invoice: InvoiceForReview) {
    setRejectTarget(invoice);
    setRejectNotes("");
    setRejectError(null);
  }

  async function confirmReject() {
    if (!user?.userId || !rejectTarget) return;

    if (!rejectNotes.trim()) {
      setRejectError("A reason is required so the SME knows what to fix.");
      return;
    }

    const invoiceId = rejectTarget.invoiceId;
    setRowBusy((prev) => ({ ...prev, [invoiceId]: "reject" }));
    try {
      await rejectInvoiceReview(invoiceId, user.userId, rejectNotes.trim());
      setRejectTarget(null);
      await fetchInvoices();
    } catch {
      setRejectError("Could not reject that invoice. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [invoiceId]: null }));
    }
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Ops Portal</p>
        <h1>Invoice review</h1>
        <p className={styles.subhead}>Review submitted invoices, then approve or reject.</p>
      </header>

      <div className={styles.tabs}>
        {TABS.map((t) => (
          <button
            key={t}
            type="button"
            className={t === tab ? `${styles.tab} ${styles.tabActive}` : styles.tab}
            onClick={() => setTab(t)}
          >
            {t}
          </button>
        ))}
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {isLoading ? (
        <p className={styles.status}>Loading invoices...</p>
      ) : invoices.length === 0 ? (
        <p className={styles.status}>No invoices in this view.</p>
      ) : (
        <div className={styles.list}>
          {invoices.map((invoice) => {
            const busy = rowBusy[invoice.invoiceId];
            return (
              <div key={invoice.invoiceId} className={styles.row}>
                <div className={styles.info}>
                  <span className={styles.company}>{invoice.companyName}</span>
                  <span className={styles.invoiceNumber}>{invoice.invoiceNumber}</span>
                  <span className={styles.meta}>
                    Debtor: {invoice.debtorName} &middot; Due{" "}
                    {new Date(invoice.dueDate).toLocaleDateString("en-ZA")}
                  </span>
                  {invoice.reviewNotes && (
                    <span className={styles.notes}>Note: {invoice.reviewNotes}</span>
                  )}
                </div>

                <div className={styles.amount}>
                  R {invoice.amount.toLocaleString("en-ZA", { minimumFractionDigits: 2 })}
                </div>

                <div className={styles.actions}>
                  <span className={`${styles.statusBadge} ${styles[`status${invoice.status}`]}`}>
                    {invoice.status}
                  </span>

                  {invoice.status === "Submitted" && (
                    <>
                      <button
                        type="button"
                        className={styles.approveBtn}
                        disabled={!!busy}
                        onClick={() => handleApprove(invoice)}
                      >
                        {busy === "approve" ? "Approving..." : "Approve"}
                      </button>
                      <button
                        type="button"
                        className={styles.rejectBtn}
                        disabled={!!busy}
                        onClick={() => openRejectModal(invoice)}
                      >
                        Reject
                      </button>
                    </>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      <Modal
        isOpen={!!rejectTarget}
        onClose={() => setRejectTarget(null)}
        title={`Reject ${rejectTarget?.invoiceNumber ?? "invoice"}`}
      >
        <div className={styles.modalBody}>
          <label className={styles.modalLabel} htmlFor="reject-notes">
            Reason for rejection
          </label>
          <textarea
            id="reject-notes"
            className={styles.modalTextarea}
            rows={4}
            value={rejectNotes}
            onChange={(e) => setRejectNotes(e.target.value)}
            placeholder="e.g. Debtor details don't match the uploaded document."
          />
          {rejectError && <p className={styles.error}>{rejectError}</p>}

          <div className={styles.modalActions}>
            <button type="button" className={styles.cancelBtn} onClick={() => setRejectTarget(null)}>
              Cancel
            </button>
            <button
              type="button"
              className={styles.rejectBtn}
              disabled={rejectTarget ? rowBusy[rejectTarget.invoiceId] === "reject" : false}
              onClick={confirmReject}
            >
              {rejectTarget && rowBusy[rejectTarget.invoiceId] === "reject"
                ? "Rejecting..."
                : "Confirm reject"}
            </button>
          </div>
        </div>
      </Modal>
    </main>
  );
}
