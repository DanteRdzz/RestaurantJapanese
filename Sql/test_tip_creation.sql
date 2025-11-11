/* =============================================================
 RestaurantJapanese - Prueba de propina
 Script para probar manualmente la creación de tickets con propina
 ============================================================= */

USE REST_JP;
GO

-- Asumiendo que tienes un usuario con ID 1 (el admin por defecto)
-- y algunos items del menú

DECLARE @CreatedBy INT = 1
DECLARE @Tip DECIMAL(10,2) = 25.50
DECLARE @TaxRate DECIMAL(5,4) = 0.1600

-- Crear TVP con algunos items de prueba
DECLARE @Items dbo.PosItemTVP
INSERT INTO @Items (IdMenuItem, Qty)
SELECT TOP 2 IdMenuItem, 1
FROM dbo.MenuItems 
WHERE IsActive = 1

-- Mostrar lo que vamos a insertar
PRINT 'Items a insertar:'
SELECT * FROM @Items

-- Ejecutar el stored procedure
EXEC dbo.sp_Pos_CreateTicket 
    @CreatedBy = @CreatedBy,
    @Tip = @Tip,
    @TaxRate = @TaxRate,
    @Items = @Items

-- Verificar el resultado
PRINT 'Último ticket creado:'
SELECT TOP 1 * FROM dbo.Tickets ORDER BY IdTicket DESC