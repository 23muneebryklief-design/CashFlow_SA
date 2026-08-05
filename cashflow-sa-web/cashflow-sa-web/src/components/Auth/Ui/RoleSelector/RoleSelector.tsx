import styles from "./RoleSelector.module.css";

export type UserRole = "Investor" | "SME";

interface RoleSelectorProps {
  selectedRole: UserRole;
  onRoleChange: (role: UserRole) => void;
}

export default function RoleSelector({
  selectedRole,
  onRoleChange,
}: RoleSelectorProps) {
  return (
    <div className={styles.container}>
      <label className={styles.label}>
        Account Type
      </label>

      <div className={styles.selector}>
        <button
          type="button"
          className={`${styles.option} ${
            selectedRole === "Investor"
              ? styles.active
              : ""
          }`}
          onClick={() => onRoleChange("Investor")}
        >
          Investor
        </button>

        <button
          type="button"
          className={`${styles.option} ${
            selectedRole === "SME"
              ? styles.active
              : ""
          }`}
          onClick={() => onRoleChange("SME")}
        >
          SME
        </button>
      </div>

      <div className={styles.description}>
        {selectedRole === "Investor" ? (
          <>
            <h4>Investor Account</h4>

            <p>
              Invest in verified South African SMEs and
              build your investment portfolio.
            </p>
          </>
        ) : (
          <>
            <h4>SME Account</h4>

            <p>
              Raise funding for your business and connect
              with verified investors.
            </p>
          </>
        )}
      </div>
    </div>
  );
}