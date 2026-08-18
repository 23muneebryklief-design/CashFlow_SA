import { api } from "./api";

// Talks to FundingRequestReviewController. This is the underwriting step: a
// Credit Analyst approves a Pending FundingRequest, which creates the
// FundingCampaign + MarketplaceListing investors actually see and fund.

export type FundingRequestStatus = "Pending" | "UnderReview" | "Approved" | "Rejected";
export type FundingModel = "SingleInvestor" | "Fractional" | "Auction";

export interface FundingRequestForReview {
  fundingRequestId: string;
  invoiceId: string;
  smeId: string;
  companyName: string;
  invoiceNumber: string;
  debtorName: string;
  invoiceAmount: number;
  dueDate: string;
  requestedAmount: number;
  fundingModel: FundingModel;
  status: FundingRequestStatus;
  submittedAt: string;
  riskScore: number | null;
  riskGrade: string | null;
}

export async function getFundingRequestsForReview(
  statusFilter?: FundingRequestStatus
): Promise<FundingRequestForReview[]> {
  const response = await api.get<FundingRequestForReview[]>("/funding-request-review", {
    params: statusFilter ? { status: statusFilter } : undefined,
  });
  return response.data;
}

export interface ApproveFundingRequestInput {
  reviewerId: string;
  // Required unless the request's fundingModel is "Auction".
  expectedReturnRate?: number;
  fundingDeadline?: string;
}

export async function approveFundingRequest(
  fundingRequestId: string,
  input: ApproveFundingRequestInput
): Promise<{ campaignId: string }> {
  const response = await api.post<{ campaignId: string }>(
    `/funding-request-review/${fundingRequestId}/approve`,
    input
  );
  return response.data;
}

export async function rejectFundingRequest(
  fundingRequestId: string,
  reviewerId: string,
  notes: string
): Promise<void> {
  await api.post(`/funding-request-review/${fundingRequestId}/reject`, {
    reviewerId,
    notes,
  });
}
