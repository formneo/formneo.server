using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using formneo.core.DTOs.EntityManager;

namespace formneo.service.IServices.EntityManager
{
    /// <summary>
    /// Form Field Mapping yönetimi için service interface
    /// </summary>
    public interface IFormFieldMappingService
    {
        /// <summary>
        /// Form'un tüm mapping'lerini getir
        /// </summary>
        Task<List<FormFieldMappingDto>> GetMappingsByFormIdAsync(Guid formId);

        /// <summary>
        /// Mapping'i ID ile getir
        /// </summary>
        Task<FormFieldMappingDto> GetMappingByIdAsync(Guid id);

        /// <summary>
        /// Yeni mapping oluştur
        /// </summary>
        Task<FormFieldMappingDto> CreateMappingAsync(FormFieldMappingCreateDto dto);

        /// <summary>
        /// Mapping güncelle
        /// </summary>
        Task<FormFieldMappingDto> UpdateMappingAsync(FormFieldMappingUpdateDto dto);

        /// <summary>
        /// Mapping sil
        /// </summary>
        Task<bool> DeleteMappingAsync(Guid id);

        /// <summary>
        /// Form design'dan otomatik mapping oluştur
        /// FormDesign JSON'ındaki field'ları parse edip entity field'larıyla eşleştirir
        /// </summary>
        Task<List<FormFieldMappingDto>> AutoMapFormFieldsAsync(AutoMapFormFieldsDto dto);

        /// <summary>
        /// Form element'inin mapping'ini getir
        /// </summary>
        Task<FormFieldMappingDto> GetMappingByFormElementIdAsync(Guid formId, string formElementId);

        /// <summary>
        /// Form field name'e göre mapping getir
        /// </summary>
        Task<FormFieldMappingDto> GetMappingByFormFieldNameAsync(Guid formId, string formFieldName);
    }
}
