-- 1.1 Lấy danh sách tất cả thuộc tính
-- [sp_GetAllProductAttributes]
CREATE PROCEDURE sp_GetAllProductAttributes
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.Id, 
        a.Name,
        (SELECT COUNT(*) FROM AttributeValues v WHERE v.AttributeId = a.Id) AS ValueCount,
        STUFF((
            SELECT TOP 3 ', ' + v.Value
            FROM AttributeValues v
            WHERE v.AttributeId = a.Id
            ORDER BY v.Value
            FOR XML PATH('')), 1, 2, '') AS SampleValuesRaw
    FROM ProductAttributes a
    ORDER BY a.Name;
END

-- 1.2 Lấy chi tiết thuộc tính theo ID
-- [sp_GetAttributeById]
CREATE PROCEDURE sp_GetAllAttributesWithValues
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.Id, 
        a.Name,
        (
            SELECT v.Id, v.Value, v.AttributeId
            FROM AttributeValues v
            WHERE v.AttributeId = a.Id
            FOR JSON PATH
        ) AS ValuesJson 
    FROM ProductAttributes a
    ORDER BY a.Name;
END

CREATE PROCEDURE sp_GetAttributeById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        a.Id, 
        a.Name,
        (
            SELECT v.Id, v.Value, v.AttributeId
            FROM AttributeValues v
            WHERE v.AttributeId = a.Id
            FOR JSON PATH
        ) AS ValuesJson
    FROM ProductAttributes a
    WHERE a.Id = @Id;
END

-- 1.3 Tạo mới thuộc tính
-- [sp_CreateProductAttribute]
CREATE PROCEDURE sp_CreateProductAttribute
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM ProductAttributes WHERE Name = @Name)
    BEGIN
        SELECT 0 AS Success, N'Thuộc tính này đã tồn tại' AS Message;
        RETURN;
    END

    INSERT INTO ProductAttributes (Id, Name)
    VALUES (NEWID(), @Name);

    SELECT 1 AS Success, N'Tạo thuộc tính thành công' AS Message;
END

-- 1.4 Cập nhật thuộc tính
-- [sp_UpdateProductAttribute]
CREATE PROCEDURE sp_UpdateProductAttribute
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM ProductAttributes WHERE Id = @Id)
    BEGIN
        SELECT 0 AS Success, N'Không tìm thấy thuộc tính' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM ProductAttributes WHERE Name = @Name AND Id <> @Id)
    BEGIN
        SELECT 0 AS Success, N'Tên thuộc tính đã tồn tại' AS Message;
        RETURN;
    END

    UPDATE ProductAttributes 
    SET Name = @Name 
    WHERE Id = @Id;

    SELECT 1 AS Success, N'Cập nhật thuộc tính thành công' AS Message;
END

-- 1.5 Xóa thuộc tính (Xóa luôn các giá trị con liên quan)
-- [sp_DeleteProductAttribute]
CREATE PROCEDURE sp_DeleteProductAttribute
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @LinkingTable NVARCHAR(255);
    DECLARE @LinkingColumn NVARCHAR(255);
    
    SELECT TOP 1 
        @LinkingTable = OBJECT_NAME(parent_object_id),
        @LinkingColumn = COL_NAME(parent_object_id, parent_column_id)
    FROM sys.foreign_key_columns
    WHERE referenced_object_id = OBJECT_ID('AttributeValues')
      AND OBJECT_NAME(parent_object_id) LIKE '%ProductVariant%';

    IF @LinkingTable IS NOT NULL
    BEGIN
        DECLARE @CheckSql NVARCHAR(MAX) = 
            N'IF EXISTS (SELECT 1 FROM ' + QUOTENAME(@LinkingTable) + 
            N' WHERE ' + QUOTENAME(@LinkingColumn) + 
            N' IN (SELECT Id FROM AttributeValues WHERE AttributeId = @AttrId))
              SET @IsUsed = 1 ELSE SET @IsUsed = 0';
        
        DECLARE @IsUsed INT = 0;
        DECLARE @ParamDef NVARCHAR(500) = N'@AttrId UNIQUEIDENTIFIER, @IsUsed INT OUTPUT';
        
        EXEC sp_executesql @CheckSql, @ParamDef, @AttrId = @Id, @IsUsed = @IsUsed OUTPUT;

        IF @IsUsed = 1
        BEGIN
            SELECT CAST(0 AS BIT) AS Success, N'Không thể xóa: Thuộc tính đang được sử dụng bởi sản phẩm' AS Message;
            RETURN;
        END
    END

    DELETE FROM AttributeValues WHERE AttributeId = @Id;
    DELETE FROM ProductAttributes WHERE Id = @Id;

    IF @@ROWCOUNT > 0
        SELECT CAST(1 AS BIT) AS Success, N'Xóa thuộc tính thành công' AS Message;
    ELSE
        SELECT CAST(0 AS BIT) AS Success, N'Không tìm thấy thuộc tính' AS Message;
END

-- 1.6 Thêm giá trị mới
-- [sp_AddAttributeValue]
Create PROCEDURE sp_AddAttributeValue
    @AttributeId UNIQUEIDENTIFIER,
    @Value NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM ProductAttributes WHERE Id = @AttributeId)
    BEGIN
        SELECT CAST(0 AS BIT) AS Success, N'Không tìm thấy thuộc tính' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM AttributeValues WHERE AttributeId = @AttributeId AND [Value] = @Value)
    BEGIN
        SELECT CAST(0 AS BIT) AS Success, N'Giá trị này đã tồn tại' AS Message;
        RETURN;
    END

    INSERT INTO AttributeValues (Id, AttributeId, [Value])
    VALUES (NEWID(), @AttributeId, @Value);

    SELECT CAST(1 AS BIT) AS Success, N'Thêm giá trị thành công' AS Message;
END

-- 1.7 Cập nhật giá trị
-- [sp_UpdateAttributeValue]
CREATE PROCEDURE sp_UpdateAttributeValue
    @Id UNIQUEIDENTIFIER,
    @Value NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AttributeId UNIQUEIDENTIFIER;
    SELECT @AttributeId = AttributeId FROM AttributeValues WHERE Id = @Id;

    IF @AttributeId IS NULL
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không tìm thấy giá trị' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM AttributeValues 
               WHERE AttributeId = @AttributeId AND [Value] = @Value AND Id <> @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Giá trị này đã tồn tại' AS Message;
        RETURN;
    END

    UPDATE AttributeValues SET [Value] = @Value WHERE Id = @Id;

    SELECT CAST(1 AS INT) AS Success, N'Cập nhật giá trị thành công' AS Message;
END

-- 1.8 Cập nhật giá trị
-- [sp_UpdateAttributeValue]
CREATE PROCEDURE sp_DeleteAttributeValue
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM AttributeValues WHERE Id = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không tìm thấy giá trị' AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM AttributeValues WHERE Id = @Id)
    BEGIN
        SELECT CAST(0 AS INT) AS Success, N'Không thể xóa giá trị đang được sử dụng bởi sản phẩm' AS Message;
        RETURN;
    END

    DELETE FROM AttributeValues WHERE Id = @Id;

    SELECT CAST(1 AS INT) AS Success, N'Xóa giá trị thành công' AS Message;
END

-- 1.9 Lấy danh sách giá trị theo ID thuộc tính cha
-- [sp_GetValuesByAttributeId]
CREATE PROCEDURE sp_GetValuesByAttributeId
    @AttributeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, 
        [Value], 
        AttributeId
    FROM AttributeValues WITH (NOLOCK)
    WHERE AttributeId = @AttributeId
    ORDER BY [Value] ASC;
END