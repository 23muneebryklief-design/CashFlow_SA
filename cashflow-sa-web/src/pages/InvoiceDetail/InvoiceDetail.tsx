import { useCallback, useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  correctInvoiceFields,
  getInvoice,
  submitInvoice,
  type InvoiceDetails,
} from "../../Services/invoiceService";
import { getInvoiceDocumentDownloadUrl } from "../../Services/invoiceDocumentService";
import styles from "./InvoiceDetail.module.css";

// yyyy-MM-dd, what <input type="date"> expects
function toDateInputValue(iso: string): string {
  return iso.slice(0, 10);
}

const EMPTY_FORM = {
  invoiceNumber: "",
  debtorName: "",
  debtorContactDetails: "",
  amount: "",
  issueDate: "",
  dueDate: "",
};

export default function InvoiceDetail() {
  const { invoiceId } = useParams<{ invoiceId: string }>();
  const navigate = useNavigate();

  const [invoice, setInvoice] = useState<InvoiceDetails | null>(null);
  const [form, setForm] = useState(EMPTY_FORM);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saveMessage, setSaveMessage] = useState<string | null>(null);
  const [documentLoading, setDocumentLoading] = useState(false);

  const load = useCallback(async () => {
    if (!invoiceId) return;
    try {
      setError(null);
      const result = await getInvoice(invoiceId);
      setInvoice(result);
      setForm({
        invoiceNumber: result.invoiceNumber.startsWith("DRAFT-") ? "" : result.invoiceNumber,
        debtorName: result.debtorName,
        debtorContactDetails: result.debtorContactDetails,
        amount: result.amount > 0 ? String(result.amount) : "",
        issueDate: toDateInputValue(result.issueDate),
        dueDate: toDateInputValue(result.dueDate),
      });
    } catch {
      setError("Could not load this invoice.");
    } finally {
      setIsLoading(false);
    }
  }, [invoiceId]);

  useEffect(() => {
    void load();
  }, [load]);

  const isEditable = invoice?.status === "Draft" || invoice?.status === "Rejected";

  function updateField(field: keyof typeof form, value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  const isFormComplete =
    form.invoiceNumber.trim() !== "" &&
    form.debtorName.trim() !== "" &&
    form.amount !== "" &&
    Number(form.amount) > 0 &&
    form.issueDate !== "" &&
    form.dueDate !== "";

  async function handleSave(): Promise<boolean> {
    if (!invoiceId) return false;
    setError(null);
    setSaveMessage(null);
    setIsSaving(true);
    try {
      await correctInvoiceFields(invoiceId, {
        invoiceNumber: form.invoiceNumber.trim(),
        debtorName: form.debtorName.trim(),
        debtorContactDetails: form.debtorContactDetails.trim(),
        amount: Number(form.amount),
        issueDate: form.issueDate,
        dueDate: form.dueDate,
      });
      setSaveMessage("Saved.");
      return true;
    } catch {
      setError("Could not save these details. Please check the fields and try again.");
      return false;
    } finally {
      setIsSaving(false);
    }
  }

  async function handleViewDocument() {
    if (!invoiceId) return;
    setDocumentLoading(true);
    setError(null);
    try {
      const { url } = await getInvoiceDocumentDownloadUrl(invoiceId);
      window.open(url, "_blank", "noopener,noreferrer");
    } catch {
      setError("Could not open the invoice document. Your account may not have access to this document.");
    } finally {
      setDocumentLoading(false);
    }
  }

  async function handleSubmit() {
    if (!invoiceId || !isFormComplete) return;
    setError(null);

    setIsSubmitting(true);
    try {
      // Fields must be saved before the backend will accept a submission --
      // save first so a person who edited but forgot to click Save doesn't
      // submit stale data.
      const saved = await handleSave();
      if (!saved) return;

      await submitInvoice(invoiceId);
      navigate("/invoices");
    } catch {
      setError("Could not submit this invoice. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isLoading) {
    return <main className={styles.page}><p className={styles.status}>Loading invoice...</p></main>;
  }

  if (!invoice) {
    return <main className={styles.page}><p className={styles.status}>{error ?? "Invoice not found."}</p></main>;
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <button type="button" className={styles.backLink} onClick={() => navigate("/invoices")}>
          ← Back to invoices
        </button>
        <div className={styles.headerRow}>
          <h1>{isEditable ? "Complete invoice details" : "Invoice details"}</h1>
          <span className={`${styles.statusBadge} ${styles[`status${invoice.status}`]}`}>
            {invoice.status}
          </span>
        </div>
        {isEditable && (
          <p className={styles.subhead}>
            Fill in the details below, then submit for review. All fields except the debtor's
            contact details are required.
          </p>
        )}
      </header>

      {invoice.status === "Rejected" && invoice.reviewNotes && (
        <section className={styles.rejectedBanner}>
          <strong>Why this was rejected:</strong>
          <p>{invoice.reviewNotes}</p>
        </section>
      )}

      <section className={styles.documentCard}>
        <div><strong>Original invoice document</strong><p>The API issues a short-lived download URL only after authorizing your role and invoice ownership.</p></div>
        <button type="button" className={styles.saveBtn} onClick={() => void handleViewDocument()} disabled={documentLoading}>
          {documentLoading ? "Opening..." : "View original PDF"}
        </button>
      </section>

      <section className={styles.form}>
        <div className={styles.field}>
          <label htmlFor="invoiceNumber">Invoice number</label>
          <input
            id="invoiceNumber"
            type="text"
            value={form.invoiceNumber}
            onChange={(e) => updateField("invoiceNumber", e.target.value)}
            disabled={!isEditable}
            placeholder="e.g. INV-2026-0142"
          />
        </div>

        <div className={styles.field}>
          <label htmlFor="debtorName">Debtor name</label>
          <input
            id="debtorName"
            type="text"
            value={form.debtorName}
            onChange={(e) => updateField("debtorName", e.target.value)}
            disabled={!isEditable}
            placeholder="Who owes this invoice"
          />
        </div>

        <div className={styles.field}>
          <label htmlFor="debtorContactDetails">Debtor contact details</label>
          <input
            id="debtorContactDetails"
            type="text"
            value={form.debtorContactDetails}
            onChange={(e) => updateField("debtorContactDetails", e.target.value)}
            disabled={!isEditable}
            placeholder="Email or phone (optional)"
          />
        </div>

        <div className={styles.field}>
          <label htmlFor="amount">Amount (ZAR)</label>
          <input
            id="amount"
            type="number"
            min="0"
            step="0.01"
            value={form.amount}
            onChange={(e) => updateField("amount", e.target.value)}
            disabled={!isEditable}
            placeholder="0.00"
          />
        </div>

        <div className={styles.fieldRow}>
          <div className={styles.field}>
            <label htmlFor="issueDate">Issue date</label>
            <input
              id="issueDate"
              type="date"
              value={form.issueDate}
              onChange={(e) => updateField("issueDate", e.target.value)}
              disabled={!isEditable}
            />
          </div>
          <div className={styles.field}>
            <label htmlFor="dueDate">Due date</label>
            <input
              id="dueDate"
              type="date"
              value={form.dueDate}
              onChange={(e) => updateField("dueDate", e.target.value)}
              disabled={!isEditable}
            />
          </div>
        </div>

        {error && <p className={styles.error}>{error}</p>}
        {saveMessage && !error && <p className={styles.success}>{saveMessage}</p>}

        {isEditable && (
          <div className={styles.actions}>
            <button
              type="button"
              className={styles.saveBtn}
              disabled={isSaving || isSubmitting}
              onClick={() => void handleSave()}
            >
              {isSaving ? "Saving..." : "Save details"}
            </button>
            <button
              type="button"
              className={styles.submitBtn}
              disabled={!isFormComplete || isSaving || isSubmitting}
              onClick={() => void handleSubmit()}
            >
              {isSubmitting ? "Submitting..." : "Submit for review"}
            </button>
          </div>
        )}
      </section>
    </main>
  );
}
