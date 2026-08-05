import styles from "./WorkflowCard.module.css";
import WorkflowStep from "../WorkflowStep/WorkflowStep";

interface Step {
  title: string;
  description: string;
}

interface WorkflowCardProps {
  title: string;
  tag: string;
  investor?: boolean;
  steps: Step[];
}

export default function WorkflowCard({
  title,
  tag,
  investor = false,
  steps,
}: WorkflowCardProps) {
  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <h3>{title}</h3>

        <span
          className={`${styles.tag} ${
            investor ? styles.gold : styles.green
          }`}
        >
          {tag}
        </span>
      </div>

      {steps.map((step, index) => (
        <WorkflowStep
          key={index}
          number={index + 1}
          title={step.title}
          description={step.description}
          investor={investor}
        />
      ))}
    </div>
  );
}