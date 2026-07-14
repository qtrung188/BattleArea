// inventory.js — xem inventory cá nhân, có phân trang.

import { apiFetch } from "./api.js";
import { toast } from "./ui.js";

const PAGE_SIZE = 10;
let currentPage = 1;

export async function loadInventory(page = currentPage) {
  try {
    const data = await apiFetch(`/inventory?page=${page}&pageSize=${PAGE_SIZE}`);
    currentPage = data.page;
    render(data);
  } catch (e) {
    toast(e.message, "error");
  }
}

function render(data) {
  const list = document.getElementById("inventoryList");
  if (!data.items.length) {
    list.innerHTML = `<p class="muted">Chưa sở hữu item nào.</p>`;
  } else {
    list.innerHTML = data.items
      .map(
        (i) => `
        <div class="item-row">
          <span>${escapeHtml(i.itemName)} <span class="muted">x${i.quantity}</span></span>
          ${i.isEquipped ? '<span class="badge">Đang trang bị</span>' : ""}
        </div>`
      )
      .join("");
  }
  renderPager(data);
}

function renderPager(data) {
  const pager = document.getElementById("inventoryPager");
  pager.innerHTML = pagerHtml(data);
  pager.querySelector("[data-prev]")?.addEventListener("click", () => loadInventory(data.page - 1));
  pager.querySelector("[data-next]")?.addEventListener("click", () => loadInventory(data.page + 1));
}

function pagerHtml(data) {
  const prevDisabled = data.page <= 1 ? "disabled" : "";
  const nextDisabled = data.page >= data.totalPages ? "disabled" : "";
  return `
    <button data-prev ${prevDisabled}>‹ Trước</button>
    <span class="muted">Trang ${data.page}/${data.totalPages || 1}</span>
    <button data-next ${nextDisabled}>Sau ›</button>`;
}

function escapeHtml(s) {
  return String(s).replace(/[&<>"']/g, (c) =>
    ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])
  );
}
