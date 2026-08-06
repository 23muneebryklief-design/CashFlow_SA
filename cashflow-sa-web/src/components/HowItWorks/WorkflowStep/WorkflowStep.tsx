import styles from "./WorkflowStep.module.css";

interface WorkflowStepProps {
  number: number;
  title: string;
  description: string;
  investor?: boolean;
}

export default function WorkflowStep({
  number,
  title,
  description,
  investor = false,
}: WorkflowStepProps) {
  return (
    <div className={styles.step}>
      <div
        className={`${styles.number} ${
          investor ? styles.investor : styles.sme
        }`}
      >
        {number}
      </div>

      <div className={styles.content}>
        <h4>{title}</h4>
        <p>{description}</p>
      </div>
    </div>
  );
}