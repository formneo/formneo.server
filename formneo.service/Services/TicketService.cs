using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NLayer.Core.Services;
using NLayer.Service.Services;
using System.Globalization;
using System.Linq;
using formneo.core.DTOs;
using formneo.core.DTOs.Budget.SF;
using formneo.core.DTOs.DashboardDto;
using formneo.core.DTOs.ProjectDtos;
using formneo.core.DTOs.Ticket;
using formneo.core.DTOs.Ticket.TicketAssigne;
using formneo.core.DTOs.Ticket.TicketDepartments;
using formneo.core.DTOs.Ticket.Tickets;
using formneo.core.EnumExtensions;
using formneo.core.Models;
using formneo.core.Models.TaskManagement;
using formneo.core.Models.Ticket;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.UnitOfWorks;
using formneo.repository.Repositories;

namespace formneo.service.Services
{
    public class TicketService : Service<Tickets>, ITicketServices
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IServiceWithDto<TicketDepartment, TicketDepartmensListDto> _ticketDepartments;
        private readonly IServiceWithDto<Tickets, TicketListDto> _ticketService;
        private readonly IUserService _userService;
        private readonly UserManager<UserApp> _userManager;
        private readonly ITenantContext _tenantContext;
        private readonly IUserTenantService _userTenantService;

        public TicketService(IGenericRepository<Tickets> repository, IUnitOfWork unitOfWork, IMapper mapper, UserManager<UserApp> userManager, IUserService userService, ITicketRepository ticketRepository, IServiceWithDto<TicketDepartment, TicketDepartmensListDto> ticketDepartments, IServiceWithDto<Tickets, TicketListDto> ticketService, ITenantContext tenantContext, IUserTenantService userTenantService) : base(repository, unitOfWork)
        {
            _mapper = mapper;
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
            _ticketDepartments = ticketDepartments;
            _ticketService = ticketService;
            _userService = userService;
            _userManager = userManager;
            _tenantContext = tenantContext;
            _userTenantService = userTenantService;


        }

        public async Task<CustomResponseDto<UserAppDto>> CreateTicketAsync(TicketInsertDto createUserDto)
        {

            return null;

            //await _ticketRepository.AddAsync(_mapper.Map<Tickets>(createUserDto));



            //_unitOfWork.CommitAsync();
            //return CustomResponseDto<UserAppDto>.Fail(400, "");


        }

