-- Tạo hóa đơn 
-- [dbo].[sp_CreateOrder]
CREATE OR ALTER   PROCEDURE [dbo].[sp_CreateOrder]
    @OrderData NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CustomerId UNIQUEIDENTIFIER = JSON_VALUE(@OrderData, '$.CustomerId');
        DECLARE @StaffId UNIQUEIDENTIFIER = JSON_VALUE(@OrderData, '$.StaffId');
        DECLARE @AmountReceived DECIMAL(18,2) = CAST(JSON_VALUE(@OrderData, '$.AmountReceived') AS DECIMAL(18,2));
        DECLARE @Discount DECIMAL(18,2) = ISNULL(CAST(JSON_VALUE(@OrderData, '$.DiscountAmount') AS DECIMAL(18,2)), 0);
        DECLARE @Note NVARCHAR(MAX) = JSON_VALUE(@OrderData, '$.Note');
        
        DECLARE @OrderId UNIQUEIDENTIFIER = NEWID();
        DECLARE @OrderCode NVARCHAR(20) = 'ORD' + FORMAT(GETUTCDATE(), 'yyyyMMddHHmmss');

        DECLARE @TotalAmount DECIMAL(18,2);
        SELECT @TotalAmount = SUM(CAST(JSON_VALUE(value, '$.Quantity') AS INT) * CAST(JSON_VALUE(value, '$.UnitPrice') AS DECIMAL(18,2)))
        FROM OPENJSON(@OrderData, '$.Items');

        DECLARE @FinalAmount DECIMAL(18,2) = @TotalAmount - @Discount;

        IF EXISTS (
            SELECT 1 FROM OPENJSON(@OrderData, '$.Items') AS Item
            JOIN ProductVariants V ON CAST(JSON_VALUE(Item.value, '$.VariantId') AS UNIQUEIDENTIFIER) = V.Id
            WHERE V.Stock < CAST(JSON_VALUE(Item.value, '$.Quantity') AS INT)
        )
        BEGIN
            ROLLBACK;
            SELECT 0 AS Success, N'Sản phẩm trong kho không đủ!' AS Message, NULL AS OrderId;
            RETURN;
        END

        INSERT INTO Orders (Id, OrderCode, CustomerId, StaffId, TotalAmount, OrderDate, AmountReceived, Discount, Note, Status)
        VALUES (@OrderId, @OrderCode, @CustomerId, @StaffId, @TotalAmount, GETUTCDATE(), @AmountReceived, @Discount, @Note, 'Completed');

        INSERT INTO OrderDetails (Id, OrderId, ProductVariantId, Quantity, UnitPrice)
        SELECT NEWID(), @OrderId, 
               CAST(JSON_VALUE(value, '$.VariantId') AS UNIQUEIDENTIFIER), 
               CAST(JSON_VALUE(value, '$.Quantity') AS INT), 
               CAST(JSON_VALUE(value, '$.UnitPrice') AS DECIMAL(18,2))
        FROM OPENJSON(@OrderData, '$.Items');

        UPDATE V
        SET V.Stock = V.Stock - Item.Qty
        FROM ProductVariants V
        JOIN (
            SELECT CAST(JSON_VALUE(value, '$.VariantId') AS UNIQUEIDENTIFIER) AS VId, 
                   CAST(JSON_VALUE(value, '$.Quantity') AS INT) AS Qty
            FROM OPENJSON(@OrderData, '$.Items')
        ) Item ON V.Id = Item.VId;

        IF @CustomerId IS NOT NULL
        BEGIN
            UPDATE Customers
            SET TotalSpent = TotalSpent + @FinalAmount,
                DebtAmount = DebtAmount + CASE WHEN @FinalAmount > @AmountReceived THEN @FinalAmount - @AmountReceived ELSE 0 END
            WHERE Id = @CustomerId;
        END

        COMMIT;
        SELECT 1 AS Success, N'Thanh toán thành công!' AS Message, @OrderId AS OrderId;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        DECLARE @ErrMsg NVARCHAR(MAX) = ERROR_MESSAGE();
        SELECT 0 AS Success, @ErrMsg AS Message, NULL AS OrderId;
    END CATCH
END
GO

-- Lấy sản phẩm bằng ID 
-- [dbo].[sp_GetOrderById]
CREATE OR ALTER PROCEDURE sp_GetOrderById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        O.Id, O.OrderCode, O.TotalAmount, O.AmountReceived, O.ChangeAmount, 
        O.Discount, O.Status, O.Note, O.OrderDate,
        C.Name AS CustomerName, C.Phone AS CustomerPhone,
        S.FullName AS StaffName
    FROM Orders O WITH (NOLOCK)
    LEFT JOIN Customers C ON O.CustomerId = C.Id
    LEFT JOIN Staffs S ON O.StaffId = S.Id
    WHERE O.Id = @Id;

    SELECT 
        OD.ProductVariantId AS VariantId,
        P.Name AS ProductName,
        PV.VariantCombination AS VariantName,
        PV.SKU,
        P.ImageUrl,
        OD.Quantity,
        OD.UnitPrice
    FROM OrderDetails OD WITH (NOLOCK)
    JOIN ProductVariants PV ON OD.ProductVariantId = PV.Id
    JOIN Products P ON PV.ProductId = P.Id
    WHERE OD.OrderId = @Id;

    SELECT 
        PaymentMethod AS Method,
        Amount,
        TransactionReference AS Reference
    FROM Payments WITH (NOLOCK)
    WHERE OrderId = @Id;
