

USE EnerGymDB;
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        IdRol  INT          PRIMARY KEY IDENTITY(1,1),
        Nombre NVARCHAR(50) NOT NULL
    );
    PRINT '✓ Tabla Roles creada.';
END
ELSE PRINT '– Tabla Roles ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    CREATE TABLE Usuarios (
        IdUsuario     INT           PRIMARY KEY IDENTITY(1,1),
        Nombre        NVARCHAR(100) NOT NULL,
        Email         NVARCHAR(150) NOT NULL UNIQUE,
        PasswordHash  NVARCHAR(255) NOT NULL,   
        IdRol         INT           NOT NULL DEFAULT 2,
        FechaRegistro DATETIME      NOT NULL DEFAULT GETDATE(),
        FotoPerfil    NVARCHAR(500) NULL,       
        Telefono      NVARCHAR(20)  NULL,
        Direccion     NVARCHAR(255) NULL,
        Ciudad        NVARCHAR(100) NULL,
        CodigoPostal  NVARCHAR(10)  NULL,
        CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (IdRol) REFERENCES Roles(IdRol)
    );
    PRINT '✓ Tabla Usuarios creada.';
END
ELSE PRINT '– Tabla Usuarios ya existe.';
GO


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuarios' AND COLUMN_NAME='FotoPerfil')
BEGIN
    ALTER TABLE Usuarios ADD FotoPerfil NVARCHAR(500) NULL;
    PRINT '✓ Columna FotoPerfil añadida a Usuarios.';
END
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuarios' AND COLUMN_NAME='Telefono')
BEGIN
    ALTER TABLE Usuarios ADD Telefono NVARCHAR(20) NULL;
    PRINT '✓ Columna Telefono añadida a Usuarios.';
END
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuarios' AND COLUMN_NAME='Direccion')
BEGIN
    ALTER TABLE Usuarios ADD Direccion NVARCHAR(255) NULL;
    PRINT '✓ Columna Direccion añadida a Usuarios.';
END
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuarios' AND COLUMN_NAME='Ciudad')
BEGIN
    ALTER TABLE Usuarios ADD Ciudad NVARCHAR(100) NULL;
    PRINT '✓ Columna Ciudad añadida a Usuarios.';
END
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuarios' AND COLUMN_NAME='CodigoPostal')
BEGIN
    ALTER TABLE Usuarios ADD CodigoPostal NVARCHAR(10) NULL;
    PRINT '✓ Columna CodigoPostal añadida a Usuarios.';
END
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categorias' AND xtype='U')
BEGIN
    CREATE TABLE Categorias (
        IdCategoria INT           PRIMARY KEY IDENTITY(1,1),
        Nombre      NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(255) NULL
    );
    PRINT '✓ Tabla Categorias creada.';
END
ELSE PRINT '– Tabla Categorias ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Productos' AND xtype='U')
BEGIN
    CREATE TABLE Productos (
        IdProducto  INT            PRIMARY KEY IDENTITY(1,1),
        Nombre      NVARCHAR(150)  NOT NULL,
        Descripcion NVARCHAR(1000) NULL,
        Precio      DECIMAL(10,2)  NOT NULL,
        Stock       INT            NOT NULL DEFAULT 0,
        Imagen      NVARCHAR(500)  NULL,
        IdCategoria INT            NOT NULL,
        CONSTRAINT FK_Productos_Categorias FOREIGN KEY (IdCategoria) REFERENCES Categorias(IdCategoria)
    );
    PRINT '✓ Tabla Productos creada.';
END
ELSE PRINT '– Tabla Productos ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Carritos' AND xtype='U')
BEGIN
    CREATE TABLE Carritos (
        IdCarrito     INT      PRIMARY KEY IDENTITY(1,1),
        IdUsuario     INT      NOT NULL UNIQUE,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Carritos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
    );
    PRINT '✓ Tabla Carritos creada.';
