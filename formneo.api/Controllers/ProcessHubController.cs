using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using formneo.core.DTOs;
using formneo.core.DTOs.ProcessHub;
using formneo.core.Models;
using formneo.core.Repositories;
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

        public ProcessHubController(
            IMapper mapper,
            IWorkflowRepository workflowRepository,
            IWorkFlowItemRepository workflowItemRepository,
            IApproveItemsRepository approveItemsRepository,
            IFormItemsRepository formItemsRepository)
        {
            _mapper = mapper;
            _workflowRepository = workflowRepository;
            _workflowItemRepository = workflowItemRepository;
            _approveItemsRepository = approveItemsRepository;
            _formItemsRepository = formItemsRepository;
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
                    // Kullanıcının onaylaması gereken kayıtlar
                    // ApproveItems tablosundan kullanıcının pending onaylarını bul
                    var workflowIdsWithPendingApprovals = await _approveItemsRepository
                        .Where(ai => 
                            ai.ApproveUserId == currentUser && 
                            ai.ApproverStatus == ApproverStatus.Pending)
                        .Select(ai => ai.WorkflowItem.WorkflowHeadId)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(w => workflowIdsWithPendingApprovals.Contains(w.Id));
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

            // Her workflow için pending item sayılarını tek sorguda al (performans)
            var pendingApprovals = await _approveItemsRepository
                .Where(ai => 
                    workflowHeadIds.Contains(ai.WorkflowItem.WorkflowHeadId) &&
                    ai.ApproveUserId == currentUser &&
                    ai.ApproverStatus == ApproverStatus.Pending)
                .GroupBy(ai => ai.WorkflowItem.WorkflowHeadId)
                .Select(g => new { WorkflowHeadId = g.Key, Count = g.Count() })
                .ToListAsync();

            var pendingForms = await _formItemsRepository
                .Where(fi => 
                    workflowHeadIds.Contains(fi.WorkflowItem.WorkflowHeadId) &&
                    fi.FormUserId == currentUser &&
                    fi.FormItemStatus == FormItemStatus.Pending)
                .GroupBy(fi => fi.WorkflowItem.WorkflowHeadId)
                .Select(g => new { WorkflowHeadId = g.Key, Count = g.Count() })
                .ToListAsync();

            // Dictionary'ye çevir (hızlı lookup için)
            var pendingApprovalsDict = pendingApprovals.ToDictionary(x => x.WorkflowHeadId, x => x.Count);
            var pendingFormsDict = pendingForms.ToDictionary(x => x.WorkflowHeadId, x => x.Count);

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
                PendingFormCount = pendingFormsDict.GetValueOrDefault(w.Id, 0)
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

            // Pending item sayılarını al
            var pendingApprovalCount = await _approveItemsRepository
                .Where(ai => 
                    ai.WorkflowItem.WorkflowHeadId == workflowHeadId &&
                    ai.ApproveUserId == currentUser &&
                    ai.ApproverStatus == ApproverStatus.Pending)
                .CountAsync();

            var pendingFormCount = await _formItemsRepository
                .Where(fi => 
                    fi.WorkflowItem.WorkflowHeadId == workflowHeadId &&
                    fi.FormUserId == currentUser &&
                    fi.FormItemStatus == FormItemStatus.Pending)
                .CountAsync();

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
                PendingFormCount = pendingFormCount
            };

            return Ok(item);
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
    }
}
