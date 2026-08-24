import { api } from "./api";

export type ReviewInvoiceStatus =
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
  status: ReviewInvoiceStatus;
  submittedAt: string | null;
  reviewNotes: string | null;
}

/**
 * Get invoices available to the Credit Analyst/Admin review queue.
 *
 * Backend:
 * GET /api/InvoiceReview?status=Submitted
 */
export async function getInvoicesForReview(
  statusFilter?: ReviewInvoiceStatus
): Promise<InvoiceForReview[]> {
  const response = await api.get<InvoiceForReview[]>("/InvoiceReview", {
    params: statusFilter
      ? { status: statusFilter }
      : undefined,
  });

  return response.data;
}

/**
 * Approve an invoice.
 *
 * Backend:
 * POST /api/InvoiceReview/{invoiceId}/approve
 */
export async function approveInvoiceReview(
  invoiceId: string,
  reviewerId: string,
  notes?: string
): Promise<void> {
  await api.post(`/InvoiceReview/${invoiceId}/approve`, {
    reviewerId,
    notes,
  });
}

/**
 * Reject an invoice.
 *
 * Backend:
 * POST /api/InvoiceReview/{invoiceId}/reject
 */
export async function rejectInvoiceReview(
  invoiceId: string,
  reviewerId: string,
  notes: string
): Promise<void> {
  await api.post(`/InvoiceReview/${invoiceId}/reject`, {
    reviewerId,
    notes,
  });
}