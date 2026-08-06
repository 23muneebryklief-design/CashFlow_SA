import { useEffect, useState } from "react";
import { getListings, type Listing } from "../../Services/marketplaceService";
import ListingCard from "../../components/Dashboard/ListingCard/ListingCard";
import styles from "./InvestorMarketplace.module.css";

export default function InvestorMarketplace() {
  const [listings, setListings] = useState<Listing[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadListings() {
      try {
        const data = await getListings();
        setListings(data);
      } catch {
        setError("Could not load the marketplace. Please try again.");
      } finally {
        setIsLoading(false);
      }
    }

    loadListings();
  }, []);

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Investor Marketplace</p>
        <h1>Live opportunities</h1>
      </header>

      {isLoading ? (
        <p className={styles.status}>Loading opportunities...</p>
      ) : error ? (
        <p className={styles.status}>{error}</p>
      ) : listings.length === 0 ? (
        <p className={styles.status}>No listings available right now.</p>
      ) : (
        <div className={styles.grid}>
          {listings.map((listing) => (
            <ListingCard key={listing.listingId} listing={listing} />
          ))}
        </div>
      )}
    </main>
  );
}
