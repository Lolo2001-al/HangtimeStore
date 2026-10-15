'use strict';
const $ = s => document.querySelector(s);
const money = n => '$' + Number(n).toLocaleString('es-AR');
const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const S = {
  user: JSON.parse(sessionStorage.getItem('hu') || 'null'),
  cart: JSON.parse(localStorage.getItem('hc') || '[]'),
  f: { q: '', categoria: '', orden: '', talle: '' }, cur: null, sel: null, tab: 'login'
};

// ---------- utilidades ----------
const shoe = c => `<svg viewBox="0 0 300 160" class="shoe" aria-hidden="true"><path d="M20 118c0-10 6-16 16-18l52-12c10-3 16-10 20-22l8-20c3-6 10-8 16-5l34 18c8 4 14 9 22 12l40 14c14 5 24 12 26 28v12c0 6-4 10-10 10H30c-6 0-10-4-10-10z" fill="${esc(c[0])}"/><path d="M20 128h260v14c0 6-4 10-10 10H30c-6 0-10-4-10-10z" fill="${esc(c[1])}"/><path d="M118 70l30 14M134 62l26 12" stroke="${esc(c[2])}" stroke-width="5" stroke-linecap="round"/><circle cx="214" cy="104" r="10" fill="${esc(c[2])}"/></svg>`;
const toast = m => { const t = $('#toast'); t.textContent = m; t.classList.add('show'); clearTimeout(toast.t); toast.t = setTimeout(() => t.classList.remove('show'), 3000); };
const api = async (path, o = {}) => {
  const h = { 'Content-Type': 'application/json' };
  const t = sessionStorage.getItem('hj'); if (t) h.Authorization = 'Bearer ' + t;
  const r = await fetch('/api/' + path, { ...o, headers: h, body: o.body ? JSON.stringify(o.body) : undefined });
  const d = await r.json().catch(() => null);
  if (!r.ok) throw new Error(d?.error || Object.values(d?.errors || {})[0]?.[0] || (r.status === 401 ? 'Iniciá sesión' : r.status === 429 ? 'Demasiados intentos, esperá un minuto' : 'Error ' + r.status));
  return d;
};
const io = new IntersectionObserver(es => es.forEach(e => { if (e.isIntersecting) { e.target.classList.add('in'); io.unobserve(e.target); } }), { threshold: .12 });
const reveal = () => document.querySelectorAll('.rv:not(.in)').forEach(el => io.observe(el));
const saveCart = () => { localStorage.setItem('hc', JSON.stringify(S.cart)); $('#cartN').textContent = S.cart.reduce((a, i) => a + i.cantidad, 0); };
const closeAll = () => ['#drawer', '#modal', '#veil'].forEach(s => $(s).classList.remove('open'));
const openPanel = s => { $(s).classList.add('open'); $('#veil').classList.add('open'); };

// ---------- navegación ----------
function nav() {
  const u = S.user;
  $('#nav').innerHTML = '<a href="#/">Colección</a>' + (u ? '<a href="#/pedidos">Mis pedidos</a>' : '') +
    (u?.rol === 'Admin' ? '<a href="#/admin">Admin</a>' : '') +
    (u ? `<button data-act="logout">Salir (${esc(u.nombre.split(' ')[0])})</button>` : '<button data-act="auth">Ingresar</button>');
}
function route() {
  const h = location.hash || '#/'; nav(); window.scrollTo(0, 0);
  if (h === '#/pedidos') return S.user ? pedidos() : (location.hash = '#/');
  if (h === '#/admin') return S.user?.rol === 'Admin' ? admin() : (location.hash = '#/');
  home();
}

