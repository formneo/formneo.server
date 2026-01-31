using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using formneo.core.DTOs.EntityManager;

namespace formneo.service.IServices.EntityManager
{
    /// <summary>
    /// Form Entity Field yönetimi için service interface
    /// </summary>
    public interface IFormEntityFieldService
    {
        /// <summary>
        /// Belirli bir entity'nin tüm field'larını getir
        /// </summary>
        Task<List<FormEntityFieldDto>> GetFieldsByEntityIdAsync(Guid entityId);

        /// <summary>
        /// Field'ı ID ile getir
        /// </summary>
        Task<FormEntityFieldDto> GetFieldByIdAsync(Guid id);

        /// <summary>
        /// Yeni field oluştur
        /// </summary>
        Task<FormEntityFieldDto> CreateFieldAsync(FormEntityFieldCreateDto dto);

        /// <summary>
        /// Field güncelle
        /// </summary>
        Task<FormEntityFieldDto> UpdateFieldAsync(FormEntityFieldUpdateDto dto);

        /// <summary>
        /// Field sil (soft delete)
        /// </summary>
        Task<bool> DeleteFieldAsync(Guid id);

        /// <summary>
        /// Field'ı kullanan mapping'leri getir
        /// </summary>
        Task<List<FormFieldMappingDto>> GetFieldMappingsAsync(Guid fieldId);

        /// <summary>
        /// Lookup field için ilişkili entity'yi getir
        /// </summary>
        Task<FormEntityDto> GetRelatedEntityAsync(Guid fieldId);
    }
}
