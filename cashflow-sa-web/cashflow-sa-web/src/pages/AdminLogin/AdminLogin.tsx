import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { useAuth } from "../../Hooks/useAuth";

import MoneyRainBackground from "../../components/MoneyRain/MoneyRainBackground";
import AmbientGlow from "../../components/AmbientGlow/AmbientGlow";
import FadeIn from "../../components/FadeIn/FadeIn";

import useMouseParallax from "../../Hooks/useMouseParallax";

import {
  AuthCard,
  AuthHeader,
  AuthInput,
  AuthButton,
} from "../../components/Auth/Ui";

import styles from "./AdminLogin.module.css";

// title(0) -> subtitle(70) -> email(140) -> password(210) -> button(280)
const STAGGER_STEP = 70;

export default function AdminLogin() {
  const navigate = useNavigate();
  const { login, logout } = useAuth();

  const { ref } = useMouseParallax();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError("");

    try {
      setLoading(true);

      const user = await login({ email, password });

      if (user.role !== "Admin" && user.role !== "SuperAdmin") {
        logout();
        setError("This portal is for administrators only.");
        return;
      }

      navigate("/admin-dashboard");
    } catch {
      setError("Invalid email or password.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <MoneyRainBackground />
      <AmbientGlow />

      <main className={styles.page}>
        <div
          ref={ref}
          className={styles.cardWrapper}
        >
          <AuthCard>
            <AuthHeader
              title="Admin sign in"
              subtitle="Restricted access. Authorized administrators only."
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
          </AuthCard>
        </div>
      </main>
    </>
  );
}