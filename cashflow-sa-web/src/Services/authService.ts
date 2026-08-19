import { api } from "./api";
import { clearAuthTokens } from "../utils/storage";

// ---- Request/response shapes, matching the real backend exactly ----

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string; // ISO date string
}

export interface RegisterSmeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  companyName: string;
  contactPerson: string;
  companyEmail: string;
  companyPhoneNumber: string;
  registrationNumber: string;
  taxNumber: string;
  address: string;
  industry: string; // e.g. "Agriculture" -- must match an IndustryType enum name exactly
}

export interface RegisterInvestorRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  address: string;
  riskAppetite: string; // "Low" | "Medium" | "High" -- must match RiskAppetite enum name
}

// ---- Actual calls ----

export async function login(credentials: LoginRequest): Promise<LoginResponse> {
  const response = await api.post<LoginResponse>("/Auth/login", credentials);
  return response.data;
}

export async function registerSme(data: RegisterSmeRequest): Promise<{ smeId: string }> {
  const response = await api.post<{ smeId: string }>("/Auth/register/sme", data);
  return response.data;
}

export async function registerInvestor(data: RegisterInvestorRequest): Promise<{ investorId: string }> {
  const response = await api.post<{ investorId: string }>("/Auth/register/investor", data);
  return response.data;
}

export function logout(): void {
  clearAuthTokens();
}