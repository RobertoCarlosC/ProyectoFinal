# 🏗️ Arquitectura del Sistema de Gestión de Pedidos

## Diagrama de Flujo

```
┌─────────────────────────────────────────────────────────────────────┐
│                     SISTEMA EnerGym - PEDIDOS                       │
└─────────────────────────────────────────────────────────────────────┘

                    ┌──────────────────┐
                    │   BASE DE DATOS  │
                    │   (SQL Server)   │
                    └────────┬─────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
    ┌────▼─────┐      ┌─────▼──────┐    ┌──────▼─────┐
    │  Pedidos  │      │ Historial  │    │ Usuarios   │
    │  (NEW)    │      │ Estados    │    │            │
    │  Campos:  │      │ (NEW)      │    │  (EXIST)   │
    │  - Fecha  │      │            │    └────────────┘
    │  - Estado │      │  Auditoría │
    │  - Confir │      │  completa  │
    │  mación   │      │            │
    └────┬──────┘      └─────┬──────┘
         │                   │
         └───────────────────┘
                 │
    ┌────────────┴────────────┐
    │                         │
    │    C# .NET API (8.0)   │
    │    Backend             │
    │                         │
    │  ┌──────────────────┐  │
    │  │ PedidosController│  │
    │  ├──────────────────┤  │
    │  │ GET /admin/todos │◄─┼─── Admin: todos pedidos
    │  │ POST admin/      │  │
    │  │  {id}/estado     │◄─┼─── Admin: cambiar estado
    │  │ GET admin/stats  │◄─┼─── Admin: estadísticas
    │  │ POST {id}/       │  │
    │  │  confirmar-entrega◄─┤─── Usuario: confirmar
    │  │ GET {id}/        │  │
    │  │  historial       │◄─┼─── Historial cambios
    │  └──────────────────┘  │
    │                         │
    └────────────┬────────────┘
                 │
    ┌────────────┴────────────┐
    │                         │
    │   Frontend SPA          │
    │   (HTML/CSS/JS)         │
    │                         │
    ├─────────────────────────┤
    │                         │
    │ ADMIN PANEL             │ USER PANEL
    │ ───────────────         │ ──────────
    │                         │
    │ /pages/admin.html       │ /pages/perfil.html
    │ ┌───────────────────┐   │ ┌─────────────────┐
    │ │ Pestaña PEDIDOS   │   │ │ Mis Pedidos      │
    │ │ (⭐ NUEVA)        │   │ │                 │
    │ ├───────────────────┤   │ ├─────────────────┤
    │ │ • Estadísticas    │   │ │ • Historial     │
    │ │ • Filtros Estado  │   │ │ • Detalles      │
    │ │ • Tabla Pedidos   │   │ │ • Timeline Envío│
    │ │ • Modal Detalles  │   │ │ • Botón Confirmar
    │ │ • Cambiar Estado  │   │ │   (cuando está  │
    │ │ • Ver Historial   │   │ │    en reparto)  │
    │ │                   │   │ │                 │
    │ └───────────────────┘   │ └─────────────────┘
    │                         │
    │ /pages/admin.html       │ /pages/pedido.html
    │ Modal: detalle pedido   │ Detalle completo pedido
    │ (30KB)                  │ Timeline visual
    │                         │
    └─────────────────────────┘
             │
    ┌────────┴─────────┐
    │                  │
    │ SCRIPTS JS       │
    │ ───────────────  │
    │                  │
    │ /js/app.js       │
    │ • Funciones base │
    │ • formatPrice()  │
    │ • escapeHtml()   │
    │ • showToast()    │
    │ • getSession()   │
    │                  │
    │ /js/gestion-     │
    │     pedidos.js   │
    │ • loadPedidosAdmin()
    │ • verDetallePedidoAdmin()
    │ • cambiarEstadoPedido()
    │ • confirmarMiEntrega()
    │ • Filtros estado │
    │ • Renderizado    │
    │                  │
    └──────────────────┘
```

---

## Estados del Pedido

```
                    Flujo Normal
                    ──────────

Pendiente ──► Procesando ──► Enviado ──► En Reparto ──► Entregado
   1               2            3            4              5
   │               │            │            │              │
   │        Admin cambia      Admin         Admin         Usuario
   │        automáticamente   envía       notifica       confirma
   └────────────────────────────────────────────────────────┘
                        Auditado en
               PedidoHistorialEstados


    Tablero Admin viendo estados:

    ⏳ Pendiente (Rojo)     → 5 pedidos
    ⚙️  Procesando (Naranja) → 3 pedidos
    📦 Enviado (Azul)       → 2 pedidos
    🚚 En Reparto (Cian)    → 4 pedidos
    ✅ Entregado (Verde)    → 28 pedidos
    ─────────────────────────────────
    Total                    → 42 pedidos
```

---

## Modelo de Datos

