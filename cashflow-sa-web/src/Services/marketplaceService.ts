import { api } from "./api";

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

export interface MarketplaceFilters {
  riskGrade?: string;
  industry?: string;
  minAmount?: number;
  maxAmount?: number;
  minTenorDays?: number;
  maxTenorDays?: number;
}

export async function getListings(filters: MarketplaceFilters = {}): Promise<Listing[]> {
  const params = Object.fromEntries(
    Object.entries(filters).filter(([, value]) => value !== undefined && value !== ""),
  );
  const response = await api.get<Listing[]>("/Marketplace/listings", { params });
  return response.data;
}
