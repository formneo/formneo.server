using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models;

namespace formneo.core.Services
{
    public interface IUserService
    {

        Task<CustomResponseDto<UserAppDto>> UpdateUserAsync(UpdateUserDto createUserDto);
        Task<CustomResponseDto<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<CustomResponseDto<UserAppDto>> GetUserByNameAsync(string userName);
        Task<CustomResponseDto<UserAppDto>> GetUserByIdAsync(string userId);
        Task<CustomResponseDto<List<UserAppDto>>> GetAllUsersAsync();

        Task<CustomResponseDto<List<UserAppDtoWithoutPhoto>>> GetAllUserWithOutPhoto();
        Task<CustomResponseDto<List<UserAppDtoWithoutPhoto>>> GetAllUserWithOutPhotoForManagement();
        Task<CustomResponseDto<List<UserAppDtoOnlyNameId>>> GetAllUsersNameIdOnly();

        Task<CustomResponseDto<List<UserAppDto>>> GetAllUsersAsyncWitName(string name);
        Task<CustomResponseDto<List<UserAppDto>>> GetAllUsersAsyncWitNameCompany(string name, List<string> companies);



        Task<CustomResponseDto<NoContentDto>> RemoveUserAsync(string userName);
        Task<bool> CheckEmailExistsAsync(string email);

        Task<CustomResponseDto<UserAppDto>> GetUserByEmailAsync(string userName);


        Task<bool> SetLoginDate(string userName);


        Task<bool> ResetPassword(string userName, string newPassword);

    }
}
