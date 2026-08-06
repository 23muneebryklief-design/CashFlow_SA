import { useEffect, useRef } from "react";
import styles from "./AmbientGlow.module.css";

export default function AmbientGlow() {
  const emeraldRef = useRef<HTMLDivElement>(null);
  const goldRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleMove(event: MouseEvent) {
      const x = event.clientX / window.innerWidth;
      const y = event.clientY / window.innerHeight;

      if (emeraldRef.current) {
        emeraldRef.current.style.transform = `
          translate(
            ${x * 40 - 20}px,
            ${y * 40 - 20}px
          )
        `;
      }

      if (goldRef.current) {
        goldRef.current.style.transform = `
          translate(
            ${(1 - x) * 30 - 15}px,
            ${(1 - y) * 30 - 15}px
          )
        `;
      }
    }

    window.addEventListener("mousemove", handleMove);

    return () => {
      window.removeEventListener("mousemove", handleMove);
    };
  }, []);

  return (
    <>
      <div
        ref={emeraldRef}
        className={`${styles.glow} ${styles.emerald}`}
      />

      <div
        ref={goldRef}
        className={`${styles.glow} ${styles.gold}`}
      />
    </>
  );
}