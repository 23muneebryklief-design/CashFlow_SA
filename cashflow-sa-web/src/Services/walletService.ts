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