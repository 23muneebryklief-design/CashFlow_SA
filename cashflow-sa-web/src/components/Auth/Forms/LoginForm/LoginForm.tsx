import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../../../Hooks/useAuth";
import {
  AuthCard,
  AuthHeader,
  AuthInput,
  AuthButton,
} from "../../Ui";
import FadeIn from "../../../FadeIn/FadeIn";

import styles from "./LoginForm.module.css";

// Entrance stagger: title(0) -> subtitle(70) -> email(140) -> password(210)
// -> button(280) -> footer link(350), ~70ms apart per Phase 3.5.
const STAGGER_STEP = 70;

export default function LoginForm() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();

    setError("");

    try {
      setLoading(true);

      const user = await login({
        email,
        password,
      });

      if (user.role === "SME") {
        navigate("/sme-dashboard");
      } else if (user.role === "Investor") {
        navigate("/investor-marketplace");
      } else {
        navigate("/");
      }
    } catch {
      setError("Invalid email or password.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthCard>
      <AuthHeader
        title="Welcome Back"
        subtitle="Sign in to your CashFlowSA account."
      />

      <form
        className={styles.form}
        onSubmit={handleSubmit}
      >
        <FadeIn variant="fast" delay={STAGGER_STEP * 2}>
          <AuthInput
            label="Email"
            type="email"
            value={email}
            onChange={setEmail}
            placeholder="Enter your email"
            autoComplete="email"
            required
          />
        </FadeIn>

        <FadeIn variant="fast" delay={STAGGER_STEP * 3}>
          <AuthInput
            label="Password"
            type="password"
            value={password}
            onChange={setPassword}
            placeholder="Enter your password"
            autoComplete="current-password"
            required
          />
        </FadeIn>

        {error && (
          <p className={styles.error}>
            {error}
          </p>
        )}

        <FadeIn variant="fast" delay={STAGGER_STEP * 4}>
          <AuthButton
            text="Sign In"
            type="submit"
            loading={loading}
          />
        </FadeIn>
      </form>

      <FadeIn variant="fast" delay={STAGGER_STEP * 5}>
        <p className={styles.footer}>
          Don't have an account?{" "}
          <Link to="/register">
            Create one
          </Link>
        </p>
      </FadeIn>
    </AuthCard>
  );
}