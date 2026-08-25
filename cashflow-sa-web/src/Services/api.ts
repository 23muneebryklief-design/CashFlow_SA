import axios, { AxiosError } from "axios";
import { API_BASE_URL } from "../utils/env";
import { clearAuthTokens, getAccessToken } from "../utils/storage";

export interface ApiError { message: string; status?: number; code?: string; }

export const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 600000,
  headers: { "Content-Type": "application/json" },
});

api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string; title?: string; detail?: string }>) => {
    const status = error.response?.status;
    if (status === 401) {
      clearAuthTokens();
      window.dispatchEvent(new Event("cashflow:auth-expired"));
    }
    const data = error.response?.data;
    const message = data?.message || data?.detail || data?.title ||
      (status === 403 ? "You do not have permission to perform this action." :
       status === 404 ? "The requested resource could not be found." :
       status && status >= 500 ? "The server is unavailable. Please try again shortly." :
       error.code === "ECONNABORTED" ? "The request took too long. Please try again." :
       "Something went wrong. Please try again.");
    return Promise.reject({ message, status, code: error.code } satisfies ApiError);
  }
);