        public async Task<TicketDtoResult> GetAllAssignTicketsWithEnumDescriptionsAsync(string createUser, int skip = 0, int top = 50, List<int>? statues = null, TicketFilters? filters = null)
        {
            var userid = await _userService.GetUserByEmailAsync(createUser);

            TicketDtoResult result = new TicketDtoResult();

            var managedDepartmentIds = await _ticketDepartments
                .Where(d => d.Manager.UserName == createUser || d.DepartmentUsers.Any(du => du.User.UserName == createUser));

            //var managedDepartmentIds = await _ticketDepartments
            //    .Where(d => d.Manager.UserName == createUser);


            var ids = managedDepartmentIds.Data.Select(d => d.Id).ToList();



            var query = _ticketRepository
                    .Where(e => e.IsDelete == false && e.Status != TicketStatus.Draft && e.Status != TicketStatus.InApprove &&
                                (
                                 e.TicketAssigne.UserApp.UserName == createUser ||
                                 e.TicketAssigne.TicketTeam.TeamList.Any(u => u.UserApp.UserName == createUser) ||
                                 ids.Contains(e.TicketDepartmentId.ToString()))// Yönetici olduğu departmanlar
                                 && (statues == null || statues.Count == 0 || statues.Contains((int)e.Status))
                     )
                    .Include(ticket => ticket.UserApp)
                    .Include(ticket => ticket.WorkCompany)
                    .Include(ticket => ticket.CustomerRef)
                    .Include(ticket => ticket.TicketAssigne)
                    .ThenInclude(assigne => assigne.UserApp)
                    .Include(ticket => ticket.TicketAssigne)
                    .ThenInclude(assigne => assigne.TicketTeam)
                    .Include(ticket => ticket.TicketDepartment)
                    .Include(e=>e.TicketProject).AsQueryable();

            if (!string.IsNullOrEmpty(filters.workCompanyId))
            {
                query = query.Where(e => e.WorkCompanyId == new Guid(filters.workCompanyId));
            }
            if (!string.IsNullOrEmpty(filters.talepNo))
            {
                query = query.Where(e => e.UniqNumber.ToString() == filters.talepNo);
            }
            if (!string.IsNullOrEmpty(filters.talepBaslik))
            {
                query = query.Where(e => e.Title.Contains(filters.talepBaslik));
            }
            if (!string.IsNullOrEmpty(filters.assignedUser))
            {
                if (filters.assignedUser == "999999")
                {
                    query = query.Where(e => e.TicketAssigne.UserApp.Id == null && e.TicketAssigne.UserApp == null && e.TicketAssigne.TicketTeam == null && e.TicketAssigne.TicketTeam.Id == null);
                }
                else
                {
                    query = query.Where(e => e.TicketAssigne.UserApp.Id == filters.assignedUser);
                }
            }

            if (!string.IsNullOrEmpty(filters.assignedTeam))
            {
                query = query.Where(e => e.TicketAssigne.TicketTeam.Id == new Guid(filters.assignedTeam));
            }
            if (!string.IsNullOrEmpty(filters.type) && int.TryParse(filters.type, out int typeValue))
            {
                query = query.Where(e => (int)e.Type == typeValue);
            }
            if (!string.IsNullOrEmpty(filters.endDate) && DateTime.TryParseExact(filters.endDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEndDate))
            {
                query = query.Where(e => e.CreatedDate.Date <= parsedEndDate.Date);
            }
            if (!string.IsNullOrEmpty(filters.startDate) && DateTime.TryParseExact(filters.startDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStartDate))
            {
                query = query.Where(e => e.CreatedDate.Date >= parsedStartDate.Date);
            }
            if (!string.IsNullOrEmpty(filters.creator))
            {
                query = query.Where(e => e.UserApp.UserName == filters.creator);
            }
            if (!string.IsNullOrEmpty(filters.customer))
            {
                query = query.Where(e => e.CustomerRefId == new Guid(filters.customer));
            }
            if (filters.departmentId?.Any() == true)
            {
                var guids = filters.departmentId
                    .Where(x => Guid.TryParse(x, out _))
                    .Select(Guid.Parse)
                    .ToList();

                if (guids.Any())
                {
                    query = query.Where(e => e.TicketDepartmentId.HasValue && guids.Contains(e.TicketDepartmentId.Value));
                }
            }
            if (filters.ticketProjectId?.Any() == true)
            {
                var guids = filters.ticketProjectId
                    .Where(x => Guid.TryParse(x, out _))
                    .Select(Guid.Parse)
                    .ToList();

                if (guids.Any())
                {
                    query = query.Where(e => e.TicketProjectId.HasValue && guids.Contains(e.TicketProjectId.Value));
                }
            }



            result.Count = await query.CountAsync();



            //var tickets = await _ticketRepository
            //    .Where(e => e.IsDelete == false && e.Status != TicketStatus.Draft && e.Status != TicketStatus.InApprove &&
            //                (
            //                 e.TicketAssigne.UserApp.UserName == createUser ||
            //                 e.TicketAssigne.TicketTeam.TeamList.Any(u => u.UserApp.UserName == createUser) ||
            //                 ids.Contains(e.TicketDepartmentId.ToString()))// Yönetici olduğu departmanlar
            //                 && (statues == null || statues.Count == 0 || statues.Contains((int)e.Status)))  // statues kontrolü eklendi

            //    .Include(ticket => ticket.UserApp)
            //    .Include(ticket => ticket.WorkCompany)
            //    .Include(ticket => ticket.CustomerRef)
            //    .Include(ticket => ticket.TicketAssigne)
            //        .ThenInclude(assigne => assigne.UserApp)
            //    .Include(ticket => ticket.TicketAssigne)
            //        .ThenInclude(assigne => assigne.TicketTeam)
            //    .Include(ticket => ticket.TicketDepartment) // Ana tablodaki departman ilişkisini yükle

            if (top == 0)
            {
                top = result.Count;
            }

            var tickets = await query.Select(ticket => new
            {
                ticket.Id,
                ticket.Status,
                ticket.TicketCode,
                ticket.Type,
                ticket.CreatedDate,
                ticket.TicketDepartment,
                ticket.ApproveStatus,
                ticket.WorkCompanyId,
                ticket.CustomerRefId,
                ticket.UserAppId,
                ticket.TicketDepartmentId,
                ticket.WorkCompanySystemInfoId,
                ticket.CreatedBy,
                ticket.UniqNumber,
                ticket.isTeam,
                WorkFlowHeadId = ticket.WorkflowHeadId.ToString(),
                ticket.WorkflowHeadId,
                ticket.Title,
                ticket.TicketProjectId,
                ticket.TicketProject,
                UserApp = ticket.UserApp != null
            ? new { ticket.UserApp.Id, ticket.UserApp.UserName, ticket.UserApp.NormalizedUserName, ticket.UserApp.FirstName, ticket.UserApp.LastName }
            : null,
                WorkCompany = ticket.WorkCompany,
                CustomerRef = ticket.CustomerRef,
                WorkCompanySystemInfo = ticket.WorkCompanySystemInfo != null
            ? new { ticket.WorkCompanySystemInfo.Id, ticket.WorkCompanySystemInfo.Name }
            : null,
                TicketAssigne = ticket.TicketAssigne != null
            ? new
            {
                ticket.TicketAssigne.Id,
                UserApp = ticket.TicketAssigne.UserApp != null
                    ? new { ticket.TicketAssigne.UserApp.Id, ticket.TicketAssigne.UserApp.NormalizedUserName, ticket.TicketAssigne.UserApp.FirstName, ticket.TicketAssigne.UserApp.LastName }
                    : null,
                TicketTeam = ticket.TicketAssigne.TicketTeam != null
                    ? new { ticket.TicketAssigne.TicketTeam.Name, ticket.TicketAssigne.TicketTeam.Id }
                    : null
            }
            : null
            }).OrderByDescending(e => e.CreatedDate).Skip(skip).Take(top).ToListAsync();




            try
            {

                var ticketDtos = tickets.Select(ticket => new TicketListDto
                {
                    TicketCode = ticket.TicketCode,
                    Id = ticket.Id,
                    Type = ticket.Type,
                    TypeText = ticket.Type!.GetDescription(),
                    ApproveStatus = ticket.ApproveStatus,
                    ApproveStatusText = ticket.ApproveStatus!.GetDescription(),
                    UserAppId = ticket.UserAppId,
                    UserAppName = ticket.UserApp.FirstName + " " + ticket.UserApp.LastName,
                    UserAppUserName = ticket.UserApp.UserName,
                    WorkCompanyId = ticket.WorkCompanyId,
                    WorkCompanyName = ticket.WorkCompany.Name,
                    CustomerRefId = ticket.CustomerRefId != null ? (Guid)ticket.CustomerRefId : Guid.Empty,
                    CustomerRefName = ticket.CustomerRef != null ? ticket.CustomerRef.Name : "",
                    WorkCompanySystemInfoId = ticket.WorkCompanySystemInfoId!,
                    WorkCompanySystemName = ticket.WorkCompanySystemInfo?.Name,
                    WorkFlowHeadId = ticket.WorkflowHeadId.ToString(),
                    CreatedDate = ticket.CreatedDate,
                    CreatedBy = ticket.CreatedBy,
                    TicketProjectId = ticket.TicketProjectId,
                    TicketprojectName = ticket.TicketProject?.Name,
                    Title = ticket.Title,
                    Status = ticket.Status,
                    TicketNumber = ticket.UniqNumber,
                    StatusText = ticket.Status!.GetDescription(),
                    isTeam = ticket.isTeam,
                    TicketDepartmentId = ticket.TicketDepartmentId!.ToString(),
                    TicketDepartmentText = ticket.TicketDepartment != null ? ticket.TicketDepartment.DepartmentText : "",
                    //TicketAssigneId = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null ? ticket.TicketAssigne.UserApp.Id : "" : "",
                    TicketAssigneId = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null
                    ? ticket.TicketAssigne.UserApp.Id
                    : ticket.TicketAssigne.TicketTeam != null
                    ? ticket.TicketAssigne.TicketTeam.Id.ToString() // Team'den alacağınız alan
                    : "" : "",
                    TicketAssigneText = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null
                    ? ticket.TicketAssigne.UserApp.FirstName + " " + ticket.TicketAssigne.UserApp.LastName
                    : ticket.TicketAssigne.TicketTeam != null
                    ? ticket.TicketAssigne.TicketTeam.Name // Team'den alacağınız alan
                    : "Atama Yok" : "Atama Yok" // H

                }).ToList();

                result.TicketList = ticketDtos;
                return result;
                //return ticketDtos;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public async Task<List<TicketListDto>> GetByIdDetail(string createUser)
        {


            var managedDepartmentIds = await _ticketDepartments
                .Where(d => d.Manager.UserName == createUser || d.DepartmentUsers.Any(du => du.User.UserName == createUser));


            var ids = managedDepartmentIds.Data.Select(d => d.Id).ToList();

            var tickets = await _ticketRepository
                .Where(e => e.IsDelete == false && e.Status != TicketStatus.Draft &&
                            (
                             e.TicketAssigne.UserApp.UserName == createUser ||
                             e.TicketAssigne.TicketTeam.TeamList.Any(u => u.UserApp.UserName == createUser) ||
                             ids.Contains(e.TicketDepartmentId.ToString()))) // Yönetici olduğu departmanlar
                .Include(ticket => ticket.UserApp)
                .Include(ticket => ticket.WorkCompany)
                .Include(ticket => ticket.TicketAssigne)
                    .ThenInclude(assigne => assigne.UserApp)
                .Include(ticket => ticket.TicketAssigne)
                    .ThenInclude(assigne => assigne.TicketTeam)
                .Include(ticket => ticket.TicketDepartment) // Ana tablodaki departman ilişkisini yükle
          .Select(ticket => new
          {
              ticket.Id,
              ticket.Status,
              ticket.TicketCode,
              ticket.Type,
              ticket.CreatedDate,
              ticket.TicketDepartment,
              ticket.ApproveStatus,
              ticket.WorkCompanyId,
              ticket.UserAppId,
              ticket.TicketDepartmentId,
              ticket.WorkCompanySystemInfoId,
              ticket.CreatedBy,
              ticket.isTeam,
              ticket.Title,
              UserApp = ticket.UserApp != null
            ? new { ticket.UserApp.Id, ticket.UserApp.UserName, ticket.UserApp.NormalizedUserName, ticket.UserApp.FirstName, ticket.UserApp.LastName }
            : null,
              WorkCompany = ticket.WorkCompany,
              WorkCompanySystemInfo = ticket.WorkCompanySystemInfo != null
            ? new { ticket.WorkCompanySystemInfo.Id, ticket.WorkCompanySystemInfo.Name }
            : null,
              TicketAssigne = ticket.TicketAssigne != null
            ? new
            {
                ticket.TicketAssigne.Id,
                UserApp = ticket.TicketAssigne.UserApp != null
                    ? new { ticket.TicketAssigne.UserApp.Id, ticket.TicketAssigne.UserApp.NormalizedUserName, ticket.TicketAssigne.UserApp.FirstName, ticket.TicketAssigne.UserApp.LastName }
                    : null,
                TicketTeam = ticket.TicketAssigne.TicketTeam != null
                    ? new { ticket.TicketAssigne.TicketTeam.Name }
                    : null
            }
            : null
          })
    .ToListAsync();


            try
            {

                var ticketDtos = tickets.Select(ticket => new TicketListDto
                {
                    TicketCode = ticket.TicketCode,
                    Id = ticket.Id,
                    Type = ticket.Type,
                    TypeText = ticket.Type!.GetDescription(),
                    ApproveStatus = ticket.ApproveStatus,
                    ApproveStatusText = ticket.ApproveStatus!.GetDescription(),
                    UserAppId = ticket.UserAppId,
                    UserAppName = ticket.UserApp.FirstName + " " + ticket.UserApp.LastName,
                    UserAppUserName = ticket.UserApp.UserName,
                    WorkCompanyId = ticket.WorkCompanyId,
                    WorkCompanyName = ticket.WorkCompany.Name,
                    WorkCompanySystemInfoId = ticket.WorkCompanySystemInfoId!,
                    WorkCompanySystemName = ticket.WorkCompanySystemInfo?.Name,

                    CreatedDate = ticket.CreatedDate,
                    CreatedBy = ticket.CreatedBy,
                    Title = ticket.Title,
                    Status = ticket.Status,
                    StatusText = ticket.Status!.GetDescription(),
                    isTeam = ticket.isTeam,
                    TicketDepartmentId = ticket.TicketDepartmentId!.ToString(),
                    TicketDepartmentText = ticket.TicketDepartment != null ? ticket.TicketDepartment.DepartmentText : "",
                    TicketAssigneText = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null
                    ? ticket.TicketAssigne.UserApp.FirstName + " " + ticket.TicketAssigne.UserApp.LastName
                    : ticket.TicketAssigne.TicketTeam != null
                    ? ticket.TicketAssigne.TicketTeam.Name // Team'den alacağınız alan
                    : "Atama Yok" : "Atama Yok" // H

                }).ToList();

                return ticketDtos;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public async Task<TicketDtoResult> GetAllTicketsWithEnumDescriptionsAsync(string createUser, int skip = 0, int top = 50, List<int>? statues = null, TicketFilters? filters = null)
        {

            TicketDtoResult result = new TicketDtoResult();

            var query = _ticketRepository.Where(e => e.IsDelete == false && e.CreatedBy == createUser && (statues == null || statues.Count == 0 || statues.Contains((int)e.Status)))
                   .Include(ticket => ticket.UserApp)
                   .Include(ticket => ticket.WorkCompany)
                   .Include(ticket => ticket.CustomerRef)
                   .Include(ticket => ticket.TicketDepartment)
                   .Include(ticket => ticket.TicketAssigne)
                   .ThenInclude(assigne => assigne.UserApp) // TicketAssigne içindeki UserApp ilişkisini yükle
                   .Include(ticket => ticket.TicketAssigne)
                   .ThenInclude(assigne => assigne.TicketTeam)
                   .Include(ticket =>ticket.TicketProject)
                   .AsQueryable();

            //Kullanıcının ticket oluşturma yetkisi var mı?
            bool userPermission = false;
            if (_tenantContext?.CurrentTenantId != null)
            {
                var userId = _userManager.Users.Where(e => e.Email == createUser).Select(e => e.Id).FirstOrDefault();
                if (!string.IsNullOrEmpty(userId))
                {
                    var ut = await _userTenantService.GetByUserAndTenantAsync(userId, _tenantContext.CurrentTenantId.Value);
                    userPermission = ut != null && ut.HasDepartmentPermission;
                }
            }
            // Tenant context yoksa, departman izni tenant-bazlı olduğundan varsayılan: false

            if (userPermission)
            {
                //Kullanıcının departmandaki tüm kullanıcıları getirir




                query = null;
            }

            if (!string.IsNullOrEmpty(filters.workCompanyId))
            {
                query = query.Where(e => e.WorkCompanyId == new Guid(filters.workCompanyId));
            }
            if (!string.IsNullOrEmpty(filters.assignedUser))
            {
                if (filters.assignedUser == "999999")
                {
                    query = query.Where(e => e.TicketAssigne.UserApp.Id == null || e.TicketAssigne.Id == Guid.Empty);
                }
                else
                {
                    query = query.Where(e => e.TicketAssigne.UserApp.Id == filters.assignedUser);
                }
            }
            if (!string.IsNullOrEmpty(filters.talepNo))
            {
                query = query.Where(e => e.UniqNumber.ToString().Contains(filters.talepNo));
            }
            if (!string.IsNullOrEmpty(filters.talepBaslik))
            {
                query = query.Where(e => e.Title.Contains(filters.talepBaslik));
            }
            if (!string.IsNullOrEmpty(filters.assignedTeam))
            {
                query = query.Where(e => e.TicketAssigne.TicketTeam.Id == new Guid(filters.assignedTeam));
            }
            if (!string.IsNullOrEmpty(filters.type) && int.TryParse(filters.type, out int typeValue))
            {
                query = query.Where(e => (int)e.Type == typeValue);
            }
            if (!string.IsNullOrEmpty(filters.endDate) && DateTime.TryParseExact(filters.endDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEndDate))
            {
                query = query.Where(e => e.CreatedDate.Date <= parsedEndDate.Date);
            }
            if (!string.IsNullOrEmpty(filters.startDate) && DateTime.TryParseExact(filters.startDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedStartDate))
            {
                query = query.Where(e => e.CreatedDate.Date >= parsedStartDate.Date);
            }
            if (!string.IsNullOrEmpty(filters.creator))
            {
                query = query.Where(e => e.UserApp.UserName == filters.creator);
            }
            if (!string.IsNullOrEmpty(filters.customer))
            {
                query = query.Where(e => e.CustomerRefId == new Guid(filters.customer));
            }
            if (filters.departmentId?.Any() == true)
            {
                var guids = filters.departmentId
                    .Where(x => Guid.TryParse(x, out _))
                    .Select(Guid.Parse)
                    .ToList();

                if (guids.Any())
                {
                    query = query.Where(e => e.TicketDepartmentId.HasValue && guids.Contains(e.TicketDepartmentId.Value));
                }
            }
            if (filters.ticketProjectId?.Any() == true)
            {
                var guids = filters.ticketProjectId
                    .Where(x => Guid.TryParse(x, out _))
                    .Select(Guid.Parse)
                    .ToList();

                if (guids.Any())
                {
                    query = query.Where(e => e.TicketProjectId.HasValue && guids.Contains(e.TicketProjectId.Value));
                }
            }

            result.Count = await query.CountAsync();

            //  var tickets = await _ticketRepository
            //.Where(e => e.IsDelete == false && e.CreatedBy == createUser && (statues == null || statues.Count == 0 || statues.Contains((int)e.Status)))
            //.Include(ticket => ticket.UserApp)
            //.Include(ticket => ticket.WorkCompany)
            //.Include(ticket => ticket.CustomerRef)
            //.Include(ticket => ticket.TicketDepartment)
            //.Include(ticket => ticket.TicketAssigne)
            //.ThenInclude(assigne => assigne.UserApp) // TicketAssigne içindeki UserApp ilişkisini yükle
            //.Include(ticket => ticket.TicketAssigne)
            //.ThenInclude(assigne => assigne.TicketTeam) // TicketAssigne içindeki TicketTeam ilişkisini yükle

            if (top == 0)
            {
                top = result.Count;
            }

            var tickets = await query.Select(ticket => new
            {
                ticket.Id,
                ticket.Status,
                ticket.TicketCode,
                ticket.Type,
                ticket.CreatedDate,
                ticket.TicketDepartment,
                ticket.ApproveStatus,
                ticket.WorkCompanyId,
                ticket.CustomerRefId,
                ticket.UserAppId,
                ticket.TicketDepartmentId,
                ticket.WorkCompanySystemInfoId,
                ticket.CreatedBy,
                ticket.isTeam,
                ticket.WorkflowHeadId,
                ticket.Title,
                ticket.UniqNumber,
                ticket.TicketProjectId,
                ticket.TicketProject,
                UserApp = ticket.UserApp != null
              ? new { ticket.UserApp.Id, ticket.UserApp.UserName, ticket.UserApp.NormalizedUserName, ticket.UserApp.FirstName, ticket.UserApp.LastName }
              : null,
                WorkCompany = ticket.WorkCompany,
                CustomerRef = ticket.CustomerRef,
                WorkCompanySystemInfo = ticket.WorkCompanySystemInfo != null
              ? new { ticket.WorkCompanySystemInfo.Id, ticket.WorkCompanySystemInfo.Name }
              : null,
                TicketAssigne = ticket.TicketAssigne != null
              ? new
              {
                  ticket.TicketAssigne.Id,
                  UserApp = ticket.TicketAssigne.UserApp != null
                      ? new { ticket.TicketAssigne.UserApp.Id, ticket.TicketAssigne.UserApp.NormalizedUserName, ticket.TicketAssigne.UserApp.FirstName, ticket.TicketAssigne.UserApp.LastName }
                      : null,
                  TicketTeam = ticket.TicketAssigne.TicketTeam != null
                      ? new { ticket.TicketAssigne.TicketTeam.Name, ticket.TicketAssigne.TicketTeam.Id }
                      : null
              }
              : null
            }).OrderByDescending(e => e.CreatedDate).Skip(skip).Take(top).ToListAsync();

            var ticketDtos = tickets.Select(ticket => new TicketListDto
            {
                TicketCode = ticket.TicketCode,
                Id = ticket.Id,
                TicketNumber = ticket.UniqNumber,
                Type = ticket.Type,
                TypeText = ticket.Type!.GetDescription(),
                ApproveStatus = ticket.ApproveStatus,
                ApproveStatusText = ticket.ApproveStatus!.GetDescription(),
                UserAppId = ticket.UserAppId,
                UserAppName = ticket.UserApp.FirstName + " " + ticket.UserApp.LastName,
                UserAppUserName = ticket.UserApp.UserName,
                WorkCompanyId = ticket.WorkCompanyId,
                WorkCompanyName = ticket.WorkCompany.Name,
                CustomerRefId = ticket.CustomerRefId != null ? (Guid)ticket.CustomerRefId : Guid.Empty,
                CustomerRefName = ticket.CustomerRef != null ? ticket.CustomerRef.Name : "",
                WorkCompanySystemInfoId = ticket.WorkCompanySystemInfoId!,
                WorkCompanySystemName = ticket.WorkCompanySystemInfo?.Name,
                CreatedDate = ticket.CreatedDate,
                CreatedBy = ticket.CreatedBy,
                Title = ticket.Title,
                Status = ticket.Status,
                WorkFlowHeadId = ticket.WorkflowHeadId.ToString(),
                StatusText = ticket.Status!.GetDescription(),
                isTeam = ticket.isTeam,
                TicketProjectId = ticket.TicketProjectId,
                TicketprojectName = ticket.TicketProject?.Name,
                TicketDepartmentId = ticket.TicketDepartmentId!.ToString(),
                TicketDepartmentText = ticket.TicketDepartment != null ? ticket.TicketDepartment.DepartmentText : "",
                //TicketAssigneId = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null ? ticket.TicketAssigne.UserApp.Id : "" : "",
                TicketAssigneId = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null
                ? ticket.TicketAssigne.UserApp.Id
                : ticket.TicketAssigne.TicketTeam != null
                ? ticket.TicketAssigne.TicketTeam.Id.ToString() // Team'den alacağınız alan
                : "" : "",
                TicketAssigneText = ticket.TicketAssigne != null ? ticket.TicketAssigne.UserApp != null
                ? ticket.TicketAssigne.UserApp.FirstName + " " + ticket.TicketAssigne.UserApp.LastName
                : ticket.TicketAssigne.TicketTeam != null
                ? ticket.TicketAssigne.TicketTeam.Name // Team'den alacağınız alan
                : "Atama Yok" : "Atama Yok" // H
                // Diğer alanlar
            }).ToList();

            result.TicketList = ticketDtos;
            return result;
            //return ticketDtos;
        }

        public Task UpdateTicket(Tickets ticket)
        {
            _unitOfWork.BeginTransaction();

            _ticketRepository.Update(ticket);



            _unitOfWork.Commit();

            return null;
        }

        public async Task<GetSumTicketDto> CountAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {

            var service = await _ticketService.Include();

            var list = service
                   .Include(t => t.TicketAssigne) // TicketAssigne ile birlikte getir
                  .Where(t => t.TicketAssigne.UserAppId == userId &&
                              t.IsDelete == false)
                  .ToList();


            //        var tickets = await _ticketRepository.
            //.Where(t => ticketService.TicketAssigne
            //    .Where(ta => ta.UserAppId == userAppId)
            //    .Select(ta => ta.Id)
            //    .Contains(t.TicketAssigneId) &&
            //    t.IsDelete == false &&
            //    t.Status == 2)
            //.ToListAsync();

            //var query = _ticketRepository.Where(x => x.UserAppId == userId);

            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            var zeroTickets = list.Where(x => x.Status == 0).ToList();
            var sumDraftTickets = list.Where(x => x.Status == TicketStatus.Draft).ToList();
            var sumOpenTickets = list.Where(x => x.Status == TicketStatus.Open).ToList();
            var sumAssignedTickets = list.Where(x => x.Status == TicketStatus.Assigned).ToList();
            var sumConsultantWaitingTickets = list.Where(x => x.Status == TicketStatus.ConsultantWaiting).ToList();
            var sumInProgressTickets = list.Where(x => x.Status == TicketStatus.InProgress).ToList();
            var sumInternalTestingTickets = list.Where(x => x.Status == TicketStatus.InternalTesting).ToList();
            var sumCustomerTestingTickets = list.Where(x => x.Status == TicketStatus.CustomerTesting).ToList();
            var sumWaitingForCustomerTickets = list.Where(x => x.Status == TicketStatus.WaitingForCustomer).ToList();
            var sumResolvedTickets = list.Where(x => x.Status == TicketStatus.Resolved).ToList();
            var sumCanceledTickets = list.Where(x => x.Status == TicketStatus.Cancelled).ToList();
            var sumClosedTickets = list.Where(x => x.Status == TicketStatus.Closed).ToList();
            var sumInApproveTickets = list.Where(x => x.Status == TicketStatus.InApprove).ToList();
            var sumTickets = list.ToList();

            return new GetSumTicketDto
            {
                DraftCount = sumDraftTickets.Count,
                OpenCount = sumOpenTickets.Count,
                AssignedCount = sumAssignedTickets.Count,
                ConsultantWaitingCount = sumConsultantWaitingTickets.Count,
                InProgressCount = sumInProgressTickets.Count,
                InternalTestingCount = sumInternalTestingTickets.Count,
                CustomerTestingCount = sumCustomerTestingTickets.Count,
                WaitingForCustomerCount = sumWaitingForCustomerTickets.Count,
                ResolvedCount = sumResolvedTickets.Count,
                CanceledCount = sumCanceledTickets.Count,
                ClosedCount = sumClosedTickets.Count,
                InApproveCount = sumInApproveTickets.Count,
                SumCount = sumTickets.Count,
                ZeroCount = zeroTickets.Count
            };
        }

        public async Task<List<GetCompanyTicketDto>> GetUserTicketAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                   .Include(t => t.TicketAssigne).Include(x => x.CustomerRef) // TicketAssigne ile birlikte getir
                  .Where(t => t.TicketAssigne.UserAppId == userId &&
                              t.IsDelete == false)
                  .ToList();
            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            var tickets = list.GroupBy(y => new { y.CustomerRefId, y.CustomerRef.Name }).Select(g => new GetCompanyTicketDto
            {
                CompanyName = g.Key.Name,
                TicketCount = g.Count()
            });
            return tickets.ToList();

            //    var query = _ticketRepository
            //.Where(x => x.UserAppId == userId && x.Status != TicketStatus.Cancelled && x.Status != TicketStatus.Draft);

            //    if (startDate.HasValue)
            //        query = query.Where(x => x.CreatedDate >= startDate.Value);

            //    if (endDate.HasValue)
            //        query = query.Where(x => x.CreatedDate <= endDate.Value);

            //    var tickets = await query
            //        .Include(x => x.WorkCompany)
            //        .GroupBy(x => new { x.WorkCompanyId, x.WorkCompany.Name })
            //        .Select(g => new GetCompanyTicketDto
            //        {
            //            CompanyName = g.Key.Name,
            //            TicketCount = g.Count()
            //        })
            //        .ToListAsync();


        }

        public async Task<List<GetCompanyTicketDto>> GetUserOpenTicketAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                   .Include(t => t.TicketAssigne).Include(x => x.CustomerRef) // TicketAssigne ile birlikte getir
                  .Where(t => t.TicketAssigne.UserAppId == userId &&
                              t.IsDelete == false)
                  .ToList();
            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            var tickets = list.GroupBy(y => new { y.CustomerRefId, y.CustomerRef.Name }).Select(g => new GetCompanyTicketDto
            {
                CompanyName = g.Key.Name,
                TicketCount = g.Count()
            });
            return tickets.ToList();
            // var query = _ticketRepository
            //.Where(x => x.UserAppId == userId
            //&& x.Status != TicketStatus.Cancelled
            //&& x.Status != TicketStatus.Draft
            //&& x.Status != TicketStatus.Closed);

            // // Eğer startDate doluysa, CreatedDate >= startDate koşulunu ekle
            // if (startDate.HasValue)
            // {
            //     query = query.Where(x => x.CreatedDate >= startDate.Value);
            // }

            // // Eğer endDate doluysa, CreatedDate <= endDate koşulunu ekle
            // if (endDate.HasValue)
            // {
            //     query = query.Where(x => x.CreatedDate <= endDate.Value);
            // }

            // var tickets = await query
            //     .Include(x => x.WorkCompany) // Şirket bilgilerini dahil et
            //     .GroupBy(x => new { x.WorkCompanyId, x.WorkCompany.Name }) // WorkCompanyId'ye göre grupla
            //     .Select(g => new GetCompanyTicketDto
            //     {
            //         CompanyName = g.Key.Name,
            //         TicketCount = g.Count()
            //     }).ToListAsync();

            // return tickets;
        }

        public async Task<List<GetCompanyTicketInfoDto>> GetUserTicketInfoAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                   .Include(t => t.TicketAssigne).Include(x => x.CustomerRef) // TicketAssigne ile birlikte getir
                  .Where(t => t.TicketAssigne.UserAppId == userId &&
                              t.IsDelete == false)
                  .ToList();
            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            var tickets = list.GroupBy(y => new { y.CustomerRefId, y.CustomerRef.Name }).Select(g => new GetCompanyTicketInfoDto
            {
                CompanyName = g.Key.Name,
                TicketCount = g.Count(),
                ResolvedCount = g.Count(x => x.Status == TicketStatus.Resolved),
                OpenCount = g.Count(x => x.Status == TicketStatus.Open),
            });
            return tickets.ToList();
            //    var query = _ticketRepository
            //.Where(x => x.UserAppId == userId && x.Status != TicketStatus.Cancelled && x.Status != TicketStatus.Draft && x.Status != TicketStatus.Closed);

            //    // startDate ve endDate filtreleri ekleniyor
            //    if (startDate.HasValue)
            //    {
            //        query = query.Where(x => x.CreatedDate >= startDate.Value);
            //    }

            //    if (endDate.HasValue)
            //    {
            //        query = query.Where(x => x.CreatedDate <= endDate.Value);
            //    }

            //    var tickets = await query
            //        .Include(x => x.WorkCompany)
            //        .GroupBy(x => new { x.WorkCompanyId, x.WorkCompany.Name })
            //        .Select(g => new GetCompanyTicketInfoDto
            //        {
            //            CompanyName = g.Key.Name,
            //            TicketCount = g.Count(),
            //            ResolvedCount = g.Count(x => x.Status == TicketStatus.Resolved),
            //            OpenCount = g.Count(x => x.Status != TicketStatus.Resolved) // Açık talepler: çözülenler hariç
            //        })
            //        .ToListAsync();

            //    return tickets;
        }

        public async Task<List<GetTicketSubjectInfoDto>> GetCustomerTicketSubjectList(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                  .Where(t => t.UserAppId == userId &&
                              t.IsDelete == false && t.Status != TicketStatus.Draft)
                  .ToList();
            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            var tickets = list
            .GroupBy(y => y.TicketSubject)
            .Select(g => new GetTicketSubjectInfoDto
            {
                SubjectName = g.Key.ToString(),
                TicketCount = g.Count()
            })
            .ToList();
            return tickets.ToList();
        }

        public async Task<GetTicketStatusDto> GetCustomerTicketStatus(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                  .Where(t => t.UserAppId == userId && t.Status != TicketStatus.Cancelled &&
                              t.IsDelete == false && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Draft)
                  .ToList();
            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            return new GetTicketStatusDto
            {
                OpenCount = list.Count(t => t.Status != TicketStatus.Resolved),
                ClosedCount = list.Count(t => t.Status == TicketStatus.Resolved)
            };
        }

        public async Task<GetSumTicketDto> CustomerCountAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                  .Where(t => t.UserAppId == userId &&
                              t.IsDelete == false && t.Status != TicketStatus.Draft)
                  .ToList();

            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }

