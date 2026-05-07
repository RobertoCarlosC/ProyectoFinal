const API_BASE = '/api';

function escapeHtml(text) {
  if (text == null) return '';
  return String(text)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#039;');
}

function isValidImageUrl(url) {
  if (!url) return false;
  return /^https?:\/\//i.test(String(url));
}

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
  const userName = (user?.nombre || 'Usuario').trim().split(' ')[0];

  if (user) {
    navRight.innerHTML = `
      <span class="nav-username">${escapeHtml(userName)}</span>
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
      <a href="/pages/carrito.html" class="cart-icon">
        <i class="fa-solid fa-bag-shopping"></i>
        <span class="cart-count">0</span>
      </a>
    `;
  }
}

/** Reemplaza #navbar-placeholder por cabecera EnerGym. */
function injectNavbar(activePage) {
  window._activePage = activePage;
  const ph = document.getElementById('navbar-placeholder');
  if (!ph) {
    updateNavbar();
    return;
  }
  ph.outerHTML = `
<div class="noise"></div>
<div class="topbar">
<div class="topbar-track">
<span>ENVÍO GRATIS EN PEDIDOS MAYORES A 49€ &nbsp;&#47;&#47;&nbsp; NUEVA COLECCIÓN 2026 &nbsp;&#47;&#47;&nbsp; CALIDAD PREMIUM GARANTIZADA &nbsp;&#47;&#47;&nbsp; ENVÍO GRATIS EN PEDIDOS MAYORES A 49€ &nbsp;&#47;&#47;&nbsp; NUEVA COLECCIÓN 2026 &nbsp;&#47;&#47;&nbsp; CALIDAD PREMIUM GARANTIZADA &nbsp;&#47;&#47;&nbsp;</span>
<span aria-hidden="true">ENVÍO GRATIS EN PEDIDOS MAYORES A 49€ &nbsp;&#47;&#47;&nbsp; NUEVA COLECCIÓN 2026 &nbsp;&#47;&#47;&nbsp; CALIDAD PREMIUM GARANTIZADA &nbsp;&#47;&#47;&nbsp;</span>
</div>
</div>
<header class="navbar">
<div class="nav-left">
  <nav>
    <a href="/index.html" data-nav="inicio">INICIO</a>
    <a href="/pages/tienda.html" data-nav="productos">PRODUCTOS</a>
    <a href="/pages/tienda.html" data-nav="colecciones">COLECCIONES</a>
    <a href="#" data-nav="nosotros">NOSOTROS</a>
  </nav>
</div>
<div class="nav-logo"><a href="/index.html"><span>EnerGym</span></a></div>
<div class="nav-right">
  <a href="/pages/login.html"><i class="fa-regular fa-user"></i></a>
  <a href="/pages/login.html"><i class="fa-regular fa-heart"></i></a>
  <a href="/pages/carrito.html" class="cart-icon">
    <i class="fa-solid fa-bag-shopping"></i>
    <span class="cart-count">0</span>
  </a>
</div>
<button class="nav-mobile-toggle" onclick="toggleMobileNav()" aria-label="Menú">
  <i class="fa-solid fa-bars"></i>
</button>
</header>`;
  updateNavbar();
  updateCartBadge();
  if (activePage) {
    document.querySelectorAll('.nav-left nav a').forEach(a => {
      if (a.dataset.nav === activePage) a.classList.add('nav-active');
    });
  }
}

