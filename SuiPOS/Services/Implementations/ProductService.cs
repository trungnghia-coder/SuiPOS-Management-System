using Dapper;
using Microsoft.EntityFrameworkCore;
using SuiPOS.Data;
using SuiPOS.Services.Interfaces;
using SuiPOS.ViewModels;
using System.Data;

namespace SuiPOS.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IFileService _fileService;

        public ProductService(IFileService fileService, IDbConnectionFactory dbFactory)
        {
            _fileService = fileService;
            _dbFactory = dbFactory;
        }

        public async Task<(bool Success, string Message)> CreateAsync(ProductInputVM model)
        {
            try
            {
                string? uploadedUrl = null;
                if (model.ImageFile != null)
                {
                    uploadedUrl = await _fileService.UploadImageAsync(model.ImageFile);
                }

                var variantDataTable = ToVariantDataTable(model.Variants);

                using var connection = await _dbFactory.CreateConnectionAsync();

                var parameters = new DynamicParameters();
                parameters.Add("@Id", Guid.NewGuid());
                parameters.Add("@Name", model.Name);
                parameters.Add("@CategoryId", model.CategoryId);
                parameters.Add("@ImageUrl", uploadedUrl);

                parameters.Add("@Variants", variantDataTable.AsTableValuedParameter("dbo.VariantType"));

                await connection.ExecuteAsync(
                    "sp_CreateProduct",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return (true, "Thêm sản phẩm thành công");
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return (false, $"Lỗi hệ thống: {innerMessage}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_DeleteProduct",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                if (result == null) return (false, "Lỗi thực thi hệ thống.");

                return (result.Success == 1, result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<List<ProductVM>> GetAllAsync(int pageNumber, int pageSize)
        {
            int actualPage = pageNumber > 0 ? pageNumber : 1;
            int actualSize = pageSize > 0 ? pageSize : 50;

            using var connection = await _dbFactory.CreateConnectionAsync();

            var result = await connection.QueryAsync<ProductVM>(
                "GetProductList",
                new { PageNumber = actualPage, PageSize = actualSize },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<ProductVM?> GetByIdAsync(Guid id)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                using var multi = await connection.QueryMultipleAsync(
                    "sp_GetProductById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                var product = await multi.ReadFirstOrDefaultAsync<ProductVM>();

                if (product != null)
                {
                    var variantsData = await multi.ReadAsync<dynamic>();

                    product.Variants = variantsData.Select(v => new VariantDisplayVM
                    {
                        Id = v.Id,
                        SKU = v.SKU,
                        Price = v.Price,
                        Stock = v.Stock,
                        Combination = v.Combination,
                        SelectedValues = v.SelectedValuesJson != null
                            ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<AttributeValueVM>>((string)v.SelectedValuesJson)
                            : new List<AttributeValueVM>()
                    }).ToList();
                }

                return product;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool Success, string Message)> UpdateAsync(Guid id, ProductInputVM model)
        {
            try
            {
                using var connection = await _dbFactory.CreateConnectionAsync();

                string? currentImageUrl = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT ImageUrl FROM Products WHERE Id = @Id",
                    new { Id = id }
                );

                if (currentImageUrl == null) return (false, "Sản phẩm không tồn tại.");

                string? newImageUrl = null;
                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(currentImageUrl))
                        await _fileService.DeleteImageAsync(currentImageUrl);

                    newImageUrl = await _fileService.UploadImageAsync(model.ImageFile);
                }

                var variantTable = ToVariantDataTable(model.Variants);

                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@Name", model.Name);
                parameters.Add("@CategoryId", model.CategoryId);
                parameters.Add("@ImageUrl", newImageUrl);
                parameters.Add("@Variants", variantTable.AsTableValuedParameter("dbo.VariantType"));

                await connection.ExecuteAsync(
                    "sp_UpdateProduct",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return (true, "Cập nhật sản phẩm thành công!");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi cập nhật: {ex.Message}");
            }
        }

        private DataTable ToVariantDataTable(List<VariantInputVM> variants)
        {
            var table = new DataTable();
            table.Columns.Add("SKU", typeof(string));
            table.Columns.Add("Price", typeof(decimal));
            table.Columns.Add("Stock", typeof(int));
            table.Columns.Add("Combination", typeof(string));
            table.Columns.Add("AttributeValueIds", typeof(string));

            if (variants != null)
            {
                foreach (var v in variants)
                {
                    string attrIds = v.SelectedAttributeValueIds != null
                        ? string.Join(",", v.SelectedAttributeValueIds)
                        : string.Empty;

                    table.Rows.Add(
                        v.SKU,
                        v.Price,
                        v.Stock,
                        v.Combination ?? string.Empty,
                        attrIds
                    );
                }
            }
            return table;
        }
    }
}
