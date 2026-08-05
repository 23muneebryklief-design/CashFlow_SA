import styles from "./AuthButton.module.css";

interface AuthButtonProps {
  text: string;
  type?: "button" | "submit";
  loading?: boolean;
  disabled?: boolean;
  onClick?: () => void;
}

export default function AuthButton({
  text,
  type = "button",
  loading = false,
  disabled = false,
  onClick,
}: AuthButtonProps) {
  return (
    <button
      type={type}
      className={styles.button}
      disabled={disabled || loading}
      onClick={onClick}
    >
      <span className={styles.text}>
        {loading ? "Please wait..." : text}
      </span>

      <span className={styles.shine} />
    </button>
  );
}