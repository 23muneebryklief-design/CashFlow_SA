import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAuth } from "../../Hooks/useAuth";
import { useKycStatus } from "../../Hooks/useKycStatus";
import {
  correctInvoiceFields,
  getInvoice,
  getInvoicesBySme,
  submitInvoice,
  uploadInvoice,
  validateInvoiceFile,
  type InvoiceDetails,
  type InvoiceSummary,
} from "../../Services/invoiceService";
import { createFundingRequest, type FundingModel } from "../../Services/fundingService";
import styles from "./Invoices.module.css";

const fundingOptions: Array<{ value: FundingModel; label: string; description: string }> = [
  { value: "SingleInvestor", label: "Single investor", description: "One investor funds the full requested amount." },
  { value: "Fractional", label: "Fractional", description: "Multiple investors can fund portions of the request." },
  { value: "Auction", label: "Auction", description: "Investors compete through bids when the opportunity is listed." },
];

function statusLabel(status: InvoiceSummary["status"]) {
  if (status === "UnderReview") return "Under review";
  return status;
}

function toDateInput(value: string) {
  return new Date(value).toISOString().slice(0, 10);
}

export default function Invoices() {
  const { user } = useAuth();
  const { status: kycStatus, isLoading: isKycLoading } = useKycStatus();
  const inputRef = useRef<HTMLInputElement | null>(null);
  const [invoices, setInvoices] = useState<InvoiceSummary[]>([]);
  const [selected, setSelected] = useState<InvoiceDetails | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isBusy, setIsBusy] = useState(false);
  const [isFunding, setIsFunding] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [fundingModel, setFundingModel] = useState<FundingModel>("SingleInvestor");
  const [requestedAmount, setRequestedAmount] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<"All" | InvoiceSummary["status"]>("All");
  const [sortBy, setSortBy] = useState<"newest" | "oldest" | "amountHigh" | "amountLow" | "dueSoon">("newest");
  const [form, setForm] = useState({ invoiceNumber: "", debtorName: "", debtorContactDetails: "", amount: "", issueDate: "", dueDate: "" });

  const loadInvoices = useCallback(async () => {
    if (!user?.profileId) return;
    try {
      setError(null);
      setInvoices(await getInvoicesBySme(user.profileId));
    } catch {
      setError("Could not load your invoices. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }, [user?.profileId]);

  useEffect(() => {
    if (user?.profileId && kycStatus === "Verified") void loadInvoices();
    else if (!isKycLoading) setIsLoading(false);
  }, [user?.profileId, kycStatus, isKycLoading, loadInvoices]);

  const visibleInvoices = useMemo(() => {
    const query = search.trim().toLowerCase();
    const filtered = invoices.filter((invoice) => {
      const matchesStatus = statusFilter === "All" || invoice.status === statusFilter;
      const matchesSearch = !query || invoice.invoiceNumber.toLowerCase().includes(query);
      return matchesStatus && matchesSearch;
    });
    return [...filtered].sort((a, b) => {
      if (sortBy === "amountHigh") return b.amount - a.amount;
      if (sortBy === "amountLow") return a.amount - b.amount;
      if (sortBy === "oldest") return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      if (sortBy === "dueSoon") return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
      return new Date(b.dueDate).getTime() - new Date(a.dueDate).getTime();
    });
  }, [invoices, search, statusFilter, sortBy]);

  const invoiceStats = useMemo(() => ({
    total: invoices.length,
    draft: invoices.filter((i) => i.status === "Draft").length,
    review: invoices.filter((i) => i.status === "Submitted" || i.status === "UnderReview").length,
    approved: invoices.filter((i) => i.status === "Approved" || i.status === "Listed").length,
    rejected: invoices.filter((i) => i.status === "Rejected").length,
  }), [invoices]);

  async function openInvoice(invoiceId: string) {
    setError(null);
    setSuccess(null);
    try {
      const details = await getInvoice(invoiceId);
      setSelected(details);
      setForm({
        invoiceNumber: details.invoiceNumber.startsWith("DRAFT-") ? "" : details.invoiceNumber,
        debtorName: details.debtorName,
        debtorContactDetails: details.debtorContactDetails,
        amount: details.amount ? String(details.amount) : "",
        issueDate: toDateInput(details.issueDate),
        dueDate: toDateInput(details.dueDate),
      });
      setRequestedAmount(details.amount ? String(details.amount) : "");
    } catch {
      setError("Could not open this invoice.");
    }
  }

  async function handleFile(file: File) {
    setError(null); setSuccess(null);
    const validationError = validateInvoiceFile(file);
    if (validationError) { setError(validationError); return; }
    setIsBusy(true);
    try {
      const result = await uploadInvoice(file);
      setSuccess("Invoice uploaded. Complete the extracted details, then submit it for review.");
      await loadInvoices();
      await openInvoice(result.invoiceId);
    } catch (requestError: unknown) {
      const responseData = (requestError as { response?: { data?: unknown } })?.response?.data;
      setError(typeof responseData === "string" ? responseData : "The invoice could not be uploaded. Please try again.");
    } finally {
      setIsBusy(false);
      if (inputRef.current) inputRef.current.value = "";
    }
  }

  async function handleSave() {
    if (!selected) return;
    if (!form.invoiceNumber.trim() || !form.debtorName.trim() || !form.amount || Number(form.amount) <= 0 || !form.issueDate || !form.dueDate) {
      setError("Complete the invoice number, debtor, amount, issue date and due date before saving.");
      return;
    }
    if (new Date(form.dueDate) < new Date(form.issueDate)) {
      setError("The due date cannot be earlier than the issue date.");
      return;
    }
    setIsBusy(true); setError(null); setSuccess(null);
    try {
      await correctInvoiceFields(selected.invoiceId, {
        invoiceNumber: form.invoiceNumber.trim(),
        debtorName: form.debtorName.trim(),
        debtorContactDetails: form.debtorContactDetails.trim(),
        amount: Number(form.amount),
        issueDate: form.issueDate,
        dueDate: form.dueDate,
      });
      setSuccess("Invoice details saved.");
      await loadInvoices();
      await openInvoice(selected.invoiceId);
    } catch (requestError: unknown) {
      const responseData = (requestError as { response?: { data?: unknown } })?.response?.data;
      setError(typeof responseData === "string" ? responseData : "The invoice could not be updated.");
    } finally { setIsBusy(false); }
  }

  async function handleSubmit() {
    if (!selected) return;
    if (!form.invoiceNumber.trim() || !form.debtorName.trim() || !form.amount || Number(form.amount) <= 0 || !form.issueDate || !form.dueDate) {
      setError("Complete all required invoice fields before submitting.");
      return;
    }
    if (new Date(form.dueDate) < new Date(form.issueDate)) {
      setError("The due date cannot be earlier than the issue date.");
      return;
    }
    setIsBusy(true); setError(null); setSuccess(null);
    try {
      await submitInvoice(selected.invoiceId);
      setSuccess("Invoice submitted for review.");
      await loadInvoices();
      await openInvoice(selected.invoiceId);
    } catch (requestError: unknown) {
      const responseData = (requestError as { response?: { data?: unknown } })?.response?.data;
      setError(typeof responseData === "string" ? responseData : "The invoice could not be submitted.");
    } finally { setIsBusy(false); }
  }

  async function handleFundingRequest() {
    if (!selected) return;
    const amount = Number(requestedAmount);
    if (!Number.isFinite(amount) || amount <= 0 || amount > selected.amount) {
      setError("Enter a funding amount greater than zero and no more than the approved invoice amount.");
      return;
    }
    setIsFunding(true); setError(null); setSuccess(null);
    try {
      await createFundingRequest({ invoiceId: selected.invoiceId, requestedAmount: amount, fundingModel });
      setSuccess("Funding request submitted. Your request is now pending credit review.");
      await loadInvoices();
      await openInvoice(selected.invoiceId);
    } catch (requestError: unknown) {
      const responseData = (requestError as { response?: { data?: unknown } })?.response?.data;
      setError(typeof responseData === "string" ? responseData : "The funding request could not be submitted.");
    } finally { setIsFunding(false); }
  }

  if (isKycLoading || isLoading) return <main className={styles.page}><p className={styles.status}>Loading your invoices…</p></main>;

  if (kycStatus !== "Verified") {
    return <main className={styles.page}><header className={styles.header}><p className={styles.eyebrow}>Business financing</p><h1>Invoices</h1><p className={styles.subhead}>Upload unpaid invoices and prepare them for funding.</p></header><section className={styles.lockedCard}><span className={styles.lockIcon}>🔒</span><div><h2>FICA verification required</h2><p>Your business must have verified FICA before you can upload an invoice.</p></div></section></main>;
  }

  return (
    <main className={styles.page}>
      <header className={styles.header}><div><p className={styles.eyebrow}>Business financing</p><h1>Your invoices</h1><p className={styles.subhead}>Upload, complete and submit invoices. Approved invoices can then be sent into the funding workflow.</p></div></header>

      {error && <p className={styles.error}>{error}</p>}
      {success && <p className={styles.success}>{success}</p>}

      <section className={styles.uploadCard}>
        <div><p className={styles.cardEyebrow}>Upload invoice</p><h2>Start with the original PDF</h2><p className={styles.muted}>PDF only · maximum 10 MB. New invoices begin in Draft status.</p></div>
        <input ref={inputRef} className={styles.fileInput} type="file" accept="application/pdf,.pdf" onChange={(event) => { const file = event.target.files?.[0]; if (file) void handleFile(file); }} disabled={isBusy} />
        <button type="button" className={styles.primaryButton} onClick={() => inputRef.current?.click()} disabled={isBusy}>{isBusy ? "Working…" : "Choose invoice PDF"}</button>
      </section>

      <section className={styles.pipelineSummary}>
        <div className={styles.summaryCard}><span>Total</span><strong>{invoiceStats.total}</strong></div>
        <div className={styles.summaryCard}><span>Draft</span><strong>{invoiceStats.draft}</strong></div>
        <div className={styles.summaryCard}><span>In review</span><strong>{invoiceStats.review}</strong></div>
        <div className={styles.summaryCard}><span>Approved / listed</span><strong>{invoiceStats.approved}</strong></div>
        <div className={styles.summaryCard}><span>Rejected</span><strong>{invoiceStats.rejected}</strong></div>
      </section>

      <section className={styles.listSection}>
        <div className={styles.sectionHeader}><div><p className={styles.cardEyebrow}>Invoice pipeline</p><h2>Recent uploads</h2></div><span className={styles.count}>{visibleInvoices.length}</span></div>
        <div className={styles.filters}>
          <input aria-label="Search invoices" placeholder="Search invoice number…" value={search} onChange={(e) => setSearch(e.target.value)} />
          <select aria-label="Filter invoices by status" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as typeof statusFilter)}>
            <option value="All">All statuses</option><option value="Draft">Draft</option><option value="Submitted">Submitted</option><option value="UnderReview">Under review</option><option value="Approved">Approved</option><option value="Rejected">Rejected</option><option value="Listed">Listed</option>
          </select>
          <select aria-label="Sort invoices" value={sortBy} onChange={(e) => setSortBy(e.target.value as typeof sortBy)}>
            <option value="newest">Newest</option><option value="oldest">Oldest</option><option value="dueSoon">Due soon</option><option value="amountHigh">Highest amount</option><option value="amountLow">Lowest amount</option>
          </select>
        </div>
        {!invoices.length ? <div className={styles.empty}><h3>No invoices yet</h3><p>Upload your first unpaid invoice above to begin.</p></div> : !visibleInvoices.length ? <div className={styles.empty}><h3>No matching invoices</h3><p>Try changing the search or status filter.</p></div> : <div className={styles.invoiceList}>
          {visibleInvoices.map((invoice) => <article className={`${styles.invoiceRow} ${selected?.invoiceId === invoice.invoiceId ? styles.selectedRow : ""}`} key={invoice.invoiceId}>
            <div className={styles.invoiceMain}><strong>{invoice.invoiceNumber.startsWith("DRAFT-") ? "New invoice" : invoice.invoiceNumber}</strong><span>Due {new Date(invoice.dueDate).toLocaleDateString("en-ZA")}</span></div>
            <div className={styles.invoiceAmount}>R {invoice.amount.toLocaleString("en-ZA", { minimumFractionDigits: 2 })}</div>
            <span className={styles.statusPill}>{statusLabel(invoice.status)}</span>
            <button type="button" className={styles.secondaryButton} onClick={() => void openInvoice(invoice.invoiceId)}>Open</button>
          </article>)}
        </div>}
      </section>

      {selected && <section className={styles.detailCard}>
        <div className={styles.sectionHeader}><div><p className={styles.cardEyebrow}>Invoice details</p><h2>{selected.invoiceNumber.startsWith("DRAFT-") ? "Complete invoice" : selected.invoiceNumber}</h2></div><button type="button" className={styles.textButton} onClick={() => setSelected(null)}>Close</button></div>
        <div className={styles.detailGrid}>
          <label>Invoice number<input value={form.invoiceNumber} onChange={(e) => setForm({ ...form, invoiceNumber: e.target.value })} disabled={isBusy} /></label>
          <label>Debtor name<input value={form.debtorName} onChange={(e) => setForm({ ...form, debtorName: e.target.value })} disabled={isBusy} /></label>
          <label>Debtor contact<input value={form.debtorContactDetails} onChange={(e) => setForm({ ...form, debtorContactDetails: e.target.value })} disabled={isBusy} /></label>
          <label>Amount (R)<input type="number" min="0.01" step="0.01" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} disabled={isBusy} /></label>
          <label>Issue date<input type="date" value={form.issueDate} onChange={(e) => setForm({ ...form, issueDate: e.target.value })} disabled={isBusy} /></label>
          <label>Due date<input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} disabled={isBusy} /></label>
        </div>
        {selected.reviewNotes && <div className={styles.reviewNote}><strong>Review note</strong><p>{selected.reviewNotes}</p></div>}
        <div className={styles.timeline} aria-label="Invoice status timeline">
          {["Draft", "Submitted", "UnderReview", "Approved", "Listed"].map((step, index) => {
            const order: Record<string, number> = { Draft: 0, Submitted: 1, UnderReview: 2, Approved: 3, Listed: 4, Rejected: -1 };
            const current = order[selected.status] ?? 0;
            const complete = selected.status !== "Rejected" && current >= index;
            return <div className={`${styles.timelineStep} ${complete ? styles.timelineComplete : ""}`} key={step}><span>{index + 1}</span><div><strong>{statusLabel(step as InvoiceSummary["status"])}</strong><small>{complete ? "Reached" : "Next step"}</small></div></div>;
          })}
        </div>
        {selected.status === "Rejected" && <div className={styles.rejectedAction}><strong>Resubmission required</strong><p>Correct the fields above, save them, and submit the invoice again for review.</p></div>}
        <div className={styles.actions}>
          {(selected.status === "Draft" || selected.status === "Rejected") && <><button type="button" className={styles.secondaryButton} onClick={() => void handleSave()} disabled={isBusy}>Save details</button><button type="button" className={styles.primaryButton} onClick={() => void handleSubmit()} disabled={isBusy}>Submit for review</button></>}
          {selected.status === "Approved" && <span className={styles.approvedMessage}>Approved — this invoice is eligible for a funding request.</span>}
        </div>

        {selected.status === "Approved" && <div className={styles.fundingStatus}>
          <div><strong>Ready for financing</strong><p>Your invoice has passed invoice review. Choose how much you want to request and how investors should fund it.</p></div>
          <span className={styles.statusPill}>Next: Funding request</span>
        </div>}
        {selected.status === "Listed" && <div className={styles.fundingStatus}>
          <div><strong>Funding opportunity listed</strong><p>Your funding request was approved and the opportunity is now available to investors. Monitor the campaign from your account as funding progresses.</p></div>
          <span className={styles.statusPill}>Listed</span>
        </div>}
        {selected.status === "Rejected" && <div className={styles.fundingStatus}>
          <div><strong>Action required</strong><p>Review the credit team's notes above, correct the invoice where necessary, and resubmit it for review.</p></div>
          <span className={styles.statusPill}>Resubmit</span>
        </div>}
        {selected.status === "Approved" && <div className={styles.fundingBox}>
          <div><p className={styles.cardEyebrow}>Funding request</p><h3>Turn the approved invoice into working capital</h3><p className={styles.muted}>The backend will create a pending funding request for credit review.</p></div>
          <div className={styles.fundingGrid}>
            <label>Requested amount (R)<input type="number" min="0.01" max={selected.amount} step="0.01" value={requestedAmount} onChange={(e) => setRequestedAmount(e.target.value)} disabled={isFunding} /></label>
            <label>Funding model<select value={fundingModel} onChange={(e) => setFundingModel(e.target.value as FundingModel)} disabled={isFunding}>{fundingOptions.map((option) => <option key={option.value} value={option.value}>{option.label}</option>)}</select></label>
          </div>
          <p className={styles.modelHelp}>{fundingOptions.find((option) => option.value === fundingModel)?.description}</p>
          <button type="button" className={styles.primaryButton} onClick={() => void handleFundingRequest()} disabled={isFunding}>{isFunding ? "Submitting…" : "Request funding"}</button>
        </div>}
      </section>}
    </main>
  );
}