// ---------- catálogo ----------
async function home() {
  $('#app').innerHTML = `<section class="hero"><div><p class="eyebrow rv">Colección retro</p><h1 class="rv" style="--d:80ms">Volá<br>bajo.<br><em>Pisá alto.</em></h1><p class="sub rv" style="--d:160ms">Siluetas icónicas con stock real por talle, sin vueltas.</p><button class="btn rv" style="--d:240ms" data-act="goto">Ver colección</button></div><div class="stage rv" style="--d:200ms">${shoe(['#111111', '#f6f5f2', '#d6001c'])}</div></section>
  <section class="cat" id="cat"><div class="filters rv"><input id="q" placeholder="Buscar modelo…" aria-label="Buscar" value="${esc(S.f.q)}">
  ${['', 'high', 'mid', 'low'].map(c => `<button class="chip ${S.f.categoria === c ? 'on' : ''}" data-act="cat" data-v="${c}">${c || 'todas'}</button>`).join('')}
  <select id="talle" aria-label="Talle"><option value="">Talle</option>${[38, 39, 40, 41, 42, 43, 44, 45, 46].map(t => `<option ${S.f.talle == t ? 'selected' : ''}>${t}</option>`).join('')}</select>
  <select id="orden" aria-label="Orden"><option value="">Destacados</option><option value="precio_asc" ${S.f.orden === 'precio_asc' ? 'selected' : ''}>Menor precio</option><option value="precio_desc" ${S.f.orden === 'precio_desc' ? 'selected' : ''}>Mayor precio</option></select></div><div class="grid" id="grid"></div></section>`;
  reveal(); load();
}
async function load() {
  const p = new URLSearchParams(Object.entries(S.f).filter(([, v]) => v));
  try {
    const l = await api('productos?' + p); S.prods = l;
    $('#grid').innerHTML = l.length ? l.map((x, i) => `<article class="card rv" style="--d:${i * 60}ms" data-act="open" data-id="${x.id}" tabindex="0"><div class="img">${shoe([x.color1, x.color2, x.color3])}</div><div class="meta"><h3>${esc(x.nombre)}</h3><span>${money(x.precio)}</span></div><small>${esc(x.categoria)}</small></article>`).join('') : '<p class="empty">No hay modelos con esos filtros.</p>';
    reveal();
  } catch (e) { toast(e.message); }
}
function product(id) {
  const p = S.prods.find(x => x.id === id); if (!p) return; S.cur = p; S.sel = null;
  $('#modal').innerHTML = `<div class="pd"><div>${shoe([p.color1, p.color2, p.color3])}</div><div><h2>${esc(p.nombre)}</h2><p style="color:var(--mute);margin:6px 0 10px">${esc(p.descripcion)}</p><b style="font-size:22px">${money(p.precio)}</b>
  <p style="margin-top:18px;font-size:13px;color:var(--mute)">Elegí tu talle</p><div class="tallas">${p.talles.map(t => `<button class="tl" data-act="talle" data-id="${t.id}" ${t.stock ? '' : 'disabled'} title="${t.stock} en stock">${t.numero}</button>`).join('')}</div>
  <button class="btn" data-act="add">Agregar al carrito</button></div></div>`;
  openPanel('#modal');
}

// ---------- carrito ----------
function drawer() {
  const total = S.cart.reduce((a, i) => a + i.precio * i.cantidad, 0);
  $('#drawer').innerHTML = `<h2>Tu carrito</h2><div class="lines">${S.cart.length ? S.cart.map((i, k) => `<div class="line">${shoe(i.c)}<div><b>${esc(i.nombre)}</b><br><small style="color:var(--mute)">Talle ${i.numero} · ${money(i.precio)}</small></div><div class="qty"><button data-act="dec" data-k="${k}" aria-label="Quitar uno">−</button>${i.cantidad}<button data-act="inc" data-k="${k}" aria-label="Sumar uno">+</button></div></div>`).join('') : '<p class="empty">Todavía no agregaste nada.</p>'}</div>
  <div class="total"><span>Total</span><span>${money(total)}</span></div><input class="field" id="dir" placeholder="Dirección de envío" autocomplete="street-address"><button class="btn" data-act="checkout" ${S.cart.length ? '' : 'disabled'}>Finalizar compra</button>`;
  openPanel('#drawer');
}
async function checkout() {
  if (!S.user) { closeAll(); return authModal(); }
  const direccion = $('#dir').value.trim(); if (direccion.length < 5) return toast('Ingresá tu dirección de envío');
  try {
    const r = await api('pedidos', { method: 'POST', body: { direccion, items: S.cart.map(i => ({ talleId: i.talleId, cantidad: i.cantidad })) } });
    S.cart = []; saveCart(); closeAll(); toast(`Pedido #${r.id} confirmado · ${money(r.total)}`); location.hash = '#/pedidos';
  } catch (e) { toast(e.message); }
}

