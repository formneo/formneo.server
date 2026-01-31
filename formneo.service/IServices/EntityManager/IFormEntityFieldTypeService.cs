using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using formneo.core.DTOs.EntityManager;
using formneo.core.Models.EntityManager;

namespace formneo.service.IServices.EntityManager
{
    /// <summary>
    /// Form Entity Field Type yönetimi için service interface
    /// </summary>
    public interface IFormEntityFieldTypeService
    {
        /// <summary>
        /// Tüm field type'ları listele
        /// </summary>
        Task<List<FormEntityFieldTypeDto>> GetAllFieldTypesAsync();

        /// <summary>
        /// Aktif field type'ları listele
        /// </summary>
        Task<List<FormEntityFieldTypeDto>> GetActiveFieldTypesAsync();

        /// <summary>
        /// Kategoriye göre field type'ları getir
        /// </summary>
        Task<List<FormEntityFieldTypeDto>> GetFieldTypesByCategoryAsync(FieldTypeCategory category);

        /// <summary>
        /// Field type'ı ID ile getir
        /// </summary>
        Task<FormEntityFieldTypeDto> GetFieldTypeByIdAsync(Guid id);

        /// <summary>
        /// Field type'ı isme göre getir
        /// </summary>
        Task<FormEntityFieldTypeDto> GetFieldTypeByNameAsync(string typeName);

        /// <summary>
        /// Yeni field type oluştur (custom types için)
        /// </summary>
        Task<FormEntityFieldTypeDto> CreateFieldTypeAsync(FormEntityFieldTypeCreateDto dto);

        /// <summary>
        /// Field type güncelle
        /// </summary>
        Task<FormEntityFieldTypeDto> UpdateFieldTypeAsync(FormEntityFieldTypeUpdateDto dto);

        /// <summary>
        /// Field type sil (sadece custom types silinebilir)
        /// </summary>
        Task<bool> DeleteFieldTypeAsync(Guid id);
    }
}
