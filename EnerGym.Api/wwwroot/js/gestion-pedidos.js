// ===== GESTIÓN DE PEDIDOS Y ENVÍOS =====

const ESTADOS_PEDIDO = {
    'Pendiente': { color: '#ef4444', label: 'Pendiente de Confirmación', icon: '⏳' },
    'Procesando': { color: '#f59e0b', label: 'Procesando', icon: '⚙️' },
    'Enviado': { color: '#3b82f6', label: 'Enviado', icon: '📦' },
    'En reparto': { color: '#06b6d4', label: 'En Reparto', icon: '🚚' },
    'Entregado': { color: '#22c55e', label: 'Entregado', icon: '✅' }
};

let pedidosCache = [];
let pedidoSeleccionado = null;

// ===== ADMIN - LISTAR PEDIDOS =====
async function loadPedidosAdmin(filtroEstado = '') {
    try {
        document.getElementById('pedidos-loading').style.display = 'block';
        document.getElementById('pedidos-table').style.display = 'none';

        let url = `${API_BASE}/pedidos/admin/todos`;
        if (filtroEstado) {
            url += `?estado=${encodeURIComponent(filtroEstado)}`;
        }

        const res = await fetch(url);
        if (!res.ok) throw new Error('Error al cargar pedidos');
        
        pedidosCache = await res.json();
        renderPedidosTable(pedidosCache);

        document.getElementById('pedidos-loading').style.display = 'none';
        document.getElementById('pedidos-table').style.display = 'table';

        // Cargar estadísticas
        loadEstadisticasPedidos();
    } catch (e) {
        console.error(e);
        document.getElementById('pedidos-loading').innerHTML = 
            `<p style="color:var(--texto2);padding:1rem;">Error al cargar pedidos: ${e.message}</p>`;
    }
}

