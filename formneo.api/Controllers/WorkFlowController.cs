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

            var workFlowItem = await _workFlowItemservice.GetByIdStringGuidAsync(new Guid(workFlowApiDto.workFlowItemId));
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



            //    var workFlowApi = new WorkFlowApi(configuration);
            //    let startDto: WorkFlowStartApiDto = { };
            //    startDto.definationId = "b08b506c-1d98-4feb-a680-64ddc67b3c39";
            //    startDto.userName = username;
            //    startDto.workFlowInfo = "Terfi Edecek Kişi:" + `${ usernameTerfi} (${ IcKaynakname})`;
            //    var result = await workFlowApi.apiWorkFlowStartPost(startDto)
            //        .then(response => {
            //        onSubmit(response.data.id!, true);
            //        dispatchAlert({ message: "Kayıt Güncelleme Başarılı", type: MessageBoxTypes.Success })
            //           dispatchBusy({ isBusy: false });

            //})



        }

        /// <summary>
        /// Kullanıcının pending task'larını getirir (FormTask ve UserTask)
        /// </summary>
        [HttpGet("my-tasks")]
        public async Task<ActionResult<MyTasksDto>> GetMyTasks()
        {
            var userName = User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized();
            }

            var myTasks = new MyTasksDto();

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
                    && e.FormUser == userName
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
                    UniqNumber = formItem.UniqNumber
                };
                myTasks.FormTasks.Add(formTaskDto);
            }

            // UserTask'ları getir (ApproverNode için - ApproveItems)
            var approveItems = await _approveItemsService.GetAllRelationTable();
            var pendingApproveItems = approveItems
                .Where(e => e.ApproverStatus == ApproverStatus.Pending 
                    && e.ApproveUser == userName
                    && e.WorkFlowHead != null
                    && e.WorkFlowHead.workFlowStatus == formneo.core.Models.WorkflowStatus.InProgress)
                .OrderByDescending(e => e.CreatedDate)
                .ToList();

            // UserTask DTO'larına map et
            foreach (var approveItem in pendingApproveItems)
            {
                // WorkflowHeadId'yi workFlowItem'dan al
                Guid workflowHeadId = Guid.Empty;
                if (approveItem.workFlowItem != null && approveItem.workFlowItem.WorkflowHead != null && !string.IsNullOrEmpty(approveItem.workFlowItem.WorkflowHead.id))
                {
                    workflowHeadId = new Guid(approveItem.workFlowItem.WorkflowHead.id);
                }
                
                // WorkFlowHead'i kopyala ve circular reference'ı kır
                var userWorkFlowHeadDto = approveItem.WorkFlowHead != null ? new WorkFlowHeadDto
                {
                    WorkflowName = approveItem.WorkFlowHead.WorkflowName,
                    CurrentNodeId = approveItem.WorkFlowHead.CurrentNodeId,
                    CurrentNodeName = approveItem.WorkFlowHead.CurrentNodeName,
                    workFlowStatus = approveItem.WorkFlowHead.workFlowStatus,
                    CreateUser = approveItem.WorkFlowHead.CreateUser,
                    WorkFlowDefinationId = approveItem.WorkFlowHead.WorkFlowDefinationId,
                    WorkFlowInfo = approveItem.WorkFlowHead.WorkFlowInfo,
                    UniqNumber = approveItem.WorkFlowHead.UniqNumber,
                    workflowItems = null // Circular reference'ı kır
                } : null;

                // WorkFlowItem'ı kopyala ve circular reference'ı kır
                WorkFlowItemDto userWorkFlowItemDto = null;
                if (approveItem.workFlowItem != null)
                {
                    userWorkFlowItemDto = _mapper.Map<WorkFlowItemDto>(approveItem.workFlowItem);
                    if (userWorkFlowItemDto != null)
                    {
                        userWorkFlowItemDto.WorkflowHead = null; // Circular reference'ı kır
                    }
                }

                var userTaskDto = new UserTaskItemDto
                {
                    Id = approveItem.Id,
                    WorkflowItemId = approveItem.WorkflowItemId,
                    WorkflowHeadId = workflowHeadId,
                    ShortId = approveItem.ShortId,
                    ShortWorkflowItemId = approveItem.ShortWorkflowItemId,
                    ApproveUser = approveItem.ApproveUser,
                    ApproveUserNameSurname = approveItem.ApproveUserNameSurname,
                    ApproverStatus = approveItem.ApproverStatus,
                    WorkFlowHead = userWorkFlowHeadDto,
                    WorkFlowItem = userWorkFlowItemDto,
                    CreatedDate = approveItem.CreatedDate,
                    UniqNumber = approveItem.UniqNumber
                };
                myTasks.UserTasks.Add(userTaskDto);
            }

            myTasks.TotalCount = myTasks.FormTasks.Count + myTasks.UserTasks.Count;

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
                .Include(e => e.approveItems)
                .Include(e => e.formItems)
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

            // WorkflowItem'ı bul - formItems ve approveItems ile birlikte
            var workflowItemQuery = await _workFlowItemService.Include();
            var workflowItem = workflowItemQuery
                .Where(e => e.Id == workflowItemId)
                .Include(e => e.WorkflowHead)
                .Include(e => e.formItems)
                .Include(e => e.approveItems)
                .FirstOrDefault();

            if (workflowItem == null)
            {
                return NotFound($"WorkflowItem with id '{workflowItemId}' not found");
            }

            // WorkflowHeadId'yi al
            Guid workflowHeadId = workflowItem.WorkflowHeadId;

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
                    .Where(e => e.FormUser == userName && e.FormItemStatus == FormItemStatus.Pending)
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
                    FormUser = formItem.FormUser,
                    FormItemStatus = formItem.FormItemStatus
                };
            }
            else if (isApproverNode)
            {
                // ApproverNode için ApproveItem'ı bul
                var approveItem = workflowItem.approveItems?
                    .Where(e => e.ApproveUser == userName && e.ApproverStatus == ApproverStatus.Pending)
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
                    ApproveUser = approveItem.ApproveUser,
                    ApproveUserNameSurname = approveItem.ApproveUserNameSurname,
                    ApproverStatus = approveItem.ApproverStatus,
                    WorkFlowDescription = approveItem.WorkFlowDescription
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

    }
}
