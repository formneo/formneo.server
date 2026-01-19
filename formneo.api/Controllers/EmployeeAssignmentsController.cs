using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLayer.Core.Services;
using formneo.core.DTOs;
using formneo.core.DTOs.EmployeeAssignments;
using formneo.core.Helpers;
using formneo.core.Models;
using formneo.core.UnitOfWorks;
using formneo.repository.UnitOfWorks;

namespace formneo.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmployeeAssignmentsController : CustomBaseController
    {
        private readonly IMapper _mapper;
        private readonly IServiceWithDto<EmployeeAssignment, EmployeeAssignmentListDto> _employeeAssignments;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserApp> _userManager;

        public EmployeeAssignmentsController(
            IMapper mapper, 
            IServiceWithDto<EmployeeAssignment, EmployeeAssignmentListDto> employeeAssignments,
            IUnitOfWork unitOfWork,
            UserManager<UserApp> userManager)
        {
            _mapper = mapper;
            _employeeAssignments = employeeAssignments;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        /// <summary>
        /// Tüm atamaları getirir (tenant filtresi otomatik)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EmployeeAssignmentListDto>>> GetAll()
        {
            try
            {
                var assignments = await _employeeAssignments.Include();
                assignments = assignments
                    .Include(ea => ea.User)
                    .Include(ea => ea.OrgUnit)
                    .Include(ea => ea.Position)
                    .Include(ea => ea.Manager)
                    .OrderByDescending(ea => ea.StartDate);

                var result = await assignments.ToListAsync();
                return Ok(_mapper.Map<List<EmployeeAssignmentListDto>>(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir kullanıcının tüm atamalarını getirir
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<EmployeeAssignmentListDto>>> GetByUser(string userId)
        {
            try
            {
                var assignments = await _employeeAssignments.Where(ea => ea.UserId == userId);
                if (assignments.StatusCode < 200 || assignments.StatusCode >= 300)
                {
                    return BadRequest(assignments.Errors);
                }

                var result = assignments.Data.OrderByDescending(ea => ea.StartDate).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir kullanıcının aktif atamasını getirir
        /// </summary>
        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<EmployeeAssignmentListDto>> GetActiveByUser(string userId)
        {
            try
            {
                var assignmentsQuery = await _employeeAssignments.Include();
                var activeAssignment = await EmployeeAssignmentHelper
                    .GetActiveAssignmentAsync(assignmentsQuery, userId);

                if (activeAssignment == null)
                {
                    return NotFound("Active assignment not found");
                }

                return Ok(_mapper.Map<EmployeeAssignmentListDto>(activeAssignment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir ID'ye sahip atamayı getirir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeAssignmentListDto>> GetById(Guid id)
        {
            try
            {
                var assignment = await _employeeAssignments.GetByIdGuidAsync(id);
                if (assignment.StatusCode < 200 || assignment.StatusCode >= 300)
                {
                    return NotFound("Assignment not found");
                }

                return Ok(assignment.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Yeni atama oluşturur (eski aktif atamayı otomatik sonlandırır)
        /// 
        /// Atama Mantığı:
        /// 1. Validasyonlar: UserId, OrgUnitId, PositionId, ManagerId kontrolü
        /// 2. Eğer Primary assignment varsa, eski aktif atama otomatik sonlandırılır
        /// 3. ManagerId belirtilmemişse ve OrgUnitId varsa, OrgUnit'in ManagerId'si kullanılır
        /// 4. StartDate belirtilmemişse, şu anki tarih kullanılır
        /// 5. EndDate belirtilmemişse, atama aktif kalır (null)
        /// 6. EndDate, StartDate'den önce olamaz
        /// 7. Kullanıcı kendine manager olamaz
        /// 8. Transaction içinde çalışır (tutarlılık için)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeAssignmentInsertDto dto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var unitOfWork = _unitOfWork as UnitOfWork;
                if (unitOfWork == null)
                {
                    return StatusCode(500, "UnitOfWork context not available");
                }

                // ========== VALIDATION ==========
                
                // 1. UserId kontrolü
                if (string.IsNullOrEmpty(dto.UserId))
                {
                    _unitOfWork.Rollback();
                    return BadRequest("UserId is required");
                }

                var user = await _userManager.FindByIdAsync(dto.UserId);
                if (user == null)
                {
                    _unitOfWork.Rollback();
                    return NotFound($"User with Id '{dto.UserId}' not found");
                }

                // 2. OrgUnitId kontrolü (varsa)
                if (dto.OrgUnitId.HasValue)
                {
                    var orgUnit = await unitOfWork._context.OrgUnits
                        .FirstOrDefaultAsync(ou => ou.Id == dto.OrgUnitId.Value);
                    
                    if (orgUnit == null)
                    {
                        _unitOfWork.Rollback();
                        return NotFound($"OrgUnit with Id '{dto.OrgUnitId.Value}' not found");
                    }
                }

                // 3. PositionId kontrolü (varsa)
                if (dto.PositionId.HasValue)
                {
                    var position = await unitOfWork._context.Positions
                        .FirstOrDefaultAsync(p => p.Id == dto.PositionId.Value);
                    
                    if (position == null)
                    {
                        _unitOfWork.Rollback();
                        return NotFound($"Position with Id '{dto.PositionId.Value}' not found");
                    }
                }

                // 4. ManagerId kontrolü (varsa)
                if (!string.IsNullOrEmpty(dto.ManagerId))
                {
                    var manager = await _userManager.FindByIdAsync(dto.ManagerId);
                    if (manager == null)
                    {
                        _unitOfWork.Rollback();
                        return NotFound($"Manager with Id '{dto.ManagerId}' not found");
                    }

                    // Kullanıcı kendine manager olamaz
                    if (dto.ManagerId == dto.UserId)
                    {
                        _unitOfWork.Rollback();
                        return BadRequest("User cannot be their own manager");
                    }
                }

                // 5. Tarih validasyonları
                var startDate = dto.StartDate ?? DateTime.UtcNow;
                if (dto.EndDate.HasValue)
                {
                    if (dto.EndDate.Value < startDate)
                    {
                        _unitOfWork.Rollback();
                        return BadRequest("EndDate cannot be earlier than StartDate");
                    }
                }

                // ========== ESKİ AKTİF ATAMAYI SONLANDIR ==========
                
                // Eğer Primary assignment oluşturuluyorsa, eski aktif atamayı sonlandır
                if (dto.AssignmentType == AssignmentType.Primary)
                {
                    var assignmentsQuery = await _employeeAssignments.Include();
                    var activeAssignment = await EmployeeAssignmentHelper
                        .GetActiveAssignmentAsync(assignmentsQuery, dto.UserId);

                    if (activeAssignment != null)
                    {
                        var trackedAssignment = await unitOfWork._context.EmployeeAssignments
                            .FirstOrDefaultAsync(ea => ea.Id == activeAssignment.Id);
                        
                        if (trackedAssignment != null)
                        {
                            // Eski atamanın EndDate'i, yeni atamanın StartDate'inden 1 saniye önce olmalı
                            var endDate = startDate.AddSeconds(-1);
                            trackedAssignment.EndDate = endDate;
                        }
                    }
                }

                // ========== MANAGER ID OTOMATİK ATAMA ==========
                
                // ManagerId belirtilmemişse ve OrgUnitId varsa, OrgUnit'in ManagerId'sini kullan
                if (string.IsNullOrEmpty(dto.ManagerId) && dto.OrgUnitId.HasValue)
                {
                    var orgUnit = await unitOfWork._context.OrgUnits
                        .FirstOrDefaultAsync(ou => ou.Id == dto.OrgUnitId.Value);
                    
                    if (orgUnit != null && !string.IsNullOrEmpty(orgUnit.ManagerId))
                    {
                        // OrgUnit'in manager'ı kullanıcının kendisi değilse kullan
                        if (orgUnit.ManagerId != dto.UserId)
                        {
                            dto.ManagerId = orgUnit.ManagerId;
                        }
                    }
                }

                // ========== YENİ ATAMA OLUŞTUR ==========
                
                var assignment = _mapper.Map<EmployeeAssignment>(dto);
                assignment.StartDate = startDate;
                assignment.EndDate = dto.EndDate; // null = aktif

                var result = await _employeeAssignments.AddAsync(_mapper.Map<EmployeeAssignmentListDto>(assignment));
                
                if (result.StatusCode < 200 || result.StatusCode >= 300)
                {
                    _unitOfWork.Rollback();
                    return BadRequest(result.Errors);
                }

                await _unitOfWork.CommitAsync();
                return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Atamayı günceller
        /// 
        /// NOT: Primary assignment'ın StartDate'i değiştirilirse, 
        /// eski aktif atama ile çakışma kontrolü yapılmalı
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(EmployeeAssignmentUpdateDto dto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var existing = await _employeeAssignments.GetByIdGuidAsync(dto.Id);
                if (existing.StatusCode < 200 || existing.StatusCode >= 300)
                {
                    return NotFound("Assignment not found");
                }

                var existingAssignment = existing.Data;
                
                // ManagerId belirtilmemişse ve OrgUnitId varsa, OrgUnit'in ManagerId'sini kullan
                if (string.IsNullOrEmpty(dto.ManagerId) && dto.OrgUnitId.HasValue)
                {
                    var unitOfWork = _unitOfWork as UnitOfWork;
                    if (unitOfWork != null)
                    {
                        var orgUnit = await unitOfWork._context.OrgUnits
                            .FirstOrDefaultAsync(ou => ou.Id == dto.OrgUnitId.Value);
                        
                        if (orgUnit != null && !string.IsNullOrEmpty(orgUnit.ManagerId))
                        {
                            dto.ManagerId = orgUnit.ManagerId;
                        }
                    }
                }

                var assignment = _mapper.Map<EmployeeAssignment>(dto);
                
                // StartDate ve EndDate kontrolü
                if (dto.StartDate.HasValue)
                {
                    assignment.StartDate = dto.StartDate.Value;
                }
                if (dto.EndDate.HasValue)
                {
                    assignment.EndDate = dto.EndDate.Value;
                    // EndDate, StartDate'den önce olamaz
                    if (assignment.EndDate < assignment.StartDate)
                    {
                        _unitOfWork.Rollback();
                        return BadRequest("EndDate cannot be earlier than StartDate");
                    }
                }

                var result = await _employeeAssignments.UpdateAsync(_mapper.Map<EmployeeAssignmentListDto>(assignment));
                
                if (result.StatusCode < 200 || result.StatusCode >= 300)
                {
                    _unitOfWork.Rollback();
                    return BadRequest(result.Errors);
                }

                await _unitOfWork.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Atamayı sonlandırır (EndDate'i günceller)
        /// </summary>
        [HttpPut("{id}/end")]
        public async Task<IActionResult> EndAssignment(Guid id, [FromBody] DateTime? endDate = null)
        {
            try
            {
                var assignment = await _employeeAssignments.GetByIdGuidAsync(id);
                if (assignment.StatusCode < 200 || assignment.StatusCode >= 300)
                {
                    return NotFound("Assignment not found");
                }

                var updateDto = new EmployeeAssignmentUpdateDto
                {
                    Id = id,
                    EndDate = endDate ?? DateTime.UtcNow
                };

                var result = await Update(updateDto);
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        /// <summary>
        /// Atamayı siler
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _employeeAssignments.RemoveAsyncByGuid(id);
                
                if (result.StatusCode < 200 || result.StatusCode >= 300)
                {
                    return NotFound("Assignment not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }
    }
}

