/* =============================================================
 RestaurantJapanese - Script de inicialización completa
 Ejecuta TODO este archivo (F5) en SQL Server Management Studio
 Ajusta el nombre de la base de datos si usas otro (pia / REST_JP)
 ============================================================= */

/*1. Crear Base de Datos (ajusta el nombre si ya existe) */
IF DB_ID('REST_JP') IS NULL
BEGIN
 PRINT 'Creando base de datos REST_JP...';
 CREATE DATABASE REST_JP;
END
GO

USE REST_JP;
GO

/*2. Tablas base */
/* Tabla Users (simplificada; PasswordText en claro – NO para producción) */
IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
 CREATE TABLE dbo.Users (
 IdUser INT IDENTITY PRIMARY KEY,
 UserName NVARCHAR(100) NOT NULL UNIQUE,
 PasswordText NVARCHAR(200) NOT NULL, -- Cambiar a hash+salt en prod
 DisplayName NVARCHAR(120) NULL,
 IsActive BIT NOT NULL DEFAULT 1,
 CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
 );
END
GO

/* Tabla Employees */
IF OBJECT_ID('dbo.Employees','U') IS NULL
BEGIN
 CREATE TABLE dbo.Employees(
 IdEmployee INT IDENTITY PRIMARY KEY,
 IdUser INT NULL UNIQUE
 REFERENCES dbo.Users(IdUser) ON DELETE SET NULL,
 FullName NVARCHAR(120) NOT NULL,
 Email NVARCHAR(120) NULL,
 Phone NVARCHAR(30) NULL,
 Role NVARCHAR(30) NOT NULL DEFAULT('Empleado'),
 HireDate DATE NULL,
 IsActive BIT NOT NULL DEFAULT 1,
 CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
 UpdatedAt DATETIME2(0) NULL
 );
END
GO

/* Tabla MenuItems */
IF OBJECT_ID('dbo.MenuItems','U') IS NULL
BEGIN
 CREATE TABLE dbo.MenuItems (
 IdMenuItem INT IDENTITY PRIMARY KEY,
 Name NVARCHAR(150) NOT NULL UNIQUE,
 Description NVARCHAR(400) NULL,
 Price DECIMAL(10,2) NOT NULL,
 IsActive BIT NOT NULL DEFAULT 1,
 CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
 );
END
GO

/* Tabla Tickets */
IF OBJECT_ID('dbo.Tickets','U') IS NULL
BEGIN
 CREATE TABLE dbo.Tickets (
 IdTicket INT IDENTITY PRIMARY KEY,
 Subtotal DECIMAL(10,2) NOT NULL,
 Tax DECIMAL(10,2) NOT NULL,
 Tip DECIMAL(10,2) NOT NULL DEFAULT 0.00,
 Total DECIMAL(10,2) NOT NULL,
 CreatedBy INT NOT NULL REFERENCES dbo.Users(IdUser),
 CreatedAt DATETIME2(0) NOT NULL DEFAULT SYSDATETIME()
 );
END
GO

/* Tabla TicketItems */
IF OBJECT_ID('dbo.TicketItems','U') IS NULL
BEGIN
 CREATE TABLE dbo.TicketItems (
 IdTicketItem INT IDENTITY PRIMARY KEY,
 IdTicket INT NOT NULL REFERENCES dbo.Tickets(IdTicket) ON DELETE CASCADE,
 IdMenuItem INT NOT NULL REFERENCES dbo.MenuItems(IdMenuItem),
 Qty INT NOT NULL CHECK (Qty >0),
 UnitPrice DECIMAL(10,2) NOT NULL
 );
END
GO

/*3. Tipos definidos por el usuario */
IF TYPE_ID('dbo.PosItemTVP') IS NULL
BEGIN
 CREATE TYPE dbo.PosItemTVP AS TABLE
 (
 IdMenuItem INT NOT NULL,
 Qty INT NOT NULL CHECK (Qty >0)
 );
END
GO

/*4. Stored Procedures */

