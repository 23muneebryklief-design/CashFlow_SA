import { api } from "./api";

export async function getInvoiceDocumentDownloadUrl(
  invoiceId: string,
): Promise<{ url: string; expiresAt: string }> {
  const response = await api.get<{ url: string; expiresAt: string }>(
    `/invoice/${invoiceId}/document/download-url`,
  );
  return response.data;
}
