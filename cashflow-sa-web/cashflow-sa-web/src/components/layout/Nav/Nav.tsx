import { Link } from 'react-router-dom';
import styles from './Nav.module.css';

export default function Nav() {
  return (
    <nav className={styles.nav}>
      <div className={styles.inner}>
        <div className={styles.logo}>
          <div className={styles.logoMark} />
          CashFlow SA
        </div>
        <div className={styles.links}>
          <a href="#how">How it works</a>
          <a href="#deals">Live invoices</a>
          <a href="#security">Security</a>
          <Link to="/login" className="btn btn-ghost">Log in</Link>
          <Link to="/register" className="btn btn-primary">Get started</Link>
        </div>
      </div>
    </nav>
  );
}