// ---------- autenticación ----------
function authModal() {
  const reg = S.tab === 'registro';
  $('#modal').innerHTML = `<div class="col" style="max-width:380px;margin:auto"><div class="tabs"><button class="${reg ? '' : 'on'}" data-act="tab" data-v="login">Ingresar</button><button class="${reg ? 'on' : ''}" data-act="tab" data-v="registro">Crear cuenta</button></div>
  <form class="col" id="authForm">${reg ? '<input class="field" name="nombre" placeholder="Nombre" required minlength="2" autocomplete="name">' : ''}<input class="field" name="email" type="email" placeholder="Email" required autocomplete="email"><input class="field" name="password" type="password" placeholder="Contraseña" required minlength="${reg ? 8 : 1}" autocomplete="${reg ? 'new-password' : 'current-password'}">${reg ? '<small style="color:var(--mute)">Mínimo 8 caracteres con mayúscula, minúscula y número.</small>' : ''}<button class="btn">${reg ? 'Crear cuenta' : 'Ingresar'}</button></form></div>`;
  openPanel('#modal');
}
async function auth(form) {
  const d = Object.fromEntries(new FormData(form));
  try {
    const r = await api('auth/' + S.tab, { method: 'POST', body: d });
    sessionStorage.setItem('hj', r.token); S.user = { nombre: r.nombre, rol: r.rol }; sessionStorage.setItem('hu', JSON.stringify(S.user));
    closeAll(); nav(); toast('Hola, ' + r.nombre.split(' ')[0]); route();
  } catch (e) { toast(e.message); }
}

// ---------- pedidos y admin ----------
const pedidoRow = (p, adm) => `<div class="row rv"><div><b>Pedido #${p.id}</b> · ${money(p.total)}<small>${new Date(p.creadoEn + 'Z').toLocaleString('es-AR')}${adm ? ' · ' + esc(p.cliente) : ''} · ${esc(p.direccion)}</small><small>${p.items.map(i => `${esc(i.producto)} (T${i.talle}) ×${i.cantidad}`).join(', ')}</small></div>
${adm && p.estado !== 'Cancelado' ? `<select data-estado="${p.id}">${['Pendiente', 'Pagado', 'Enviado', 'Entregado', 'Cancelado'].map(e => `<option ${e === p.estado ? 'selected' : ''}>${e}</option>`).join('')}</select>` : `<span class="est">${esc(p.estado)}</span>`}</div>`;
async function pedidos() {
  $('#app').innerHTML = '<div class="page"><h2 class="rv">Mis pedidos</h2><div id="lst"></div></div>'; reveal();
  try { const l = await api('pedidos/mios'); $('#lst').innerHTML = l.length ? l.map(p => pedidoRow(p)).join('') : '<p class="empty">Todavía no hiciste pedidos.</p>'; reveal(); } catch (e) { toast(e.message); }
}
async function admin() {
  $('#app').innerHTML = '<div class="page"><h2 class="rv">Panel</h2><div class="kpis" id="kpi"></div><h3 style="margin:0 0 12px">Stock crítico</h3><div id="low"></div><h3 style="margin:28px 0 12px">Pedidos</h3><div id="lst"></div><h3 style="margin:28px 0 12px">Nuevo modelo</h3><form class="col" id="pf" style="max-width:420px"><input class="field" name="nombre" placeholder="Nombre" required><input class="field" name="precio" type="number" min="1" placeholder="Precio" required><input class="field" name="categoria" placeholder="Categoría (high, mid, low)" value="mid" required><input class="field" name="talles" placeholder="Talles y stock: 40:5, 41:3" required><button class="btn">Crear</button></form></div>'; reveal();
  try {
    const [r, l] = await Promise.all([api('admin/resumen'), api('pedidos')]);
    $('#kpi').innerHTML = [[money(r.ventas), 'Ventas'], [r.pedidos, 'Pedidos activos'], [r.top[0]?.producto ?? '—', 'Más vendido']].map(([a, b], i) => `<div class="kpi rv" style="--d:${i * 80}ms"><b>${esc(a)}</b><span>${b}</span></div>`).join('');
    $('#low').innerHTML = r.stockBajo.length ? r.stockBajo.map(s => `<div class="row"><span>${esc(s.producto)} · T${s.talle}</span><span class="est">${s.stock} u.</span></div>`).join('') : '<p class="empty">Sin alertas.</p>';
    $('#lst').innerHTML = l.map(p => pedidoRow(p, true)).join(''); reveal();
  } catch (e) { toast(e.message); }
}
async function nuevoProducto(f) {
  const d = Object.fromEntries(new FormData(f));
  const talles = d.talles.split(',').map(x => x.trim().split(':').map(Number)).filter(x => x.length === 2).map(([numero, stock]) => ({ numero, stock }));
  try { await api('productos', { method: 'POST', body: { nombre: d.nombre, precio: +d.precio, categoria: d.categoria, talles } }); toast('Modelo creado'); f.reset(); } catch (e) { toast(e.message); }
}

