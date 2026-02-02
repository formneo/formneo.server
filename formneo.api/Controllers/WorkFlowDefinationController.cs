using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using NLayer.Core.Services;
using NLayer.Service.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Models.FormEnums;
using formneo.core.Operations;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.service.Services;
using formneo.workflow;
using WorkflowCore.Models;
using WorkflowCore.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace formneo.api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WorkFlowDefinationController : CustomBaseController
    {

        private readonly IMapper _mapper;
        private readonly IWorkFlowDefinationService _service;
        private readonly IServiceWithDto<WorkFlowDefination, WorkFlowDefinationDto> _workFlowDefinationDto;
        private readonly IFormRepository _formRepository;

        public WorkFlowDefinationController(IMapper mapper, IWorkFlowDefinationService workFlowService, IFormRepository formRepository)
        {
            _mapper = mapper;
            _service = workFlowService;
            _formRepository = formRepository;
        }
        [HttpGet]
        public async Task<List<WorkFlowDefinationListDto>> All()
        {
            var definations = await _service.GetAllAsync();
            var definationsList = definations.ToList();
            var dto = _mapper.Map<List<WorkFlowDefinationListDto>>(definationsList);
            
            // FormId'leri topla
            var formIds = definationsList
                .Where(d => d.FormId.HasValue)
                .Select(d => d.FormId.Value)
                .Distinct()
                .ToList();
            
            if (formIds.Any())
            {
                // Bu form ID'lerinin ait olduğu aileleri (ParentFormId) belirle
                var relatedForms = await _formRepository
                    .Where(f => formIds.Contains(f.Id))
                    .ToListAsync();
                
                var familyRootIds = relatedForms
                    .Select(f => f.ParentFormId ?? f.Id)
                    .Distinct()
                    .ToList();
                
                // O ailelere ait tüm formları yükle
                var familyForms = await _formRepository
                    .Where(f => familyRootIds.Contains(f.Id) || (f.ParentFormId.HasValue && familyRootIds.Contains(f.ParentFormId.Value)))
                    .ToListAsync();
                
                // Her aile için en son revizyonu belirle (Published varsa onu, yoksa en yüksek revizyon)
                var latestByRoot = familyForms
                    .GroupBy(f => f.ParentFormId ?? f.Id)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(f => f.PublicationStatus == FormPublicationStatus.Published)
                              .OrderByDescending(f => f.Revision)
                              .FirstOrDefault() ?? g.OrderByDescending(f => f.Revision).First()
                    );
                
                // DTO'lara en son revizyon bilgilerini map et
                for (int i = 0; i < definationsList.Count && i < dto.Count; i++)
                {
                    var def = definationsList[i];
                    if (def.FormId.HasValue)
                    {
                        var assignedForm = relatedForms.FirstOrDefault(f => f.Id == def.FormId.Value);
                        if (assignedForm != null)
                        {
                            var rootId = assignedForm.ParentFormId ?? assignedForm.Id;
                            if (latestByRoot.TryGetValue(rootId, out var latestForm))
                            {
                                dto[i].FormId = latestForm.Id;
                                dto[i].FormName = latestForm.FormName;
                                dto[i].FormRevision = latestForm.Revision;
                            }
                        }
                    }
                }
            }
            
            return dto;
        }

        [HttpGet("{id}")]
        public async Task<WorkFlowDefinationWithInitScriptDto> GetById(string id)
        {
            var workFlowDefination = await _service.GetByIdStringGuidAsync(new Guid(id));
            
            // En son form revizyonunu bul
            Guid? latestFormId = workFlowDefination?.FormId;
            string? latestFormName = workFlowDefination?.Form?.FormName;
            int? latestFormRevision = workFlowDefination?.Form?.Revision;
            
            if (workFlowDefination?.FormId.HasValue == true && workFlowDefination.Form != null)
            {
                var rootId = workFlowDefination.Form.ParentFormId ?? workFlowDefination.Form.Id;
                
                // O ailedeki tüm formları getir
                var familyForms = await _formRepository
                    .Where(f => f.Id == rootId || f.ParentFormId == rootId)
                    .ToListAsync();
                
                // En son revizyonu bul (Published varsa onu, yoksa en yüksek revizyon)
                var latestForm = familyForms
                    .Where(f => f.PublicationStatus == FormPublicationStatus.Published)
                    .OrderByDescending(f => f.Revision)
                    .FirstOrDefault() ?? familyForms.OrderByDescending(f => f.Revision).First();
                
                if (latestForm != null)
                {
                    latestFormId = latestForm.Id;
                    latestFormName = latestForm.FormName;
                    latestFormRevision = latestForm.Revision;
                }
            }
            
            var workFlowDefinationDto = new WorkFlowDefinationWithInitScriptDto
            {
                Id = workFlowDefination.Id,
                WorkflowName = workFlowDefination.WorkflowName,
                IsActive = workFlowDefination.IsActive,
                Revision = workFlowDefination.Revision,
                FormId = latestFormId,
                FormName = latestFormName,
                FormRevision = latestFormRevision,
                InitScript = string.Empty
            };
            
            // Defination JSON'ını parse edip formNode'ların initScript'lerini çıkar ve birleştir
            if (!string.IsNullOrEmpty(workFlowDefination.Defination))
            {
                try
                {
                    var definitionJson = JObject.Parse(workFlowDefination.Defination);
                    var nodes = definitionJson["nodes"] as JArray;
                    var initScripts = new List<string>();
                    
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            var nodeType = node["type"]?.ToString();
                            if (nodeType == "formNode")
                            {
                                var initScript = node["data"]?["initScript"]?.ToString();
                                
                                if (!string.IsNullOrEmpty(initScript))
                                {
                                    initScripts.Add(initScript);
                                }
                            }
                        }
                    }
                    
                    // Tüm initScript'leri birleştir
                    workFlowDefinationDto.InitScript = string.Join("\n\n", initScripts);
                }
                catch (Exception)
                {
                    // JSON parse hatası durumunda boş string döner
                }
            }
            
            return workFlowDefinationDto;
        }

        /// <summary>
        /// Workflow sayfası için definition'ı (JSON) döndürür
        /// </summary>
        [HttpGet("{id}/for-workflow")]
        public async Task<ActionResult<WorkFlowDefinationDetailDto>> GetForWorkflow(string id)
        {
            if (!Guid.TryParse(id, out var guid))
            {
                return BadRequest("Geçersiz id formatı");
            }

            var workFlowDefination = await _service.GetByIdStringGuidAsync(guid);
            if (workFlowDefination == null)
            {
                return NotFound("WorkFlowDefination bulunamadı");
            }

            var workFlowDefinationDto = _mapper.Map<WorkFlowDefinationDetailDto>(workFlowDefination);
            
            // En son form revizyonunu bul
            if (workFlowDefination?.FormId.HasValue == true && workFlowDefination.Form != null)
            {
                var rootId = workFlowDefination.Form.ParentFormId ?? workFlowDefination.Form.Id;
                
                // O ailedeki tüm formları getir
                var familyForms = await _formRepository
                    .Where(f => f.Id == rootId || f.ParentFormId == rootId)
                    .ToListAsync();
                
                // En son revizyonu bul (Published varsa onu, yoksa en yüksek revizyon)
                var latestForm = familyForms
                    .Where(f => f.PublicationStatus == FormPublicationStatus.Published)
                    .OrderByDescending(f => f.Revision)
                    .FirstOrDefault() ?? familyForms.OrderByDescending(f => f.Revision).First();
                
                if (latestForm != null)
                {
                    workFlowDefinationDto.FormId = latestForm.Id;
                    workFlowDefinationDto.FormName = latestForm.FormName;
                    workFlowDefinationDto.FormRevision = latestForm.Revision;
                }
            }
            
            return Ok(workFlowDefinationDto);
        }

        [HttpPost]
        public async Task<ActionResult<WorkFlowDefination>> Save(WorkFlowDefinationInsertDto workFlowDefinationDto)
        {
            //var employee = await _service.AddAsync(_mapper.Map<Employee>(employeeDto));
            WorkflowValidator validator = new workflow.WorkflowValidator();
            string error = "";

            if (validator.ValidateWorkflow(workFlowDefinationDto.Defination, out error))
            {
                var result = await _service.AddAsync(_mapper.Map<WorkFlowDefination>(workFlowDefinationDto));
                return result;
            }
            else
            {
                return NotFound(error);
            }
        }
        [HttpPut]
        public async Task<ActionResult<WorkFlowDefinationUpdateDto>> Update(WorkFlowDefinationUpdateDto workFlowDefinationDto)
        {

            string error = "";

            WorkflowValidator validator = new workflow.WorkflowValidator();

            if (validator.ValidateWorkflow(workFlowDefinationDto.Defination, out error))
            {
                await _service.UpdateAsync(_mapper.Map<WorkFlowDefination>(workFlowDefinationDto));
                return workFlowDefinationDto;
            }
            else
            {
                return NotFound(error);
            }
        }
       



        [HttpGet("[action]")]
        public async Task<IActionResult> GetWorkFlowListByMenu()
        {
            var definations = await _service.GetAllAsync();
            var dto = _mapper.Map<List<WorkFlowDefinationDto>>(definations);
            return CreateActionResult(CustomResponseDto<List<WorkFlowDefinationDto>>.Success(200, dto));
        }

        /// <summary>
        /// Workflow listesini menü yapısında döner - Performans optimize edilmiş
        /// Tek sorguda sadece gerekli alanları çeker ve Form revizyonlarını resolve eder
        /// </summary>
        [HttpGet("menu-structure")]
        public async Task<ActionResult<WorkFlowMenuResponseDto>> GetMenuStructure()
        {
            // 1. Tek sorguda sadece gerekli alanları çek (performans için projection kullan)
                var workflowDefinitions = await _service.GetAllAsync();
            
            var activeWorkflows = workflowDefinitions
                .Where(w => w.IsActive)
                .ToList();

            if (!activeWorkflows.Any())
            {
                return new WorkFlowMenuResponseDto
                {
                    Menus = new List<WorkFlowMenuGroupDto>
                    {
                        new WorkFlowMenuGroupDto
                        {
                            Id = "processes",
                            Label = "Süreçler",
                            Icon = "folder",
                            Children = new List<WorkFlowMenuItemDto>()
                        }
                    }
                };
            }

            // 2. FormId'leri topla ve tek sorguda tüm form ailelerini çek (N+1 query'den kaçın)
            var formIds = activeWorkflows
                .Where(w => w.FormId.HasValue)
                .Select(w => w.FormId.Value)
                .Distinct()
                .ToList();

            Dictionary<Guid, (Guid Id, string Name, int Revision)> latestFormsByOriginalId = new Dictionary<Guid, (Guid, string, int)>();

            if (formIds.Any())
            {
                // FormId'lerin ait olduğu aileleri bul
                var relatedForms = await _formRepository
                    .Where(f => formIds.Contains(f.Id))
                    .Select(f => new { f.Id, f.ParentFormId })
                    .ToListAsync();

                var familyRootIds = relatedForms
                    .Select(f => f.ParentFormId ?? f.Id)
                    .Distinct()
                    .ToList();

                // Tek sorguda tüm aile formlarını çek - sadece gerekli alanlar
                var familyForms = await _formRepository
                    .Where(f => familyRootIds.Contains(f.Id) || 
                                (f.ParentFormId.HasValue && familyRootIds.Contains(f.ParentFormId.Value)))
                    .Select(f => new { 
                        f.Id, 
                        f.ParentFormId, 
                        f.FormName, 
                        f.Revision, 
                        f.PublicationStatus 
                    })
                    .ToListAsync();

                // Her aile için en son revizyonu belirle (memory'de group - daha performanslı)
                var latestByRoot = familyForms
                    .GroupBy(f => f.ParentFormId ?? f.Id)
                    .ToDictionary(
                        g => g.Key,
                        g => {
                            var published = g.Where(f => f.PublicationStatus == FormPublicationStatus.Published)
                                           .OrderByDescending(f => f.Revision)
                                           .FirstOrDefault();
                            var selected = published ?? g.OrderByDescending(f => f.Revision).First();
                            return (selected.Id, selected.FormName, selected.Revision);
                        }
                    );

                // Original FormId'leri en son revizyonlara map et
                foreach (var relatedForm in relatedForms)
                {
                    var rootId = relatedForm.ParentFormId ?? relatedForm.Id;
                    if (latestByRoot.TryGetValue(rootId, out var latestForm))
                    {
                        latestFormsByOriginalId[relatedForm.Id] = latestForm;
                    }
                }
            }

            // 3. Menü yapısını oluştur (memory'de işlem - hızlı)
            var workflowItems = activeWorkflows.Select(w =>
            {
                // Form bilgilerini resolve et
                Guid? resolvedFormId = w.FormId;
                string? resolvedFormName = w.Form?.FormName;
                int? resolvedFormRevision = w.Form?.Revision;

                if (w.FormId.HasValue && latestFormsByOriginalId.TryGetValue(w.FormId.Value, out var latestForm))
                {
                    resolvedFormId = latestForm.Id;
                    resolvedFormName = latestForm.Name;
                    resolvedFormRevision = latestForm.Revision;
                }

                // Workflow string ID'si oluştur (küçük harf, boşlukları alt çizgi ile değiştir)
                var workflowIdString = $"wf_{w.Id.ToString().Substring(0, 8)}";
                var itemId = w.WorkflowName?.ToLowerInvariant().Replace(" ", "_").Replace("ı", "i").Replace("ş", "s").Replace("ç", "c").Replace("ğ", "g").Replace("ü", "u").Replace("ö", "o") 
                             ?? workflowIdString;

                return new WorkFlowMenuItemDto
                {
                    Id = itemId,
                    Label = w.WorkflowName ?? "İsimsiz Workflow",
                    Icon = "workflow",
                    WorkflowId = workflowIdString,
                    WorkflowGuid = w.Id,
                    Revision = w.Revision,
                    FormId = resolvedFormId,
                    FormName = resolvedFormName,
                    FormRevision = resolvedFormRevision,
                    Views = new List<WorkFlowMenuViewDto>
                    {
                        new WorkFlowMenuViewDto
                        {
                            Id = "my_requests",
                            Label = "Benim Taleplerim",
                            Path = $"/workflows/{workflowIdString}/my"
                        },
                        new WorkFlowMenuViewDto
                        {
                            Id = "approvals",
                            Label = "Onay Bekleyenler",
                            Path = $"/workflows/{workflowIdString}/approvals"
                        },
                        new WorkFlowMenuViewDto
                        {
                            Id = "all",
                            Label = "Tüm Kayıtlar",
                            Path = $"/workflows/{workflowIdString}/all"
                        }
                    }
                };
            }).ToList();

            // 4. Response oluştur
            var response = new WorkFlowMenuResponseDto
            {
                Menus = new List<WorkFlowMenuGroupDto>
                {
                    new WorkFlowMenuGroupDto
                    {
                        Id = "processes",
                        Label = "Süreçler",
                        Icon = "folder",
                        Children = workflowItems
                    }
                }
            };

            return Ok(response);
        }

    }
    public class Node
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class Edge
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string SourceHandle { get; set; }
    }

}
