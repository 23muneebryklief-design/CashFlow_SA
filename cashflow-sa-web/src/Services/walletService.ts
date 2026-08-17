import { api } from "./api";

// Matches WalletBalanceDto exactly.
export interface WalletBalance {
  walletId: string;
  balance: number;
  currency: string;
}

export async function getWalletBalance(userId: string): Promise<WalletBalance> {
  const response = await api.get<WalletBalance>(`/Wallet/${userId}/balance`);
  return response.data;
}

// Matches WalletController.Deposit -- POST /api/Wallet/deposit, body is a
// DepositFundsCommand. Confirmed via a 400 validation response that the
// command requires CardNumber/ExpiryMonth/ExpiryYear/Cvv in addition to
// UserId/Amount (i.e. this is a card-charge-to-deposit flow, not a plain
// top-up). ExpiryMonth/ExpiryYear/Cvv are sent as strings since the
// validator checks digit-count/format ("01"-"12", "YY" or "YYYY", 3-4
// digits) rather than numeric ranges.
export interface DepositFundsRequest {
  userId: string;
  amount: number;
  cardNumber: string;
  expiryMonth: string;
  expiryYear: string;
  cvv: string;
}

export interface CardDetails {
  cardNumber: string;
  expiryMonth: string;
  expiryYear: string;
  cvv: string;
}

// Matches WithdrawFundsCommand -- same sandbox-simulation idea as deposit's
// card details, just for a payout instead of a charge. AccountNumber ending
// in 0002 simulates a sandbox decline, same convention as the deposit
// test-decline card.
export interface WithdrawFundsRequest {
  userId: string;
  amount: number;
  accountHolderName: string;
  bankName: string;
  accountNumber: string;
  branchCode: string;
}

export interface BankDetails {
  accountHolderName: string;
  bankName: string;
  accountNumber: string;
  branchCode: string;
}

// Matches DepositResultDto / WithdrawResultDto exactly (both handlers return
// the same shape). On a declined card or insufficient balance, the backend
// soft-fails with HTTP 200 + success: false rather than throwing -- so
// callers MUST check `.success`, not just whether the call resolved.
export interface WalletActionResult {
  success: boolean;
  message: string;
  newBalance: number;
  transactionId: string | null;
}

export async function depositToWallet(
  userId: string,
  amount: number,
  card: CardDetails
): Promise<WalletActionResult> {
  const response = await api.post<WalletActionResult>("/Wallet/deposit", {
    userId,
    amount,
    cardNumber: card.cardNumber,
    expiryMonth: card.expiryMonth,
    expiryYear: card.expiryYear,
    cvv: card.cvv,
  } satisfies DepositFundsRequest);
  return response.data;
}

// Matches WalletController.Withdraw -- POST /api/Wallet/withdraw, body is a
// WithdrawFundsCommand (userId, amount only -- no card details, since a
// withdrawal debits the wallet directly rather than charging a card).
// Server re-validates balance authoritatively even though the modal also
// blocks over-balance withdrawals client-side; on insufficient funds the
// handler soft-fails (success: false) rather than throwing, same shape as
// a declined deposit card.
export async function withdrawFromWallet(
  userId: string,
  amount: number,
  bank: BankDetails
): Promise<WalletActionResult> {
  const response = await api.post<WalletActionResult>("/Wallet/withdraw", {
    userId,
    amount,
    accountHolderName: bank.accountHolderName,
    bankName: bank.bankName,
    accountNumber: bank.accountNumber,
    branchCode: bank.branchCode,
  } satisfies WithdrawFundsRequest);
  return response.data;
}
export interface WalletTransaction {
  transactionId: string;
  walletId: string;
  type: string;
  amount: number;
  referenceType: string;
  referenceId: string | null;
  description: string;
  createdAt: string;
}

export async function getWalletTransactions(userId: string): Promise<WalletTransaction[]> {
  const response = await api.get<WalletTransaction[]>(`/Wallet/${userId}/transactions`);
  return response.data;
}
