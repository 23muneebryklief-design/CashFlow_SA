/**
 * Represents a single animated money note.
 */
export interface MoneyNote {
  /** Current position */
  x: number;
  y: number;

  /** Size */
  width: number;
  height: number;

  /** Current velocity */
  velocityX: number;
  velocityY: number;

  /** Rotation */
  rotation: number;
  rotationSpeed: number;

  /** Gentle left/right sway */
  swayAmplitude: number;
  swaySpeed: number;
  swayPhase: number;

  /** Vertical falling speed */
  fallSpeed: number;

  /** Transparency */
  opacity: number;

  /** Colour used to draw the note */
  color: string;

  /**
   * Fake depth value.
   * Used later for parallax and blur effects.
   */
  depth: number;
}

/**
 * Tracks the user's mouse.
 */
export interface MouseState {
  x: number;
  y: number;
  active: boolean;
}

/**
 * Canvas dimensions.
 */
export interface CanvasSize {
  width: number;
  height: number;
}