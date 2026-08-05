import styles from './Footer.module.css';

export default function Footer() {
  return (
    <footer className={styles.footer}>
      <div className="wrap">
        <div className={styles.inner}>
          <div className={styles.logo}>
            <div className={styles.logoMark}></div>
            CashFlow SA
          </div>

          <div className={styles.links}>
            <a href="#how">How it works</a>
            <a href="#deals">For SMEs</a>
            <a href="#security">For investors</a>
            <a href="#security">Security</a>
          </div>

          <div className={styles.copy}>
             2026 CashFlow SA · Cape Town, South Africa
          </div>
        </div>
      </div>
    </footer>
  );
}