            var zeroTickets = list.Where(x => x.Status == 0).ToList();
            var sumDraftTickets = list.Where(x => x.Status == TicketStatus.Draft).ToList();
            var sumOpenTickets = list.Where(x => x.Status == TicketStatus.Open).ToList();
            var sumAssignedTickets = list.Where(x => x.Status == TicketStatus.Assigned).ToList();
            var sumConsultantWaitingTickets = list.Where(x => x.Status == TicketStatus.ConsultantWaiting).ToList();
            var sumInProgressTickets = list.Where(x => x.Status == TicketStatus.InProgress).ToList();
            var sumInternalTestingTickets = list.Where(x => x.Status == TicketStatus.InternalTesting).ToList();
            var sumCustomerTestingTickets = list.Where(x => x.Status == TicketStatus.CustomerTesting).ToList();
            var sumWaitingForCustomerTickets = list.Where(x => x.Status == TicketStatus.WaitingForCustomer).ToList();
            var sumResolvedTickets = list.Where(x => x.Status == TicketStatus.Resolved).ToList();
            var sumCanceledTickets = list.Where(x => x.Status == TicketStatus.Cancelled).ToList();
            var sumClosedTickets = list.Where(x => x.Status == TicketStatus.Closed).ToList();
            var sumInApproveTickets = list.Where(x => x.Status == TicketStatus.InApprove).ToList();
            var sumTickets = list.ToList();