/** Reemplaza #footer-placeholder por footer EnerGym. */
function injectFooter() {
  const ph = document.getElementById('footer-placeholder');
  if (!ph) return;
  ph.outerHTML = `
<footer class="footer">
<div class="footer-top-bar">
  <div class="footer-logo"><span>EnerGym</span></div>
  <div class="social-links">
    <a href="https://www.instagram.com/energym164"><i class="fa-brands fa-instagram"></i></a>
    <a href="#"><i class="fa-brands fa-facebook"></i></a>
    <a href="#"><i class="fa-brands fa-tiktok"></i></a>
    <a href="#"><i class="fa-brands fa-pinterest"></i></a>
  </div>
</div>
<div class="footer-divider"></div>
<div class="footer-main reveal">
  <div class="footer-about">
    <p>Suplementación de élite para hombres que no piden permiso. Calidad sin concesiones, resultados sin excusas.</p>
  </div>
  <div class="footer-links">
    <h4>NAVEGACIÓN</h4>
    <a href="/index.html">Inicio</a>
    <a href="/pages/tienda.html">Productos</a>
    <a href="#">Nosotros</a>
  </div>
  <div class="footer-links">
    <h4>AYUDA</h4>
    <a href="#">Preguntas frecuentes</a>
    <a href="#">Envíos y entregas</a>
    <a href="#">Devoluciones</a>
    <a href="#">Política de privacidad</a>
  </div>
  <div class="footer-newsletter">
    <h4>NEWSLETTER</h4>
    <p>Suscríbete y recibe 10% OFF en tu primera compra.</p>
    <div class="newsletter-form">
      <input type="email" placeholder="tu@correo.com">
      <button>→</button>
    </div>
  </div>
</div>
<div class="footer-divider"></div>
<div class="footer-bottom">
  <p>© 2026 ENERGYM. TODOS LOS DERECHOS RESERVADOS.</p>
  <div class="payment-icons">
    <i class="fa-brands fa-cc-visa"></i>
    <i class="fa-brands fa-cc-mastercard"></i>
    <i class="fa-brands fa-cc-paypal"></i>
    <i class="fa-brands fa-apple-pay"></i>
  </div>
</div>
</footer>`;
}

