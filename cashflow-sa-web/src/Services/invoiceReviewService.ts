import { api } from "./api";

// Talks to InvoiceReviewController. This is the Credit Analyst's approve/reject
// step on a Submitted invoice -- approving here is what kicks off the Ollama
// risk scoring pipeline on the backend before the invoice can be financed.

export type ReviewInvoiceStatus =
  | "Draft"
  | "Submitted"
  | "UnderReview"
  | "Approved"
  | "Rejected";

export interface InvoiceForReview {
  invoiceId: string;
  smeId: string;
  companyName: string;
  invoiceNumber: string;
  debtorName: string;
  amount: number;
  issueDate: string;
  dueDate: string;
  status: ReviewInvoiceStatus;
  submittedAt: string | null;
  reviewNotes: string | null;
}

export async function getInvoicesForReview(
  statusFilter?: ReviewInvoiceStatus
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
  await api.post(`/invoice-review/${invoiceId}/approve`, {
    reviewerId,
    notes,
  });
}

export async function rejectInvoiceReview(
  invoiceId: string,
  reviewerId: string,
  notes: string
): Promise<void> {
  await api.post(`/invoice-review/${invoiceId}/reject`, {
    reviewerId,
    notes,
  });
}
