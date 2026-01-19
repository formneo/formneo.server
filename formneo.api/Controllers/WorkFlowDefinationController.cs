using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using NLayer.Core.Services;
using NLayer.Service.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Operations;
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

        public WorkFlowDefinationController(IMapper mapper, IWorkFlowDefinationService workFlowService)
        {
            _mapper = mapper;
            _service = workFlowService;
        }
        [HttpGet]
        public async Task<List<WorkFlowDefinationListDto>> All()
        {
            var definations = await _service.GetAllAsync();
            var dto = _mapper.Map<List<WorkFlowDefinationListDto>>(definations);
            return dto;
        }

        [HttpGet("{id}")]
        public async Task<WorkFlowDefinationListDto> GetById(string id)
        {


            var workFlowDefination = await _service.GetByIdStringGuidAsync(new Guid(id));
            var workFlowDefinationDto = _mapper.Map<WorkFlowDefinationListDto>(workFlowDefination);
            return workFlowDefinationDto;
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
