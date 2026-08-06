import styles from "./FeaturedDeals.module.css";
import DealCard, { type Deal } from "./DealCard/DealCard";

const deals: Deal[] = [
  {
    id: 1,
    business: "Kagiso Logistics",
    sector: "Freight & Transport",
    amount: "R 96,000",
    tenor: "45 Days",
    returnRate: "12.8%",
    funded: "64%",
    risk: "Low",
  },
  {
    id: 2,
    business: "Sunrise Bakery Co.",
    sector: "Food & Retail",
    amount: "R 41,250",
    tenor: "30 Days",
    returnRate: "15.4%",
    funded: "29%",
    risk: "Medium",
  },
  {
    id: 3,
    business: "Mzansi Fabrication",
    sector: "Manufacturing",
    amount: "R 212,800",
    tenor: "60 Days",
    returnRate: "13.1%",
    funded: "91%",
    risk: "Low",
  },
];

export default function Marketplace() {
  return (
    <section className={styles.section} id="deals">
      <div className="wrap">
        <div className={styles.heading}>
          <span>Live on the marketplace</span>

          <h2>Invoices open for funding</h2>

          <p>
            A snapshot of verified invoices currently seeking investors.
          </p>
        </div>

        <div className={styles.grid}>
          {deals.map((deal) => (
            <DealCard key={deal.id} deal={deal} />
          ))}
        </div>
      </div>
    </section>
  );
}