# Documentación Funcional Completa — EnerGym

> Versión: 1.0 | Fecha: 2026-04-30  
> Esta documentación describe todas las funcionalidades de la aplicación EnerGym, el flujo de datos entre frontend y backend, los archivos involucrados y los endpoints de la API.

---

## Índice

1. [Visión General de la Arquitectura](#1-visión-general-de-la-arquitectura)
2. [Autenticación y Sesión](#2-autenticación-y-sesión)
3. [Gestión de Usuarios y Perfiles](#3-gestión-de-usuarios-y-perfiles)
4. [Catálogo de Productos y Likes](#4-catálogo-de-productos-y-likes)
5. [Carrito de Compras](#5-carrito-de-compras)
6. [Pedidos y Estados](#6-pedidos-y-estados)
7. [Panel de Administración](#7-panel-de-administración)
8. [Soporte — Mensajes](#8-soporte--mensajes)
9. [Páginas del Frontend y Flujos de Usuario](#9-páginas-del-frontend-y-flujos-de-usuario)
10. [Resumen de Archivos por Funcionalidad](#10-resumen-de-archivos-por-funcionalidad)

---

## 1. Visión General de la Arquitectura

EnerGym es una aplicación web de comercio electrónico de suplementación deportiva compuesta por:

| Capa | Tecnología | Descripción |
|------|------------|-------------|
| **Backend** | .NET 8 Web API | API REST sin Entity Framework; usa ADO.NET directo (`Microsoft.Data.SqlClient`) |
| **Frontend** | HTML + CSS + Vanilla JS | Sitio multi-página estático; diseño "dark premium" con un único `app.js` y `styles.css` compartidos |
| **Base de datos** | SQL Server (Somee.com) | Esquema relacional con tablas para usuarios, productos, carritos, pedidos, likes y mensajes |
| **Seguridad** | BCrypt.Net | Hash de contraseñas. Sin JWT ni cookies; el frontend envía `idUsuario` explícitamente |
| **Autorización** | Verificación manual por rol | Cada endpoint de admin consulta la BD para validar `IdRol == 1` |

**Flujo general de una petición:**

```
frontend (HTML/JS)  →  POST/GET/PUT/DELETE /api/...  →  .NET Controller  →  SQL Server
       ↑                                                                      ↓
   localStorage                     JSON response  ←  SqlDataReader / SqlCommand
```

---

## 2. Autenticación y Sesión

### 2.1 Registro de nuevos usuarios

**Archivos involucrados:**
- Backend: `EnerGym.Api/Controllers/AuthController.cs`
- Frontend: `frontend/pages/login.html`
- DB: `EnerGym.Api/EnerGymDB_Setup.sql` (tabla `Usuarios`)

**Flujo:**

| Paso | Archivo / Endpoint | Descripción |
|------|-------------------|-------------|
| 1 | `login.html` | El usuario rellena nombre, email y contraseña (mín. 6 caracteres) |
| 2 | `AuthController.cs` → `POST api/register` | Recibe `RegisterDto { Nombre, Email, Password }` |
| 3 | `AuthController.cs` | Valida campos, comprueba que el email no exista ya |
| 4 | `AuthController.cs` | Hashea la contraseña con **BCrypt** |
| 5 | `AuthController.cs` | Inserta en `Usuarios` con **`IdRol = 2`** (cliente por defecto) |
| 6 | `login.html` | Muestra mensaje de éxito y cambia automáticamente a la pestaña de login |

> **Importante:** No es posible registrarse como administrador desde el frontend. El único admin inicial es `admin@energym.es`.

---

### 2.2 Login y cierre de sesión

**Archivos involucrados:**
- Backend: `EnerGym.Api/Controllers/AuthController.cs`
- Frontend: `frontend/pages/login.html`, `frontend/js/app.js`

**Flujo:**

| Paso | Archivo / Endpoint | Descripción |
|------|-------------------|-------------|
| 1 | `login.html` | El usuario introduce email y contraseña |
| 2 | `AuthController.cs` → `POST api/login` | Recibe `LoginDto { Email, Password }` |
| 3 | `AuthController.cs` | Busca el usuario por email y verifica el hash BCrypt |
| 4 | `AuthController.cs` | Devuelve JSON: `{ idUsuario, nombre, email, idRol }` |
| 5 | `login.html` / `app.js` | Llama a `setSession(data)` → guarda en **`localStorage`** como `energym_user` |
| 6 | `login.html` | Redirige según rol: `idRol === 1` → `/pages/admin.html`; otro → `/index.html` |

**Cierre de sesión:**
- Función `logout()` en `app.js` elimina `energym_user` y `energym_cart_count` de `localStorage`, luego recarga la página.

---

### 2.3 Gestión de sesión en el frontend

**Archivos involucrados:** `frontend/js/app.js`

| Función | Qué hace |
|---------|----------|
| `getSession()` | Lee `energym_user` de `localStorage` y lo parsea como JSON |
| `setSession(user)` | Guarda el objeto usuario en `localStorage` |
| `clearSession()` | Borra `energym_user` y `energym_cart_count` |
| `requireLogin()` | Si no hay sesión, redirige a `/pages/login.html` |
| `requireAdmin()` | Si no hay sesión o `idRol !== 1`, redirige a `/index.html` |
| `updateNavbar()` | Según sesión: muestra nombre de usuario, link a admin o perfil, badge del carrito, botón de salir |

---

## 3. Gestión de Usuarios y Perfiles

### 3.1 Ver y editar perfil (cliente)

**Archivos involucrados:**
- Backend: `EnerGym.Api/Controllers/UsuariosController.cs`
- Frontend: Referenciado en `app.js` pero página `perfil.html` **no existe aún**
- Modelos: `EditarPerfilDto`, `CambiarContraseñaDto`

**Endpoints:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/usuarios/{idUsuario}/perfil` | Devuelve datos completos del perfil (nombre, email, teléfono, dirección, ciudad, CP, foto) |
| `PUT` | `/api/usuarios/{idUsuario}/editar-perfil` | Actualiza los datos del perfil. Valida que el email sea único |
| `PUT` | `/api/usuarios/{idUsuario}/cambiar-contraseña` | Verifica contraseña actual con BCrypt, luego actualiza con la nueva |
| `GET` | `/api/usuarios/{idUsuario}/pedidos` | Lista todos los pedidos del usuario |
| `GET` | `/api/usuarios/{idUsuario}/pedidos/{idPedido}` | Devuelve un pedido concreto si pertenece al usuario |

**Flujo típico:**
```
perfil.html (no existe)  →  GET /api/usuarios/{id}/perfil  →  Muestra datos
       ↓
Editar datos  →  PUT /api/usuarios/{id}/editar-perfil  →  Guarda cambios
       ↓
Cambiar pass  →  PUT /api/usuarios/{id}/cambiar-contraseña  →  Actualiza hash
```

---

### 3.2 Gestión de usuarios (admin)

**Archivos involucrados:** `EnerGym.Api/Controllers/AdminController.cs`

**Endpoints protegidos** (todos verifican `EsAdmin(idAdmin)`):

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/admin/{idAdmin}/usuarios/todos` | Lista todos los usuarios |
| `PUT` | `/api/admin/{idAdmin}/usuarios/{idUsuario}/editar` | Edita nombre, email, rol, teléfono, dirección, ciudad, CP |
| `DELETE` | `/api/admin/{idAdmin}/usuarios/{idUsuario}` | Elimina usuario (no permite auto-eliminación). Borra primero su carrito y likes |
| `GET` | `/api/admin/{idAdmin}/usuarios/{idUsuario}/carrito` | Ver el carrito de otro usuario |

---

## 4. Catálogo de Productos y Likes

### 4.1 Catálogo de productos

**Archivos involucrados:**
- Backend: `EnerGym.Api/Controllers/ProductosController.cs`
- Frontend: `frontend/pages/tienda.html`, `frontend/index.html`, `frontend/pages/producto.html`
- DB: tablas `Productos`, `Categorias`, `ProductoImagenes`

**Endpoints:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/productos` | Lista todos los productos con nombre de categoría. Si se pasa `?idUsuario`, indica si el usuario dio like (`TieneLike`) |
| `GET` | `/api/productos/categorias` | Lista todas las categorías disponibles |
| `GET` | `/api/productos/{id}` | Devuelve un producto concreto + imágenes adicionales de `ProductoImagenes` |
| `POST` | `/api/productos` | Crea un producto (usado también por admin) |
| `PUT` | `/api/productos/{id}` | Actualiza un producto |
| `DELETE` | `/api/productos/{id}` | Elimina un producto y sus dependencias (imágenes, likes, carrito) |

**Flujo en la tienda (`tienda.html`):**

| Paso | Acción |
|------|--------|
| 1 | Carga categorías → genera botones de filtro |
| 2 | Carga productos → guarda en variable `todosLosProductos` |
| 3 | Renderiza grid con `renderProductCard()` de `app.js` |
| 4 | Al filtrar por categoría, filtra en memoria y re-renderiza |

**Flujo en ficha de producto (`producto.html`):**

| Paso | Acción |
|------|--------|
| 1 | Lee `?id=X` de la URL |
| 2 | GET `/api/productos/{id}` (con `?idUsuario` si hay sesión) |
| 3 | Muestra carrusel de imágenes, info, stock y descripción |
| 4 | Permite seleccionar cantidad y añadir al carrito |
| 5 | Permite dar/quitar like con el botón de corazón |

---

### 4.2 Sistema de Likes (Favoritos)

**Archivos involucrados:** `EnerGym.Api/Controllers/LikesController.cs`, `frontend/js/app.js`

**Endpoints:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/likes/toggle` | Activa o desactiva el like de un usuario sobre un producto. Devuelve `{ liked: true/false }` |
| `GET` | `/api/likes/producto/{idProducto}/count` | Devuelve el número total de likes de un producto |
| `GET` | `/api/likes/{idUsuario}` | Lista todos los productos que el usuario ha marcado como favoritos |

**Flujo:**
```
Usuario clicka ♥ en producto.html / tienda.html / index.html
       ↓
POST /api/likes/toggle  { idUsuario, idProducto }
       ↓
Backend inserta o borra de LikesProductos (UNIQUE por usuario+producto)
       ↓
Frontend cambia clase CSS del botón (relleno/vacío) según respuesta
```

---

## 5. Carrito de Compras

**Archivos involucrados:**
- Backend: `EnerGym.Api/Controllers/CarritoController.cs`
- Frontend: `frontend/js/app.js` (funciones de carrito), página `carrito.html` **no existe aún**
- DB: tablas `Carritos`, `CarritoProductos`

**Endpoints:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/carrito/{idUsuario}` | Obtiene o **crea** el carrito del usuario. Devuelve items con subtotales y total |
| `POST` | `/api/carrito/agregar` | Añade producto al carrito. Si ya existe, suma cantidad. Valida stock disponible |
| `PUT` | `/api/carrito/item/{id}` | Actualiza cantidad de un item. Si cantidad ≤ 0, elimina el item. Valida stock |
| `DELETE` | `/api/carrito/item/{id}` | Elimina un item del carrito |

**Modelo de datos:**
- `Carritos`: un carrito por usuario (`IdUsuario` es UNIQUE)
- `CarritoProductos`: líneas del carrito con `IdCarrito`, `IdProducto`, `Cantidad`

**Flujo frontend (desde `app.js`):**

| Función | Qué hace |
|---------|----------|
| `addToCart(idProducto)` | POST a `/api/carrito/agregar` con cantidad 1. Muestra toast y actualiza badge |
| `updateCartBadge()` | GET carrito del usuario, suma cantidades, guarda en `localStorage` como `energym_cart_count` y actualiza el DOM |

> Nota: Aunque `carrito.html` no existe, los estilos para el carrito ya están definidos en `styles.css` (`.cart-layout`, `.cart-panel`, `.qty-ctrl`, etc.).

---

## 6. Pedidos y Estados

### 6.1 Crear un pedido (checkout)

**Archivos involucrados:**
- Backend: `EnerGym.Api/Controllers/PedidosController.cs`
- Frontend: Referenciado pero página de checkout **no existe aún**
- DB: tablas `Pedidos`, `PedidoProductos`, `Carritos`, `CarritoProductos`, `Productos`

**Endpoint:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/pedidos/confirmar` | Convierte el carrito del usuario en un pedido. Recibe `ConfirmarPedidoDto { IdUsuario, DireccionEnvio?, MetodoPago? }` |

**Flujo del backend al confirmar:**

| Paso | Acción |
|------|--------|
| 1 | Inicia transacción SQL |
| 2 | Obtiene el carrito del usuario |
| 3 | Valida que haya items y que haya stock suficiente para cada producto |
| 4 | Crea el pedido en `Pedidos` con estado "Pendiente" |
| 5 | Inserta líneas en `PedidoProductos` con precio snapshot |
| 6 | Descuenta el stock de cada producto |
| 7 | Elimina todos los items del carrito (vacía el carrito) |
| 8 | Confirma la transacción |

---

### 6.2 Ver pedidos (cliente)

**Archivos involucrados:** `frontend/pages/pedido.html` (detalle), `frontend/js/app.js`

**Endpoints del cliente:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/pedidos/{idUsuario}` | Lista resumen de pedidos del usuario |
| `GET` | `/api/pedidos/detalle/{idPedido}` | Devuelve pedido completo con productos, imágenes, dirección, método de pago |
| `POST` | `/api/pedidos/{idPedido}/confirmar-entrega` | El usuario confirma que recibió el pedido (solo si está en estado "Enviado" o "En reparto"). Registra en historial |

**Flujo en `pedido.html`:**

| Paso | Acción |
|------|--------|
| 1 | Requiere sesión (`requireLogin`) |
| 2 | Lee `?id=X` de la URL |
| 3 | GET `/api/pedidos/detalle/{idPedido}` |
| 4 | Muestra skeleton loader mientras carga |
| 5 | Renderiza: productos con imágenes, estado con color, resumen de precios (subtotal, envío, impuestos 21%, total), dirección y método de pago |

**Lógica de envío (calculada en frontend):**
- Envío gratis si total > 50€
- Si no, envío = 5.99€
- Impuestos = 21% del subtotal

---

### 6.3 Gestión de pedidos (admin)

**Archivos involucrados:** `EnerGym.Api/Controllers/PedidosController.cs`, `EnerGym.Api/Controllers/AdminController.cs`

**Endpoints admin:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/pedidos/admin/todos` | Lista todos los pedidos con filtros opcionales (`?estado=`, `?idUsuario=`) |
| `GET` | `/api/pedidos/admin/estadisticas` | Estadísticas agregadas: total, pendientes, en proceso, enviados, entregados, ventas totales, promedio |
| `GET` | `/api/pedidos/admin/ventas-por-dia` | Ventas diarias de los últimos N días (por defecto 14). Usa CTE recursiva en SQL |
| `POST` | `/api/pedidos/admin/{idPedido}/estado` | Cambia el estado de un pedido. Registra el cambio en `PedidoHistorialEstados` |
| `GET` | `/api/pedidos/{idPedido}/historial` | Ver historial de cambios de estado de un pedido |

**Estados posibles de un pedido:**
- `Pendiente`
- `En proceso`
- `Enviado` / `En reparto`
- `Entregado`
- `Cancelado`

**Tabla `PedidoHistorialEstados`:**
- Guarda: estado anterior, estado nuevo, fecha del cambio, quién lo cambió (`CambiadoPor`), y notas.

---

## 7. Panel de Administración

### 7.1 Productos (admin)

**Archivos involucrados:** `EnerGym.Api/Controllers/AdminController.cs`

**Endpoints:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/admin/{idAdmin}/productos/todos` | Lista productos con conteo de likes |
| `POST` | `/api/admin/{idAdmin}/productos/crear` | Crea producto. Recibe `CrearProductoAdminDto` |
| `PUT` | `/api/admin/{idAdmin}/productos/{idProducto}/editar` | Actualiza producto. Recibe `EditarProductoAdminDto` |
| `DELETE` | `/api/admin/{idAdmin}/productos/{idProducto}` | Elimina producto y sus dependencias (likes, carrito, imágenes) |

---

### 7.2 Usuarios (admin)

Ver sección **3.2 Gestión de usuarios (admin)**.

---

### 7.3 Pedidos (admin)

Ver sección **6.3 Gestión de pedidos (admin)**.

---

### 7.4 Estadísticas generales (admin)

**Endpoint:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/admin/{idAdmin}/estadisticas` | Devuelve: total usuarios, total pedidos, ingresos totales, total productos, productos sin stock, distribución de pedidos por estado |

---

## 8. Soporte — Mensajes

**Archivos involucrados:** `EnerGym.Api/Controllers/MensajesController.cs`
- DB: tabla `MensajesSoporte`

**Endpoints:**

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `POST` | `/api/mensajes` | Crea un mensaje de soporte. Puede ser anónimo (`IdUsuario` nullable) o de un usuario logueado |
| `GET` | `/api/mensajes` | Lista **todos** los mensajes (para el panel de admin). Incluye campo `Respuesta` |
| `GET` | `/api/mensajes/usuario/{idUsuario}` | Lista mensajes de un usuario concreto |
| `PUT` | `/api/mensajes/{idMensaje}/responder` | Admin responde un mensaje. Guarda texto en `Respuesta` y marca `Respondido = 1` |
| `PUT` | `/api/mensajes/{idMensaje}/leido` | Marca mensaje como leído (`Leido = 1`) |
| `PUT` | `/api/mensajes/{idMensaje}/respondido` | Marca mensaje como respondido (`Respondido = 1`) |
| `DELETE` | `/api/mensajes/{idMensaje}` | Elimina mensaje |
| `GET` | `/api/mensajes/no-leidos/count` | Devuelve cuántos mensajes no leídos hay (para badge de notificación admin) |

**Modelo de mensaje:**
- `IdMensaje`, `IdUsuario` (nullable), `Nombre`, `Email`, `Asunto`, `Mensaje`, `Fecha`, `Leido` (bit), `Respondido` (bit), `Respuesta`

---

## 9. Páginas del Frontend y Flujos de Usuario

### 9.1 Páginas existentes

| Página | Archivo | Rol requerido | Funcionalidad |
|--------|---------|---------------|---------------|
| **Inicio** | `frontend/index.html` | Pública | Landing page con hero, categorías, productos destacados, promociones, beneficios, galería y footer |
| **Login / Registro** | `frontend/pages/login.html` | Pública | Pestañas de login y registro. Redirige según rol tras login |
| **Tienda** | `frontend/pages/tienda.html` | Pública | Catálogo completo con filtros por categoría. Carga dinámica desde API |
| **Ficha Producto** | `frontend/pages/producto.html` | Pública (ver) | Carrusel de imágenes, info, selector de cantidad, añadir al carrito, like. Requiere login para acciones |
| **Detalle Pedido** | `frontend/pages/pedido.html` | Cliente | Factura/resumen de un pedido ya realizado. Muestra productos, estado, dirección, totales |

### 9.2 Páginas referenciadas pero NO implementadas aún

| Página | Referencias encontradas | Qué debería hacer |
|--------|------------------------|-------------------|
| `/pages/admin.html` | Redirección post-login (rol 1), link en navbar | Panel de administración con productos, usuarios, pedidos, estadísticas y mensajes |
| `/pages/perfil.html` | Link en navbar para clientes, redirección desde `pedido.html` | Perfil del usuario: datos personales, cambiar contraseña, listado de pedidos |
| `/pages/carrito.html` | Link en icono del carrito en navbar | Ver carrito, modificar cantidades, eliminar items, proceder al checkout |

### 9.3 Flujos de usuario

#### Flujo A — Visitante anónimo
```
index.html  →  tienda.html  →  producto.html?id=X
   ↓              ↓                ↓
Ver landing   Ver catálogo    Ver detalle producto
                 ↓                ↓
           Filtrar categorías  Click "Añadir" o "♥"
                                    ↓
                              login.html (redirección forzada)
```

#### Flujo B — Registro
```
login.html  →  Tab "Registrarse"  →  POST /api/register
                                      ↓
                              Éxito → auto-switch a login tras 1.2s
```

#### Flujo C — Login con redirección por rol
```
login.html  →  POST /api/login  →  setSession() en localStorage
                                    ↓
                            ┌──────┴──────┐
                       idRol === 1      idRol === 2
                            ↓                ↓
                    /pages/admin.html    /index.html
```

#### Flujo D — Cliente: compra completa
```
tienda.html / producto.html  →  Añadir al carrito  →  carrito.html (no existe)
                                                         ↓
                                               Revisar items + checkout
                                                         ↓
                                               POST /api/pedidos/confirmar
                                                         ↓
                                               perfil.html (no existe)  →  Ver pedidos
```

#### Flujo E — Cliente: confirmar entrega
```
perfil.html  →  pedido.html?id=X (estado "Enviado")
                     ↓
              Click "Confirmar entrega"
                     ↓
              POST /api/pedidos/{id}/confirmar-entrega
                     ↓
              Estado pasa a "Entregado"
```

#### Flujo F — Admin: gestionar pedido
```
admin.html  →  Ver pedidos  →  Cambiar estado
                                     ↓
                              POST /api/pedidos/admin/{id}/estado
                                     ↓
                              Registro en historial + notificación
```

---

## 10. Resumen de Archivos por Funcionalidad

### Base de datos
| Archivo | Funcionalidad |
|---------|---------------|
| `EnerGym.Api/EnerGymDB_Setup.sql` | Esquema completo: tablas, relaciones, seed data, migraciones inline |

### Backend — Modelos y DTOs
| Archivo | Funcionalidad |
|---------|---------------|
| `EnerGym.Api/Models/Models.cs` | 25 clases: entidades (Usuario, Producto, Carrito, Pedido...) y DTOs para auth, carrito, pedidos, likes, perfiles, admin y mensajes |

### Backend — Controladores
| Archivo | Funcionalidad |
|---------|---------------|
| `EnerGym.Api/Controllers/AuthController.cs` | Registro, login, listado de usuarios |
| `EnerGym.Api/Controllers/ProductosController.cs` | CRUD de productos, categorías, likes count, imágenes adicionales |
| `EnerGym.Api/Controllers/CarritoController.cs` | Ver carrito, añadir item, cambiar cantidad, eliminar item |
| `EnerGym.Api/Controllers/LikesController.cs` | Toggle like, conteo de likes, productos favoritos por usuario |
| `EnerGym.Api/Controllers/PedidosController.cs` | Confirmar pedido, listar pedidos (cliente y admin), estadísticas, historial de estados, confirmar entrega |
| `EnerGym.Api/Controllers/UsuariosController.cs` | Perfil, editar perfil, cambiar contraseña, pedidos del usuario |
| `EnerGym.Api/Controllers/AdminController.cs` | CRUD de productos, usuarios, pedidos y estadísticas (todo protegido por `EsAdmin`) |
| `EnerGym.Api/Controllers/MensajesController.cs` | Enviar, listar, responder, marcar leído/respondido, eliminar mensajes de soporte |

### Backend — Configuración
| Archivo | Funcionalidad |
|---------|---------------|
| `EnerGym.Api/Program.cs` | Registro de servicios, CORS "AllowAll", static files, SPA fallback |
| `EnerGym.Api/Database.cs` | Singleton que lee connection string y expone `GetConnection()` |
| `EnerGym.Api/appsettings.json` | Connection string a SQL Server en Somee.com |
| `EnerGym.Api/EnerGym.csproj` | Dependencias: `Microsoft.Data.SqlClient`, `BCrypt.Net-Next` |

### Frontend — Páginas
| Archivo | Funcionalidad |
|---------|---------------|
| `frontend/index.html` | Landing page completa |
| `frontend/pages/login.html` | Login y registro con redirección por rol |
| `frontend/pages/tienda.html` | Catálogo con filtros dinámicos |
| `frontend/pages/producto.html` | Ficha de producto con carrusel, cantidad, carrito y likes |
| `frontend/pages/pedido.html` | Detalle de pedido con resumen de facturación |

### Frontend — Assets globales
| Archivo | Funcionalidad |
|---------|---------------|
| `frontend/js/app.js` | Único archivo JS: sesión, navbar, carrito, likes, renderizado de productos, animaciones, toasts, utilidades |
| `frontend/css/styles.css` | Único archivo CSS: diseño dark premium, componentes, animaciones, responsive, skeleton loaders |

---

## Modelo de Datos (Diagrama textual)

```
Roles (1,2)
   │
   ▼
Usuarios ──┬──► Carritos (1:1) ────┬──► CarritoProductos (N) ───► Productos
           │                        │
           ├──► LikesProductos (N) ──┘
           │
           ├──► Pedidos (N) ─────────┬──► PedidoProductos (N) ───► Productos
           │                         │
           │                         └──► PedidoHistorialEstados (N)
           │
           ├──► MensajesSoporte (N)
           │
           └──► ProductoImagenes (N) ───► Productos

Productos ──► Categorias
```

---

## Notas Finales

- **Autenticación stateless:** No hay JWT ni cookies. El frontend envía `idUsuario` explícitamente en rutas o body, y el backend confirma identidad y rol consultando la BD.
- **Seguridad admin:** Todos los endpoints de `AdminController` y los admin de `PedidosController` verifican `IdRol == 1` en cada llamada mediante el método privado `EsAdmin()`.
- **Transacciones:** La confirmación de pedidos usa transacción SQL para garantizar consistencia entre stock, pedido, detalles y vaciado de carrito.
- **Frontend incompleto:** Las páginas `admin.html`, `perfil.html` y `carrito.html` están referenciadas en el código pero no existen aún en el repositorio. Sin embargo, `app.js` y `styles.css` ya contienen la lógica y estilos necesarios para soportarlas.
