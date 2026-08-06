import type { Listing } from "../../../Services/marketplaceService";
import styles from "./ListingCard.module.css";

interface ListingCardProps {
  listing: Listing;
}

// Risk grades A/B are treated as "good", C as neutral, D/E as "caution" --
// purely a visual grouping for the badge color, not a business rule.
function riskClass(grade: string): string {
  if (grade === "A" || grade === "B") return styles.low;
  if (grade === "C") return styles.medium;
  return styles.high;
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("en-ZA", { maximumFractionDigits: 0 }).format(amount);
}

export default function ListingCard({ listing }: ListingCardProps) {
  const fundedPercent = listing.targetAmount > 0
    ? Math.round((listing.fundedAmount / listing.targetAmount) * 100)
    : 0;

  return (
    <article className={styles.card}>
      <div className={styles.header}>
        <div>
          <h3>{listing.industry}</h3>
          <p>Campaign {listing.campaignId.slice(0, 8)}</p>
        </div>

        <span className={`${styles.risk} ${riskClass(listing.riskGrade)}`}>
          Grade {listing.riskGrade}
        </span>
      </div>

      <div className={styles.body}>
        <h2>
          {formatCurrency(listing.targetAmount)}
          <span> ZAR</span>
        </h2>

        <div className={styles.meta}>
          <div>
            <span>Tenor</span>
            <strong>{listing.tenorDays}d</strong>
          </div>

          <div>
            <span>Risk score</span>
            <strong>{listing.riskScore.toFixed(1)}</strong>
          </div>

          <div>
            <span>Funded</span>
            <strong>{fundedPercent}%</strong>
          </div>
        </div>
      </div>
    </article>
  );
}