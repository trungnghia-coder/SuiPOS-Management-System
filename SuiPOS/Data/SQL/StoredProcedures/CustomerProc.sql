-- Thêm khách hàng 
-- [sp_CreateCustomer]
CREATE OR ALTER PROCEDURE sp_CreateCustomer
    @Name NVARCHAR(100),
    @Phone NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.Customers WITH (NOLOCK) WHERE Phone = @Phone)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Số điện thoại đã tồn tại' AS Message;
        RETURN;
    END

    INSERT INTO dbo.Customers (Id, Name, Phone, IsActive, DebtAmount, TotalSpent, Points, CreatedAt)
    VALUES (NEWID(), @Name, @Phone, 1, 0, 0, 0, GETUTCDATE());

    SELECT CAST(1 AS INT) AS Success, N'Thành công' AS Message;
END

-- Xóa khách hàng 
-- [sp_DeleteCustomer]
CREATE OR ALTER PROCEDURE sp_DeleteCustomer
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Customers WHERE Id = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không tìm thấy khách hàng.' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Customers WHERE Id = @Id AND DebtAmount > 0)
    BEGIN
         SELECT CAST(0 AS INT) AS Success, N'Khách hàng còn nợ, không thể xóa.' AS Message;
         RETURN;
    END

    UPDATE Customers 
    SET IsActive = 0 
    WHERE Id = @Id;

    SELECT CAST(1 AS INT) AS Success, N'Xóa khách hàng thành công.' AS Message;
END

-- Lấy danh sách khách hàng 
-- [GetProductList]
CREATE OR ALTER PROCEDURE [dbo].[GetProductList]
    @PageNumber INT = 1,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    SELECT 
        p.Id,
        p.Name AS ProductName,
        p.ImageUrl,
        p.isActive,
        p.CategoryId,
        ISNULL(SUM(pv.Stock), 0) AS Inventory,
        ISNULL(SUM(pv.Stock), 0) AS Available,
        c.Name AS CategoryName,
        COUNT(*) OVER() AS TotalRecords 
    FROM Products p
    INNER JOIN Categories c ON p.CategoryId = c.Id
    LEFT JOIN ProductVariants pv ON p.Id = pv.ProductId
    WHERE p.isActive = 1
    GROUP BY p.Id, p.Name, p.ImageUrl, p.isActive, p.CategoryId, c.Name
    ORDER BY p.Name ASC
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- Tìm khách hàng bằng ID
-- [sp_GetCustomerById]
CREATE OR ALTER PROCEDURE sp_GetCustomerById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, 
        Name, 
        Phone AS PhoneNumber, 
        DebtAmount,
        TotalSpent
    FROM Customers WITH (NOLOCK)
    WHERE Id = @Id;
END

-- Cập nhật khách hàng 
-- [sp_UpdateCustomer]
CREATE OR ALTER PROCEDURE sp_UpdateCustomer
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @Phone NVARCHAR(20),
    @DebtAmount DECIMAL(18,2) = NULL,
    @TotalSpent DECIMAL(18,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Customers WHERE Id = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không tìm thấy khách hàng' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Customers WHERE Phone = @Phone AND Id <> @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Số điện thoại này đã được khách hàng khác sử dụng.' AS Message;
        RETURN;
    END

    UPDATE Customers
    SET Name = @Name,
        Phone = @Phone,
        DebtAmount = COALESCE(@DebtAmount, DebtAmount),
        TotalSpent = COALESCE(@TotalSpent, TotalSpent)
    WHERE Id = @Id;

    SELECT CAST(1 AS INT) AS Success, N'Khách hàng đã cập nhật thành công.' AS Message;
END

-- Tìm kiếm khách hàng bằng Id, tên, số điện thoại
-- [sp_SearchCustomers]
CREATE OR ALTER PROCEDURE sp_SearchCustomers
    @Query NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 10 
        Id, 
        Name, 
        Phone
    FROM Customers WITH (NOLOCK)
    WHERE IsActive = 1 
      AND (Name LIKE N'%' + @Query + N'%' OR Phone LIKE '%' + @Query + '%')
    ORDER BY Name;
END



