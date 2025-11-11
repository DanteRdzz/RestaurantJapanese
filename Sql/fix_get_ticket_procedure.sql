/* =============================================================
 RestaurantJapanese - Fix para GetTicket
 Ejecuta este script para agregar el stored procedure faltante
 ============================================================= */

USE REST_JP;
GO

/* Obtener Ticket por ID */
CREATE OR ALTER PROCEDURE dbo.sp_Pos_GetTicket
    @IdTicket INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Obtener header del ticket
    SELECT 
        t.IdTicket, 
        t.CreatedAt, 
        t.Subtotal, 
        t.Tax, 
        t.Tip, 
        t.Total, 
        t.CreatedBy,
        u.UserName
    FROM dbo.Tickets t 
    LEFT JOIN dbo.Users u ON u.IdUser = t.CreatedBy
    WHERE t.IdTicket = @IdTicket;
    
    -- Obtener detalles del ticket
    SELECT 
        ti.IdTicketItem, 
        ti.IdTicket, 
        ti.IdMenuItem, 
        mi.Name, 
        ti.Qty, 
        ti.UnitPrice,
        CAST(ti.Qty * ti.UnitPrice AS DECIMAL(10,2)) AS LineTotal
    FROM dbo.TicketItems ti 
    JOIN dbo.MenuItems mi ON mi.IdMenuItem = ti.IdMenuItem
    WHERE ti.IdTicket = @IdTicket
    ORDER BY ti.IdTicketItem;
END
GO

-- Verificar que el stored procedure se creó correctamente
IF OBJECT_ID('dbo.sp_Pos_GetTicket') IS NOT NULL
    PRINT 'Stored procedure sp_Pos_GetTicket creado exitosamente'
ELSE
    PRINT 'ERROR: No se pudo crear el stored procedure sp_Pos_GetTicket'

-- Verificar que el stored procedure de creación funciona con propinas
PRINT 'Verificando stored procedure sp_Pos_CreateTicket...'
IF OBJECT_ID('dbo.sp_Pos_CreateTicket') IS NOT NULL
    PRINT 'Stored procedure sp_Pos_CreateTicket existe'
ELSE
    PRINT 'ERROR: No existe el stored procedure sp_Pos_CreateTicket'

-- Mostrar un ticket de ejemplo si existe alguno
IF EXISTS (SELECT 1 FROM dbo.Tickets)
BEGIN
    PRINT 'Ejemplo de ticket existente:'
    SELECT TOP 1 IdTicket, Subtotal, Tax, Tip, Total FROM dbo.Tickets ORDER BY IdTicket DESC
END
ELSE
    PRINT 'No hay tickets en la base de datos para verificar'