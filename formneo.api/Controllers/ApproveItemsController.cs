using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using NLayer.Core.Services;
using NLayer.Service.Services;
using System;
using formneo.core.DTOs;
using formneo.core.DTOs.Budget.JobCodeRequest;
using formneo.core.DTOs.Budget.NormCodeRequest;
using formneo.core.DTOs.Budget.PeriodUserDto;
using formneo.core.DTOs.Ticket.Tickets;
using formneo.core.Models;
using formneo.core.Models.BudgetManagement;
using formneo.core.Models.Ticket;
using formneo.core.Operations;
using formneo.core.Services;
using formneo.service.Services;
using formneo.workflow;
using WorkflowCore.Models;
using WorkflowCore.Services;

namespace formneo.api.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApproveItemsController : CustomBaseController
    {
            

        private readonly IMapper _mapper;
        private readonly IApproveItemsService _service;
        private readonly IServiceWithDto<BudgetJobCodeRequest, BudgetJobCodeRequestListDto> _jobCodeservice;
        private readonly IServiceWithDto<BudgetNormCodeRequest, BudgetNormCodeRequestListDto> _normCodeRequest;
        private readonly IServiceWithDto<Tickets, TicketListDto> _ticketGenericService;
        private readonly UserManager<UserApp> _userManager;

        public ApproveItemsController(IServiceWithDto<Tickets, TicketListDto> ticketGenericService, IMapper mapper, IApproveItemsService approveItemsService, IServiceWithDto<BudgetJobCodeRequest, BudgetJobCodeRequestListDto> jobcodeService, IServiceWithDto<BudgetNormCodeRequest, BudgetNormCodeRequestListDto> normCodeRequest, UserManager<UserApp> userManager)
        {

            _mapper = mapper;
            _service = approveItemsService;
            _jobCodeservice = jobcodeService;
            _userManager = userManager;
            _normCodeRequest = normCodeRequest;
            _ticketGenericService = ticketGenericService;
        }

        [HttpGet]
        public async Task<ApproveHeadInfo> All()
        {

            var headInfo = new ApproveHeadInfo();
            var items = await _service.GetAllRelationTable();
            headInfo.items = items;
            headInfo.ApproveCount = items.Where(e => e.ApproverStatus == ApproverStatus.Approve).Count();
            headInfo.RejectCount = items.Where(e => e.ApproverStatus == ApproverStatus.Reject).Count();
            headInfo.PendingCount = items.Where(e => e.ApproverStatus == ApproverStatus.Pending).Count();

            return headInfo;
        }

        [HttpGet]
        public async Task<ApproveItemsDtoResult> GetApproves([FromQuery] ApproverStatus type, int skip = 0, int top = 50,string WorkFlowDefinationId="",string createUser="")  
        {


            string username = User.Identity.Name;
            
            // UserName'i UserId'ye çevir
            var currentUser = await _userManager.FindByNameAsync(username);
            if (currentUser == null)
            {
                return new ApproveItemsDtoResult { ApproveItemsDtoList = new List<ApproveItemsDto>(), Count = 0 };
            }
            string userId = currentUser.Id;

            createUser = "";// User.Identity.Name;
            var items = await _service.GetAllRelationTable();


            if(WorkFlowDefinationId!="")
                items =  items.Where(e => e.workFlowItem.WorkflowHead.WorkFlowDefinationId == new Guid(WorkFlowDefinationId)).ToList();

            if (createUser != "")
                items = items.Where(e => e.workFlowItem.WorkflowHead.CreateUser == createUser).ToList();

            var dto = new List<ApproveItemsDto>();

            var _count = 0;
            if (type == ApproverStatus.Send)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.workFlowItem.WorkflowHead.CreateUser == username));
                dto = dto.Where(e => e.workFlowItem.WorkflowHead.workFlowStatus == core.Models.WorkflowStatus.InProgress).ToList();

                _count = dto.Count;
                items = dto.OrderByDescending(e => e.CreatedDate).Skip(skip).Take(top).ToList();
            }
            if (type == ApproverStatus.Pending)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.ApproverStatus == type && e.ApproveUser == username));
                dto = dto.Where(e => e.workFlowItem.WorkflowHead.workFlowStatus == core.Models.WorkflowStatus.InProgress).ToList();


                _count = dto.Count;
                items = dto.OrderByDescending(e => e.CreatedDate).Skip(skip).Take(top).ToList();

            }
            if (type == ApproverStatus.Approve)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.ApproverStatus == type && e.ApprovedUser_Runtime == username));



                _count = dto.Count;
                items = dto.OrderByDescending(e => e.CreatedDate).Skip(skip).Take(top).ToList();
            }
            if (type == ApproverStatus.Reject)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.ApproverStatus == type && e.ApprovedUser_Runtime == username));

                _count = dto.Count;
                items = dto.OrderByDescending(e => e.CreatedDate).Skip(skip).Take(top).ToList();

            }

            foreach (var item in dto)
            {
                item.ShortId = Utils.ShortenGuid(item.Id);
                item.ShortWorkflowItemId = Utils.ShortenGuid(new Guid(item.workFlowItem.WorkflowHead.id));

            }

            ApproveItemsDtoResult result = new ApproveItemsDtoResult();

            result.ApproveItemsDtoList = items.OrderByDescending(e => e.CreatedDate).ToList();
            result.Count = _count;

            return result;
        }


        [HttpGet("{userName}")]
        public async Task<List<ApproveItemsDto>> GetAdminApproves([FromQuery] ApproverStatus type)
        {
            var items = await _service.GetAllRelationTable();
            var dto = new List<ApproveItemsDto>();
            if (type == ApproverStatus.Send)
            {
                dto = dto.Where(e => e.workFlowItem.WorkflowHead.workFlowStatus == core.Models.WorkflowStatus.InProgress).ToList();
            }
            if (type == ApproverStatus.Pending)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.ApproverStatus == type));
                dto = dto.Where(e => e.workFlowItem.WorkflowHead.workFlowStatus == core.Models.WorkflowStatus.InProgress).ToList();
            }
            if (type == ApproverStatus.Approve)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.ApproverStatus == type ));
            }
            if (type == ApproverStatus.Reject)
            {
                dto = _mapper.Map<List<ApproveItemsDto>>(items.Where(e => e.ApproverStatus == type));
            }
            foreach (var item in dto)
            {
                item.ShortId = Utils.ShortenGuid(item.Id);
                item.ShortWorkflowItemId = Utils.ShortenGuid(new Guid(item.workFlowItem.WorkflowHead.id));
            }

            return dto;
        }

        [HttpGet("{userName}")]
        public async Task<List<ApproveItemsDto>> GetOpenApproves(string userName)
        {
            var approveItem = await _service.GetAllAsync();
            var dto = _mapper.Map<List<ApproveItemsDto>>(approveItem.Where(e => e.ApproverStatus == ApproverStatus.Pending));



            foreach (var item in dto)
            {
                item.ShortId = Utils.ShortenGuid(item.Id);
                //item.ShortWorkflowItemId = Utils.ShortenGuid(new Guid(item.workFlowItem.WorkflowHead.id));
                if (item.workFlowItem != null)
                {
                    if (item.workFlowItem.WorkflowHead != null)
                    {
                        item.ShortWorkflowItemId = Utils.ShortenGuid(new Guid(item.workFlowItem.WorkflowHead.id));
                    }
                    else
                    {
                        item.ShortWorkflowItemId = "";
                    }

                }

            }

            return dto;

        }

        [HttpGet("{id}")]
        public async Task<ApproveItemsDto> GetById(string id)
        {
            var approveItem = await _service.GetByIdStringAsync(id);
            var approveItemsDto = _mapper.Map<ApproveItemsDto>(approveItem);
            return approveItemsDto;
        }


        [HttpGet("GetOpenDetail")]
        public async Task<string> GetOpenDetail(string workFlowId)
        {
            var result = await _jobCodeservice.Find(e => e.WorkflowHeadId == new Guid(workFlowId));
            var normDetail = await _normCodeRequest.Find(e => e.WorkflowHeadId == new Guid(workFlowId));



            if (result.Data != null)
            {
                return "/PositionDetail?id=" + result.Data.Id;
            }
            else
            {

                if (normDetail.Data != null)
                    return "/NormDetail?id=" + normDetail.Data.Id;
                else
                    return "";
            }
            return "";


        }


        [HttpGet("GetTicketId")]
        public async Task<string> GetTicketId(string workflowHeadId)
        {
            var service = await _ticketGenericService.Include();
            var result = service.Where(e => e.WorkflowHeadId == new Guid(workflowHeadId)).FirstOrDefault();

            if (result != null)
                return result.Id.ToString();
            else
                return "";
        }

        [HttpGet("GetPendingCount")]
        public async Task<int> GetPendingCount()
        {
            var loginName = User.Identity.Name;
            
            // UserName'i UserId'ye çevir
            var currentUser = await _userManager.FindByNameAsync(loginName);
            if (currentUser == null)
            {
                return 0;
            }
            string userId = currentUser.Id;
            
            var approveItem = await _service.GetAllAsync();
            var dto = _mapper.Map<List<ApproveItemsDto>>(approveItem.Where(e => e.ApproverStatus == ApproverStatus.Pending && e.ApproveUserId == userId));  // ✅ UserId ile karşılaştır

            return dto.Count;

        }

    }
}
