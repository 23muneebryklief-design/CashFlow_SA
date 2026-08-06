import styles from "./SecurityCard.module.css";
import { type ReactNode } from "react";

interface SecurityCardProps {
  icon: ReactNode;
  title: string;
  description: string;
}

export default function SecurityCard({
  icon,
  title,
  description,
}: SecurityCardProps) {
  return (
    <div className={styles.card}>
      <div className={styles.icon}>{icon}</div>

      <h4>{title}</h4>

      <p>{description}</p>
    </div>
  );
}