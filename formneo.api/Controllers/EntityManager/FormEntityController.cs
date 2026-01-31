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
    /// Form Entity yönetimi - Entity Library
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FormEntityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FormEntityController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tenant'ın tüm entity'lerini listele (Entity Library)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FormEntityListDto>>> GetAllEntities()
        {
            var entities = await _context.FormEntities
                .Include(e => e.Fields)
                .Where(e => e.IsActive)
                .Select(e => new FormEntityListDto
                {
                    Id = e.Id,
                    EntityName = e.EntityName,
                    EntityDescription = e.EntityDescription,
                    TableName = e.TableName,
                    IsActive = e.IsActive,
                    FieldCount = e.Fields.Count,
                    FormCount = e.FormEntityRelations.Count,
                    CreatedAt = e.CreatedDate
                })
                .OrderBy(e => e.EntityName)
                .ToListAsync();

            return Ok(entities);
        }

        /// <summary>
        /// Entity detayını getir (field'larıyla birlikte)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FormEntityDto>> GetEntityById(Guid id)
        {
            var entity = await _context.FormEntities
                .Include(e => e.Fields)
                    .ThenInclude(f => f.FieldType)
                .Include(e => e.ParentEntity)
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (entity == null)
            {
                return NotFound(new { message = "Entity bulunamadı" });
            }

            var dto = new FormEntityDto
            {
                Id = entity.Id,
                EntityName = entity.EntityName,
                EntityDescription = entity.EntityDescription,
                TableName = entity.TableName,
                SchemaName = entity.SchemaName,
                NamespacePath = entity.NamespacePath,
                ClassName = entity.ClassName,
                IsActive = entity.IsActive,
                AllowCreate = entity.AllowCreate,
                AllowRead = entity.AllowRead,
                AllowUpdate = entity.AllowUpdate,
                AllowDelete = entity.AllowDelete,
                ApiEndpoint = entity.ApiEndpoint,
                DisplayField = entity.DisplayField,
                OrderByField = entity.OrderByField,
                ParentEntityId = entity.ParentEntityId,
                ParentEntityName = entity.ParentEntity?.EntityName,
                Fields = entity.Fields.Select(f => new FormEntityFieldDto
                {
                    Id = f.Id,
                    FormEntityId = f.FormEntityId,
                    FieldName = f.FieldName,
                    FieldDescription = f.FieldDescription,
                    FieldTypeId = f.FieldTypeId,
                    FieldTypeName = f.FieldType.TypeName,
                    IsRequired = f.IsRequired,
                    IsUnique = f.IsUnique,
                    IsActive = f.IsActive,
                    MaxLength = f.MaxLength,
                    DisplayLabel = f.DisplayLabel,
                    DisplayOrder = f.DisplayOrder
                }).OrderBy(f => f.DisplayOrder).ToList(),
                CreatedAt = entity.CreatedDate,
                UpdatedAt = entity.UpdatedDate
            };

            return Ok(dto);
        }

        /// <summary>
        /// Yeni entity oluştur
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FormEntityDto>> CreateEntity([FromBody] FormEntityCreateDto dto)
        {
            // İsim kontrolü
            var exists = await _context.FormEntities
                .AnyAsync(e => e.EntityName.ToLower() == dto.EntityName.ToLower());

            if (exists)
            {
                return BadRequest(new { message = "Bu isimde bir entity zaten mevcut" });
            }

            var entity = new FormEntity
            {
                Id = Guid.NewGuid(),
                EntityName = dto.EntityName,
                EntityDescription = dto.EntityDescription,
                TableName = dto.TableName,
                SchemaName = dto.SchemaName,
                NamespacePath = dto.NamespacePath,
                ClassName = dto.ClassName,
                IsActive = dto.IsActive,
                AllowCreate = dto.AllowCreate,
                AllowRead = dto.AllowRead,
                AllowUpdate = dto.AllowUpdate,
                AllowDelete = dto.AllowDelete,
                ApiEndpoint = dto.ApiEndpoint,
                DisplayField = dto.DisplayField,
                OrderByField = dto.OrderByField,
                ParentEntityId = dto.ParentEntityId,
                CreatedDate = DateTime.UtcNow
            };

            _context.FormEntities.Add(entity);
            await _context.SaveChangesAsync();

            var result = new FormEntityDto
            {
                Id = entity.Id,
                EntityName = entity.EntityName,
                EntityDescription = entity.EntityDescription,
                TableName = entity.TableName,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedDate
            };

            return CreatedAtAction(nameof(GetEntityById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Entity güncelle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<FormEntityDto>> UpdateEntity(Guid id, [FromBody] FormEntityUpdateDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest(new { message = "ID uyuşmazlığı" });
            }

            var entity = await _context.FormEntities.FindAsync(id);

            if (entity == null)
            {
                return NotFound(new { message = "Entity bulunamadı" });
            }

            // İsim kontrolü
            var exists = await _context.FormEntities
                .AnyAsync(e => e.EntityName.ToLower() == dto.EntityName.ToLower() && e.Id != id);

            if (exists)
            {
                return BadRequest(new { message = "Bu isimde başka bir entity zaten mevcut" });
            }

            entity.EntityName = dto.EntityName;
            entity.EntityDescription = dto.EntityDescription;
            entity.TableName = dto.TableName;
            entity.SchemaName = dto.SchemaName;
            entity.NamespacePath = dto.NamespacePath;
            entity.ClassName = dto.ClassName;
            entity.IsActive = dto.IsActive;
            entity.AllowCreate = dto.AllowCreate;
            entity.AllowRead = dto.AllowRead;
            entity.AllowUpdate = dto.AllowUpdate;
            entity.AllowDelete = dto.AllowDelete;
            entity.ApiEndpoint = dto.ApiEndpoint;
            entity.DisplayField = dto.DisplayField;
            entity.OrderByField = dto.OrderByField;
            entity.ParentEntityId = dto.ParentEntityId;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetEntityById(id);
        }

        /// <summary>
        /// Entity sil (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEntity(Guid id)
        {
            var entity = await _context.FormEntities.FindAsync(id);

            if (entity == null)
            {
                return NotFound(new { message = "Entity bulunamadı" });
            }

            // Kullanımda mı kontrol et
            var isUsed = await _context.FormEntityRelations
                .AnyAsync(r => r.FormEntityId == id);

            if (isUsed)
            {
                return BadRequest(new { message = "Bu entity kullanımda olduğu için silinemez" });
            }

            entity.IsDelete = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Entity silindi" });
        }

        /// <summary>
        /// Entity'yi kullanan formları getir
        /// </summary>
        [HttpGet("{id}/forms")]
        public async Task<ActionResult<List<object>>> GetFormsUsingEntity(Guid id)
        {
            var forms = await _context.FormEntityRelations
                .Where(r => r.FormEntityId == id)
                .Include(r => r.Form)
                .Select(r => new
                {
                    r.FormId,
                    r.Form.FormName,
                    r.RelationName,
                    r.IsPrimary,
                    r.RelationType
                })
                .Distinct()
                .ToListAsync();

            return Ok(forms);
        }

        /// <summary>
        /// Form'da kullanılabilecek entity'leri getir (Dropdown için)
        /// </summary>
        [HttpGet("for-form")]
        public async Task<ActionResult<List<FormEntityListDto>>> GetEntitiesForForm()
        {
            var entities = await _context.FormEntities
                .Where(e => e.IsActive)
                .Select(e => new FormEntityListDto
                {
                    Id = e.Id,
                    EntityName = e.EntityName,
                    EntityDescription = e.EntityDescription,
                    TableName = e.TableName,
                    FieldCount = e.Fields.Count(f => f.IsActive)
                })
                .OrderBy(e => e.EntityName)
                .ToListAsync();

            return Ok(entities);
        }

        /// <summary>
        /// 🔥 Veritabanındaki tabloları listele (Import için)
        /// </summary>
        [HttpGet("database-tables")]
        public async Task<ActionResult<List<object>>> GetDatabaseTables()
        {
            try
            {
                // PostgreSQL için tablo listesi
                var tables = await _context.Database
                    .SqlQueryRaw<string>(@"
                        SELECT table_name 
                        FROM information_schema.tables 
                        WHERE table_schema = 'public' 
                        AND table_type = 'BASE TABLE'
                        AND table_name NOT LIKE '__EF%'
                        ORDER BY table_name
                    ")
                    .ToListAsync();

                var result = new List<object>();

                foreach (var tableName in tables)
                {
                    // Zaten entity olarak tanımlı mı kontrol et
                    var exists = await _context.FormEntities
                        .AnyAsync(e => e.TableName.ToLower() == tableName.ToLower());

                    // Kolon sayısını al
                    var columnCount = await _context.Database
                        .SqlQueryRaw<int>($@"
                            SELECT COUNT(*) as Value
                            FROM information_schema.columns 
                            WHERE table_name = '{tableName}' 
                            AND table_schema = 'public'
                        ")
                        .FirstOrDefaultAsync();

                    result.Add(new
                    {
                        TableName = tableName,
                        ColumnCount = columnCount,
                        AlreadyImported = exists,
                        IsSystem = tableName.StartsWith("AspNet") || 
                                 tableName.StartsWith("__")
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Tablolar alınamadı", error = ex.Message });
            }
        }

        /// <summary>
        /// 🔥 Seçili tabloları entity olarak import et
        /// </summary>
        [HttpPost("import-from-database")]
        public async Task<ActionResult<object>> ImportFromDatabase([FromBody] List<string> tableNames)
        {
            var imported = new List<string>();
            var skipped = new List<string>();

            foreach (var tableName in tableNames)
            {
                try
                {
                    // Zaten var mı kontrol et
                    var exists = await _context.FormEntities
                        .AnyAsync(e => e.TableName.ToLower() == tableName.ToLower());

                    if (exists)
                    {
                        skipped.Add(tableName);
                        continue;
                    }

                    // Entity oluştur
                    var entity = new FormEntity
                    {
                        Id = Guid.NewGuid(),
                        EntityName = tableName,
                        EntityDescription = $"{tableName} tablosundan import edildi",
                        TableName = tableName,
                        SchemaName = "public",
                        IsActive = true,
                        AllowCreate = true,
                        AllowRead = true,
                        AllowUpdate = true,
                        AllowDelete = false,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.FormEntities.Add(entity);
                    await _context.SaveChangesAsync();

                    // Kolonları al ve field olarak ekle
                    await ImportTableColumns(entity.Id, tableName);

                    imported.Add(tableName);
                }
                catch (Exception ex)
                {
                    skipped.Add($"{tableName} (Hata: {ex.Message})");
                }
            }

            return Ok(new
            {
                message = "Import tamamlandı",
                imported = imported,
                skipped = skipped,
                importedCount = imported.Count,
                skippedCount = skipped.Count
            });
        }

        /// <summary>
        /// Tablo kolonlarını field olarak ekle (yardımcı method)
        /// </summary>
        private async Task ImportTableColumns(Guid entityId, string tableName)
        {
            // Kolon bilgilerini al
            var columns = await _context.Database
                .SqlQueryRaw<ColumnInfo>($@"
                    SELECT 
                        column_name as ColumnName,
                        data_type as DataType,
                        character_maximum_length as MaxLength,
                        is_nullable as IsNullable
                    FROM information_schema.columns 
                    WHERE table_name = '{tableName}' 
                    AND table_schema = 'public'
                    ORDER BY ordinal_position
                ")
                .ToListAsync();

            int displayOrder = 0;
            foreach (var column in columns)
            {
                // SQL tipini FieldType'a map et
                var fieldTypeId = MapSqlTypeToFieldTypeId(column.DataType);

                var field = new FormEntityField
                {
                    Id = Guid.NewGuid(),
                    FormEntityId = entityId,
                    FieldName = column.ColumnName,
                    ColumnName = column.ColumnName,
                    PropertyName = column.ColumnName,
                    FieldTypeId = fieldTypeId,
                    IsRequired = column.IsNullable?.ToLower() == "no",
                    IsNullable = column.IsNullable?.ToLower() == "yes",
                    MaxLength = column.MaxLength,
                    DisplayLabel = column.ColumnName,
                    DisplayOrder = displayOrder++,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.FormEntityFields.Add(field);
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// SQL tipini FieldType ID'sine map et
        /// </summary>
        private Guid MapSqlTypeToFieldTypeId(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "character varying" or "varchar" or "text" => Guid.Parse("00000000-0000-0000-0000-000000000001"), // String
                "integer" or "int" or "int4" => Guid.Parse("00000000-0000-0000-0000-000000000003"), // Integer
                "numeric" or "decimal" => Guid.Parse("00000000-0000-0000-0000-000000000004"), // Decimal
                "boolean" or "bit" => Guid.Parse("00000000-0000-0000-0000-000000000005"), // Boolean
                "date" => Guid.Parse("00000000-0000-0000-0000-000000000006"), // Date
                "timestamp" or "datetime" or "datetime2" => Guid.Parse("00000000-0000-0000-0000-000000000007"), // DateTime
                "uuid" or "uniqueidentifier" => Guid.Parse("00000000-0000-0000-0000-000000000012"), // Guid
                _ => Guid.Parse("00000000-0000-0000-0000-000000000001") // Default: String
            };
        }

        // Helper class for database column info
        private class ColumnInfo
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public int? MaxLength { get; set; }
            public string IsNullable { get; set; }
        }
    }
}
