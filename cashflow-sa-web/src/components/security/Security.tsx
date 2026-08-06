import styles from "./Security.module.css";
import SecurityCard from "./SecurityCard/SecurityCard";

const cards = [
  {
    title: "Secure authentication",
    description:
      "User passwords are securely hashed using industry-standard cryptographic techniques, providing a strong foundation for account security.",
    icon: (
      <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M12 2l8 4v6c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10V6l8-4z"/>
      </svg>
    ),
  },
  {
    title: "Privacy-focused design",
    description:
      "The platform is designed with data privacy in mind. Features such as document encryption, retention policies, and compliance workflows are planned for future development.",
    icon: (
      <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <rect x="4" y="10" width="16" height="10" rx="1.5"/>
        <path d="M8 10V7a4 4 0 018 0v3"/>
      </svg>
    ),
  },
  {
    title: "Verification-ready architecture",
    description:
      "Invoice verification and risk assessment are planned as future capabilities. The current application models the financing workflow while providing a foundation for automated validation services.",
    icon: (
      <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <path d="M3 12h4l2-8 4 16 2-8h6"/>
      </svg>
    ),
  },
  {
    title: "Simulated funding wallet",
    description:
      "Investor balances and funding transactions are simulated within the application to demonstrate marketplace functionality. This is a development feature and does not represent a real escrow or banking service.",
    icon: (
      <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
        <rect x="3" y="6" width="18" height="12" rx="2"/>
        <path d="M3 10h18"/>
      </svg>
    ),
  },
];

export default function Security() {
  return (
    <section className={styles.section} id="security">
      <div className="wrap">
        <div className={styles.heading}>
          <span>Trust &amp; Security</span>

          <h2>Built with secure foundations</h2>

          <p>
            CashFlow SA is a portfolio project exploring how a digital invoice
            financing marketplace could work. The platform focuses on secure
            authentication, realistic funding workflows, and an architecture
            designed to support future fintech features.
          </p>
        </div>

        <div className={styles.grid}>
          {cards.map((card) => (
            <SecurityCard
              key={card.title}
              icon={card.icon}
              title={card.title}
              description={card.description}
            />
          ))}
        </div>
      </div>
    </section>
  );
}