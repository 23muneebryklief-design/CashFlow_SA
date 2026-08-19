import { api } from "./api";

export interface FundingVolume {
  totalCampaigns: number;
  totalTargetAmount: number;
  totalFundedAmount: number;
  totalSettledAmount: number;
  averageFundingPercentage: number;
}

export interface RiskDistributionItem {
  riskGrade: string | number;
  count: number;
}

export async function getFundingVolume(fromDate?: string, toDate?: string) {
  const response = await api.get<FundingVolume>("/Analytics/funding-volume", {
    params: { fromDate, toDate },
  });
  return response.data;
}

export async function getRiskDistribution() {
  const response = await api.get<RiskDistributionItem[]>("/Analytics/risk-distribution");
  return response.data;
}
