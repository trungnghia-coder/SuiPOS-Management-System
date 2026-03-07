-- 1. Lấy tất cả danh mục sản phẩm
-- [sp_GetAllCategories]
CREATE OR ALTER PROCEDURE sp_GetAllCategories
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Id, 
        c.Name, 
        COUNT(p.Id) AS ProductCount,
        CAST(1 AS BIT) AS IsActive
    FROM Categories c WITH (NOLOCK)
    LEFT JOIN Products p ON c.Id = p.CategoryId
    GROUP BY c.Id, c.Name
    ORDER BY c.Name ASC;
END

-- 2. Lấy danh mục sản phẩm bằng ID 
-- [sp_GetCategoryById]
CREATE OR ALTER PROCEDURE sp_GetCategoryById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, 
        Name,
        -- Nếu Nghĩa muốn lấy thêm số lượng sản phẩm ở trang chi tiết thì dùng subquery này
        (SELECT COUNT(*) FROM Products WHERE CategoryId = @Id) AS ProductCount,
        CAST(1 AS BIT) AS IsActive
    FROM Categories WITH (NOLOCK)
    WHERE Id = @Id;
END

-- 3. Tạo danh mục sản phẩm 
-- [sp_CreateCategory]
CREATE OR ALTER PROCEDURE sp_CreateCategory
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Categories WHERE Name = @Name)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Tên loại sản phẩm này đã tồn tại.' AS Message;
        RETURN;
    END

    INSERT INTO Categories (Id, Name)
    VALUES (NEWID(), @Name);

    SELECT CAST(1 AS INT) AS Success, N'Thêm loại sản phẩm thành công.' AS Message;
END

-- 4. Cập nhật danh mục sản phẩm 
-- [sp_UpdateCategory]
CREATE OR ALTER PROCEDURE sp_UpdateCategory
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không tìm thấy loại sản phẩm.' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Categories WHERE Name = @Name AND Id <> @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Tên loại sản phẩm này đã tồn tại.' AS Message;
        RETURN;
    END

    UPDATE Categories 
    SET Name = @Name 
    WHERE Id = @Id;

    SELECT CAST(1 AS INT) AS Success, N'Cập nhật thành công.' AS Message;
END

-- 5. Xóa danh mục sản phẩm 
-- [sp_DeleteCategory]
CREATE OR ALTER PROCEDURE sp_DeleteCategory
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE Id = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Loại sản phẩm không tồn tại.' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM Products WHERE CategoryId = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không thể xóa vì loại này đang chứa sản phẩm. Hãy xóa sản phẩm trước.' AS Message;
        RETURN;
    END

    DELETE FROM Categories WHERE Id = @Id;

    SELECT CAST(1 AS INT) AS Success, N'Xóa thành công.' AS Message;
END
