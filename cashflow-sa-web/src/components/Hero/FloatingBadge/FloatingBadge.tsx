import styles from "./FloatingBadge.module.css";

interface FloatingBadgeProps {
  text: string;
  value: string;
  position: "top" | "bottom";
}

export default function FloatingBadge({
  text,
  value,
  position,
}: FloatingBadgeProps) {
  return (
    <div
      className={`${styles.badge} ${
        position === "top" ? styles.top : styles.bottom
      }`}
    >
      <span className={styles.dot}></span>

      <span>
        {text} <strong>{value}</strong>
      </span>
    </div>
  );
}