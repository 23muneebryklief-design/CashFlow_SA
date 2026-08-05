import styles from "./DealCard.module.css";

export interface Deal {
  id: number;
  business: string;
  sector: string;
  amount: string;
  tenor: string;
  returnRate: string;
  funded: string;
  risk: "Low" | "Medium";
}

interface DealCardProps {
  deal: Deal;
}

export default function DealCard({ deal }: DealCardProps) {
  return (
    <article className={styles.card}>
      <div className={styles.header}>
        <div>
          <h3>{deal.business}</h3>
          <p>{deal.sector}</p>
        </div>

        <span
          className={`${styles.risk} ${
            deal.risk === "Low" ? styles.low : styles.medium
          }`}
        >
          {deal.risk} Risk
        </span>
      </div>

      <div className={styles.body}>
        <h2>
          {deal.amount}
          <span> ZAR</span>
        </h2>

        <div className={styles.meta}>
          <div>
            <span>Tenor</span>
            <strong>{deal.tenor}</strong>
          </div>

          <div>
            <span>Return</span>
            <strong>{deal.returnRate}</strong>
          </div>

          <div>
            <span>Funded</span>
            <strong>{deal.funded}</strong>
          </div>
        </div>
      </div>
    </article>
  );
}