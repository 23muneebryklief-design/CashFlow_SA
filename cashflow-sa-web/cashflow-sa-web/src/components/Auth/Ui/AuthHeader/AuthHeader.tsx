import styles from "./AuthHeader.module.css";
import FadeIn from "../../../FadeIn/FadeIn";

interface AuthHeaderProps {
  title: string;
  subtitle: string;
  /** Delay (ms) at which the title appears; subtitle follows 70ms after. */
  startDelay?: number;
}

export default function AuthHeader({
  title,
  subtitle,
  startDelay = 0,
}: AuthHeaderProps) {
  return (
    <div className={styles.header}>
      <FadeIn variant="fast" delay={startDelay}>
        <h1>{title}</h1>
      </FadeIn>

      <FadeIn variant="fast" delay={startDelay + 70}>
        <p>{subtitle}</p>
      </FadeIn>
    </div>
  );
}