import { useCallback, useEffect, useMemo, useState } from "react";
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
type SortOption = "company" | "newest" | "oldest" | "status";

type RowAction = "approve" | "reject" | "view" | null;

const TABS: FilterTab[] = ["Pending", "Approved", "Rejected", "All"];
const STATUS_ORDER: Record<DocumentStatus, number> = { Pending: 0, Rejected: 1, Approved: 2 };

export default function AuditorDashboard() {
  const { user } = useAuth();

  const [sections, setSections] = useState<SmeKycReviewSection[]>([]);
  const [tab, setTab] = useState<FilterTab>("Pending");
  const [search, setSearch] = useState("");
  const [documentType, setDocumentType] = useState("All");
  const [sortBy, setSortBy] = useState<SortOption>("company");
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [rowBusy, setRowBusy] = useState<Record<string, RowAction>>({});
  const [rejectTarget, setRejectTarget] = useState<AuditorKycDocument | null>(null);
  const [selectedDocument, setSelectedDocument] = useState<AuditorKycDocument | null>(null);
  const [selectedCompany, setSelectedCompany] = useState<string>("");
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

  const counts = useMemo(() => {
    const allDocuments = sections.flatMap((section) => section.documents);
    return {
      Pending: allDocuments.filter((doc) => doc.status === "Pending").length,
      Approved: allDocuments.filter((doc) => doc.status === "Approved").length,
      Rejected: allDocuments.filter((doc) => doc.status === "Rejected").length,
      All: allDocuments.length,
    };
  }, [sections]);

  const documentTypes = useMemo(() => {
    const types = new Set(sections.flatMap((section) => section.documents.map((doc) => doc.documentType)));
    return Array.from(types).sort();
  }, [sections]);

  const visibleSections = useMemo(() => {
    const query = search.trim().toLowerCase();

    return sections
      .map((section) => {
        const companyMatches =
          !query ||
          section.companyName.toLowerCase().includes(query) ||
          section.contactPerson.toLowerCase().includes(query) ||
          section.smeId.toLowerCase().includes(query);

        const documents = section.documents.filter((doc) => {
          const typeMatches = documentType === "All" || doc.documentType === documentType;
          const documentMatches =
            companyMatches ||
            doc.fileName.toLowerCase().includes(query) ||
            doc.documentType.toLowerCase().includes(query) ||
            Boolean(doc.reviewNotes?.toLowerCase().includes(query));
          return typeMatches && documentMatches;
        });

        return { ...section, documents };
      })
      .filter((section) => section.documents.length > 0)
      .sort((a, b) => {
        if (sortBy === "company") return a.companyName.localeCompare(b.companyName);
        if (sortBy === "status") {
          return STATUS_ORDER[a.documents[0].status] - STATUS_ORDER[b.documents[0].status];
        }
        const aTime = Math.max(...a.documents.map((doc) => new Date(doc.uploadedAt).getTime()));
        const bTime = Math.max(...b.documents.map((doc) => new Date(doc.uploadedAt).getTime()));
        return sortBy === "newest" ? bTime - aTime : aTime - bTime;
      });
  }, [sections, search, documentType, sortBy]);

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

  function openDetails(document: AuditorKycDocument, companyName: string) {
    setSelectedDocument(document);
    setSelectedCompany(companyName);
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Auditor Portal</p>
        <h1>KYC document review</h1>
        <p className={styles.subhead}>
          Search, filter and review SME submissions. Open a document to inspect its full review history.
        </p>
      </header>

      <div className={styles.summaryGrid}>
        {TABS.map((status) => (
          <button
            key={status}
            type="button"
            className={styles.summaryCard}
            onClick={() => setTab(status)}
          >
            <span>{status === "All" ? "Total documents" : `${status} documents`}</span>
            <strong>{counts[status]}</strong>
          </button>
        ))}
      </div>

      <div className={styles.tabs}>
        {TABS.map((t) => (
          <button
            key={t}
            type="button"
            className={t === tab ? `${styles.tab} ${styles.tabActive}` : styles.tab}
            onClick={() => setTab(t)}
          >
            {t}
            <span className={styles.tabCount}>{counts[t]}</span>
          </button>
        ))}
      </div>

      <div className={styles.filters}>
        <input
          className={styles.search}
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search company, contact, SME ID or document..."
          aria-label="Search KYC documents"
        />
        <select
          className={styles.select}
          value={documentType}
          onChange={(event) => setDocumentType(event.target.value)}
          aria-label="Filter by document type"
        >
          <option value="All">All document types</option>
          {documentTypes.map((type) => <option key={type} value={type}>{type}</option>)}
        </select>
        <select
          className={styles.select}
          value={sortBy}
          onChange={(event) => setSortBy(event.target.value as SortOption)}
          aria-label="Sort documents"
        >
          <option value="company">Company A–Z</option>
          <option value="newest">Newest upload</option>
          <option value="oldest">Oldest upload</option>
          <option value="status">Status</option>
        </select>
        <button
          type="button"
          className={styles.clearBtn}
          onClick={() => { setSearch(""); setDocumentType("All"); setSortBy("company"); }}
        >
          Clear
        </button>
      </div>

      {error && <p className={styles.error}>{error}</p>}

      {isLoading ? (
        <p className={styles.status}>Loading documents...</p>
      ) : visibleSections.length === 0 ? (
        <div className={styles.emptyState}>
          <strong>No documents found</strong>
          <span>Try changing the status, search term or document type filter.</span>
        </div>
      ) : (
        <div className={styles.sections}>
          {visibleSections.map((section) => (
            <section key={section.smeId} className={styles.section}>
              <div className={styles.sectionHeader}>
                <div>
                  <h2>{section.companyName}</h2>
                  <span className={styles.contact}>{section.contactPerson}</span>
                  <span className={styles.smeId}>SME ID: {section.smeId}</span>
                </div>
                <div className={styles.sectionMeta}>
                  <span className={styles.docCount}>{section.documents.length} document{section.documents.length === 1 ? "" : "s"}</span>
                  {section.applicationStatus && (
                    <span className={`${styles.appBadge} ${styles[`app${section.applicationStatus}`]}`}>
                      Application: {section.applicationStatus}
                    </span>
                  )}
                </div>
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
                          {doc.reviewedAt ? ` · Reviewed ${new Date(doc.reviewedAt).toLocaleDateString("en-ZA")}` : ""}
                        </span>
                        {doc.reviewNotes && <span className={styles.docNotes}>Note: {doc.reviewNotes}</span>}
                      </div>

                      <div className={styles.docActions}>
                        <span className={`${styles.statusBadge} ${styles[`status${doc.status}`]}`}>{doc.status}</span>
                        <button type="button" className={styles.viewBtn} disabled={!!busy} onClick={() => handleView(doc)}>
                          {busy === "view" ? "Opening..." : "View"}
                        </button>
                        <button type="button" className={styles.detailsBtn} disabled={!!busy} onClick={() => openDetails(doc, section.companyName)}>
                          Details
                        </button>
                        {doc.status === "Pending" && (
                          <>
                            <button type="button" className={styles.approveBtn} disabled={!!busy} onClick={() => handleApprove(doc)}>
                              {busy === "approve" ? "Approving..." : "Approve"}
                            </button>
                            <button type="button" className={styles.rejectBtn} disabled={!!busy} onClick={() => openRejectModal(doc)}>
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

      <Modal isOpen={!!selectedDocument} onClose={() => setSelectedDocument(null)} title="Document review details">
        {selectedDocument && (
          <div className={styles.detailsPanel}>
            <div className={styles.detailGrid}>
              <div><span>Company</span><strong>{selectedCompany}</strong></div>
              <div><span>Document type</span><strong>{selectedDocument.documentType}</strong></div>
              <div><span>File</span><strong>{selectedDocument.fileName}</strong></div>
              <div><span>Status</span><strong>{selectedDocument.status}</strong></div>
              <div><span>Uploaded</span><strong>{new Date(selectedDocument.uploadedAt).toLocaleString("en-ZA")}</strong></div>
              <div><span>Reviewed</span><strong>{selectedDocument.reviewedAt ? new Date(selectedDocument.reviewedAt).toLocaleString("en-ZA") : "Not reviewed yet"}</strong></div>
              <div><span>File size</span><strong>{(selectedDocument.fileSize / 1024 / 1024).toFixed(2)} MB</strong></div>
            </div>
            <div className={styles.reviewHistory}>
              <h3>Review notes</h3>
              <p>{selectedDocument.reviewNotes || "No reviewer notes have been recorded."}</p>
            </div>
            <div className={styles.modalActions}>
              <button type="button" className={styles.viewBtn} onClick={() => handleView(selectedDocument)}>Open document</button>
              <button type="button" className={styles.cancelBtn} onClick={() => setSelectedDocument(null)}>Close</button>
            </div>
          </div>
        )}
      </Modal>

      <Modal isOpen={!!rejectTarget} onClose={() => setRejectTarget(null)} title={`Reject ${rejectTarget?.fileName ?? "document"}`}>
        <div className={styles.modalBody}>
          <label className={styles.modalLabel} htmlFor="reject-notes">Reason for rejection</label>
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
            <button type="button" className={styles.cancelBtn} onClick={() => setRejectTarget(null)}>Cancel</button>
            <button
              type="button"
              className={styles.rejectBtn}
              disabled={rejectTarget ? rowBusy[rejectTarget.documentId] === "reject" : false}
              onClick={confirmReject}
            >
              {rejectTarget && rowBusy[rejectTarget.documentId] === "reject" ? "Rejecting..." : "Confirm reject"}
            </button>
          </div>
        </div>
      </Modal>
    </main>
  );
}
