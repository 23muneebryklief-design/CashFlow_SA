import styles from "./AuthBrandPanel.module.css";

interface BrandBullet {
  text: string;
  accent?: "emerald" | "gold";
}

interface AuthBrandPanelProps {
  title: string;
  highlight?: string;
  subtitle: string;
  bullets?: BrandBullet[];
  badge?: string;
}

export default function AuthBrandPanel({
  title,
  highlight,
  subtitle,
  bullets,
  badge,
}: AuthBrandPanelProps) {
  return (
    <div className={styles.panel}>
      <div className={styles.logoMark}>
        <span className={styles.logoLineEmerald} />
        <span className={styles.logoLineGold} />
      </div>

      <h2 className={styles.title}>
        {title} {highlight && <em>{highlight}</em>}
      </h2>

      <p className={styles.subtitle}>{subtitle}</p>

      {bullets && bullets.length > 0 && (
        <ul className={styles.bulletList}>
          {bullets.map((bullet) => (
            <li key={bullet.text}>
              <span
                className={`${styles.dot} ${
                  bullet.accent === "gold" ? styles.dotGold : styles.dotEmerald
                }`}
              />
              {bullet.text}
            </li>
          ))}
        </ul>
      )}

      {badge && (
        <div className={styles.badge}>
          <span>{badge}</span>
        </div>
      )}
    </div>
  );
}
