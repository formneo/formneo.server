using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using formneo.core.DTOs.EntityManager;

namespace formneo.service.IServices.EntityManager
{
    /// <summary>
    /// Form Entity Relation yönetimi için service interface
    /// </summary>
    public interface IFormEntityRelationService
    {
        /// <summary>
        /// Form'un tüm entity ilişkilerini getir
        /// </summary>
        Task<List<FormEntityRelationDto>> GetRelationsByFormIdAsync(Guid formId);

        /// <summary>
        /// Entity'nin kullanıldığı form ilişkilerini getir
        /// </summary>
        Task<List<FormEntityRelationDto>> GetRelationsByEntityIdAsync(Guid entityId);

        /// <summary>
        /// İlişkiyi ID ile getir
        /// </summary>
        Task<FormEntityRelationDto> GetRelationByIdAsync(Guid id);

        /// <summary>
        /// Form'un primary entity'sini getir
        /// </summary>
        Task<FormEntityRelationDto> GetPrimaryRelationAsync(Guid formId);

        /// <summary>
        /// Yeni ilişki oluştur
        /// </summary>
        Task<FormEntityRelationDto> CreateRelationAsync(FormEntityRelationCreateDto dto);

        /// <summary>
        /// İlişki güncelle
        /// </summary>
        Task<FormEntityRelationDto> UpdateRelationAsync(FormEntityRelationUpdateDto dto);

        /// <summary>
        /// İlişki sil
        /// </summary>
        Task<bool> DeleteRelationAsync(Guid id);

        /// <summary>
        /// Form'a multiple entity'ler ekle (örn: Customer + Address + Order)
        /// </summary>
        Task<List<FormEntityRelationDto>> CreateMultipleRelationsAsync(Guid formId, List<FormEntityRelationCreateDto> relations);
    }
}
