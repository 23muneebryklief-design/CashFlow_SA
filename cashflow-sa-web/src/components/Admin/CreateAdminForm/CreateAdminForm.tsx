import { useState } from "react";
import { AuthInput, AuthButton, AuthSelect } from "../../Auth/Ui";
import { createAdmin } from "../../../Services/adminService";
import styles from "./CreateAdminForm.module.css";

export default function CreateAdminForm() {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState("Admin");

  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setServerError("");
    setSuccessMessage("");
    setErrors({});

    try {
      setLoading(true);

      await createAdmin({ firstName, lastName, email, phoneNumber, password, role });

      setSuccessMessage(`${role} account created for ${email}.`);
      setFirstName("");
      setLastName("");
      setEmail("");
      setPhoneNumber("");
      setPassword("");
      setRole("Admin");
    } catch (error: any) {
      const data = error.response?.data;

      if (data?.errors) {
        const fieldErrors: Record<string, string> = {};
        const messages: string[] = [];

        for (const [key, value] of Object.entries(
          data.errors as Record<string, string[] | string>
        )) {
          const message = Array.isArray(value) ? value[0] : String(value);
          messages.push(message);
          const fieldKey = key.charAt(0).toLowerCase() + key.slice(1);
          fieldErrors[fieldKey] = message;
        }

        setErrors(fieldErrors);
        setServerError(messages.join(" "));
      } else if (data?.detail) {
        setServerError(data.detail);
      } else if (data?.title) {
        setServerError(data.title);
      } else {
        setServerError("Could not create admin account. Please try again.");
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <section className={styles.card}>
      <h2>Create admin account</h2>
      <p className={styles.subtitle}>
        Only your Super Admin account can create new admins.
      </p>

      <form className={styles.grid} onSubmit={handleSubmit}>
        <AuthInput
          label="First name"
          value={firstName}
          onChange={setFirstName}
          required
          error={errors.firstName}
        />
        <AuthInput
          label="Last name"
          value={lastName}
          onChange={setLastName}
          required
          error={errors.lastName}
        />
        <AuthInput
          label="Email"
          type="email"
          value={email}
          onChange={setEmail}
          required
          error={errors.email}
        />
        <AuthInput
          label="Phone number"
          value={phoneNumber}
          onChange={setPhoneNumber}
          required
          error={errors.phoneNumber}
        />
        <div className={styles.fullWidth}>
          <AuthSelect
            label="Role"
            value={role}
            onChange={setRole}
            required
            error={errors.role}
            options={[
              { value: "Admin", label: "Admin" },
              { value: "CreditAnalyst", label: "Credit Analyst" },
              { value: "Auditor", label: "Auditor" },
            ]}
          />
        </div>
        <div className={styles.fullWidth}>
          <AuthInput
            label="Temporary password"
            type="password"
            value={password}
            onChange={setPassword}
            required
            error={errors.password}
          />
        </div>

        {serverError && <p className={styles.error}>{serverError}</p>}
        {successMessage && <p className={styles.success}>{successMessage}</p>}

        <div className={styles.fullWidth}>
          <AuthButton text="Create admin" type="submit" loading={loading} />
        </div>
      </form>
    </section>
  );
}
