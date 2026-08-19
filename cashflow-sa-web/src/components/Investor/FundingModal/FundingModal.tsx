import { useEffect, useState } from "react";
import Modal from "../../Shared/Modal/Modal";
import type { Listing } from "../../../Services/marketplaceService";
import { getListingDetail, commitFractional, commitSingleInvestor, placeAuctionBid, type ListingDetail } from "../../../Services/fundingService";
import { useAuth } from "../../../Hooks/useAuth";
import styles from "./FundingModal.module.css";

interface Props { listing: Listing | null; onClose: () => void; onSuccess: () => void; }

const money = (n: number) => new Intl.NumberFormat("en-ZA", { style: "currency", currency: "ZAR", maximumFractionDigits: 2 }).format(n);

export default function FundingModal({ listing, onClose, onSuccess }: Props) {
  const { user } = useAuth();
  const [detail, setDetail] = useState<ListingDetail | null>(null);
  const [amount, setAmount] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!listing) return;
    setDetail(null); setAmount(""); setError(null);
    getListingDetail(listing.listingId).then(setDetail).catch(() => setError("Could not load this opportunity."));
  }, [listing]);

  if (!listing) return null;
  const model = detail?.fundingModel ?? "";
  const remaining = detail ? Math.max(0, detail.targetAmount - detail.fundedAmount) : 0;
  const deadlinePassed = !!detail?.fundingDeadline && new Date(detail.fundingDeadline).getTime() <= Date.now();
  const campaignOpen = detail ? ["Listed", "Funding"].includes(detail.campaignStatus) : false;
  const fundingUnavailable = !!detail && (!campaignOpen || deadlinePassed || remaining <= 0);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    if (!user || !detail) return;
    if (fundingUnavailable) { setError(deadlinePassed ? "This campaign funding deadline has passed." : "This campaign is no longer accepting funding."); return; }
    const value = Number(amount);
    if (!Number.isFinite(value) || value <= 0) { setError("Enter an investment amount greater than zero."); return; }
    if (value > remaining) { setError("That amount is greater than the remaining campaign target."); return; }
    if (model === "SingleInvestor" && value !== remaining) { setError(`Single-investor funding must cover the full remaining ${money(remaining)}.`); return; }
    setLoading(true); setError(null);
    try {
      if (model === "SingleInvestor") await commitSingleInvestor(detail.campaignId, user.profileId ?? "", value);
      else if (model === "Fractional") await commitFractional(detail.campaignId, user.profileId ?? "", value);
      else if (model === "Auction") await placeAuctionBid(detail.campaignId, user.profileId ?? "", value);
      else throw new Error("Unsupported funding model.");
      onSuccess(); onClose();
    } catch (err: any) {
      setError(err?.response?.data?.detail ?? err?.response?.data?.title ?? "The funding action could not be completed.");
    } finally { setLoading(false); }
  }

  return (
    <Modal isOpen={!!listing} onClose={() => !loading && onClose()} title="Funding opportunity">
      <div className={styles.summary}>
        <span>{detail?.industry ?? listing.industry}</span>
        <strong>{detail ? money(detail.targetAmount) : "Loading..."}</strong>
        {detail && <small>{detail.fundingModel} · {detail.tenorDays} days · {detail.riskGrade} risk</small>}
      </div>
      {detail && fundingUnavailable && !error && <p className={styles.error}>{deadlinePassed ? "Funding deadline has passed." : remaining <= 0 ? "This campaign is fully funded." : `Campaign is ${detail.campaignStatus.toLowerCase()} and is not accepting funding.`}</p>}
      {error && <p className={styles.error}>{error}</p>}
      {detail && (
        <form onSubmit={submit} className={styles.form}>
          <p>Remaining target: <strong>{money(remaining)}</strong></p>
          <label htmlFor="funding-amount">{model === "Auction" ? "Bid amount" : "Investment amount"}</label>
          <input id="funding-amount" type="number" min="0.01" step="0.01" value={amount} disabled={loading} onChange={e => setAmount(e.target.value)} placeholder="0.00" />
          <button type="submit" disabled={loading || fundingUnavailable}>{loading ? "Processing..." : model === "Auction" ? "Place bid" : "Invest now"}</button>
        </form>
      )}
    </Modal>
  );
}
