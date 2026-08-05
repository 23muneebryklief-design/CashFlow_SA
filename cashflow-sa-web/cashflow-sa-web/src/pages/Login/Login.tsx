import MoneyRainBackground from "../../components/MoneyRain/MoneyRainBackground";
import AmbientGlow from "../../components/AmbientGlow/AmbientGlow";
import FadeIn from "../../components/FadeIn/FadeIn";

import useMouseParallax from "../../Hooks/useMouseParallax";

import LoginForm from "../../components/Auth/Forms/LoginForm/LoginForm";
import { AuthBrandPanel } from "../../components/Auth/Ui";

import styles from "./Login.module.css";

export default function Login() {
  const { ref } = useMouseParallax();

  return (
    <>
      <MoneyRainBackground />
      <AmbientGlow />

      <main className={styles.page}>
        <div
          ref={ref}
          className={styles.splitCard}
        >
          <FadeIn delay={100}>
            <AuthBrandPanel
              title="Funding South African business,"
              highlight="one invoice at a time."
              subtitle="Join verified SMEs and investors already trading on CashFlowSA."
              badge="R2.4M+ funded to date"
            />
          </FadeIn>

          <div className={styles.formSide}>
            {/* LoginForm stages its own field-by-field entrance (Phase 3.5) */}
            <LoginForm />
          </div>
        </div>
      </main>
    </>
  );
}