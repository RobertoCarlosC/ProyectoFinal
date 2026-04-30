# ⚙️ Sistema de Gestión de Pedidos y Envíos - EnerGym

## 📋 Descripción General

Sistema completo de gestión de pedidos y envíos implementado para la tienda online de suplementos EnerGym. Incluye panel administrativo completo y panel de usuario con seguimiento de entregas en tiempo real.

---

## 🚀 Características Implementadas

### 👨‍💼 **Panel de Administración**

#### 1. **Gestión de Pedidos**
- ✅ Ver todos los pedidos del sistema
- ✅ Filtrar pedidos por estado
- ✅ Vista detallada de cada pedido con información completa
- ✅ Cambiar estado de pedidos (Pendiente → Procesando → Enviado → En Reparto → Entregado)
- ✅ Agregar notas al cambiar estado
- ✅ Botones rápidos para acelerar cambios de estado
- ✅ Historial de cambios de estado (auditoría)

#### 2. **Estadísticas Dashboard**
- Total de pedidos
- Pedidos pendientes de confirmación
- Pedidos en proceso
- Pedidos enviados
- Pedidos entregados
- Ventas totales
- Promedio de venta por pedido

#### 3. **Información de Pedidos**
- Número de pedido
- Cliente (nombre, email)
- Productos pedidos con cantidades y precios
- Fecha del pedido
- Estado actual
- Dirección de envío
- Método de pago
- Total del pedido

---

### 👤 **Panel de Usuario**

#### 1. **Historial de Pedidos**
- Ver todos los pedidos realizados
- Estado actual de cada pedido
- Fecha del pedido
- Total pagado
- Acceso rápido a detalles

#### 2. **Detalle de Pedido**
- Productos comprados (imagen, nombre, cantidad, precio)
- Total con detalles (subtotal, envío, impuestos)
- Dirección de envío
- Método de pago
- Timeline visual del estado del envío

#### 3. **Confirmación de Entrega**
- Botón de confirmación cuando pedido está "Enviado" o "En Reparto"
- Al confirmar se marca automáticamente como "Entregado"
- Se guarda la fecha de confirmación de entrega
- Se registra en el historial de cambios

---

## 🗄️ **Cambios en Base de Datos**

### Nueva Tabla: `PedidoHistorialEstados`
Registra todos los cambios de estado de los pedidos para auditoría:
```sql
- IdHistorial: INT PRIMARY KEY IDENTITY
- IdPedido: INT FOREIGN KEY
- EstadoAnterior: NVARCHAR(50)
- EstadoNuevo: NVARCHAR(50)
- Fecha: DATETIME DEFAULT GETDATE()
- CambiadoPor: NVARCHAR(100) -- 'Admin_ID' o 'Usuario_ID'
- Notas: NVARCHAR(500) -- Notas opcionales
```

### Campos Nuevos en Tabla `Pedidos`
```sql
- FechaConfirmacionEntrega: DATETIME NULL
  ↳ Se llena cuando el usuario confirma la entrega
  
- FechaActualizacion: DATETIME DEFAULT GETDATE()
  ↳ Se actualiza cada vez que cambia el estado
```

---

## 🔧 **APIs Implementadas**

### Endpoints de Usuario
```
GET  /api/pedidos/{idUsuario}
     ↳ Obtiene lista de pedidos del usuario

GET  /api/pedidos/detalle/{idPedido}
     ↳ Obtiene detalles completos de un pedido

POST /api/pedidos/{idPedido}/confirmar-entrega
Body: { idPedido, idUsuario }
     ↳ Usuario confirma que recibió el pedido
```

### Endpoints de Admin
```
GET  /api/pedidos/admin/todos
     ↳ Lista todos los pedidos (con filtro opcional por estado)
     Parámetros opcionales:
       - ?estado=Pendiente
       - ?estado=Procesando
       - ?estado=Enviado
       - ?estado=En reparto
       - ?estado=Entregado
       - ?idUsuario=123

GET  /api/pedidos/admin/estadisticas
     ↳ Obtiene estadísticas generales de pedidos

POST /api/pedidos/admin/{idPedido}/estado
Body: { 
  idPedido, 
  nuevoEstado,      // 'Pendiente', 'Procesando', 'Enviado', 'En reparto', 'Entregado'
  notas,           // Opcional
  idAdmin          // ID del admin que realiza el cambio
}
     ↳ Cambia el estado de un pedido

GET  /api/pedidos/{idPedido}/historial
     ↳ Obtiene historial de cambios de estado
```

---

## 📁 **Archivos Modificados**

### Backend (C#)
1. **`EnerGymDB_Setup.sql`**
   - Actualización de tabla Pedidos con nuevos campos
   - Nueva tabla PedidoHistorialEstados

2. **`Models/Models.cs`**
   - DTOs nuevos:
     - `PedidoExtendidoDto`
     - `HistorialEstadoPedidoDto`
     - `CambiarEstadoPedidoAdminDto`
     - `ConfirmarEntregaDto`
     - `ListaPedidosAdminDto`
     - `EstadisticasPedidosDto`

3. **`Controllers/PedidosController.cs`**
   - Nuevos métodos:
     - `GetAllPedidos()` - admin
     - `GetEstadisticas()` - admin
     - `CambiarEstadoAdmin()` - admin
     - `GetHistorialPedido()` - usuario/admin
     - `ConfirmarEntrega()` - usuario

