-- =====================================================================
--  ALMACEN - Script completo (se puede ejecutar varias veces sin perder datos)
--  * Si la base de datos no existe, la crea.
--  * Si ya existe (version anterior), la actualiza sin borrar productos ni usuarios.
--  Ejecutar completo en SSMS con F5.
-- =====================================================================
IF DB_ID('Almacen') IS NULL
    CREATE DATABASE Almacen;
GO
USE Almacen;
GO

-- ===================== TABLAS BASE =====================
IF OBJECT_ID('dbo.Productos', 'U') IS NULL
CREATE TABLE dbo.Productos (
    Id      INT IDENTITY(1,1) PRIMARY KEY,
    Nombre  NVARCHAR(100)  NOT NULL,
    Precio  DECIMAL(18,2)  NOT NULL CONSTRAINT CK_Productos_Precio CHECK (Precio >= 0),
    Stock   INT            NOT NULL CONSTRAINT CK_Productos_Stock  CHECK (Stock  >= 0),
    Marca   NVARCHAR(100)  NOT NULL
);
GO

IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
CREATE TABLE dbo.Usuarios (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    NombreUsuario NVARCHAR(50) NOT NULL UNIQUE,
    Contrasena    VARCHAR(64)  NOT NULL,
    Rol           VARCHAR(20)  NOT NULL
);
GO

-- ===================== ACTUALIZACION DE USUARIOS (turno, activo, rol Cajero) =====================
IF COL_LENGTH('dbo.Usuarios', 'Turno') IS NULL
    ALTER TABLE dbo.Usuarios ADD Turno NVARCHAR(10) NULL;
GO
IF COL_LENGTH('dbo.Usuarios', 'Activo') IS NULL
    ALTER TABLE dbo.Usuarios ADD Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT 1;
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Usuarios_Rol')
    ALTER TABLE dbo.Usuarios DROP CONSTRAINT CK_Usuarios_Rol;
GO
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_Rol
    CHECK (Rol IN ('Administrador', 'Cajero', 'Invitado'));
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Usuarios_Turno')
    ALTER TABLE dbo.Usuarios DROP CONSTRAINT CK_Usuarios_Turno;
GO
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_Turno
    CHECK (Turno IS NULL OR Turno IN (N'Mañana', N'Tarde', N'Noche'));
GO
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Usuarios_CajeroTurno')
    ALTER TABLE dbo.Usuarios DROP CONSTRAINT CK_Usuarios_CajeroTurno;
GO
ALTER TABLE dbo.Usuarios ADD CONSTRAINT CK_Usuarios_CajeroTurno
    CHECK (Rol <> 'Cajero' OR Turno IS NOT NULL);
GO

-- ===================== CLIENTES, FACTURAS, CUENTAS POR COBRAR =====================
IF OBJECT_ID('dbo.Clientes', 'U') IS NULL
CREATE TABLE dbo.Clientes (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre   NVARCHAR(100) NOT NULL,
    Telefono NVARCHAR(20)  NULL,
    Cedula   NVARCHAR(20)  NULL
);
GO

-- El cliente Id = 1 es el "Cliente de contado" (ventas sin cliente registrado)
IF NOT EXISTS (SELECT 1 FROM dbo.Clientes)
    INSERT INTO dbo.Clientes (Nombre) VALUES (N'Cliente de contado');
GO

-- Contador de facturas (sin saltos de numero)
IF OBJECT_ID('dbo.Secuencias', 'U') IS NULL
CREATE TABLE dbo.Secuencias (
    Nombre VARCHAR(30) NOT NULL PRIMARY KEY,
    Ultimo INT         NOT NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Secuencias WHERE Nombre = 'Factura')
    INSERT INTO dbo.Secuencias (Nombre, Ultimo) VALUES ('Factura', 0);
GO

