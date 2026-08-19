import { api } from "./api";

export type DocumentType =
  | "CompanyRegistration"
  | "IdentityDocument"
  | "ProofOfAddress"
  | "TaxCertificate"
  | "BankStatement"
  | "Other";

export type KycStatus = "Pending" | "Verified" | "Rejected";
export type KycStatusView = KycStatus | "NotSubmitted";

export type DocumentStatus = "Pending" | "Approved" | "Rejected";

export interface UploadedKycDocument {
  fileName: string;
  filePath: string;
  fileSize: number;
}

export interface KycDocumentInput {
  documentType: DocumentType;
  fileName: string;
  filePath: string;
  fileSize: number;
}

export interface KycDocumentStatus {
  documentType: DocumentType;
  fileName: string;
  status: DocumentStatus;
  uploadedAt: string;
}

export interface KycStatusResponse {
  applicationId: string;
  status: KycStatus;
  applicationDate: string;
  documents: KycDocumentStatus[];
}

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024;
const ALLOWED_CONTENT_TYPES = ["application/pdf", "image/jpeg", "image/png"];

export function validateKycFile(file: File): string | null {
  if (file.size > MAX_FILE_SIZE_BYTES) return "File exceeds the 10MB size limit.";
  if (!ALLOWED_CONTENT_TYPES.includes(file.type)) return "Only PDF, JPEG, and PNG files are accepted.";
  return null;
}

export async function uploadKycDocument(file: File): Promise<UploadedKycDocument> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await api.post<UploadedKycDocument>("/Kyc/upload-document", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return response.data;
}

export async function submitKycApplication(
  smeId: string,
  documents: KycDocumentInput[]
): Promise<{ applicationId: string }> {
  const response = await api.post<{ applicationId: string }>("/Kyc/submit", {
    smeId,
    documents,
  });
  return response.data;
}

export async function getKycStatus(smeId: string): Promise<KycStatusResponse | null> {
  try {
    const response = await api.get<KycStatusResponse>(`/Kyc/status/${smeId}`);
    return response.data;
  } catch (error: unknown) {
    const status = (error as { response?: { status?: number } })?.response?.status;
    if (status === 404) return null;
    throw error;
  }
}

// --- Auditor review -------------------------------------------------------
// Everything below talks to AuditorKycController. Documents come back
// grouped into a "section" per SME so a reviewer can go through a business's
// whole submission together, rather than as a flat, unattributed list.

export interface AuditorKycDocument {
  documentId: string;
  documentType: DocumentType;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
  status: DocumentStatus;
  reviewedAt: string | null;
  reviewNotes: string | null;
}

export interface SmeKycReviewSection {
  smeId: string;
  userId: string;
  companyName: string;
  contactPerson: string;
  applicationStatus: KycStatus | null;
  documents: AuditorKycDocument[];
}

export async function getKycDocumentsForReview(
  statusFilter?: DocumentStatus
): Promise<SmeKycReviewSection[]> {
  const response = await api.get<SmeKycReviewSection[]>("/auditor/kyc/documents", {
    params: statusFilter ? { status: statusFilter } : undefined,
  });
  return response.data;
}

export async function getKycDocumentDownloadUrl(
  documentId: string
): Promise<{ url: string; expiresAt: string }> {
  const response = await api.get<{ url: string; expiresAt: string }>(
    `/auditor/kyc/documents/${documentId}/download-url`
  );
  return response.data;
}

export async function approveKycDocument(
  documentId: string,
  reviewerId: string,
  notes?: string
): Promise<void> {
  await api.post(`/auditor/kyc/documents/${documentId}/approve`, {
    reviewerId,
    notes,
  });
}

export async function rejectKycDocument(
  documentId: string,
  reviewerId: string,
  notes: string
): Promise<void> {
  await api.post(`/auditor/kyc/documents/${documentId}/reject`, {
    reviewerId,
    notes,
  });
}

// --- Credit Analyst / Admin application-level review -----------------------
// Everything below talks to AdminKycController. This is a separate, higher
// level step from the Auditor's per-document review above: it approves or
// rejects the whole KYC application (SRS 5.2), not an individual document.

export interface PendingKycApplication {
  applicationId: string;
  smeId: string;
  companyName: string;
  applicationDate: string;
}

export async function getPendingKycApplications(): Promise<PendingKycApplication[]> {
  const response = await api.get<PendingKycApplication[]>("/admin/kyc/pending");
  return response.data;
}

export async function approveKycApplication(
  applicationId: string,
  reviewerId: string,
  notes?: string
): Promise<void> {
  await api.post(`/admin/kyc/${applicationId}/approve`, {
    reviewerId,
    notes,
  });
}

export async function rejectKycApplication(
  applicationId: string,
  reviewerId: string,
  notes: string
): Promise<void> {
  await api.post(`/admin/kyc/${applicationId}/reject`, {
    reviewerId,
    notes,
  });
}
