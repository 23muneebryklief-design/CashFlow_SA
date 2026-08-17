import { useEffect, useState } from "react";
import { getListings, type Listing } from "../../Services/marketplaceService";
import ListingCard from "../../components/Dashboard/ListingCard/ListingCard";
import FundingModal from "../../components/Investor/FundingModal/FundingModal";
import styles from "./InvestorMarketplace.module.css";

export default function InvestorMarketplace() {
  const [listings, setListings] = useState<Listing[]>([]);
  const [selected, setSelected] = useState<Listing | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function loadListings() {
    try { setError(null); setListings(await getListings()); }
    catch { setError("Could not load the marketplace. Please try again."); }
    finally { setIsLoading(false); }
  }

  useEffect(() => { loadListings(); }, []);

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Investor Marketplace</p>
        <h1>Live opportunities</h1>
        <p className={styles.description}>Compare risk, tenor and funding progress, then commit from your investor wallet.</p>
      </header>
      {isLoading ? <p className={styles.status}>Loading opportunities...</p> : error ? <p className={styles.status}>{error}</p> : listings.length === 0 ? <p className={styles.status}>No listings available right now.</p> :
        <div className={styles.grid}>{listings.map(l => <ListingCard key={l.listingId} listing={l} onOpen={setSelected} />)}</div>}
      <FundingModal listing={selected} onClose={() => setSelected(null)} onSuccess={loadListings} />
    </main>
  );
}
