import { api } from "./api";

// Matches ListingSummaryDto exactly -- RiskGrade and Industry come back as
// strings ("A".."E", "Agriculture", etc.), since the backend stores every
// enum as a string, not an int.
export interface Listing {
  listingId: string;
  campaignId: string;
  riskGrade: string;
  riskScore: number;
  industry: string;
  targetAmount: number;
  fundedAmount: number;
  tenorDays: number;
  publishedAt: string;
}

export async function getListings(): Promise<Listing[]> {
  const response = await api.get<Listing[]>("/Marketplace/listings");
  return response.data;
}