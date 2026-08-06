import styles from "./AuthSelect.module.css";

interface Option {
  value: string;
  label: string;
}

interface AuthSelectProps {
  label: string;
  value: string;
  options: Option[];
  onChange: (value: string) => void;
  required?: boolean;
  disabled?: boolean;
  error?: string;
}

export default function AuthSelect({
  label,
  value,
  options,
  onChange,
  required = false,
  disabled = false,
  error,
}: AuthSelectProps) {
  return (
    <div className={styles.container}>
      <label className={styles.label}>
        {label}
        {required && <span>*</span>}
      </label>

      <div
        className={`${styles.selectWrapper} ${
          error ? styles.errorBorder : ""
        }`}
      >
        <select
          value={value}
          disabled={disabled}
          onChange={(e) => onChange(e.target.value)}
        >
          <option value="">Select...</option>

          {options.map((option) => (
            <option
              key={option.value}
              value={option.value}
            >
              {option.label}
            </option>
          ))}
        </select>
      </div>

      {error && <small>{error}</small>}
    </div>
  );
}