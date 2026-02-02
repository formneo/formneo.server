using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using NLayer.Core.Services;
using NLayer.Service.Services;
using System.Linq;
using formneo.core;
using formneo.core.DTOs;
using formneo.core.DTOs.Budget.JobCodeRequest;
using formneo.core.Models;
using formneo.core.Operations;
using formneo.core.Services;
using formneo.core.DTOs.EmployeeAssignments;
using formneo.service.Services;
using formneo.workflow;
using formneo.workflow.Services;
using WorkflowCore.Models;
using WorkflowCore.Services;
namespace formneo.api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
     [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WorkFlowController : CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly IWorkFlowService _service;
        private readonly IWorkFlowItemService _workFlowItemservice;
        private readonly IApproveItemsService _approveItemsService;
        private readonly IFormItemsService _formItemsService;
        private readonly IFormInstanceService _formInstanceService;
        private readonly IServiceWithDto<WorkFlowDefination, WorkFlowDefinationDto> _workFlowDefinationDto;
        private readonly IServiceWithDto<WorkflowHead, WorkFlowHeadDto> _workFlowHeadService;
        private readonly IServiceWithDto<WorkflowItem, WorkFlowItemDto> _workFlowItemService;
        private readonly ITicketServices _ticketService;
        private readonly IFormService _formService;
        private readonly WorkflowResponseBuilder _responseBuilder;
        private readonly UserManager<UserApp> _userManager;
        private readonly IServiceWithDto<EmployeeAssignment, EmployeeAssignmentListDto> _employeeAssignmentService;
        
        public WorkFlowController(
            IMapper mapper, 
            IWorkFlowService workFlowService, 
            IWorkFlowItemService workFlowItemservice, 
            IApproveItemsService approveItemsService, 
            IFormItemsService formItemsService,
            IFormInstanceService formInstanceService,
            IServiceWithDto<WorkFlowDefination, WorkFlowDefinationDto> definationdto, 
            IServiceWithDto<WorkflowHead, WorkFlowHeadDto> workFlowHeadService, 
            IServiceWithDto<WorkflowItem, WorkFlowItemDto> workFlowItemService, 
            ITicketServices ticketServices,
            IFormService formService,
            UserManager<UserApp> userManager,
            IServiceWithDto<EmployeeAssignment, EmployeeAssignmentListDto> employeeAssignmentService)
        {
            _mapper = mapper;
            _service = workFlowService;
            _workFlowDefinationDto = definationdto;
            _workFlowItemservice = workFlowItemservice;
            _approveItemsService = approveItemsService;
            _formItemsService = formItemsService;
            _formInstanceService = formInstanceService;
            _workFlowHeadService = workFlowHeadService;
            _workFlowItemService = workFlowItemService;
            _ticketService = ticketServices;
            _formService = formService;
            _userManager = userManager;
            _employeeAssignmentService = employeeAssignmentService;
            _responseBuilder = new WorkflowResponseBuilder(mapper);
        }
        [HttpPost]
        public async Task<ActionResult<WorkFlowHeadDtoResultStartOrContinue>> Contiune(WorkFlowContiuneApiDto workFlowApiDto)
        {
            // Validation: workFlowItemId is required
            if (string.IsNullOrEmpty(workFlowApiDto.workFlowItemId))
            {
                return BadRequest(new { error = "workFlowItemId is required" });
            }

            WorkFlowExecute execute = new WorkFlowExecute();
            WorkFlowDto workFlowDto = new WorkFlowDto();
            WorkFlowParameters parameters = new WorkFlowParameters();
            parameters.workFlowService = _service;
            parameters.workFlowItemService = _workFlowItemservice;
            parameters._workFlowDefination = _workFlowDefinationDto;
            parameters._approverItemsService = _approveItemsService;
            parameters._formItemsService = _formItemsService;
            parameters._formInstanceService = _formInstanceService;
            parameters._formService = _formService;
            parameters._ticketService = _ticketService;
            parameters.UserManager = _userManager;
            parameters.EmployeeAssignmentService = _employeeAssignmentService;


            workFlowApiDto.UserName = User.Identity.Name;

            // Validate GUID format
            if (!Guid.TryParse(workFlowApiDto.workFlowItemId, out Guid workflowItemGuid))
            {
                return BadRequest(new { error = "Invalid workFlowItemId format" });
            }

            var workFlowItem = await _workFlowItemservice.GetByIdStringGuidAsync(workflowItemGuid);
            if (workFlowItem == null)
            {
                return NotFound(new { error = $"WorkflowItem with id '{workFlowApiDto.workFlowItemId}' not found" });
            }

            var workFlowHead = await _workFlowHeadService.GetByIdGuidAsync(new Guid(workFlowItem.WorkflowHeadId.ToString()));


            workFlowDto.WorkFlowDefinationId = workFlowHead.Data.WorkFlowDefinationId;
            workFlowDto.NodeId = workFlowItem.Id.ToString();
            workFlowDto.WorkFlowId = workFlowItem.WorkflowHeadId.ToString(); ;
            workFlowDto.ApproverItemId = workFlowApiDto.ApproveItem;
            // Artık Input yerine Action kullanılıyor (buton bazlı sistem)
            workFlowDto.Action = workFlowApiDto.Action;
            workFlowDto.UserName = workFlowApiDto.UserName;
            workFlowDto.Note = workFlowApiDto.Note;
            workFlowDto.NumberManDay = workFlowApiDto.NumberManDay;

            // FormData'yı payloadJson olarak gönder (Continue metodunda kullanılacak)
            var payloadJson = workFlowApiDto.FormData;

            var result = await execute.StartAsync(workFlowDto, parameters, payloadJson);
            
            // Generic response builder kullan
            var mapResult = _responseBuilder.BuildResponse(result);

            return mapResult;
        }

        [HttpPost]
        public async Task<ActionResult<WorkFlowHeadDtoResultStartOrContinue>> Start(WorkFlowStartApiDto workFlowApiDto)
        {
            WorkFlowExecute execute = new WorkFlowExecute();
            WorkFlowDto workFlowDto = new WorkFlowDto();

            WorkFlowParameters parameters = new WorkFlowParameters();
            parameters.workFlowService = _service;
            parameters.workFlowItemService = _workFlowItemservice;
            parameters._workFlowDefination = _workFlowDefinationDto;
            parameters._formItemsService = _formItemsService;
            parameters._formInstanceService = _formInstanceService;
            parameters._formService = _formService;
            parameters.UserManager = _userManager;
            parameters.EmployeeAssignmentService = _employeeAssignmentService;

            workFlowDto.WorkFlowDefinationId = new Guid(workFlowApiDto.DefinationId);
            workFlowDto.UserName = workFlowApiDto.UserName ?? User.Identity.Name;
            workFlowDto.WorkFlowInfo = workFlowApiDto.WorkFlowInfo;
            workFlowDto.Action = workFlowApiDto.Action;
            workFlowDto.Note = workFlowApiDto.Note;

            // FormData'yı payloadJson olarak gönder
            var payloadJson = workFlowApiDto.FormData;

            var result = await execute.StartAsync(workFlowDto, parameters, payloadJson);
            
            // Generic response builder kullan
            var mapResult = _responseBuilder.BuildResponse(result);

            return mapResult;
        }

        [HttpPost]
        public async Task<ActionResult<WorkFlowHeadDtoResultStartOrContinue>> StartAndTicket(WorkFlowStartApiDto workFlowApiDto)
        {
            WorkFlowExecute execute = new WorkFlowExecute();
            WorkFlowDto workFlowDto = new WorkFlowDto();

            WorkFlowParameters parameters = new WorkFlowParameters();
            parameters.workFlowService = _service;
            parameters.workFlowItemService = _workFlowItemservice;
            parameters._workFlowDefination = _workFlowDefinationDto;
            parameters._formItemsService = _formItemsService;
            parameters._formInstanceService = _formInstanceService;
            parameters._formService = _formService;
            parameters.UserManager = _userManager;
            parameters.EmployeeAssignmentService = _employeeAssignmentService;
            workFlowApiDto.UserName = User.Identity.Name;

            workFlowDto.WorkFlowDefinationId = new Guid(workFlowApiDto.DefinationId);
            workFlowDto.UserName = workFlowApiDto.UserName;

            workFlowDto.WorkFlowInfo = workFlowApiDto.WorkFlowInfo;


            var result = await execute.StartAsync(workFlowDto, parameters, null);
            
            // Generic response builder kullan
            var mapResult = _responseBuilder.BuildResponse(result);

            return mapResult;




        }


        /// <summary>
        /// Kullanıcının pending task'larını getirir (FormTask ve UserTask) - filtreleme desteği ile
        /// </summary>
        [HttpGet("my-tasks")]
        public async Task<ActionResult<MyTasksDto>> GetMyTasks([FromQuery] WorkflowFilterDto? filter)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }
            
            // Filter null ise default değerler kullan
            filter ??= new WorkflowFilterDto();

            // UserName'i UserId'ye çevir
            var currentUser = await _userManager.FindByNameAsync(userName);
            if (currentUser == null)
            {
                return Unauthorized("User not found");
            }
            string userId = currentUser.Id;

            var myTasks = new MyTasksDto();
            
            // EmployeeAssignment sorgusu için hazırlık (başlatan bilgileri için)
            var employeeAssignmentsQuery = await _employeeAssignmentService.Include();
            
            // Başlatan kullanıcıları cache'lemek için dictionary
            var starterUsersCache = new Dictionary<string, (string adSoyad, string departman, string pozisyon)>();

            // FormTask'ları getir (FormTaskNode için - FormItems)
            // GetAllRelationTable kullanarak tüm FormItems'ları al, sonra filtrele
            var allFormItems = await _formItemsService.GetAllRelationTable();
            
            // WorkFlowHead'i manuel olarak set et (eğer AutoMapper map edemediyse)
            foreach (var formItem in allFormItems)
            {
                if (formItem.WorkFlowHead == null && formItem.workFlowItem != null && formItem.workFlowItem.WorkflowHead != null)
                {
                    formItem.WorkFlowHead = _mapper.Map<WorkFlowHeadDto>(formItem.workFlowItem.WorkflowHead);
                }
            }
            
            var pendingFormItems = allFormItems
                .Where(e => e.FormItemStatus == FormItemStatus.Pending 
                    && e.FormUserId == userId  // ✅ UserId ile karşılaştır
                    && e.WorkFlowHead != null
                    && e.WorkFlowHead.workFlowStatus == formneo.core.Models.WorkflowStatus.InProgress)
                .OrderByDescending(e => e.CreatedDate)
                .ToList();

            // FormTask DTO'larına map et
            foreach (var formItem in pendingFormItems)
            {
                // WorkflowHeadId'yi workFlowItem'dan al
                Guid workflowHeadId = Guid.Empty;
                if (formItem.workFlowItem != null && formItem.workFlowItem.WorkflowHead != null && !string.IsNullOrEmpty(formItem.workFlowItem.WorkflowHead.id))
                {
                    workflowHeadId = new Guid(formItem.workFlowItem.WorkflowHead.id);
                }
                
                // Başlatan bilgilerini al
                string baslatanAdSoyad = "";
                string baslatanDepartman = "";
                string baslatanPozisyon = "";
                
                if (formItem.WorkFlowHead != null && !string.IsNullOrEmpty(formItem.WorkFlowHead.CreateUser))
                {
                    var createUserEmail = formItem.WorkFlowHead.CreateUser;
                    
                    // Cache'de var mı kontrol et
                    if (!starterUsersCache.ContainsKey(createUserEmail))
                    {
                        var starterUser = await _userManager.FindByNameAsync(createUserEmail);
                        if (starterUser != null)
                        {
                            baslatanAdSoyad = $"{starterUser.FirstName} {starterUser.LastName}".Trim();
                            
                            // EmployeeAssignment'tan departman ve pozisyon al
                            var starterAssignment = employeeAssignmentsQuery
                                .Where(ea => ea.UserId == starterUser.Id 
                                    && (ea.EndDate == null || ea.EndDate > DateTime.UtcNow)
                                    && ea.AssignmentType == AssignmentType.Primary)
                                .Include(ea => ea.OrgUnit)
                                .Include(ea => ea.Position)
                                .OrderByDescending(ea => ea.StartDate)
                                .FirstOrDefault();
                            
                            baslatanDepartman = starterAssignment?.OrgUnit?.Name ?? "";
                            baslatanPozisyon = starterAssignment?.Position?.Name ?? "";
                            
                            // Cache'e ekle
                            starterUsersCache[createUserEmail] = (baslatanAdSoyad, baslatanDepartman, baslatanPozisyon);
                        }
                    }
                    else
                    {
                        // Cache'den al
                        var cached = starterUsersCache[createUserEmail];
                        baslatanAdSoyad = cached.adSoyad;
                        baslatanDepartman = cached.departman;
                        baslatanPozisyon = cached.pozisyon;
                    }
                }
                
                // WorkFlowHead'i kopyala ve circular reference'ı kır
                var workFlowHeadDto = formItem.WorkFlowHead != null ? new WorkFlowHeadDto
                {
                    WorkflowName = formItem.WorkFlowHead.WorkflowName,
                    CurrentNodeId = formItem.WorkFlowHead.CurrentNodeId,
                    CurrentNodeName = formItem.WorkFlowHead.CurrentNodeName,
                    workFlowStatus = formItem.WorkFlowHead.workFlowStatus,
                    CreateUser = formItem.WorkFlowHead.CreateUser,
                    WorkFlowDefinationId = formItem.WorkFlowHead.WorkFlowDefinationId,
                    WorkFlowInfo = formItem.WorkFlowHead.WorkFlowInfo,
                    UniqNumber = formItem.WorkFlowHead.UniqNumber,
                    workflowItems = null // Circular reference'ı kır
                } : null;

                // WorkFlowItem'ı kopyala ve circular reference'ı kır
                WorkFlowItemDto workFlowItemDto = null;
                if (formItem.workFlowItem != null)
                {
                    workFlowItemDto = _mapper.Map<WorkFlowItemDto>(formItem.workFlowItem);
                    if (workFlowItemDto != null)
                    {
                        workFlowItemDto.WorkflowHead = null; // Circular reference'ı kır
                    }
                }

                // Form adını al
                string formAdi = "";
                if (formItem.FormId.HasValue)
                {
                    try
                    {
                        var form = await _formService.GetByIdStringGuidAsync(formItem.FormId.Value);
                        if (form != null)
                        {
                            formAdi = form.FormName ?? "";
                        }
                    }
                    catch
                    {
                        // Form bulunamazsa devam et
                    }
                }

                var formTaskDto = new FormTaskItemDto
                {
                    Id = formItem.Id,
                    WorkflowItemId = formItem.WorkflowItemId,
                    WorkflowHeadId = workflowHeadId,
                    ShortId = formItem.ShortId ?? formneo.workflow.Utils.ShortenGuid(formItem.Id),
                    ShortWorkflowItemId = formItem.ShortWorkflowItemId ?? (workflowHeadId != Guid.Empty ? formneo.workflow.Utils.ShortenGuid(workflowHeadId) : ""),
                    FormDesign = formItem.FormDesign,
                    FormId = formItem.FormId,
                    FormTaskMessage = formItem.FormTaskMessage,
                    FormDescription = formItem.FormDescription,
                    FormItemStatus = formItem.FormItemStatus,
                    WorkFlowHead = workFlowHeadDto,
                    WorkFlowItem = workFlowItemDto,
                    CreatedDate = formItem.CreatedDate,
                    UniqNumber = formItem.WorkFlowHead?.UniqNumber ?? 0,  // WorkflowHead'in UniqNumber'ı (ana süreç numarası)
                    BaslatanAdSoyad = baslatanAdSoyad,
                    BaslatanDepartman = baslatanDepartman,
                    BaslatanPozisyon = baslatanPozisyon,
                    // Yeni alanlar (GetMyStartedForms ile uyumlu)
                    SurecAdi = formItem.WorkFlowHead?.WorkflowName ?? "Bilinmiyor",
                    FormAdi = formAdi,
                    MevcutAdim = formItem.workFlowItem?.NodeName ?? "Form Doldurma",
                    Sure = CalculateDuration(formItem.CreatedDate),
                    SureDetayli = FormatDuration(formItem.CreatedDate),
                    Durum = GetStatusText(formItem.WorkFlowHead?.workFlowStatus)
                };
                myTasks.FormTasks.Add(formTaskDto);
            }

            // ApproveItems kullanılmıyor - UserTasks boş kalacak

            // MVP Filtreler - Sadece 3 basit filtre
            
            // 1. Süreç Tipi Filtresi
            if (filter.WorkFlowDefinationId.HasValue)
            {
                myTasks.FormTasks = myTasks.FormTasks
                    .Where(t => t.WorkFlowHead != null && t.WorkFlowHead.WorkFlowDefinationId == filter.WorkFlowDefinationId.Value)
                    .ToList();
            }
            
            // 2. Durum Filtresi
            if (filter.Durum.HasValue && filter.Durum.Value >= 0)
            {
                myTasks.FormTasks = myTasks.FormTasks
                    .Where(t => t.WorkFlowHead != null && (int)(t.WorkFlowHead.workFlowStatus ?? 0) == filter.Durum.Value)
                    .ToList();
            }
            
            // 3. Tarih Aralığı Filtreleri
            if (filter.BaslangicTarihiMin.HasValue)
            {
                myTasks.FormTasks = myTasks.FormTasks
                    .Where(t => t.CreatedDate >= filter.BaslangicTarihiMin.Value)
                    .ToList();
            }
            
            if (filter.BaslangicTarihiMax.HasValue)
            {
                myTasks.FormTasks = myTasks.FormTasks
                    .Where(t => t.CreatedDate <= filter.BaslangicTarihiMax.Value)
                    .ToList();
            }
            
            // Tarihe göre sırala (en yeniler üstte)
            myTasks.FormTasks = myTasks.FormTasks.OrderByDescending(t => t.CreatedDate).ToList();
            myTasks.TotalCount = myTasks.FormTasks.Count;

            return Ok(myTasks);
        }

        /// <summary>
        /// Workflow detail bilgilerini getirir (nodes, edges, approve items ve form items dahil)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkFlowHeadDetailDto>> GetDetail(Guid id)
        {
            var workFlowHead = await _workFlowHeadService.GetByIdGuidAsync(id);
            if (workFlowHead.Data == null)
            {
                return NotFound();
            }

            // Workflow items, approve items ve form items'ları dahil et
            var workflowItemsQuery = await _workFlowItemService.Include();
            var itemsWithApproves = workflowItemsQuery
                .Where(e => e.WorkflowHeadId == id)
                .Include(e => e.approveItems).ThenInclude(a => a.ApproveUser)  // ✅ Navigation property
                .Include(e => e.formItems).ThenInclude(f => f.FormUser)  // ✅ Navigation property
                .ToList();
            
            // Workflow definition'dan nodes ve edges bilgilerini al
            var workFlowDefination = await _workFlowDefinationDto.GetByIdGuidAsync(workFlowHead.Data.WorkFlowDefinationId);
            
            // WorkFlowItemDtoWithApproveItems'a map et ve FormItems'ları da ekle
            var workflowItemsDto = _mapper.Map<List<WorkFlowItemDtoWithApproveItems>>(itemsWithApproves);
            
            // FormItems'ları da map et
            foreach (var itemDto in workflowItemsDto)
            {
                var workflowItem = itemsWithApproves.FirstOrDefault(e => e.NodeId == itemDto.NodeId);
                if (workflowItem != null && workflowItem.formItems != null && workflowItem.formItems.Count > 0)
                {
                    itemDto.formItems = _mapper.Map<List<FormItemsDto>>(workflowItem.formItems);
                    // ShortId'leri ekle
                    foreach (var formItemDto in itemDto.formItems)
                    {
                        formItemDto.ShortId = formneo.workflow.Utils.ShortenGuid(formItemDto.Id);
                        // WorkflowHeadId'yi workFlowItem'dan al
                        if (formItemDto.workFlowItem != null && formItemDto.workFlowItem.WorkflowHead != null && !string.IsNullOrEmpty(formItemDto.workFlowItem.WorkflowHead.id))
                        {
                            var workflowHeadId = new Guid(formItemDto.workFlowItem.WorkflowHead.id);
                            formItemDto.ShortWorkflowItemId = formneo.workflow.Utils.ShortenGuid(workflowHeadId);
                        }
                    }
                }
            }
            
            var detailDto = new WorkFlowHeadDetailDto
            {
                Id = id.ToString(), // Id'yi parametreden al
                WorkflowName = workFlowHead.Data.WorkflowName,
                WorkFlowInfo = workFlowHead.Data.WorkFlowInfo,
                WorkFlowStatus = workFlowHead.Data.workFlowStatus,
                CreateUser = workFlowHead.Data.CreateUser,
                UniqNumber = workFlowHead.Data.UniqNumber,
                WorkFlowDefinationId = workFlowHead.Data.WorkFlowDefinationId,
                WorkflowItems = workflowItemsDto,
                WorkFlowDefinationJson = workFlowDefination?.Data?.Defination
            };

            return detailDto;
        }

        /// <summary>
        /// WorkflowItem ID'sine göre görev detayını getirir (BMP mantığı)
        /// WorkflowItem'ın NodeType'ına göre FormTask veya UserTask detaylarını döndürür
        /// Kullanıcı "görevlerim" sayfasından bir göreve tıkladığında bu endpoint çağrılır
        /// </summary>
        /// <param name="workflowItemId">WorkflowItem ID'si</param>
        /// <returns>Görev tipine göre FormTask veya UserTask detayları ve form bilgileri</returns>
        [HttpGet("workflowitem/{workflowItemId}/task-detail")]
        public async Task<ActionResult<TaskFormDto>> GetTaskDetailByWorkflowItemId(Guid workflowItemId)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            // UserName'i UserId'ye çevir
            var currentUser = await _userManager.FindByNameAsync(userName);
            if (currentUser == null)
            {
                return Unauthorized("User not found");
            }
            string userId = currentUser.Id;

            // WorkflowItem'ı bul - formItems ve approveItems ile birlikte
            var workflowItemQuery = await _workFlowItemService.Include();
            var workflowItem = workflowItemQuery
                .Where(e => e.Id == workflowItemId)
                .Include(e => e.WorkflowHead)
                    .ThenInclude(wh => wh.WorkFlowDefination)  // WorkflowDefinition'ı da include et
                .Include(e => e.formItems).ThenInclude(f => f.FormUser)  // ✅ Navigation property
                .Include(e => e.approveItems).ThenInclude(a => a.ApproveUser)  // ✅ Navigation property
                .FirstOrDefault();

            if (workflowItem == null)
            {
                return NotFound($"WorkflowItem with id '{workflowItemId}' not found");
            }

            // WorkflowHeadId'yi al
            Guid workflowHeadId = workflowItem.WorkflowHeadId;

            // Node'un script'ini WorkflowDefinition'dan al
            string nodeScript = null;
            if (workflowItem.WorkflowHead?.WorkFlowDefination?.Defination != null)
            {
                try
                {
                    var definition = JObject.Parse(workflowItem.WorkflowHead.WorkFlowDefination.Defination);
                    var nodes = definition["nodes"] as JArray;
                    if (nodes != null)
                    {
                        var currentNode = nodes.FirstOrDefault(n => n["id"]?.ToString() == workflowItem.NodeId);
                        if (currentNode != null)
                        {
                            // Script'i node.data.script'ten al
                            nodeScript = currentNode["data"]?["initScript"]?.ToString();
                        }
                    }
                }
                catch
                {
                    // JSON parse hatası durumunda nodeScript null kalır
                }
            }

            // NodeType'a göre FormTask mı UserTask mı belirle
            bool isFormTaskNode = workflowItem.NodeType == "formTaskNode";
            bool isApproverNode = workflowItem.NodeType == "approverNode";

            if (!isFormTaskNode && !isApproverNode)
            {
                return BadRequest($"WorkflowItem with NodeType '{workflowItem.NodeType}' is not supported. Only 'formTaskNode' and 'approverNode' are supported.");
            }

            TaskFormDto taskFormDto = null;

            if (isFormTaskNode)
            {
                // FormTaskNode için FormItem'ı bul
                var formItem = workflowItem.formItems?
                    .Where(e => e.FormUserId == userId && e.FormItemStatus == FormItemStatus.Pending)  // ✅ UserId ile karşılaştır
                    .OrderByDescending(e => e.CreatedDate)
                    .FirstOrDefault();

                if (formItem == null)
                {
                    return NotFound($"FormTask for user '{userName}' not found in WorkflowItem '{workflowItemId}'");
                }

                // FormInstance'dan FormData ve FormDesign'ı al (WorkflowHeadId ile)
                // FormInstance her zaman güncel form verisini tutar
                // FormInstance yoksa (form henüz başlamamış), FormItem'dan FormDesign al
                string formData = null;
                string formDesign = formItem.FormDesign; // Varsayılan olarak FormItem'dan al
                Guid? formId = formItem.FormId;

                if (workflowHeadId != Guid.Empty)
                {
                    var formInstanceQuery = _formInstanceService.Where(e => e.WorkflowHeadId == workflowHeadId);
                    var formInstance = await formInstanceQuery.FirstOrDefaultAsync();
                    if (formInstance != null)
                    {
                        // FormInstance varsa, güncel verileri kullan
                        formData = formInstance.FormData;
                        // FormInstance'da FormDesign varsa onu kullan (daha güncel olabilir)
                        if (!string.IsNullOrEmpty(formInstance.FormDesign))
                        {
                            formDesign = formInstance.FormDesign;
                        }
                        // FormInstance'da FormId varsa onu kullan
                        if (formInstance.FormId.HasValue)
                        {
                            formId = formInstance.FormId;
                        }
                    }
                    else
                    {
                        // FormInstance yoksa (form henüz başlamamış)
                        // FormItem'dan FormDesign zaten alındı, ama boşsa FormId varsa Form tablosundan al
                        if (string.IsNullOrEmpty(formDesign) && formId.HasValue)
                        {
                            try
                            {
                                var form = await _formService.GetByIdStringGuidAsync(formId.Value);
                                if (form != null && !string.IsNullOrEmpty(form.FormDesign))
                                {
                                    formDesign = form.FormDesign;
                                }
                            }
                            catch
                            {
                                // Form bulunamazsa devam et
                            }
                        }
                    }
                }

                taskFormDto = new TaskFormDto
                {
                    NodeType = workflowItem.NodeType,
                    NodeName = workflowItem.NodeName,
                    TaskType = "formTask",
                    WorkflowHeadId = workflowHeadId,
                    WorkflowItemId = workflowItemId,
                    FormDesign = formDesign,
                    FormData = formData,
                    FormId = formId,
                    // FormTask detayları
                    FormItemId = formItem.Id,
                    FormTaskMessage = formItem.FormTaskMessage,
                    FormDescription = formItem.FormDescription,
                    FormUser = formItem.FormUser?.UserName ?? formItem.FormUserId,  // ✅ Navigation property'den email al
                    FormItemStatus = formItem.FormItemStatus,
                    NodeScript = nodeScript  // Node'un script'i (scriptNode için)
                };
            }
            else if (isApproverNode)
            {
                // ApproverNode için ApproveItem'ı bul
                var approveItem = workflowItem.approveItems?
                    .Where(e => e.ApproveUserId == userId && e.ApproverStatus == ApproverStatus.Pending)  // ✅ UserId ile karşılaştır
                    .OrderByDescending(e => e.CreatedDate)
                    .FirstOrDefault();

                if (approveItem == null)
                {
                    return NotFound($"UserTask for user '{userName}' not found in WorkflowItem '{workflowItemId}'");
                }

                // FormInstance'dan FormDesign ve FormData'yı al (WorkflowHeadId ile)
                // FormInstance yoksa, WorkflowItem'dan FormItem'ları bul veya FormId varsa Form tablosundan al
                string formDesign = null;
                string formData = null;
                Guid? formId = null;

                if (workflowHeadId != Guid.Empty)
                {
                    var formInstanceQuery = _formInstanceService.Where(e => e.WorkflowHeadId == workflowHeadId);
                    var formInstance = await formInstanceQuery.FirstOrDefaultAsync();
                    if (formInstance != null)
                    {
                        // FormInstance varsa, güncel verileri kullan
                        formDesign = formInstance.FormDesign;
                        formData = formInstance.FormData;
                        formId = formInstance.FormId;
                    }
                    else
                    {
                        // FormInstance yoksa (form henüz başlamamış), WorkflowItem'dan FormItem'ları bul
                        // Aynı WorkflowHeadId'ye sahip diğer WorkflowItem'lardan FormItem'ları bul
                        var allWorkflowItemsQuery = await _workFlowItemService.Include();
                        var allWorkflowItems = allWorkflowItemsQuery
                            .Where(e => e.WorkflowHeadId == workflowHeadId)
                            .Include(e => e.formItems)
                            .ToList();

                        // Tüm FormItem'ları topla
                        var allFormItems = allWorkflowItems
                            .Where(e => e.formItems != null && e.formItems.Count > 0)
                            .SelectMany(e => e.formItems)
                            .OrderByDescending(e => e.CreatedDate)
                            .ToList();

                        if (allFormItems.Count > 0)
                        {
                            // En son FormItem'dan FormDesign al
                            var lastFormItem = allFormItems.First();
                            formDesign = lastFormItem.FormDesign;
                            formId = lastFormItem.FormId;
                        }

                        // Eğer hala FormDesign yoksa ve FormId varsa, Form tablosundan al
                        if (string.IsNullOrEmpty(formDesign) && formId.HasValue)
                        {
                            try
                            {
                                var form = await _formService.GetByIdStringGuidAsync(formId.Value);
                                if (form != null && !string.IsNullOrEmpty(form.FormDesign))
                                {
                                    formDesign = form.FormDesign;
                                }
                            }
                            catch
                            {
                                // Form bulunamazsa devam et
                            }
                        }
                    }
                }

                taskFormDto = new TaskFormDto
                {
                    NodeType = workflowItem.NodeType,
                    NodeName = workflowItem.NodeName,
                    TaskType = "userTask",
                    WorkflowHeadId = workflowHeadId,
                    WorkflowItemId = workflowItemId,
                    FormDesign = formDesign,
                    FormData = formData,
                    FormId = formId,
                    // UserTask detayları
                    ApproveItemId = approveItem.Id,
                    ApproveUser = approveItem.ApproveUser?.UserName ?? approveItem.ApproveUserId,  // ✅ Navigation property'den
                    ApproveUserNameSurname = approveItem.ApproveUserNameSurname,
                    ApproverStatus = approveItem.ApproverStatus,
                    WorkFlowDescription = approveItem.WorkFlowDescription,
                    NodeScript = nodeScript  // Node'un script'i (scriptNode için)
                };
            }

            if (taskFormDto == null)
            {
                return NotFound("Task not found");
            }

            return Ok(taskFormDto);
        }

        private void Validations()      
        {


        }

        /// <summary>
        /// Kullanıcının başlattığı tüm formları listeler (filtreleme desteği ile)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<MyStartedFormsResultDto>> GetMyStartedForms([FromQuery] WorkflowFilterDto? filter)
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("Kullanıcı bulunamadı");
            }
            
            // Filter null ise default değerler kullan
            filter ??= new WorkflowFilterDto();

            // Kullanıcının başlattığı workflow'ları getir - DATABASE SEVİYESİNDE FİLTRELEME
            var workflowHeadsQuery = await _workFlowHeadService.Include();
            
            // Temel filtre: Kullanıcının başlattığı, silinmemiş kayıtlar
            var query = workflowHeadsQuery
                .Where(w => w.CreateUser == userName && !w.IsDelete);
            
            // MVP Filtreler - Sadece 3 tane
            
            // 1. Süreç Tipi Filtresi
            if (filter.WorkFlowDefinationId.HasValue)
            {
                query = query.Where(w => w.WorkFlowDefinationId == filter.WorkFlowDefinationId.Value);
            }
            
            // 2. Durum Filtresi
            if (filter.Durum.HasValue && filter.Durum.Value >= 0)
            {
                query = query.Where(w => (int?)w.workFlowStatus == filter.Durum.Value);
            }
            
            // 3. Tarih Aralığı Filtreleri
            if (filter.BaslangicTarihiMin.HasValue)
            {
                query = query.Where(w => w.CreatedDate >= filter.BaslangicTarihiMin.Value);
            }
            
            if (filter.BaslangicTarihiMax.HasValue)
            {
                query = query.Where(w => w.CreatedDate <= filter.BaslangicTarihiMax.Value);
            }
            
            // Include'ları ekle ve tarihe göre sırala (en yeniler üstte)
            var myStartedWorkflows = await query
                .Include(w => w.WorkFlowDefination)
                .Include(w => w.Form)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
            
            var totalCount = myStartedWorkflows.Count;

            // Başlatan kullanıcının bilgilerini al
            var currentUser = await _userManager.FindByNameAsync(userName);
            if (currentUser == null)
            {
                return Unauthorized("Kullanıcı bilgileri bulunamadı");
            }

            // Aktif EmployeeAssignment'ı al (departman ve pozisyon bilgileri için)
            var employeeAssignmentsQuery = await _employeeAssignmentService.Include();
            var activeAssignment = employeeAssignmentsQuery
                .Where(ea => ea.UserId == currentUser.Id 
                    && (ea.EndDate == null || ea.EndDate > DateTime.UtcNow)
                    && ea.AssignmentType == AssignmentType.Primary)
                .Include(ea => ea.OrgUnit)
                .Include(ea => ea.Position)
                .OrderByDescending(ea => ea.StartDate)
                .FirstOrDefault();

            // Kullanıcı bilgilerini oluştur
            var baslatanAdSoyad = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            var departman = activeAssignment?.OrgUnit?.Name ?? "Belirtilmemiş";
            var pozisyon = activeAssignment?.Position?.Name ?? "Belirtilmemiş";

            // FormItems ve ApproveItems'ları getir (kimde bekliyor bilgisi için)
            var allFormItems = await _formItemsService.GetAllRelationTable();
            var allApproveItems = await _approveItemsService.GetAllRelationTable();

            // Sonuçları oluştur
            var result = new List<MyStartedFormDto>();
            
            foreach (var w in myStartedWorkflows)
            {
                string kimdeOnayda = "";
                string bekleyenKullaniciAdi = "";
                string bekleyenKullaniciDepartman = "";
                string bekleyenKullaniciPozisyon = "";
                string mevcutAdim = w.CurrentNodeName ?? "Başlangıç";  // Varsayılan değer

                // Eğer workflow devam ediyorsa, kimde bekliyor bilgisini bul
                if (w.workFlowStatus == formneo.core.Models.WorkflowStatus.InProgress)
                {
                    // Önce FormItems'da pending olanı ara
                    var pendingFormItem = allFormItems
                        .Where(fi => fi.WorkFlowHead != null 
                            && fi.WorkFlowHead.UniqNumber == w.UniqNumber 
                            && fi.FormItemStatus == FormItemStatus.Pending)
                        .FirstOrDefault();

                    if (pendingFormItem != null && !string.IsNullOrEmpty(pendingFormItem.FormUserId))
                    {
                        var pendingUser = await _userManager.FindByIdAsync(pendingFormItem.FormUserId);
                        if (pendingUser != null)
                        {
                            bekleyenKullaniciAdi = $"{pendingUser.FirstName} {pendingUser.LastName}".Trim();
                            
                            // Bekleyen kullanıcının departman ve pozisyon bilgisini al
                            var pendingUserAssignment = employeeAssignmentsQuery
                                .Where(ea => ea.UserId == pendingUser.Id 
                                    && (ea.EndDate == null || ea.EndDate > DateTime.UtcNow)
                                    && ea.AssignmentType == AssignmentType.Primary)
                                .Include(ea => ea.OrgUnit)
                                .Include(ea => ea.Position)
                                .FirstOrDefault();

                            bekleyenKullaniciDepartman = pendingUserAssignment?.OrgUnit?.Name ?? "";
                            bekleyenKullaniciPozisyon = pendingUserAssignment?.Position?.Name ?? "";
                            kimdeOnayda = "Form Doldurma";
                            
                            // Mevcut adım: Pending olan item'ın NodeName'i
                            mevcutAdim = pendingFormItem.workFlowItem?.NodeName ?? mevcutAdim;
                        }
                    }

                    // FormItem yoksa ApproveItems'da pending olanı ara
                    if (string.IsNullOrEmpty(bekleyenKullaniciAdi))
                    {
                        var pendingApproveItem = allApproveItems
                            .Where(ai => ai.WorkFlowHead != null 
                                && ai.WorkFlowHead.UniqNumber == w.UniqNumber 
                                && ai.ApproverStatus == ApproverStatus.Pending)
                            .FirstOrDefault();

                        if (pendingApproveItem != null && !string.IsNullOrEmpty(pendingApproveItem.ApproveUserId))
                        {
                            var pendingUser = await _userManager.FindByIdAsync(pendingApproveItem.ApproveUserId);
                            if (pendingUser != null)
                            {
                                bekleyenKullaniciAdi = $"{pendingUser.FirstName} {pendingUser.LastName}".Trim();
                                
                                // Bekleyen kullanıcının departman ve pozisyon bilgisini al
                                var pendingUserAssignment = employeeAssignmentsQuery
                                    .Where(ea => ea.UserId == pendingUser.Id 
                                        && (ea.EndDate == null || ea.EndDate > DateTime.UtcNow)
                                        && ea.AssignmentType == AssignmentType.Primary)
                                    .Include(ea => ea.OrgUnit)
                                    .Include(ea => ea.Position)
                                    .FirstOrDefault();

                                bekleyenKullaniciDepartman = pendingUserAssignment?.OrgUnit?.Name ?? "";
                                bekleyenKullaniciPozisyon = pendingUserAssignment?.Position?.Name ?? "";
                                kimdeOnayda = "Onay";
                                
                                // Mevcut adım: Pending olan item'ın NodeName'i
                                mevcutAdim = pendingApproveItem.workFlowItem?.NodeName ?? mevcutAdim;
                            }
                        }
                    }
                }

                result.Add(new MyStartedFormDto
                {
                    Id = w.Id,
                    SurecAdi = w.WorkflowName ?? w.WorkFlowDefination?.WorkflowName ?? "Bilinmiyor",
                    Baslatan = w.CreateUser,
                    BaslatanAdSoyad = baslatanAdSoyad,
                    BaslatanDepartman = departman,
                    BaslatanPozisyon = pozisyon,
                    MevcutAdim = mevcutAdim,  // Pending olan item'ın NodeName'i
                    BaslangicTarihi = w.CreatedDate,
                    Sure = CalculateDuration(w.CreatedDate),
                    SureDetayli = FormatDuration(w.CreatedDate),
                    Durum = GetStatusText(w.workFlowStatus),
                    DurumEnum = w.workFlowStatus,
                    FormAdi = w.Form?.FormName ?? "",
                    WorkFlowDefinationId = w.WorkFlowDefinationId,
                    UniqNumber = w.UniqNumber,
                    // Kimde bekliyor bilgileri
                    KimdeOnayda = kimdeOnayda,
                    BekleyenKullanici = bekleyenKullaniciAdi,
                    BekleyenDepartman = bekleyenKullaniciDepartman,
                    BekleyenPozisyon = bekleyenKullaniciPozisyon
                });
            }

            var responseDto = new MyStartedFormsResultDto
            {
                TotalCount = totalCount,
                Data = result
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// Tarih farkını gün cinsinden hesaplar
        /// </summary>
        private int CalculateDuration(DateTime startDate)
        {
            // Database'den gelen tarih zaten local time (GmtPlus3)
            var duration = DateTime.Now - startDate;
            return (int)duration.TotalDays;
        }

        /// <summary>
        /// Tarih farkını okunabilir formatta döner
        /// </summary>
        private string FormatDuration(DateTime startDate)
        {
            // Database'den gelen tarih zaten local time (GmtPlus3)
            var duration = DateTime.Now - startDate;
            
            if (duration.TotalMinutes < 60)
            {
                return $"{(int)duration.TotalMinutes} dakika önce";
            }
            else if (duration.TotalHours < 24)
            {
                return $"{(int)duration.TotalHours} saat önce";
            }
            else if (duration.TotalDays < 30)
            {
                return $"{(int)duration.TotalDays} gün önce";
            }
            else if (duration.TotalDays < 365)
            {
                int months = (int)(duration.TotalDays / 30);
                return $"{months} ay önce";
            }
            else
            {
                int years = (int)(duration.TotalDays / 365);
                return $"{years} yıl önce";
            }
        }

        /// <summary>
        /// WorkflowStatus enum'unu Türkçe metne çevirir
        /// </summary>
        private string GetStatusText(formneo.core.Models.WorkflowStatus? status)
        {
            return status switch
            {
                formneo.core.Models.WorkflowStatus.NotStarted => "Başlamadı",
                formneo.core.Models.WorkflowStatus.InProgress => "Devam Ediyor",
                formneo.core.Models.WorkflowStatus.Completed => "Tamamlandı",
                formneo.core.Models.WorkflowStatus.Pending => "Beklemede",
                formneo.core.Models.WorkflowStatus.SendBack => "Geri Gönderildi",
                _ => "Bilinmiyor"
            };
        }

    }
}
