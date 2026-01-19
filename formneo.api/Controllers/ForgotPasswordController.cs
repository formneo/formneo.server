using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using formneo.api.Helper;
using formneo.core.DTOs;
using formneo.core.DTOs.Ticket.Tickets;
using formneo.core.Models.TaskManagement;
using formneo.core.Services;

namespace formneo.api.Controllers
{
    public class ForgotPasswordController : CustomBaseController
    {
        private readonly IUserService _userService;
        private readonly DbNameHelper _dbNameHelper;

        public ForgotPasswordController(IUserService userService, DbNameHelper dbNameHelper)
        {
            _userService = userService;
            _dbNameHelper = dbNameHelper;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string mail)
        {
            var user = await _userService.GetUserByEmailAsync(mail);
            if (user.Data == null)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Kullanıcı Bulunamadı..!"));
            }

            var resetCode = new Random().Next(100000, 999999).ToString();
            var resetCodeExpiry = DateTime.UtcNow.AddMinutes(10);

            var updateDto = new UpdateUserDto
            {
                Id = user.Data.Id,
                Company = user.Data.Company,
                UserName = user.Data.UserName,
                Email = user.Data.Email,
                Password = user.Data.Password,
                FirstName = user.Data.FirstName,
                LastName = user.Data.LastName,
                isSystemAdmin = user.Data.isSystemAdmin,
                isBlocked = user.Data.isBlocked,
                vacationMode = user.Data.vacationMode,
                LastLoginDate = user.Data.LastLoginDate,
                LastLoginIp = user.Data.LastLoginIp,
                canSsoLogin = user.Data.canSsoLogin,
                profileInfo = user.Data.profileInfo,
                photo = user.Data.photo,
                PhoneNumber = user.Data.PhoneNumber,
                Location = user.Data.Location,
                FacebookUrl = user.Data.FacebookUrl,
                InstagramUrl = user.Data.InstagramUrl,
                TwitterUrl = user.Data.TwitterUrl,
                LinkedinUrl = user.Data.LinkedinUrl,
                Title = user.Data.Title,
                RoleIds = user.Data.Roles,

                ResetPasswordCode = resetCode,
                ResetCodeExpiry = resetCodeExpiry
            };


            await _userService.UpdateUserAsync(updateDto);

            List<string> tolist = new List<string>();
            tolist.Add(mail);

            await SendMail(resetCode, tolist, "Şifre Sıfırlama Talebi");

            return Ok(new { message = "Şifre sıfırlama kodu e-posta adresinize gönderildi." });
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode(string mail, string code)
        {
            var user = await _userService.GetUserByEmailAsync(mail);
            if (user.Data == null || user.Data.ResetPasswordCode != code)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Geçersiz kod."));

            if (user.Data.ResetCodeExpiry < DateTime.UtcNow)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Kodun süresi dolmuştur."));

            return Ok(new { message = "Kod doğrulandı, yeni şifre belirleyebilirsiniz." });
        }

        [HttpPost("change-pw")]
        public async Task<IActionResult> ChangePassword(string mail, string code,string password)
        {
            var user = await _userService.GetUserByEmailAsync(mail);
            if (user.Data == null || user.Data.ResetPasswordCode != code)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Geçersiz kod."));

            if (user.Data.ResetCodeExpiry < DateTime.UtcNow)
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Kodun süresi dolmuştur."));


            var result = await _userService.ResetPassword(mail, password);

            if (result)
            {
                return Ok(new { message = "Kod doğrulandı, yeni şifre belirleyebilirsiniz." });
            }
            else
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, "Şifre değiştirilirken bir hata oluştu."));
            }
        }




        private async Task SendMail(string resetCode, List<string> tolist, string subject)
        {
            string dbName = _dbNameHelper.GetDatabaseName();

            string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Şifre Sıfırlama</title>
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
                            <h2 style=""font-size:18px; margin-bottom:10px;"">{subject}</h2>
                            <h1 style=""font-size:14px; margin-bottom:10px;"">Şifrenizi sıfırlamak için aşağıdaki kodu kullanabilirsiniz:</h1>
                            <h1 style=""color:#0073e6; margin: 0; padding:10px;"">{resetCode}</h1>
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


            utils.Utils.SendMail(subject, emailBody, tolist);

        }

    }


}
