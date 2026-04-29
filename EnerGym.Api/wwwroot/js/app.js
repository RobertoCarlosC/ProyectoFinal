

const API_BASE = '/api';


function getSession() {
  try {
    const s = localStorage.getItem('energym_user');
    return s ? JSON.parse(s) : null;
  } catch (e) {
    return null;
  }
}

function setSession(user) {
  localStorage.setItem('energym_user', JSON.stringify(user));
}

function clearSession() {
  localStorage.removeItem('energym_user');
  localStorage.removeItem('energym_cart_count');
}

function requireLogin() {
  if (!getSession()) {
    window.location.href = '/pages/login.html';
    return false;
  }
  return true;
}

function requireAdmin() {
  const user = getSession();
  if (!user || user.idRol !== 1) {
    window.location.href = '/index.html';
    return false;
  }
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


function buildNavbar(activePage) {
  const user     = getSession();
  const isAdmin  = user && user.idRol === 1;
  const cartCount = parseInt(localStorage.getItem('energym_cart_count') || '0');
  const cartBadge = cartCount > 0 ? `<span class="cart-badge">${cartCount}</span>` : '';

  return `
  <nav class="navbar navbar-expand-lg">
    <div class="container">
      <a class="navbar-brand" href="/index.html">Ener<span>Gym</span></a>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navMenu">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navMenu">
        <ul class="navbar-nav me-auto">
          <li class="nav-item">
            <a class="nav-link ${activePage==='home'?'active':''}" href="/index.html">Inicio</a>
          </li>
          <li class="nav-item">
            <a class="nav-link ${activePage==='tienda'?'active':''}" href="/pages/tienda.html">Tienda</a>
          </li>
          ${isAdmin ? `<li class="nav-item"><a class="nav-link ${activePage==='admin'?'active':''}" href="/pages/admin.html">Admin</a></li>` : ''}
        </ul>
        <ul class="navbar-nav align-items-center gap-2">
          ${user ? `
            <li class="nav-item">
              <span class="nav-link" style="font-family:'Barlow Condensed',sans-serif;font-size:0.8rem;letter-spacing:1px;text-transform:uppercase;color:var(--texto2);">
                Hola, ${user.nombre.split(' ')[0]}
              </span>
            </li>
            <li class="nav-item">
              <a class="nav-link ${activePage==='carrito'?'active':''}" href="/pages/carrito.html">
                <span class="nav-icon-wrap">🛒 Carrito ${cartBadge}</span>
              </a>
            </li>
            <li class="nav-item">
              <button onclick="logout()" class="btn-outline-fire" style="font-size:0.75rem;padding:0.4rem 1.2rem;">Salir</button>
            </li>
          ` : `
            <li class="nav-item">
              <a class="nav-link ${activePage==='login'?'active':''}" href="/pages/login.html">Login</a>
            </li>
            <li class="nav-item">
              <a class="btn-fire" href="/pages/registro.html" style="font-size:0.75rem;padding:0.5rem 1.2rem;clip-path:none;">Registrarse</a>
            </li>
          `}
        </ul>
      </div>
    </div>
  </nav>`;
}

function injectNavbar(activePage) {
  const el = document.getElementById('navbar-placeholder');
  if (el) el.innerHTML = buildNavbar(activePage);
}


function formatPrice(price) {
  return Number(price).toFixed(2).replace('.', ',') + ' €';
}


async function updateCartBadge() {
  const user = getSession();
  if (!user) return;
  try {
    const res  = await fetch(`${API_BASE}/carrito/${user.idUsuario}`);
    if (!res.ok) return;
    const data = await res.json();
    const count = Array.isArray(data.items) ? data.items.length : 0;
    localStorage.setItem('energym_cart_count', count);
  } catch (e) {
    
  }
}


function renderProductCard(p) {
  const user   = getSession();
  const stock  = p.stock ?? p.Stock ?? 0;

  let stockBadge = '';
  if (stock === 0)    stockBadge = `<span class="stock-badge stock-out">Agotado</span>`;
  else if (stock < 5) stockBadge = `<span class="stock-badge stock-low">Últimas ${stock}</span>`;
  else                stockBadge = `<span class="stock-badge stock-ok">En stock</span>`;

  const imagen          = p.imagen || p.Imagen || 'https://via.placeholder.com/300x300/1a1a1a/ff5500?text=EnerGym';
  const nombre          = p.nombre || p.Nombre || '';
  const descripcion     = p.descripcion || p.Descripcion || '';
  const precio          = p.precio ?? p.Precio ?? 0;
  const idProducto      = p.idProducto || p.IdProducto;
  const nombreCategoria = p.nombreCategoria || p.NombreCategoria || '';
  const liked           = p.tieneLike || p.TieneLike || false;
  const desc80          = descripcion.length > 80 ? descripcion.substring(0, 80) + '...' : descripcion;

  return `
  <div class="col-lg-3 col-md-4 col-sm-6 mb-4">
    <div class="product-card h-100">
      <div class="card-img-wrap">
        <img src="${imagen}" alt="${nombre}"
             onerror="this.src='https://via.placeholder.com/300x300/1a1a1a/ff5500?text=EnerGym'">
      </div>
      <div class="card-body d-flex flex-column">
        <div class="d-flex justify-content-between align-items-start mb-1">
          <span class="cat-badge">${nombreCategoria}</span>
          ${stockBadge}
        </div>
        <div class="card-title">${nombre}</div>
        <p style="font-size:0.82rem;color:var(--texto2);flex:1;margin:0.3rem 0 0.8rem;line-height:1.5;">${desc80}</p>
        <div class="card-price">${formatPrice(precio)}</div>
        <div class="card-actions">
          ${user ? `
            <button class="btn-like ${liked ? 'liked' : ''}"
                    onclick="toggleLike(${idProducto}, this)" title="Me gusta">
              ${liked ? '❤️' : '🤍'}
            </button>
            <button class="btn-cart" onclick="addToCart(${idProducto})"
              ${stock === 0 ? 'disabled style="opacity:0.5;cursor:not-allowed;"' : ''}>
              + Añadir
            </button>
          ` : `
            <a href="/pages/login.html" class="btn-cart"
               style="text-decoration:none;text-align:center;display:flex;align-items:center;justify-content:center;">
              Inicia sesión
            </a>
          `}
        </div>
      </div>
    </div>
  </div>`;
}


async function addToCart(idProducto) {
  const user = getSession();
  if (!user) { window.location.href = '/pages/login.html'; return; }

  try {
    const res = await fetch(`${API_BASE}/carrito/agregar`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ idUsuario: user.idUsuario, idProducto, cantidad: 1 })
    });
    const data = await res.json();
    if (res.ok) {
      showToast('Producto añadido al carrito ✓', 'success');
      await updateCartBadge();
      injectNavbar(window._activePage || 'home');
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
      method:  'POST',
      headers: { 'Content-Type': 'application/json' },
      body:    JSON.stringify({ idUsuario: user.idUsuario, idProducto })
    });
    const data = await res.json();
    if (res.ok) {
      btn.classList.toggle('liked', data.liked);
      btn.textContent = data.liked ? '❤️' : '🤍';
    } else {
      showToast(data.error || 'Error al procesar like', 'error');
    }
  } catch (e) {
    showToast('Error de conexión', 'error');
  }
}
