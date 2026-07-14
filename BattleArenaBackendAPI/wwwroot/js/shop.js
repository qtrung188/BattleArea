// shop.js — danh sách item (phân trang) và mua item.

import { apiFetch } from "./api.js";
import { toast, setGold } from "./ui.js";
import { loadInventory } from "./inventory.js";

const PAGE_SIZE = 10;
let currentPage = 1;

export function initShop() {
  document.getElementById("btnReloadShop")?.addEventListener("click", () => loadShop());

  // Event delegation cho các nút "Mua" render động.
  document.getElementById("shopList")?.addEventListener("click", (e) => {
    const btn = e.target.closest("[data-buy]");
    if (btn) buyItem(Number(btn.dataset.buy));
  });
}

export async function loadShop(page = currentPage) {
  try {
    const data = await apiFetch(`/shop/items?page=${page}&pageSize=${PAGE_SIZE}`);
    currentPage = data.page;
    render(data);
  } catch (e) {
    toast(e.message, "error");
  }
}

async function buyItem(itemId) {
  try {
    // BuyResponse: { itemName, quantityPurchased, totalCost, remainingGold, quantityOwned }
    const r = await apiFetch("/shop/buy", "POST", { itemId, quantity: 1 });
    setGold(r.remainingGold);
    toast(`Đã mua ${r.itemName} x${r.quantityPurchased}. Gold còn lại: ${r.remainingGold}`, "success");
    loadInventory();
  } catch (e) {
    toast(e.message, "error");
  }
}

function render(data) {
  const list = document.getElementById("shopList");
  if (!data.items.length) {
    list.innerHTML = `<p class="muted">Shop trống.</p>`;
  } else {
    list.innerHTML = data.items
      .map(
        (i) => `
        <div class="item-row">
          <span>${escapeHtml(i.name)} <span class="tag">${escapeHtml(i.type)}</span></span>
          <span>
            <span class="muted">${i.price} Gold</span>
            <button data-buy="${i.id}">Mua</button>
          </span>
        </div>`
      )
      .join("");
  }
  renderPager(data);
}

function renderPager(data) {
  const pager = document.getElementById("shopPager");
  pager.innerHTML = pagerHtml(data);
  pager.querySelector("[data-prev]")?.addEventListener("click", () => loadShop(data.page - 1));
  pager.querySelector("[data-next]")?.addEventListener("click", () => loadShop(data.page + 1));
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