```
┌─────────────────────────────────┐
│          PEDIDOS (Existing)     │
├─────────────────────────────────┤
│ IdPedido (PK)                   │
│ IdUsuario (FK) → Usuarios       │
│ Fecha                           │
│ Total                           │
│ Estado (NEW LOGIC)              │
│ DireccionEnvio                  │
│ MetodoPago                      │
│ FechaConfirmacionEntrega (NEW)  │◄─ Usuario confirma aquí
│ FechaActualizacion (NEW)        │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ PEDIDO_HISTORIAL_ESTADOS (NEW)  │
├─────────────────────────────────┤
│ IdHistorial (PK)                │
│ IdPedido (FK) → Pedidos         │
│ EstadoAnterior                  │
│ EstadoNuevo                     │
│ Fecha                           │
│ CambiadoPor (Usuario/Admin ID)  │
│ Notas (opcional)                │
└─────────────────────────────────┘
      ▲
      │
   Audit Trail: Cada cambio se registra automáticamente
```

---

## Componentes Frontend

```
admin.html (⭐ Nueva sección)
│
├─── Tab Pedidos (⭐ NUEVO)
│    ├─── Estadísticas Dashboard
│    │    ├─ Total Pedidos
│    │    ├─ Pendientes
│    │    ├─ En Proceso
│    │    ├─ Enviados
│    │    ├─ Entregados
│    │    ├─ Ventas Total
│    │    └─ Promedio Venta
│    │
│    ├─── Filtros (buttons)
│    │    ├─ Todos
│    │    ├─ Pendiente
│    │    ├─ Procesando
│    │    ├─ Enviado
│    │    └─ Entregado
│    │
│    ├─── Tabla Pedidos
│    │    ├─ #Pedido
│    │    ├─ Cliente
│    │    ├─ Email
│    │    ├─ Fecha
│    │    ├─ Total
│    │    ├─ Estado (badge color)
│    │    ├─ Items
│    │    └─ Botón "Ver"
│    │
│    └─── Modal Detalles (⭐ NUEVO)
│         ├─ Header: #Pedido, Cliente, Fecha, Estado
│         ├─ Tabla Productos
│         ├─ Resumen (Dirección, Pago)
│         ├─ Cambiar Estado
│         │  ├─ Dropdown selector
│         │  ├─ Notas textarea
│         │  └─ Botón guardar
│         └─ Botones rápidos
│            ├─ ⚙️ Procesando
│            ├─ 📦 Enviado
│            └─ 🚚 En Reparto
│
├─── Tab Productos (Existing)
├─── Tab Usuarios (Existing)
│
└─── Navegación Sidebar
     ├─ 📦 Pedidos (active)
     ├─ 📦 Productos
     ├─ 👥 Usuarios
     └─ Cerrar Sesión


pedido.html (USER VIEW - MEJORADO)
│
├─── Header Pedido
│    ├─ #Número
│    ├─ Estado (badge)
│    └─ Fecha
│
├─── Sección Principal
│    └─ Tabla de Productos
│        ├─ Imagen
│        ├─ Nombre
│        ├─ Cantidad
│        └─ Precio
│
└─── Sidebar (MEJORADO)
     ├─ Resumen
     │  ├─ Subtotal
     │  ├─ Envío
     │  ├─ Impuestos
     │  └─ Total
     │
     ├─ Info Envío
     │  ├─ Dirección
     │  └─ Método Pago
     │
     ├─── Timeline Estados (⭐ NUEVO)
     │    │
     │    ├─ ⏳ Pendiente
     │    ├─ ⚙️  Procesando
     │    ├─ 📦 Enviado
     │    ├─ 🚚 En Reparto
     │    └─ ✅ Entregado (filled)
     │
     ├─── Botón Confirmar (⭐ NUEVO)
     │    └─ "✅ He recibido mi pedido"
     │       (visible solo si estado es
     │        "Enviado" o "En Reparto")
     │
     └─ Botones
        ├─ Volver a mis pedidos
        └─ Seguir comprando
```

---

## Flujo de Cambio de Estado

```
Admin click en "Ver" pedido
          │
          ▼
Modal detalle se abre
          │
          ├──► Dropdown selector "Nuevo Estado"
          │           │
          │           ▼
          │    Admin selecciona estado
          │           │
          │    Opción: agregar nota
          │           │
          │           ▼
          └──► Click "💾 Guardar Cambio"
                      │
                      ▼
              POST /api/pedidos/admin/{id}/estado
              Body: {
                idPedido,
                nuevoEstado,
                notas,
                idAdmin
              }
                      │
                      ▼
              Backend valida:
              ✓ Usuario es admin
              ✓ Estado válido
              ✓ Pedido existe
                      │
                      ▼
              Actualiza tabla Pedidos:
              - Estado = nuevoEstado
              - FechaActualizacion = NOW()
                      │
                      ▼
              Inserta en PedidoHistorialEstados:
              - EstadoAnterior
              - EstadoNuevo
              - Fecha
              - CambiadoPor = "Admin_123"
              - Notas
                      │
                      ▼
              Retorna OK
                      │
                      ▼
              Frontend muestra toast: "Estado actualizado ✓"
                      │
                      ▼
              Modal se refresca con nuevo estado
                      │
                      ▼
              Tabla se actualiza automatically
```

