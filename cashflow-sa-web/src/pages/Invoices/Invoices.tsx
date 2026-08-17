import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../Hooks/useAuth";
import { useKycStatus } from "../../Hooks/useKycStatus";
import { getInvoicesBySme, uploadInvoice, validateInvoiceFile, type InvoiceSummary } from "../../Services/invoiceService";
import styles from "./Invoices.module.css";

function statusLabel(status: InvoiceSummary["status"]) {
  if (status === "UnderReview") return "Under review";
  return status;
}

export default function Invoices() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { status: kycStatus, isLoading: isKycLoading } = useKycStatus();
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [invoices, setInvoices] = useState<InvoiceSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadInvoices = useCallback(async () => {
    if (!user?.profileId) return;
    try {
      setError(null);
      const result = await getInvoicesBySme(user.profileId);
      setInvoices(result);
    } catch {
      setError("Could not load your invoices. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [user]);

  useEffect(() => {
    if (user?.profileId && kycStatus === "Verified") {
      // Fetching server state on mount is intentional here.
      // eslint-disable-next-line react-hooks/set-state-in-effect
      void loadInvoices();
    } else if (!isKycLoading) {
      setIsLoading(false);
    }
  }, [user?.profileId, kycStatus, isKycLoading, loadInvoices]);

  async function handleFile(file: File) {
    setError(null);
    setSuccess(null);

    const validationError = validateInvoiceFile(file);
    if (validationError) {
      setError(validationError);
      return;
    }

    setIsUploading(true);
    try {
      await uploadInvoice(file);
      setSuccess("Invoice uploaded successfully. It is now saved as a draft while its details are completed.");
      await loadInvoices();
    } catch (requestError: unknown) {
      const responseData = (requestError as { response?: { data?: unknown } })?.response?.data;
      setError(
        typeof responseData === "string"
          ? responseData
          : "The invoice could not be uploaded. Please try again."
      );
    } finally {
      setIsUploading(false);
      if (inputRef.current) inputRef.current.value = "";
    }
  }

  if (isKycLoading || isLoading) {
    return <main className={styles.page}><p className={styles.status}>Loading your invoices…</p></main>;
  }

  if (kycStatus !== "Verified") {
    return (
      <main className={styles.page}>
        <header className={styles.header}>
          <p className={styles.eyebrow}>Business financing</p>
          <h1>Invoices</h1>
          <p className={styles.subhead}>Upload unpaid invoices and prepare them for funding.</p>
        </header>
        <section className={styles.lockedCard}>
          <span className={styles.lockIcon}>🔒</span>
          <div>
            <h2>FICA verification required</h2>
            <p>Your business must have verified FICA before you can upload an invoice.</p>
          </div>
        </section>
      </main>
    );
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <p className={styles.eyebrow}>Business financing</p>
          <h1>Your invoices</h1>
          <p className={styles.subhead}>Upload an unpaid invoice to start a funding request.</p>
        </div>
      </header>

      <section className={styles.uploadCard}>
        <div>
          <p className={styles.cardEyebrow}>Upload invoice</p>
          <h2>Start with the original PDF</h2>
          <p className={styles.muted}>PDF only · maximum 10 MB. Your invoice is saved securely and starts in Draft status.</p>
        </div>
        <input
          ref={inputRef}
          className={styles.fileInput}
          type="file"
          accept="application/pdf,.pdf"
          onChange={(event) => {
            const file = event.target.files?.[0];
            if (file) void handleFile(file);
          }}
          disabled={isUploading}
        />
        <button
          type="button"
          className={styles.primaryButton}
          onClick={() => inputRef.current?.click()}
          disabled={isUploading}
        >
          {isUploading ? "Uploading…" : "Choose invoice PDF"}
        </button>
        {error && <p className={styles.error}>{error}</p>}
        {success && <p className={styles.success}>{success}</p>}
      </section>

      <section className={styles.listSection}>
        <div className={styles.sectionHeader}>
          <div>
            <p className={styles.cardEyebrow}>Invoice pipeline</p>
            <h2>Recent uploads</h2>
          </div>
          <span className={styles.count}>{invoices.length}</span>
        </div>

        {!invoices.length ? (
          <div className={styles.empty}>
            <h3>No invoices yet</h3>
            <p>Upload your first unpaid invoice above to begin.</p>
          </div>
        ) : (
          <div className={styles.invoiceList}>
            {invoices.map((invoice) => {
              const needsAction = invoice.status === "Draft" || invoice.status === "Rejected";
              return (
                <article
                  className={styles.invoiceRow}
                  key={invoice.invoiceId}
                  onClick={() => navigate(`/invoices/${invoice.invoiceId}`)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") navigate(`/invoices/${invoice.invoiceId}`);
                  }}
                  role="button"
                  tabIndex={0}
                >
                  <div className={styles.invoiceMain}>
                    <strong>{invoice.invoiceNumber.startsWith("DRAFT-") ? "New invoice" : invoice.invoiceNumber}</strong>
                    <span>Due {new Date(invoice.dueDate).toLocaleDateString("en-ZA")}</span>
                  </div>
                  <div className={styles.invoiceAmount}>
                    {invoice.amount > 0 ? `R ${invoice.amount.toLocaleString("en-ZA", { minimumFractionDigits: 2 })}` : "Details pending"}
                  </div>
                  <span className={styles.statusPill}>
                    {statusLabel(invoice.status)}
                    {needsAction && <span className={styles.actionDot} aria-hidden="true" />}
                  </span>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
