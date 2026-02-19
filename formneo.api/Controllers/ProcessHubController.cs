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

            List<ProcessHubItemDto> items;
            int totalCount;

            if (parsedViewType == ProcessViewType.Approvals)
            {
                // Approvals: WorkflowItem bazında dönüş - her satır = kullanıcının onay/form doldurması gereken bir WorkflowItem
                var approvalWorkflowItemIds = await _approveItemsRepository
                    .Where(ai => ai.ApproveUserId == currentUserId && ai.ApproverStatus == ApproverStatus.Pending)
                    .Select(ai => ai.WorkflowItemId)
                    .Distinct()
                    .ToListAsync();

                var formWorkflowItemIds = await _formItemsRepository
                    .Where(fi => fi.FormUserId == currentUserId && fi.FormItemStatus == FormItemStatus.Pending)
                    .Select(fi => fi.WorkflowItemId)
                    .Distinct()
                    .ToListAsync();

                var combinedWorkflowItemIds = approvalWorkflowItemIds.Union(formWorkflowItemIds).Distinct().ToList();
                var approvalItemIds = approvalWorkflowItemIds.ToHashSet();
                var formItemIds = formWorkflowItemIds.ToHashSet();

                if (combinedWorkflowItemIds.Count == 0)
                {
                    items = new List<ProcessHubItemDto>();
                    totalCount = 0;
                }
                else
                {
                    var workflowItemsQuery = _workflowItemRepository
                        .Where(wi => combinedWorkflowItemIds.Contains(wi.Id))
                        .Include(wi => wi.WorkflowHead)
                            .ThenInclude(wh => wh.FormInstance)
                            .ThenInclude(fi => fi.Form)
                        .Include(wi => wi.WorkflowHead)
                            .ThenInclude(wh => wh.WorkFlowDefination)
                        .Where(wi => wi.WorkflowHead.WorkFlowDefinationId == workflowId);

                    totalCount = await workflowItemsQuery.CountAsync();

                    var workflowItems = await workflowItemsQuery
                        .OrderByDescending(wi => wi.CreatedDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    var pageWorkflowItemIds = workflowItems.Select(wi => wi.Id).ToList();
                    var formUserIdsByItem = (await _formItemsRepository
                        .Where(fi => fi.FormItemStatus == FormItemStatus.Pending && pageWorkflowItemIds.Contains(fi.WorkflowItemId))
                        .Select(fi => new { fi.WorkflowItemId, fi.FormUserId })
                        .ToListAsync())
                        .GroupBy(x => x.WorkflowItemId)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.FormUserId).Distinct().ToList());

                    items = workflowItems.Select(wi => new ProcessHubItemDto
                    {
                        Id = wi.WorkflowHeadId,
                        WorkflowItemId = wi.Id,
                        WorkflowDefinationId = wi.WorkflowHead.WorkFlowDefinationId,
                        WorkflowName = wi.WorkflowHead.WorkflowName,
                        CurrentNodeId = wi.NodeId,
                        CurrentNodeName = wi.NodeName,
                        WorkFlowStatus = wi.WorkflowHead.workFlowStatus?.ToString() ?? "NotStarted",
                        WorkFlowStatusText = GetWorkflowStatusText(wi.WorkflowHead.workFlowStatus),
                        WorkFlowInfo = wi.WorkflowHead.WorkFlowInfo,
                        CreateUser = wi.WorkflowHead.CreateUser,
                        CreatedDate = wi.WorkflowHead.CreatedDate,
                        UpdatedDate = wi.WorkflowHead.UpdatedDate,
                        FormId = wi.WorkflowHead.FormInstance?.FormId,
                        FormName = wi.WorkflowHead.FormInstance?.Form?.FormName,
                        FormData = null,
                        PendingApprovalCount = approvalItemIds.Contains(wi.Id) ? 1 : 0,
                        PendingFormCount = formItemIds.Contains(wi.Id) ? 1 : 0,
                        FormUserIds = formUserIdsByItem.GetValueOrDefault(wi.Id, new List<string>())
                    }).ToList();
                }
            }
            else
            {
                // My ve All: WorkflowHead bazında dönüş
                var query = _workflowRepository
                    .Where(w => w.WorkFlowDefinationId == workflowId)
                    .Include(w => w.WorkFlowDefination)
                    .Include(w => w.FormInstance)
                    .ThenInclude(fi => fi.Form)
                    .AsQueryable();

                if (parsedViewType == ProcessViewType.My)
                    query = query.Where(w => w.CreateUser == currentUser);

                totalCount = await query.CountAsync();
                var workflowHeads = await query
                    .OrderByDescending(w => w.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var workflowHeadIds = workflowHeads.Select(w => w.Id).ToList();

                if (workflowHeadIds.Count == 0)
                {
                    items = new List<ProcessHubItemDto>();
                }
                else
                {
                    var workflowItemMap = await _workflowItemRepository
                        .Where(wi => workflowHeadIds.Contains(wi.WorkflowHeadId))
                        .Select(wi => new { wi.Id, wi.WorkflowHeadId, wi.NodeId, wi.NodeName, wi.NodeType })
                        .ToListAsync();

                    var workflowItemToHead = workflowItemMap.ToDictionary(x => x.Id, x => x.WorkflowHeadId);
                    // My için: başlattığım startNode WorkflowItem Id'si (history'ye gerek yok, başlangıç node'u)
                    var startNodeByHead = workflowItemMap
                        .Where(wi => wi.NodeType == "startNode")
                        .GroupBy(wi => wi.WorkflowHeadId)
                        .ToDictionary(g => g.Key, g => g.First().Id);
                    var relevantIds = workflowItemToHead.Keys.ToList();

                    var approvalCounts = (await _approveItemsRepository
                        .Where(ai => ai.ApproverStatus == ApproverStatus.Pending && ai.ApproveUserId == currentUserId && relevantIds.Contains(ai.WorkflowItemId))
                        .Select(ai => ai.WorkflowItemId)
                        .ToListAsync())
                        .Where(id => workflowItemToHead.ContainsKey(id))
                        .GroupBy(id => workflowItemToHead[id])
                        .ToDictionary(g => g.Key, g => g.Count());

                    var formCounts = (await _formItemsRepository
                        .Where(fi => fi.FormItemStatus == FormItemStatus.Pending && fi.FormUserId == currentUserId && relevantIds.Contains(fi.WorkflowItemId))
                        .Select(fi => fi.WorkflowItemId)
                        .ToListAsync())
                        .Where(id => workflowItemToHead.ContainsKey(id))
                        .GroupBy(id => workflowItemToHead[id])
                        .ToDictionary(g => g.Key, g => g.Count());

                    var formUserIdsByHead = (await _formItemsRepository
                        .Where(fi => fi.FormItemStatus == FormItemStatus.Pending && relevantIds.Contains(fi.WorkflowItemId))
                        .Select(fi => new { fi.WorkflowItemId, fi.FormUserId })
                        .ToListAsync())
                        .Where(x => workflowItemToHead.ContainsKey(x.WorkflowItemId))
                        .GroupBy(x => workflowItemToHead[x.WorkflowItemId])
                        .ToDictionary(g => g.Key, g => g.Select(x => x.FormUserId).Distinct().ToList());

                    items = workflowHeads.Select(w => new ProcessHubItemDto
                    {
                        Id = w.Id,
                        WorkflowItemId = parsedViewType == ProcessViewType.My ? startNodeByHead.GetValueOrDefault(w.Id) : null,
                        WorkflowDefinationId = w.WorkFlowDefinationId,
                        WorkflowName = w.WorkflowName,
                        CurrentNodeId = null,
                        CurrentNodeName = null,
                        WorkFlowStatus = w.workFlowStatus?.ToString() ?? "NotStarted",
                        WorkFlowStatusText = GetWorkflowStatusText(w.workFlowStatus),
                        WorkFlowInfo = w.WorkFlowInfo,
                        CreateUser = w.CreateUser,
                        CreatedDate = w.CreatedDate,
                        UpdatedDate = w.UpdatedDate,
                        FormId = w.FormInstance?.FormId,
                        FormName = w.FormInstance?.Form?.FormName,
                        FormData = null,
                        PendingApprovalCount = approvalCounts.GetValueOrDefault(w.Id, 0),
                        PendingFormCount = formCounts.GetValueOrDefault(w.Id, 0),
                        FormUserIds = formUserIdsByHead.GetValueOrDefault(w.Id, new List<string>())
                    }).ToList();
                }
            }

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

            // 3. Tüm FormItems'ları tek sorguda al (Pending + Completed - kimde bekliyor / kim tamamladı)
            var formItems = await _formItemsRepository
                .Where(fi => workflowItemIds.Contains(fi.WorkflowItemId))
                .Select(fi => new
                {
                    fi.WorkflowItemId,
                    fi.FormUserId,
                    fi.FormUserNameSurname,
                    fi.FormItemStatus,
                    fi.UpdatedDate
                })
                .ToListAsync();

            // 4. Tüm Pending ve Completed FormUserId'leri topla, UserOrgInfoService ile Ad Soyad, Departman, Pozisyon al
            var allUserIds = formItems
                .Select(fi => fi.FormUserId)
                .Distinct()
                .ToList();

            var userOrgInfos = allUserIds.Count > 0
                ? await _userOrgInfoService.GetUserOrgInfosBatchAsync(allUserIds)
                : new Dictionary<string, formneo.core.DTOs.UserOrgInfoDto>();

            // 5. WorkflowItemHistoryDto listesi oluştur
            var historyItems = workflowItems.Select(wi =>
            {
                var pendingFormItems = formItems
                    .Where(fi => fi.WorkflowItemId == wi.Id && fi.FormItemStatus == FormItemStatus.Pending)
                    .ToList();

                var completedFormItems = formItems
                    .Where(fi => fi.WorkflowItemId == wi.Id && fi.FormItemStatus == FormItemStatus.Completed)
                    .ToList();

                string summary = null;
                DateTime? operationDate = null;

                if (completedFormItems.Count > 0)
                {
                    // Tamamlandı: "Mehmet Demir - 10.02.2025, Ahmet Yılmaz - 11.02.2025" (kullanıcı bazında en son tarih)
                    var byUser = completedFormItems
                        .GroupBy(fi => fi.FormUserId)
                        .Select(g =>
                        {
                            var fi = g.First();
                            var name = userOrgInfos.GetValueOrDefault(fi.FormUserId)?.UserName ?? fi.FormUserNameSurname ?? fi.FormUserId;
                            var latestDate = g.Max(x => x.UpdatedDate);
                            var dateStr = latestDate.HasValue ? latestDate.Value.ToString("dd.MM.yyyy") : "";
                            return string.IsNullOrEmpty(dateStr) ? name : $"{name} - {dateStr}";
                        })
                        .ToList();
                    summary = string.Join(", ", byUser);
                    operationDate = completedFormItems.Max(fi => fi.UpdatedDate);
                }
                else if (pendingFormItems.Count > 0)
                {
                    // Beklemede: "Ahmet Yılmaz, Mehmet Demir"
                    var names = pendingFormItems
                        .Select(fi => userOrgInfos.GetValueOrDefault(fi.FormUserId)?.UserName ?? fi.FormUserNameSurname ?? fi.FormUserId)
                        .Distinct()
                        .ToList();
                    summary = string.Join(", ", names);
                }

                var nodeStatusText = GetWorkflowNodeStatusText(wi.workFlowNodeStatus);
                var nodeDescription = string.IsNullOrEmpty(summary)
                    ? nodeStatusText
                    : $"{nodeStatusText}: {summary}";

                return new WorkflowItemHistoryDto
                {
                    Id = wi.Id,
                    WorkflowHeadId = wi.WorkflowHeadId,
                    WorkflowNodeId = wi.NodeId,
                    NodeName = wi.NodeName,
                    NodeDescription = nodeDescription,
                    NodeType = wi.NodeType,
                    NodeStatus = wi.workFlowNodeStatus.ToString(),
                    NodeStatusText = nodeStatusText,
                    Summary = summary,
                    OperationDate = operationDate,
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
                WorkflowStatus.Draft => "Taslak",
                WorkflowStatus.InProgress => "Başladı",
                WorkflowStatus.Completed => "Bitti",
                WorkflowStatus.Cancelled => "İptal Edildi",
                WorkflowStatus.Pending => "Beklemede", // WorkflowItem için
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
                WorkflowStatus.InProgress => "Devam Ediyor",
                WorkflowStatus.Completed => "Tamamlandı",
                WorkflowStatus.Pending => "Beklemede",
                WorkflowStatus.Draft => "Taslak",
                WorkflowStatus.Cancelled => "İptal",
                _ => "Bilinmiyor"
            };
        }
    }
}
