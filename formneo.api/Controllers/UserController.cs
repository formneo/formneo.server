using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using NLayer.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using formneo.api.Controllers;
using formneo.api.Helper;
using formneo.core.DTOs;
using formneo.core.DTOs.EmployeeAssignments;
using formneo.core.DTOs.OrgUnits;
using formneo.core.EnumExtensions;
using formneo.core.Helpers;
using formneo.core.Models;
using formneo.core.Models.TaskManagement;
using formneo.core.Models.Ticket;
using formneo.core.Services;

namespace formneo.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;
        private readonly ITicketServices _ticketService;
        private readonly IMapper _mapper;
        private readonly UserManager<UserApp> _userManager;
        private readonly IServiceWithDto<WorkCompanyTicketMatris, WorkCompanyTicketMatrisListDto> _workCompanyMatrisService;
        private readonly IServiceWithDto<OrgUnit, OrgUnitListDto> _orgUnits;
        private readonly IServiceWithDto<EmployeeAssignment, EmployeeAssignmentListDto> _employeeAssignmentService;
        private readonly DbNameHelper _dbNameHelper;
        private readonly ITenantContext _tenantContext;
        private readonly IUserTenantService _userTenantService;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(DbNameHelper dbNameHelper, IMapper mapper, IServiceWithDto<OrgUnit, OrgUnitListDto> orgUnitsService, IUserService userService, UserManager<UserApp> userManager, IServiceWithDto<WorkCompanyTicketMatris, WorkCompanyTicketMatrisListDto> workCompanyMatrisService, ITicketServices ticketService, ITenantContext tenantContext, IUserTenantService userTenantService, RoleManager<IdentityRole> roleManager, IServiceWithDto<EmployeeAssignment, EmployeeAssignmentListDto> employeeAssignmentService)
        {
            _dbNameHelper = dbNameHelper;
            _orgUnits = orgUnitsService;
            _userService = userService;
            _mapper = mapper;
            _userManager = userManager;
            _workCompanyMatrisService = workCompanyMatrisService;
            _ticketService = ticketService;
            _tenantContext = tenantContext;
            _userTenantService = userTenantService;
            _roleManager = roleManager;
            _employeeAssignmentService = employeeAssignmentService;
        }

        private async Task<IQueryable<UserApp>> ApplyTenantFilterAsync(IQueryable<UserApp> query)
        {
            // X-Tenant-Id varsa ve global admin değilse, sadece bu tenant'a üye kullanıcılar listelensin
            if (_tenantContext?.CurrentTenantId != null)
            {
                var tenantId = _tenantContext.CurrentTenantId.Value;

                // UserTenant tablosu üzerinden üye kullanıcıları çekecek alt sorgu
                var userIdsInTenant = (await _userTenantService.GetUsersByTenantAsync(tenantId))
                    .Select(x => x.UserId)
                    .ToHashSet();

                query = query.Where(u => userIdsInTenant.Contains(u.Id));
            }

            return query;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto, bool isSendMail = false)
        {
            //_userService.GetUserByNameAsync()


            if (isSendMail)
            {
                List<string> mails = new List<string>();
                CreateMailUserDto dto = new CreateMailUserDto();
                dto.Email = createUserDto.Email;
                dto.FirstName = createUserDto.FirstName;
                dto.LastName = createUserDto.LastName;
                dto.Password = createUserDto.Password;
                mails.Add(dto.Email);
                await SendUserInfoMail(dto, mails, "formneo Ticket Sistemine Hoş Geldiniz");
            }
            // Gelen fotoğraf formatına göre sadece base64 kaydet
            int commaIndex = createUserDto.photo.IndexOf(",");
            if (commaIndex != -1)
            {
                createUserDto.photo = createUserDto.photo.Substring(commaIndex + 1);
            }
            return CreateActionResult(await _userService.CreateUserAsync(createUserDto));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(UpdateUserDto createUserDto)
        {


            // Gelen fotoğraf formatına göre sadece base64 kaydet
            int commaIndex = createUserDto.photo.IndexOf(",");
            if (commaIndex != -1)
            {
                createUserDto.photo = createUserDto.photo.Substring(commaIndex + 1);
            }

            return CreateActionResult(await _userService.UpdateUserAsync(createUserDto));
        }

        [HttpGet("RemoveUser")]
        public async Task<IActionResult> RemoveUser(string userName)
        {
            return CreateActionResult(await _userService.RemoveUserAsync(userName));
        }

        // Tek bir kullanıcı verisi
        [HttpGet]
        public async Task<ActionResult<UserAppDto>> GetUser(string userName)
        {
            var data = await _userService.GetUserByNameAsync(userName);

            return data.Data;
        }

        // ID ile kullanıcı verisi getirir
        [HttpGet("GetById/{userId}")]
        public async Task<ActionResult<UserAppDto>> GetUserById(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("UserId parameter is required");
            }

            var data = await _userService.GetUserByIdAsync(userId);
            
            if (data.Data == null)
            {
                return NotFound();
            }

            return data.Data;
        }

        // Kullanıcı adına göre kullanıcıları getirir (UserName ile başlayan)
        [HttpGet("GetAllUsersWithName/{name?}")]
        public async Task<List<UserAppDto>> GetAllUsersWithName(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new List<UserAppDto>();
            }

            var result = await _userService.GetAllUsersAsyncWitName(name);
            return result.Data ?? new List<UserAppDto>();
        }

        // Tüm kullanıcıları getirir (fotoğrafsız)
        // includeBlocked: true ise blokeli kullanıcılar da dahil edilir (yönetim için)
        [HttpGet("GetAllWithOuthPhoto")]
        public async Task<List<UserAppDtoWithoutPhoto>> GetAllWithOuthPhoto([FromQuery] bool includeBlocked = false)
        {
            var list = includeBlocked 
                ? await _userService.GetAllUserWithOutPhotoForManagement()
                : await _userService.GetAllUserWithOutPhoto();
            
            var items = list.Data.OrderBy(e => e.FirstName).ToList();

            // Tenant bazlı filtre: header varsa sadece o tenant üyeleri
            if (_tenantContext?.CurrentTenantId != null)
            {
                var allowed = await _userTenantService.GetUsersByTenantAsync(_tenantContext.CurrentTenantId.Value);
                var allowedIds = new HashSet<string>(allowed.Select(x => x.UserId));
                items = items.Where(u => allowedIds.Contains(u.Id)).ToList();
            }

            return items;
        }

        // Backward compatibility için eski endpoint (yönetim için)
        [HttpGet("GetAllWithOuthPhotoForManagement")]
        public async Task<List<UserAppDtoWithoutPhoto>> GetAllWithOuthPhotoForManagement()
        {
            return await GetAllWithOuthPhoto(includeBlocked: true);
        }



        #region Giriş yapan kullanıcı veya şifre işlemleri
        // JWT token'dan kullanıcı bilgilerini getirir
        [HttpGet("GetCurrentUserFromToken")]
        public ActionResult<LoginUserDto> GetCurrentUserFromToken()
        {

            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();



            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { error = "Bearer token is missing or invalid." });
            }

            // "Bearer " kısmını çıkararak token'ı al
            var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            try
            {
                // JWT token çözümleme
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(accessToken))
                {
                    return BadRequest(new { error = "Invalid token format." });
                }

                var jwtToken = handler.ReadJwtToken(accessToken);

                // Token'dan veri çıkarma
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                               ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
                var expiration = jwtToken.ValidTo;

                return new LoginUserDto { Email = email, UserName = username };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while decoding the token.", details = ex.Message });
            }
        }

        [HttpGet("GetLoginUserDetail")]

        public async Task<ActionResult<UserAppDto>> GetLoginUserDetail()
        {
            string userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return NotFound();
            }

            // "Bearer " kısmını çıkararak token'ı al
            var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            try
            {
                // JWT token çözümleme
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(accessToken))
                {
                    return NotFound();
                }

                var jwtToken = handler.ReadJwtToken(accessToken);

                // Token'dan veri çıkarma
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                               ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;


                if (username == null)
                {

                    username = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                        ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                }

                var data = await _userService.GetUserByNameAsync(username);

                return data.Data;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while decoding the token.", details = ex.Message });
            }
        }
        [HttpGet("GetLoginUser")]

        public async Task<ActionResult<UserAppDtoOnlyNameId>> GetLoginUser()
        {
            string userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return NotFound();
            }

            // "Bearer " kısmını çıkararak token'ı al
            var accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            try
            {
                // JWT token çözümleme
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(accessToken))
                {
                    return NotFound();
                }

                var jwtToken = handler.ReadJwtToken(accessToken);

                // Token'dan veri çıkarma
                var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value
                               ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;


                if (username == null)
                {

                    username = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                        ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                }

                var data = await _userService.GetUserByNameAsync(username);
                var result = new UserAppDtoOnlyNameId
                {
                    Id = data.Data.Id,
                    FirstName = data.Data.FirstName,
                    LastName = data.Data.LastName,
                    UserName = data.Data.UserName,
                };
                return result;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while decoding the token.", details = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("GetJwtBearerOptionsAsString")]
        public string GetJwtBearerOptionsAsString(IConfiguration configuration)
        {
            var authority = $"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}";
            var validIssuer = configuration["AzureAd:Issuer"];
            var validAudience = configuration["AzureAd:Audience"];

            // Ayarları bir string formatında oluştur
            return $@"
                Authority: {authority}
                ValidIssuer: {validIssuer}
                ValidAudience: {validAudience}
                Token Validation Parameters:
                - ValidateIssuer: true
                - ValidateAudience: true
                - ValidateLifetime: true
                ";
        }

        [HttpGet("CheckEmail")]
        public async Task<IActionResult> CheckEmailExists2(string email)
        {
            var emailCheck = await _userService.CheckEmailExistsAsync(email);
            return Ok(emailCheck);
        }

        [HttpGet("validatetokenAndUser")]
        public async Task<IActionResult> validatetokenAndUser()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;


            return Ok(userId);
        }

        [AllowAnonymous]
        [HttpGet("CheckSSOEmailControl")]
        public async Task<ActionResult<UserAppDto>> GetUCheckSSOEmailControlser(string mail)
        {
            var data = await _userService.GetUserByEmailAsync(mail);
            if (data.Data != null)
            {
                await _userService.SetLoginDate(data.Data.UserName);
                return data.Data;
            }
            else
            {
                return null;
            }
        }

        [HttpGet("ResetPassWord")]
        public async Task<IActionResult> ResetPassWord(string userName, string passWord, bool? isSendMail)
        {

            if (isSendMail == true)
            {
                var user = _userManager.Users.Where(e => e.Email == userName).Select(e => new { e.Id, e.FirstName, e.LastName, e.Email }).FirstOrDefault();
                CreateMailUserDto createMailUserDto = new CreateMailUserDto()
                {
                    Email = userName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Password = passWord
                };
                SendUserPasswordResetInfoMail(createMailUserDto, new List<string> { userName }, "Ticket Sisteminizde ki Şifreniz Güncellendi");

            }


            var result = await _userService.ResetPassword(userName, passWord);

            return Ok(result);

        }
        private async Task SendUserPasswordResetInfoMail(CreateMailUserDto dto, List<string> toList, string subject)
        {
            string dbName = _dbNameHelper.GetDatabaseName();
            string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Şifre Güncellemesi</title>
</head>
<body style=""margin:0; padding:0; font-family: Arial, sans-serif; background-color:#f4f7fc;"">
     <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#f4f7fc; padding:20px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""800"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#ffffff; box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);"">
                    <!-- HEADER -->
                    <td style=""background-color:white;"">
                        <table style=""width: 100%; table-layout: fixed; display: inline-table;"">
                            <tr>
                                <!-- Logo ve Başlık birlikte yer alacak -->

                                    <td style=""width: 25%; padding:12px;"">
                                            <img src=""{formneoLogo.Logo}"" alt=""Logo"" style=""width: 100%; max-width: 150px; height: auto; display: block;"">
                                    </td>
                                    <td style=""width: 75%; padding:12px;"">
                                            <img src=""{formneoLogo.ColorImg}"" alt=""Logo"" style=""width: 100%; max-width: 600px; height: auto; display: block;"">
                                    </td>
                            </tr>
                        </table>
                    </td>

                    <!-- CONTENT -->
                    <tr>
                        <td style=""padding:20px;"">
                            <h2 style=""font-size:18px; margin-bottom:10px;"">Merhaba {dto.FirstName} {dto.LastName},</h2>
                            <p style=""font-size:14px; margin-bottom:10px;"">Admin tarafından kullanıcınızın şifresi güncellenmiştir.</p>
                            <p style=""font-size:14px; margin-bottom:10px;"">Kullanıcı Adınız : {dto.Email}</p>
                            <p style=""font-size:14px; margin-bottom:10px;"">Şifreniz : <strong>{dto.Password} </strong></p>
                            <p style=""font-size:14px; margin-bottom:10px;"">Önemli :<strong>Lütfen en kısa zamanda şifrenizi değiştirin. </strong></p>
                            <p style=""font-size:14px; margin-bottom:10px;"">Giriş yapabilmek için lütfen <a href=""https://support.formneo-tech.com/"">Tıklayınız</a></p>
                        </td>
                    </tr>
                    <!-- FOOTER -->
                    <tr>
                        <td style=""background-color:#f4f7fc; padding:15px; text-align:center; font-size:12px; color:#555;"">
                            Bu e-posta otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız. {dbName}
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
            utils.Utils.SendMail($"{subject}", emailBody, toList, null, false);
        }

            private async Task SendUserInfoMail(CreateMailUserDto dto, List<string> toList, string subject)
        {
            string dbName = _dbNameHelper.GetDatabaseName();
            string emailBody = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='UTF-8'>
                <title>Yeni Kullanıcı Oluşturmak</title>
            </head>
            <body style=""margin:0; padding:0; font-family: Arial, sans-serif; background-color:#f4f7fc;"">
                 <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#f4f7fc; padding:20px;"">
                    <tr>
                        <td align=""center"">
                            <table role=""presentation"" width=""800"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#ffffff; box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);"">
                                <!-- HEADER -->
                                <td style=""background-color:white;"">
                                    <table style=""width: 100%; table-layout: fixed; display: inline-table;"">
                                        <tr>
                                            <!-- Logo ve Başlık birlikte yer alacak -->
                                            <td style=""background-color: white; padding:12px; width: auto;"">
                                                <img src=""{formneoLogo.Logo}"" alt=""Logo"" width=""100"" height=""60"" style=""display: block; width: 100%; height: auto;"">
                                            </td>
                                            <td style=""background-color: white; padding:12px; width: auto;"">
                                                <img src=""{formneoLogo.ColorImg}"" alt=""Logo"" width=""650"" height=""20"" style=""display: block; width: 100%; height: auto;"">
                                            </td>
                                        </tr>
                                    </table>
                                </td>

                                <!-- CONTENT -->
                                <tr>
                                    <td style=""padding:20px;"">
                                        <h2 style=""font-size:18px; margin-bottom:10px;"">Merhaba {dto.FirstName} {dto.LastName},</h2>
                                        <p style=""font-size:14px; margin-bottom:10px;"">Kullanıcınız oluşturulmuştur.</p>
                                        <p style=""font-size:14px; margin-bottom:10px;"">Kullanıcı Adınız : {dto.Email}</p>
                                        <p style=""font-size:14px; margin-bottom:10px;"">Şifreniz : <strong>{dto.Password} </strong></p>
                                        <p style=""font-size:14px; margin-bottom:10px;"">Giriş yapabilmek için lütfen <a href=""https://support.formneo-tech.com/"">Tıklayınız</a></p>
                                    </td>
                                </tr>
                                <!-- FOOTER -->
                                <tr>
                                    <td style=""background-color:#f4f7fc; padding:15px; text-align:center; font-size:12px; color:#555;"">
                                        Bu e-posta otomatik olarak oluşturulmuştur, lütfen yanıtlamayınız. {dbName}
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>";
            utils.Utils.SendMail($"{subject}", emailBody, toList, null, true);
        }

        #endregion

        // Giriş yapan kullanıcının temel bilgilerini getirir
        [HttpGet("GetCurrentUserBasic")]
        public async Task<ActionResult<UserAppDto>> GetCurrentUserBasic()
        {
            var loginName = User.Identity.Name;

            var loginUser = await _userManager.Users
                .Where(e => e.Email == loginName)
                .Select(e => new { e.Id, e.FirstName, e.LastName })
                .FirstOrDefaultAsync();

            if (loginUser == null)
            {
                return NotFound("User not found");
            }

            var sendData = new UserAppDto
            {
                Id = loginUser.Id,
                FirstName = loginUser.FirstName,
                LastName = loginUser.LastName
            };
            return Ok(sendData);
        }

        // Backward compatibility için eski endpoint
        [HttpGet("UserCompany")]
        public async Task<ActionResult<UserAppDto>> GetUserUserCompany()
        {
            return await GetCurrentUserBasic();
        }

        // Kullanıcının aktif organizasyon bilgisini getirir
        [HttpGet("GetCurrentUserActiveOrganization/{userId?}")]
        public async Task<ActionResult<EmployeeAssignmentListDto>> GetCurrentUserActiveOrganization(string userId = null)
        {
            string targetUserId;
            
            if (string.IsNullOrWhiteSpace(userId))
            {
                // userId verilmemişse, giriş yapan kullanıcının bilgilerini getir
                var loginName = User.Identity.Name;
                if (string.IsNullOrEmpty(loginName))
                {
                    return Unauthorized("User not authenticated");
                }

                var loginUser = await _userManager.Users
                    .Where(e => e.Email == loginName)
                    .FirstOrDefaultAsync();

                if (loginUser == null)
                {
                    return NotFound("User not found");
                }

                targetUserId = loginUser.Id;
            }
            else
            {
                // userId verilmişse, o kullanıcının bilgilerini getir
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                targetUserId = userId;
            }

            // Aktif EmployeeAssignment'ı bul (tüm navigation property'lerle birlikte)
            var assignmentsQuery = await _employeeAssignmentService.Include();
            var now = DateTime.UtcNow;
            var activeAssignment = await assignmentsQuery
                .Where(ea => ea.UserId == targetUserId
                    && ea.AssignmentType == formneo.core.Models.AssignmentType.Primary
                    && ea.StartDate <= now
                    && (ea.EndDate == null || ea.EndDate > now))
                .Include(ea => ea.User)
                .Include(ea => ea.OrgUnit)
                .Include(ea => ea.Position)
                .Include(ea => ea.Manager)
                .OrderByDescending(ea => ea.StartDate)
                .FirstOrDefaultAsync();

            if (activeAssignment == null)
            {
                return NotFound("Active organization assignment not found");
            }

            var result = _mapper.Map<EmployeeAssignmentListDto>(activeAssignment);
            return Ok(result);
        }

        // Kullanıcının tüm organizasyon geçmişini getirir
        [HttpGet("GetCurrentUserOrganizationHistory/{userId?}")]
        public async Task<ActionResult<List<EmployeeAssignmentListDto>>> GetCurrentUserOrganizationHistory(string userId = null)
        {
            string targetUserId;
            
            if (string.IsNullOrWhiteSpace(userId))
            {
                // userId verilmemişse, giriş yapan kullanıcının bilgilerini getir
                var loginName = User.Identity.Name;
                if (string.IsNullOrEmpty(loginName))
                {
                    return Unauthorized("User not authenticated");
                }

                var loginUser = await _userManager.Users
                    .Where(e => e.Email == loginName)
                    .FirstOrDefaultAsync();

                if (loginUser == null)
                {
                    return NotFound("User not found");
                }

                targetUserId = loginUser.Id;
            }
            else
            {
                // userId verilmişse, o kullanıcının bilgilerini getir
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                targetUserId = userId;
            }

            // Tüm EmployeeAssignment'ları getir (geçmiş ve aktif)
            var assignmentsQuery = await _employeeAssignmentService.Include();
            var allAssignments = await assignmentsQuery
                .Where(ea => ea.UserId == targetUserId)
                .Include(ea => ea.User)
                .Include(ea => ea.OrgUnit)
                .Include(ea => ea.Position)
                .Include(ea => ea.Manager)
                .OrderByDescending(ea => ea.StartDate) // En yeni önce
                .ToListAsync();

            var result = _mapper.Map<List<EmployeeAssignmentListDto>>(allAssignments);
            return Ok(result);
        }

        // Kullanıcı seviyelerini getirir
        [HttpGet("UserLevels")]
        public IActionResult GetUserLevels()
        {
            var values = Enum.GetValues(typeof(UserLevel))
                             .Cast<UserLevel>()
                             .Select(e => new
                             {
                                 Id = (int)e,
                                 Name = e.ToString(),
                                 Description = e.GetDescription()
                             });

            return Ok(values);
        }

        // Giriş yapan kullanıcının talep yönetiminde default filtreleri isteyip istemediği kontrol edilir
        [HttpGet("CheckApplyDefaultFilters")]
        public async Task<ActionResult<bool>> CheckApplyDefaultFilters()
        {
            var loginName = User.Identity.Name;
            //var loginUser = await _userService.GetUserByEmailAsync(loginName);
            var loginUser = _userManager.Users.Where(e => e.Email == loginName).Select(e => new { e.Id, e.FirstName, e.LastName }).FirstOrDefault();

            if (loginUser == null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı" });
            }

            bool dontApply = false;
            if (_tenantContext?.CurrentTenantId != null)
            {
                var ut = await _userTenantService.GetByUserAndTenantAsync(loginUser.Id, _tenantContext.CurrentTenantId.Value);
                dontApply = ut != null && ut.DontApplyDefaultFilters;
            }
            return dontApply;
        }

        // Giriş yapan kullanıcının admin olup olmadığı kontrol edilir
        [HttpGet("checkIsAdmin")]
        public async Task<ActionResult<bool>> CheckIsAdmin()
        {
            var loginName = User.Identity.Name;
            //var loginUser = await _userService.GetUserByEmailAsync(loginName);
            var isAdmin = _userManager.Users.Where(e => e.Email == loginName).Select(e => e.isSystemAdmin).FirstOrDefault();
            return isAdmin;
        }

        // Belirtilen rol Id'si global admin ise kullanıcı bu rolde mi?
        [HttpGet("is-global-admin")]
        public async Task<ActionResult<bool>> IsGlobalAdmin([FromQuery] string userId)
        {
            const string globalAdminRoleId = "ed3a94a5-1586-41f8-9906-a0053b6de848";

            UserApp user;
            if (string.IsNullOrWhiteSpace(userId))
            {
                var loginName = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(loginName)) return false;
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginName);
            }
            else
            {
                user = await _userManager.FindByIdAsync(userId);
            }

            if (user == null) return false;

            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == globalAdminRoleId);
            if (role == null) return false;
            var inRole = await _userManager.IsInRoleAsync(user, role.Name!.Trim());
            return inRole;
        }

    }
}