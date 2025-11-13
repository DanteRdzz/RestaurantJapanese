-- Script de prueba para verificar el stored procedure de reportes
-- Ejecuta este script en SQL Server Management Studio para verificar
-- cuántos registros devuelve realmente el SP

USE REST_JP;
GO

-- Primero, ver cuántos tickets hay en total
SELECT COUNT(*) AS TotalTicketsEnBD FROM dbo.Tickets;

-- Ver los últimos tickets creados
SELECT TOP 15 
    IdTicket, 
    CreatedAt, 
    Subtotal, 
    Tax, 
    Total,
    CAST(CreatedAt AS DATE) AS FechaSolo
FROM dbo.Tickets 
ORDER BY CreatedAt DESC;

-- Probar el SP con un rango amplio (últimos 60 días)
DECLARE @FechaInicio DATETIME = DATEADD(DAY, -60, GETDATE());
DECLARE @FechaFin DATETIME = GETDATE();

PRINT 'Ejecutando SP con rango de ' + CAST(@FechaInicio AS VARCHAR(20)) + ' a ' + CAST(@FechaFin AS VARCHAR(20));

EXEC dbo.sp_RestJP_Sales_Report 
    @FechaInicio = @FechaInicio,
    @FechaFin = @FechaFin;

-- Ver cuántos registros devolvió
SELECT @@ROWCOUNT AS RegistrosDevueltosPorSP;