function renderPedidosTable(pedidos) {
    const tbody = document.getElementById('pedidos-tbody');
    if (!pedidos || pedidos.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" style="text-align:center;color:var(--texto2);padding:2rem;">Sin pedidos</td></tr>`;
        return;
    }

    tbody.innerHTML = pedidos.map(p => {
        const estadoInfo = ESTADOS_PEDIDO[p.estado] || { color: '#999', label: p.estado, icon: '?' };
        return `
            <tr>
                <td style="color:var(--texto2);font-weight:600;">#${p.idPedido}</td>
                <td style="color:var(--blanco);">${escapeHtml(p.usuarioNombre)}</td>
                <td style="color:var(--texto2);font-size:0.85rem;">${escapeHtml(p.usuarioEmail)}</td>
                <td style="color:var(--texto2);">${new Date(p.fecha).toLocaleDateString('es-ES')}</td>
                <td style="font-family:'Bebas Neue',sans-serif;font-size:1.1rem;color:var(--naranja);">${formatPrice(p.total)}</td>
                <td style="text-align:center;">
                    <span style="display:inline-flex;align-items:center;gap:0.4rem;background:${estadoInfo.color}22;color:${estadoInfo.color};padding:0.4rem 0.8rem;border-radius:2px;font-size:0.8rem;font-weight:600;">
                        ${estadoInfo.icon} ${p.estado}
                    </span>
                </td>
                <td style="color:var(--texto2);text-align:center;">${p.cantidadProductos} items</td>
                <td>
                    <button onclick="verDetallePedidoAdmin(${p.idPedido})" class="btn-dark-sm" style="padding:0.4rem 0.8rem;font-size:0.8rem;">
                        👁️ Ver
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

// ===== ADMIN - VER DETALLE PEDIDO =====
async function verDetallePedidoAdmin(idPedido) {
    try {
        const res = await fetch(`${API_BASE}/pedidos/detalle/${idPedido}`);
        if (!res.ok) throw new Error('Pedido no encontrado');
        const data = await res.json();

        pedidoSeleccionado = {
            ...data.pedido,
            productos: data.productos || []
        };

        renderDetalleAdmin(pedidoSeleccionado);
        document.getElementById('modal-detalle-pedido').style.display = 'flex';
    } catch (e) {
        showToast(`Error: ${e.message}`, 'error');
    }
}

function renderDetalleAdmin(pedido) {
    const modal = document.getElementById('modal-detalle-pedido');
    const estadoInfo = ESTADOS_PEDIDO[pedido.estado] || { color: '#999', label: pedido.estado, icon: '?' };

    const productosHTML = (pedido.productos || []).map((p, i) => `
        <tr>
            <td style="color:var(--texto2);font-size:0.85rem;">${i + 1}</td>
            <td style="color:var(--blanco);">${escapeHtml(p.nombreProducto)}</td>
            <td style="text-align:center;color:var(--texto2);">${p.cantidad}x</td>
            <td style="font-family:'Bebas Neue',sans-serif;color:var(--naranja);">${formatPrice(p.precio)}</td>
            <td style="font-family:'Bebas Neue',sans-serif;color:var(--verde);">${formatPrice(p.cantidad * p.precio)}</td>
        </tr>
    `).join('');

    const precioTotal = (pedido.productos || []).reduce((sum, p) => sum + (p.cantidad * p.precio), 0);

    document.getElementById('detalle-numero').textContent = `#${pedido.idPedido}`;
    document.getElementById('detalle-usuario').textContent = pedido.usuarioNombre || '—';
    document.getElementById('detalle-email').textContent = pedido.usuarioEmail || '—';
    document.getElementById('detalle-fecha').textContent = new Date(pedido.fecha).toLocaleDateString('es-ES');
    document.getElementById('detalle-estado').innerHTML = `
        <span style="display:inline-flex;align-items:center;gap:0.5rem;background:${estadoInfo.color}22;color:${estadoInfo.color};padding:0.5rem 1rem;border-radius:2px;font-size:0.85rem;font-weight:600;">
            ${estadoInfo.icon} ${pedido.estado}
        </span>
    `;
    
    document.getElementById('detalle-direccion').textContent = pedido.direccionEnvio || 'No especificada';
    document.getElementById('detalle-pago').textContent = pedido.metodoPago || 'No especificado';
    document.getElementById('tabla-productos-detalle').innerHTML = productosHTML;
    document.getElementById('detalle-total').textContent = formatPrice(precioTotal);

    // Llenar selector de estados
    const selectEstado = document.getElementById('nuevo-estado-select');
    selectEstado.value = pedido.estado;

    // Mostrar/ocultar botones según estado actual
    document.getElementById('btn-marcar-procesando').style.display = 
        pedido.estado === 'Pendiente' ? 'block' : 'none';
    
    document.getElementById('btn-marcar-enviado').style.display = 
        ['Pendiente', 'Procesando'].includes(pedido.estado) ? 'block' : 'none';
    
    document.getElementById('btn-marcar-reparto').style.display = 
        ['Enviado'].includes(pedido.estado) ? 'block' : 'none';
}

async function cambiarEstadoPedido(nuevoEstado) {
    if (!pedidoSeleccionado) return;
    
    const notas = document.getElementById('notas-cambio-estado').value.trim();
    const user = getSession();
    if (!user) return;

    try {
        const res = await fetch(`${API_BASE}/pedidos/admin/${pedidoSeleccionado.idPedido}/estado`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                idPedido: pedidoSeleccionado.idPedido,
                nuevoEstado,
                notas,
                idAdmin: user.idUsuario
            })
        });

        const data = await res.json();
        if (!res.ok) throw new Error(data.error || 'Error al cambiar estado');

        showToast('Estado actualizado correctamente ✓', 'success');
        pedidoSeleccionado.estado = nuevoEstado;
        document.getElementById('notas-cambio-estado').value = '';
        renderDetalleAdmin(pedidoSeleccionado);
        
        // Recargar lista
        setTimeout(() => {
            loadPedidosAdmin();
        }, 500);
    } catch (e) {
        showToast(`Error: ${e.message}`, 'error');
    }
}

// Callbacks para botones rápidos
async function marcarProcesando() { await cambiarEstadoPedido('Procesando'); }
async function marcarEnviado() { await cambiarEstadoPedido('Enviado'); }
async function marcarEnReparto() { await cambiarEstadoPedido('En reparto'); }
async function marcarEntregado() { await cambiarEstadoPedido('Entregado'); }

// ===== CARGADOR DE ESTADÍSTICAS =====
async function loadEstadisticasPedidos() {
    try {
        const res = await fetch(`${API_BASE}/pedidos/admin/estadisticas`);
        if (!res.ok) return;
        const stats = await res.json();

        document.getElementById('stat-total').textContent = stats.totalPedidos;
        document.getElementById('stat-pendientes').textContent = stats.pendientesConfirmacion;
        document.getElementById('stat-proceso').textContent = stats.enProceso;
        document.getElementById('stat-enviados').textContent = stats.enviados;
        document.getElementById('stat-entregados').textContent = stats.entregados;
        document.getElementById('stat-ventas').textContent = formatPrice(stats.ventasTotal);
        document.getElementById('stat-promedio').textContent = formatPrice(stats.promedioVenta);
    } catch (e) {
        console.error('Error cargando estadísticas:', e);
    }
}

// ===== MODAL CERRAR =====
function cerrarModalDetalle() {
    document.getElementById('modal-detalle-pedido').style.display = 'none';
    pedidoSeleccionado = null;
}

// Cerrar modal al pulsar fuera
document.addEventListener('click', function(e) {
    const modal = document.getElementById('modal-detalle-pedido');
    if (modal && e.target === modal) {
        cerrarModalDetalle();
    }
});

