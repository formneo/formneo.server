using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using NLayer.Core.Services;
using formneo.core.DTOs;
using formneo.core.DTOs.PositionsDtos;
using formneo.core.DTOs.Ticket.TicketRuleEngine;
using formneo.core.DTOs.Ticket.Tickets;
using formneo.core.Models;
using formneo.core.Models.Ticket;
using formneo.core.Services;



namespace formneo.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PositionsController : CustomBaseController
    {
        //private readonly IPositionService _positionService;
        private readonly IServiceWithDto<Positions, PositionListDto> _positionsService;
        private readonly IMapper _mapper;

        public PositionsController(IServiceWithDto<Positions, PositionListDto> positionsService, IMapper mapper)
        {
            _positionsService = positionsService;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<ActionResult<List<PositionListDto>>> GetPositionAllList()
        {
            var service = await _positionsService.Include();
            var list = service            
             .Select(e => new PositionListDto
             {
                 Id = e.Id,
                 Name = e.Name,
                 Description = e.Description,
                 ParentPositionId = e.ParentPositionId
             }).ToList();

            return Ok(_mapper.Map<List<PositionListDto >>(list));
        }

        [HttpGet("GetPositionsByCompany")]
        public async Task<ActionResult<List<PositionListDto>>> GetPositionsByCompany([FromQuery] Guid id)
        {
            var service = await _positionsService.Include();
            var list = service
                .Where(e=>e.MainClientId == id)
             .Select(e => new PositionListDto
             {
                 Id = e.Id,
                 Name = e.Name,
                 Description = e.Description,
                 ParentPositionId = e.ParentPositionId
             }).ToList();

            return Ok(_mapper.Map<List<PositionListDto>>(list));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionListDto>> GetPositionById(Guid id)
        {
            var service = await _positionsService.Include();
            var pos = service
              .Where(e => e.Id == id)
             .Select(e => new PositionListDto
             {
                 Id = e.Id,
                 Name = e.Name,
                 Description = e.Description,
                 ParentPositionId = e.ParentPositionId
             }).FirstOrDefault();

            return Ok(_mapper.Map<PositionListDto>(pos));         
        }

        [HttpPost]
        public async Task<IActionResult> CreatePosition(CreatePositionDto dto)
        {
            try
            {
                await _positionsService.AddAsync(_mapper.Map<PositionListDto>(dto));

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));

            }
            catch (Exception ex)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ex.Message));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePosition(UpdatePositionDto dto)
        {
            try
            {
                var positions = await _positionsService.Include();
                var exist = await positions.Where(e => e.Id ==dto.Id).FirstOrDefaultAsync();

                if (exist == null)
                {
                    return NotFound("Position not found.");
                }

                exist.Name = dto.Name;
                exist.Description = dto.Description;
                exist.ParentPositionId = dto.ParentPositionId;
                await _positionsService.UpdateAsync(_mapper.Map<PositionListDto>(exist));

                return Ok("Updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating position: {ex.Message}");
            }         
        }
        [HttpDelete]
        public async Task<IActionResult> RemovePosition(Guid id)
        {
            try
            {
                var existPos = await _positionsService.GetByIdGuidAsync(id);

                if (existPos == null)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Pozisyon bilgisi bulunamadı"));
                }

                await _positionsService.RemoveAsyncByGuid(id);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch (Exception ex)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ex.Message));
            }          
        }
    }
}
