import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import {
  getInvoicesForReview,
  approveInvoiceReview,
  rejectInvoiceReview,
  type InvoiceForReview,
} from "../../Services/invoiceReviewService";
import {
  getFundingRequestsForReview,
  approveFundingRequest,
  rejectFundingRequest,
  type FundingRequestForReview,
} from "../../Services/fundingRequestReviewService";
import {
  getPendingKycApplications,
  approveKycApplication,
  rejectKycApplication,
  type PendingKycApplication,
} from "../../Services/kycService";
import Modal from "../../components/Shared/Modal/Modal";
import styles from "./CreditAnalystDashboard.module.css";

type QueueTab = "invoices" | "funding" | "kyc";

const QUEUE_TABS: { key: QueueTab; label: string }[] = [
  { key: "invoices", label: "Invoices" },
  { key: "funding", label: "Funding Requests" },
  { key: "kyc", label: "KYC Applications" },
];

// Which row is mid-action, so only that row's button shows a loading state.
type RowAction = "approve" | "reject" | null;

// A pending reject, generalized across all three queues -- the confirm
// handler dispatches to the right service call based on `type`.
interface RejectTarget {
  type: QueueTab;
  id: string;
  label: string;
}

// Funding-request approval needs an extra field (expected return rate), so
// it gets its own modal instead of reusing the plain reject-style one.
interface ApproveFundingTarget {
  id: string;
  label: string;
  fundingModel: string;
}

function formatZAR(amount: number): string {
  return new Intl.NumberFormat("en-ZA", { style: "currency", currency: "ZAR" }).format(amount);
}

