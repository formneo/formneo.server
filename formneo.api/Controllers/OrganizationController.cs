using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph.Models;
using Microsoft.OpenApi.Validations;
using NLayer.Core.Services;
using System.Runtime.ConstrainedExecution;
using formneo.core.DTOs;
using formneo.core.DTOs.DepartmentUserDto;
using formneo.core.DTOs.OrgUnits;
using formneo.core.Models;
using formneo.core.Services;

namespace formneo.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrganizationController : CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly IServiceWithDto<OrgUnit, OrgUnitListDto> _orgUnits;
        private readonly UserManager<UserApp> _userManager;
        public OrganizationController(IMapper mapper, IServiceWithDto<OrgUnit, OrgUnitListDto> orgUnits, UserManager<UserApp> userManager)
        {
            _mapper = mapper;
            _orgUnits = orgUnits;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<OrganizationDto>> GetAll()
        {
            try
            {
                var service = await _orgUnits.Include();
                var orgUnits = service
                    .Include(e => e.Manager)
                    .ToList();

                OrganizationDto result = new()
                {
                    Id = "root",
                    Name = "Organization",
                    Expanded = true,
                    Type = "orgunit",
                    Children = new List<OrganizationDto>()
                };

                var rootUnits = orgUnits.Where(e => e.ParentOrgUnitId == null).ToList();
                foreach (var unit in rootUnits)
                {
                    var unitDto = new OrganizationDto
                    {
                        Id = unit.Id.ToString(),
                        Name = unit.Name,
                        Expanded = true,
                        Type = "orgunit",
                        Children = new List<OrganizationDto>()
                    };
                    result.Children.Add(unitDto);
                    await Recursive(unit.Id, result, orgUnits);
                }

                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }

        }
        private async Task<OrganizationDto> Recursive(Guid parentOrgUnitId, OrganizationDto result, List<OrgUnit> orgUnits)
        {
            try
            {
                // Üst birimi parentOrgUnitId olan tüm birimleri al
                var childUnits = orgUnits
                       .Where(e => e.ParentOrgUnitId == parentOrgUnitId)
                       .Select(e => new OrganizationDto
                       {
                           Id = e.Id.ToString(),
                           Name = e.Name,
                           Expanded = true,
                           Type = "orgunit",
                           Children = new List<OrganizationDto>()
                       }).ToList();

                // Birim yöneticisini bul ve child olarak ekle
                var parentUnitDto = FindById(result, parentOrgUnitId.ToString());
                var parentUnit = orgUnits.FirstOrDefault(e => e.Id == parentOrgUnitId);
                var managerDto = parentUnit == null ? null : new OrganizationDto
                {
                    Id = parentUnit.ManagerId,
                    Name = $"{parentUnit.Manager?.FirstName} {parentUnit.Manager?.LastName}".Trim(),
                    // NOT: Position artık EmployeeAssignment tablosunda tutuluyor
                    Title = null,
                    Email = parentUnit.Manager?.Email,
                    Photo = parentUnit.Manager?.photo,
                    Expanded = true,
                    ClassName = "manager-node",
                    Children = new List<OrganizationDto>()
                };

                if (parentUnitDto != null)
                {
                    if (parentUnitDto.Children == null)
                        parentUnitDto.Children = new List<OrganizationDto>();

                    if (managerDto != null)
                        parentUnitDto.Children.Add(managerDto);
                }

                // Eğer alt birim yoksa
                if (!childUnits.Any())
                {
                    if (parentUnitDto != null)
                    {
                        var attachNode = managerDto ?? parentUnitDto;
                        if (attachNode.Children == null)
                            attachNode.Children = new List<OrganizationDto>();

                        // NOT: OrgUnitId kaldırıldı, EmployeeAssignment tablosu kullanılmalı
                        // Şimdilik boş bırakılıyor - EmployeeAssignment ile join yapılmalı
                        // var depUsers = await _userManager.Users
                        //  .Where(e => e.OrgUnitId == parentOrgUnitId && (managerDto == null || e.Id != managerDto.Id))
                        //  .ToListAsync();
                        // foreach (var user in depUsers)
                        // {
                        //     var userDto = new OrganizationDto
                        //     {
                        //         Id = user.Id.ToString(),
                        //         Name = $"{user.FirstName} {user.LastName}".Trim(),
                        //         Title = null, // EmployeeAssignment'dan alınmalı
                        //         Email = user.Email,
                        //         Photo = user.photo,
                        //         ClassName = user.Email == "murat.merdogan@formneo.com" ? "ceo-node" :
                        //                    (user.Email == "murat.merdogan@formneo.com" || user.Email == "murat.merdogan@formneo.com") ? "executive-node" :
                        //                    "employee-node",
                        //         Children = new List<OrganizationDto>()
                        //     };
                        //     attachNode.Children.Add(userDto);
                        // }
                    }
                }
                else
                {
                    foreach (var item in childUnits)
                    {
                        // Alt birimi ekle
                        if (managerDto != null)
                        {
                            if (managerDto.Children == null)
                                managerDto.Children = new List<OrganizationDto>();
                            managerDto.Children.Add(item);
                        }
                        else if (parentUnitDto != null)
                        {
                            if (parentUnitDto.Children == null)
                                parentUnitDto.Children = new List<OrganizationDto>();
                            parentUnitDto.Children.Add(item);
                        }
                        await Recursive(Guid.Parse(item.Id), result, orgUnits);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
          
        }
        private static OrganizationDto? FindById(OrganizationDto node, string id)
        {
            if (node.Id == id)
                return node;

            if (node.Children != null && node.Children.Any())
            {
                foreach (var child in node.Children)
                {
                    var found = FindById(child, id);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }


        [HttpGet("GetByDepartment")]
        [HttpGet("GetByOrgUnit")]
        public async Task<ActionResult<OrganizationDto>> GetByDepartment([FromQuery] Guid? orgUnitId, [FromQuery(Name = "dptId")] Guid? dptId)
        {
            try
            {
                var targetId = orgUnitId ?? dptId;
                if (targetId == null)
                    return BadRequest("Org unit id is required.");

                // Tüm org birimlerini yükle
                var service = await _orgUnits.Include();
                var orgUnits = service
                    .Include(e => e.Manager)
                    .ToList();

                // Başlangıç birimini bul
                var rootUnit = orgUnits.Where(d => d.Id == targetId).FirstOrDefault();
                if (rootUnit == null)
                    return NotFound("Org unit not found");

                // Root birim DTO'su oluştur
                var rootDto = new OrganizationDto
                {
                    Id = rootUnit.Id.ToString(),
                    Name = rootUnit.Name,
                    Expanded = true,
                    Type = "orgunit",
                    Children = new List<OrganizationDto>()
                };

                // Hiyerarşiyi oluştur
                var result = await Recursive(rootUnit.Id, rootDto, orgUnits);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }
    }
}
