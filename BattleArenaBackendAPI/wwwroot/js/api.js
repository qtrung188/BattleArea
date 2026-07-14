// api.js — fetch wrapper dùng chung cho toàn bộ FE.
// Tự gắn Bearer token, chuẩn hóa lỗi ProblemDetails, và tự refresh access
// token 1 lần khi gặp 401 (nếu còn refresh token hợp lệ).

const API_BASE = "/api/v1";

export function getAccessToken() {
  return localStorage.getItem("accessToken");
}

export function getRefreshToken() {
  return localStorage.getItem("refreshToken");
}

export function setTokens(accessToken, refreshToken) {
  if (accessToken) localStorage.setItem("accessToken", accessToken);
  if (refreshToken) localStorage.setItem("refreshToken", refreshToken);
}

export function clearTokens() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
}

// Gộp các lần refresh đồng thời lại thành 1 request duy nhất.
let refreshingPromise = null;

async function tryRefresh() {
  const refreshToken = getRefreshToken();
  if (!refreshToken) return false;

  if (!refreshingPromise) {
    refreshingPromise = fetch(`${API_BASE}/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    })
      .then(async (res) => {
        if (!res.ok) return false;
        const data = await res.json();
        setTokens(data.accessToken, null); // refresh chỉ trả accessToken mới
        return true;
      })
      .catch(() => false)
      .finally(() => {
        refreshingPromise = null;
      });
  }

  return refreshingPromise;
}

/**
 * Gọi API. Trả về JSON đã parse, hoặc null với 204 No Content.
 * Ném Error(message) khi response không thành công.
 */
export async function apiFetch(path, method = "GET", body = null, _retried = false) {
  const headers = { "Content-Type": "application/json" };
  const token = getAccessToken();
  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : null,
  });

  // Access token hết hạn → thử refresh rồi retry đúng 1 lần.
  if (res.status === 401 && !_retried && getRefreshToken()) {
    const ok = await tryRefresh();
    if (ok) return apiFetch(path, method, body, true);

    clearTokens();
    window.dispatchEvent(new CustomEvent("auth:expired"));
    throw new Error("Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.");
  }

  if (res.status === 204) return null;

  const data = await res.json().catch(() => null);
  if (!res.ok) {
    // ProblemDetails RFC 7807: ưu tiên detail, rồi title.
    throw new Error(data?.detail || data?.title || `Lỗi ${res.status}`);
  }
  return data;
}
