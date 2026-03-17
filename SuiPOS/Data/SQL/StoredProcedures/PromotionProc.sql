-- Lấy tất cả khuyến mãi
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetAllPromotions]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Now DATETIME = GETDATE();

    SELECT 
        Id, 
        Name, 
        Code, 
        [Type], 
        DiscountValue, 
        MinOrderAmount, 
        StartDate, 
        EndDate, 
        IsActive,
        CAST(CASE 
            WHEN IsActive = 1 AND StartDate <= @Now AND EndDate >= @Now THEN 1 
            ELSE 0 
        END AS BIT) AS IsValid
    FROM Promotions
    ORDER BY CreatedAt DESC;
END
GO

-- Lấy khuyến mãi hợp lệ cho đơn hàng dựa trên số tiền đơn hàng
CREATE OR ALTER PROCEDURE sp_GetValidPromotionsForOrder
    @OrderAmount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME = GETDATE();

    SELECT 
        Id, Name, Code, [Type], DiscountValue, MinOrderAmount, 
        StartDate, EndDate, IsActive,
        CAST(1 AS BIT) AS IsValid 
    FROM Promotions
    WHERE IsActive = 1
      AND StartDate <= @Now
      AND EndDate >= @Now
      AND (MinOrderAmount IS NULL OR MinOrderAmount <= @OrderAmount)
    ORDER BY 
        CASE WHEN MinOrderAmount IS NOT NULL THEN 1 ELSE 0 END DESC, 
        Name ASC;
END

-- Tìm kiếm sản phẩm bằng Id
CREATE OR ALTER PROCEDURE sp_GetPromotionById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, 
        Name, 
        Code, 
        CAST([Type] AS NVARCHAR(50)) AS [Type], 
        DiscountValue, 
        MinOrderAmount, 
        MaxDiscountAmount,
        StartDate, 
        EndDate, 
        IsActive
    FROM Promotions
    WHERE Id = @Id;
END

-- Thêm mã giảm giá
CREATE OR ALTER PROCEDURE sp_CreatePromotion
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Code NVARCHAR(50),
    @Type INT, 
    @DiscountValue DECIMAL(18,2),
    @MinOrderAmount DECIMAL(18,2),
    @MaxDiscountAmount DECIMAL(18,2),
    @StartDate DATETIME,
    @EndDate DATETIME,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM Promotions WHERE Code = @Code)
    BEGIN
        SELECT 0 AS Success, N'Mã khuyến mãi đã tồn tại' AS Message;
        RETURN;
    END

    IF (@EndDate <= @StartDate)
    BEGIN
        SELECT 0 AS Success, N'Ngày kết thúc phải sau ngày bắt đầu' AS Message;
        RETURN;
    END

    INSERT INTO Promotions (Id, Name, Code, [Type], DiscountValue, MinOrderAmount, MaxDiscountAmount, StartDate, EndDate, IsActive, CreatedAt)
    VALUES (@Id, @Name, @Code, @Type, @DiscountValue, @MinOrderAmount, @MaxDiscountAmount, @StartDate, @EndDate, @IsActive, GETUTCDATE());

    SELECT 1 AS Success, N'Thêm khuyến mãi thành công' AS Message;
END

-- Cập nhật mã giảm giá 
CREATE OR ALTER PROCEDURE sp_UpdatePromotion
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @Code NVARCHAR(50),
    @Type INT,
    @DiscountValue DECIMAL(18,2),
    @MinOrderAmount DECIMAL(18,2),
    @MaxDiscountAmount DECIMAL(18,2),
    @StartDate DATETIME,
    @EndDate DATETIME,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Promotions WHERE Id = @Id)
    BEGIN
        SELECT 0 AS Success, N'Không tìm thấy khuyến mãi' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Promotions WHERE Code = @Code AND Id <> @Id)
    BEGIN
        SELECT 0 AS Success, N'Mã khuyến mãi đã tồn tại' AS Message;
        RETURN;
    END

    IF (@EndDate <= @StartDate)
    BEGIN
        SELECT 0 AS Success, N'Ngày kết thúc phải sau ngày bắt đầu' AS Message;
        RETURN;
    END

    UPDATE Promotions
    SET Name = @Name,
        Code = @Code,
        [Type] = @Type,
        DiscountValue = @DiscountValue,
        MinOrderAmount = @MinOrderAmount,
        MaxDiscountAmount = @MaxDiscountAmount,
        StartDate = @StartDate,
        EndDate = @EndDate,
        IsActive = @IsActive,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;

    SELECT 1 AS Success, N'Cập nhật khuyến mãi thành công' AS Message;
END

-- Xóa mã giảm giá
CREATE OR ALTER PROCEDURE sp_DeletePromotion
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Promotions WHERE Id = @Id)
    BEGIN
        SELECT 0 AS Success, N'Không tìm thấy khuyến mãi' AS Message;
        RETURN;
    END

    DELETE FROM Promotions WHERE Id = @Id;

    SELECT 1 AS Success, N'Xóa khuyến mãi thành công' AS Message;
END

-- Chuyển đổi trạng thái của giảm giá
CREATE OR ALTER PROCEDURE sp_TogglePromotionActive
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Promotions WHERE Id = @Id)
    BEGIN
        SELECT 0 AS Success, N'Không tìm thấy khuyến mãi' AS Message;
        RETURN;
    END

    UPDATE Promotions 
    SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;

    DECLARE @NewStatus BIT;
    SELECT @NewStatus = IsActive FROM Promotions WHERE Id = @Id;

    IF (@NewStatus = 1)
        SELECT 1 AS Success, N'Đã kích hoạt khuyến mãi' AS Message;
    ELSE
        SELECT 1 AS Success, N'Đã vô hiệu hóa khuyến mãi' AS Message;
END