// ---------- eventos (sin handlers inline: compatible con CSP) ----------
const acts = {
  goto: () => $('#cat').scrollIntoView({ behavior: 'smooth' }),
  cat: a => { S.f.categoria = a.dataset.v; home(); },
  open: a => product(+a.dataset.id),
  close: closeAll, cart: drawer, auth: authModal,
  tab: a => { S.tab = a.dataset.v; authModal(); },
  talle: a => { document.querySelectorAll('.tl').forEach(b => b.classList.remove('sel')); a.classList.add('sel'); S.sel = +a.dataset.id; },
  add: () => {
    const t = S.cur?.talles.find(x => x.id === S.sel); if (!t) return toast('Elegí un talle');
    const l = S.cart.find(i => i.talleId === t.id), p = S.cur;
    if ((l?.cantidad || 0) >= Math.min(t.stock, 5)) return toast('No hay más stock de ese talle');
    l ? l.cantidad++ : S.cart.push({ talleId: t.id, nombre: p.nombre, numero: t.numero, precio: p.precio, cantidad: 1, c: [p.color1, p.color2, p.color3] });
    saveCart(); closeAll(); toast('Agregado al carrito');
  },
  inc: a => { const i = S.cart[+a.dataset.k]; if (i.cantidad < 5) i.cantidad++; saveCart(); drawer(); },
  dec: a => { const k = +a.dataset.k; if (--S.cart[k].cantidad <= 0) S.cart.splice(k, 1); saveCart(); drawer(); },
  checkout,
  logout: () => { sessionStorage.clear(); S.user = null; location.hash = '#/'; route(); toast('Sesión cerrada'); }
};
document.addEventListener('click', e => { const a = e.target.closest('[data-act]'); if (a && !(a.id === 'veil' && e.target !== a)) acts[a.dataset.act]?.(a); });
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeAll(); if (e.key === 'Enter' && e.target.matches('.card')) e.target.click(); });
document.addEventListener('submit', e => { e.preventDefault(); if (e.target.id === 'authForm') auth(e.target); if (e.target.id === 'pf') nuevoProducto(e.target); });
document.addEventListener('input', e => { if (e.target.id === 'q') { clearTimeout(S.dt); S.dt = setTimeout(() => { S.f.q = e.target.value.trim(); load(); }, 300); } });
document.addEventListener('change', async e => {
  const t = e.target;
  if (t.id === 'talle') { S.f.talle = t.value; load(); }
  if (t.id === 'orden') { S.f.orden = t.value; load(); }
  if (t.dataset.estado) { try { await api(`pedidos/${t.dataset.estado}/estado`, { method: 'PATCH', body: { estado: t.value } }); toast('Estado actualizado'); admin(); } catch (x) { toast(x.message); } }
});
window.addEventListener('hashchange', route);
saveCart(); route();
