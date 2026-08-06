import { type ReactNode } from "react";
import styles from "./AuthPanel.module.css";

interface AuthPanelProps {
  eyebrow?: string;
  title: ReactNode;
  subtitle: string;
  children?: ReactNode;
}

export default function AuthPanel({
  eyebrow,
  title,
  subtitle,
  children,
}: AuthPanelProps) {
  return (
    <div className={styles.panel}>
      <div className={styles.logo}>
        <div className={styles.logoMark} />
        CashFlow SA
      </div>

      {eyebrow && <div className={styles.eyebrow}>{eyebrow}</div>}

      <h2 className={styles.title}>{title}</h2>
      <p className={styles.subtitle}>{subtitle}</p>

      {children && <div className={styles.extra}>{children}</div>}
    </div>
  );
}
