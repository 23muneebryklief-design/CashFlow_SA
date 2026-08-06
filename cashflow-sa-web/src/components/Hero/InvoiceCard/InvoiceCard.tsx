import styles from "./InvoiceCard.module.css";

export default function InvoiceCard() {
  return (
    <div className={styles.wrapper}>
      <div className={styles.card}>
        <div className={styles.top}>
          <div>
            <span className={styles.label}>Invoice</span>
            <h3>#INV-08213</h3>
          </div>

          <span className={styles.stamp}>FUNDED</span>
        </div>

        <p className={styles.amountLabel}>Financed amount</p>

        <h2 className={styles.amount}>
          R 184,500<span>.00</span>
        </h2>

        <div className={styles.details}>
          <div>
            <span>Debtor</span>
            <strong>Thusong Retail Group</strong>
          </div>

          <div>
            <span>Tenor</span>
            <strong>60 Days</strong>
          </div>

          <div>
            <span>Advance</span>
            <strong>85%</strong>
          </div>
        </div>

        <div className={styles.progress}>
          <div className={styles.track}>
            <div className={styles.fill}></div>
          </div>

          <span>78% Funded</span>
        </div>
      </div>

      <div className={`${styles.floatCard} ${styles.topFloat}`}>
        ✓ Investor matched — <strong>R42,000</strong>
      </div>

      <div className={`${styles.floatCard} ${styles.bottomFloat}`}>
        ✓ Payout in <strong>2 Days</strong>
      </div>
    </div>
  );
}