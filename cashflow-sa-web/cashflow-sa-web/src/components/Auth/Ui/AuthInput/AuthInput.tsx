import { useState } from "react";
import styles from "./AuthInput.module.css";

interface AuthInputProps {
  label: string;
  type?: "text" | "email" | "password";
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  required?: boolean;
  error?: string;
  disabled?: boolean;
  autoComplete?: string;
}

export default function AuthInput({
  label,
  type = "text",
  value,
  onChange,
  placeholder,
  required = false,
  error,
  disabled = false,
  autoComplete,
}: AuthInputProps) {
  const [showPassword, setShowPassword] = useState(false);

  const inputType =
    type === "password"
      ? showPassword
        ? "text"
        : "password"
      : type;

  return (
    <div className={styles.container}>
      <label className={styles.label}>
        {label}

        {required && <span>*</span>}
      </label>

      <div
        className={`${styles.inputWrapper} ${
          error ? styles.errorBorder : ""
        }`}
      >
        <input
          type={inputType}
          value={value}
          disabled={disabled}
          autoComplete={autoComplete}
          placeholder={placeholder}
          onChange={(e) => onChange(e.target.value)}
        />

        {type === "password" && (
          <button
            type="button"
            className={styles.toggle}
            onClick={() => setShowPassword(!showPassword)}
          >
            {showPassword ? "Hide" : "Show"}
          </button>
        )}
      </div>

      {error && <small>{error}</small>}
    </div>
  );
}