/* Validar usuario (Login) */
CREATE OR ALTER PROCEDURE dbo.RestJP_sp_ValidateUser
 @UserName NVARCHAR(100),
 @Password NVARCHAR(100)
AS
BEGIN
 SET NOCOUNT ON;
 SELECT TOP(1) *
 FROM dbo.Users
 WHERE UserName = @UserName AND PasswordText = @Password AND IsActive =1;
END
GO

/* Crear empleado + usuario */
CREATE OR ALTER PROCEDURE dbo.sp_Employees_CreateWithUser
 @FullName NVARCHAR(120),
 @Email NVARCHAR(120) = NULL,
 @Phone NVARCHAR(30) = NULL,
 @Role NVARCHAR(30) = 'Empleado',
 @UserName NVARCHAR(100),
 @PasswordText NVARCHAR(100),
 @IsActive BIT =1
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 SET @FullName = LTRIM(RTRIM(@FullName));
 SET @Email = NULLIF(LTRIM(RTRIM(@Email)), '');
 SET @Phone = NULLIF(LTRIM(RTRIM(@Phone)), '');
 SET @Role = LTRIM(RTRIM(@Role));
 SET @UserName = LTRIM(RTRIM(@UserName));
 SET @PasswordText = LTRIM(RTRIM(@PasswordText));
 IF (@FullName = '' OR @UserName = '' OR @PasswordText = '')
 BEGIN RAISERROR('FullName, UserName y PasswordText son requeridos.',16,1); RETURN; END
 IF UPPER(@Role) NOT IN ('ADMIN','EMPLEADO')
 BEGIN RAISERROR('Role inválido. Use "Admin" o "Empleado".',16,1); RETURN; END
 SET @Role = CASE WHEN UPPER(@Role)='ADMIN' THEN 'Admin' ELSE 'Empleado' END;
 BEGIN TRAN;
 IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName=@UserName)
 BEGIN RAISERROR('UserName ya existe.',16,1); ROLLBACK TRAN; RETURN; END
 INSERT INTO dbo.Users(UserName, PasswordText, IsActive) VALUES(@UserName,@PasswordText,@IsActive);
 DECLARE @NewIdUser INT = SCOPE_IDENTITY();
 INSERT INTO dbo.Employees(IdUser, FullName, Email, Phone, Role, IsActive)
 VALUES(@NewIdUser,@FullName,@Email,@Phone,@Role,@IsActive);
 DECLARE @NewIdEmployee INT = SCOPE_IDENTITY();
 COMMIT TRAN;
 SELECT e.IdEmployee, e.FullName, e.Email, e.Phone, e.Role, e.IsActive,
 u.IdUser, u.UserName, u.IsActive AS UserIsActive
 FROM dbo.Employees e JOIN dbo.Users u ON u.IdUser = e.IdUser
 WHERE e.IdEmployee = @NewIdEmployee;
END
GO

/* Obtener todos (con filtro opcional de activo y búsqueda) */
CREATE OR ALTER PROCEDURE dbo.sp_Employees_GetAll
 @OnlyActive BIT = NULL,
 @Search NVARCHAR(150) = NULL
AS
BEGIN
 SET NOCOUNT ON;
 SELECT e.IdEmployee, e.FullName, e.Email, e.Phone, e.Role, e.IsActive,
 e.CreatedAt, e.UpdatedAt,
 u.IdUser, u.UserName, u.DisplayName, u.IsActive AS UserIsActive
 FROM dbo.Employees e
 LEFT JOIN dbo.Users u ON u.IdUser = e.IdUser
 WHERE (@OnlyActive IS NULL OR e.IsActive = @OnlyActive)
 AND (@Search IS NULL OR (
 e.FullName LIKE '%' + @Search + '%' OR
 e.Email LIKE '%' + @Search + '%' OR
 u.UserName LIKE '%' + @Search + '%' OR
 u.DisplayName LIKE '%' + @Search + '%'))
 ORDER BY e.FullName;
END
GO

/* Obtener por Id */
CREATE OR ALTER PROCEDURE dbo.sp_Employees_GetById
 @IdEmployee INT
