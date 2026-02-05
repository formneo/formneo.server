using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using NLayer.Core.Services;
using System.ComponentModel;
using System.Reflection;
using formneo.api.Filters;
using formneo.core.DTOs;
using formneo.core.DTOs.FormAuth;
using formneo.core.DTOs.FormDatas;
using formneo.core.EnumExtensions;
using formneo.core.Models;
using formneo.core.Models.FormEnums;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.DTOs.Menu;

namespace formneo.api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FormDataController : CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly IFormService _service;
        private readonly IServiceWithDto<FormRuleEngine, FormRuleEngineDto> _formRuleEngineService;
        private readonly IFormRepository _formRepository;
        private readonly IGlobalServiceWithDto<Menu, MenuListDto> _menuService;


        public FormDataController(IMapper mapper, IFormService formService, IServiceWithDto<FormRuleEngine, FormRuleEngineDto> formRuleEngineService, IFormRepository formRepository, IGlobalServiceWithDto<Menu, MenuListDto> menuService)
        {
            _mapper = mapper;
            _service = formService;
            _formRuleEngineService = formRuleEngineService;
            _formRepository = formRepository;
            _menuService = menuService;
        }
        /// GET api/products
        [HttpGet]
        public async Task<ActionResult<List<FormDataListDto>>> All()
        {
            var query = _formRepository.GetAll().Include(x => x.WorkFlowDefination).OrderByDescending(e => e.CreatedDate);
            var dto = await query.Select(x => new FormDataListDto
            {
                CreatedDate = x.CreatedDate,
                FormCategory = x.FormCategory,
                FormName = x.FormName,
                FormCategoryText = x.FormCategory.GetDescription(),
                FormDescription = x.FormDescription,
                FormPriority = x.FormPriority,
                FormType = x.FormType,
                FormDesign = x.FormDesign,
                FormPriorityText = x.FormPriority.GetDescription(),
                FormTypeText = x.FormType.GetDescription(),
                Id = x.Id,
                IsActive = x.IsActive,
                JavaScriptCode = x.JavaScriptCode,
                Revision = x.Revision,
                WorkFlowDefinationId = x.WorkFlowDefinationId,
                WorkFlowName= x.WorkFlowDefination != null ? x.WorkFlowDefination.WorkflowName : null,
                ParentFormId=x.ParentFormId,
                CanEdit=x.CanEdit,
                ShowInMenu=x.ShowInMenu,
                PublicationStatus = x.PublicationStatus,
                PublicationStatusText = x.PublicationStatus.GetDescription(),
            }).ToListAsync();
            return dto;
        }
   
        [HttpGet("{id}")]
        public async Task<ActionResult<FormDataListDto>> GetById(string id)
        {
                if (!Guid.TryParse(id, out var guid))
            {
                return BadRequest("Geçersiz id formatı");
            }
            var query = _formRepository.Where(x=>x.Id==guid).Include(x => x.WorkFlowDefination);
            var dto = await query.Select(x => new FormDataListDto
            {
                CreatedDate = x.CreatedDate,
                
                FormCategory = x.FormCategory,
                FormName = x.FormName,
                FormCategoryText = x.FormCategory.GetDescription(),
                FormDescription = x.FormDescription,
                FormPriority = x.FormPriority,
                FormType = x.FormType,
                FormDesign = x.FormDesign,
                FormPriorityText = x.FormPriority.GetDescription(),
                FormTypeText = x.FormType.GetDescription(),
                Id = x.Id,
                IsActive = x.IsActive,
                JavaScriptCode = x.JavaScriptCode,
                Revision = x.Revision,
                WorkFlowDefinationId = x.WorkFlowDefinationId,
                WorkFlowName = x.WorkFlowDefination != null ? x.WorkFlowDefination.WorkflowName : null,
                ParentFormId=x.ParentFormId,
                CanEdit = x.CanEdit,
                ShowInMenu = x.ShowInMenu,
                PublicationStatus = x.PublicationStatus,
                PublicationStatusText = x.PublicationStatus.GetDescription(),
            }).FirstAsync();

            return dto;
        }

        // GET /api/products/5

        //de
        [HttpPost]
        public async Task<IActionResult> Save(FormDataInsertDto formDto)
        {
            var result = await _service.AddAsync(_mapper.Map<Form>(formDto));
            if (result.PublicationStatus == 0)
            {
                result.PublicationStatus = FormPublicationStatus.Draft;
                await _service.UpdateAsync(result);
            }
            if (result.ParentFormId == null) {
                result.ParentFormId = result.Id;
                await _service.UpdateAsync(result);
            }
            // Yeni form Draft olacağından menü oluşturma yapılmaz; Publish/update sırasında senkronize edilir

            return CreateActionResult(CustomResponseDto<FormDataInsertDto>.Success(204));

        }
        [HttpPut]
        public async Task<IActionResult> Update(FormDataUpdateDto formDto)
        {
            var existing = await _formRepository.GetByIdStringGuidAsync(formDto.Id);
            if (existing == null)
                return NotFound();
            if (existing.PublicationStatus != FormPublicationStatus.Draft)
                return BadRequest("Only Draft forms can be updated.");

            // Map updatable fields only
            existing.FormName = formDto.FormName;
            existing.FormDescription = formDto.FormDescription;
            existing.FormDesign = formDto.FormDesign;
            existing.IsActive = formDto.IsActive;
            existing.JavaScriptCode = formDto.JavaScriptCode;
            existing.FormType = formDto.FormType;
            existing.FormCategory = formDto.FormCategory;
            existing.FormPriority = formDto.FormPriority;
            existing.WorkFlowDefinationId = formDto.WorkFlowDefinationId;
            existing.CanEdit = formDto.CanEdit;
            existing.ShowInMenu = formDto.ShowInMenu;
            existing.PublicationStatus = formDto.PublicationStatus; // optionally keep Draft

            await _service.UpdateAsync(existing);

            // Yayın/menü görünürlük koşullarına göre menü senkronizasyonu
            await SyncFormMenuAsync(existing);

            return CreateActionResult(CustomResponseDto<FormDataUpdateDto>.Success(204));
        }

        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> CreateRevision(Guid id)
        {
            var newRev = await _service.CreateRevisionAsync(id);
            return Ok(new { id = newRev.Id, revision = newRev.Revision });
        }

        [HttpPost("[action]/{id}")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var published = await _service.PublishAsync(id);
            // Yayınlanınca menü senkronizasyonu
            await SyncFormMenuAsync(published);
            return Ok(new { id = published.Id, revision = published.Revision, status = published.PublicationStatus.ToString() });
        }

        [HttpGet("[action]/{parentId}")]
        public async Task<ActionResult<List<FormDataListDto>>> Versions(Guid parentId)
        {
            var list = await _service.GetVersionsAsync(parentId);
            var dto = list.Select(x => new FormDataListDto
            {
                CreatedDate = x.CreatedDate,
                FormCategory = x.FormCategory,
                FormName = x.FormName,
                FormCategoryText = x.FormCategory.GetDescription(),
                FormDescription = x.FormDescription,
                FormPriority = x.FormPriority,
                FormType = x.FormType,
                FormDesign = x.FormDesign,
                FormPriorityText = x.FormPriority.GetDescription(),
                FormTypeText = x.FormType.GetDescription(),
                Id = x.Id,
                IsActive = x.IsActive,
                JavaScriptCode = x.JavaScriptCode,
                Revision = x.Revision,
                WorkFlowDefinationId = x.WorkFlowDefinationId,
                WorkFlowName = x.WorkFlowDefination != null ? x.WorkFlowDefination.WorkflowName : null,
                ParentFormId=x.ParentFormId,
                CanEdit = x.CanEdit,
                ShowInMenu = x.ShowInMenu,
                PublicationStatus = x.PublicationStatus,
                PublicationStatusText = x.PublicationStatus.GetDescription(),
            }).ToList();
            return dto;
        }

        // Aile bazında en son revizyonu döner (Published varsa onu, yoksa en yüksek revizyon)
        [HttpGet("latest-per-family")]
        public async Task<ActionResult<List<FormDataListDto>>> GetLatestPerFamily()
        {
            var all = await _service.GetAllAsync();
            var list = all.ToList();
            if (list == null || list.Count == 0) return new List<FormDataListDto>();

            // ParentFormId null olanlar kendisi aile kökü sayılır
            var families = list.GroupBy(f => f.ParentFormId ?? f.Id).ToList();
            var latest = new List<Form>();
            foreach (var fam in families)
            {
                var published = fam.Where(f => f.PublicationStatus == FormPublicationStatus.Published)
                                   .OrderByDescending(f => f.Revision)
                                   .FirstOrDefault();
                var pick = published ?? fam.OrderByDescending(f => f.Revision).First();
                latest.Add(pick);
            }

            var dto = latest.Select(x => new FormDataListDto
            {
                CreatedDate = x.CreatedDate,
                FormCategory = x.FormCategory,
                FormName = x.FormName,
                FormCategoryText = x.FormCategory.GetDescription(),
                FormDescription = x.FormDescription,
                FormPriority = x.FormPriority,
                FormType = x.FormType,
                FormDesign = x.FormDesign,
                FormPriorityText = x.FormPriority.GetDescription(),
                FormTypeText = x.FormType.GetDescription(),
                Id = x.Id,
                IsActive = x.IsActive,
                JavaScriptCode = x.JavaScriptCode,
                Revision = x.Revision,
                WorkFlowDefinationId = x.WorkFlowDefinationId,
                WorkFlowName = null,
                ParentFormId = x.ParentFormId,
                CanEdit = x.CanEdit,
                ShowInMenu = x.ShowInMenu,
                PublicationStatus = x.PublicationStatus,
                PublicationStatusText = x.PublicationStatus.GetDescription(),
            }).ToList();

            return dto;
        }

        // DELETE api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            var form = await _service.GetByIdStringGuidAsync(id);


            await _service.RemoveAsync(form);

            // Form silinince menü öğesini de kaldır
            await RemoveFormMenuAsync(id);

            return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
        }

        [HttpGet("[action]/{guid}")]
        public async Task<ActionResult<List<FieldInfoDto>>> GetFieldByForm(Guid guid)
        {
            

            var form = await _service.GetByIdStringGuidAsync(guid);

            JToken token = JObject.Parse(form.FormDesign);


            List<FieldInfoDto> fieldInfos = new List<FieldInfoDto>();

            // JSON verisini JToken nesnesine dönüştürelim


            // "components" dizisinin her bir öğesini işleyelim
            foreach (JToken component in token["components"])
            {
                // Özelliklere erişim örnekleri
                string label = component["label"]?.ToString();
                string type = component["type"]?.ToString();
                string id = component["id"]?.ToString();

                fieldInfos.Add(new FieldInfoDto
                {
                    Id = id,
                    Label = label,
                    Type = type
                });
            }

            return fieldInfos;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddFormRuleEngine(FormRuleEngineDto dto)
        {
            var result =  await _formRuleEngineService.AddAsync(_mapper.Map<FormRuleEngineDto>(dto));
            return CreateActionResult(CustomResponseDto<FormRuleEngineDto>.Success(204));
        }
        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateFormRuleEngine(FormRuleEngineDto dto)
        {
            var result = await _formRuleEngineService.UpdateAsync(_mapper.Map<FormRuleEngineDto>(dto));
            return CreateActionResult(CustomResponseDto<FormRuleEngineDto>.Success(204));
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<List<FormDataListDto>>> GetFormListByMenu()
        {
            var forms = await _service.GetAllAsync();
            var dto = forms
                .Where(e => e.IsActive == 1 && e.PublicationStatus == FormPublicationStatus.Published)
                .Select(x => new FormDataListDto
                {
                    CreatedDate = x.CreatedDate,
                    FormCategory = x.FormCategory,
                    FormName = x.FormName,
                    FormCategoryText = x.FormCategory.GetDescription(),
                    FormDescription = x.FormDescription,
                    FormPriority = x.FormPriority,
                    FormType = x.FormType,
                    FormDesign = x.FormDesign,
                    FormPriorityText = x.FormPriority.GetDescription(),
                    FormTypeText = x.FormType.GetDescription(),
                    Id = x.Id,
                    IsActive = x.IsActive,
                    JavaScriptCode = x.JavaScriptCode,
                    Revision = x.Revision,
                    WorkFlowDefinationId = x.WorkFlowDefinationId,
                    WorkFlowName = null,
                    ParentFormId = x.ParentFormId,
                    CanEdit = x.CanEdit,
                    ShowInMenu = x.ShowInMenu,
                    PublicationStatus = x.PublicationStatus,
                    PublicationStatusText = x.PublicationStatus.GetDescription(),
                })
                .ToList();

            return dto;
        }
        /// <summary>
        /// Tüm formların sadece ID ve adını getirir
        /// </summary>
        /// <returns>Form ID ve adı listesi</returns>
        [HttpGet("names")]
        public async Task<ActionResult<List<FormNameIdDto>>> GetFormNames()
        {
            var forms = await _formRepository.GetAll()
                .Select(x => new FormNameIdDto
                {
                    Id = x.Id,
                    FormName = x.FormName
                })
                .OrderBy(x => x.FormName)
                .ToListAsync();

            return forms;
        }

        [HttpGet("GetFormPrioritiesEnum")]
        public IActionResult GetFormPrioritiesEnum()
        {
            var values = Enum.GetValues(typeof(FormPriority))
                             .Cast<FormPriority>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString(),
                                 Description = GetEnumDescription(e)
                             });

            return Ok(values);
        }
        [HttpGet("GetFormCategoriesEnum")]
        public IActionResult GetFormCategoriesEnum()
        {
            var values = Enum.GetValues(typeof(FormCategory))
                             .Cast<FormCategory>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString(),
                                 Description = GetEnumDescription(e)
                             });

            return Ok(values);
        }
        [HttpGet("GetFormTypesEnum")]
        public IActionResult GetFormTypesEnum()
        {
            var values = Enum.GetValues(typeof(FormType))
                             .Cast<FormType>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString(),
                                 Description = GetEnumDescription(e)
                             });

            return Ok(values);
        }
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? value.ToString();
        }


        // Yardımcı metotlar
        private async Task<MenuListDto?> EnsureFormsRootAsync()
        {
            // FORMS_ROOT var mı?
            var existingRootResp = await _menuService.Where(e => e.MenuCode == "FORMS_ROOT" && e.ParentMenuId == null);
            var existingRoot = existingRootResp.Data.FirstOrDefault();
            if (existingRoot != null) return existingRoot;

            // Kök menüler arasından sipariş belirle
            var rootsResp = await _menuService.Where(e => e.ParentMenuId == null && e.IsActive == true);
            var nextOrder = (rootsResp?.Data?.Any() == true) ? rootsResp.Data.Max(m => m.Order) + 1 : 10000;

            var root = new MenuListDto
            {
                Id = new Guid("78681502-ac05-4a53-8b88-dc5b1231d3bb"),
                MenuCode = "FORMS_ROOT",
                ParentMenuId = null,
                Name = "Formlar",
                Href = "/userFormList",
                Icon = null,
                IsActive = true,
                ShowMenu = true,
                IsTenantOnly = false,
                IsGlobalOnly = false,
                Order = nextOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Description = "",
                SubMenus = null
            };
            var added = await _menuService.AddAsync(root);
            return added.Data;
        }

        private async Task SyncFormMenuAsync(Form form)
        {
            // Form menü koşulu: Published + IsActive == 1 + ShowInMenu == true
            bool shouldHaveMenu = form != null && form.PublicationStatus == FormPublicationStatus.Published && form.IsActive == 1 && form.ShowInMenu;
            string menuCode = $"FORM_{form.Id}";

            // Var olan menüyü bul
            var existingResp = await _menuService.Where(m => m.MenuCode == menuCode);
            var existing = existingResp.Data.FirstOrDefault();

            if (!shouldHaveMenu)
            {
                if (existing != null)
                {
                    // Soft delete uygula
                    await _menuService.SoftDeleteAsync(existing.Id);
                }
                return;
            }

            // Kökü hazırla
            var root = await EnsureFormsRootAsync();
            if (root == null) return;

            // Çocuklar arasında sırayı belirle
            var siblingsResp = await _menuService.Where(e => e.ParentMenuId == root.Id && e.IsActive == true);
            var nextOrder = (siblingsResp?.Data?.Any() == true) ? siblingsResp.Data.Max(m => m.Order) + 1 : root.Order + 1;

            if (existing == null)
            {
                var child = new MenuListDto
                {
                    Id = Guid.NewGuid(),
                    MenuCode = menuCode,
                    ParentMenuId = root.Id,
                    Name = form.FormName,
                    Href = $"/userFormList?formId={form.Id}",
                    Icon = null,
                    IsActive = true,
                    ShowMenu = true,
                    IsTenantOnly = false,
                    IsGlobalOnly = false,
                    Order = nextOrder,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Description = "",
                    SubMenus = null
                };
                await _menuService.AddAsync(child);
            }
            else
            {
                existing.ParentMenuId = root.Id;
                existing.Name = form.FormName;
                existing.Href = $"/userFormList?formId={form.Id}";
                existing.IsActive = true;
                existing.ShowMenu = true;
                // Order'ı muhafaza et; yoksa sona taşı
                if (existing.Order <= 0)
                {
                    existing.Order = nextOrder;
                }
                await _menuService.UpdateAsync(existing);
            }
        }

        private async Task RemoveFormMenuAsync(Guid formId)
        {
            var menuCode = $"FORM_{formId}";
            var existingResp = await _menuService.Where(m => m.MenuCode == menuCode);
            var existing = existingResp.Data.FirstOrDefault();
            if (existing != null)
            {
                await _menuService.SoftDeleteAsync(existing.Id);
            }
        }

    }
}