END

-- Lấy danh sách hóa đơn
-- [dbo].[sp_GetOrdersList]
CREATE OR ALTER PROCEDURE sp_GetOrdersList
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ActualToDate DATETIME = NULL;
    IF @ToDate IS NOT NULL 
        SET @ActualToDate = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(@ToDate AS DATE)));

    SELECT 
        O.Id,
        O.OrderCode,
        ISNULL(C.Name, N'Khách lẻ') AS CustomerName,
        O.TotalAmount,
        O.Status,
        O.OrderDate
    FROM Orders O WITH (NOLOCK)
    LEFT JOIN Customers C ON O.CustomerId = C.Id
    WHERE (@FromDate IS NULL OR O.OrderDate >= @FromDate)
      AND (@ActualToDate IS NULL OR O.OrderDate <= @ActualToDate)
    ORDER BY O.OrderDate DESC;
END

-- Lấy danh sách sản phẩm dựa trên ngày 
-- [dbo].[sp_GetOrdersList]
CREATE OR ALTER PROCEDURE sp_GetOrdersList
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ActualToDate DATETIME = NULL;
    IF @ToDate IS NOT NULL 
    BEGIN
        SET @ActualToDate = DATEADD(SECOND, -1, CAST(DATEADD(DAY, 1, CAST(@ToDate AS DATE)) AS DATETIME));
    END

    SELECT 
        O.Id,
        O.OrderCode,
        ISNULL(C.Name, N'Khách lẻ') AS CustomerName,
        O.TotalAmount,
        O.Status,
        O.OrderDate
    FROM Orders O WITH (NOLOCK)
    LEFT JOIN Customers C ON O.CustomerId = C.Id
    WHERE (@FromDate IS NULL OR O.OrderDate >= @FromDate)
      AND (@ActualToDate IS NULL OR O.OrderDate <= @ActualToDate)
    ORDER BY O.OrderDate DESC;
END

-- Hủy đơn hàng 
-- [dbo].[sp_CancelOrder]
CREATE OR ALTER PROCEDURE sp_CancelOrder
    @OrderId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CurrentStatus NVARCHAR(50);
        SELECT @CurrentStatus = Status FROM Orders WHERE Id = @OrderId;

        IF @CurrentStatus IS NULL
        BEGIN
            ROLLBACK;
            SELECT 0 AS Success, N'Không tìm thấy đơn hàng' AS Message;
            RETURN;
        END

        IF @CurrentStatus = 'Cancelled'
        BEGIN
            ROLLBACK;
            SELECT 0 AS Success, N'Đơn hàng đã bị hủy trước đó' AS Message;
            RETURN;
        END

        UPDATE V
        SET V.Stock = V.Stock + OD.Quantity
        FROM ProductVariants V
        JOIN OrderDetails OD ON V.Id = OD.ProductVariantId
        WHERE OD.OrderId = @OrderId;

        UPDATE Orders SET Status = 'Cancelled' WHERE Id = @OrderId;

        COMMIT;
        SELECT 1 AS Success, N'Hủy đơn hàng thành công' AS Message;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END

-- Kiểm tra tồn kho trước khi đặt lại đơn hàng 
-- [dbo].[sp_ValidateStockForReorder]
CREATE OR ALTER PROCEDURE sp_ValidateStockForReorder
    @JsonItems NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TempResults TABLE (
        VariantId UNIQUEIDENTIFIER,
        ProductName NVARCHAR(250),
        VariantName NVARCHAR(250),
        SKU NVARCHAR(100),
        ImageUrl NVARCHAR(MAX),
        UnitPrice DECIMAL(18,2),
        CurrentStock INT,
        RequestedQuantity INT,
        IsActive BIT,
        IsAvailable BIT,
        Note NVARCHAR(MAX)
    );

    INSERT INTO @TempResults
    SELECT 
        V.Id, P.Name, V.VariantCombination, V.SKU, P.ImageUrl, V.Price,
        V.Stock, Item.Quantity, P.IsActive,
        CASE WHEN P.IsActive = 1 AND V.Stock >= Item.Quantity THEN 1 ELSE 0 END,
        CASE 
            WHEN P.IsActive = 0 THEN N'Sản phẩm ngừng kinh doanh'
            WHEN V.Stock < Item.Quantity THEN N'Kho còn ' + CAST(V.Stock AS NVARCHAR) + N', cần ' + CAST(Item.Quantity AS NVARCHAR)
            ELSE N'Hợp lệ'
        END
    FROM OPENJSON(@JsonItems) 
    WITH (VariantId UNIQUEIDENTIFIER '$.VariantId', Quantity INT '$.Quantity') AS Item
    JOIN ProductVariants V ON Item.VariantId = V.Id
    JOIN Products P ON V.ProductId = P.Id;

    DECLARE @ErrorCount INT = (SELECT COUNT(*) FROM @TempResults WHERE IsAvailable = 0);
    DECLARE @IsSuccess BIT = CASE WHEN @ErrorCount = 0 THEN 1 ELSE 0 END;
    DECLARE @FinalMessage NVARCHAR(MAX) = CASE 
        WHEN @ErrorCount = 0 THEN N'Tất cả sản phẩm khả dụng'
        ELSE CAST(@ErrorCount AS NVARCHAR) + N' món gặp vấn đề'
    END;

    SELECT @IsSuccess AS Success, @FinalMessage AS Message;

    SELECT * FROM @TempResults;
END