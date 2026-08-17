import { api } from "./api";

export type InvoiceStatus =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "Approved"
  | "Rejected"
  | "Listed";

export interface InvoiceSummary {
  invoiceId: string;
  invoiceNumber: string;
  amount: number;
  dueDate: string;
  status: InvoiceStatus;
}

export interface InvoiceDetails extends InvoiceSummary {
  smeId: string;
  debtorName: string;
  debtorContactDetails: string;
  issueDate: string;
  processingComplete: boolean;
  reviewNotes: string | null;
}

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;

export function validateInvoiceFile(file: File): string | null {
  if (file.size > MAX_FILE_SIZE_BYTES) return "Invoice file exceeds the 10MB size limit.";
  if (file.type !== "application/pdf") return "Only PDF invoice files are accepted.";
  return null;
}

export async function uploadInvoice(file: File): Promise<{ invoiceId: string }> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await api.post<{ invoiceId: string }>("/Invoice/upload", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });

  return response.data;
}

export async function getInvoicesBySme(smeId: string): Promise<InvoiceSummary[]> {
  const response = await api.get<InvoiceSummary[]>(`/Invoice/sme/${smeId}`);
  return response.data;
}

export async function getInvoice(invoiceId: string): Promise<InvoiceDetails> {
  const response = await api.get<InvoiceDetails>(`/Invoice/${invoiceId}`);
  return response.data;
}

export async function correctInvoiceFields(
  invoiceId: string,
  fields: {
    invoiceNumber: string;
    debtorName: string;
    debtorContactDetails: string;
    amount: number;
    issueDate: string;
    dueDate: string;
  }
): Promise<void> {
  await api.put(`/Invoice/${invoiceId}/correct`, fields);
}

export async function submitInvoice(invoiceId: string): Promise<void> {
  await api.post(`/Invoice/${invoiceId}/submit`);
}

// --- Ops review (CreditAnalyst/Admin) --------------------------------------

export type InvoiceReviewStatus =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "Approved"
  | "Rejected"
  | "Listed";

export interface InvoiceForReview {
  invoiceId: string;
  smeId: string;
  companyName: string;
  invoiceNumber: string;
  debtorName: string;
  amount: number;
  issueDate: string;
  dueDate: string;
  status: InvoiceReviewStatus;
  submittedAt: string | null;
  reviewNotes: string | null;
}

export async function getInvoicesForReview(
  statusFilter?: InvoiceReviewStatus
): Promise<InvoiceForReview[]> {
  const response = await api.get<InvoiceForReview[]>("/invoice-review", {
    params: statusFilter ? { status: statusFilter } : undefined,
  });
  return response.data;
}

export async function approveInvoiceReview(
  invoiceId: string,
  reviewerId: string,
  notes?: string
): Promise<void> {
  await api.post(`/invoice-review/${invoiceId}/approve`, { reviewerId, notes });
}

export async function rejectInvoiceReview(
  invoiceId: string,
  reviewerId: string,
  notes: string
): Promise<void> {
  await api.post(`/invoice-review/${invoiceId}/reject`, { reviewerId, notes });
}
