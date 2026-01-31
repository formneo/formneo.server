using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using formneo.core.DTOs.EntityManager;
using formneo.core.Models.EntityManager;
using formneo.repository;

namespace formneo.api.Controllers.EntityManager
{
    /// <summary>
    /// Form Entity Field Type yönetimi
    /// Alan tiplerini (String, Number, Date, vb.) yönetir
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FormEntityFieldTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FormEntityFieldTypeController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm alan tiplerini getir (Combobox için)
        /// </summary>
        /// <returns>Alan tipleri listesi</returns>
        [HttpGet]
        public async Task<ActionResult<List<FormEntityFieldTypeDto>>> GetAllFieldTypes()
        {
            var fieldTypes = await _context.FormEntityFieldTypes
                .Where(ft => ft.IsActive)
                .OrderBy(ft => ft.Category)
                .ThenBy(ft => ft.TypeName)
                .Select(ft => new FormEntityFieldTypeDto
                {
                    Id = ft.Id,
                    TypeName = ft.TypeName,
                    TypeDescription = ft.TypeDescription,
                    CSharpType = ft.CSharpType,
                    TypeScriptType = ft.TypeScriptType,
                    SqlServerType = ft.SqlServerType,
                    DefaultComponentType = ft.DefaultComponentType,
                    Category = ft.Category,
                    CategoryName = ft.Category.ToString(),
                    IsActive = ft.IsActive,
                    IsSystemType = ft.IsSystemType,
                    ValidationOptions = ft.ValidationOptions,
                    ComponentOptions = ft.ComponentOptions,
                    CreatedAt = ft.CreatedDate
                })
                .ToListAsync();

            return Ok(fieldTypes);
        }

        /// <summary>
        /// Sadece aktif alan tiplerini getir
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<FormEntityFieldTypeListDto>>> GetActiveFieldTypes()
        {
            var fieldTypes = await _context.FormEntityFieldTypes
                .Where(ft => ft.IsActive)
                .OrderBy(ft => ft.Category)
                .ThenBy(ft => ft.TypeName)
                .Select(ft => new FormEntityFieldTypeListDto
                {
                    Id = ft.Id,
                    TypeName = ft.TypeName,
                    CSharpType = ft.CSharpType,
                    DefaultComponentType = ft.DefaultComponentType,
                    CategoryName = ft.Category.ToString(),
                    IsSystemType = ft.IsSystemType
                })
                .ToListAsync();

            return Ok(fieldTypes);
        }

        /// <summary>
        /// Kategoriye göre alan tiplerini getir (Primitive, Reference, Collection, Complex, File)
        /// </summary>
        /// <param name="category">Alan tipi kategorisi (0=Primitive, 1=Reference, 2=Collection, 3=Complex, 4=File)</param>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<FormEntityFieldTypeDto>>> GetFieldTypesByCategory(FieldTypeCategory category)
        {
            var fieldTypes = await _context.FormEntityFieldTypes
                .Where(ft => ft.IsActive && ft.Category == category)
                .OrderBy(ft => ft.TypeName)
                .Select(ft => new FormEntityFieldTypeDto
                {
                    Id = ft.Id,
                    TypeName = ft.TypeName,
                    TypeDescription = ft.TypeDescription,
                    CSharpType = ft.CSharpType,
                    TypeScriptType = ft.TypeScriptType,
                    SqlServerType = ft.SqlServerType,
                    DefaultComponentType = ft.DefaultComponentType,
                    Category = ft.Category,
                    CategoryName = ft.Category.ToString(),
                    IsActive = ft.IsActive,
                    IsSystemType = ft.IsSystemType,
                    ValidationOptions = ft.ValidationOptions,
                    ComponentOptions = ft.ComponentOptions,
                    CreatedAt = ft.CreatedDate
                })
                .ToListAsync();

            return Ok(fieldTypes);
        }

