import styles from "./LoadingScreen.module.css";

interface LoadingScreenProps {
  message?: string;
  fullScreen?: boolean;
}

export default function LoadingScreen({ message = "Loading…", fullScreen = true }: LoadingScreenProps) {
  return (
    <div className={`${styles.container} ${fullScreen ? styles.fullScreen : styles.inline}`} role="status" aria-live="polite">
      <div className={styles.spinner} aria-hidden="true" />
      <span>{message}</span>
    </div>
  );
}
