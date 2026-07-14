// app.js — điểm vào: gắn event listener, điều phối luồng đăng nhập và khởi tạo module.

import { register, login, logout, isLoggedIn } from "./auth.js";
import { toast, setStatus, setGold } from "./ui.js";
import { initShop, loadShop } from "./shop.js";
import { loadInventory } from "./inventory.js";
import { initMatch, loadLeaderboard } from "./match.js";
import { initChat, connectChat, disconnectChat } from "./chat.js";

function showApp(loggedIn) {
  document.getElementById("appSection").hidden = !loggedIn;
  setStatus(loggedIn ? "Đã đăng nhập" : "Chưa đăng nhập");
  if (!loggedIn) setGold(null);
}

// Nạp toàn bộ dữ liệu sau khi đăng nhập / khi mở lại trang lúc còn token.
function bootstrapLoggedIn() {
  showApp(true);
  loadShop();
  loadInventory();
  loadLeaderboard();
  connectChat();
}

async function handleRegister() {
  const username = document.getElementById("username").value.trim();
  const password = document.getElementById("password").value;
  if (!username || !password) return toast("Nhập username và password.", "error");
  try {
    await register(username, password);
    toast("Đăng ký thành công — giờ đăng nhập nhé!", "success");
  } catch (e) {
    toast(e.message, "error");
  }
}

async function handleLogin() {
  const username = document.getElementById("username").value.trim();
  const password = document.getElementById("password").value;
  if (!username || !password) return toast("Nhập username và password.", "error");
  try {
    await login(username, password);
    bootstrapLoggedIn();
    toast(`Xin chào, ${username}!`, "success");
  } catch (e) {
    toast(e.message, "error");
  }
}

async function handleLogout() {
  await logout();
  await disconnectChat();
  showApp(false);
  toast("Đã đăng xuất.", "info");
}

function init() {
  // Gắn listener cho các nút auth.
  document.getElementById("btnRegister").addEventListener("click", handleRegister);
  document.getElementById("btnLogin").addEventListener("click", handleLogin);
  document.getElementById("btnLogout").addEventListener("click", handleLogout);

  document.getElementById("btnReloadInventory").addEventListener("click", () => loadInventory());

  // Khởi tạo listener nội bộ của từng module.
  initShop();
  initMatch();
  initChat();

  // Token hết hạn không refresh được → về màn đăng nhập.
  window.addEventListener("auth:expired", () => {
    disconnectChat();
    showApp(false);
    toast("Phiên đã hết hạn, vui lòng đăng nhập lại.", "error");
  });

  // Còn token khi mở trang → tự vào thẳng.
  if (isLoggedIn()) bootstrapLoggedIn();
  else showApp(false);
}

init();