export default function CreditAnalystDashboard() {
  const { user } = useAuth();

  const [tab, setTab] = useState<QueueTab>("invoices");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [invoices, setInvoices] = useState<InvoiceForReview[]>([]);
  const [fundingRequests, setFundingRequests] = useState<FundingRequestForReview[]>([]);
  const [kycApplications, setKycApplications] = useState<PendingKycApplication[]>([]);

  const [rowBusy, setRowBusy] = useState<Record<string, RowAction>>({});

  const [rejectTarget, setRejectTarget] = useState<RejectTarget | null>(null);
  const [rejectNotes, setRejectNotes] = useState("");
  const [rejectError, setRejectError] = useState<string | null>(null);

  const [approveFundingTarget, setApproveFundingTarget] = useState<ApproveFundingTarget | null>(null);
  const [riskTarget, setRiskTarget] = useState<FundingRequestForReview | null>(null);
  const [approveRate, setApproveRate] = useState("");
  const [approveDeadline, setApproveDeadline] = useState("");
  const [approveError, setApproveError] = useState<string | null>(null);

  const fetchQueue = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      if (tab === "invoices") {
        setInvoices(await getInvoicesForReview("Submitted"));
      } else if (tab === "funding") {
        setFundingRequests(await getFundingRequestsForReview("Pending"));
      } else {
        setKycApplications(await getPendingKycApplications());
      }
    } catch {
      setError("Could not load this queue. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [tab]);

  useEffect(() => {
    fetchQueue();
  }, [fetchQueue]);

  function busyKey(type: QueueTab, id: string) {
    return `${type}:${id}`;
  }

  // --- Invoices ---------------------------------------------------------

  async function handleApproveInvoice(invoiceId: string) {
    if (!user?.userId) return;
    const key = busyKey("invoices", invoiceId);
    setRowBusy((prev) => ({ ...prev, [key]: "approve" }));
    try {
      await approveInvoiceReview(invoiceId, user.userId);
      await fetchQueue();
    } catch {
      setError("Could not approve that invoice. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [key]: null }));
    }
  }

  // --- Funding requests ---------------------------------------------------

  function openApproveFundingModal(request: FundingRequestForReview) {
    setApproveFundingTarget({
      id: request.fundingRequestId,
      label: `${request.companyName} — ${request.invoiceNumber}`,
      fundingModel: request.fundingModel,
    });
    setApproveRate("");
    setApproveDeadline("");
    setApproveError(null);
  }

  async function confirmApproveFunding() {
    if (!user?.userId || !approveFundingTarget) return;

    const needsRate = approveFundingTarget.fundingModel !== "Auction";
    const rateValue = approveRate.trim() ? Number(approveRate) : undefined;

    if (needsRate && (!rateValue || rateValue <= 0)) {
      setApproveError("An expected return rate is required for this funding model.");
      return;
    }

    const key = busyKey("funding", approveFundingTarget.id);
    setRowBusy((prev) => ({ ...prev, [key]: "approve" }));
    try {
      await approveFundingRequest(approveFundingTarget.id, {
        reviewerId: user.userId,
        expectedReturnRate: needsRate ? rateValue : undefined,
        fundingDeadline: approveDeadline ? new Date(approveDeadline).toISOString() : undefined,
      });
      setApproveFundingTarget(null);
      await fetchQueue();
    } catch {
      setApproveError("Could not approve that funding request. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [key]: null }));
    }
  }

  // --- KYC applications ----------------------------------------------------

  async function handleApproveKycApplication(applicationId: string) {
    if (!user?.userId) return;
    const key = busyKey("kyc", applicationId);
    setRowBusy((prev) => ({ ...prev, [key]: "approve" }));
    try {
      await approveKycApplication(applicationId, user.userId);
      await fetchQueue();
    } catch {
      setError("Could not approve that application. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [key]: null }));
    }
  }

  // --- Shared reject flow ----------------------------------------------

  function openRejectModal(target: RejectTarget) {
    setRejectTarget(target);
    setRejectNotes("");
    setRejectError(null);
  }

  async function confirmReject() {
    if (!user?.userId || !rejectTarget) return;

    if (!rejectNotes.trim()) {
      setRejectError("A reason is required so the SME knows what to fix.");
      return;
    }

    const key = busyKey(rejectTarget.type, rejectTarget.id);
    setRowBusy((prev) => ({ ...prev, [key]: "reject" }));
    try {
      if (rejectTarget.type === "invoices") {
        await rejectInvoiceReview(rejectTarget.id, user.userId, rejectNotes.trim());
      } else if (rejectTarget.type === "funding") {
        await rejectFundingRequest(rejectTarget.id, user.userId, rejectNotes.trim());
      } else {
        await rejectKycApplication(rejectTarget.id, user.userId, rejectNotes.trim());
      }
      setRejectTarget(null);
      await fetchQueue();
    } catch {
      setRejectError("Could not reject that item. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [key]: null }));
    }
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Credit Analyst Portal</p>
        <h1>Review queues</h1>
        <p className={styles.subhead}>
          Approve or reject submitted invoices, funding requests, and KYC applications.
        </p>
      </header>

      <div className={styles.tabs}>
        {QUEUE_TABS.map((t) => (
          <button
            key={t.key}
            type="button"
            className={t.key === tab ? `${styles.tab} ${styles.tabActive}` : styles.tab}
            onClick={() => setTab(t.key)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {isLoading ? (
        <p className={styles.status}>Loading...</p>
      ) : (
        <div className={styles.sections}>
          <section className={styles.section}>
            {tab === "invoices" && (
              invoices.length === 0 ? (
                <p className={styles.status}>No invoices waiting for review.</p>
              ) : (
                <div className={styles.docList}>
                  {invoices.map((inv) => {
                    const key = busyKey("invoices", inv.invoiceId);
                    const busy = rowBusy[key];
                    return (
                      <div key={inv.invoiceId} className={styles.row}>
                        <div className={styles.rowInfo}>
                          <span className={styles.rowTitle}>
                            {inv.companyName} — {inv.invoiceNumber}
                          </span>
                          <span className={styles.rowSub}>Debtor: {inv.debtorName}</span>
                          <span className={styles.rowMeta}>
                            Due {new Date(inv.dueDate).toLocaleDateString("en-ZA")}
                          </span>
                        </div>
                        <div className={styles.rowActions}>
                          <span className={styles.amount}>{formatZAR(inv.amount)}</span>
                          <button
                            type="button"
                            className={styles.approveBtn}
                            disabled={!!busy}
                            onClick={() => handleApproveInvoice(inv.invoiceId)}
                          >
                            {busy === "approve" ? "Approving..." : "Approve"}
                          </button>
                          <button
                            type="button"
                            className={styles.rejectBtn}
                            disabled={!!busy}
                            onClick={() =>
                              openRejectModal({
                                type: "invoices",
                                id: inv.invoiceId,
                                label: `${inv.companyName} — ${inv.invoiceNumber}`,
                              })
                            }
                          >
                            Reject
                          </button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )
            )}

            {tab === "funding" && (
              fundingRequests.length === 0 ? (
                <p className={styles.status}>No funding requests waiting for review.</p>
              ) : (
                <div className={styles.docList}>
                  {fundingRequests.map((fr) => {
                    const key = busyKey("funding", fr.fundingRequestId);
                    const busy = rowBusy[key];
                    return (
                      <div key={fr.fundingRequestId} className={styles.row}>
                        <div className={styles.rowInfo}>
                          <span className={styles.rowTitle}>
                            {fr.companyName} — {fr.invoiceNumber}
                          </span>
                          <span className={styles.rowSub}>
                            {fr.fundingModel} · Invoice {formatZAR(fr.invoiceAmount)}
                          </span>
                          <span className={styles.rowMeta}>
                            Due {new Date(fr.dueDate).toLocaleDateString("en-ZA")}
                          </span>
                        </div>
                        <div className={styles.rowActions}>
                          {fr.riskGrade && (
                            <button
                              type="button"
                              className={styles.riskBadgeButton}
                              onClick={() => setRiskTarget(fr)}
                              disabled={!!busy}
                              title="View risk assessment"
                            >
                              {fr.riskGrade} · {fr.riskScore}
                            </button>
                          )}
                          <span className={styles.amount}>{formatZAR(fr.requestedAmount)}</span>
                          <button
                            type="button"
                            className={styles.approveBtn}
                            disabled={!!busy}
                            onClick={() => openApproveFundingModal(fr)}
                          >
                            {busy === "approve" ? "Approving..." : "Approve"}
                          </button>
                          <button
                            type="button"
                            className={styles.rejectBtn}
                            disabled={!!busy}
                            onClick={() =>
                              openRejectModal({
                                type: "funding",
                                id: fr.fundingRequestId,
                                label: `${fr.companyName} — ${fr.invoiceNumber}`,
                              })
                            }
                          >
                            Reject
                          </button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )
            )}

            {tab === "kyc" && (
              kycApplications.length === 0 ? (
                <p className={styles.status}>No KYC applications waiting for review.</p>
              ) : (
                <div className={styles.docList}>
                  {kycApplications.map((app) => {
                    const key = busyKey("kyc", app.applicationId);
                    const busy = rowBusy[key];
                    return (
                      <div key={app.applicationId} className={styles.row}>
                        <div className={styles.rowInfo}>
                          <span className={styles.rowTitle}>{app.companyName}</span>
                          <span className={styles.rowMeta}>
                            Submitted {new Date(app.applicationDate).toLocaleDateString("en-ZA")}
                          </span>
                        </div>
                        <div className={styles.rowActions}>
                          <button
                            type="button"
                            className={styles.approveBtn}
                            disabled={!!busy}
                            onClick={() => handleApproveKycApplication(app.applicationId)}
                          >
                            {busy === "approve" ? "Approving..." : "Approve"}
                          </button>
                          <button
                            type="button"
                            className={styles.rejectBtn}
                            disabled={!!busy}
                            onClick={() =>
                              openRejectModal({
                                type: "kyc",
                                id: app.applicationId,
                                label: app.companyName,
                              })
                            }
                          >
                            Reject
                          </button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )
            )}
          </section>
        </div>
      )}

      {/* Reject modal, shared across all three queues */}
      <Modal
        isOpen={!!rejectTarget}
        onClose={() => setRejectTarget(null)}
        title={`Reject ${rejectTarget?.label ?? ""}`}
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
            placeholder="Explain what needs to change before this can be resubmitted."
          />
          {rejectError && <p className={styles.error}>{rejectError}</p>}

          <div className={styles.modalActions}>
            <button type="button" className={styles.cancelBtn} onClick={() => setRejectTarget(null)}>
              Cancel
            </button>
            <button
              type="button"
              className={styles.rejectBtn}
              disabled={rejectTarget ? rowBusy[busyKey(rejectTarget.type, rejectTarget.id)] === "reject" : false}
              onClick={confirmReject}
            >
              {rejectTarget && rowBusy[busyKey(rejectTarget.type, rejectTarget.id)] === "reject"
                ? "Rejecting..."
                : "Confirm reject"}
            </button>
          </div>
        </div>
      </Modal>

      {/* Risk assessment modal. The funding-review API currently exposes the
          persisted score/grade, but not scoring factors or the AI explanation. */}
      <Modal
        isOpen={!!riskTarget}
        onClose={() => setRiskTarget(null)}
        title={`Risk assessment — ${riskTarget?.companyName ?? ""}`}
      >
        <div className={styles.riskPanel}>
          <div className={styles.riskMetric}>
            <span className={styles.riskMetricLabel}>Risk grade</span>
            <strong>{riskTarget?.riskGrade ?? "Not assessed"}</strong>
          </div>
          <div className={styles.riskMetric}>
            <span className={styles.riskMetricLabel}>Risk score</span>
            <strong>{riskTarget?.riskScore ?? "Not available"}</strong>
          </div>
          <div className={styles.riskContext}>
            <p><strong>Invoice:</strong> {riskTarget?.invoiceNumber}</p>
            <p><strong>Invoice amount:</strong> {riskTarget ? formatZAR(riskTarget.invoiceAmount) : "—"}</p>
            <p><strong>Requested funding:</strong> {riskTarget ? formatZAR(riskTarget.requestedAmount) : "—"}</p>
            <p><strong>Funding model:</strong> {riskTarget?.fundingModel}</p>
          </div>
          <p className={styles.riskNotice}>
            The current funding-review API provides the persisted risk score and grade.
            Scoring factors and the AI-generated explanation are not yet exposed by that API,
            so they are not invented or displayed as if they were available.
          </p>
          <div className={styles.modalActions}>
            <button type="button" className={styles.cancelBtn} onClick={() => setRiskTarget(null)}>
              Close
            </button>
          </div>
        </div>
      </Modal>

      {/* Approve modal for funding requests -- needs the return rate a
          plain approve button can't collect. */}
      <Modal
        isOpen={!!approveFundingTarget}
        onClose={() => setApproveFundingTarget(null)}
        title={`Approve ${approveFundingTarget?.label ?? ""}`}
      >
        <div className={styles.modalBody}>
          {approveFundingTarget?.fundingModel !== "Auction" && (
            <div className={styles.modalField}>
              <label className={styles.modalLabel} htmlFor="approve-rate">
                Expected return rate (%)
              </label>
              <input
                id="approve-rate"
                type="number"
                min="0"
                step="0.1"
                className={styles.modalInput}
                value={approveRate}
                onChange={(e) => setApproveRate(e.target.value)}
                placeholder="e.g. 12.5"
              />
            </div>
          )}

          {approveFundingTarget?.fundingModel === "Auction" && (
            <p className={styles.hint}>
              Auction campaigns derive their rate from investor bids, so no rate is needed here.
            </p>
          )}

          <div className={styles.modalField}>
            <label className={styles.modalLabel} htmlFor="approve-deadline">
              Funding deadline (optional, defaults to 14 days)
            </label>
            <input
              id="approve-deadline"
              type="date"
              className={styles.modalInput}
              value={approveDeadline}
              onChange={(e) => setApproveDeadline(e.target.value)}
            />
          </div>

          {approveError && <p className={styles.error}>{approveError}</p>}

          <div className={styles.modalActions}>
            <button type="button" className={styles.cancelBtn} onClick={() => setApproveFundingTarget(null)}>
              Cancel
            </button>
            <button
              type="button"
              className={styles.approveBtn}
              disabled={
                approveFundingTarget
                  ? rowBusy[busyKey("funding", approveFundingTarget.id)] === "approve"
                  : false
              }
              onClick={confirmApproveFunding}
            >
              {approveFundingTarget && rowBusy[busyKey("funding", approveFundingTarget.id)] === "approve"
                ? "Approving..."
                : "Confirm approve"}
            </button>
          </div>
        </div>
      </Modal>
    </main>
  );
}
