import styles from "./Ticker.module.css";

const tickerItems = [
  { label: "Total funded to date", value: "R 214.6M" },
  { label: "Avg. time to funding", value: "1.8 days" },
  { label: "Active investors", value: "1,240" },
  { label: "Avg. investor return", value: "↑ 14.2% p.a.", highlight: true },
  { label: "SMEs funded", value: "3,096" },
];

export default function Ticker() {
  const items = [...tickerItems, ...tickerItems];

  return (
    <section className={styles.ticker}>
      <div className={styles.track}>
        {items.map((item, index) => (
          <div key={index} className={styles.item}>
            <span>{item.label}</span>

            <strong className={item.highlight ? styles.highlight : ""}>
              {item.value}
            </strong>
          </div>
        ))}
      </div>
    </section>
  );
}