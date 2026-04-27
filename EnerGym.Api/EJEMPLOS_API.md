# 📚 Ejemplos de Uso - API EnerGym

## 🔥 Casos de Uso Reales

### 1️⃣ REGISTRO E INICIO DE SESIÓN

#### Registrar nuevo usuario
```bash
curl -X POST http://localhost:5000/api/register \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Carlos López",
    "email": "carlos@example.com",
    "password": "MiPassword123"
  }'
```

**Respuesta (200):**
```json
{
  "message": "Usuario registrado correctamente."
}
```

#### Iniciar sesión
```bash
curl -X POST http://localhost:5000/api/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "carlos@example.com",
    "password": "MiPassword123"
  }'
```

**Respuesta (200):**
```json
{
  "idUsuario": 5,
  "nombre": "Carlos López",
  "email": "carlos@example.com",
  "idRol": 2
}
```

---

### 2️⃣ VER PRODUCTOS

#### Listar todos los productos
```bash
curl http://localhost:5000/api/productos
```

**Respuesta (200):**
```json
[
  {
    "idProducto": 1,
    "nombre": "Whey Protein Gold",
    "descripcion": "Proteína de suero de alta calidad con 24g de proteína",
    "precio": 34.99,
    "stock": 120,
    "imagen": "https://images.unsplash.com/photo-1593095948071-474c5cc2989d",
    "idCategoria": 1,
    "nombreCategoria": "Proteínas",
    "tieneLike": false,
    "totalLikes": 5
  },
  ...
]
```

#### Listar productos mostrando si el usuario ha dado like
```bash
curl "http://localhost:5000/api/productos?idUsuario=5"
```

**Respuesta (200):**
```json
[
  {
    "idProducto": 1,
    "nombre": "Whey Protein Gold",
    ...
    "tieneLike": true,        // ✅ Este usuario ya le dio like
    "totalLikes": 5
  },
  ...
]
```

---

### 3️⃣ GESTIÓN DEL CARRITO

#### Añadir producto al carrito
```bash
curl -X POST http://localhost:5000/api/carrito/agregar \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5,
    "idProducto": 1,
    "cantidad": 2
  }'
```

**Respuesta (200):**
```json
{
  "message": "Producto añadido al carrito."
}
```

#### Ver carrito del usuario
```bash
curl http://localhost:5000/api/carrito/5
```

**Respuesta (200):**
```json
{
  "idCarrito": 3,
  "items": [
    {
      "id": 10,
      "idCarrito": 3,
      "idProducto": 1,
      "nombreProducto": "Whey Protein Gold",
      "imagen": "https://...",
      "precio": 34.99,
      "cantidad": 2,
      "subtotal": 69.98
    },
    {
      "id": 11,
      "idCarrito": 3,
      "idProducto": 5,
      "nombreProducto": "Dark Energy Formula",
      "imagen": "https://...",
      "precio": 39.99,
      "cantidad": 1,
      "subtotal": 39.99
    }
  ],
  "total": 109.97
}
```

#### Cambiar cantidad de producto en carrito
```bash
curl -X PUT http://localhost:5000/api/carrito/item/10 \
  -H "Content-Type: application/json" \
  -d '{
    "cantidad": 3
  }'
```

**Respuesta (200):**
```json
{
  "message": "Cantidad actualizada."
}
```

#### Eliminar producto del carrito
```bash
curl -X DELETE http://localhost:5000/api/carrito/item/10
```

**Respuesta (200):**
```json
{
  "message": "Producto eliminado del carrito."
}
```

---

### 4️⃣ LIKES DE PRODUCTOS

#### Dar like a un producto
```bash
curl -X POST http://localhost:5000/api/likes/toggle \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5,
    "idProducto": 1
  }'
```

**Respuesta (200):**
```json
{
  "liked": true,
  "message": "Like añadido."
}
```

#### Quitar like (segunda vez que clickea)
```bash
curl -X POST http://localhost:5000/api/likes/toggle \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5,
    "idProducto": 1
  }'
```

**Respuesta (200):**
```json
{
  "liked": false,
  "message": "Like eliminado."
}
```

#### Ver productos favoritos del usuario
```bash
curl http://localhost:5000/api/likes/5
```

**Respuesta (200):**
```json
[
  {
    "idProducto": 2,
    "nombre": "Whey Isolada Zero",
    "precio": 44.99,
    "stock": 85,
    "imagen": "https://..."
  },
  {
    "idProducto": 7,
    "nombre": "BCAA 2:1:1 Powder",
    "precio": 24.99,
    "stock": 110,
    "imagen": "https://..."
  }
]
```

#### Contar likes de un producto
```bash
curl http://localhost:5000/api/likes/producto/1/count
```

**Respuesta (200):**
```json
{
  "idProducto": 1,
  "totalLikes": 5
}
```

---

### 5️⃣ PERFIL DE USUARIO

#### Ver mi perfil
```bash
curl http://localhost:5000/api/usuarios/5/perfil
```