END
ELSE PRINT '– Tabla Carritos ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CarritoProductos' AND xtype='U')
BEGIN
    CREATE TABLE CarritoProductos (
        Id         INT PRIMARY KEY IDENTITY(1,1),
        IdCarrito  INT NOT NULL,
        IdProducto INT NOT NULL,
        Cantidad   INT NOT NULL DEFAULT 1,
        CONSTRAINT FK_CarritoProductos_Carritos  FOREIGN KEY (IdCarrito)  REFERENCES Carritos(IdCarrito),
        CONSTRAINT FK_CarritoProductos_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
    );
    PRINT '✓ Tabla CarritoProductos creada.';
END
ELSE PRINT '– Tabla CarritoProductos ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LikesProductos' AND xtype='U')
BEGIN
    CREATE TABLE LikesProductos (
        IdLike     INT PRIMARY KEY IDENTITY(1,1),
        IdUsuario  INT NOT NULL,
        IdProducto INT NOT NULL,
        CONSTRAINT FK_Likes_Usuarios  FOREIGN KEY (IdUsuario)  REFERENCES Usuarios(IdUsuario),
        CONSTRAINT FK_Likes_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto),
        CONSTRAINT UQ_Like UNIQUE (IdUsuario, IdProducto)
    );
    PRINT '✓ Tabla LikesProductos creada.';
END
ELSE PRINT '– Tabla LikesProductos ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pedidos' AND xtype='U')
BEGIN
    CREATE TABLE Pedidos (
        IdPedido                   INT           PRIMARY KEY IDENTITY(1,1),
        IdUsuario                  INT           NOT NULL,
        Fecha                      DATETIME      NOT NULL DEFAULT GETDATE(),
        Total                      DECIMAL(10,2) NOT NULL,
        Estado                     NVARCHAR(50)  NOT NULL DEFAULT 'Pendiente',
        DireccionEnvio             NVARCHAR(500) NULL,
        MetodoPago                 NVARCHAR(50)  NULL,
        FechaConfirmacionEntrega   DATETIME      NULL,
        FechaActualizacion         DATETIME      NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Pedidos_Usuarios FOREIGN KEY (IdUsuario) REFERENCES Usuarios(IdUsuario)
    );
    PRINT '✓ Tabla Pedidos creada.';
END
ELSE 
BEGIN
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pedidos' AND COLUMN_NAME='Estado')
    BEGIN
        ALTER TABLE Pedidos ADD Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente';
        PRINT '✓ Columna Estado añadida a Pedidos.';
    END
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pedidos' AND COLUMN_NAME='DireccionEnvio')
    BEGIN
        ALTER TABLE Pedidos ADD DireccionEnvio NVARCHAR(500) NULL;
        PRINT '✓ Columna DireccionEnvio añadida a Pedidos.';
    END
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pedidos' AND COLUMN_NAME='MetodoPago')
    BEGIN
        ALTER TABLE Pedidos ADD MetodoPago NVARCHAR(50) NULL;
        PRINT '✓ Columna MetodoPago añadida a Pedidos.';
    END
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pedidos' AND COLUMN_NAME='FechaConfirmacionEntrega')
    BEGIN
        ALTER TABLE Pedidos ADD FechaConfirmacionEntrega DATETIME NULL;
        PRINT '✓ Columna FechaConfirmacionEntrega añadida a Pedidos.';
    END
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Pedidos' AND COLUMN_NAME='FechaActualizacion')
    BEGIN
        ALTER TABLE Pedidos ADD FechaActualizacion DATETIME NOT NULL DEFAULT GETDATE();
        PRINT '✓ Columna FechaActualizacion añadida a Pedidos.';
    END
    PRINT '– Tabla Pedidos ya existe.';
