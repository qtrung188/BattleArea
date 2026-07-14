// chat.js — chat real-time qua SignalR (biến toàn cục `signalR` nạp từ CDN).

import { getAccessToken } from "./api.js";
import { toast } from "./ui.js";

let connection = null;

export async function connectChat() {
  if (connection) return; // đã kết nối

  connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub/global-chat", { accessTokenFactory: () => getAccessToken() })
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveMessage", (username, message) => appendLine(username, message));

  try {
    await connection.start();
    appendSystem("Đã kết nối chat.");
  } catch (err) {
    console.error("SignalR error:", err);
    connection = null;
    toast("Không kết nối được chat.", "error");
  }
}

export async function disconnectChat() {
  if (connection) {
    try {
      await connection.stop();
    } catch {
      /* ignore */
    }
    connection = null;
  }
}

export async function sendMessage() {
  const input = document.getElementById("chatInput");
  const message = input.value.trim();
  if (!message) return;

  if (!connection || connection.state !== "Connected") {
    toast("Chat chưa sẵn sàng.", "error");
    return;
  }

  try {
    await connection.invoke("SendMessage", message);
    input.value = "";
  } catch (err) {
    // HubException khi vượt rate limit (5 tin / 10 giây).
    toast(err?.message?.replace(/^.*HubException:\s*/, "") || "Gửi tin thất bại.", "error");
  }
}

export function initChat() {
  document.getElementById("btnSendChat")?.addEventListener("click", sendMessage);
  document.getElementById("chatInput")?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") sendMessage();
  });
}

function appendLine(username, message) {
  const log = document.getElementById("chatLog");
  const row = document.createElement("div");
  row.innerHTML = `<b>${escapeHtml(username)}:</b> ${escapeHtml(message)}`;
  log.appendChild(row);
  log.scrollTop = log.scrollHeight;
}

function appendSystem(text) {
  const log = document.getElementById("chatLog");
  const row = document.createElement("div");
  row.className = "muted";
  row.textContent = text;
  log.appendChild(row);
  log.scrollTop = log.scrollHeight;
}

function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, (c) =>
    ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])
  );
}
