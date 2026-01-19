using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using formneo.core.DTOs.UserTenants;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.UnitOfWorks;

namespace formneo.service.Services
{
    public class UserTenantService : IUserTenantService
    {
        private readonly IUserTenantRepository _repository;
        private readonly IGenericRepository<UserTenant> _genericRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserTenantService(IUserTenantRepository repository, IGenericRepository<UserTenant> genericRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repository = repository;
            _genericRepository = genericRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserTenantListDto> AddAsync(UserTenantInsertDto dto)
        {
            var entity = _mapper.Map<UserTenant>(dto);
            // Ensure defaults are respected; properties come from dto
            await _genericRepository.AddAsync(entity);
            await _unitOfWork.CommitAsync();
            return _mapper.Map<UserTenantListDto>(entity);
        }

        public async Task<UserTenantListDto> UpdateAsync(UserTenantUpdateDto dto)
        {
            var exists = await _genericRepository.GetByIdStringGuidAsync(dto.Id);
            exists.IsActive = dto.IsActive;
            // Map tenant-scoped fields
            exists.HasTicketPermission = dto.HasTicketPermission;
            exists.HasDepartmentPermission = dto.HasDepartmentPermission;
            exists.HasOtherCompanyPermission = dto.HasOtherCompanyPermission;
            exists.HasOtherDeptCalendarPerm = dto.HasOtherDeptCalendarPerm;
            exists.canEditTicket = dto.canEditTicket;
            exists.DontApplyDefaultFilters = dto.DontApplyDefaultFilters;
            exists.PCname = dto.PCname;
            _genericRepository.Update(exists);
            await _unitOfWork.CommitAsync();
            return _mapper.Map<UserTenantListDto>(exists);
        }

        public async Task<UserTenantListDto> GetByUserAndTenantAsync(string userId, Guid tenantId)
        {
            var entity = await _repository.GetByUserAndTenantAsync(userId, tenantId);
            return _mapper.Map<UserTenantListDto>(entity);
        }

        public async Task<IEnumerable<UserTenantFullDto>> GetByUserAsync(string userId)
        {
            // Tek sorguda sadece gerekli alanları çek (Include yok, projection + AsNoTracking)
            var rows = await _genericRepository.Where(x => x.UserId == userId)
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.TenantId,
                    x.IsActive,
                    UserFirstName = x.User.FirstName,
                    UserLastName = x.User.LastName,
                    UserEmail = x.User.Email,
                    TenantName = x.Tenant.Name,
                    TenantSlug = x.Tenant.Slug
                })
                .ToListAsync();

            var result = rows.Select(x => new UserTenantFullDto
            {
                Id = x.Id,
                UserId = x.UserId,
                TenantId = x.TenantId,
                IsActive = x.IsActive,
                UserFullName = ((x.UserFirstName ?? "").Trim() + " " + (x.UserLastName ?? "").Trim()).Trim(),
                UserEmail = x.UserEmail ?? string.Empty,
                TenantName = x.TenantName ?? string.Empty,
                TenantSlug = x.TenantSlug ?? string.Empty
            }).ToList();
            return result;
        }

        public async Task<IEnumerable<UserTenantListDto>> GetByTenantAsync(Guid tenantId)
        {
            var query = _genericRepository.Where(x => x.TenantId == tenantId);
            var list = await query.ToListAsync();
            return _mapper.Map<IEnumerable<UserTenantListDto>>(list);
        }

        public async Task<IEnumerable<UserTenantByTenantDto>> GetUsersByTenantAsync(Guid tenantId)
        {
            var list = await _repository.GetByTenantWithIncludesAsync(tenantId);

            var result = list.Select(x => new UserTenantByTenantDto
            {
                UserId = x.UserId,
                UserFullName = ((x.User?.FirstName ?? "").Trim() + " " + (x.User?.LastName ?? "").Trim()).Trim(),
                TenantName = x.Tenant?.Name ?? string.Empty
            });

            return result;
        }

