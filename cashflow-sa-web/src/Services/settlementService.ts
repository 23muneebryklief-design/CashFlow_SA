import { api } from "./api";

export interface Settlement {
  settlementId: string;
  campaignId: string;
  settledAmount: number;
  status: number | string;
  paymentProvider: string;
  referenceNumber: string;
  settlementDate: string;
}

export interface TriggerSettlementRequest {
  settledAmount: number;
  paymentProvider: string;
  referenceNumber: string;
}

export async function getSettlement(settlementId: string): Promise<Settlement> {
  const response = await api.get<Settlement>(`/Settlement/${settlementId}`);
  return response.data;
}

export async function triggerSettlement(
  campaignId: string,
  request: TriggerSettlementRequest
): Promise<{ settlementId: string }> {
  const response = await api.post<{ settlementId: string }>(
    `/Settlement/${campaignId}/trigger`,
    request
  );
  return response.data;
}