---

## Flujo de Confirmación por Usuario

```
Usuario ve pedido en estado "Enviado"
          │
          ▼
Página pedido.html muestra:
"✅ He recibido mi pedido"
          │
          ▼
Usuario click en botón
          │
          ▼
Confirmación: "¿Confirmas que has recibido?"
          │
          ├─ NO ──► Cancelar
          │
          └─ SÍ ──► POST /api/pedidos/{id}/confirmar-entrega
                         Body: { idPedido, idUsuario }
                         │
                         ▼
                  Backend valida:
                  ✓ Usuario dueño del pedido
                  ✓ Estado es Enviado o En Reparto
                         │
                         ▼
                  Actualiza Pedidos:
                  - Estado = "Entregado"
                  - FechaConfirmacionEntrega = NOW()
                         │
                         ▼
                  Inserta Historial:
                  - "Enviado" → "Entregado"
                  - CambiadoPor = "Usuario_456"
                         │
                         ▼
                  Retorna OK
                         │
                         ▼
                  Toast: "Entrega confirmada ✓"
                         │
                         ▼
                  Recarga página
                         │
                         ▼
                  Botón desaparece
                  Timeline muestra "Entregado" ✅
```

---

## Validaciones de Seguridad

```
┌─────────────────────────────────────────────────────────┐
│           CONTROL DE ACCESO Y PERMISOS                  │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ ADMIN                        │  USUARIO                 │
│ ─────────────────────────────┼───────────────────────── │
│                              │                          │
│ ✅ Ver todos los pedidos     │  ❌ Ver todos pedidos   │
│ ✅ Filtrar por estado        │  ✅ Ver solo sus pedidos│
│ ✅ Ver detalles de N         │  ✅ Ver detalles de   │
│    pedidos                   │     sus pedidos         │
│ ✅ Cambiar estado            │  ❌ Cambiar estado      │
│    (Pendiente → ...)         │     (bloqueado)         │
│ ✅ Agregar notas             │  ❌ Agregar notas      │
│ ✅ Ver historial de cambios  │  ❌ Ver historial      │
│ ✅ Ver estadísticas          │  ❌ Ver estadísticas   │
│ ✅ Filtrar por usuario       │  ❌ Filtrar usuarios   │
│                              │                          │
│                              │  ✅ Confirmar entrega   │
│                              │     (solo si "Enviado") │
│                              │  ✅ Ver timeline envío  │
│                              │  ✅ Ver productos       │
│                              │  ✅ Ver total           │
│                              │                          │
└─────────────────────────────────────────────────────────┘
```

---

## Estándares de Código

```
├─ Lenguajes: C#, SQL, JavaScript, HTML, CSS
├─ Patrón: MVC Backend + SPA Frontend
├─ DB: SQL Server
├─ API: RESTful JSON
│
├─ Backend Estructura:
│  ├─ Controllers (manejo de requests)
│  ├─ Models (DTOs de datos)
│  ├─ Database (conexión SQL)
│  └─ Validación en cada endpoint
│
├─ Frontend Estructura:
│  ├─ HTML semántico
│  ├─ CSS inline (consistente con proyecto existente)
│  ├─ JavaScript vanilla (sin frameworks)
│  ├─ Manejo de errores con try-catch
│  └─ Feedback visual (toasts)
│
└─ Seguridad:
   ├─ Validación de roles (admin vs usuario)
   ├─ SQL Parameters (anti-injection)
   ├─ Auditoría en BD (historial)
   ├─ Scope de datos (ver solo lo permitido)
   └─ Métodos HTTP correctos (GET, POST, PUT)
```

---

## Archivos Generados/Modificados

```
Generados:
├─ 📄 wwwroot/js/gestion-pedidos.js (8KB, 260 líneas)
├─ 📄 SISTEMA_GESTION_PEDIDOS.md (documentación)
└─ 📄 INICIO_RAPIDO.md (guía rápida)

Modificados:
├─ 📊 EnerGymDB_Setup.sql (+20 líneas: nuevos campos)
├─ 🏗️  Models/Models.cs (+60 líneas: 6 DTOs nuevos)
├─ 🎮 Controllers/PedidosController.cs (+350 líneas, 5 endpoints)
├─ 🎨 pages/admin.html (Tab NUEVO + Modal + Estadísticas)
├─ 📄 pages/pedido.html (Timeline + Botón confirmación)
└─ 👤 pages/perfil.html (corrección de endpoint)

Total: +800 líneas de código nuevo
```

---

**¡Sistema de gestión de pedidos completamente integrado! 🎉**