END
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PedidoProductos' AND xtype='U')
BEGIN
    CREATE TABLE PedidoProductos (
        IdDetalle  INT           PRIMARY KEY IDENTITY(1,1),
        IdPedido   INT           NOT NULL,
        IdProducto INT           NOT NULL,
        Cantidad   INT           NOT NULL,
        Precio     DECIMAL(10,2) NOT NULL,
        CONSTRAINT FK_PedidoProductos_Pedidos   FOREIGN KEY (IdPedido)   REFERENCES Pedidos(IdPedido),
        CONSTRAINT FK_PedidoProductos_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto)
    );
    PRINT '✓ Tabla PedidoProductos creada.';
END
ELSE PRINT '– Tabla PedidoProductos ya existe.';
GO


IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PedidoHistorialEstados' AND xtype='U')
BEGIN
    CREATE TABLE PedidoHistorialEstados (
        IdHistorial INT          PRIMARY KEY IDENTITY(1,1),
        IdPedido    INT          NOT NULL,
        EstadoAnterior NVARCHAR(50),
        EstadoNuevo NVARCHAR(50) NOT NULL,
        Fecha       DATETIME     NOT NULL DEFAULT GETDATE(),
        CambiadoPor NVARCHAR(100) NULL,
        Notas       NVARCHAR(500) NULL,
        CONSTRAINT FK_PedidoHistorial_Pedidos FOREIGN KEY (IdPedido) REFERENCES Pedidos(IdPedido) ON DELETE CASCADE
    );
    PRINT '✓ Tabla PedidoHistorialEstados creada.';
END
ELSE PRINT '– Tabla PedidoHistorialEstados ya existe.';
GO



IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MensajesSoporte' AND xtype='U')
BEGIN
    CREATE TABLE MensajesSoporte (
        IdMensaje  INT            PRIMARY KEY IDENTITY(1,1),
        IdUsuario  INT            NULL,
        Nombre     NVARCHAR(100)  NOT NULL,
        Email      NVARCHAR(150)  NOT NULL,
        Asunto     NVARCHAR(200)  NOT NULL,
        Mensaje    NVARCHAR(2000) NOT NULL,
        Fecha      DATETIME       NOT NULL DEFAULT GETDATE(),
        Leido      BIT            NOT NULL DEFAULT 0,
        Respondido BIT            NOT NULL DEFAULT 0
    );
    PRINT '✓ Tabla MensajesSoporte creada.';
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='MensajesSoporte' AND COLUMN_NAME='Respuesta')
    BEGIN
        ALTER TABLE MensajesSoporte ADD Respuesta NVARCHAR(2000) NULL;
        PRINT '✓ Columna Respuesta añadida a MensajesSoporte.';
    END
    PRINT '– Tabla MensajesSoporte ya existe.';
END
GO



IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductoImagenes' AND xtype='U')
BEGIN
    CREATE TABLE ProductoImagenes (
        IdImagen   INT            PRIMARY KEY IDENTITY(1,1),
        IdProducto INT            NOT NULL,
        UrlImagen  NVARCHAR(500)  NOT NULL,
        Orden      INT            NOT NULL DEFAULT 0,
        CONSTRAINT FK_ProductoImagenes_Productos FOREIGN KEY (IdProducto) REFERENCES Productos(IdProducto) ON DELETE CASCADE
    );
    PRINT '✓ Tabla ProductoImagenes creada.';
END
ELSE PRINT '– Tabla ProductoImagenes ya existe.';
GO


IF NOT EXISTS (SELECT 1 FROM Roles WHERE IdRol = 1)
BEGIN
    SET IDENTITY_INSERT Roles ON;
    INSERT INTO Roles (IdRol, Nombre) VALUES (1, 'admin');
    INSERT INTO Roles (IdRol, Nombre) VALUES (2, 'cliente');
    SET IDENTITY_INSERT Roles OFF;
    PRINT '✓ Roles insertados: 1=admin, 2=cliente';
END
ELSE PRINT '– Roles ya existen.';
GO


IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@energym.es')
BEGIN
    INSERT INTO Usuarios (Nombre, Email, PasswordHash, IdRol, FechaRegistro)
    VALUES (
        'Administrador',
        'admin@energym.es',
        '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',
        1,
        GETDATE()
    );
    PRINT '✓ Admin creado → Email: admin@energym.es | Password: Admin1234';