        /// <summary>
        /// Kategorilere göre gruplu alan tipleri (Combobox için gruplu liste)
        /// </summary>
        [HttpGet("grouped")]
        public async Task<ActionResult<Dictionary<string, List<FormEntityFieldTypeListDto>>>> GetGroupedFieldTypes()
        {
            var fieldTypes = await _context.FormEntityFieldTypes
                .Where(ft => ft.IsActive)
                .OrderBy(ft => ft.Category)
                .ThenBy(ft => ft.TypeName)
                .ToListAsync();

            var grouped = fieldTypes
                .GroupBy(ft => ft.Category.ToString())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ft => new FormEntityFieldTypeListDto
                    {
                        Id = ft.Id,
                        TypeName = ft.TypeName,
                        CSharpType = ft.CSharpType,
                        DefaultComponentType = ft.DefaultComponentType,
                        CategoryName = ft.Category.ToString(),
                        IsSystemType = ft.IsSystemType
                    }).ToList()
                );

            return Ok(grouped);
        }

        /// <summary>
        /// Belirli bir alan tipinin detayını getir
        /// </summary>
        /// <param name="id">Alan tipi ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<FormEntityFieldTypeDto>> GetFieldTypeById(Guid id)
        {
            var fieldType = await _context.FormEntityFieldTypes
                .Where(ft => ft.Id == id)
                .Select(ft => new FormEntityFieldTypeDto
                {
                    Id = ft.Id,
                    TypeName = ft.TypeName,
                    TypeDescription = ft.TypeDescription,
                    CSharpType = ft.CSharpType,
                    TypeScriptType = ft.TypeScriptType,
                    SqlServerType = ft.SqlServerType,
                    DefaultComponentType = ft.DefaultComponentType,
                    Category = ft.Category,
                    CategoryName = ft.Category.ToString(),
                    IsActive = ft.IsActive,
                    IsSystemType = ft.IsSystemType,
                    ValidationOptions = ft.ValidationOptions,
                    ComponentOptions = ft.ComponentOptions,
                    CreatedAt = ft.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (fieldType == null)
            {
                return NotFound(new { message = "Alan tipi bulunamadı" });
            }

            return Ok(fieldType);
        }

        /// <summary>
        /// İsme göre alan tipi ara
        /// </summary>
        /// <param name="typeName">Tip adı (örn: String, Number)</param>
        [HttpGet("by-name/{typeName}")]
        public async Task<ActionResult<FormEntityFieldTypeDto>> GetFieldTypeByName(string typeName)
        {
            var fieldType = await _context.FormEntityFieldTypes
                .Where(ft => ft.TypeName.ToLower() == typeName.ToLower())
                .Select(ft => new FormEntityFieldTypeDto
                {
                    Id = ft.Id,
                    TypeName = ft.TypeName,
                    TypeDescription = ft.TypeDescription,
                    CSharpType = ft.CSharpType,
                    TypeScriptType = ft.TypeScriptType,
                    SqlServerType = ft.SqlServerType,
                    DefaultComponentType = ft.DefaultComponentType,
                    Category = ft.Category,
                    CategoryName = ft.Category.ToString(),
                    IsActive = ft.IsActive,
                    IsSystemType = ft.IsSystemType,
                    ValidationOptions = ft.ValidationOptions,
                    ComponentOptions = ft.ComponentOptions,
                    CreatedAt = ft.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (fieldType == null)
            {
                return NotFound(new { message = $"'{typeName}' adında alan tipi bulunamadı" });
            }

            return Ok(fieldType);
        }

        /// <summary>
        /// Lookup tipi için - Sadece referans tiplerini getir
        /// Başka entity'lere referans için kullanılacak tipler
        /// </summary>
        [HttpGet("reference-types")]
        public async Task<ActionResult<List<FormEntityFieldTypeListDto>>> GetReferenceTypes()
        {
            var fieldTypes = await _context.FormEntityFieldTypes
                .Where(ft => ft.IsActive && ft.Category == FieldTypeCategory.Reference)
                .OrderBy(ft => ft.TypeName)
                .Select(ft => new FormEntityFieldTypeListDto
                {
                    Id = ft.Id,
                    TypeName = ft.TypeName,
                    CSharpType = ft.CSharpType,
                    DefaultComponentType = ft.DefaultComponentType,
                    CategoryName = ft.Category.ToString(),
                    IsSystemType = ft.IsSystemType
                })
                .ToListAsync();

            return Ok(fieldTypes);
        }

        /// <summary>
        /// Validation seçeneklerini getir (Frontend için)
        /// Seçilen tip için hangi validation'lar geçerli?
        /// </summary>
        /// <param name="id">Alan tipi ID</param>
        [HttpGet("{id}/validation-options")]
        public async Task<ActionResult<object>> GetValidationOptions(Guid id)
        {
            var fieldType = await _context.FormEntityFieldTypes
                .Where(ft => ft.Id == id)
                .FirstOrDefaultAsync();

            if (fieldType == null)
            {
                return NotFound(new { message = "Alan tipi bulunamadı" });
            }

            return Ok(new
            {
                fieldType.TypeName,
                fieldType.Category,
                ValidationOptions = fieldType.ValidationOptions,
                ComponentOptions = fieldType.ComponentOptions
            });
        }

        /// <summary>
        /// Yeni custom alan tipi oluştur (Sadece admin)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FormEntityFieldTypeDto>> CreateFieldType([FromBody] FormEntityFieldTypeCreateDto dto)
        {
            // İsim kontrolü
            var exists = await _context.FormEntityFieldTypes
                .AnyAsync(ft => ft.TypeName.ToLower() == dto.TypeName.ToLower());

            if (exists)
            {
                return BadRequest(new { message = "Bu isimde bir alan tipi zaten mevcut" });
            }

            var fieldType = new FormEntityFieldType
            {
                Id = Guid.NewGuid(),
                TypeName = dto.TypeName,
                TypeDescription = dto.TypeDescription,
                CSharpType = dto.CSharpType,
                TypeScriptType = dto.TypeScriptType,
                SqlServerType = dto.SqlServerType,
                DefaultComponentType = dto.DefaultComponentType,
                Category = dto.Category,
                IsActive = dto.IsActive,
                IsSystemType = dto.IsSystemType,
                ValidationOptions = dto.ValidationOptions,
                ComponentOptions = dto.ComponentOptions,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.FormEntityFieldTypes.Add(fieldType);
            await _context.SaveChangesAsync();

            var result = new FormEntityFieldTypeDto
            {
                Id = fieldType.Id,
                TypeName = fieldType.TypeName,
                TypeDescription = fieldType.TypeDescription,
                CSharpType = fieldType.CSharpType,
                TypeScriptType = fieldType.TypeScriptType,
                SqlServerType = fieldType.SqlServerType,
                DefaultComponentType = fieldType.DefaultComponentType,
                Category = fieldType.Category,
                CategoryName = fieldType.Category.ToString(),
                IsActive = fieldType.IsActive,
                IsSystemType = fieldType.IsSystemType,
                ValidationOptions = fieldType.ValidationOptions,
                ComponentOptions = fieldType.ComponentOptions,
                CreatedAt = fieldType.CreatedDate
            };

            return CreatedAtAction(nameof(GetFieldTypeById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Alan tipini güncelle (Sadece custom tipler güncellenebilir)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<FormEntityFieldTypeDto>> UpdateFieldType(Guid id, [FromBody] FormEntityFieldTypeUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID uyuşmazlığı" });
            }

            var fieldType = await _context.FormEntityFieldTypes.FindAsync(id);

            if (fieldType == null)
            {
                return NotFound(new { message = "Alan tipi bulunamadı" });
            }

            if (fieldType.IsSystemType)
            {
                return BadRequest(new { message = "Sistem tipleri güncellenemez" });
            }

            // İsim kontrolü (başka bir kayıtta aynı isim var mı?)
            var exists = await _context.FormEntityFieldTypes
                .AnyAsync(ft => ft.TypeName.ToLower() == dto.TypeName.ToLower() && ft.Id != id);

            if (exists)
            {
                return BadRequest(new { message = "Bu isimde başka bir alan tipi zaten mevcut" });
            }

            fieldType.TypeName = dto.TypeName;
            fieldType.TypeDescription = dto.TypeDescription;
            fieldType.CSharpType = dto.CSharpType;
            fieldType.TypeScriptType = dto.TypeScriptType;
            fieldType.SqlServerType = dto.SqlServerType;
            fieldType.DefaultComponentType = dto.DefaultComponentType;
            fieldType.Category = dto.Category;
            fieldType.IsActive = dto.IsActive;
            fieldType.ValidationOptions = dto.ValidationOptions;
            fieldType.ComponentOptions = dto.ComponentOptions;
            fieldType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var result = new FormEntityFieldTypeDto
            {
                Id = fieldType.Id,
                TypeName = fieldType.TypeName,
                TypeDescription = fieldType.TypeDescription,
                CSharpType = fieldType.CSharpType,
                TypeScriptType = fieldType.TypeScriptType,
                SqlServerType = fieldType.SqlServerType,
                DefaultComponentType = fieldType.DefaultComponentType,
                Category = fieldType.Category,
                CategoryName = fieldType.Category.ToString(),
                IsActive = fieldType.IsActive,
                IsSystemType = fieldType.IsSystemType,
                ValidationOptions = fieldType.ValidationOptions,
                ComponentOptions = fieldType.ComponentOptions,
                CreatedAt = fieldType.CreatedDate
            };

            return Ok(result);
        }

        /// <summary>
        /// Alan tipini sil (Sadece custom tipler silinebilir)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFieldType(Guid id)
        {
            var fieldType = await _context.FormEntityFieldTypes.FindAsync(id);

            if (fieldType == null)
            {
                return NotFound(new { message = "Alan tipi bulunamadı" });
            }

            if (fieldType.IsSystemType)
            {
                return BadRequest(new { message = "Sistem tipleri silinemez" });
            }

            // Bu tipi kullanan field var mı kontrol et
            var isUsed = await _context.FormEntityFields
                .AnyAsync(f => f.FieldTypeId == id);

            if (isUsed)
            {
                return BadRequest(new { message = "Bu alan tipi kullanımda olduğu için silinemez" });
            }

            _context.FormEntityFieldTypes.Remove(fieldType);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Alan tipi silindi" });
        }

        /// <summary>
        /// Alan tipi kullanım istatistikleri
        /// </summary>
        [HttpGet("{id}/usage-stats")]
        public async Task<ActionResult<object>> GetUsageStats(Guid id)
        {
            var fieldType = await _context.FormEntityFieldTypes.FindAsync(id);

            if (fieldType == null)
            {
                return NotFound(new { message = "Alan tipi bulunamadı" });
            }

            var usageCount = await _context.FormEntityFields
                .CountAsync(f => f.FieldTypeId == id);

            var entities = await _context.FormEntityFields
                .Where(f => f.FieldTypeId == id)
                .Select(f => f.FormEntity.EntityName)
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                TypeName = fieldType.TypeName,
                UsageCount = usageCount,
                UsedInEntities = entities
            });
        }
    }
}
