import { type MoneyNote } from "./types";

/**
 * CashFlow SA colour palette
 */
export const COLORS = [
  "#0B1B2B", // Navy
  "#0E7C5E", // Emerald
  "#B8902E", // Gold
  "#C7D4CF", // Silver
];

/**
 * Random number between min and max.
 */
export function random(min: number, max: number): number {
  return min + Math.random() * (max - min);
}

/**
 * Returns the number of money notes to render
 * based on the screen width.
 */
export function getParticleCount(width: number): number {
  if (width < 500) return 18;
  if (width < 768) return 30;
  if (width < 1200) return 45;
  return 60;
}

/**
 * Creates a new money note.
 */
export function createMoneyNote(
  canvasWidth: number,
  canvasHeight: number
): MoneyNote {
  const size = random(42, 60);

  return {
    x: random(0, canvasWidth),
    y: random(-canvasHeight, 0),

    width: size,
    height: size * 0.55,

    velocityX: 0,
    velocityY: 0,

    rotation: random(0, Math.PI * 2),
    rotationSpeed: random(-0.35, 0.35),

    swayAmplitude: random(10, 35),
    swaySpeed: random(0.4, 1),

    swayPhase: random(0, Math.PI * 2),

    // Slower, smoother fall
    fallSpeed: random(18, 40),

    // More visible
    opacity: random(0.30, 0.55),

    depth: random(0.7, 1.3),

    color: COLORS[
      Math.floor(Math.random() * COLORS.length)
    ],
  };
}
/**
 * Reset a note back to the top of the screen.
 */
export function recycleMoneyNote(
  note: MoneyNote,
  canvasWidth: number
): void {
  const size = random(42, 60);

  note.x = random(0, canvasWidth);
  note.y = -size * 2;

  note.width = size;
  note.height = size * 0.55;

  note.velocityX = 0;
  note.velocityY = 0;

  note.rotation = random(0, Math.PI * 2);
  note.rotationSpeed = random(-0.35, 0.35);

  note.swayAmplitude = random(10, 35);
  note.swaySpeed = random(0.4, 1);
  note.swayPhase = random(0, Math.PI * 2);

  // Slower, smoother fall
  note.fallSpeed = random(18, 40);

  // More visible
  note.opacity = random(0.30, 0.55);

  note.depth = random(0.7, 1.3);

  note.color =
    COLORS[Math.floor(Math.random() * COLORS.length)];
}

/**
 * Distance between two points.
 */
export function distance(
  x1: number,
  y1: number,
  x2: number,
  y2: number
): number {
  return Math.hypot(x2 - x1, y2 - y1);
}

/**
 * Clamp a value between min and max.
 */
export function clamp(
  value: number,
  min: number,
  max: number
): number {
  return Math.min(
    Math.max(value, min),
    max
  );
}