-- Dictionary
CREATE OR ALTER PROCEDURE sp_GetSettingsByCategory
    @Category NVARCHAR(100)
AS
BEGIN
    SELECT [Key], [Value] 
    FROM SystemSettings 
    WHERE Category = @Category;
END

-- Cập nhật thông tin Setting
CREATE OR ALTER PROCEDURE sp_UpdateSystemSettings
    @JsonData NVARCHAR(MAX),
    @UpdatedBy UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        MERGE INTO SystemSettings AS target
        USING (
            SELECT 
                [key], 
                [value],
                CASE 
                    WHEN [key] LIKE 'store_%' OR [key] LIKE '%invoice_footer%' THEN 'Store'
                    WHEN [key] LIKE 'show_%' OR [key] LIKE '%print%' OR [key] LIKE '%paper%' THEN 'Invoice'
                    WHEN [key] LIKE '%printer%' THEN 'Printer'
                    ELSE 'General'
                END AS Category,
                CASE 
                    WHEN [value] IN ('true', 'false') THEN 'bool'
                    WHEN ISNUMERIC([value]) = 1 AND [value] NOT LIKE '%.%' THEN 'int'
                    WHEN ISNUMERIC([value]) = 1 THEN 'decimal'
                    ELSE 'string'
                END AS DataType
            FROM OPENJSON(@JsonData)
            WITH ([key] NVARCHAR(100), [value] NVARCHAR(MAX))
        ) AS src
        ON (target.[Key] = src.[key])
        
        WHEN MATCHED THEN
            UPDATE SET 
                Value = src.[value],
                UpdatedAt = GETUTCDATE(),
                UpdatedBy = @UpdatedBy
        
        WHEN NOT MATCHED THEN
            INSERT (Id, [Key], Value, Category, DataType, CreatedAt, UpdatedBy)
            VALUES (NEWID(), src.[key], src.[value], src.Category, src.DataType, GETUTCDATE(), @UpdatedBy);

        COMMIT TRANSACTION;
        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END