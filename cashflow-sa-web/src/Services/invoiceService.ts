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
