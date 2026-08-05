import { type ReactNode } from "react";
import styles from "./AuthShell.module.css";

interface AuthShellProps {
  panel: ReactNode;
  children: ReactNode;
  variant?: "balanced" | "narrow-panel";
}

export default function AuthShell({
  panel,
  children,
  variant = "balanced",
}: AuthShellProps) {
  return (
    <main className={styles.page}>
      <div
        className={styles.shell}
        data-variant={variant}
      >
        {panel}

        <div className={styles.formPane}>{children}</div>
      </div>
    </main>
  );
}
