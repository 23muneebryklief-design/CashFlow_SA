import { useState } from "react";
import Modal from "../../Shared/Modal/Modal";
import { depositToWallet, withdrawFromWallet } from "../../../Services/walletService";
import styles from "./WalletActionModal.module.css";

interface WalletActionModalProps {
  isOpen: boolean;
  onClose: () => void;
  mode: "deposit" | "withdraw";
  userId: string;
  currentBalance: number;
  currency: string;
  onSuccess: () => void;
}

const COPY = {
  deposit: {
    title: "Add money",
    label: "Amount to add",
    action: "Add money",
    workingAction: "Processing payment...",
  },
  withdraw: {
    title: "Withdraw",
    label: "Amount to withdraw",
    action: "Withdraw",
    workingAction: "Processing withdrawal...",
  },
};

const SA_BANKS = [
  "ABSA",
  "Capitec",
  "FNB",
  "Nedbank",
  "Standard Bank",
  "Discovery Bank",
  "TymeBank",
  "Investec",
  "African Bank",
  "Bidvest Bank",
];

function isValidCardNumber(value: string) {
  const digitsOnly = value.replace(/\s+/g, "");
  return /^\d{13,19}$/.test(digitsOnly);
}

function isValidExpiryMonth(value: string) {
  return /^(0[1-9]|1[0-2])$/.test(value);
}

function isValidExpiryYear(value: string) {
  return /^\d{2}$|^\d{4}$/.test(value);
}

function isValidCvv(value: string) {
  return /^\d{3,4}$/.test(value);
}

function isValidAccountNumber(value: string) {
  return /^\d{6,11}$/.test(value);
}

function isValidBranchCode(value: string) {
  return /^\d{6}$/.test(value);
}