IF OBJECT_ID('dbo.Facturas', 'U') IS NULL
CREATE TABLE dbo.Facturas (
    Numero      INT           NOT NULL PRIMARY KEY,
    Fecha       DATETIME      NOT NULL DEFAULT GETDATE(),
    Tipo        VARCHAR(7)    NOT NULL CHECK (Tipo IN ('Contado', 'Credito')),
    IdCajero    INT           NOT NULL REFERENCES dbo.Usuarios(Id),
    IdCliente   INT           NOT NULL REFERENCES dbo.Clientes(Id),
    TotalBruto  DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalItbis  DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalPagar  DECIMAL(18,2) NOT NULL DEFAULT 0,
    MontoPagado DECIMAL(18,2) NOT NULL DEFAULT 0,
    Devuelta    DECIMAL(18,2) NOT NULL DEFAULT 0
);
GO

IF OBJECT_ID('dbo.FacturaDetalle', 'U') IS NULL
CREATE TABLE dbo.FacturaDetalle (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    NumeroFactura  INT           NOT NULL REFERENCES dbo.Facturas(Numero),
    IdProducto     INT           NOT NULL REFERENCES dbo.Productos(Id),
    NombreProducto NVARCHAR(100) NOT NULL,
    Cantidad       INT           NOT NULL CHECK (Cantidad > 0),
    Valor          DECIMAL(18,2) NOT NULL,
    Itbis          DECIMAL(18,2) NOT NULL,
    Total          DECIMAL(18,2) NOT NULL
);
GO

IF OBJECT_ID('dbo.CuentasPorCobrar', 'U') IS NULL
CREATE TABLE dbo.CuentasPorCobrar (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    NumeroFactura INT           NOT NULL UNIQUE REFERENCES dbo.Facturas(Numero),
    IdCliente     INT           NOT NULL REFERENCES dbo.Clientes(Id),
    Fecha         DATETIME      NOT NULL DEFAULT GETDATE(),
    Monto         DECIMAL(18,2) NOT NULL,
    Saldo         DECIMAL(18,2) NOT NULL,
    Estado        VARCHAR(10)   NOT NULL DEFAULT 'Pendiente'
);
GO

-- ===================== USUARIOS DE PRUEBA =====================
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'admin')
    INSERT INTO dbo.Usuarios (NombreUsuario, Contrasena, Rol)
    VALUES ('admin', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2), 'Administrador');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'invitado')
    INSERT INTO dbo.Usuarios (NombreUsuario, Contrasena, Rol)
    VALUES ('invitado', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'invitado123'), 2), 'Invitado');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'cajero_manana')
    INSERT INTO dbo.Usuarios (NombreUsuario, Contrasena, Rol, Turno)
    VALUES ('cajero_manana', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'cajero123'), 2), 'Cajero', N'Mañana');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'cajero_tarde')
    INSERT INTO dbo.Usuarios (NombreUsuario, Contrasena, Rol, Turno)
    VALUES ('cajero_tarde', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'cajero123'), 2), 'Cajero', N'Tarde');
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE NombreUsuario = 'cajero_noche')
    INSERT INTO dbo.Usuarios (NombreUsuario, Contrasena, Rol, Turno)
    VALUES ('cajero_noche', CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'cajero123'), 2), 'Cajero', N'Noche');
GO

-- ===================== PROCEDIMIENTOS: PRODUCTOS =====================
CREATE OR ALTER PROCEDURE dbo.sp_Producto_Insertar
    @Nombre NVARCHAR(100), @Precio DECIMAL(18,2), @Stock INT, @Marca NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Productos (Nombre, Precio, Stock, Marca) VALUES (@Nombre, @Precio, @Stock, @Marca);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_Actualizar
    @Id INT, @Nombre NVARCHAR(100), @Precio DECIMAL(18,2), @Stock INT, @Marca NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Productos SET Nombre = @Nombre, Precio = @Precio, Stock = @Stock, Marca = @Marca
     WHERE Id = @Id;
    SELECT @@ROWCOUNT AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_Eliminar
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM FacturaDetalle WHERE IdProducto = @Id)
        THROW 50010, N'No se puede eliminar: el producto tiene ventas registradas.', 1;
    DELETE FROM Productos WHERE Id = @Id;
    SELECT @@ROWCOUNT AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Producto_Buscar
    @Codigo INT = NULL, @Nombre NVARCHAR(100) = NULL, @Marca NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Precio, Stock, Marca
      FROM Productos
     WHERE (@Codigo IS NULL OR Id = @Codigo)
       AND (@Nombre IS NULL OR Nombre LIKE '%' + @Nombre + '%')
       AND (@Marca  IS NULL OR Marca  LIKE '%' + @Marca  + '%')
     ORDER BY Id;
