import styles from "./AuthCard.module.css";
import { type ReactNode } from "react";

interface Props {
  children: ReactNode;
}

export default function AuthCard({ children }: Props) {
  return <div className={styles.card}>{children}</div>;
}