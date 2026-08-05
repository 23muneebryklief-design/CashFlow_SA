import styles from "./PasswordStrength.module.css";

interface PasswordStrengthProps {
  password: string;
}

export default function PasswordStrength({
  password,
}: PasswordStrengthProps) {
  let strength = 0;

  if (password.length >= 8) strength++;

  if (/[A-Z]/.test(password)) strength++;

  if (/[0-9]/.test(password)) strength++;

  if (/[^A-Za-z0-9]/.test(password)) strength++;

  const labels = [
    "Very Weak",
    "Weak",
    "Fair",
    "Strong",
    "Excellent",
  ];

  return (
    <div className={styles.container}>
      <div className={styles.bar}>
        <div
          className={styles.fill}
          style={{
            width: `${strength * 25}%`,
          }}
        />
      </div>

      <span>{labels[strength]}</span>
    </div>
  );
}