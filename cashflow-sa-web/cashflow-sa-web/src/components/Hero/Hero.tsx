import styles from "./Hero.module.css";
import InvoiceCard from "./InvoiceCard/InvoiceCard";

export default function Hero() {
  return (
    <section className={styles.hero}>
      <div className={`wrap ${styles.inner}`}>
        <div className={styles.content}>
          <div className={styles.eyebrow}>
            POPIA-aligned · SME & Investor Marketplace
          </div>

          <h1>
            Turn unpaid invoices into <em>working capital</em>, in days —
            not months.
          </h1>

          <p className={styles.lede}>
            CashFlow SA connects verified South African SMEs with vetted
            investors, funding approved invoices within 48 hours through a fully
            digital, transparent marketplace.
          </p>

          <div className={styles.actions}>
            <a href="#" className="btn btn-primary">
              I'm an SME — Get funded
            </a>

            <a href="#" className="btn btn-ghost">
              I'm an Investor — Browse deals
            </a>
          </div>

          <div className={styles.trust}>
            <span>✓ POPIA-aligned data handling</span>
            <span>✓ Bank-grade encryption</span>
            <span>✓ Funds held in escrow</span>
          </div>
        </div>

        <InvoiceCard />
      </div>
    </section>
  );
}