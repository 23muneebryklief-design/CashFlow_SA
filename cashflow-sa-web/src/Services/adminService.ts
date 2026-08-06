import { api } from "./api";

export interface CreateAdminRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  role: string; // "Admin" | "CreditAnalyst" | "Auditor"
}

export async function createAdmin(data: CreateAdminRequest): Promise<{ adminId: string }> {
  const response = await api.post<{ adminId: string }>("/Admin/create-admin", data);
  return response.data;
}
