import axios from "axios";

export const api = axios.create({
  baseURL: "http://localhost:5081/api",
  headers: {
    "Content-Type": "application/json",
  },
});

// Attaches the stored access token to every outgoing request, if one exists.
// This runs before every request, so individual calls in authService (or
// anywhere else) never have to manually set the Authorization header themselves.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});