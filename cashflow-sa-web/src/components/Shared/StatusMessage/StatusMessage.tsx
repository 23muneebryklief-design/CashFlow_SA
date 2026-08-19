import styles from "./StatusMessage.module.css";
interface StatusMessageProps { title:string; message:string; actionLabel?:string; onAction?:()=>void; tone?:"error"|"empty"|"success"; }
export default function StatusMessage({title,message,actionLabel,onAction,tone="empty"}:StatusMessageProps){return <div className={`${styles.card} ${styles[tone]}`} role={tone === "error" ? "alert" : "status"}><h2>{title}</h2><p>{message}</p>{actionLabel&&onAction&&<button type="button" className={styles.button} onClick={onAction}>{actionLabel}</button>}</div>}
