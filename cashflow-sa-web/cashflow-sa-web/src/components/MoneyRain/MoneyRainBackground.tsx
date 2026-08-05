import { useEffect, useRef } from "react";
import styles from "./MoneyRainBackground.module.css";

import type { MoneyNote, MouseState } from "./types";

import {
  createMoneyNote,
  recycleMoneyNote,
  getParticleCount,
  distance,
} from "./utils";

export default function MoneyRainBackground() {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const animationFrameRef = useRef<number | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;

    if (canvas === null) {
      return;
    }

    const ctx = canvas.getContext("2d");

    if (ctx === null) {
      return;
    }

    const canvasElement: HTMLCanvasElement = canvas;
    const context: CanvasRenderingContext2D = ctx;

    let width = window.innerWidth;
    let height = window.innerHeight;

    canvasElement.width = width;
    canvasElement.height = height;

    const mouse: MouseState = {
      x: -9999,
      y: -9999,
      active: false,
    };

    let notes: MoneyNote[] = [];

    function buildNotes() {
      notes = [];

      const count = getParticleCount(width);

      for (let i = 0; i < count; i++) {
        notes.push(createMoneyNote(width, height));
      }
    }

    buildNotes();

    function resizeCanvas() {
      width = window.innerWidth;
      height = window.innerHeight;

      canvasElement.width = width;
      canvasElement.height = height;

      buildNotes();
    }

    window.addEventListener("resize", resizeCanvas);

    function handlePointerMove(event: PointerEvent) {
      const rect = canvasElement.getBoundingClientRect();

      mouse.x = event.clientX - rect.left;
      mouse.y = event.clientY - rect.top;
      mouse.active = true;
    }

    function handlePointerLeave() {
      mouse.active = false;
    }

    function handlePointerDown(event: PointerEvent) {
      const rect = canvasElement.getBoundingClientRect();

      const clickX = event.clientX - rect.left;
      const clickY = event.clientY - rect.top;

      for (const note of notes) {
        const d = distance(
          note.x,
          note.y,
          clickX,
          clickY
        );

        if (d > 180) continue;

        const dx = note.x - clickX;
        const dy = note.y - clickY;

        const force = (1 - d / 180) * 7;

        note.velocityX += (dx / (d || 1)) * force;
        note.velocityY += (dy / (d || 1)) * force;
      }
    }

    canvasElement.addEventListener(
      "pointermove",
      handlePointerMove
    );

    canvasElement.addEventListener(
      "pointerleave",
      handlePointerLeave
    );

    canvasElement.addEventListener(
      "pointerdown",
      handlePointerDown
    );

    function drawNote(note: MoneyNote) {
      context.save();

      context.translate(note.x, note.y);
      context.rotate(note.rotation);
      context.globalAlpha = note.opacity;

      const w = note.width;
      const h = note.height;
      const radius = h * 0.2;

      // Rounded note shape
      context.beginPath();

      context.moveTo(-w / 2 + radius, -h / 2);

      context.arcTo(
        w / 2,
        -h / 2,
        w / 2,
        h / 2,
        radius
      );

      context.arcTo(
        w / 2,
        h / 2,
        -w / 2,
        h / 2,
        radius
      );

      context.arcTo(
        -w / 2,
        h / 2,
        -w / 2,
        -h / 2,
        radius
      );

      context.arcTo(
        -w / 2,
        -h / 2,
        w / 2,
        -h / 2,
        radius
      );

      context.closePath();

      // Premium gradient fill
      const gradient = context.createLinearGradient(
        -w / 2,
        -h / 2,
        w / 2,
        h / 2
      );

      gradient.addColorStop(0, note.color);
      gradient.addColorStop(1, "#06121E");

      context.fillStyle = gradient;
      context.fill();

      // White outline
      context.strokeStyle = "rgba(255,255,255,.45)";
      context.lineWidth = 1;
      context.stroke();
            // Decorative centre stripe
      context.beginPath();

      context.moveTo(-w * 0.35, 0);
      context.lineTo(w * 0.35, 0);

      context.strokeStyle = "rgba(255,255,255,.18)";
      context.lineWidth = 2;
      context.stroke();

      // Centre emblem
      context.beginPath();
      context.arc(
        0,
        0,
        h * 0.18,
        0,
        Math.PI * 2
      );

      context.fillStyle = "rgba(255,255,255,.22)";
      context.fill();

      context.fillStyle = "rgba(255,255,255,.9)";
      context.font = `bold ${h * 0.36}px Arial`;
      context.textAlign = "center";
      context.textBaseline = "middle";
      context.fillText("R", 0, 1);

      // Decorative corner dots
      const dot = h * 0.08;

      context.fillStyle = "rgba(255,255,255,.18)";

      context.beginPath();
      context.arc(
        -w * 0.32,
        -h * 0.18,
        dot,
        0,
        Math.PI * 2
      );
      context.fill();

      context.beginPath();
      context.arc(
        w * 0.32,
        h * 0.18,
        dot,
        0,
        Math.PI * 2
      );
      context.fill();

      context.restore();
    }

    let lastFrameTime = performance.now();

    function animate(now: number) {
      const deltaTime = Math.min(
        (now - lastFrameTime) / 1000,
        0.05
      );

      lastFrameTime = now;

      context.clearRect(0, 0, width, height);

      for (const note of notes) {
        // Gentle side-to-side movement
        note.swayPhase += note.swaySpeed * deltaTime;

        note.x +=
          Math.sin(note.swayPhase) *
          note.swayAmplitude *
          deltaTime;

        // Falling
        note.y += note.fallSpeed * deltaTime;

        // Rotation
        note.rotation +=
          note.rotationSpeed * deltaTime;

        // Apply click velocity
        note.x += note.velocityX;
        note.y += note.velocityY;

        // Slow down after click
        note.velocityX *= 0.96;
        note.velocityY *= 0.96;

        // Mouse repulsion
        if (mouse.active) {
          const d = distance(
            note.x,
            note.y,
            mouse.x,
            mouse.y
          );

          if (d < 140) {
            const dx = note.x - mouse.x;
            const dy = note.y - mouse.y;

            const force = (140 - d) / 140;

            note.x +=
              (dx / (d || 1)) * force * 2;

            note.y +=
              (dy / (d || 1)) * force * 2;
          }
        }

        // Recycle note when off screen
        if (note.y > height + note.height * 2) {
          recycleMoneyNote(note, width);
        }

        drawNote(note);
      }

      animationFrameRef.current =
        requestAnimationFrame(animate);
    }

    animationFrameRef.current =
      requestAnimationFrame(animate);

    return () => {
      if (animationFrameRef.current !== null) {
        cancelAnimationFrame(
          animationFrameRef.current
        );
      }

      window.removeEventListener(
        "resize",
        resizeCanvas
      );

      canvasElement.removeEventListener(
        "pointermove",
        handlePointerMove
      );

      canvasElement.removeEventListener(
        "pointerleave",
        handlePointerLeave
      );

      canvasElement.removeEventListener(
        "pointerdown",
        handlePointerDown
      );
    };
  }, []);

  return (
    <div className={styles.wrapper}>
      <canvas
        ref={canvasRef}
        className={styles.canvas}
      />
    </div>
  );
}