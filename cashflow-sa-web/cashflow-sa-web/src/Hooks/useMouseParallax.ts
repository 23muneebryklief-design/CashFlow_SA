import { useEffect, useRef } from "react";

export default function useMouseParallax(strength = 8) {
  const ref = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const element = ref.current;

    if (!element) return;

    function handleMove(event: MouseEvent) {
      const el = ref.current;
      if (!el) return;

      const rect = el.getBoundingClientRect();

      const x = (event.clientX - rect.left) / rect.width;
      const y = (event.clientY - rect.top) / rect.height;

      const rotateY = (x - 0.5) * strength;
      const rotateX = (0.5 - y) * strength;

      el.style.transform = `
        perspective(1200px)
        rotateX(${rotateX}deg)
        rotateY(${rotateY}deg)
      `;
    }

    function reset() {
      const el = ref.current;
      if (!el) return;

      el.style.transform = `
        perspective(1200px)
        rotateX(0deg)
        rotateY(0deg)
      `;
    }

    element.addEventListener("mousemove", handleMove);
    element.addEventListener("mouseleave", reset);

    return () => {
      element.removeEventListener("mousemove", handleMove);
      element.removeEventListener("mouseleave", reset);
    };
  }, [strength]);

  return { ref };
}