import styles from "./HowItWorks.module.css";
import WorkflowCard from "./WorkflowCard/WorkflowCard";

const smeSteps = [
  {
    title: "Submit your invoice",
    description:
      "Upload an approved invoice and basic business documents — takes about 10 minutes.",
  },
  {
    title: "Get verified",
    description:
      "Our risk team confirms the debtor and sets a fair advance rate, usually within a day.",
  },
  {
    title: "Receive funds",
    description:
      "Once investors commit, capital lands in your account — often within 48 hours.",
  },
];

const investorSteps = [
  {
    title: "Browse live invoices",
    description:
      "Review verified invoices with risk grading, debtor history, and expected return.",
  },
  {
    title: "Fund in full or fractionally",
    description:
      "Commit any amount from R500, alone or alongside other investors.",
  },
  {
    title: "Get repaid with returns",
    description:
      "Once the debtor pays, your principal and return settle automatically.",
  },
];

export default function HowItWorks() {
  return (
    <section className={styles.section} id="how">
      <div className="wrap">
        <div className={styles.heading}>
          <span>How it works</span>

          <h2>Two sides of one ledger</h2>

          <p>
            Every funded invoice is a match between an SME that needs capital
            now and an investor looking for short-term, asset-backed returns.
          </p>
        </div>

        <div className={styles.grid}>
          <WorkflowCard
            title="For SMEs"
            tag="Get Funded"
            steps={smeSteps}
          />

          <WorkflowCard
            title="For Investors"
            tag="Earn Returns"
            investor
            steps={investorSteps}
          />
        </div>
      </div>
    </section>
  );
}