using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using formneo.core.DTOs.EntityManager;

namespace formneo.service.IServices.EntityManager
{
    /// <summary>
    /// Form Entity yönetimi için service interface
    /// </summary>
    public interface IFormEntityService
    {
        /// <summary>
        /// Tüm entity'leri listele
        /// </summary>
        Task<List<FormEntityListDto>> GetAllEntitiesAsync();

        /// <summary>
        /// Entity'yi ID ile getir (field'ları dahil)
        /// </summary>
        Task<FormEntityDto> GetEntityByIdAsync(Guid id);

        /// <summary>
        /// Yeni entity oluştur
        /// </summary>
        Task<FormEntityDto> CreateEntityAsync(FormEntityCreateDto dto);

        /// <summary>
        /// Entity güncelle
        /// </summary>
        Task<FormEntityDto> UpdateEntityAsync(FormEntityUpdateDto dto);

        /// <summary>
        /// Entity sil (soft delete)
        /// </summary>
        Task<bool> DeleteEntityAsync(Guid id);

        /// <summary>
        /// Entity'nin field'larını getir
        /// </summary>
        Task<List<FormEntityFieldDto>> GetEntityFieldsAsync(Guid entityId);

        /// <summary>
        /// Entity'yi kullanan formları getir
        /// </summary>
        Task<List<Guid>> GetFormsUsingEntityAsync(Guid entityId);
    }
}
