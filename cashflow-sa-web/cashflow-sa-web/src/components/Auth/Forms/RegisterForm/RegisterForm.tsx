import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import {
  AuthCard,
  AuthHeader,
  AuthInput,
  AuthButton,
  PasswordStrength,
  RoleSelector,
  type UserRole,
} from "../../Ui";

import InvestorFields from "../../Fields/InvestorFields/InvestorFields";
import SMEFields from "../../Fields/SMEFields/SMEFields";

import {
  registerInvestor,
  registerSme,
} from "../../../../Services/authService";

import FadeIn from "../../../FadeIn/FadeIn";

import styles from "./RegisterForm.module.css";

// Register has ~13 fields, so we don't stagger every input individually
// (that would push the last field out past ~1s and feel sluggish). Instead:
// title(0) -> subtitle(70) -> role selector(140) -> field grid(210) -> button(280) -> footer(350).
const STAGGER_STEP = 70;

export default function RegisterForm() {
  const navigate = useNavigate();

  const [role, setRole] = useState<UserRole>("Investor");

  // Personal Information
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [address, setAddress] = useState("");

  // Security
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  // Investor
  const [riskAppetite, setRiskAppetite] = useState("");

  // SME
  const [companyName, setCompanyName] = useState("");
  const [contactPerson, setContactPerson] = useState("");
  const [companyEmail, setCompanyEmail] = useState("");
  const [companyPhoneNumber, setCompanyPhoneNumber] = useState("");
  const [registrationNumber, setRegistrationNumber] = useState("");
  const [taxNumber, setTaxNumber] = useState("");
  const [industry, setIndustry] = useState("");

  // UI
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [serverError, setServerError] = useState("");
  const [loading, setLoading] = useState(false);

  function validate() {
    const validationErrors: Record<string, string> = {};

    if (!firstName.trim())
      validationErrors.firstName = "First name is required.";

    if (!lastName.trim())
      validationErrors.lastName = "Last name is required.";

    if (!email.trim())
      validationErrors.email = "Email is required.";

    if (!phoneNumber.trim())
      validationErrors.phoneNumber = "Phone number is required.";

    if (!address.trim())
      validationErrors.address = "Address is required.";

    if (!password)
      validationErrors.password = "Password is required.";

    if (!confirmPassword)
      validationErrors.confirmPassword =
        "Please confirm your password.";

    if (
      password &&
      confirmPassword &&
      password !== confirmPassword
    ) {
      validationErrors.confirmPassword =
        "Passwords do not match.";
    }

    if (role === "Investor") {
      if (!riskAppetite) {
        validationErrors.riskAppetite =
          "Please choose your risk appetite.";
      }
    }

    if (role === "SME") {
      if (!companyName.trim())
        validationErrors.companyName =
          "Company name is required.";

      if (!contactPerson.trim())
        validationErrors.contactPerson =
          "Contact person is required.";

      if (!companyEmail.trim())
        validationErrors.companyEmail =
          "Company email is required.";

      if (!companyPhoneNumber.trim())
        validationErrors.companyPhoneNumber =
          "Company phone number is required.";

      if (!registrationNumber.trim())
        validationErrors.registrationNumber =
          "Registration number is required.";

      if (!taxNumber.trim())
        validationErrors.taxNumber =
          "Tax number is required.";

      if (!industry.trim())
        validationErrors.industry =
          "Industry is required.";
    }

    setErrors(validationErrors);

    return Object.keys(validationErrors).length === 0;
  }

  async function handleSubmit(
    e: React.FormEvent<HTMLFormElement>
  ) {
    e.preventDefault();

    setServerError("");

    if (!validate()) return;

    try {
      setLoading(true);

      if (role === "Investor") {
        await registerInvestor({
          firstName,
          lastName,
          email,
          phoneNumber,
          password,
          address,
          riskAppetite,
        });
      } else {
        await registerSme({
          firstName,
          lastName,
          email,
          phoneNumber,
          password,
          companyName,
          contactPerson,
          companyEmail,
          companyPhoneNumber,
          registrationNumber,
          taxNumber,
          address,
          industry,
        });
      }

      navigate("/login");
    } catch (error: any) {
      const data = error.response?.data;

      if (data?.errors) {
        // ASP.NET Core ValidationProblemDetails: { errors: { FieldName: ["message"] } }
        const fieldErrors: Record<string, string> = {};
        const messages: string[] = [];

        for (const [key, value] of Object.entries(
          data.errors as Record<string, string[] | string>
        )) {
          const message = Array.isArray(value) ? value[0] : String(value);
          messages.push(message);

          // Backend sends PascalCase field names; our state uses camelCase.
          const fieldKey = key.charAt(0).toLowerCase() + key.slice(1);
          fieldErrors[fieldKey] = message;
        }

        setErrors((prev) => ({ ...prev, ...fieldErrors }));
        setServerError(messages.join(" "));
      } else if (data?.detail) {
        setServerError(data.detail);
      } else if (data?.title) {
        setServerError(data.title);
      } else {
        setServerError(
          "Registration failed. Please try again."
        );
      }
    } finally {
      setLoading(false);
    }
  }

return (
  <AuthCard>
    <AuthHeader
      title="Create Your Account"
      subtitle="Join CashFlowSA and start your investment journey."
    />

    <form
      className={styles.form}
      onSubmit={handleSubmit}
    >
      <FadeIn variant="fast" delay={STAGGER_STEP * 2}>
        <RoleSelector
          selectedRole={role}
          onRoleChange={setRole}
        />
      </FadeIn>

      <FadeIn variant="fast" delay={STAGGER_STEP * 3}>
      <div className={styles.grid}>
        <AuthInput
          label="First Name"
          value={firstName}
          onChange={setFirstName}
          required
          error={errors.firstName}
        />

        <AuthInput
          label="Last Name"
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
          label="Phone Number"
          value={phoneNumber}
          onChange={setPhoneNumber}
          required
          error={errors.phoneNumber}
        />

        <AuthInput
          label="Address"
          value={address}
          onChange={setAddress}
          required
          error={errors.address}
        />

        <AuthInput
          label="Password"
          type="password"
          value={password}
          onChange={setPassword}
          required
          error={errors.password}
        />

        <PasswordStrength password={password} />

        <AuthInput
          label="Confirm Password"
          type="password"
          value={confirmPassword}
          onChange={setConfirmPassword}
          required
          error={errors.confirmPassword}
        />

        {role === "Investor" && (
          <InvestorFields
            riskAppetite={riskAppetite}
            setRiskAppetite={setRiskAppetite}
            errors={{
              riskAppetite: errors.riskAppetite,
            }}
          />
        )}

        {role === "SME" && (
          <SMEFields
            companyName={companyName}
            contactPerson={contactPerson}
            companyEmail={companyEmail}
            companyPhoneNumber={companyPhoneNumber}
            registrationNumber={registrationNumber}
            taxNumber={taxNumber}
            industry={industry}
            setCompanyName={setCompanyName}
            setContactPerson={setContactPerson}
            setCompanyEmail={setCompanyEmail}
            setCompanyPhoneNumber={setCompanyPhoneNumber}
            setRegistrationNumber={setRegistrationNumber}
            setTaxNumber={setTaxNumber}
            setIndustry={setIndustry}
            errors={{
              companyName: errors.companyName,
              contactPerson: errors.contactPerson,
              companyEmail: errors.companyEmail,
              companyPhoneNumber: errors.companyPhoneNumber,
              registrationNumber: errors.registrationNumber,
              taxNumber: errors.taxNumber,
              industry: errors.industry,
            }}
          />
        )}

        {serverError && (
          <p className={styles.serverError}>
            {serverError}
          </p>
        )}
      </div>
      </FadeIn>

      <FadeIn variant="fast" delay={STAGGER_STEP * 4}>
        <AuthButton
          type="submit"
          text="Create Account"
          loading={loading}
        />
      </FadeIn>

      <FadeIn variant="fast" delay={STAGGER_STEP * 5}>
        <p className={styles.footer}>
          Already have an account?{" "}
          <Link to="/login">
            Sign In
          </Link>
        </p>
      </FadeIn>
    </form>
  </AuthCard>
);
}