END
GO

-- ===================== PROCEDIMIENTOS: USUARIOS / CAJEROS =====================
CREATE OR ALTER PROCEDURE dbo.sp_Usuario_Login
    @NombreUsuario NVARCHAR(50), @Contrasena VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, NombreUsuario, Rol, Turno
      FROM Usuarios
     WHERE NombreUsuario = @NombreUsuario AND Contrasena = @Contrasena AND Activo = 1;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cajero_Insertar
    @NombreUsuario NVARCHAR(50), @Contrasena VARCHAR(64), @Turno NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario)
        THROW 50020, N'Ya existe un usuario con ese nombre.', 1;
    INSERT INTO Usuarios (NombreUsuario, Contrasena, Rol, Turno)
    VALUES (@NombreUsuario, @Contrasena, 'Cajero', @Turno);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cajero_Actualizar
    @Id INT, @Turno NVARCHAR(10), @Activo BIT, @Contrasena VARCHAR(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Usuarios
       SET Turno = @Turno,
           Activo = @Activo,
           Contrasena = ISNULL(@Contrasena, Contrasena)
     WHERE Id = @Id AND Rol = 'Cajero';
    SELECT @@ROWCOUNT AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cajero_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, NombreUsuario, Turno, Activo
      FROM Usuarios
     WHERE Rol = 'Cajero'
     ORDER BY Id;
END
GO

-- ===================== PROCEDIMIENTOS: CLIENTES =====================
CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Insertar
    @Nombre NVARCHAR(100), @Telefono NVARCHAR(20) = NULL, @Cedula NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Clientes (Nombre, Telefono, Cedula) VALUES (@Nombre, @Telefono, @Cedula);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NuevoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Actualizar
    @Id INT, @Nombre NVARCHAR(100), @Telefono NVARCHAR(20) = NULL, @Cedula NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Id = 1
        THROW 50030, N'El cliente de contado no se puede modificar.', 1;
    UPDATE Clientes SET Nombre = @Nombre, Telefono = @Telefono, Cedula = @Cedula WHERE Id = @Id;
    SELECT @@ROWCOUNT AS Filas;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Cliente_Buscar
    @Texto NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Telefono, Cedula
      FROM Clientes
     WHERE @Texto IS NULL
        OR Nombre LIKE '%' + @Texto + '%'
        OR Cedula LIKE '%' + @Texto + '%'
        OR CAST(Id AS NVARCHAR(10)) = @Texto
     ORDER BY Id;
END
GO

-- ===================== PROCEDIMIENTOS: FACTURACION =====================
CREATE OR ALTER PROCEDURE dbo.sp_Factura_Crear
    @Tipo VARCHAR(7), @IdCajero INT, @IdCliente INT
AS
BEGIN
    SET NOCOUNT ON;
    IF @Tipo = 'Credito' AND @IdCliente = 1
        THROW 50005, N'Una venta a crédito requiere un cliente registrado.', 1;

    DECLARE @Numero INT;
    UPDATE Secuencias SET @Numero = Ultimo = Ultimo + 1 WHERE Nombre = 'Factura';

    INSERT INTO Facturas (Numero, Tipo, IdCajero, IdCliente)
    VALUES (@Numero, @Tipo, @IdCajero, @IdCliente);

    SELECT @Numero AS Numero;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Factura_AgregarLinea
    @Numero INT, @IdProducto INT, @Cantidad INT, @TasaItbis DECIMAL(5,4)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Nombre NVARCHAR(100), @Precio DECIMAL(18,2), @Stock INT, @Msg NVARCHAR(200);

    SELECT @Nombre = Nombre, @Precio = Precio, @Stock = Stock FROM Productos WHERE Id = @IdProducto;

    IF @Nombre IS NULL
        THROW 50002, N'El producto no existe.', 1;

    IF @Stock < @Cantidad
    BEGIN
        SET @Msg = CONCAT(N'Stock insuficiente de ', @Nombre, N' (disponible: ', @Stock, N').');
        THROW 50001, @Msg, 1;
    END

    UPDATE Productos SET Stock = Stock - @Cantidad WHERE Id = @IdProducto AND Stock >= @Cantidad;
    IF @@ROWCOUNT = 0
        THROW 50001, N'Stock insuficiente.', 1;

    INSERT INTO FacturaDetalle (NumeroFactura, IdProducto, NombreProducto, Cantidad, Valor, Itbis, Total)
    VALUES (@Numero, @IdProducto, @Nombre, @Cantidad, @Precio,
            ROUND(@Precio * @Cantidad * @TasaItbis, 2),
            ROUND(@Precio * @Cantidad, 2));
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Factura_Cerrar
    @Numero INT, @MontoPagado DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Bruto DECIMAL(18,2), @Itbis DECIMAL(18,2), @Total DECIMAL(18,2),
            @Tipo VARCHAR(7), @IdCliente INT;

    SELECT @Bruto = ISNULL(SUM(Total), 0), @Itbis = ISNULL(SUM(Itbis), 0)
      FROM FacturaDetalle WHERE NumeroFactura = @Numero;

    IF @Bruto = 0
        THROW 50003, N'La factura no tiene productos.', 1;

    SELECT @Tipo = Tipo, @IdCliente = IdCliente FROM Facturas WHERE Numero = @Numero;
    SET @Total = @Bruto + @Itbis;

    IF @Tipo = 'Contado' AND @MontoPagado < @Total
        THROW 50004, N'El monto pagado es menor al total a pagar.', 1;

    IF @Tipo = 'Credito'
        SET @MontoPagado = 0;

    UPDATE Facturas
       SET TotalBruto = @Bruto, TotalItbis = @Itbis, TotalPagar = @Total,
           MontoPagado = @MontoPagado,
           Devuelta = CASE WHEN @Tipo = 'Contado' THEN @MontoPagado - @Total ELSE 0 END
     WHERE Numero = @Numero;

    IF @Tipo = 'Credito'
        INSERT INTO CuentasPorCobrar (NumeroFactura, IdCliente, Monto, Saldo)
        VALUES (@Numero, @IdCliente, @Total, @Total);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Factura_Obtener
    @Numero INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT f.Numero, f.Fecha, f.Tipo, u.NombreUsuario AS Cajero, u.Turno,
           c.Id AS IdCliente, c.Nombre AS Cliente,
           f.TotalBruto, f.TotalItbis, f.TotalPagar, f.MontoPagado, f.Devuelta
      FROM Facturas f
      JOIN Usuarios u ON u.Id = f.IdCajero
      JOIN Clientes c ON c.Id = f.IdCliente
     WHERE f.Numero = @Numero;

    SELECT IdProducto, NombreProducto, Cantidad, Valor, Itbis, Total
      FROM FacturaDetalle
     WHERE NumeroFactura = @Numero
     ORDER BY Id;
END
GO

-- ===================== PROCEDIMIENTOS: REPORTES =====================
CREATE OR ALTER PROCEDURE dbo.sp_Rep_CuadreDia
    @Fecha DATE, @IdCajero INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Tipo, Facturas, Bruto, Itbis, Total
      FROM (
            SELECT 0 AS Orden,
                   CASE WHEN Tipo = 'Credito' THEN N'Crédito' ELSE N'Contado' END AS Tipo,
                   COUNT(*) AS Facturas,
                   SUM(TotalBruto) AS Bruto, SUM(TotalItbis) AS Itbis, SUM(TotalPagar) AS Total
              FROM Facturas
             WHERE Fecha >= @Fecha AND Fecha < DATEADD(DAY, 1, @Fecha)
               AND (@IdCajero IS NULL OR IdCajero = @IdCajero)
             GROUP BY Tipo
            UNION ALL
            SELECT 1, N'TOTAL', COUNT(*),
                   ISNULL(SUM(TotalBruto), 0), ISNULL(SUM(TotalItbis), 0), ISNULL(SUM(TotalPagar), 0)
              FROM Facturas
             WHERE Fecha >= @Fecha AND Fecha < DATEADD(DAY, 1, @Fecha)
               AND (@IdCajero IS NULL OR IdCajero = @IdCajero)
           ) x
     ORDER BY Orden, Tipo;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Rep_PorCajero
    @Desde DATE, @Hasta DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.NombreUsuario AS Cajero, u.Turno,
           COUNT(f.Numero) AS Facturas,
           ISNULL(SUM(CASE WHEN f.Tipo = 'Contado' THEN f.TotalPagar END), 0) AS Contado,
           ISNULL(SUM(CASE WHEN f.Tipo = 'Credito' THEN f.TotalPagar END), 0) AS Credito,
           ISNULL(SUM(f.TotalPagar), 0) AS Total
      FROM Usuarios u
      LEFT JOIN Facturas f ON f.IdCajero = u.Id
                          AND f.Fecha >= @Desde AND f.Fecha < DATEADD(DAY, 1, @Hasta)
     WHERE u.Rol = 'Cajero'
     GROUP BY u.NombreUsuario, u.Turno
     ORDER BY Total DESC, Cajero;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Rep_Inventario
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Marca, Stock, Precio, CAST(Stock * Precio AS DECIMAL(18,2)) AS Valor
      FROM Productos
     WHERE Stock > 0
     ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Rep_Facturas
    @Desde DATE, @Hasta DATE, @Tipo VARCHAR(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RIGHT('00000' + CAST(f.Numero AS VARCHAR(5)), 5) AS Numero,
           f.Fecha,
           CASE WHEN f.Tipo = 'Credito' THEN N'Crédito' ELSE N'Contado' END AS Tipo,
           u.NombreUsuario AS Cajero, c.Nombre AS Cliente, f.TotalPagar AS Total
      FROM Facturas f
      JOIN Usuarios u ON u.Id = f.IdCajero
      JOIN Clientes c ON c.Id = f.IdCliente
     WHERE f.Fecha >= @Desde AND f.Fecha < DATEADD(DAY, 1, @Hasta)
       AND (@Tipo IS NULL OR f.Tipo = @Tipo)
     ORDER BY f.Numero;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Rep_TopClientes
    @Desde DATE, @Hasta DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 5 c.Nombre AS Cliente, COUNT(f.Numero) AS Facturas, SUM(f.TotalPagar) AS Comprado
      FROM Facturas f
      JOIN Clientes c ON c.Id = f.IdCliente
     WHERE c.Id <> 1
       AND f.Fecha >= @Desde AND f.Fecha < DATEADD(DAY, 1, @Hasta)
     GROUP BY c.Id, c.Nombre
     ORDER BY Comprado DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Rep_CuentasPorCobrar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RIGHT('00000' + CAST(x.NumeroFactura AS VARCHAR(5)), 5) AS Factura,
           c.Nombre AS Cliente, x.Fecha, x.Monto, x.Saldo, x.Estado
      FROM CuentasPorCobrar x
      JOIN Clientes c ON c.Id = x.IdCliente
     ORDER BY x.NumeroFactura;
END
GO
