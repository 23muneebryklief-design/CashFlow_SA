import { api } from "./api";

export interface Investment {
  investmentId: string;
  campaignId: string;
  industry: string;
  amount: number;
  status: string;
  investedAt: string;
  tenorDays: number;
  returnAmount: number | null;
}

export async function getInvestorInvestments(investorId: string) {
  const response = await api.get<Investment[]>(`/Investment/investor/${investorId}`);
  return response.data;
}