        public async Task<IEnumerable<UserTenantFullDto>> GetAllFullAsync()
        {
            var list = await _repository.GetAllWithIncludesAsync();
            // Map manually to include composite fields
            var result = list.Select(x => new UserTenantFullDto
            {
                Id = x.Id,
                UserId = x.UserId,
                TenantId = x.TenantId,
                IsActive = x.IsActive,
                UserFullName = ((x.User?.FirstName ?? "").Trim() + " " + (x.User?.LastName ?? "").Trim()).Trim(),
                UserEmail = x.User?.Email ?? string.Empty,
                TenantName = x.Tenant?.Name ?? string.Empty,
                TenantSlug = x.Tenant?.Slug ?? string.Empty
            });
            return result;
        }



        public async Task BulkAssignUsersAsync(UserTenantBulkAssignUsersDto dto)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                // Tenant'a ait mevcut kayıtları filtreleri yok sayarak temizle (duplicate'i önle)
                var existing = await _genericRepository.GetAll().IgnoreQueryFilters().Where(x => x.TenantId == dto.TenantId).ToListAsync();
                if (existing.Any())
                {
                    _genericRepository.RemoveRange(existing);
                }

                // Sadece mevcut (AspNetUsers) kullanıcılarını al
                var distinctUserIds = dto.UserIds.Distinct().ToList();
                // DbContext'e erişim için repository'nin context'ini kullan
                // IGenericRepository üzerinden Users setine doğrudan erişim yok; bu yüzden EF sorgusu yerine FK ihlalini önlemek için
                // var olan UserId'leri doğrulayan hafif bir kontrol yapacağız.
                // GenericRepository, AppDbContext'e sahip olduğundan, oradan doğrulama yapmak en sağlıklısı olur; ancak
                // mevcut mimaride doğrudan erişim yoksa, ekleme sırasında başarısız olacak kayıtları filtreleyebilmek için
                // UserTenantRepository'de Include ile okuma yapılabildiği varsayımıyla _repository üzerinden context'e erişiyoruz.

                // Context'e erişim: _repository as formneo.repository.Repositories.UserTenantRepository
                var concreteRepo = _repository as formneo.repository.Repositories.UserTenantRepository;
                var ctx = concreteRepo != null ? concreteRepo.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(concreteRepo) as formneo.repository.AppDbContext : null;

                List<string> validUserIds = distinctUserIds;
                if (ctx != null)
                {
                    validUserIds = await ctx.Users
                        .Where(u => distinctUserIds.Contains(u.Id))
                        .Select(u => u.Id)
                        .ToListAsync();
                }

                var invalidUserIds = distinctUserIds.Except(validUserIds).ToList();
                if (invalidUserIds.Any())
                {
                    // Geçersiz kullanıcı ID'leri varsa 400 fırlat
                    throw new InvalidOperationException($"Geçersiz kullanıcı Id'leri: {string.Join(", ", invalidUserIds)}");
                }

                // Geçerli kullanıcı id listesiyle yeni kayıtları hazırla
                var newEntities = validUserIds.Select(userId => new UserTenant
                {
                    UserId = userId,
                    TenantId = dto.TenantId,
                    IsActive = dto.IsActive
                }).ToList();