AS
BEGIN
 SET NOCOUNT ON;
 SELECT TOP(1) e.IdEmployee, e.FullName, e.Email, e.Phone, e.Role, e.IsActive,
 e.CreatedAt, e.UpdatedAt,
 u.IdUser, u.UserName, u.DisplayName, u.IsActive AS UserIsActive
 FROM dbo.Employees e
 LEFT JOIN dbo.Users u ON u.IdUser = e.IdUser
 WHERE e.IdEmployee = @IdEmployee;
END
GO

/* Update empleado */
CREATE OR ALTER PROCEDURE dbo.sp_Employees_Update
 @IdEmployee INT,
 @FullName NVARCHAR(120),
 @Email NVARCHAR(120) = NULL,
 @Phone NVARCHAR(30) = NULL,
 @Role NVARCHAR(30) = 'Empleado',
 @IsActive BIT =1
AS
BEGIN
 SET NOCOUNT ON;
 IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE IdEmployee=@IdEmployee)
 BEGIN RAISERROR('Empleado no encontrado.',16,1); RETURN; END
 SET @FullName = LTRIM(RTRIM(@FullName));
 SET @Email = NULLIF(LTRIM(RTRIM(@Email)), '');
 SET @Phone = NULLIF(LTRIM(RTRIM(@Phone)), '');
 SET @Role = LTRIM(RTRIM(@Role));
 IF (@FullName='') BEGIN RAISERROR('FullName es requerido.',16,1); RETURN; END
 IF UPPER(@Role) NOT IN ('ADMIN','EMPLEADO') BEGIN RAISERROR('Role inválido.',16,1); RETURN; END
 SET @Role = CASE WHEN UPPER(@Role)='ADMIN' THEN 'Admin' ELSE 'Empleado' END;
 IF @Email IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.Employees WHERE Email=@Email AND IdEmployee<>@IdEmployee)
 BEGIN RAISERROR('Email duplicado.',16,1); RETURN; END
 UPDATE dbo.Employees
 SET FullName=@FullName, Email=@Email, Phone=@Phone, Role=@Role, IsActive=@IsActive, UpdatedAt=SYSDATETIME()
 WHERE IdEmployee=@IdEmployee;
 SELECT e.IdEmployee, e.FullName, e.Email, e.Phone, e.Role, e.IsActive,
 e.CreatedAt, e.UpdatedAt,
 u.IdUser, u.UserName, u.DisplayName, u.IsActive AS UserIsActive
 FROM dbo.Employees e LEFT JOIN dbo.Users u ON u.IdUser = e.IdUser
 WHERE e.IdEmployee=@IdEmployee;
END
GO

/* Soft delete */
CREATE OR ALTER PROCEDURE dbo.sp_Employees_SoftDelete
 @IdEmployee INT
AS
BEGIN
 SET NOCOUNT ON;
 IF NOT EXISTS (SELECT 1 FROM dbo.Employees WHERE IdEmployee=@IdEmployee)
 BEGIN RAISERROR('Empleado no encontrado.',16,1); RETURN; END
 UPDATE dbo.Employees SET IsActive=0, UpdatedAt=SYSDATETIME() WHERE IdEmployee=@IdEmployee;
 SELECT e.IdEmployee, e.FullName, e.Email, e.Phone, e.Role, e.IsActive,
 e.CreatedAt, e.UpdatedAt,
 u.IdUser, u.UserName, u.DisplayName, u.IsActive AS UserIsActive
 FROM dbo.Employees e LEFT JOIN dbo.Users u ON u.IdUser = e.IdUser
 WHERE e.IdEmployee=@IdEmployee;
END
GO

/* Crear Ticket POS */
CREATE OR ALTER PROCEDURE dbo.sp_Pos_CreateTicket
 @CreatedBy INT,
 @Tip DECIMAL(10,2) =0.00,
 @TaxRate DECIMAL(5,4) =0.1600,
 @Items dbo.PosItemTVP READONLY