### Frontend (HTML/JS)
1. **`admin.html`**
   - Nueva pestaña "Pedidos" en el panel
   - Modal para detalles de pedidos
   - Filtros por estado
   - Dashboard de estadísticas
   - Selector para cambiar estado

2. **`pedido.html`**
   - Timeline visual del envío
   - Botón de confirmación de entrega
   - Mejoras visuales

3. **`perfil.html`**
   - Corrección de endpoint para cargar pedidos

4. **`js/gestion-pedidos.js`** (NUEVO)
   - Lógica completa de gestión de pedidos
   - Funciones admin y usuario
   - Manejo de estados

---

## 🎨 **Estados de Pedidos**

| Estado | Color | Icono | Descripción |
|--------|-------|-------|-------------|
| Pendiente | Rojo (#ef4444) | ⏳ | Pedido confirmado, no procesado aún |
| Procesando | Naranja (#f59e0b) | ⚙️ | En preparación para envío |
| Enviado | Azul (#3b82f6) | 📦 | Enviado desde almacén |
| En Reparto | Cian (#06b6d4) | 🚚 | En manos del transportista |
| Entregado | Verde (#22c55e) | ✅ | Entregado al cliente |

---

## 🔐 **Control de Permisos**

### Admin
- ✅ Ver todos los pedidos
- ✅ Filtrar por estado
- ✅ Cambiar estado manualmente
- ✅ Agregar notas
- ✅ Ver historial de cambios
- ✅ Ver estadísticas

### Usuario
- ✅ Ver sus propios pedidos
- ✅ Ver detalles de sus pedidos
- ✅ Confirmar entrega (cuando estado es "Enviado" o "En reparto")
- ❌ NO puede cambiar estado manualmente
- ❌ NO puede ver pedidos de otros usuarios

---

## 📊 **Timeline Visual**

El usuario ve un timeline progresivo del estado del pedido:
```
⏳ Pendiente        ← Estado inicial
↓
⚙️ Procesando        ← Se prepara el envío
↓
📦 Enviado           ← Sale del almacén
↓
🚚 En Reparto        ← En poder del transportista
↓
✅ Entregado         ← Cliente confirma recepción
```

---

## 🔄 **Flujo de Control de Cambios de Estado**

1. **Admin cambia estado** → `/api/pedidos/admin/{id}/estado`
2. Sistema registra:
   - Nuevo estado en tabla `Pedidos`
   - Cambio en `PedidoHistorialEstados`
   - Fecha de actualización
   - Admin que realizó el cambio
3. Usuario ve cambio reflejado en tiempo real

---

## 📝 **Notas Importantes**

1. **Validación de Usuarios**: Solo el usuario que realizó el pedido puede confirmar su entrega
2. **Auditoría**: Todos los cambios de estado se registran con fecha y quien lo realizó
3. **Transacciones**: Los cambios de estado son atómicos (todo o nada)
4. **Historial**: Se mantiene historial completo de cambios para cada pedido

---

## 🧪 **Probar el Sistema**

### Como Admin:
1. Ir a `/pages/admin.html`
2. Hacer login con: **admin@energym.es** / **Admin1234**
3. Clic en pestaña "Pedidos"
4. Ver lista de pedidos
5. Hacer clic en "Ver" para ver detalles
6. Cambiar estado del pedido
7. Ver historial de cambios

### Como Usuario:
1. Crear cuenta o hacer login
2. Comprar algo
3. Ir a `/pages/perfil.html`
4. Clic en "Mis pedidos"
5. Hacer clic en un pedido
6. Si está en "Enviado" o "En Reparto", verá botón "He recibido mi pedido"
7. Al confirmar, se marca como "Entregado"

---

## 📞 **Integración con Sistema de Notificaciones** (Futuro)

El sistema está preparado para agregar notificaciones por email:
- Cuando estado cambia a "Enviado" → Email al cliente
- Cuando estado cambia a "En Reparto" → SMS notificación
- Cuando usuario confirma entrega → Confirmación al admin

---

## 🚀 **Instalación / Deploy**

1. Ejecutar script SQL: `EnerGymDB_Setup.sql`
2. Recompilan proyecto C# (automático)
3. Los archivos JS se cargan desde `/js/`
4. Las modelos ya están en place

---

## ✅ **Checklist de Funcionalidades**

- [x] Base de datos actualizada
- [x] APIs backend completas
- [x] Panel admin con gestión de pedidos
- [x] Dashboard de estadísticas
- [x] Filtros de estado
- [x] Modal de detalles
- [x] Cambio de estado (admin)
- [x] Confirmación de entrega (usuario)
- [x] Timeline visual
- [x] Historial de cambios
- [x] Control de permisos
- [x] UI coherente con diseño
- [x] Validaciones
- [x] Manejo de errores

---

## 🎯 **Resumen**

Sistema completo y funcional de gestión de pedidos y envíos que permite:
- ✅ Admin: control total sobre pedidos y seguimiento
- ✅ Usuario: seguimiento en tiempo real y confirmación de entrega
- ✅ Auditoría: registro completo de todos los cambios
- ✅ UX mejorada: diseño coherente y intuitivo