            return new GetSumTicketDto
            {
                DraftCount = sumDraftTickets.Count,
                OpenCount = sumOpenTickets.Count,
                AssignedCount = sumAssignedTickets.Count,
                ConsultantWaitingCount = sumConsultantWaitingTickets.Count,
                InProgressCount = sumInProgressTickets.Count,
                InternalTestingCount = sumInternalTestingTickets.Count,
                CustomerTestingCount = sumCustomerTestingTickets.Count,
                WaitingForCustomerCount = sumWaitingForCustomerTickets.Count,
                ResolvedCount = sumResolvedTickets.Count,
                CanceledCount = sumCanceledTickets.Count,
                ClosedCount = sumClosedTickets.Count,
                InApproveCount = sumInApproveTickets.Count,
                SumCount = sumTickets.Count,
                ZeroCount = zeroTickets.Count
            };
        }

        public async Task<List<GetTicketCustomerOpenCloseDto>> CustomerOpenClose(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
                  .Where(t => t.UserAppId == userId &&
                              t.IsDelete == false && (t.Status != TicketStatus.Closed && t.Status != TicketStatus.Draft && t.Status != TicketStatus.Cancelled))
                  .ToList();



            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }




            var openCount = list.ToList();


            var listClose = service
                  .Where(t => t.UserAppId == userId &&
                              t.IsDelete == false && (t.Status == TicketStatus.Closed || t.Status == TicketStatus.Cancelled))
                  .ToList();



            if (startDate.HasValue)
            {
                listClose = listClose.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                listClose = listClose.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }



            var closeCount = listClose.ToList();

            List<GetTicketCustomerOpenCloseDto> listreesult = new List<GetTicketCustomerOpenCloseDto>();

            listreesult.Add(new GetTicketCustomerOpenCloseDto { Count = openCount.Count, Name = "Açık" });
            listreesult.Add(new GetTicketCustomerOpenCloseDto { Count = closeCount.Count, Name = "Kapalı" });
            return listreesult;
        }

        public async Task<List<GetTicketCustomerAssignGroupGroup>> CustomerAssignTeamInfo(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var service = await _ticketService.Include();

            var list = service
       .Include(e => e.TicketAssigne)
           .ThenInclude(e => e.TicketTeam)
       .Include(e => e.TicketAssigne)
           .ThenInclude(e => e.UserApp)
       .Where(t => t.UserAppId == userId && t.Status != TicketStatus.Draft && t.IsDelete == false)
       .Select(t => new
       {
           t.Id,
           t.Title,
           t.Description,
           t.Status,
           t.CreatedDate,
           TicketAssigne = t.TicketAssigne != null ? new
           {
               t.TicketAssigne.Id,
               TicketTeam = t.TicketAssigne.TicketTeam != null ? new
               {
                   t.TicketAssigne.TicketTeam.Id,
                   t.TicketAssigne.TicketTeam.Name
               } : null, // Eğer TicketTeam null ise buraya null atanır.
               UserApp = t.TicketAssigne.UserApp != null ? new
               {
                   t.TicketAssigne.UserApp.Id,
                   t.TicketAssigne.UserApp.FirstName,
                   t.TicketAssigne.UserApp.LastName,
                   TicketTeamID = t.TicketAssigne.TicketTeamID
               } : null // Eğer UserApp null ise buraya null atanır.
           } : null // Eğer TicketAssigne null ise buraya null atanır.
       })
       .ToList();



            if (startDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                list = list.Where(x => x.CreatedDate <= endDate.Value).ToList();
            }


            List<GetTicketCustomerAssignGroup> teamlist = new List<GetTicketCustomerAssignGroup>();
            foreach (var t in list)
            {

                var itemGrp = new GetTicketCustomerAssignGroup();
                if (t.TicketAssigne != null)
                {
                    if (t.TicketAssigne!.UserApp != null)
                    {
                        itemGrp.Name = t.TicketAssigne.UserApp.FirstName + " " + t.TicketAssigne.UserApp.LastName;
                        itemGrp.status = t.Status;
                    }
                    else if (t.TicketAssigne!.TicketTeam != null)
                    {
                        itemGrp.Name = t.TicketAssigne.TicketTeam.Name;
                        itemGrp.status = t.Status;
                    }

                }
                else
                {
                    itemGrp.Name = "Atama Yok";
                    itemGrp.status = t.Status;
                }

                teamlist.Add(itemGrp);
            }

            List<GetTicketCustomerAssignGroupGroup> grp = new List<GetTicketCustomerAssignGroupGroup>();

            foreach (var t in teamlist.GroupBy(e => e.Name))
            {
                GetTicketCustomerAssignGroupGroup item = new GetTicketCustomerAssignGroupGroup();
                item.Name = t.Key;

                item.TotalCount = teamlist.Where(e => e.Name == t.Key).Count();
                item.OpenCount = teamlist.Where(e => e.Name == t.Key && (e.status != TicketStatus.Resolved && e.status != TicketStatus.Closed && e.status != TicketStatus.Cancelled && e.status != TicketStatus.CustomerTesting)).Count();
                item.UnitTest = teamlist.Where(e => e.Name == t.Key && e.status == TicketStatus.InternalTesting).Count();
                item.CustomerTest = teamlist.Where(e => e.status == TicketStatus.CustomerTesting && e.Name == t.Key).Count();
                grp.Add(item);
            }


            return grp.OrderByDescending(e => e.TotalCount).ToList();
        }

        //public async Task<IEnumerable<GetProjectListDto>> GetAllProductListAsync()
        //{
        //    var values = await _ticketRepository.GetAll().ToListAsync();
        //    return values != null ? values.Select(y => new GetProjectListDto
        //    {
        //        Id=y.Id,
        //        Description = y.Description,
        //        Name = y.Name,
        //        UserId = y.UserId,
        //        CategoryName= GetEnumDescription((Category)y.CategoryId)
        //    }).ToList() : new List<GetProjectListDto>();
        //}

        //public async Task<IEnumerable<GetProjectListDto>> GetByUserProductListAsync(string userId)
        //{
        //    var values= await _projectRepository.Where(x=>x.UserId == userId).ToListAsync();
        //    return values!=null ? values.Select(y=>new GetProjectListDto
        //    {
        //        UserId=y.UserId,
        //        CategoryName= GetEnumDescription((Category)y.CategoryId),
        //        Description=y.Description,
        //        Name = y.Name,
        //        Id=y.Id
        //    }).ToList() : new List<GetProjectListDto>();
        //}
        //private string GetEnumDescription(Category category)
        //{
        //    var field = category.GetType().GetField(category.ToString());
        //    var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        //    return attribute?.Description ?? category.ToString(); // Description yoksa enum adını döndür
        //}
    }
}
