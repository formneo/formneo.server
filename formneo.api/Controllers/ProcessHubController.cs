using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using formneo.core.DTOs;
using formneo.core.DTOs.ProcessHub;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace formneo.api.Controllers
{
    [Route("api/process-hub")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProcessHubController : CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkFlowItemRepository _workflowItemRepository;
        private readonly IApproveItemsRepository _approveItemsRepository;
        private readonly IFormItemsRepository _formItemsRepository;
        private readonly IUserOrgInfoService _userOrgInfoService;

        public ProcessHubController(
            IMapper mapper,
            IWorkflowRepository workflowRepository,
            IWorkFlowItemRepository workflowItemRepository,
            IApproveItemsRepository approveItemsRepository,
            IFormItemsRepository formItemsRepository,
            IUserOrgInfoService userOrgInfoService)
        {
            _mapper = mapper;
            _workflowRepository = workflowRepository;
            _workflowItemRepository = workflowItemRepository;
            _approveItemsRepository = approveItemsRepository;
            _formItemsRepository = formItemsRepository;
            _userOrgInfoService = userOrgInfoService;
        }

        /// <summary>
        /// Workflow data'sını view type'a göre getirir - Performans optimize edilmiş
        /// </summary>
        /// <param name="workflowId">Workflow Definition GUID</param>
        /// <param name="viewType">my, approvals, all</param>
        /// <param name="page">Sayfa numarası (1'den başlar)</param>
        /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
        /// <returns>Paginated workflow data</returns>
        [HttpGet("data")]
        public async Task<ActionResult<ProcessHubDataResponseDto>> GetData(
            [FromQuery] Guid workflowId,
            [FromQuery] string viewType = "my",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Kullanıcı bilgisini al
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı");
            }

            // Kullanıcı ID'sini al (Approvals için gerekli)
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("Kullanıcı ID'si alınamadı");
            }

            // Frontend'den gelen değerleri normalize et (my_requests -> My)
            var normalizedViewType = viewType.ToLower() switch
            {
                "my" or "my_requests" => "My",
                "approvals" or "my_approvals" => "Approvals",
                "all" => "All",
                _ => viewType
            };

            // ViewType'ı parse et
            if (!Enum.TryParse<ProcessViewType>(normalizedViewType, true, out var parsedViewType))
            {
                return BadRequest($"Geçersiz viewType: {viewType}. Geçerli değerler: my, my_requests, approvals, my_approvals, all");
            }

            // Validation
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Max limit

            // Base query - sadece gerekli alanları include et (performans için)
            var query = _workflowRepository
                .Where(w => w.WorkFlowDefinationId == workflowId)
                .Include(w => w.WorkFlowDefination)
                .Include(w => w.FormInstance)
                .ThenInclude(fi => fi.Form)
                .AsQueryable();

            // ViewType'a göre filtrele
            switch (parsedViewType)
            {
                case ProcessViewType.My:
                    // Kullanıcının oluşturduğu kayıtlar
                    query = query.Where(w => w.CreateUser == currentUser);
                    break;

                case ProcessViewType.Approvals:
                    // Kullanıcının onaylaması gereken kayıtlar (USER ID ile çalışır)
                    // 1. ApproveItems'dan WorkflowItemId'leri al
                    var approveWorkflowItemIds = await _approveItemsRepository
                        .Where(ai => 
                            ai.ApproveUserId == currentUserId && 
                            ai.ApproverStatus == ApproverStatus.Pending)
                        .Select(ai => ai.WorkflowItemId)
                        .Distinct()
                        .ToListAsync();

                    // WorkflowItem'ları çek ve WorkflowHeadId'leri al
                    var workflowIdsWithPendingApprovals = await _workflowItemRepository
                        .Where(wi => approveWorkflowItemIds.Contains(wi.Id))
                        .Select(wi => wi.WorkflowHeadId)
                        .Distinct()
                        .ToListAsync();

                    // 2. FormItems'dan WorkflowItemId'leri al
                    var formWorkflowItemIds = await _formItemsRepository
                        .Where(fi => 
                            fi.FormUserId == currentUserId && 
                            fi.FormItemStatus == FormItemStatus.Pending)
                        .Select(fi => fi.WorkflowItemId)
                        .Distinct()
                        .ToListAsync();

                    // WorkflowItem'ları çek ve WorkflowHeadId'leri al
                    var workflowIdsWithPendingForms = await _workflowItemRepository
                        .Where(wi => formWorkflowItemIds.Contains(wi.Id))
                        .Select(wi => wi.WorkflowHeadId)
                        .Distinct()
                        .ToListAsync();

                    // 3. Her iki listeyi birleştir (kullanıcının ya onayı ya da formu olan tüm workflow'lar)
                    var combinedWorkflowIds = workflowIdsWithPendingApprovals
                        .Union(workflowIdsWithPendingForms)
                        .Distinct()
                        .ToList();

                    query = query.Where(w => combinedWorkflowIds.Contains(w.Id));
                    break;

                case ProcessViewType.All:
                    // Tüm kayıtlar - Yetkiye göre filtreleme yapılabilir
                    // Şimdilik tüm kayıtları göster
                    break;
            }

            // Toplam kayıt sayısı (pagination için)
            var totalCount = await query.CountAsync();

            // Sayfalama ve sıralama (en yeni kayıtlar önce)
            var workflowHeads = await query
                .OrderByDescending(w => w.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // WorkflowHead ID'lerini topla
            var workflowHeadIds = workflowHeads.Select(w => w.Id).ToList();

            // Her workflow için pending item sayılarını tek sorguda al (performans) - USER ID ile
            // İki aşamalı sorgu: WorkflowItemId → WorkflowHeadId mapping
            
            // 1. WorkflowHeadId'lere ait WorkflowItem'ları mapping'le
            var workflowItemToHeadMap = await _workflowItemRepository
                .Where(wi => workflowHeadIds.Contains(wi.WorkflowHeadId))
                .Select(wi => new { wi.Id, wi.WorkflowHeadId })
                .ToListAsync();
            
            var workflowItemToHeadDict = workflowItemToHeadMap.ToDictionary(x => x.Id, x => x.WorkflowHeadId);
            var relevantWorkflowItemIds = workflowItemToHeadDict.Keys.ToList();

            // 2. ApproveItems'dan pending olanları al
            var approvalItems = await _approveItemsRepository
                .Where(ai => 
                    ai.ApproverStatus == ApproverStatus.Pending &&
                    ai.ApproveUserId == currentUserId &&
                    relevantWorkflowItemIds.Contains(ai.WorkflowItemId))
                .Select(ai => ai.WorkflowItemId)
                .ToListAsync();
            
            var pendingApprovals = approvalItems
                .Where(workflowItemId => workflowItemToHeadDict.ContainsKey(workflowItemId))
                .GroupBy(workflowItemId => workflowItemToHeadDict[workflowItemId])
                .Select(g => new { WorkflowHeadId = g.Key, Count = g.Count() })
                .ToList();

            // 3. FormItems'dan pending olanları al
            var formItems = await _formItemsRepository
                .Where(fi => 
                    fi.FormItemStatus == FormItemStatus.Pending &&
                    fi.FormUserId == currentUserId &&
                    relevantWorkflowItemIds.Contains(fi.WorkflowItemId))
                .Select(fi => fi.WorkflowItemId)
                .ToListAsync();
            
            var pendingForms = formItems
                .Where(workflowItemId => workflowItemToHeadDict.ContainsKey(workflowItemId))
                .GroupBy(workflowItemId => workflowItemToHeadDict[workflowItemId])
                .Select(g => new { WorkflowHeadId = g.Key, Count = g.Count() })
                .ToList();

            // Her workflow için pending olan tüm FormUserIds'leri al (kullanıcıdan bağımsız)
            var allFormItems = await _formItemsRepository
                .Where(fi => 
                    fi.FormItemStatus == FormItemStatus.Pending &&
                    relevantWorkflowItemIds.Contains(fi.WorkflowItemId))
                .Select(fi => new { fi.WorkflowItemId, fi.FormUserId })
                .ToListAsync();
            
            var formUserIdsByWorkflow = allFormItems
                .Where(fi => workflowItemToHeadDict.ContainsKey(fi.WorkflowItemId))
                .GroupBy(fi => workflowItemToHeadDict[fi.WorkflowItemId])
                .Select(g => new { 
                    WorkflowHeadId = g.Key, 
                    FormUserIds = g.Select(x => x.FormUserId).Distinct().ToList() 
                })
                .ToList();

            // Dictionary'ye çevir (hızlı lookup için)
            var pendingApprovalsDict = pendingApprovals.ToDictionary(x => x.WorkflowHeadId, x => x.Count);
            var pendingFormsDict = pendingForms.ToDictionary(x => x.WorkflowHeadId, x => x.Count);
            var formUserIdsDict = formUserIdsByWorkflow.ToDictionary(x => x.WorkflowHeadId, x => x.FormUserIds);

            // DTO'ya map et (FormData performans için çıkartıldı - detay endpoint'inden alınmalı)
            var items = workflowHeads.Select(w => new ProcessHubItemDto
            {
                Id = w.Id,
                WorkflowDefinationId = w.WorkFlowDefinationId,
                WorkflowName = w.WorkflowName,
                CurrentNodeId = w.CurrentNodeId,
                CurrentNodeName = w.CurrentNodeName,
                WorkFlowStatus = w.workFlowStatus?.ToString() ?? "NotStarted",
                WorkFlowStatusText = GetWorkflowStatusText(w.workFlowStatus),
                WorkFlowInfo = w.WorkFlowInfo,
                CreateUser = w.CreateUser,
                CreatedDate = w.CreatedDate,
                UpdatedDate = w.UpdatedDate,
                FormId = w.FormInstance?.FormId,
                FormName = w.FormInstance?.Form?.FormName,
                FormData = null, // Performans için liste görünümünde göndermiyoruz
                PendingApprovalCount = pendingApprovalsDict.GetValueOrDefault(w.Id, 0),
                PendingFormCount = pendingFormsDict.GetValueOrDefault(w.Id, 0),
                FormUserIds = formUserIdsDict.GetValueOrDefault(w.Id, new List<string>())
            }).ToList();

            // Response oluştur
            var response = new ProcessHubDataResponseDto
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(response);
        }

        /// <summary>
        /// Tek bir workflow kaydının detayını getirir (FormData ile birlikte)
        /// </summary>
        /// <param name="workflowHeadId">Workflow Head ID</param>
        /// <returns>Workflow detay bilgisi</returns>
        [HttpGet("detail/{workflowHeadId}")]
        public async Task<ActionResult<ProcessHubItemDto>> GetDetail(Guid workflowHeadId)
        {
            // Kullanıcı bilgisini al
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized("Kullanıcı bilgisi alınamadı");
            }

            // Kullanıcı ID'sini al (Approvals için gerekli)
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("Kullanıcı ID'si alınamadı");
            }

            // Workflow'u bul
            var workflow = await _workflowRepository
                .Where(w => w.Id == workflowHeadId)
                .Include(w => w.WorkFlowDefination)
                .Include(w => w.FormInstance)
                .ThenInclude(fi => fi.Form)
                .FirstOrDefaultAsync();

            if (workflow == null)
            {
                return NotFound($"Workflow bulunamadı: {workflowHeadId}");
            }

            // Pending item sayılarını al (USER ID ile)
            // İki aşamalı sorgu: önce WorkflowItem'ları al, sonra ApproveItems/FormItems
            
            // 1. Bu WorkflowHead'e ait WorkflowItem ID'lerini al
            var workflowItemIds = await _workflowItemRepository
                .Where(wi => wi.WorkflowHeadId == workflowHeadId)
                .Select(wi => wi.Id)
                .ToListAsync();

            // 2. ApproveItems sayısını al
            var pendingApprovalCount = await _approveItemsRepository
                .Where(ai => 
                    ai.ApproveUserId == currentUserId &&
                    ai.ApproverStatus == ApproverStatus.Pending &&
                    workflowItemIds.Contains(ai.WorkflowItemId))
                .CountAsync();

            // 3. FormItems sayısını al
            var pendingFormCount = await _formItemsRepository
                .Where(fi => 
                    fi.FormUserId == currentUserId &&
                    fi.FormItemStatus == FormItemStatus.Pending &&
                    workflowItemIds.Contains(fi.WorkflowItemId))
                .CountAsync();

            // 4. Bu workflow'daki pending FormUserIds'leri al
            var formUserIds = await _formItemsRepository
                .Where(fi => 
                    fi.FormItemStatus == FormItemStatus.Pending &&
                    workflowItemIds.Contains(fi.WorkflowItemId))
                .Select(fi => fi.FormUserId)
                .Distinct()
                .ToListAsync();

            // DTO'ya map et (FormData dahil)
            var item = new ProcessHubItemDto
            {
                Id = workflow.Id,
                WorkflowDefinationId = workflow.WorkFlowDefinationId,
                WorkflowName = workflow.WorkflowName,
                CurrentNodeId = workflow.CurrentNodeId,
                CurrentNodeName = workflow.CurrentNodeName,
                WorkFlowStatus = workflow.workFlowStatus?.ToString() ?? "NotStarted",
                WorkFlowStatusText = GetWorkflowStatusText(workflow.workFlowStatus),
                WorkFlowInfo = workflow.WorkFlowInfo,
                CreateUser = workflow.CreateUser,
                CreatedDate = workflow.CreatedDate,
                UpdatedDate = workflow.UpdatedDate,
                FormId = workflow.FormInstance?.FormId,
                FormName = workflow.FormInstance?.Form?.FormName,
                FormData = workflow.FormInstance?.FormData, // Detay'da FormData'yı gönderiyoruz
                PendingApprovalCount = pendingApprovalCount,
                PendingFormCount = pendingFormCount,
                FormUserIds = formUserIds
            };

            return Ok(item);
        }

        /// <summary>
        /// Onay akışı history'sini döndürür - sadece formNode ve formTaskNode tipleri
        /// WorkflowItem bazında history, her item için FormItems'dan kimde beklediği (FormUser) bilgisi
        /// </summary>
        /// <param name="workflowHeadId">Workflow Head ID</param>
        /// <returns>WorkflowHistoryResponseDto</returns>
        [HttpGet("history/{workflowHeadId}")]
        public async Task<ActionResult<WorkflowHistoryResponseDto>> GetHistory(Guid workflowHeadId)
        {
            // 1. WorkflowHead'i al (tek sorgu)
            var workflowHead = await _workflowRepository
                .Where(w => w.Id == workflowHeadId)
                .Select(w => new
                {
                    w.Id,
                    w.WorkflowName,
                    w.WorkFlowDefinationId,
                    w.CreateUser,
                    w.CreatedDate,
                    w.CurrentNodeId,
                    w.CurrentNodeName,
                    w.workFlowStatus,
                    w.WorkFlowInfo,
                    FormId = w.FormInstance != null ? w.FormInstance.FormId : (Guid?)null,
                    FormName = w.FormInstance != null && w.FormInstance.Form != null ? w.FormInstance.Form.FormName : (string)null,
                    FormData = w.FormInstance != null ? w.FormInstance.FormData : (string)null
                })
                .FirstOrDefaultAsync();

            if (workflowHead == null)
                return NotFound($"Workflow bulunamadı: {workflowHeadId}");

            // 2. Sadece formNode ve formTaskNode tiplerindeki WorkflowItem'ları al (tek sorgu)
            var workflowItems = await _workflowItemRepository
                .Where(wi => wi.WorkflowHeadId == workflowHeadId &&
                    (wi.NodeType == "formNode" || wi.NodeType == "formTaskNode"))
                .OrderBy(wi => wi.CreatedDate)
                .Select(wi => new
                {
                    wi.Id,
                    wi.WorkflowHeadId,
                    wi.NodeId,
                    wi.NodeName,
                    wi.NodeDescription,
                    wi.NodeType,
                    wi.workFlowNodeStatus,
                    wi.CreatedDate,
                    wi.UpdatedDate
                })
                .ToListAsync();

            if (workflowItems.Count == 0)
            {
                return Ok(new WorkflowHistoryResponseDto
                {
                    HeadInfo = new WorkflowHeadInfoDto
                    {
                        Id = workflowHead.Id,
                        WorkflowName = workflowHead.WorkflowName,
                        WorkflowDefinationId = workflowHead.WorkFlowDefinationId,
                        StartedBy = workflowHead.CreateUser,
                        StartedDate = workflowHead.CreatedDate,
                        CurrentNodeId = workflowHead.CurrentNodeId,
                        CurrentNodeName = workflowHead.CurrentNodeName,
                        WorkFlowStatus = workflowHead.workFlowStatus?.ToString() ?? "NotStarted",
                        WorkFlowStatusText = GetWorkflowStatusText(workflowHead.workFlowStatus),
                        WorkFlowInfo = workflowHead.WorkFlowInfo,
                        FormId = workflowHead.FormId,
                        FormName = workflowHead.FormName,
                        FormData = workflowHead.FormData
                    },
                    HistoryItems = new List<WorkflowItemHistoryDto>(),
                    TotalItemCount = 0
                });
            }

            var workflowItemIds = workflowItems.Select(wi => wi.Id).ToList();

            // 3. Tüm FormItems'ları tek sorguda al (önemli: FormUserId ve FormUserNameSurname - kimde bekliyor)
            var formItems = await _formItemsRepository
                .Where(fi => workflowItemIds.Contains(fi.WorkflowItemId))
                .Select(fi => new
                {
                    fi.WorkflowItemId,
                    fi.FormUserId,
                    fi.FormUserNameSurname,
                    fi.FormItemStatus
                })
                .ToListAsync();

            // 4. Tüm Pending FormUserId'leri topla ve UserOrgInfoService ile Ad Soyad, Departman, Pozisyon al
            var allPendingUserIds = formItems
                .Where(fi => fi.FormItemStatus == FormItemStatus.Pending)
                .Select(fi => fi.FormUserId)
                .Distinct()
                .ToList();

            var userOrgInfos = allPendingUserIds.Count > 0
                ? await _userOrgInfoService.GetUserOrgInfosBatchAsync(allPendingUserIds)
                : new Dictionary<string, formneo.core.DTOs.UserOrgInfoDto>();

            // 5. WorkflowItemHistoryDto listesi oluştur
            var historyItems = workflowItems.Select(wi =>
            {
                // Pending FormItems = kimde bekliyor (FormUser)
                var pendingFormItems = formItems
                    .Where(fi => fi.WorkflowItemId == wi.Id && fi.FormItemStatus == FormItemStatus.Pending)
                    .ToList();

                var pendingUsers = pendingFormItems
                    .Select(fi =>
                    {
                        var orgInfo = userOrgInfos.GetValueOrDefault(fi.FormUserId);
                        return new PendingUserDto
                        {
                            UserId = fi.FormUserId,
                            UserName = orgInfo?.UserName ?? fi.FormUserNameSurname ?? fi.FormUserId,
                            Department = orgInfo?.Department,
                            DepartmentId = orgInfo?.DepartmentId,
                            Position = orgInfo?.Position,
                            PositionId = orgInfo?.PositionId
                        };
                    })
                    .GroupBy(u => u.UserId)
                    .Select(g => g.First())
                    .ToList();

                return new WorkflowItemHistoryDto
                {
                    Id = wi.Id,
                    WorkflowHeadId = wi.WorkflowHeadId,
                    WorkflowNodeId = wi.NodeId,
                    NodeName = wi.NodeName,
                    NodeDescription = wi.NodeDescription,
                    NodeType = wi.NodeType,
                    NodeStatus = wi.workFlowNodeStatus.ToString(),
                    NodeStatusText = GetWorkflowNodeStatusText(wi.workFlowNodeStatus),
                    PendingWithUsers = pendingUsers,
                    PendingUserCount = pendingUsers.Count,
                    CreatedDate = wi.CreatedDate,
                    UpdatedDate = wi.UpdatedDate
                };
            }).ToList();

            var response = new WorkflowHistoryResponseDto
            {
                HeadInfo = new WorkflowHeadInfoDto
                {
                    Id = workflowHead.Id,
                    WorkflowName = workflowHead.WorkflowName,
                    WorkflowDefinationId = workflowHead.WorkFlowDefinationId,
                    StartedBy = workflowHead.CreateUser,
                    StartedDate = workflowHead.CreatedDate,
                    CurrentNodeId = workflowHead.CurrentNodeId,
                    CurrentNodeName = workflowHead.CurrentNodeName,
                    WorkFlowStatus = workflowHead.workFlowStatus?.ToString() ?? "NotStarted",
                    WorkFlowStatusText = GetWorkflowStatusText(workflowHead.workFlowStatus),
                    WorkFlowInfo = workflowHead.WorkFlowInfo,
                    FormId = workflowHead.FormId,
                    FormName = workflowHead.FormName,
                    FormData = workflowHead.FormData
                },
                HistoryItems = historyItems,
                TotalItemCount = historyItems.Count
            };

            return Ok(response);
        }

        /// <summary>
        /// Workflow status'ü Türkçe açıklamaya çevirir
        /// </summary>
        private string GetWorkflowStatusText(WorkflowStatus? status)
        {
            return status switch
            {
                WorkflowStatus.NotStarted => "Başlamadı",
                WorkflowStatus.InProgress => "Devam Ediyor",
                WorkflowStatus.Completed => "Tamamlandı",
                WorkflowStatus.Pending => "Beklemede",
                WorkflowStatus.SendBack => "Geri Gönderildi",
                _ => "Bilinmiyor"
            };
        }

        /// <summary>
        /// Workflow node status'ü Türkçe açıklamaya çevirir
        /// </summary>
        private string GetWorkflowNodeStatusText(WorkflowStatus status)
        {
            return status switch
            {
                WorkflowStatus.NotStarted => "Başlamadı",
                WorkflowStatus.InProgress => "Devam Ediyor",
                WorkflowStatus.Completed => "Tamamlandı",
                WorkflowStatus.Pending => "Beklemede",
                WorkflowStatus.SendBack => "Geri Gönderildi",
                _ => "Bilinmiyor"
            };
        }
    }
}
