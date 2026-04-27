

const API_BASE = '/api';


function getSession() {
  try {
    const s = localStorage.getItem('energym_user');
    return s ? JSON.parse(s) : null;
  } catch (e) { return null; }
}
function setSession(user) {
  localStorage.setItem('energym_user', JSON.stringify(user));
}
function clearSession() {
  localStorage.removeItem('energym_user');
  localStorage.removeItem('energym_cart_count');
}
function requireLogin() {
  if (!getSession()) { window.location.href = '/pages/login.html'; return false; }
  return true;
}
function requireAdmin() {
  const user = getSession();
  if (!user || user.idRol !== 1) { window.location.href = '/index.html'; return false; }
  return true;
}
function logout() {
  clearSession();
  window.location.href = '/index.html';
}


function showToast(msg, type = 'info') {
  let container = document.getElementById('toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container';
    document.body.appendChild(container);
  }
  const toast = document.createElement('div');
  toast.className = `toast-msg ${type}`;
  toast.textContent = msg;
  container.appendChild(toast);
  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transition = 'opacity 0.3s';
    setTimeout(() => toast.remove(), 300);
  }, 3000);
}


function updateNavbar() {
  const user = getSession();
  const navRight = document.querySelector('.nav-right');
  if (!navRight) return;

  const cartCount = parseInt(localStorage.getItem('energym_cart_count') || '0');

  if (user) {
    navRight.innerHTML = `
      <span class="nav-username">${user.nombre.split(' ')[0]}</span>
      ${user.idRol === 1
        ? `<a href="/pages/admin.html" title="Panel admin"><i class="fa-solid fa-screwdriver-wrench"></i></a>`
        : `<a href="/pages/perfil.html" title="Mi perfil"><i class="fa-regular fa-user"></i></a>`
      }
      <a href="/pages/carrito.html" class="cart-icon" title="Carrito">
        <i class="fa-solid fa-bag-shopping"></i>
        <span class="cart-count">${cartCount}</span>
      </a>
      <button class="nav-logout" onclick="logout()">Salir</button>
    `;
  } else {
    navRight.innerHTML = `
      <a href="/pages/login.html" title="Iniciar sesión"><i class="fa-regular fa-user"></i></a>
      <a href="/pages/login.html"><i class="fa-regular fa-heart"></i></a>
      <a href="/pages/login.html" class="cart-icon">
        <i class="fa-solid fa-bag-shopping"></i>
        <span class="cart-count">0</span>
      </a>
    `;
  }
}


function formatPrice(price) {
  return Number(price).toFixed(2).replace('.', ',') + ' €';
}


async function updateCartBadge() {
  const user = getSession();
  if (!user) return;
  try {
    const res = await fetch(`${API_BASE}/carrito/${user.idUsuario}`);
    if (!res.ok) return;
    const data = await res.json();
    const count = Array.isArray(data.items) ? data.items.length : 0;
    localStorage.setItem('energym_cart_count', count);
    const badge = document.querySelector('.cart-count');
    if (badge) badge.textContent = count;
  } catch (e) {}
}


async function addToCart(idProducto) {
  const user = getSession();
  if (!user) { window.location.href = '/pages/login.html'; return; }
  try {
    const res = await fetch(`${API_BASE}/carrito/agregar`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ idUsuario: user.idUsuario, idProducto, cantidad: 1 })
    });
    const data = await res.json();
    if (res.ok) {
      showToast('Producto añadido al carrito ✓', 'success');
      await updateCartBadge();
    } else {
      showToast(data.error || 'Error al añadir al carrito', 'error');
    }
  } catch (e) {
    showToast('Error de conexión con el servidor', 'error');
  }
}


async function toggleLike(idProducto, btn) {
  const user = getSession();
  if (!user) { window.location.href = '/pages/login.html'; return; }
  try {
    const res = await fetch(`${API_BASE}/likes/toggle`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ idUsuario: user.idUsuario, idProducto })
    });
    const data = await res.json();
    if (res.ok) {
      const liked = data.liked;
      btn.classList.toggle('wish-btn-active', liked);
      btn.querySelector('i').className = liked ? 'fa-solid fa-heart' : 'fa-regular fa-heart';
      btn.style.color = liked ? 'var(--red)' : '';
    } else {
      showToast(data.error || 'Error al procesar like', 'error');
    }
  } catch (e) {
    showToast('Error de conexión', 'error');
  }
}


function renderProductCard(p) {
  const user   = getSession();
  const stock  = p.stock ?? p.Stock ?? 0;
  const imagen          = p.imagen || p.Imagen || '';
  const nombre          = p.nombre || p.Nombre || '';
  const descripcion     = p.descripcion || p.Descripcion || '';
  const precio          = p.precio ?? p.Precio ?? 0;
  const idProducto      = p.idProducto || p.IdProducto;
  const nombreCategoria = p.nombreCategoria || p.NombreCategoria || '';
  const liked           = p.tieneLike || p.TieneLike || false;

  let badge = '';
  if (stock === 0)     badge = `<div class="prod-badge sale">AGOTADO</div>`;
  else if (stock < 5)  badge = `<div class="prod-badge">ÚLTIMAS ${stock}</div>`;

  const imgHtml = imagen
    ? `<img src="${imagen}" alt="${nombre}" style="width:100%;height:100%;object-fit:cover;"
           onerror="this.parentElement.innerHTML='<div class=\'img-placeholder prod-img\'><span>${nombre}</span></div>'">`
    : `<div class="img-placeholder prod-img"><span>${nombre}</span></div>`;

  const actions = user ? `
    <button class="wish-btn" onclick="toggleLike(${idProducto}, this)" title="Me gusta">
      <i class="${liked ? 'fa-solid' : 'fa-regular'} fa-heart"
         style="${liked ? 'color:var(--red)' : ''}"></i>
    </button>
    <button class="add-cart" onclick="addToCart(${idProducto})"
      ${stock === 0 ? 'disabled style="opacity:0.5;cursor:not-allowed;"' : ''}>
      + AGREGAR
    </button>
  ` : `
    <a href="/pages/login.html" class="add-cart"
       style="text-decoration:none;text-align:center;flex:1;display:flex;align-items:center;justify-content:center;">
      INICIAR SESIÓN
    </a>
  `;

  return `
  <div class="product-card">
    <div class="product-img-wrap">
      ${imgHtml}
      ${badge}
      <div class="product-actions">${actions}</div>
    </div>
    <div class="product-info">
      <span class="prod-cat">${nombreCategoria}</span>
      <h4>${nombre}</h4>
      <p class="prod-price">${formatPrice(precio)}</p>
    </div>
  </div>`;
}
