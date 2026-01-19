using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLayer.Core.Services;
using formneo.core.DTOs;
using formneo.core.DTOs.FormAuth;
using formneo.core.DTOs.Menu;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.Options;
using System.Linq;
using formneo.core.Models.FormEnums;

namespace formneo.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MenuController : CustomBaseController
    {
        private readonly IGlobalServiceWithDto<AspNetRolesMenu, RoleMenuListDto> _roleMenuService;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserApp> _userManager;
        private readonly IGlobalServiceWithDto<Menu, MenuListDto> _menuService;
        private readonly IServiceWithDto<FormRuleEngine, FormRuleEngineDto> _formRuleEngineService;
        private readonly IFormService _formservice;
        private readonly IMemoryCache _memoryCache;

        private readonly IFormRepository _formRepository;
        private readonly IServiceWithDto<FormAuth, FormAuthDto> _formAuthService;
        private readonly IUserService _userService;
        private readonly ITenantContext _tenantContext;
        private readonly IRoleTenantMenuService _roleTenantMenuService;
        private readonly IUserTenantRoleService _userTenantRoleService;
        private readonly IUserTenantService _userTenantService;
        private readonly IOptions<RoleScopeOptions> _roleScopeOptions;
        private readonly IRoleTenantFormService _roleTenantFormService;
        private readonly IUserTenantFormRoleService _userTenantFormRoleService;

        public MenuController(IMapper mapper, IGlobalServiceWithDto<Menu, MenuListDto> Service, IServiceWithDto<FormRuleEngine, FormRuleEngineDto> formRuleEngineService,
            IFormService formService, IGlobalServiceWithDto<AspNetRolesMenu, RoleMenuListDto> roleMenuService, IMemoryCache memoryCache, RoleManager<IdentityRole> roleManager, UserManager<UserApp> userManager, IFormRepository formRepository,
            IServiceWithDto<FormAuth, FormAuthDto> formAuthService, IUserService userService, ITenantContext tenantContext, IRoleTenantMenuService roleTenantMenuService, IUserTenantRoleService userTenantRoleService, IUserTenantService userTenantService, IOptions<RoleScopeOptions> roleScopeOptions, IRoleTenantFormService roleTenantFormService, IUserTenantFormRoleService userTenantFormRoleService)
        {

            _menuService = Service;
            _mapper = mapper;
            _formRuleEngineService = formRuleEngineService;
            _formservice = formService;
            _roleManager = roleManager;
            _roleMenuService = roleMenuService;
            _memoryCache = memoryCache;
            _userManager = userManager;

            _formRepository = formRepository;
            _formAuthService = formAuthService;
            _userService = userService;
            _tenantContext = tenantContext;
            _roleTenantMenuService = roleTenantMenuService;
            _userTenantRoleService = userTenantRoleService;
            _userTenantService = userTenantService;
            _roleScopeOptions = roleScopeOptions;
            _roleTenantFormService = roleTenantFormService;
            _userTenantFormRoleService = userTenantFormRoleService;
        }
        private async Task<bool> IsCurrentUserGlobalAdminAsync()
        {
            try
            {
                var userName = User?.Identity?.Name;
                if (string.IsNullOrWhiteSpace(userName)) return false;

                // Cache by user id if possible
                UserApp user = await _userManager.FindByNameAsync(userName);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(userName);
                }
                if (user == null) return false;

                var cacheKey = $"global-admin:{user.Id}";
                if (_memoryCache.TryGetValue(cacheKey, out bool cachedIsGlobal))
                {
                    return cachedIsGlobal;
                }

                var roleNames = await _userManager.GetRolesAsync(user);
                if (roleNames == null || roleNames.Count == 0) return false;

                var roleIds = _roleManager.Roles.Where(r => roleNames.Contains(r.Name)).Select(r => r.Id).ToList();
                var globalOnlyRoleIds = _roleScopeOptions?.Value?.GlobalOnlyRoleIds ?? new List<string>();
                bool isGlobalAdmin = roleIds.Any(id => globalOnlyRoleIds.Contains(id));

                _memoryCache.Set(cacheKey, isGlobalAdmin, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)));
                return isGlobalAdmin;
            }
            catch
            {
                return false;
            }
        }

        private void ClearAllMenuCaches()
        {
            try
            {
                // Tüm kullanıcıların menü cache'lerini temizle
                // Bu işlem biraz maliyetli olabilir ama menü değişiklikleri nadiren olur
                var allUsers = _userManager.Users.ToList();
                foreach (var user in allUsers)
                {
                    // Global admin cache'ini temizle
                    _memoryCache.Remove($"global-admin:{user.Id}");
                    
                    // Eski format menü cache'ini temizle (RoleMenuController'dan)
                    _memoryCache.Remove($"{user.UserName}menus");
                    
                    // Kullanıcının tüm tenant'ları için menü cache'ini temizle
                    var userTenants = _userTenantService.GetByUserAsync(user.Id).Result;
                    foreach (var userTenant in userTenants)
                    {
                        var cacheKey = $"{user.UserName}:{userTenant.TenantId}:menus";
                        _memoryCache.Remove(cacheKey);
                    }
                }
            }
            catch (Exception ex)
            {
                // Cache temizleme hatası log'lanabilir ama ana işlemi etkilememeli
                // Log hatası burada eklenebilir
            }
        }
        [HttpGet]
        public async Task<List<Menu>> All()
        {
            var isGlobalAdmin = await IsCurrentUserGlobalAdminAsync();

            var menus = await _menuService.Include();

            var menuList = menus.ToList();


            var rootMenus = menuList.Where(m => m.ParentMenuId == null && m.IsDelete == false).Select(m => new Menu
            {
                Id = m.Id,
                Name = m.Name,
                ParentMenuId = m.ParentMenuId,
                Order = m.Order,
                Href = m.Href,
                Icon = m.Icon,
                IsDelete = m.IsDelete,
                MenuCode = m.MenuCode,
                IsTenantOnly = m.IsTenantOnly,
                IsGlobalOnly = m.IsGlobalOnly
            }).ToList().OrderBy(e => e.Order);

            // Alt menüleri ekleyelim
            foreach (var menu in rootMenus)
            {
                //AddSubMenus(menu, menus.ToList().OrderBy(e => e.Order).ToList());
                AddSubMenus(menu, menuList.OrderBy(e => e.Order).ToList());
            }

            var rootMenusList = rootMenus.ToList();


            // Global mod (tenant header yok/boş) ise ve kullanıcı global admin ise tüm menüyü döndür
            if (isGlobalAdmin && (_tenantContext?.CurrentTenantId == null || _tenantContext.CurrentTenantId == Guid.Empty))
            {
                // Tenant-only işaretli kök dalları gizle
                var filtered = rootMenusList.Where(r => r.IsTenantOnly == false).ToList();
                return filtered;
            }
       
            var data = await GetAuthByUser();
            var authorizedMenuIds = new HashSet<Guid>((data ?? new List<Menu>()).Select(d => d.Id));

            var filteredMenus = rootMenusList
               .Select(menu =>
               {
                   // Yetkili olan submenu'leri filtreliyoruz (Menu ID bazlı)
                   var subs = menu.SubMenus ?? new List<Menu>();
                   menu.SubMenus = subs
                       .Where(sub => sub != null && authorizedMenuIds.Contains(sub.Id))
                       .ToList();

                   return menu;
               })
               .Where(menu => menu.SubMenus != null && menu.SubMenus.Any())
               .ToList();

            return filteredMenus;

        }


        [HttpGet("AllListData")]
        public async Task<List<MenuListDto>> AllListData()
        {
            var isGlobalAdmin = await IsCurrentUserGlobalAdminAsync();
            
            // Global moddaysa ve kullanıcı global admin ise tüm listeyi döndür
            if (isGlobalAdmin && (_tenantContext?.CurrentTenantId == null || _tenantContext.CurrentTenantId == Guid.Empty))
            {
                // Sadece global menüleri çek (tenant only olmayanlar)
                var menusResponse = await _menuService.Where(e => e.IsDelete == false && e.IsTenantOnly == false);
                var menus = menusResponse.Data.ToList();
                // Gereksiz alanları temizle
                return CleanMenuTree(menus);
            }

            // Yetkili menüleri al (GetAuthByUser zaten cache'li ve optimize edilmiş)
            var data = await GetAuthByUser();
            var authorizedMenuIds = new HashSet<Guid>((data ?? new List<Menu>()).Select(d => d.Id));
            
            // Sadece yetkili menüleri çek (gereksiz veri çekimini önle)
            var authorizedMenusResponse = await _menuService.Where(e => e.IsDelete == false && authorizedMenuIds.Contains(e.Id));
            var authorizedMenus = authorizedMenusResponse.Data.ToList();
            
            // Parent menü kontrolü: Eğer bir alt menüye yetki varsa, parent menüsünü de ekle
            var parentMenuIds = new HashSet<Guid>();
            foreach (var menu in authorizedMenus)
            {
                if (menu.ParentMenuId.HasValue)
                {
                    parentMenuIds.Add(menu.ParentMenuId.Value);
                }
            }
            
            // Parent menüleri de listeye ekle (eğer zaten listede yoksa)
            // Sadece eksik parent menüleri çek
            var missingParentIds = parentMenuIds.Where(id => !authorizedMenus.Any(am => am.Id == id)).ToList();
            if (missingParentIds.Count > 0)
            {
                var parentMenusResponse = await _menuService.Where(m => m.IsDelete == false && missingParentIds.Contains(m.Id));
                var parentMenus = parentMenusResponse.Data.ToList();
                authorizedMenus.AddRange(parentMenus);
            }

            // Kullanıcının form rollerine göre form menülerini ekle ve her zaman üst "Formlar" menüsünü dahil et
            var tenantId = _tenantContext?.CurrentTenantId;
            if (tenantId.HasValue && tenantId.Value != Guid.Empty)
            {
                var formMenus = await GetAuthorizedFormMenusForCurrentUserAsync();
                if (formMenus.Count > 0)
                {
                    var existingIds = new HashSet<Guid>(authorizedMenus.Select(x => x.Id));
                    foreach (var fm in formMenus)
                    {
                        if (!existingIds.Contains(fm.Id))
                        {
                            authorizedMenus.Add(fm);
                            existingIds.Add(fm.Id);
                        }
                    }
                }
            }

            // Tekilleştir
            authorizedMenus = authorizedMenus
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .ToList();

            // FORMS_GENERIC_ROOT'u bul ve ParentMenuId'sini null yap (root yap)
            var genericRootMenu = authorizedMenus.FirstOrDefault(m => m.MenuCode == "FORMS_GENERIC_ROOT");
            if (genericRootMenu != null)
            {
                genericRootMenu.ParentMenuId = null;
                
                // FORMS_ROOT'u bul ve ona bağlı formları FORMS_GENERIC_ROOT'a taşı
                var formsRootMenu = authorizedMenus.FirstOrDefault(m => m.MenuCode == "FORMS_ROOT");
                if (formsRootMenu != null)
                {
                    // FORMS_ROOT'un altındaki tüm form menülerini FORMS_GENERIC_ROOT'un altına taşı
                    foreach (var menu in authorizedMenus.Where(m => m.ParentMenuId == formsRootMenu.Id))
                    {
                        menu.ParentMenuId = genericRootMenu.Id;
                    }
                    // FORMS_ROOT'u listeden çıkar
                    authorizedMenus = authorizedMenus.Where(m => m.MenuCode != "FORMS_ROOT").ToList();
                }
            }

            // Ağaç yapısına dönüştür (kökler + SubMenus)
            var tree = BuildMenuTree(authorizedMenus);
            
            // Gereksiz alanları temizle (performans için)
            return CleanMenuTree(tree);
        }
        
        // Gereksiz alanları temizleyen helper metod (sadece CreatedAt ve UpdatedAt exclude edilir)
        private List<MenuListDto> CleanMenuTree(List<MenuListDto> menus)
        {
            return menus.Select(m => new MenuListDto
            {
                Id = m.Id,
                MenuCode = m.MenuCode,
                ParentMenuId = m.ParentMenuId,
                Name = m.Name,
                Route = m.Route,
                Href = m.Href,
                Icon = m.Icon,
                Order = m.Order,
                IsActive = m.IsActive,
                ShowMenu = m.ShowMenu,
                IsTenantOnly = m.IsTenantOnly,
                IsGlobalOnly = m.IsGlobalOnly,
                Description = m.Description,
                SubMenus = m.SubMenus != null && m.SubMenus.Any() ? CleanMenuTree(m.SubMenus.ToList()) : null,
                // Exclude edilen alanlar: CreatedAt, UpdatedAt
            }).ToList();
        }

        /// <summary>
        /// Sadece ana menüleri döndürür (root menus, ParentMenuId == null olanlar)
        /// AllListData ile aynı yetki kontrolünü kullanır, sadece root menüleri döndürür
        /// </summary>
        [HttpGet("RootMenus")]
        public async Task<List<MenuListDto>> GetRootMenus()
        {
            var isGlobalAdmin = await IsCurrentUserGlobalAdminAsync();
            
            // Global moddaysa ve kullanıcı global admin ise
            if (isGlobalAdmin && (_tenantContext?.CurrentTenantId == null || _tenantContext.CurrentTenantId == Guid.Empty))
            {
                // Sadece global root menüleri çek
                var menusResponse = await _menuService.Where(e => e.IsDelete == false && e.IsTenantOnly == false && e.ParentMenuId == null);
                var menus = menusResponse.Data.ToList();
                // RootMenus için SubMenus'ları kaldır
                return menus.Select(m => new MenuListDto
                {
                    Id = m.Id,
                    MenuCode = m.MenuCode,
                    ParentMenuId = m.ParentMenuId,
                    Name = m.Name,
                    Route = m.Route,
                    Href = m.Href,
                    Icon = m.Icon,
                    Order = m.Order,
                    IsActive = m.IsActive,
                    ShowMenu = m.ShowMenu,
                    IsTenantOnly = m.IsTenantOnly,
                    IsGlobalOnly = m.IsGlobalOnly,
                    Description = m.Description,
                    SubMenus = null // RootMenus için SubMenus döndürülmez
                }).ToList();
            }

            // Yetkili menüleri al (AllListData ile aynı mantık)
            var data = await GetAuthByUser();
            var authorizedMenuIds = new HashSet<Guid>((data ?? new List<Menu>()).Select(d => d.Id));
            
            // Sadece yetkili menüleri çek (AllListData ile aynı)
            var authorizedMenusResponse = await _menuService.Where(e => e.IsDelete == false && authorizedMenuIds.Contains(e.Id));
            var authorizedMenus = authorizedMenusResponse.Data.ToList();
            
            // Parent menü kontrolü: Eğer bir alt menüye yetki varsa, parent menüsünü de ekle (AllListData ile aynı)
            var parentMenuIds = new HashSet<Guid>();
            foreach (var menu in authorizedMenus)
            {
                if (menu.ParentMenuId.HasValue)
                {
                    parentMenuIds.Add(menu.ParentMenuId.Value);
                }
            }
            
            // Parent menüleri de listeye ekle (eğer zaten listede yoksa)
            var missingParentIds = parentMenuIds.Where(id => !authorizedMenus.Any(am => am.Id == id)).ToList();
            if (missingParentIds.Count > 0)
            {
                var parentMenusResponse = await _menuService.Where(m => m.IsDelete == false && missingParentIds.Contains(m.Id));
                var parentMenus = parentMenusResponse.Data.ToList();
                authorizedMenus.AddRange(parentMenus);
            }

            // Kullanıcının form rollerine göre form menülerini ekle (AllListData ile aynı)
            var tenantId = _tenantContext?.CurrentTenantId;
            if (tenantId.HasValue && tenantId.Value != Guid.Empty)
            {
                var formMenus = await GetAuthorizedFormMenusForCurrentUserAsync();
                if (formMenus.Count > 0)
                {
                    var existingIds = new HashSet<Guid>(authorizedMenus.Select(x => x.Id));
                    foreach (var fm in formMenus)
                    {
                        if (!existingIds.Contains(fm.Id))
                        {
                            authorizedMenus.Add(fm);
                            existingIds.Add(fm.Id);
                        }
                    }
                }
            }

            // Tekilleştir
            authorizedMenus = authorizedMenus
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .ToList();

            // FORMS_GENERIC_ROOT'u bul ve ParentMenuId'sini null yap (root yap) - AllListData ile aynı
            var genericRootMenu = authorizedMenus.FirstOrDefault(m => m.MenuCode == "FORMS_GENERIC_ROOT");
            if (genericRootMenu != null)
            {
                genericRootMenu.ParentMenuId = null;
                
                // FORMS_ROOT'u bul ve ona bağlı formları FORMS_GENERIC_ROOT'a taşı
                var formsRootMenu = authorizedMenus.FirstOrDefault(m => m.MenuCode == "FORMS_ROOT");
                if (formsRootMenu != null)
                {
                    // FORMS_ROOT'un altındaki tüm form menülerini FORMS_GENERIC_ROOT'un altına taşı
                    foreach (var menu in authorizedMenus.Where(m => m.ParentMenuId == formsRootMenu.Id))
                    {
                        menu.ParentMenuId = genericRootMenu.Id;
                    }
                    // FORMS_ROOT'u listeden çıkar
                    authorizedMenus = authorizedMenus.Where(m => m.MenuCode != "FORMS_ROOT").ToList();
                }
            }

            // Sadece root menüleri filtrele (ParentMenuId == null)
            var rootMenus = authorizedMenus
                .Where(m => m.ParentMenuId == null)
                .OrderBy(m => m.Order)
                .ToList();

            // RootMenus için SubMenus'ları kaldır (sadece root menüler dönecek)
            return rootMenus.Select(m => new MenuListDto
            {
                Id = m.Id,
                MenuCode = m.MenuCode,
                ParentMenuId = m.ParentMenuId,
                Name = m.Name,
                Route = m.Route,
                Href = m.Href,
                Icon = m.Icon,
                Order = m.Order,
                IsActive = m.IsActive,
                ShowMenu = m.ShowMenu,
                IsTenantOnly = m.IsTenantOnly,
                IsGlobalOnly = m.IsGlobalOnly,
                Description = m.Description,
                SubMenus = null // RootMenus için SubMenus döndürülmez
            }).ToList();
        }

        /// <summary>
        /// Belirli bir menü ID'sine göre o menünün submenülerini döndürür
        /// </summary>
        [HttpGet("SubMenus/{menuId}")]
        public async Task<List<MenuListDto>> GetSubMenus(Guid menuId)
        {
            var isGlobalAdmin = await IsCurrentUserGlobalAdminAsync();
            
            // Yetkili menüleri al
            var data = await GetAuthByUser();
            var authorizedMenuIds = new HashSet<Guid>((data ?? new List<Menu>()).Select(d => d.Id));
            
            // Belirtilen menü ID'sine sahip menüyü kontrol et (yetkili olmalı)
            var menuResponse = await _menuService.Where(e => e.Id == menuId && e.IsDelete == false);
            var menu = menuResponse.Data.FirstOrDefault();
            
            if (menu == null)
            {
                return new List<MenuListDto>();
            }

            // Global admin değilse, menüye yetkisi olup olmadığını kontrol et
            if (!isGlobalAdmin && !authorizedMenuIds.Contains(menuId))
            {
                return new List<MenuListDto>();
            }

            // Bu menünün submenülerini çek
            var subMenusResponse = await _menuService.Where(e => e.IsDelete == false && e.ParentMenuId == menuId);
            var subMenus = subMenusResponse.Data.ToList();

            // Global admin değilse, sadece yetkili submenüleri filtrele
            if (!isGlobalAdmin)
            {
                subMenus = subMenus.Where(m => authorizedMenuIds.Contains(m.Id)).ToList();
            }

            // Kullanıcının form rollerine göre form menülerini ekle (eğer parent menu FORMS_GENERIC_ROOT ise)
            var tenantId = _tenantContext?.CurrentTenantId;
            if (tenantId.HasValue && tenantId.Value != Guid.Empty && menu.MenuCode == "FORMS_GENERIC_ROOT")
            {
                var formMenus = await GetAuthorizedFormMenusForCurrentUserAsync();
                var formSubMenus = formMenus.Where(fm => fm.ParentMenuId == menuId).ToList();
                if (formSubMenus.Count > 0)
                {
                    var existingIds = new HashSet<Guid>(subMenus.Select(x => x.Id));
                    foreach (var fm in formSubMenus)
                    {
                        if (!existingIds.Contains(fm.Id))
                        {
                            subMenus.Add(fm);
                            existingIds.Add(fm.Id);
                        }
                    }
                }
            }

            // Tekilleştir ve sırala
            subMenus = subMenus
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .OrderBy(m => m.Order)
                .ToList();

            return CleanMenuTree(subMenus);
        }

        // GET: api/Menu/all-without-auth
        [HttpGet("all-without-auth")]
        public async Task<List<Menu>> GetAllMenusWithoutAuth()
        {
            var menus = await _menuService.Include();
            var menuList = menus.ToList();

            var rootMenus = menuList.Where(m => m.ParentMenuId == null && m.IsDelete == false).Select(m => new Menu
            {
                Id = m.Id,
                Name = m.Name,
                ParentMenuId = m.ParentMenuId,
                Order = m.Order,
                Href = m.Href,
                Icon = m.Icon,
                IsDelete = m.IsDelete,
                MenuCode = m.MenuCode,
                IsTenantOnly = m.IsTenantOnly,
            }).ToList().OrderBy(e => e.Order);

            // Alt menüleri ekleyelim
            foreach (var menu in rootMenus)
            {
                AddSubMenus(menu, menuList.OrderBy(e => e.Order).ToList());
            }

            return rootMenus.ToList();
        }

        // Global admin önizleme: Belirli tenant ve kullanıcı için efektif menüyü döndürür
        [HttpGet("effective-preview")]
        public async Task<ActionResult<List<Menu>>> EffectivePreview([FromQuery] Guid tenantId, [FromQuery] string userId)
        {
            var isGlobalAdmin = await IsCurrentUserGlobalAdminAsync();
            if (!isGlobalAdmin)
            {
                return Forbid();
            }
            if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("tenantId ve userId zorunludur.");
            }

            // Tüm menü ağacını hazırla
            var menusQuery = await _menuService.Include();
            var allMenus = menusQuery.ToList();
            var rootMenus = allMenus.Where(m => m.ParentMenuId == null && m.IsDelete == false)
                .Select(m => new Menu
                {
                    Id = m.Id,
                    Name = m.Name,
                    ParentMenuId = m.ParentMenuId,
                    Order = m.Order,
                    Href = m.Href,
                    Icon = m.Icon,
                    IsDelete = m.IsDelete,
                    IsGlobalOnly = m.IsGlobalOnly,
                    IsTenantOnly = m.IsTenantOnly
                })
                .ToList()
                .OrderBy(e => e.Order)
                .ToList();

            foreach (var menu in rootMenus)
            {
                AddSubMenus(menu, allMenus.OrderBy(e => e.Order).ToList());
            }

            // Kullanıcının bu tenant’taki rol kimlikleri
            var userTenantRoles = await _userTenantRoleService.GetByUserAndTenantAsync(userId, tenantId);
            var roleIds = userTenantRoles
                .Where(x => x.IsActive && x.RoleTenant != null && x.RoleTenant.IsActive)
                .Select(x => x.RoleTenant.RoleId)
                .Distinct()
                .ToList();

            if (roleIds.Count == 0)
            {
                return new List<Menu>();
            }

            // Tenant + rol bazlı menü izinlerinden görüntülenebilir olanlar
            var query = await _roleTenantMenuService.Include();
            var allowedMenus = await query
                .Where(x => x.TenantId == tenantId && roleIds.Contains(x.RoleId) && x.CanView)
                .Include(x => x.Menu)
                .Select(x => x.Menu)
                .ToListAsync();

            var authorizedHrefs = new HashSet<string>(
                (allowedMenus ?? new List<Menu>())
                    .Where(m => m != null && !string.IsNullOrEmpty(m.Href))
                    .Select(m => m.Href)
            );

            // Yetkili alt menülerle filtrelenmiş kökler
            var filteredMenus = rootMenus
                .Select(menu =>
                {
                    var subs = menu.SubMenus ?? new List<Menu>();
                    menu.SubMenus = subs
                        .Where(sub => sub != null && !string.IsNullOrEmpty(sub.Href) && authorizedHrefs.Contains(sub.Href))
                        .ToList();
                    return menu;
                })
                .Where(menu => menu.SubMenus != null && menu.SubMenus.Any())
                .ToList();

            return filteredMenus;
        }

        // Yetki kontrolü yapmadan, silinmemiş tüm menü ağacını döner.
        // Rol atama başlangıcında kullanılabilir.
        [HttpGet("all-plain")]
        public async Task<List<MenuListDto>> AllPlain([FromQuery] bool tenantOnly = false)
        {
            // Tüm menüler (IsDelete == false), opsiyonel tenantOnly filtresi
            var menus = _menuService.Where(e => e.IsDelete == false && (!tenantOnly || e.IsTenantOnly)).Result.Data.ToList();
            return menus;
        }

		[HttpGet("tenant-only")]
		public async Task<List<MenuListDto>> TenantOnly()
		{
			var response = await _menuService.Where(e => e.IsDelete == false && e.IsTenantOnly);
			return response.Data.OrderBy(m => m.Order).ToList();
		}

        public static void AddSubMenus(Menu menu, List<Menu> allMenus)
        {
            var subMenus = allMenus.Where(m => m.ParentMenuId == menu.Id && m.IsDelete == false).Select(m => new Menu
            {
                Id = m.Id,
                Name = m.Name,
                ParentMenuId = m.ParentMenuId,
                Order = m.Order,
                Href = m.Href,
                Icon = m.Icon,
                IsDelete = m.IsDelete,
                IsTenantOnly = m.IsTenantOnly,
                IsGlobalOnly = m.IsGlobalOnly
            }).ToList();
            foreach (var subMenu in subMenus)
            {

                if (menu.SubMenus == null)
                    menu.SubMenus = new List<Menu>();

                menu.SubMenus.Add(subMenu);
                // Eğer alt menülerin alt menüleri varsa, onları da ekleyelim
                AddSubMenus(subMenu, allMenus);
            }
        }


        [HttpPost]
        public async Task<ActionResult<MenuListDto>> Save(MenuInsertDto dto)
        {
            var result = await _menuService.AddAsync(_mapper.Map<MenuListDto>(dto));

            // Menü cache'lerini temizle - yeni menü eklendiğinde tüm kullanıcıların menü cache'i geçersiz olur
            ClearAllMenuCaches();

            return result.Data;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var menu = await _menuService.GetByIdGuidAsync(id);

                if (menu == null)
                {

                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Menü bulunamadı"));
                }

                // Alt menüleri olan bir menüyü silmeye çalışıyorsa engelleyelim
                var hasChildren = await _menuService.AnyAsync(x => x.ParentMenuId == id);
                if (hasChildren.Data == true)
                {

                    return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(404, "Alt menüleri olan bir menüyü silemezsiniz"));
                }

                await _menuService.SoftDeleteAsync(id);

                // Menü cache'lerini temizle - menü silindiğinde tüm kullanıcıların menü cache'i geçersiz olur
                ClearAllMenuCaches();

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch (Exception ex)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<MenuListDto> GetById(Guid id)
        {
            try
            {
                var menu = await _menuService.GetByIdGuidAsync(id);


                var menuDto = _mapper.Map<MenuListDto>(menu.Data);
                return menuDto;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(MenuUpdateDto dto)
        {
            try
            {

                var result = await _menuService.UpdateAsync(_mapper.Map<MenuListDto>(dto));

                // Menü cache'lerini temizle - menü güncellendiğinde tüm kullanıcıların menü cache'i geçersiz olur
                ClearAllMenuCaches();

                return CreateActionResult(CustomResponseDto<NoContentDto>.Success(204));
            }
            catch (Exception ex)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(500, ex.Message));
            }
        }


        //public List<MenuListDto> BuildMenuHierarchy(List<Menu> allMenus, Guid? parentMenuId)
        //{
        //    var menuItems = allMenus
        //        .Where(m => m.ParentMenuId == parentMenuId)
        //        .OrderBy(m => m.Order)
        //        .Select(m => new MenuListDto({ Name = m.Name,Route= m.Route, Icon=m.Icon })
        //        {
        //            SubMenu = BuildMenuHierarchy(allMenus, m.Id) // Alt menüleri al
        //        })
        //        .ToList();

        //    return menuItems;
        //}
        [HttpGet("GetAuthByUser")]
        public async Task<List<Menu>> GetAuthByUser()
        {
            var tenantId = _tenantContext.CurrentTenantId;
            if (tenantId == null || tenantId == Guid.Empty)
            {
                // Global mod: sadece global admin'e tam erişim ver
                var isGlobalAdmin = await IsCurrentUserGlobalAdminAsync();
                if (!isGlobalAdmin)
                {
                    return new List<Menu>();
                }

                // User.Identity.Name null ise cache kullanma
                var currentUserName = User.Identity?.Name;
                if (!string.IsNullOrEmpty(currentUserName))
                {
                    var globalCacheKey = $"{currentUserName}:global:menus";
                    if (_memoryCache.TryGetValue(globalCacheKey, out var cachedGlobal))
                    {
                        return cachedGlobal as List<Menu> ?? new List<Menu>();
                    }
                }
                var menusQueryGlobal = await _menuService.Include();
                var allMenusGlobal = menusQueryGlobal
                    .Where(m => m.IsDelete == false && m.IsTenantOnly==false)
                    .ToList();

                // Cache'e kaydet (sadece currentUserName varsa)
                if (!string.IsNullOrEmpty(currentUserName))
                {
                    var globalCacheKey = $"{currentUserName}:global:menus";
                    _memoryCache.Set(globalCacheKey, allMenusGlobal, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1)));
                }
                return allMenusGlobal;
            }
            var cacheKey = $"{User.Identity.Name}:{tenantId}:menus";
            if (_memoryCache.TryGetValue(cacheKey, out var cachedValue))
            {
                return cachedValue as List<Menu> ?? new List<Menu>();
            }
            string userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new List<Menu>();
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new List<Menu>();
            }

            // Kullanıcının aktif tenant'taki rol kimliklerini al (menü yetkileri için)
            var userTenantRoles = await _userTenantRoleService.GetByUserAndTenantAsync(user.Id, tenantId.Value);
            var roleIds = userTenantRoles
                .Where(x => x.IsActive && x.RoleTenant != null && x.RoleTenant.IsActive)
                .Select(x => x.RoleTenant.RoleId)
                .Distinct()
                .ToList();

            var combinedMenus = new List<Menu>();

            if (roleIds.Count > 0)
            {
                // Tenant + rol bazlı menü izinlerinden görüntüleyebilecekleri çek
                var query = await _roleTenantMenuService.Include();
                var allowedMenus = await query
                    .Where(x => x.TenantId == tenantId.Value && roleIds.Contains(x.RoleId) && x.CanView)
                    .AsNoTracking()
                    .Include(x => x.Menu)
                    .Select(x => x.Menu)
                    .ToListAsync();

                // Tekilleştir ve ekle
                var menus = allowedMenus
                    .Where(m => m != null && m.IsDelete == false)
                    .GroupBy(m => m.Id)
                    .Select(g => g.First())
                    .ToList();

                combinedMenus.AddRange(menus);
            }

            // EK: Kullanıcının FormTenantRole yetkilerine göre formları da menü olarak ekle
            var userFormRoles = await _userTenantFormRoleService.GetByUserAsync(user.Id);
            var formRoleIds = userFormRoles
                .Where(x => x.IsActive)
                .Select(x => x.FormTenantRoleId)
                .Distinct()
                .ToList();

            if (formRoleIds.Count > 0)
            {
                // FORMS_GENERIC_ROOT'u (varsa) bul veya oluştur
                var genericRootDtoResp = await _menuService.Where(e => e.MenuCode == "FORMS_GENERIC_ROOT" && e.IsActive && e.IsDelete == false);
                var genericRootDto = genericRootDtoResp.Data.FirstOrDefault();
                var genericRootId = genericRootDto?.Id ?? Guid.NewGuid();

                var genericRoot = new Menu
                {
                    Id = genericRootId,
                    MenuCode = "FORMS_GENERIC_ROOT",
                    ParentMenuId = null,
                    Name = "Formlar",
                    Href = null,
                    Icon = null,
                    IsActive = true,
                    IsDelete = false,
                    IsTenantOnly = true,
                    IsGlobalOnly = false,
                    Order = genericRootDto?.Order ?? 10000,
                    Description = genericRootDto?.Description ?? string.Empty
                };

                // Yetkili formlar: sadece gerekli kolonları proje et (FormDesign/JavaScriptCode çekme)
                var formPermQuery = await _roleTenantFormService.Include();
                var allowedFormInfos = await formPermQuery
                    .Where(x => formRoleIds.Contains(x.FormTenantRoleId) && x.CanView)
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.FormId,
                        x.Form.FormName,
                        x.Form.PublicationStatus,
                        x.Form.IsActive,
                        x.Form.ShowInMenu
                    })
                    .ToListAsync();

                // Yayında + aktif + menüde göster olanları filtrele ve tekilleştir
                var filteredForms = allowedFormInfos
                    .Where(f => f != null && f.PublicationStatus == FormPublicationStatus.Published && f.IsActive == 1)
                    .GroupBy(f => f.FormId)
                    .Select(g => g.First())
                    .ToList();

                if (filteredForms.Count > 0)
                {
                    // Root'u ekle (eğer zaten yoksa eklenmiş sayılacak şekilde)
                    combinedMenus.Add(genericRoot);

                    // Mevcut form menülerini tek seferde al
                    var formMenuCodes = filteredForms.Select(f => $"FORM_{f.FormId}").ToList();
                    var existingMenusResp = await _menuService.Where(m => m.IsDelete == false && formMenuCodes.Contains(m.MenuCode));
                    var existingMenusByCode = (existingMenusResp?.Data ?? new List<MenuListDto>())
                        .GroupBy(m => m.MenuCode)
                        .ToDictionary(g => g.Key, g => g.First());

                    int orderCounter = 1;
                    foreach (var f in filteredForms)
                    {
                        var code = $"FORM_{f.FormId}";
                        existingMenusByCode.TryGetValue(code, out var existingMenuDto);

                        var menuId = existingMenuDto?.Id ?? Guid.NewGuid();
                        var name = existingMenuDto?.Name ?? f.FormName;
                        var href = $"/userForm/{f.FormId}";

                        combinedMenus.Add(new Menu
                        {
                            Id = menuId,
                            MenuCode = code,
                            ParentMenuId = genericRootId,
                            Name = name,
                            Href = href,
                            Icon = null,
                            IsActive = true,
                            IsDelete = false,
                            IsTenantOnly = true,
                            IsGlobalOnly = false,
                            Order = (genericRootDto?.Order ?? 10000) + orderCounter++,
                            Description = string.Empty
                        });
                    }
                }
            }

            // Tekilleştir
            combinedMenus = combinedMenus
                .Where(m => m != null)
                .GroupBy(m => m.Id)
                .Select(g => g.First())
                .ToList();

            // FORMS_GENERIC_ROOT'u bul ve ParentMenuId'sini null yap (root yap)
            var genericRootMenu = combinedMenus.FirstOrDefault(m => m.MenuCode == "FORMS_GENERIC_ROOT");
            if (genericRootMenu != null)
            {
                genericRootMenu.ParentMenuId = null;
                
                // FORMS_ROOT'u bul ve ona bağlı formları FORMS_GENERIC_ROOT'a taşı
                var formsRootMenu = combinedMenus.FirstOrDefault(m => m.MenuCode == "FORMS_ROOT");
                if (formsRootMenu != null)
                {
                    // FORMS_ROOT'un altındaki tüm form menülerini FORMS_GENERIC_ROOT'un altına taşı
                    foreach (var menu in combinedMenus.Where(m => m.ParentMenuId == formsRootMenu.Id))
                    {
                        menu.ParentMenuId = genericRootMenu.Id;
                    }
                    // FORMS_ROOT'u listeden çıkar
                    combinedMenus = combinedMenus.Where(m => m.MenuCode != "FORMS_ROOT").ToList();
                }
            }

            // Dinamik form menüleri için üst-alt ilişkisini kur (normal menülerdeki gibi)
            var rootMenusForForms = combinedMenus.Where(m => m.ParentMenuId == null && m.IsDelete == false).ToList();
            foreach (var rootMenu in rootMenusForForms)
            {
                AddSubMenus(rootMenu, combinedMenus.OrderBy(e => e.Order).ToList());
            }

            // Cache'e yaz
            _memoryCache.Set(cacheKey, combinedMenus, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1)));
            return combinedMenus;
        }

        private async Task<List<MenuListDto>> GetAuthorizedFormMenusForCurrentUserAsync()
        {
            var result = new List<MenuListDto>();
            var tenantId = _tenantContext?.CurrentTenantId;
            if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            {
                return result;
            }

            string userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            if (string.IsNullOrWhiteSpace(userName))
            {
                return result;
            }

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return result;
            }

            // Kullanıcının atanmış form tenant rollerini al
            var userFormRoles = await _userTenantFormRoleService.GetByUserAsync(user.Id);
            var roleIds = userFormRoles
                .Where(x => x.IsActive)
                .Select(x => x.FormTenantRoleId)
                .Distinct()
                .ToList();

            // Her durumda üst menüyü eklemek için root'u hazırla
            var genericRootMenuDto = await TryGetFormsGenericRootAsync() ?? new MenuListDto
            {
                Id = new Guid("78681502-ac05-4a53-8b88-dc5b1231d3bb"),
                MenuCode = "FORMS_GENERIC_ROOT",
                ParentMenuId = null,
                Name = "Formlar",
                Href = "/userFormList",
                Icon = null,
                IsActive = true,
                ShowMenu = true,
                IsTenantOnly = true,
                IsGlobalOnly = false,
                Order = 10000,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Description = string.Empty,
                SubMenus = null
            };
 
            // Her zaman üst menüyü ekle
            result.Add(genericRootMenuDto);
 
            if (roleIds.Count == 0)
            {
                return result;
            }
 
            // Bu roller için görüntülenebilir formlar
            var query = await _roleTenantFormService.Include();
            var allowedFormInfos = await query
                .Where(x => roleIds.Contains(x.FormTenantRoleId) && x.CanView)
                .AsNoTracking()
                .Select(x => new
                {
                    x.FormId,
                    x.Form.FormName,
                    x.Form.PublicationStatus,
                    x.Form.IsActive,
                    x.Form.ShowInMenu
                })
                .ToListAsync();

            if (allowedFormInfos == null || allowedFormInfos.Count == 0)
            {
                return result;
            }

            // Yayında/aktif formlar (ShowInMenu filtrelenmedi ise üstteki değişiklikle uyumlu kalır)
            var filteredForms = allowedFormInfos
                .Where(f => f != null && f.PublicationStatus == FormPublicationStatus.Published && f.IsActive == 1)
                .GroupBy(f => f.FormId)
                .Select(g => g.First())
                .ToList();

            if (filteredForms.Count == 0)
            {
                return result;
            }

            // Mevcut menüleri tek sorguda çek
            var formCodes = filteredForms.Select(f => $"FORM_{f.FormId}").ToList();
            var existingMenusResp = await _menuService.Where(m => m.IsDelete == false && formCodes.Contains(m.MenuCode));
            var existingByCode = (existingMenusResp?.Data ?? new List<MenuListDto>())
                .GroupBy(m => m.MenuCode)
                .ToDictionary(g => g.Key, g => g.First());

            int orderCounter = 1;
            foreach (var f in filteredForms)
            {
                var code = $"FORM_{f.FormId}";
                existingByCode.TryGetValue(code, out var existing);
                if (existing != null)
                {
                    existing.ParentMenuId = genericRootMenuDto.Id;
                    existing.Route = $"/userForm/{f.FormId}";
                    existing.Href = $"/userForm/{f.FormId}";
                    existing.IsTenantOnly = true;
                    existing.Order = genericRootMenuDto.Order + orderCounter++;
                    result.Add(existing);
                    continue;
                }

                var child = new MenuListDto
                {
                    Id = Guid.NewGuid(),
                    MenuCode = code,
                    ParentMenuId = genericRootMenuDto.Id,
                    Name = f.FormName,
                    Route = $"/userForm/{f.FormId}",
                    Href = $"/userForm/{f.FormId}",
                    Icon = null,
                    IsActive = true,
                    ShowMenu = true,
                    IsTenantOnly = true,
                    IsGlobalOnly = false,
                    Order = genericRootMenuDto.Order + orderCounter++,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Description = string.Empty,
                    SubMenus = null
                };
                result.Add(child);
            }
 
            result = result.GroupBy(x => x.Id).Select(g => g.First()).ToList();
            
            // FORMS_GENERIC_ROOT'u bul ve ParentMenuId'sini null yap (root yap)
            var genericRootMenu = result.FirstOrDefault(m => m.MenuCode == "FORMS_GENERIC_ROOT");
            if (genericRootMenu != null)
            {
                genericRootMenu.ParentMenuId = null;
                
                // FORMS_ROOT'u bul ve ona bağlı formları FORMS_GENERIC_ROOT'a taşı
                var formsRootMenu = result.FirstOrDefault(m => m.MenuCode == "FORMS_ROOT");
                if (formsRootMenu != null)
                {
                    // FORMS_ROOT'un altındaki tüm form menülerini FORMS_GENERIC_ROOT'un altına taşı
                    foreach (var menu in result.Where(m => m.ParentMenuId == formsRootMenu.Id))
                    {
                        menu.ParentMenuId = genericRootMenu.Id;
                    }
                    // FORMS_ROOT'u listeden çıkar
                    result = result.Where(m => m.MenuCode != "FORMS_ROOT").ToList();
                }
            }
            
            return result;
        }
 
        private async Task<MenuListDto?> TryGetFormsRootAsync()
        {
            var existingRootResp = await _menuService.Where(e => e.MenuCode == "FORMS_ROOT" && e.ParentMenuId == null);
            return existingRootResp?.Data?.FirstOrDefault();
        }

        private async Task<MenuListDto?> TryGetFormsGenericRootAsync()
        {
            var existingRootResp = await _menuService.Where(e => e.MenuCode == "FORMS_GENERIC_ROOT" && e.IsActive && e.IsDelete == false);
            return existingRootResp?.Data?.FirstOrDefault();
        }

        private static List<MenuListDto> BuildMenuTree(List<MenuListDto> flat)
        {
            var byId = flat.ToDictionary(m => m.Id, m => new MenuListDto
            {
                Id = m.Id,
                MenuCode = m.MenuCode,
                ParentMenuId = m.ParentMenuId,
                SubMenus = new List<MenuListDto>(),
                Name = m.Name,
                Route = m.Route,
                Href = (m.MenuCode == "FORMS_ROOT" || m.MenuCode == "FORMS_GENERIC_ROOT") ? null : m.Href,
                Icon = m.Icon,
                IsActive = m.IsActive,
                Order = m.Order,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                Description = m.Description,
                ShowMenu = m.ShowMenu,
                IsTenantOnly = m.IsTenantOnly,
                IsGlobalOnly = m.IsGlobalOnly
            });

            var roots = new List<MenuListDto>();
            foreach (var item in byId.Values)
            {
                if (item.ParentMenuId.HasValue && byId.TryGetValue(item.ParentMenuId.Value, out var parent))
                {
                    (parent.SubMenus ??= new List<MenuListDto>()).Add(item);
                }
                else
                {
                    roots.Add(item);
                }
            }

            // Sıralama
            void SortTree(IEnumerable<MenuListDto> nodes)
            {
                foreach (var n in nodes)
                {
                    if (n.SubMenus != null && n.SubMenus.Count > 0)
                    {
                        n.SubMenus = n.SubMenus.OrderBy(x => x.Order).ToList();
                        SortTree(n.SubMenus);
                    }
                }
            }

            roots = roots.OrderBy(r => r.Order).ToList();
            SortTree(roots);
            return roots;
        }
    }
}