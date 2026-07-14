// match.js — bắt đầu trận, nộp điểm (có validate phía server) và xem Top 10.

import { apiFetch } from "./api.js";
import { toast } from "./ui.js";

let currentMatchId = null;

export function initMatch() {
  document.getElementById("btnStartMatch")?.addEventListener("click", startMatch);
  document.getElementById("btnSubmitScore")?.addEventListener("click", submitScore);
  document.getElementById("btnReloadLeaderboard")?.addEventListener("click", loadLeaderboard);
}

async function startMatch() {
  try {
    const data = await apiFetch("/matches/start", "POST");
    currentMatchId = data.matchId;
    document.getElementById("matchStatus").textContent = `Trận đấu đã bắt đầu (id: ${currentMatchId})`;
    toast("Bắt đầu trận đấu — chơi rồi nộp điểm nhé!", "info");
  } catch (e) {
    toast(e.message, "error");
  }
}

async function submitScore() {
  if (!currentMatchId) {
    toast("Bấm 'Bắt đầu trận đấu' trước.", "error");
    return;
  }
  const score = parseInt(document.getElementById("scoreInput").value, 10);
  if (Number.isNaN(score) || score < 0) {
    toast("Điểm số không hợp lệ.", "error");
    return;
  }

  try {
    // 204 khi hợp lệ; 409 khi điểm bất hợp lý hoặc match đã nộp rồi.
    await apiFetch("/leaderboard/score", "POST", { matchId: currentMatchId, score });
    toast("Nộp điểm thành công!", "success");
    currentMatchId = null;
    document.getElementById("matchStatus").textContent = "";
    document.getElementById("scoreInput").value = "";
    loadLeaderboard();
  } catch (e) {
    // Thông báo rõ lý do bị từ chối (điểm quá cao / đã nộp).
    toast(e.message, "error");
  }
}

export async function loadLeaderboard() {
  try {
    const rows = await apiFetch("/leaderboard/top");
    const list = document.getElementById("leaderboardList");
    if (!rows.length) {
      list.innerHTML = `<p class="muted">Chưa có ai trên bảng xếp hạng.</p>`;
      return;
    }
    list.innerHTML = rows
      .map(
        (r) => `
        <div class="item-row">
          <span class="rank">#${r.rank}</span>
          <span>${escapeHtml(r.username)}</span>
          <span class="muted">${r.score} điểm</span>
        </div>`
      )
      .join("");
  } catch (e) {
    toast(e.message, "error");
  }
}

function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, (c) =>
    ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])
  );
}