// ===== USUARIO - VER HISTORIAL =====
async function loadPedidosUsuario() {
    try {
        const user = getSession();
        if (!user) {
            window.location.href = '/pages/login.html';
            return;
        }

        document.getElementById('mis-pedidos-loading').style.display = 'block';
        document.getElementById('mis-pedidos-list').style.display = 'none';

        const res = await fetch(`${API_BASE}/pedidos/${user.idUsuario}`);
        if (!res.ok) throw new Error('Error al cargar pedidos');
        
        const pedidos = await res.json();
        renderMisPedidos(pedidos);

        document.getElementById('mis-pedidos-loading').style.display = 'none';
        document.getElementById('mis-pedidos-list').style.display = 'block';
    } catch (e) {
        document.getElementById('mis-pedidos-loading').innerHTML = 
            `<p style="color:var(--texto2);padding:1rem;">Error: ${e.message}</p>`;
    }
}

function renderMisPedidos(pedidos) {
    const container = document.getElementById('mis-pedidos-list');
    
    if (!pedidos || pedidos.length === 0) {
        container.innerHTML = `
            <div style="text-align:center;padding:3rem 1rem;color:var(--texto2);">
                <p style="font-size:1.1rem;margin-bottom:0.5rem;">📭 No tienes pedidos aún</p>
                <p style="font-size:0.9rem;">¡Comienza a comprar en nuestra tienda!</p>
                <a href="/pages/tienda.html" class="btn-fire" style="display:inline-block;margin-top:1rem;clip-path:none;border-radius:2px;">
                    Ver Tienda
                </a>
            </div>
        `;
        return;
    }

    container.innerHTML = pedidos.map(p => {
        const estadoInfo = ESTADOS_PEDIDO[p.estado] || { color: '#999', label: p.estado, icon: '?' };
        return `
            <div style="background:var(--gris);border:1px solid var(--borde);margin-bottom:1rem;overflow:hidden;border-radius:4px;">
                <div style="display:grid;grid-template-columns:1fr auto auto;gap:1rem;align-items:center;padding:1rem;border-bottom:1px solid var(--borde);">
                    <div>
                        <div style="font-family:'Bebas Neue',sans-serif;font-size:1.2rem;letter-spacing:2px;color:var(--blanco);margin-bottom:0.3rem;">
                            #${p.idPedido}
                        </div>
                        <div style="color:var(--texto2);font-size:0.85rem;">
                            ${new Date(p.fecha).toLocaleDateString('es-ES', { year: 'numeric', month: 'long', day: 'numeric' })}
                        </div>
                    </div>
                    <div style="text-align:right;">
                        <div style="font-family:'Bebas Neue',sans-serif;font-size:1.4rem;color:var(--naranja);">
                            ${formatPrice(p.total)}
                        </div>
                    </div>
                    <div style="text-align:center;">
                        <span style="display:inline-flex;align-items:center;gap:0.4rem;background:${estadoInfo.color}22;color:${estadoInfo.color};padding:0.5rem 0.8rem;border-radius:2px;font-size:0.75rem;font-weight:600;text-transform:uppercase;">
                            ${estadoInfo.icon} ${p.estado}
                        </span>
                    </div>
                </div>
                <div style="padding:1rem;display:flex;gap:0.5rem;">
                    <a href="/pages/pedido.html?id=${p.idPedido}" class="btn-fire" style="flex:1;text-align:center;clip-path:none;border-radius:2px;text-decoration:none;">
                        👁️ Ver Detalles
                    </a>
                    ${['Enviado', 'En reparto'].includes(p.estado) ? `
                        <button onclick="confirmarMiEntrega(${p.idPedido})" class="btn-fire" style="flex:1;clip-path:none;border-radius:2px;">
                            ✅ Recibí mi pedido
                        </button>
                    ` : ''}
                </div>
            </div>
        `;
    }).join('');
}

// ===== USUARIO - CONFIRMAR ENTREGA =====
async function confirmarMiEntrega(idPedido) {
    const user = getSession();
    if (!user) return;

    if (!confirm('¿Confirmas que has recibido tu pedido?')) return;

    try {
        const res = await fetch(`${API_BASE}/pedidos/${idPedido}/confirmar-entrega`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                idPedido,
                idUsuario: user.idUsuario
            })
        });

        const data = await res.json();
        if (!res.ok) throw new Error(data.error || 'Error al confirmar entrega');

        showToast('✅ Entrega confirmada correctamente', 'success');
        loadPedidosUsuario();
    } catch (e) {
        showToast(`Error: ${e.message}`, 'error');
    }
}

// ===== FILTROS ADMIN =====
function filtrarPorEstado(estado) {
    // Marcar filtro activo
    document.querySelectorAll('.filtro-estado').forEach(btn => btn.classList.remove('active'));
    if (event && event.target) {
        event.target.classList.add('active');
    }
    
    loadPedidosAdmin(estado);
}