END
ELSE PRINT '– Usuario admin ya existe.';
GO


IF NOT EXISTS (SELECT 1 FROM Categorias)
BEGIN
    INSERT INTO Categorias (Nombre, Descripcion) VALUES
        ('Proteínas',       'Suplementos proteicos para recuperación y crecimiento muscular'),
        ('Pre-Entreno',     'Fórmulas de energía y enfoque para antes del entrenamiento'),
        ('Creatinas',       'Monohidrato y fórmulas de creatina para fuerza y potencia'),
        ('Aminoácidos',     'BCAAs, glutamina y aminoácidos esenciales'),
        ('Vitaminas',       'Multivitamínicos y micronutrientes para la salud general'),
        ('Quemadores',      'Termogénicos y fat burners para definición muscular'),
        ('Gainers',         'Hipercalóricos para aumento de masa muscular'),
        ('Barras y Snacks', 'Snacks proteicos y barritas energéticas');
    PRINT '✓ Categorías insertadas.';
END
ELSE PRINT '– Categorías ya existen.';
GO


IF NOT EXISTS (SELECT 1 FROM Productos)
BEGIN
    INSERT INTO Productos (Nombre, Descripcion, Precio, Stock, Imagen, IdCategoria) VALUES
    ('Whey Protein Gold',
     'Proteína de suero de alta calidad con 24g de proteína por servicio. Sabor chocolate belga.',
     34.99, 120,
     'https://images.unsplash.com/photo-1593095948071-474c5cc2989d?w=400&q=80', 1),

    ('Whey Isolada Zero',
     'Proteína isolada sin lactosa con 27g de proteína. Perfecta para definición muscular.',
     44.99, 85,
     'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400&q=80', 1),

    ('Proteína Vegana Pro',
     'Blend de proteína de guisante y arroz integral. 100% vegana, sin gluten. 22g por servicio.',
     38.99, 60,
     'https://images.unsplash.com/photo-1576618148400-f54bed99fcfd?w=400&q=80', 1),

    ('Atomic Pre-Workout',
     'Pre-entreno explosivo con cafeína, beta-alanina y L-citrulina. Máxima energía y bombeo.',
     29.99, 75,
     'https://images.unsplash.com/photo-1534258936925-c58bed479fcb?w=400&q=80', 2),

    ('Dark Energy Formula',
     'Fórmula avanzada con 300mg de cafeína y nootrópicos. Para atletas de alto rendimiento.',
     39.99, 50,
     'https://images.unsplash.com/photo-1548690312-e3b507d8c110?w=400&q=80', 2),

    ('Creatina Monohidrato',
     'Creatina monohidrato pura micronizada. 300g sin aditivos. Aumenta fuerza y potencia.',
     19.99, 200,
     'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=400&q=80', 3),

    ('BCAA 2:1:1 Powder',
     'Aminoácidos ramificados en proporción óptima. 400g en polvo. Previene el catabolismo.',
     24.99, 110,
     'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400&q=80', 4),

    ('L-Glutamina Pura',
     'L-Glutamina en polvo micronizada. 300g. Favorece la recuperación y el sistema inmune.',
     18.99, 150,
     'https://images.unsplash.com/photo-1577401132921-cb39bb0adcff?w=400&q=80', 4),

    ('Multivitamínico Sport',
     'Complejo vitamínico para deportistas. 30 vitaminas y minerales. 60 cápsulas sin gluten.',
     16.99, 180,
     'https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=400&q=80', 5),

    ('ThermoBurn Xtreme',
     'Termogénico con extracto de té verde, L-carnitina y capsaicina. Acelera el metabolismo.',
     32.99, 65,
     'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400&q=80', 6),

    ('Mass Gainer 3000',
     'Hipercalórico con 1000 kcal por servicio. 40g de proteína y carbohidratos complejos.',
     42.99, 55,
     'https://images.unsplash.com/photo-1593095948071-474c5cc2989d?w=400&q=80', 7),

    ('Caja Barras Proteicas x12',
     'Pack de 12 barras con 20g de proteína. Bajo en azúcar. Varios sabores.',
     23.99, 95,
     'https://images.unsplash.com/photo-1590779033100-9f60a05a013d?w=400&q=80', 8);

    PRINT '✓ 12 productos de ejemplo insertados.';