**Respuesta (200):**
```json
{
  "idUsuario": 5,
  "nombre": "Carlos López",
  "email": "carlos@example.com",
  "telefono": "+34 600 123 456",
  "direccion": "Calle Principal 123, 3º A",
  "ciudad": "Madrid",
  "codigoPostal": "28001",
  "fotoPerfil": "https://...",
  "fechaRegistro": "2024-04-23T10:30:00"
}
```

#### Editar perfil
```bash
curl -X PUT http://localhost:5000/api/usuarios/5/editar-perfil \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5,
    "nombre": "Carlos López",
    "email": "carlos.nuevo@example.com",
    "telefono": "+34 600 999 888",
    "direccion": "Avenida Nueva 456",
    "ciudad": "Barcelona",
    "codigoPostal": "08001",
    "fotoPerfil": "https://..."
  }'
```

**Respuesta (200):**
```json
{
  "message": "Perfil actualizado correctamente."
}
```

#### Cambiar contraseña
```bash
curl -X PUT http://localhost:5000/api/usuarios/5/cambiar-contraseña \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5,
    "contraseñaActual": "MiPassword123",
    "contraseñaNueva": "NuevaPassword456",
    "confirmarContraseña": "NuevaPassword456"
  }'
```

**Respuesta (200):**
```json
{
  "message": "Contraseña cambiada correctamente."
}
```

---

### 6️⃣ REALIZAR UN PEDIDO (CHECKOUT)

#### Confirmar pedido (compra)
```bash
curl -X POST http://localhost:5000/api/pedidos/confirmar \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5
  }'
```

**Respuesta (200):**
```json
{
  "message": "Pedido confirmado correctamente.",
  "idPedido": 42,
  "total": 150.47
}
```

**Qué ocurre automáticamente:**
1. ✅ Se crea un nuevo Pedido con estado "Pendiente"
2. ✅ Se crean registros en PedidoProductos (productos del pedido)
3. ✅ El Stock se descuenta de cada producto
4. ✅ El carrito se vacía
5. ✅ Se calcula el total correcto

---

### 7️⃣ VER PEDIDOS INTEGRALES

#### Ver todos mis pedidos
```bash
curl http://localhost:5000/api/usuarios/5/pedidos
```

**Respuesta (200):**
```json
[
  {
    "idPedido": 42,
    "fecha": "2024-04-23T15:45:30",
    "total": 150.47,
    "estado": "Pendiente"
  },
  {
    "idPedido": 41,
    "fecha": "2024-04-20T10:20:15",
    "total": 89.98,
    "estado": "Entregado"
  }
]
```

#### Ver detalles completos de un pedido
```bash
curl http://localhost:5000/api/usuarios/5/pedidos/42
```

**Respuesta (200):**
```json
{
  "pedido": {
    "idPedido": 42,
    "fecha": "2024-04-23T15:45:30",
    "total": 150.47,
    "estado": "Pendiente"
  },
  "detalles": [
    {
      "idDetalle": 125,
      "idProducto": 1,
      "nombre": "Whey Protein Gold",
      "imagen": "https://...",
      "cantidad": 2,
      "precio": 34.99,
      "subtotal": 69.98
    },
    {
      "idDetalle": 126,
      "idProducto": 5,
      "nombre": "Dark Energy Formula",
      "imagen": "https://...",
      "cantidad": 1,
      "precio": 39.99,
      "subtotal": 39.99
    }
  ]
}
```

---

### 8️⃣ PANEL ADMIN

#### Ver estadísticas (solo admin con idRol=1)
```bash
curl http://localhost:5000/api/admin/1/estadisticas
```

**Respuesta (200):**
```json
{
  "totalUsuarios": 42,
  "totalPedidos": 156,
  "ingresosTotales": 12450.75,
  "totalProductos": 12,
  "productosSinStock": 2,
  "pedidosPorEstado": [
    {
      "estado": "Pendiente",
      "cantidad": 15
    },
    {
      "estado": "Preparando pedido",
      "cantidad": 8
    },
    {
      "estado": "Enviado",
      "cantidad": 22
    },
    {
      "estado": "En reparto",
      "cantidad": 18
    },
    {
      "estado": "Entregado",
      "cantidad": 93
    }
  ]
}
```

#### Crear nuevo producto (admin)
```bash
curl -X POST http://localhost:5000/api/admin/1/productos/crear \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Nuevo Suplemento X",
    "descripcion": "Fórmula revolucionaria",
    "precio": 49.99,
    "stock": 100,
    "imagen": "https://example.com/producto.jpg",
    "idCategoria": 1
  }'
```

**Respuesta (201):**
```json
{
  "idProducto": 13,
  "message": "Producto creado correctamente."
}
```

#### Editar producto (admin)
```bash
curl -X PUT http://localhost:5000/api/admin/1/productos/13/editar \
  -H "Content-Type: application/json" \
  -d '{
    "idProducto": 13,
    "nombre": "Nuevo Suplemento X Plus",
    "descripcion": "Fórmula revolucionaria mejorada",
    "precio": 59.99,
    "stock": 150,
    "imagen": "https://example.com/producto-plus.jpg",
    "idCategoria": 1
  }'
```

**Respuesta (200):**
```json
{
  "message": "Producto actualizado correctamente."
}
```

