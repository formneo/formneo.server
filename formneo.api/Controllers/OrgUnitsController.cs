using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLayer.Core.Services;
using formneo.core.DTOs;
using formneo.core.DTOs.OrgUnits;
using formneo.core.Models;

namespace formneo.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrgUnitsController : CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly IServiceWithDto<OrgUnit, OrgUnitListDto> _orgUnits;

        public OrgUnitsController(IMapper mapper, IServiceWithDto<OrgUnit, OrgUnitListDto> orgUnits)
        {
            _mapper = mapper;
            _orgUnits = orgUnits;
        }

        [HttpGet]
        public async Task<List<OrgUnitListDto>> All()
        {
            var orgUnits = await _orgUnits.Include();
            orgUnits = orgUnits.Include(x => x.SubOrgUnits).Include(x => x.Manager);
            orgUnits = orgUnits
                .OrderBy(e => e.Name)
                .Select(e => new OrgUnit
                {
                    Id = e.Id,
                    Name = e.Name,
                    Code = e.Code,
                    Type = e.Type,
                    IsActive = e.IsActive,
                    ManagerId = e.ManagerId,
                    ParentOrgUnitId = e.ParentOrgUnitId,
                    SubOrgUnits = e.SubOrgUnits.Where(y => y.ParentOrgUnitId == e.Id).Select(y => new OrgUnit
                    {
                        Id = y.Id,
                        Name = y.Name,
                        Code = y.Code,
                        Type = y.Type
                    }).ToList(),
                    Manager = e.Manager != null ? new UserApp
                    {
                        Id = e.Manager.Id,
                        FirstName = e.Manager.FirstName,
                        LastName = e.Manager.LastName
                    } : null
                });

            return _mapper.Map<List<OrgUnitListDto>>(orgUnits);
        }

        [HttpPost]
        public async Task<IActionResult> Save(OrgUnitInsertDto dto)
        {
            try
            {
                await _orgUnits.AddAsync(_mapper.Map<OrgUnitListDto>(dto));
                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch (Exception ex)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ex.Message));
            }
        }

        [HttpPut]
        public async Task<ActionResult<OrgUnitUpdateDto>> Update(OrgUnitUpdateDto dto)
        {
            var orgUnits = await _orgUnits.Include();
            var existing = await orgUnits.Where(e => e.Id == dto.Id).FirstOrDefaultAsync();

            if (existing == null)
            {
                return NotFound("Org unit not found.");
            }

            existing.Name = dto.Name;
            existing.Code = dto.Code;
            existing.Type = dto.Type;
            existing.IsActive = dto.IsActive;
            existing.ManagerId = dto.ManagerId;
            existing.ParentOrgUnitId = dto.ParentOrgUnitId;

            await _orgUnits.UpdateAsync(_mapper.Map<OrgUnitListDto>(existing));

            return dto;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var orgUnit = await _orgUnits.GetByIdGuidAsync(id);

                if (orgUnit == null)
                {
                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Org unit not found"));
                }

                await _orgUnits.RemoveAsyncByGuid(id);

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch (Exception ex)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrgUnitListDto>> GetById(Guid id)
        {
            try
            {
                var orgUnits = await _orgUnits.Include();
                var orgUnit = await orgUnits
                    .Include(e => e.Manager)
                    .Where(e => e.Id == id)
                    .FirstOrDefaultAsync();

                if (orgUnit == null)
                {
                    return null;
                }

                return _mapper.Map<OrgUnitListDto>(orgUnit);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

