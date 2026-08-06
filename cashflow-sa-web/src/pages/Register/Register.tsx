import MoneyRainBackground from "../../components/MoneyRain/MoneyRainBackground";
import AmbientGlow from "../../components/AmbientGlow/AmbientGlow";
import FadeIn from "../../components/FadeIn/FadeIn";

import useMouseParallax from "../../Hooks/useMouseParallax";

import RegisterForm from "../../components/Auth/Forms/RegisterForm/RegisterForm";
import { AuthBrandPanel } from "../../components/Auth/Ui";

import styles from "./Register.module.css";

export default function Register() {
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
              title="Join the"
              highlight="marketplace."
              subtitle="Whether you're raising capital or looking to invest, verification takes minutes."
              bullets={[
                { text: "Verified SMEs only", accent: "emerald" },
                { text: "Bank-level encryption", accent: "emerald" },
                { text: "R2.4M+ funded to date", accent: "gold" },
              ]}
            />
          </FadeIn>

          <div className={styles.formSide}>
            {/* RegisterForm stages its own entrance (Phase 3.5) */}
            <RegisterForm />
          </div>
        </div>
      </main>
    </>
  );
}