AS
BEGIN
 SET NOCOUNT ON; SET XACT_ABORT ON;
 IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE IdUser=@CreatedBy AND IsActive=1)
 BEGIN RAISERROR('Usuario inválido o inactivo.',16,1); RETURN; END
 IF NOT EXISTS (SELECT 1 FROM @Items)
 BEGIN RAISERROR('Debe haber al menos un item.',16,1); RETURN; END
 ;WITH Det AS (
 SELECT i.IdMenuItem, i.Qty, m.Price AS UnitPrice,
 CAST(i.Qty * m.Price AS DECIMAL(10,2)) AS LineTotal
 FROM @Items i JOIN dbo.MenuItems m ON m.IdMenuItem=i.IdMenuItem AND m.IsActive=1
 ) SELECT * INTO #det FROM Det;
 IF NOT EXISTS (SELECT 1 FROM #det)
 BEGIN RAISERROR('Items inválidos (inactivos o inexistentes).',16,1); RETURN; END
 DECLARE @Subtotal DECIMAL(10,2) = (SELECT SUM(LineTotal) FROM #det);
 DECLARE @Tax DECIMAL(10,2) = ROUND(@Subtotal * @TaxRate,2);
 DECLARE @Total DECIMAL(10,2) = @Subtotal + @Tax + @Tip;
 BEGIN TRAN;
 INSERT INTO dbo.Tickets(Subtotal,Tax,Tip,Total,CreatedBy) VALUES(@Subtotal,@Tax,@Tip,@Total,@CreatedBy);
 DECLARE @IdTicket INT = SCOPE_IDENTITY();
 INSERT INTO dbo.TicketItems(IdTicket, IdMenuItem, Qty, UnitPrice)
 SELECT @IdTicket, IdMenuItem, Qty, UnitPrice FROM #det;
 COMMIT TRAN;
 SELECT t.IdTicket, t.CreatedAt, t.Subtotal, t.Tax, t.Tip, t.Total, t.CreatedBy
 FROM dbo.Tickets t WHERE t.IdTicket=@IdTicket;
 SELECT ti.IdTicketItem, ti.IdTicket, ti.IdMenuItem, mi.Name, ti.Qty, ti.UnitPrice,
 CAST(ti.Qty*ti.UnitPrice AS DECIMAL(10,2)) AS LineTotal
 FROM dbo.TicketItems ti JOIN dbo.MenuItems mi ON mi.IdMenuItem=ti.IdMenuItem
 WHERE ti.IdTicket=@IdTicket;
END
GO

/* Menú activo */
CREATE OR ALTER PROCEDURE dbo.sp_Menu_GetActive
AS
BEGIN
 SET NOCOUNT ON;
 SELECT IdMenuItem, Name, Description, Price
 FROM dbo.MenuItems WHERE IsActive=1 ORDER BY Name;
END
GO

/* Reportes */
CREATE PROCEDURE [dbo].[sp_RestJP_Sales_Report]
  @From     DATE        = NULL,      -- inicio (incluyente). Si es NULL: últimos 30 días
  @To       DATE        = NULL,      -- fin (incluyente). Si es NULL: hoy
  @GroupBy  NVARCHAR(10) = N'DAY'    -- 'DAY' | 'WEEK' | 'MONTH'
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------------------
    -- 1) Rango por defecto: últimos 30 días hasta hoy
    ------------------------------------------------------------
    IF @To   IS NULL SET @To   = CONVERT(date, SYSDATETIME());
    IF @From IS NULL SET @From = DATEADD(DAY, -30, @To);

    ------------------------------------------------------------
    -- 2) Filtrado base (t.CreatedAt en [@From, @To+1))
    ------------------------------------------------------------
    ;WITH src AS (
        SELECT
            t.CreatedAt,
            t.Subtotal,
            t.Tax,
            t.Tip,
            t.Total
        FROM dbo.Tickets AS t
        WHERE t.CreatedAt >= @From
          AND t.CreatedAt <  DATEADD(DAY, 1, @To) -- fin exclusivo
    ),
    buckets AS (
        SELECT
            CASE UPPER(@GroupBy)
                WHEN 'DAY' THEN
                    CAST(CAST(s.CreatedAt AS date) AS datetime2(0))

                WHEN 'WEEK' THEN
                    -- Lunes de esa semana (independiente de SET DATEFIRST)
                    CAST(
                        DATEADD(DAY,
                                -((DATEDIFF(DAY, 0, CAST(s.CreatedAt AS date)) + 6) % 7),
                                CAST(s.CreatedAt AS date)
                        ) AS datetime2(0)
                    )

                WHEN 'MONTH' THEN
                    CAST(DATEFROMPARTS(YEAR(s.CreatedAt), MONTH(s.CreatedAt), 1) AS datetime2(0))

                ELSE
                    CAST(CAST(s.CreatedAt AS date) AS datetime2(0))
            END AS BucketStart,
            s.Subtotal,
            s.Tax,
            s.Tip,
            s.Total
        FROM src AS s
    )
    SELECT
        b.BucketStart,                                         -- clave de agrupación (inicio de período)
        -- Etiqueta legible
        CASE UPPER(@GroupBy)
            WHEN 'DAY'   THEN CONVERT(nvarchar(10), b.BucketStart, 120)                       -- yyyy-MM-dd
            WHEN 'WEEK'  THEN CONCAT(CONVERT(nvarchar(10), b.BucketStart, 120), ' - ',
                                     CONVERT(nvarchar(10), DATEADD(DAY, 6, b.BucketStart), 120))
            WHEN 'MONTH' THEN CONCAT(DATENAME(YEAR, b.BucketStart), '-',
                                     RIGHT('0' + CONVERT(varchar(2), DATEPART(MONTH, b.BucketStart)), 2))
            ELSE CONVERT(nvarchar(10), b.BucketStart, 120)
        END AS Label,
        COUNT(*)                 AS Tickets,
        SUM(b.Subtotal)          AS Subtotal,
        SUM(b.Tax)               AS IVA,
        SUM(b.Tip)               AS Propina,
        SUM(b.Total)             AS Total
    FROM buckets AS b
    GROUP BY b.BucketStart
    ORDER BY b.BucketStart;
