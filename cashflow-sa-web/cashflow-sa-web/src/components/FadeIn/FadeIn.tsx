import { type ReactNode } from "react";
import styles from "./FadaeIn.module.css";

interface FadeInProps {
  children: ReactNode;
  delay?: number;
  /**
   * "default" – the original 700ms/20px entrance, for big container-level
   * reveals (panels, cards).
   * "fast" – a quicker 450ms/10px entrance, meant to be stacked in a
   * sequence (e.g. every ~70ms) for staggered field-by-field reveals.
   */
  variant?: "default" | "fast";
}

export default function FadeIn({
  children,
  delay = 0,
  variant = "default",
}: FadeInProps) {
  return (
    <div
      className={variant === "fast" ? styles.fadeFast : styles.fade}
      style={{
        animationDelay: `${delay}ms`,
      }}
    >
      {children}
    </div>
  );
}