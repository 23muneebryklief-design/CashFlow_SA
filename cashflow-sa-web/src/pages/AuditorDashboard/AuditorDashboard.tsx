import { useCallback, useEffect, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import {
  approveKycDocument,
  rejectKycDocument,
  getKycDocumentDownloadUrl,
  getKycDocumentsForReview,
  type AuditorKycDocument,
  type DocumentStatus,
  type SmeKycReviewSection,
} from "../../Services/kycService";
import Modal from "../../components/Shared/Modal/Modal";
import styles from "./AuditorDashboard.module.css";

type FilterTab = DocumentStatus | "All";

const TABS: FilterTab[] = ["Pending", "Approved", "Rejected", "All"];

// Tracks the in-flight action per document so only the button that was
// clicked shows a loading state, not every row on the page.
type RowAction = "approve" | "reject" | "view" | null;

export default function AuditorDashboard() {
  const { user } = useAuth();

  const [sections, setSections] = useState<SmeKycReviewSection[]>([]);
  const [tab, setTab] = useState<FilterTab>("Pending");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [rowBusy, setRowBusy] = useState<Record<string, RowAction>>({});
  const [rejectTarget, setRejectTarget] = useState<AuditorKycDocument | null>(null);
  const [rejectNotes, setRejectNotes] = useState("");
  const [rejectError, setRejectError] = useState<string | null>(null);

  const fetchSections = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const statusFilter = tab === "All" ? undefined : tab;
      const result = await getKycDocumentsForReview(statusFilter);
      setSections(result);
    } catch {
      setError("Could not load KYC documents. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [tab]);

  useEffect(() => {
    fetchSections();
  }, [fetchSections]);

  async function handleView(document: AuditorKycDocument) {
    setRowBusy((prev) => ({ ...prev, [document.documentId]: "view" }));
    try {
      const { url } = await getKycDocumentDownloadUrl(document.documentId);
      window.open(url, "_blank", "noopener,noreferrer");
    } catch {
      setError("Could not open that document. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [document.documentId]: null }));
    }
  }

  async function handleApprove(document: AuditorKycDocument) {
    if (!user?.userId) return;

    setRowBusy((prev) => ({ ...prev, [document.documentId]: "approve" }));
    try {
      await approveKycDocument(document.documentId, user.userId);
      await fetchSections();
    } catch {
      setError("Could not approve that document. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [document.documentId]: null }));
    }
  }

  function openRejectModal(document: AuditorKycDocument) {
    setRejectTarget(document);
    setRejectNotes("");
    setRejectError(null);
  }

  async function confirmReject() {
    if (!user?.userId || !rejectTarget) return;

    if (!rejectNotes.trim()) {
      setRejectError("A reason is required so the SME knows what to fix.");
      return;
    }

    const documentId = rejectTarget.documentId;
    setRowBusy((prev) => ({ ...prev, [documentId]: "reject" }));
    try {
      await rejectKycDocument(documentId, user.userId, rejectNotes.trim());
      setRejectTarget(null);
      await fetchSections();
    } catch {
      setRejectError("Could not reject that document. Please try again.");
    } finally {
      setRowBusy((prev) => ({ ...prev, [documentId]: null }));
    }
  }

  const visibleSections = sections.filter((section) => section.documents.length > 0);

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Auditor Portal</p>
        <h1>KYC document review</h1>
        <p className={styles.subhead}>
          Documents are grouped by SME. Review each one, then approve or reject it.
        </p>
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
        <p className={styles.status}>Loading documents...</p>
      ) : visibleSections.length === 0 ? (
        <p className={styles.status}>No documents in this view.</p>
      ) : (
        <div className={styles.sections}>
          {visibleSections.map((section) => (
            <section key={section.smeId} className={styles.section}>
              <div className={styles.sectionHeader}>
                <div>
                  <h2>{section.companyName}</h2>
                  <span className={styles.contact}>{section.contactPerson}</span>
                </div>
                {section.applicationStatus && (
                  <span
                    className={`${styles.appBadge} ${styles[`app${section.applicationStatus}`]}`}
                  >
                    Application: {section.applicationStatus}
                  </span>
                )}
              </div>

              <div className={styles.docList}>
                {section.documents.map((doc) => {
                  const busy = rowBusy[doc.documentId];
                  return (
                    <div key={doc.documentId} className={styles.docRow}>
                      <div className={styles.docInfo}>
                        <span className={styles.docType}>{doc.documentType}</span>
                        <span className={styles.docFileName}>{doc.fileName}</span>
                        <span className={styles.docMeta}>
                          Uploaded {new Date(doc.uploadedAt).toLocaleDateString("en-ZA")}
                        </span>
                        {doc.reviewNotes && (
                          <span className={styles.docNotes}>Note: {doc.reviewNotes}</span>
                        )}
                      </div>

                      <div className={styles.docActions}>
                        <span className={`${styles.statusBadge} ${styles[`status${doc.status}`]}`}>
                          {doc.status}
                        </span>

                        <button
                          type="button"
                          className={styles.viewBtn}
                          disabled={busy === "view"}
                          onClick={() => handleView(doc)}
                        >
                          {busy === "view" ? "Opening..." : "View"}
                        </button>

                        {doc.status === "Pending" && (
                          <>
                            <button
                              type="button"
                              className={styles.approveBtn}
                              disabled={!!busy}
                              onClick={() => handleApprove(doc)}
                            >
                              {busy === "approve" ? "Approving..." : "Approve"}
                            </button>
                            <button
                              type="button"
                              className={styles.rejectBtn}
                              disabled={!!busy}
                              onClick={() => openRejectModal(doc)}
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
            </section>
          ))}
        </div>
      )}

      <Modal
        isOpen={!!rejectTarget}
        onClose={() => setRejectTarget(null)}
        title={`Reject ${rejectTarget?.fileName ?? "document"}`}
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
            placeholder="e.g. Document is expired, or the name doesn't match the application."
          />
          {rejectError && <p className={styles.error}>{rejectError}</p>}

          <div className={styles.modalActions}>
            <button
              type="button"
              className={styles.cancelBtn}
              onClick={() => setRejectTarget(null)}
            >
              Cancel
            </button>
            <button
              type="button"
              className={styles.rejectBtn}
              disabled={rejectTarget ? rowBusy[rejectTarget.documentId] === "reject" : false}
              onClick={confirmReject}
            >
              {rejectTarget && rowBusy[rejectTarget.documentId] === "reject"
                ? "Rejecting..."
                : "Confirm reject"}
            </button>
          </div>
        </div>
      </Modal>
    </main>
  );
}