END
GO

-- CREAR
CREATE OR ALTER PROCEDURE dbo.sp_Menu_Create
  @Name        NVARCHAR(100),
  @Description NVARCHAR(250) = NULL,
  @Price       DECIMAL(10,2),
  @IsActive    BIT = 1
AS
BEGIN
  SET NOCOUNT ON;
  SET @Name = LTRIM(RTRIM(@Name));
  IF (@Name = '') BEGIN RAISERROR('Name es requerido.',16,1); RETURN; END
  IF (@Price < 0) BEGIN RAISERROR('Price inválido.',16,1); RETURN; END

  INSERT INTO dbo.MenuItems (Name, Description, Price, IsActive)
  VALUES (@Name, NULLIF(@Description,''), @Price, @IsActive);

  DECLARE @Id INT = SCOPE_IDENTITY();
  EXEC dbo.sp_Menu_GetById @IdMenuItem = @Id;
END
GO

-- ACTUALIZAR
CREATE OR ALTER PROCEDURE dbo.sp_Menu_Update
  @IdMenuItem  INT,
  @Name        NVARCHAR(100),
  @Description NVARCHAR(250) = NULL,
  @Price       DECIMAL(10,2),
  @IsActive    BIT
AS
BEGIN
  SET NOCOUNT ON;
  IF NOT EXISTS(SELECT 1 FROM dbo.MenuItems WHERE IdMenuItem = @IdMenuItem)
  BEGIN RAISERROR('Item no encontrado.',16,1); RETURN; END

  SET @Name = LTRIM(RTRIM(@Name));
  IF (@Name='') BEGIN RAISERROR('Name es requerido.',16,1); RETURN; END
  IF (@Price < 0) BEGIN RAISERROR('Price inválido.',16,1); RETURN; END

  UPDATE dbo.MenuItems
  SET Name=@Name,
      Description = NULLIF(@Description,''),
      Price=@Price,
      IsActive=@IsActive
  WHERE IdMenuItem=@IdMenuItem;

  EXEC dbo.sp_Menu_GetById @IdMenuItem;
END
GO