#### Eliminar producto (admin)
```bash
curl -X DELETE http://localhost:5000/api/admin/1/productos/13
```

**Respuesta (200):**
```json
{
  "message": "Producto eliminado correctamente."
}
```

---

### 9️⃣ GESTIÓN DE USUARIOS (ADMIN)

#### Ver todos los usuarios (admin)
```bash
curl http://localhost:5000/api/admin/1/usuarios/todos
```

**Respuesta (200):**
```json
[
  {
    "idUsuario": 1,
    "nombre": "Administrador",
    "email": "admin@energym.es",
    "telefono": "",
    "direccion": null,
    "ciudad": null,
    "codigoPostal": null,
    "fechaRegistro": "2024-03-01T08:00:00",
    "rol": "admin",
    "idRol": 1
  },
  {
    "idUsuario": 5,
    "nombre": "Carlos López",
    "email": "carlos@example.com",
    "telefono": "+34 600 123 456",
    "direccion": "Calle Principal 123",
    "ciudad": "Madrid",
    "codigoPostal": "28001",
    "fechaRegistro": "2024-04-23T10:30:00",
    "rol": "cliente",
    "idRol": 2
  }
]
```

#### Cambiar rol de usuario a admin (admin)
```bash
curl -X PUT http://localhost:5000/api/admin/1/usuarios/5/editar \
  -H "Content-Type: application/json" \
  -d '{
    "idUsuario": 5,
    "nombre": "Carlos López",
    "email": "carlos@example.com",
    "idRol": 1,
    "telefono": "+34 600 123 456",
    "direccion": "Calle Principal 123",
    "ciudad": "Madrid",
    "codigoPostal": "28001"
  }'
```

**Respuesta (200):**
```json
{
  "message": "Usuario actualizado correctamente."
}
```

#### Eliminar usuario (admin)
```bash
curl -X DELETE http://localhost:5000/api/admin/1/usuarios/5
```

**Respuesta (200):**
```json
{
  "message": "Usuario eliminado correctamente."
}
```

---

### 🔟 GESTIÓN DE PEDIDOS (ADMIN)

#### Ver todos los pedidos (admin)
```bash
curl http://localhost:5000/api/admin/1/pedidos/todos
```

**Respuesta (200):**
```json
[
  {
    "idPedido": 42,
    "idUsuario": 5,
    "nombreUsuario": "Carlos López",
    "fecha": "2024-04-23T15:45:30",
    "total": 150.47,
    "estado": "Pendiente"
  },
  {
    "idPedido": 41,
    "idUsuario": 5,
    "nombreUsuario": "Carlos López",
    "fecha": "2024-04-20T10:20:15",
    "total": 89.98,
    "estado": "Preparando pedido"
  }
]
```

#### Cambiar estado de un pedido (admin)
```bash
curl -X PUT http://localhost:5000/api/admin/1/pedidos/42/cambiar-estado \
  -H "Content-Type: application/json" \
  -d '{
    "idPedido": 42,
    "nuevoEstado": "Enviado"
  }'
```

**Respuesta (200):**
```json
{
  "message": "Estado del pedido cambiado a 'Enviado'."
}
```

**Estados válidos:** Pendiente, Preparando pedido, Enviado, En reparto, Entregado

#### Ver detalles de un pedido (admin)
```bash
curl http://localhost:5000/api/admin/1/pedidos/42/detalles
```

**Respuesta (200):**
```json
{
  "pedido": {
    "idPedido": 42,
    "idUsuario": 5,
    "nombreUsuario": "Carlos López",
    "fecha": "2024-04-23T15:45:30",
    "total": 150.47,
    "estado": "Enviado"
  },
  "detalles": [
    {
      "idDetalle": 125,
      "idProducto": 1,
      "nombre": "Whey Protein Gold",
      "imagen": "https://...",
      "cantidad": 2,
      "precio": 34.99,
      "subtotal": 69.98
    }
  ]
}
```

---

## 🔴 CÓDIGOS DE ERROR

### 400 - Bad Request
```json
{
  "error": "Todos los campos son obligatorios."
}
```

### 401 - Unauthorized
```json
{
  "error": "Email o contraseña incorrectos."
}
```

### 403 - Forbidden / No Admin
```json
{
  "error": "No tienes permisos de administrador."
}
```

### 404 - Not Found
```json
{
  "error": "Usuario no encontrado."
}
```

### 409 - Conflict (Email duplicado)
```json
{
  "error": "El email ya está registrado."
}
```

### 500 - Server Error
```json
{
  "error": "Error de base de datos..."
}
```

---

## 💡 CONSEJOS DE USO

1. **Guarda el `idUsuario` del login** en localStorage
2. **Siempre envía `idUsuario`** en endpoints de usuario
3. **Verificar roles**: Si `idRol = 1`, mostrar botón admin
4. **Manejar errores**: Mostrar `error` al usuario
5. **Usar `GET` para lectura, POST/PUT/DELETE** para escritura
6. **Content-Type siempre**: `application/json`

---

**¡Todos los endpoints están listos para usar!** ✅
