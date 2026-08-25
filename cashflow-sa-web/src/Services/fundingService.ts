import { api } from "./api";

export type FundingModel = "SingleInvestor" | "Fractional" | "Auction";

export interface CreateFundingRequest {
  invoiceId: string;
  requestedAmount: number;
  fundingModel: FundingModel;
}

export interface ListingDetail {
  listingId: string;
  campaignId: string;
  riskGrade: string;
  riskScore: number;
  industry: string;
  fundingModel: string;
  targetAmount: number;
  fundedAmount: number;
  tenorDays: number;
  campaignStatus: string;
  fundingDeadline: string | null;
  publishedAt: string;
  riskExplanationText: string | null;
  investmentSummary: string | null;
  explanationAvailable: boolean;
}

/**
 * SME:
 * Create a funding request from an approved invoice.
 */
export async function createFundingRequest(
  request: CreateFundingRequest
) {
  const response = await api.post(
    "/Funding/request",
    request
  );

  return response.data;
}

/**
 * Investor:
 * Get marketplace listing details.
 */
export async function getListingDetail(listingId: string) {
  const response = await api.get<ListingDetail>(
    `/Marketplace/listings/${listingId}`
  );

  return response.data;
}

/**
 * Investor:
 * Commit the full funding amount as a single investor.
 */
export async function commitSingleInvestor(
  campaignId: string,
  investorId: string,
  amount: number
) {
  const response = await api.post<{ investmentId: string }>(
    `/Funding/single-investor/${campaignId}/commit`,
    {
      investorId,
      amount,
    }
  );

  return response.data;
}

/**
 * Investor:
 * Commit part of a funding campaign.
 */
export async function commitFractional(
  campaignId: string,
  investorId: string,
  amount: number
) {
  const response = await api.post<{ investmentId: string }>(
    `/Funding/fractional/${campaignId}/commit`,
    {
      investorId,
      amount,
    }
  );

  return response.data;
}

/**
 * Investor:
 * Place a bid on an auction campaign.
 */
export async function placeAuctionBid(
  campaignId: string,
  investorId: string,
  amount: number
) {
  const response = await api.post<{ bidId: string }>(
    `/Funding/auction/${campaignId}/bid`,
    {
      investorId,
      amount,
    }
  );

  return response.data;
}
export interface CampaignStatus {
  campaignId: string;
  status: string;
  fundingModel: string;
  targetAmount: number;
  fundedAmount: number;
  fundingDeadline: string | null;
}

export async function getCampaignStatus(campaignId: string) {
  const response = await api.get<CampaignStatus>(`/Funding/campaign/${campaignId}/status`);
  return response.data;
}