-- BAJA LÓGICA (soft delete = IsActive=0)
CREATE OR ALTER PROCEDURE dbo.sp_Menu_SoftDelete
  @IdMenuItem INT
AS
BEGIN
  SET NOCOUNT ON;
  IF NOT EXISTS(SELECT 1 FROM dbo.MenuItems WHERE IdMenuItem=@IdMenuItem)
  BEGIN RAISERROR('Item no encontrado.',16,1); RETURN; END

  UPDATE dbo.MenuItems SET IsActive=0 WHERE IdMenuItem=@IdMenuItem;
  EXEC dbo.sp_Menu_GetById @IdMenuItem;
END
GO
GO
ALTER PROCEDURE dbo.sp_RestJP_Sales_Report
	@FechaInicio		DATETIME,
	@FechaFin			DATETIME
AS
BEGIN
	SELECT IdTicket, CreatedAt, Subtotal, Tax, Total
	FROM Tickets
	WHERE CreatedAt BETWEEN @FechaInicio AND @FechaFin
END;
GO

/*5. Datos iniciales (seed idempotente) */
PRINT 'Seed de MenuItems...';
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Ramen Shoyu')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Ramen Shoyu',N'Caldo de soya, chashu, alga nori',120.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Ramen Miso')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Ramen Miso',N'Caldo de miso, maíz, mantequilla',130.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Ramen Picante')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Ramen Picante',N'Caldo rojo, pasta de chile, ajitama',135.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Niguiri Salmón (2p)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Niguiri Salmón (2p)',N'Salmón fresco sobre arroz sushi',60.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'California Roll (8p)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'California Roll (8p)',N'Cangrejo, aguacate, pepino',95.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Spicy Tuna Roll (8p)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Spicy Tuna Roll (8p)',N'Atún picante, cebollín',115.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Dragon Roll (8p)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Dragon Roll (8p)',N'Anguila, aguacate, salsa unagi',165.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Sushi Mix12p')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Sushi Mix12p',N'Variedad del día (12 piezas)',180.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Gyoza (6)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Gyoza (6)',N'Empanadillas de cerdo con jengibre',75.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Edamame')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Edamame',N'Vainas de soya con sal de mar',55.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Karaage')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Karaage',N'Pollo frito estilo japonés',110.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Takoyaki (6)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Takoyaki (6)',N'Bolas de pulpo con katsuobushi',95.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Katsudon')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Katsudon',N'Cerdo empanizado con huevo y cebolla',135.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Gyudon')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Gyudon',N'Tazón de res con cebolla y salsa',125.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Sopa Miso')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Sopa Miso',N'Caldo dashi, miso, tofu y alga',35.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Ensalada Wakame')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Ensalada Wakame',N'Alga wakame aliñada',70.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Té Verde')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Té Verde',N'Caliente o frío',30.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Ramune')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Ramune',N'Refresco japonés (botella)',55.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Agua Mineral')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Agua Mineral',N'Botella500 ml',25.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Mochi (2)')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Mochi (2)',N'Relleno de helado (sabores del día)',75.00,1);
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems WHERE Name=N'Dorayaki')
INSERT INTO dbo.MenuItems(Name,Description,Price,IsActive) VALUES(N'Dorayaki',N'Panquesito relleno de anko',65.00,1);
GO

/* Usuario / Empleado inicial admin (solo si no existe) */
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserName='admin')
BEGIN
 EXEC dbo.sp_Employees_CreateWithUser @FullName='Administrador General', @Email='admin@restjp.local',
 @Phone='000-000', @Role='Admin', @UserName='admin', @PasswordText='admin123', @IsActive=1;
END
GO

/*6. Verificaciones rápidas */
SELECT TOP(5) IdUser, UserName, IsActive FROM dbo.Users ORDER BY IdUser;
SELECT TOP(5) IdEmployee, FullName, Role, IsActive FROM dbo.Employees ORDER BY IdEmployee;
SELECT TOP(5) IdMenuItem, Name, Price FROM dbo.MenuItems ORDER BY Name;
GO

/* FIN DEL SCRIPT */
