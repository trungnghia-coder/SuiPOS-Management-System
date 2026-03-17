-- 1. Tạo sản phẩm mới
CREATE OR ALTER PROCEDURE [dbo].[sp_CreateProduct]
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @CategoryId UNIQUEIDENTIFIER,
    @ImageUrl NVARCHAR(MAX),
    @Variants dbo.VariantType READONLY 
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Products (Id, Name, CategoryId, ImageUrl, isActive)
        VALUES (@Id, @Name, @CategoryId, @ImageUrl, 1);

        DECLARE @InsertedVariants TABLE (VId UNIQUEIDENTIFIER, AttrIds NVARCHAR(MAX));

        MERGE INTO ProductVariants AS target
        USING @Variants AS src
        ON (1 = 0) 
        WHEN NOT MATCHED THEN
            INSERT (Id, ProductId, SKU, Price, Stock, VariantCombination)
            VALUES (NEWID(), @Id, src.SKU, src.Price, src.Stock, src.Combination)
            OUTPUT inserted.Id, src.AttributeValueIds INTO @InsertedVariants(VId, AttrIds);

        INSERT INTO ProductVariantAttributeValues (SelectedValuesId, ProductVariantId)
        SELECT 
            CAST(s.value AS UNIQUEIDENTIFIER), 
            iv.VId
        FROM @InsertedVariants AS iv
        CROSS APPLY STRING_SPLIT(iv.AttrIds, ',') AS s
        WHERE ISNULL(iv.AttrIds, '') <> '';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Err NVARCHAR(MAX) = ERROR_MESSAGE();
        RAISERROR(@Err, 16, 1);
    END CATCH
END
GO

-- Xóa sản phẩm bằng id
CREATE OR ALTER PROCEDURE sp_DeleteProduct
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Products WHERE Id = @Id)
        BEGIN
            ROLLBACK;
            SELECT 0 AS Success, N'Sản phẩm không tồn tại' AS Message;
            RETURN;
        END

        UPDATE Products SET isActive = 0 WHERE Id = @Id;

        UPDATE ProductVariants SET Stock = 0 WHERE ProductId = @Id; 

        COMMIT;
        SELECT 1 AS Success, N'Xóa sản phẩm thành công' AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;
        SELECT 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END

-- Cập nhật sản phẩm
ALTER PROCEDURE [dbo].[sp_UpdateProduct]
    @Id UNIQUEIDENTIFIER,
    @Name NVARCHAR(200),
    @CategoryId UNIQUEIDENTIFIER,
    @ImageUrl NVARCHAR(MAX),
    @Variants dbo.VariantType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE Products 
        SET Name = @Name, 
            CategoryId = @CategoryId, 
            ImageUrl = ISNULL(@ImageUrl, ImageUrl)
        WHERE Id = @Id;

        DECLARE @UpdatedVariantIds TABLE (VId UNIQUEIDENTIFIER, AttrIds NVARCHAR(MAX));

        MERGE INTO ProductVariants AS target
        USING @Variants AS src
        ON (target.ProductId = @Id AND target.SKU = src.SKU)
        
        WHEN MATCHED THEN
            UPDATE SET 
                Price = src.Price, 
                Stock = src.Stock, 
                VariantCombination = src.Combination

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (Id, ProductId, SKU, Price, Stock, VariantCombination)
            VALUES (NEWID(), @Id, src.SKU, src.Price, src.Stock, src.Combination)

        WHEN NOT MATCHED BY SOURCE AND target.ProductId = @Id THEN
            DELETE
            
        OUTPUT ISNULL(inserted.Id, deleted.Id), src.AttributeValueIds 
        INTO @UpdatedVariantIds(VId, AttrIds);

        DELETE FROM ProductVariantAttributeValues 
        WHERE ProductVariantId IN (SELECT Id FROM ProductVariants WHERE ProductId = @Id);

        INSERT INTO ProductVariantAttributeValues (SelectedValuesId, ProductVariantId)
        SELECT CAST(s.value AS UNIQUEIDENTIFIER), uv.VId
        FROM @UpdatedVariantIds uv
        CROSS APPLY STRING_SPLIT(uv.AttrIds, ',') s
        WHERE ISNULL(uv.AttrIds, '') <> '';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Tìm sản phẩm bằng id 
CREATE OR ALTER   PROCEDURE [dbo].[sp_GetProductById]
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.Id, 
        p.Name AS ProductName, 
        p.CategoryId, 
        c.Name AS CategoryName, 
        p.ImageUrl, 
        p.isActive
    FROM Products p
    LEFT JOIN Categories c ON p.CategoryId = c.Id
    WHERE p.Id = @Id;

    SELECT 
        v.Id, 
        v.SKU, 
        v.Price, 
        v.Stock, 
        v.VariantCombination AS Combination,
        (
            SELECT av.Id, av.Value
            FROM AttributeValues av
            INNER JOIN ProductVariantAttributeValues pvav ON av.Id = pvav.SelectedValuesId
            WHERE pvav.ProductVariantId = v.Id
            FOR JSON PATH
        ) AS SelectedValuesJson 
    FROM ProductVariants v
    WHERE v.ProductId = @Id;
END
GO