function toggleMobileNav() {
  document.querySelector('.nav-left')?.classList.toggle('nav-open');
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
    const count = Array.isArray(data.items)
      ? data.items.reduce((sum, i) => sum + (i.cantidad || 0), 0)
      : 0;
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
      updateNavbar();
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
  const stock  = p.stock ?? 0;
  const imagen = isValidImageUrl(p.imagen) ? p.imagen : '';
  const nombre = escapeHtml(p.nombre || '');
  const descripcion = escapeHtml(p.descripcion || '');
  const precio = p.precio ?? 0;
  const idProducto = p.idProducto;
  const nombreCategoria = escapeHtml(p.nombreCategoria || '');
  const liked = p.tieneLike || false;

  let badge = '';
  if (stock === 0)     badge = `<div class="prod-badge sale">AGOTADO</div>`;
  else if (stock < 5)  badge = `<div class="prod-badge">ÚLTIMAS ${stock}</div>`;

  const imgHtml = imagen
    ? `<img src="${escapeHtml(imagen)}" alt="${nombre}" style="width:100%;height:100%;object-fit:cover;"
           onerror="this.parentElement.innerHTML='<div class=\'img-placeholder prod-img\'><span>${nombre}</span></div>'">`
    : `<div class="img-placeholder prod-img"><span>${nombre}</span></div>`;

  const actions = user ? `
    <button class="wish-btn" onclick="event.stopPropagation(); toggleLike(${idProducto}, this)" title="Me gusta">
      <i class="${liked ? 'fa-solid' : 'fa-regular'} fa-heart"
         style="${liked ? 'color:var(--red)' : ''}"></i>
    </button>
    <button class="add-cart" onclick="event.stopPropagation(); addToCart(${idProducto})"
      ${stock === 0 ? 'disabled style="opacity:0.5;cursor:not-allowed;"' : ''}>
      + AGREGAR
    </button>
  ` : `
    <a href="/pages/login.html" class="add-cart" onclick="event.stopPropagation();"
       style="text-decoration:none;text-align:center;flex:1;display:flex;align-items:center;justify-content:center;">
      INICIAR SESIÓN
    </a>
  `;

  return `
  <div class="product-card hover-lift hover-shine" onclick="window.location.href='/pages/producto.html?id=${idProducto}'">
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

/* ============================================================
   ANIMACIONES ENERGYM — Motion Engine v1
   ============================================================ */

(function() {
  'use strict';

  function initRevealObserver() {
    const revealSelectors = [
      '.reveal', '.reveal-left', '.reveal-right',
      '.reveal-scale', '.reveal-blur', '.reveal-line', '.reveal-stagger'
    ];
    const elements = document.querySelectorAll(revealSelectors.join(','));
    if (!elements.length) return;

    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('revealed');
          observer.unobserve(entry.target);
        }
      });
    }, {
      threshold: 0.12,
      rootMargin: '0px 0px -40px 0px'
    });

    elements.forEach(el => observer.observe(el));
  }

  function initNavbarScroll() {
    const navbar = document.querySelector('.navbar');
    if (!navbar) return;

    let lastY = window.scrollY;
    let ticking = false;

    window.addEventListener('scroll', () => {
      if (!ticking) {
        requestAnimationFrame(() => {
          const currentY = window.scrollY;
          if (currentY > lastY && currentY > 120) {
            navbar.classList.add('nav-hidden');
          } else {
            navbar.classList.remove('nav-hidden');
          }
          if (currentY > 60) {
            navbar.classList.add('nav-solid');
          } else {
            navbar.classList.remove('nav-solid');
          }
          lastY = currentY;
          ticking = false;
        });
        ticking = true;
      }
    }, { passive: true });
  }

  function initHeroParallax() {
    const hero = document.querySelector('.hero');
    if (!hero) return;

    const heroBg = hero.querySelector('.hero-bg');
    const heroContent = hero.querySelector('.hero-content');
    const heroNumber = hero.querySelector('.hero-number');

    let ticking = false;
    window.addEventListener('scroll', () => {
      if (!ticking) {
        requestAnimationFrame(() => {
          const y = window.scrollY;
          const rate = y * 0.35;
          const opacity = Math.max(0, 1 - y / 500);

          if (heroBg) heroBg.style.transform = `translateY(${rate}px)`;
          if (heroContent) heroContent.style.opacity = opacity;
          if (heroNumber) heroNumber.style.transform = `translateY(${-y * 0.08}px)`;

          ticking = false;
        });
        ticking = true;
      }
    }, { passive: true });
  }

  function initCursorGlow() {
    if (!window.matchMedia('(hover: hover) and (pointer: fine)').matches) return;
    const glow = document.createElement('div');
    glow.className = 'cursor-glow';
    document.body.appendChild(glow);

    let mx = 0, my = 0, gx = 0, gy = 0, active = false, raf = null;

    function loop() {
      gx += (mx - gx) * 0.12;
      gy += (my - gy) * 0.12;
      glow.style.left = gx + 'px';
      glow.style.top = gy + 'px';
      raf = requestAnimationFrame(loop);
    }

    document.addEventListener('mousemove', (e) => {
      mx = e.clientX;
      my = e.clientY;
      if (!active) {
        active = true;
        glow.classList.add('active');
        raf = requestAnimationFrame(loop);
      }
    });

    document.addEventListener('mouseleave', () => {
      active = false;
      glow.classList.remove('active');
      if (raf) cancelAnimationFrame(raf);
    });
  }

  const _origUpdateCartBadge = window.updateCartBadge;
  window.updateCartBadge = async function(...args) {
    const result = await _origUpdateCartBadge.apply(this, args);
    const badge = document.querySelector('.cart-count');
    if (badge) {
      badge.classList.remove('pulse');
      void badge.offsetWidth;
      badge.classList.add('pulse');
    }
    return result;
  };

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
      initRevealObserver();
      initNavbarScroll();
      initHeroParallax();
      initCursorGlow();
    });
  } else {
    initRevealObserver();
    initNavbarScroll();
    initHeroParallax();
    initCursorGlow();
  }

  window.initRevealObserver = initRevealObserver;
})();
