// ui.js — helper hiển thị dùng chung: toast thông báo và cập nhật trạng thái.

export function toast(message, type = "info") {
  const container = document.getElementById("toastContainer");
  if (!container) {
    // Fallback nếu chưa có container.
    alert(message);
    return;
  }

  const el = document.createElement("div");
  el.className = `toast toast-${type}`;
  el.textContent = message;
  container.appendChild(el);

  // Kích hoạt animation vào.
  requestAnimationFrame(() => el.classList.add("show"));

  setTimeout(() => {
    el.classList.remove("show");
    setTimeout(() => el.remove(), 300);
  }, 3500);
}

export function setStatus(text) {
  const el = document.getElementById("status");
  if (el) el.textContent = text;
}

export function setGold(gold) {
  const el = document.getElementById("goldDisplay");
  if (el) el.textContent = gold == null ? "—" : gold;
}