                if (newEntities.Any())
                {
                    await _genericRepository.AddRangeAsync(newEntities);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        // IServiceWithDto methods
        public async Task<formneo.core.DTOs.CustomResponseDto<UserTenantListDto>> GetByIdAsync(string id)
        {
            var entity = await _genericRepository.GetByIdStringAsync(id);
            var dto = _mapper.Map<UserTenantListDto>(entity);
            return formneo.core.DTOs.CustomResponseDto<UserTenantListDto>.Success(200, dto);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<UserTenantListDto>> GetByIdGuidAsync(Guid id)
        {
            var entity = await _genericRepository.GetByIdStringGuidAsync(id);
            var dto = _mapper.Map<UserTenantListDto>(entity);
            return formneo.core.DTOs.CustomResponseDto<UserTenantListDto>.Success(200, dto);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<IEnumerable<UserTenantListDto>>> GetAllAsync()
        {
            var list = await _genericRepository.GetAll().ToListAsync();
            var dtos = _mapper.Map<IEnumerable<UserTenantListDto>>(list);
            return formneo.core.DTOs.CustomResponseDto<IEnumerable<UserTenantListDto>>.Success(200, dtos);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<IEnumerable<UserTenantListDto>>> Where(System.Linq.Expressions.Expression<Func<UserTenant, bool>> expression)
        {
            var list = await _genericRepository.Where(expression).ToListAsync();
            var dtos = _mapper.Map<IEnumerable<UserTenantListDto>>(list);
            return formneo.core.DTOs.CustomResponseDto<IEnumerable<UserTenantListDto>>.Success(200, dtos);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<bool>> AnyAsync(System.Linq.Expressions.Expression<Func<UserTenant, bool>> expression)
        {
            var exists = await _genericRepository.AnyAsync(expression);
            return formneo.core.DTOs.CustomResponseDto<bool>.Success(200, exists);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<UserTenantListDto>> AddAsync(UserTenantListDto dto)
        {
            var entity = _mapper.Map<UserTenant>(dto);
            await _genericRepository.AddAsync(entity);
            await _unitOfWork.CommitAsync();
            var newDto = _mapper.Map<UserTenantListDto>(entity);
            return formneo.core.DTOs.CustomResponseDto<UserTenantListDto>.Success(201, newDto);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<IEnumerable<UserTenantListDto>>> AddRangeAsync(IEnumerable<UserTenantListDto> dtos)
        {
            var entities = _mapper.Map<IEnumerable<UserTenant>>(dtos);
            await _genericRepository.AddRangeAsync(entities);
            await _unitOfWork.CommitAsync();
            var newDtos = _mapper.Map<IEnumerable<UserTenantListDto>>(entities);
            return formneo.core.DTOs.CustomResponseDto<IEnumerable<UserTenantListDto>>.Success(201, newDtos);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>> UpdateAsync(UserTenantListDto dto)
        {
            var entity = _mapper.Map<UserTenant>(dto);
            _genericRepository.Update(entity);
            await _unitOfWork.CommitAsync();
            return formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>.Success(204);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>> RemoveAsync(string id)
        {
            var entity = await _genericRepository.GetByIdStringAsync(id);
            _genericRepository.Remove(entity);
            await _unitOfWork.CommitAsync();
            return formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>.Success(204);
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>> RemoveAsyncByGuid(Guid id)
        {
            var entity = await _genericRepository.GetByIdStringGuidAsync(id);
            _genericRepository.Remove(entity);
            await _unitOfWork.CommitAsync();
            return formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>.Success(204);
        }

        public Task<formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>> RemoveRangeAsync(IEnumerable<int> ids)
        {
            return Task.FromResult(formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>.Success(204));
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<UserTenantListDto>> Find(System.Linq.Expressions.Expression<Func<UserTenant, bool>> expression)
        {
            var entity = await _genericRepository.Where(expression).FirstOrDefaultAsync();
            var dto = _mapper.Map<UserTenantListDto>(entity);
            return formneo.core.DTOs.CustomResponseDto<UserTenantListDto>.Success(200, dto);
        }

        public Task<IQueryable<UserTenant>> Include()
        {
            return Task.FromResult(_genericRepository.GetAll());
        }

        public async Task<formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>> SoftDeleteAsync(Guid id)
        {
            var entity = await _genericRepository.GetByIdStringGuidAsync(id);
            entity.IsDelete = true;
            await _unitOfWork.CommitAsync();
            return formneo.core.DTOs.CustomResponseDto<formneo.core.DTOs.NoContentDto>.Success(204);
        }
    }
}