export default function WalletActionModal({
  isOpen,
  onClose,
  mode,
  userId,
  currentBalance,
  currency,
  onSuccess,
}: WalletActionModalProps) {
  const [amount, setAmount] = useState("");
  const [cardNumber, setCardNumber] = useState("");
  const [expiryMonth, setExpiryMonth] = useState("");
  const [expiryYear, setExpiryYear] = useState("");
  const [cvv, setCvv] = useState("");
  const [accountHolderName, setAccountHolderName] = useState("");
  const [bankName, setBankName] = useState("");
  const [accountNumber, setAccountNumber] = useState("");
  const [branchCode, setBranchCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const copy = COPY[mode];

  function resetForm() {
    setAmount("");
    setCardNumber("");
    setExpiryMonth("");
    setExpiryYear("");
    setCvv("");
    setAccountHolderName("");
    setBankName("");
    setAccountNumber("");
    setBranchCode("");
    setError(null);
  }

  function handleClose() {
    if (loading) return;

    resetForm();
    onClose();
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();

    const numericAmount = Number(amount);

    if (!amount || Number.isNaN(numericAmount) || numericAmount <= 0) {
      setError("Enter an amount greater than 0.");
      return;
    }

    if (mode === "withdraw" && numericAmount > currentBalance) {
      setError("You can't withdraw more than your wallet balance.");
      return;
    }

    if (mode === "deposit") {
      if (!isValidCardNumber(cardNumber)) {
        setError("Card number must be 13-19 digits.");
        return;
      }

      if (!isValidExpiryMonth(expiryMonth)) {
        setError("Expiry month must be between 01 and 12.");
        return;
      }

      if (!isValidExpiryYear(expiryYear)) {
        setError("Expiry year must be YY or YYYY.");
        return;
      }

      if (!isValidCvv(cvv)) {
        setError("CVV must be 3-4 digits.");
        return;
      }
    }

    if (mode === "withdraw") {
      if (!accountHolderName.trim()) {
        setError("Account holder name is required.");
        return;
      }

      if (!bankName) {
        setError("Select a bank.");
        return;
      }

      if (!isValidAccountNumber(accountNumber)) {
        setError("Account number must be 6-11 digits.");
        return;
      }

      if (!isValidBranchCode(branchCode)) {
        setError("Branch code must be 6 digits.");
        return;
      }
    }

    setLoading(true);
    setError(null);

    try {
      const result =
        mode === "deposit"
          ? await depositToWallet(userId, numericAmount, {
              cardNumber: cardNumber.replace(/\s+/g, ""),
              expiryMonth,
              expiryYear,
              cvv,
            })
          : await withdrawFromWallet(userId, numericAmount, {
              accountHolderName,
              bankName,
              accountNumber,
              branchCode,
            });

      if (!result.success) {
        setError(result.message);
        return;
      }

      onSuccess();
      resetForm();
      onClose();
    } catch {
      setError(
        mode === "deposit"
          ? "Could not process the card payment. Check your card details and try again."
          : "Could not process the withdrawal. Check your bank details and try again."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title={copy.title}>
      <form className={styles.form} onSubmit={handleSubmit}>
        <p className={styles.balanceHint}>
          Current balance: {currentBalance.toLocaleString("en-ZA", { maximumFractionDigits: 2 })}{" "}
          {currency}
        </p>

        <label className={styles.label} htmlFor="wallet-amount">
          {copy.label}
        </label>

        <div className={styles.inputWrapper}>
          <span className={styles.currency}>{currency}</span>

          <input
            id="wallet-amount"
            type="number"
            inputMode="decimal"
            min="0"
            step="0.01"
            placeholder="0.00"
            value={amount}
            disabled={loading}
            onChange={(event) => setAmount(event.target.value)}
            autoFocus
          />
        </div>

        {mode === "deposit" && (
          <>
            <label className={styles.label} htmlFor="card-number">
              Card number
            </label>

            <input
              id="card-number"
              type="text"
              inputMode="numeric"
              autoComplete="cc-number"
              placeholder="4111 1111 1111 1111"
              maxLength={24}
              value={cardNumber}
              disabled={loading}
              onChange={(event) => setCardNumber(event.target.value)}
              className={styles.plainInput}
            />

            <div className={styles.cardRow}>
              <div className={styles.cardField}>
                <label className={styles.label} htmlFor="expiry-month">
                  Expiry month
                </label>

                <input
                  id="expiry-month"
                  type="text"
                  inputMode="numeric"
                  autoComplete="cc-exp-month"
                  placeholder="MM"
                  maxLength={2}
                  value={expiryMonth}
                  disabled={loading}
                  onChange={(event) => setExpiryMonth(event.target.value)}
                  className={styles.plainInput}
                />
              </div>

              <div className={styles.cardField}>
                <label className={styles.label} htmlFor="expiry-year">
                  Expiry year
                </label>

                <input
                  id="expiry-year"
                  type="text"
                  inputMode="numeric"
                  autoComplete="cc-exp-year"
                  placeholder="YYYY"
                  maxLength={4}
                  value={expiryYear}
                  disabled={loading}
                  onChange={(event) => setExpiryYear(event.target.value)}
                  className={styles.plainInput}
                />
              </div>

              <div className={styles.cardField}>
                <label className={styles.label} htmlFor="cvv">
                  CVV
                </label>

                <input
                  id="cvv"
                  type="text"
                  inputMode="numeric"
                  autoComplete="cc-csc"
                  placeholder="123"
                  maxLength={4}
                  value={cvv}
                  disabled={loading}
                  onChange={(event) => setCvv(event.target.value)}
                  className={styles.plainInput}
                />
              </div>
            </div>
          </>
        )}

        {mode === "withdraw" && (
          <>
            <label className={styles.label} htmlFor="account-holder-name">
              Account holder name
            </label>

            <input
              id="account-holder-name"
              type="text"
              autoComplete="name"
              placeholder="As it appears on the account"
              value={accountHolderName}
              disabled={loading}
              onChange={(event) => setAccountHolderName(event.target.value)}
              className={styles.plainInput}
            />

            <label className={styles.label} htmlFor="bank-name">
              Bank
            </label>

            <select
              id="bank-name"
              value={bankName}
              disabled={loading}
              onChange={(event) => setBankName(event.target.value)}
              className={styles.plainInput}
            >
              <option value="" disabled>
                Select your bank
              </option>
              {SA_BANKS.map((bank) => (
                <option key={bank} value={bank}>
                  {bank}
                </option>
              ))}
            </select>

            <div className={styles.cardRow}>
              <div className={styles.cardField}>
                <label className={styles.label} htmlFor="account-number">
                  Account number
                </label>

                <input
                  id="account-number"
                  type="text"
                  inputMode="numeric"
                  placeholder="1234567890"
                  maxLength={11}
                  value={accountNumber}
                  disabled={loading}
                  onChange={(event) => setAccountNumber(event.target.value)}
                  className={styles.plainInput}
                />
              </div>

              <div className={styles.cardField}>
                <label className={styles.label} htmlFor="branch-code">
                  Branch code
                </label>

                <input
                  id="branch-code"
                  type="text"
                  inputMode="numeric"
                  placeholder="632005"
                  maxLength={6}
                  value={branchCode}
                  disabled={loading}
                  onChange={(event) => setBranchCode(event.target.value)}
                  className={styles.plainInput}
                />
              </div>
            </div>
          </>
        )}

        {error && <p className={styles.error}>{error}</p>}

        <button
          type="submit"
          className={styles.submit}
          disabled={loading}
        >
          {loading ? copy.workingAction : copy.action}
        </button>
      </form>
    </Modal>
  );
}