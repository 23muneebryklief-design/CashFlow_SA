import { useEffect, useMemo, useState } from "react";
import { getListings, type Listing, type MarketplaceFilters } from "../../Services/marketplaceService";
import ListingCard from "../../components/Dashboard/ListingCard/ListingCard";
import FundingModal from "../../components/Investor/FundingModal/FundingModal";
import styles from "./InvestorMarketplace.module.css";

type SortOption = "newest" | "funding" | "riskLow" | "targetLow" | "tenorShort";

const INDUSTRIES = [
  "Agriculture", "Mining", "Manufacturing", "Construction", "WholesaleTrade", "RetailTrade",
  "TransportAndLogistics", "InformationTechnology", "Telecommunications", "FinancialServices",
  "Insurance", "RealEstate", "ProfessionalServices", "LegalServices", "AccountingAndAuditing",
  "Consulting", "Healthcare", "Education", "Hospitality", "Tourism", "FoodAndBeverage", "Energy",
  "Utilities", "MediaAndEntertainment", "SecurityServices", "CleaningServices", "Automotive",
  "GovernmentContractor", "NonProfit",
];

const prettyIndustry = (value: string) => value.replace(/([a-z])([A-Z])/g, "$1 $2");

export default function InvestorMarketplace() {
  const [listings, setListings] = useState<Listing[]>([]);
  const [selected, setSelected] = useState<Listing | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [risk, setRisk] = useState("all");
  const [industry, setIndustry] = useState("all");
  const [minAmount, setMinAmount] = useState("");
  const [maxAmount, setMaxAmount] = useState("");
  const [minTenor, setMinTenor] = useState("");
  const [maxTenor, setMaxTenor] = useState("");
  const [sortBy, setSortBy] = useState<SortOption>("newest");

  async function loadListings(filters: MarketplaceFilters = {}) {
    setIsLoading(true);
    try {
      setError(null);
      setListings(await getListings(filters));
    } catch {
      setError("Could not load the marketplace. Please try again.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => { void loadListings(); }, []);

  const visibleListings = useMemo(() => {
    const query = search.trim().toLowerCase();
    const riskRank: Record<string, number> = { A: 1, B: 2, C: 3, D: 4, E: 5 };
    const filtered = listings.filter((listing) => !query || [listing.industry, listing.riskGrade, listing.campaignId, listing.listingId].some((value) => value.toLowerCase().includes(query)));
    return [...filtered].sort((a, b) => {
      if (sortBy === "funding") {
        const ap = a.targetAmount > 0 ? a.fundedAmount / a.targetAmount : 0;
        const bp = b.targetAmount > 0 ? b.fundedAmount / b.targetAmount : 0;
        return bp - ap;
      }
      if (sortBy === "riskLow") return (riskRank[a.riskGrade] ?? 99) - (riskRank[b.riskGrade] ?? 99);
      if (sortBy === "targetLow") return a.targetAmount - b.targetAmount;
      if (sortBy === "tenorShort") return a.tenorDays - b.tenorDays;
      return new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime();
    });
  }, [listings, search, sortBy]);

  function applyFilters() {
    const filters: MarketplaceFilters = {
      riskGrade: risk !== "all" ? risk : undefined,
      industry: industry !== "all" ? industry : undefined,
      minAmount: minAmount ? Number(minAmount) : undefined,
      maxAmount: maxAmount ? Number(maxAmount) : undefined,
      minTenorDays: minTenor ? Number(minTenor) : undefined,
      maxTenorDays: maxTenor ? Number(maxTenor) : undefined,
    };
    void loadListings(filters);
  }

  function resetFilters() {
    setSearch(""); setIndustry("all"); setRisk("all"); setMinAmount(""); setMaxAmount(""); setMinTenor(""); setMaxTenor(""); setSortBy("newest");
    void loadListings();
  }

  const hasFilters = Boolean(search || industry !== "all" || risk !== "all" || minAmount || maxAmount || minTenor || maxTenor || sortBy !== "newest");

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <p className={styles.eyebrow}>Investor Marketplace</p>
        <h1>Live opportunities</h1>
        <p className={styles.description}>Filter active listings by risk grade, amount, industry and tenor using the marketplace API.</p>
      </header>

      <section className={styles.toolbar} aria-label="Marketplace filters">
        <div className={styles.filterRow}>
          <input className={styles.search} aria-label="Search marketplace" placeholder="Search campaign, industry or risk…" value={search} onChange={(event) => setSearch(event.target.value)} />
          <select value={risk} onChange={(event) => setRisk(event.target.value)} aria-label="Filter by risk grade">
            <option value="all">All risk grades</option>
            {['A','B','C','D','E'].map((item) => <option key={item} value={item}>Risk {item}</option>)}
          </select>
          <select value={industry} onChange={(event) => setIndustry(event.target.value)} aria-label="Filter by industry">
            <option value="all">All industries</option>
            {INDUSTRIES.map((item) => <option key={item} value={item}>{prettyIndustry(item)}</option>)}
          </select>
          <select value={sortBy} onChange={(event) => setSortBy(event.target.value as SortOption)} aria-label="Sort marketplace">
            <option value="newest">Newest</option><option value="funding">Most funded</option><option value="riskLow">Lowest risk</option><option value="targetLow">Lowest target</option><option value="tenorShort">Shortest tenor</option>
          </select>
        </div>
        <div className={styles.rangeRow}>
          <input type="number" min="0" step="0.01" placeholder="Min amount (R)" value={minAmount} onChange={(e) => setMinAmount(e.target.value)} aria-label="Minimum amount" />
          <input type="number" min="0" step="0.01" placeholder="Max amount (R)" value={maxAmount} onChange={(e) => setMaxAmount(e.target.value)} aria-label="Maximum amount" />
          <input type="number" min="0" step="1" placeholder="Min tenor (days)" value={minTenor} onChange={(e) => setMinTenor(e.target.value)} aria-label="Minimum tenor" />
          <input type="number" min="0" step="1" placeholder="Max tenor (days)" value={maxTenor} onChange={(e) => setMaxTenor(e.target.value)} aria-label="Maximum tenor" />
          <button type="button" className={styles.applyButton} onClick={applyFilters} disabled={isLoading}>Apply filters</button>
        </div>
        <div className={styles.toolbarMeta}>
          <span>{visibleListings.length} visible opportunities</span>
          {hasFilters && <button type="button" className={styles.clearButton} onClick={resetFilters}>Clear filters</button>}
        </div>
      </section>

      {isLoading ? <p className={styles.status}>Loading opportunities...</p> : error ? (
        <div className={styles.status}><p>{error}</p><button type="button" onClick={() => void loadListings()}>Try again</button></div>
      ) : listings.length === 0 ? <p className={styles.status}>No listings match the selected filters.</p> : visibleListings.length === 0 ? (
        <div className={styles.status}><h3>No matching opportunities</h3><p>Try changing your search or filters.</p></div>
      ) : (
        <div className={styles.grid}>{visibleListings.map((listing) => <ListingCard key={listing.listingId} listing={listing} onOpen={setSelected} />)}</div>
      )}
      <FundingModal listing={selected} onClose={() => setSelected(null)} onSuccess={() => void loadListings()} />
    </main>
  );
}
