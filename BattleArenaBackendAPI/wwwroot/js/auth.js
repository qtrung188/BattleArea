// auth.js — đăng ký / đăng nhập / đăng xuất và quản lý token.

import { apiFetch, setTokens, clearTokens, getAccessToken } from "./api.js";

export function isLoggedIn() {
  return !!getAccessToken();
}

export async function register(username, password) {
  await apiFetch("/auth/register", "POST", { username, password });
}

export async function login(username, password) {
  const data = await apiFetch("/auth/login", "POST", { username, password });
  setTokens(data.accessToken, data.refreshToken);
  return data;
}

export async function logout() {
  const refreshToken = localStorage.getItem("refreshToken");
  try {
    // Thu hồi refresh token phía server; lỗi ở bước này không cản trở logout.
    await apiFetch("/auth/logout", "POST", { refreshToken });
  } catch {
    /* ignore */
  }
  clearTokens();
}