END
ELSE PRINT '– Productos ya existen.';
GO


IF NOT EXISTS (SELECT 1 FROM ProductoImagenes)
BEGIN
    INSERT INTO ProductoImagenes (IdProducto, UrlImagen, Orden) VALUES
    (1, 'https://images.unsplash.com/photo-1593095948071-474c5cc2989d?w=600&q=80', 0),
    (1, 'https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=600&q=80', 1),
    (1, 'https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=600&q=80', 2),
    (2, 'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600&q=80', 0),
    (2, 'https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=600&q=80', 1),
    (3, 'https://images.unsplash.com/photo-1576618148400-f54bed99fcfd?w=600&q=80', 0),
    (3, 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&q=80', 1),
    (4, 'https://images.unsplash.com/photo-1534258936925-c58bed479fcb?w=600&q=80', 0),
    (4, 'https://images.unsplash.com/photo-1548690312-e3b507d8c110?w=600&q=80', 1),
    (5, 'https://images.unsplash.com/photo-1548690312-e3b507d8c110?w=600&q=80', 0),
    (5, 'https://images.unsplash.com/photo-1534258936925-c58bed479fcb?w=600&q=80', 1),
    (6, 'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=600&q=80', 0),
    (6, 'https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=600&q=80', 1),
    (7, 'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600&q=80', 0),
    (7, 'https://images.unsplash.com/photo-1576618148400-f54bed99fcfd?w=600&q=80', 1),
    (8, 'https://images.unsplash.com/photo-1577401132921-cb39bb0adcff?w=600&q=80', 0),
    (8, 'https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=600&q=80', 1),
    (9, 'https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=600&q=80', 0),
    (9, 'https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=600&q=80', 1),
    (10, 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=600&q=80', 0),
    (10, 'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600&q=80', 1),
    (11, 'https://images.unsplash.com/photo-1593095948071-474c5cc2989d?w=600&q=80', 0),
    (11, 'https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=600&q=80', 1),
    (12, 'https://images.unsplash.com/photo-1590779033100-9f60a05a013d?w=600&q=80', 0),
    (12, 'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=600&q=80', 1);
    PRINT '✓ Imágenes adicionales de productos insertadas.';
END
ELSE PRINT '– Imágenes de productos ya existen.';
GO


SELECT 'Roles'            AS Tabla, COUNT(*) AS Registros FROM Roles          UNION ALL
SELECT 'Usuarios',                  COUNT(*)              FROM Usuarios        UNION ALL
SELECT 'Categorias',                COUNT(*)              FROM Categorias      UNION ALL
SELECT 'Productos',                 COUNT(*)              FROM Productos       UNION ALL
SELECT 'Carritos',                  COUNT(*)              FROM Carritos        UNION ALL
SELECT 'CarritoProductos',          COUNT(*)              FROM CarritoProductos UNION ALL
SELECT 'LikesProductos',            COUNT(*)              FROM LikesProductos  UNION ALL
SELECT 'Pedidos',                   COUNT(*)              FROM Pedidos         UNION ALL
SELECT 'PedidoProductos',           COUNT(*)              FROM PedidoProductos UNION ALL
SELECT 'ProductoImagenes',          COUNT(*)              FROM ProductoImagenes;

PRINT '';
PRINT '============================================';
PRINT 'EnerGymDB lista. Credenciales admin:';
PRINT '  Email:    admin@energym.es';
PRINT '  Password: Admin1234';
PRINT '============================================';
GO
