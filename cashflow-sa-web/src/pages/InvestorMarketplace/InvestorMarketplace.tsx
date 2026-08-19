import { useEffect, useMemo, useState } from "react";
import { getListings, type Listing } from "../../Services/marketplaceService";
import ListingCard from "../../components/Dashboard/ListingCard/ListingCard";
import FundingModal from "../../components/Investor/FundingModal/FundingModal";
import styles from "./InvestorMarketplace.module.css";

type SortOption = "newest" | "funding" | "riskLow" | "targetLow" | "tenorShort";

export default function InvestorMarketplace() {
  const [listings, setListings] = useState<Listing[]>([]);
  const [selected, setSelected] = useState<Listing | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [industry, setIndustry] = useState("all");
  const [risk, setRisk] = useState("all");
  const [sortBy, setSortBy] = useState<SortOption>("newest");

  async function loadListings() {
    setIsLoading(true);
    try {
      setError(null);
      setListings(await getListings());
    } catch {
      setError("Could not load the marketplace. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => { loadListings(); }, []);

  const industries = useMemo(
    () => Array.from(new Set(listings.map((listing) => listing.industry).filter(Boolean))).sort(),
    [listings],
  );

  const visibleListings = useMemo(() => {
    const query = search.trim().toLowerCase();
    const riskRank: Record<string, number> = { A: 1, B: 2, C: 3, D: 4, E: 5 };

    const filtered = listings.filter((listing) => {
      const matchesSearch = !query || [listing.industry, listing.riskGrade, listing.campaignId, listing.listingId]
        .some((value) => value.toLowerCase().includes(query));
      const matchesIndustry = industry === "all" || listing.industry === industry;
      const matchesRisk = risk === "all" || listing.riskGrade === risk;
      return matchesSearch && matchesIndustry && matchesRisk;
    });

    return [...filtered].sort((a, b) => {
      if (sortBy === "funding") {
        const aProgress = a.targetAmount > 0 ? a.fundedAmount / a.targetAmount : 0;
        const bProgress = b.targetAmount > 0 ? b.fundedAmount / b.targetAmount : 0;
        return bProgress - aProgress;
      }
      if (sortBy === "riskLow") return (riskRank[a.riskGrade] ?? 99) - (riskRank[b.riskGrade] ?? 99);
      if (sortBy === "targetLow") return a.targetAmount - b.targetAmount;
      if (sortBy === "tenorShort") return a.tenorDays - b.tenorDays;
      return new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime();
    });
  }, [listings, search, industry, risk, sortBy]);

  function resetFilters() {
    setSearch("");
    setIndustry("all");
    setRisk("all");
    setSortBy("newest");
  }

  const hasFilters = Boolean(search || industry !== "all" || risk !== "all" || sortBy !== "newest");

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Investor Marketplace</p>
        <h1>Live opportunities</h1>
        <p className={styles.description}>Compare risk, tenor and funding progress, then commit from your investor wallet.</p>
      </header>

      {!isLoading && !error && listings.length > 0 && (
        <section className={styles.toolbar} aria-label="Marketplace filters">
          <div className={styles.filterRow}>
            <input
              className={styles.search}
              aria-label="Search marketplace"
              placeholder="Search industry, campaign or risk grade…"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
            <select aria-label="Filter by industry" value={industry} onChange={(event) => setIndustry(event.target.value)}>
              <option value="all">All industries</option>
              {industries.map((item) => <option key={item} value={item}>{item}</option>)}
            </select>
            <select aria-label="Filter by risk grade" value={risk} onChange={(event) => setRisk(event.target.value)}>
              <option value="all">All risk grades</option>
              {['A', 'B', 'C', 'D', 'E'].map((item) => <option key={item} value={item}>Risk {item}</option>)}
            </select>
            <select aria-label="Sort marketplace" value={sortBy} onChange={(event) => setSortBy(event.target.value as SortOption)}>
              <option value="newest">Newest</option>
              <option value="funding">Most funded</option>
              <option value="riskLow">Lowest risk</option>
              <option value="targetLow">Lowest target</option>
              <option value="tenorShort">Shortest tenor</option>
            </select>
          </div>
          <div className={styles.toolbarMeta}>
            <span>{visibleListings.length} of {listings.length} opportunities</span>
            {hasFilters && <button type="button" className={styles.clearButton} onClick={resetFilters}>Clear filters</button>}
          </div>
        </section>
      )}

      {isLoading ? <p className={styles.status}>Loading opportunities...</p> : error ? (
        <div className={styles.status}><p>{error}</p><button type="button" onClick={loadListings}>Try again</button></div>
      ) : listings.length === 0 ? <p className={styles.status}>No listings available right now.</p> : visibleListings.length === 0 ? (
        <div className={styles.status}><h3>No matching opportunities</h3><p>Try changing your search or filters.</p><button type="button" onClick={resetFilters}>Clear filters</button></div>
      ) : (
        <div className={styles.grid}>{visibleListings.map((listing) => <ListingCard key={listing.listingId} listing={listing} onOpen={setSelected} />)}</div>
      )}
      <FundingModal listing={selected} onClose={() => setSelected(null)} onSuccess={loadListings} />
    </main>
  );
}
