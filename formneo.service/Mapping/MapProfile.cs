using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core;
using formneo.core.DTOs;
using formneo.core.DTOs.Budget.BudgetAdminUser;
using formneo.core.DTOs.Budget.JobCodeRequest;
using formneo.core.DTOs.Budget.NormCodeRequest;
using formneo.core.DTOs.Budget.PeriodUserDto;
using formneo.core.DTOs.Budget.SF;
using formneo.core.DTOs.Clients;
using formneo.core.DTOs.Company;
using formneo.core.Models.CRM;
using formneo.core.DTOs.DepartmentUserDto;
using formneo.core.DTOs.FormAssign;
using formneo.core.DTOs.FormAuth;
using formneo.core.DTOs.FormDatas;
using formneo.core.DTOs.Inventory;
using formneo.core.DTOs.Kanban;
using formneo.core.DTOs.Menu;
using formneo.core.DTOs.OrgUnits;
using formneo.core.DTOs.EmployeeAssignments;
using formneo.core.DTOs.PCTrack;
using formneo.core.DTOs.Plants;
using formneo.core.DTOs.PositionsDtos;
using formneo.core.DTOs.ProjectDtos;
using formneo.core.DTOs.ProjectTasks;
using formneo.core.DTOs.TaskManagement;
using formneo.core.DTOs.UserTenants;
using formneo.core.DTOs.Ticket;
using formneo.core.DTOs.Ticket.TicketAssigne;
using formneo.core.DTOs.Ticket.TicketComment;
using formneo.core.DTOs.Ticket.TicketDepartments;
using formneo.core.DTOs.Ticket.TicketNotifications;
using formneo.core.DTOs.Ticket.TicketRuleEngine;
using formneo.core.DTOs.Ticket.Tickets;
using formneo.core.DTOs.Ticket.TicketTeams;
using formneo.core.DTOs.TicketProjects;
using formneo.core.GenericListDto;
using formneo.core.Models;
using formneo.core.Models.BudgetManagement;
using formneo.core.Models.Inventory;
using formneo.core.Models.NewFolder;
using formneo.core.Models.PCTracking;
using formneo.core.Models.TaskManagement;
using formneo.core.Models.Ticket;
using formneo.core.Models.CRM;
using formneo.core.DTOs.CRM;
using formneo.core.DTOs.Lookup;
using formneo.core.Models.Lookup;
using formneo.core.DTOs.RoleForm;

namespace formneo.service.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<EmpSalary, EmpSalaryDto>().ReverseMap();
            CreateMap<Employee, EmployeeDto>().ReverseMap();
            CreateMap<Departments, DepartmentsUpdateDto>().ReverseMap();
            CreateMap<Departments, DepartmentsInsertDto>().ReverseMap();
            CreateMap<Departments, DepartmentsListDto>().ReverseMap();


            CreateMap<UserApp, UserAppDto>()
	            // NOT: OrgUnit ve WorkCompany kaldırıldı
	            // OrgUnitName artık EmployeeAssignment tablosundan alınmalı
	            .ReverseMap();

            CreateMap<UserApp, CreateUserDto>().ReverseMap();
            CreateMap<UserApp, UpdateUserDto>().ReverseMap();
            CreateMap<WorkFlowDefination, WorkFlowDefinationDto>().ReverseMap();
            CreateMap<WorkFlowDefination, WorkFlowDefinationDto>().ReverseMap();
            
            // ApproveItems mapping - Navigation property'den email al
            CreateMap<ApproveItems, ApproveItemsDto>()
                .ForMember(dest => dest.ApproveUser, opt => opt.MapFrom(src => 
                    src.ApproveUser != null ? src.ApproveUser.Email ?? src.ApproveUser.UserName : src.ApproveUserId))
                .ForMember(dest => dest.ApprovedUser_Runtime, opt => opt.MapFrom(src => 
                    src.ApprovedUser_Runtime != null ? src.ApprovedUser_Runtime.Email ?? src.ApprovedUser_Runtime.UserName : src.ApprovedUser_RuntimeId))
                .ReverseMap()
                .ForMember(dest => dest.ApproveUser, opt => opt.Ignore())  // Navigation property'yi ignore et
                .ForMember(dest => dest.ApprovedUser_Runtime, opt => opt.Ignore());  // Navigation property'yi ignore et
            
            // FormItems mapping - Navigation property'den email al
            CreateMap<FormItems, FormItemsDto>()
                .ForMember(dest => dest.FormUser, opt => opt.MapFrom(src => 
                    src.FormUser != null ? src.FormUser.Email ?? src.FormUser.UserName : src.FormUserId))
                .ForMember(dest => dest.WorkFlowHead, opt => opt.MapFrom(src => src.WorkflowItem != null ? src.WorkflowItem.WorkflowHead : null))
                .ReverseMap()
                .ForMember(dest => dest.FormUser, opt => opt.Ignore());  // Navigation property'yi ignore et
            CreateMap<WorkflowHead, WorkFlowHeadDto>().ReverseMap();
            CreateMap<Form, FormDataListDto>().ReverseMap();
            CreateMap<FormRuleEngineDto, FormRuleEngine>().ReverseMap();
            CreateMap<FormRuntimeDto, FormRuntime>().ReverseMap();
            CreateMap<FormRuntimeDto, FormRuntimeSubmission>().ReverseMap();

            CreateMap<MainClient, MainClientInsertDto>().ReverseMap();
            CreateMap<MainClient, MainClientListDto>().ReverseMap();
            CreateMap<MainClient, MainClientUpdateDto>().ReverseMap();

            CreateMap<Company, CompanyListDto>().ReverseMap();
            CreateMap<Company, CompanyInsertDto>().ReverseMap();
            CreateMap<Company, CompanyUpdateDto>().ReverseMap();

            CreateMap<Plant, PlantListDto>().ReverseMap();
            CreateMap<Plant, PlantInsertDto>().ReverseMap();
            CreateMap<Plant, PlantUpdateDto>().ReverseMap();

            CreateMap<Form, FormDataInsertDto>().ReverseMap();
            CreateMap<Form, FormDataUpdateDto>().ReverseMap();
            CreateMap<Form, FormDataListDto>().ReverseMap();
            CreateMap<Form, FormDataGetByIdDto>().ReverseMap();

            CreateMap<Project, GetProjectListDto>().ReverseMap();
            CreateMap<Project, CreateProjectDto>().ReverseMap();
            CreateMap<Project, UpdateProjectDto>().ReverseMap();

            CreateMap<Positions, UpdatePositionDto>().ReverseMap();
            CreateMap<Positions, CreatePositionDto>().ReverseMap();
            CreateMap<Positions, PositionListDto>().ReverseMap();


            CreateMap<UserAppDtoWithoutPhoto, UserApp>()
                // NOT: OrgUnit kaldırıldı
                .ReverseMap();
            CreateMap<UserAppDtoWithoutPhoto, UserAppDto>().ReverseMap();


            CreateMap<UserAppDtoWithoutPhoto, support_UsersGenericList>()
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.externalCode, opt => opt.MapFrom(src => src.UserName)).ReverseMap();

            CreateMap<UserAppDto, support_UsersGenericList>()
            .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.externalCode, opt => opt.MapFrom(src => src.UserName)).ReverseMap();

            #region Budget

            CreateMap<BudgetPeriodListDto, BudgetPeriodInsertDto>().ReverseMap();
            CreateMap<BudgetPeriodListDto, BudgetPeriodUpdateDto>().ReverseMap();
            CreateMap<BudgetPeriod, BudgetPeriodListDto>().ReverseMap();



            CreateMap<BudgetPeriodUserListDto, BudgetPeriodUserInsertDto>().ReverseMap();
            CreateMap<BudgetPeriodUserListDto, BudgetPeriodUserUpdateDto>().ReverseMap();
            CreateMap<BudgetPeriodUser, BudgetPeriodUserListDto>().ReverseMap();
            CreateMap<BudgetPeriod, BudgetPeriodUserListDto>().ReverseMap();

            CreateMap<BudgetPeriodUser, BudgetPeriodUserListDto>().ReverseMap();

            CreateMap<BudgetJobCodeRequestListDto, BudgetJobCodeRequestInsertDto>().ReverseMap();
            CreateMap<BudgetJobCodeRequestListDto, BudgetJobCodeRequestUpdateDto>().ReverseMap();
            CreateMap<BudgetJobCodeRequest, BudgetJobCodeRequestListDto>().ReverseMap();



            CreateMap<BudgetNormCodeRequestListDto, BudgetNormCodeRequest>().ReverseMap();

            CreateMap<BudgetNormCodeRequestListDto, BudgetNormCodeRequestInsertDto>().ReverseMap();
            CreateMap<BudgetNormCodeRequestListDto, BudgetNormCodeRequestUpdateDto>().ReverseMap();
            CreateMap<BudgetNormCodeRequest, BudgetNormCodeRequestListDto>().ReverseMap();
            CreateMap<BudgetNormCodeRequest, BudgetNormCodeRequestListOnlyCodeDto>().ReverseMap();




            CreateMap<ApproveItemsDto, ApproveItems>().ReverseMap();

            CreateMap<WorkFlowHeadDto, WorkflowHead>().ReverseMap();


            CreateMap<WorkFlowHeadDtoWithoutItems, WorkflowHead>().ReverseMap();
            CreateMap<WorkFlowHeadDtoResultStartOrContinue, WorkflowHead>().ReverseMap();



            CreateMap<WorkFlowItemDto, WorkflowItem>().ReverseMap();

            CreateMap<WorkFlowItemDtoWithApproveItems, WorkflowItem>().ReverseMap();


            CreateMap<WorkFlowDefination, WorkFlowDefinationListDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.WorkflowName, opt => opt.MapFrom(src => src.WorkflowName))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision))
                .ForMember(dest => dest.FormId, opt => opt.MapFrom(src => src.FormId))
                .ReverseMap();
            CreateMap<WorkFlowDefination, WorkFlowDefinationDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.WorkflowName, opt => opt.MapFrom(src => src.WorkflowName))
                .ForMember(dest => dest.Defination, opt => opt.MapFrom(src => src.Defination))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision))
                .ForMember(dest => dest.FormId, opt => opt.MapFrom(src => src.FormId))
                .ReverseMap();
            CreateMap<WorkFlowDefinationInsertDto, WorkFlowDefination>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.MainClientId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDelete, opt => opt.Ignore())
                .ForMember(dest => dest.UniqNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
                .ForMember(dest => dest.workflows, opt => opt.Ignore())
                .ForMember(dest => dest.Form, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<WorkFlowDefinationUpdateDto, WorkFlowDefination>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.MainClientId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDelete, opt => opt.Ignore())
                .ForMember(dest => dest.UniqNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
                .ForMember(dest => dest.workflows, opt => opt.Ignore())
                .ForMember(dest => dest.Form, opt => opt.Ignore())
                .ReverseMap();




            CreateMap<PickListDto, PickListDTO>()
            .ForMember(dest => dest.label_localized, opt => opt.MapFrom(src => src.name))
            .ForMember(dest => dest.externalCode, opt => opt.MapFrom(src => src.externalCode)).ReverseMap();




            CreateMap<BudgetPromotionRequestListDto, BudgetPromotionRequestInsertDto>().ReverseMap();
            CreateMap<BudgetPromotionRequestListDto, BudgetPromotionRequestUpdateDto>().ReverseMap();
            CreateMap<BudgetPromotionRequest, BudgetPromotionRequestListDto>().ReverseMap();




            CreateMap<BudgetAdminUserListDto, BudgetAdminUserInsertDto>().ReverseMap();
            CreateMap<BudgetAdminUserListDto, BudgetAdminUserUpdateDto>().ReverseMap();
            CreateMap<BudgetAdminUser, BudgetAdminUserListDto>().ReverseMap();


            #endregion


            CreateMap<TicketDepartmensListDto, TicketDepartment>().ReverseMap();
            CreateMap<TicketDepartmensListDto, TicketDepartmensInsertDto>().ReverseMap();
            CreateMap<TicketDepartmensListDto, TicketDepartmensUpdateDto>().ReverseMap();

            CreateMap<OrgUnit, OrgUnitListDto>().ReverseMap();
            CreateMap<OrgUnitListDto, OrgUnitInsertDto>().ReverseMap();
            CreateMap<OrgUnitListDto, OrgUnitUpdateDto>().ReverseMap();

            // EmployeeAssignment mappings
            CreateMap<EmployeeAssignment, EmployeeAssignmentListDto>()
                .ForMember(dest => dest.UserFullName,
                    opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : null))
                .ForMember(dest => dest.OrgUnitName,
                    opt => opt.MapFrom(src => src.OrgUnit != null ? src.OrgUnit.Name : null))
                .ForMember(dest => dest.PositionName,
                    opt => opt.MapFrom(src => src.Position != null ? src.Position.Name : null))
                .ForMember(dest => dest.ManagerFullName,
                    opt => opt.MapFrom(src => src.Manager != null ? $"{src.Manager.FirstName} {src.Manager.LastName}".Trim() : null))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.EndDate == null || src.EndDate > DateTime.UtcNow))
                .ForMember(dest => dest.AssignmentTypeText,
                    opt => opt.MapFrom(src => src.AssignmentType == AssignmentType.Primary ? "Ana Atama" :
                        src.AssignmentType == AssignmentType.Secondary ? "İkincil Atama" :
                        src.AssignmentType == AssignmentType.Temporary ? "Geçici Atama" :
                        src.AssignmentType == AssignmentType.Matrix ? "Matrix Atama" :
                        "Bilinmeyen"))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrgUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore());

            CreateMap<EmployeeAssignmentInsertDto, EmployeeAssignment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.MainClientId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDelete, opt => opt.Ignore())
                .ForMember(dest => dest.UniqNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrgUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.MainClient, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.OrgUnitId, opt => opt.MapFrom(src => src.OrgUnitId))
                .ForMember(dest => dest.PositionId, opt => opt.MapFrom(src => src.PositionId))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate ?? DateTime.UtcNow))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.AssignmentType))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

            CreateMap<EmployeeAssignmentUpdateDto, EmployeeAssignment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.MainClientId, opt => opt.Ignore())
                .ForMember(dest => dest.IsDelete, opt => opt.Ignore())
                .ForMember(dest => dest.UniqNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyToken, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrgUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Position, opt => opt.Ignore())
                .ForMember(dest => dest.Manager, opt => opt.Ignore())
                .ForMember(dest => dest.MainClient, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Update'te UserId değiştirilmez
                .ForMember(dest => dest.StartDate, opt => opt.Ignore()) // Controller'da manuel set ediliyor
                .ForMember(dest => dest.OrgUnitId, opt => opt.MapFrom(src => src.OrgUnitId))
                .ForMember(dest => dest.PositionId, opt => opt.MapFrom(src => src.PositionId))
                .ForMember(dest => dest.ManagerId, opt => opt.MapFrom(src => src.ManagerId))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.AssignmentType, opt => opt.MapFrom(src => src.AssignmentType))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));

            CreateMap<EmployeeAssignmentListDto, EmployeeAssignmentInsertDto>().ReverseMap();
            CreateMap<EmployeeAssignmentListDto, EmployeeAssignmentUpdateDto>().ReverseMap();

            CreateMap<DepartmentUserListDto, DepartmentUser>().ReverseMap();
            CreateMap<DepartmentUserListDto, DepartmentUserInsertDto>().ReverseMap();
            CreateMap<DepartmentUserListDto, DepartmentUserUpdateDto>().ReverseMap();

            CreateMap<Menu, MenuListDto>().ReverseMap();
            CreateMap<Menu, MenuUpdateDto>().ReverseMap();
            CreateMap<MenuListDto, MenuInsertDto>().ReverseMap();
            CreateMap<MenuListDto, MenuUpdateDto>().ReverseMap();
            CreateMap<RoleMenuListDto, RoleMenuInsertDto>().ReverseMap();
            CreateMap<RoleMenuListDto, AspNetRolesMenu>().ReverseMap();
            CreateMap<RoleTenantMenuListDto, AspNetRolesTenantMenu>().ReverseMap();
            this.AddRoleFormMappings();
            CreateMap<FormTenantRole, FormTenantRoleListDto>().ReverseMap();
            CreateMap<FormDataListDto, Form>().ReverseMap();
            CreateMap<TicketTeam, TicketTeamListDto>().ReverseMap();
            CreateMap<TicketTeamListDto, TicketTeamInsertDto>().ReverseMap();
            CreateMap<TicketTeamListDto, TicketTeamUpdateDto>().ReverseMap();
            CreateMap<TicketTeamUserAppListDto, TicketTeamUserAppInsertDto>().ReverseMap();
            CreateMap<TicketTeamUserApp, TicketTeamUserAppInsertDto>().ReverseMap();
            CreateMap<WorkCompany, WorkCompanyDto>().ReverseMap();
            CreateMap<WorkCompanyInsertDto, WorkCompanyDto>().ReverseMap();
            CreateMap<WorkCompanyUpdateDto, WorkCompanyDto>().ReverseMap();
            CreateMap<WorkCompanySystemInfo, WorkCompanySystemInfoListDto>().ReverseMap();
            CreateMap<WorkCompanySystemInfoInsertDto, WorkCompanySystemInfoListDto>().ReverseMap();
            CreateMap<WorkCompanySystemInfoUpdateDto, WorkCompanySystemInfoListDto>().ReverseMap();
            CreateMap<WorkCompanyTicketMatris, WorkCompanyTicketMatrisListDto>().ReverseMap();
            CreateMap<WorkCompanyTicketMatrisInsertDto, WorkCompanyTicketMatrisListDto>().ReverseMap();
            CreateMap<WorkCompanyTicketMatrisUpdateDto, WorkCompanyTicketMatrisListDto>().ReverseMap();
            CreateMap<WorkCompanyTicketMatris, WorkCompanyTicketMatrisUpdateDto>().ReverseMap();
            CreateMap<Tickets, TicketInsertDto>().ReverseMap();
            CreateMap<Tickets, TicketListDto>().ReverseMap();
            CreateMap<TicketComment, TicketCommentDto>().ReverseMap();
            CreateMap<TicketComment, TicketCommentInsertDto>().ReverseMap();
            CreateMap<TicketFileDto, TicketFile>().ReverseMap();
            CreateMap<TicketFileInsertDto, TicketFile>().ReverseMap();
            CreateMap<Tickets, TicketUpdateManagerDto>().ReverseMap();
            CreateMap<Tickets, TicketUpdateDto>().ReverseMap();
            CreateMap<TicketFileInsertDto, TicketFile>().ReverseMap();
            CreateMap<TicketAssigneDto, TicketAssigne>().ReverseMap();
            CreateMap<DepartmentUser, DepartmentUserInsertDto>().ReverseMap();


            CreateMap<TicketInsertDto, Tickets>()
                .ForMember(dest => dest.TicketComment, opt => opt.MapFrom(src => src.TicketComment));

            CreateMap<TicketNotifications, TicketNotificationsInsertDto>().ReverseMap();
            CreateMap<TicketNotificationsListDto, TicketNotificationsInsertDto>().ReverseMap();
            CreateMap<TicketNotificationsListDto, TicketNotifications>().ReverseMap();



            CreateMap<TicketRuleEngineListDto, TicketRuleEngine>().ReverseMap();
            CreateMap<TicketRuleEngineInsertDto, TicketRuleEngineListDto>().ReverseMap();
            CreateMap<TicketRuleEngineUpdateDto, TicketRuleEngineListDto>().ReverseMap();



            CreateMap<Positions, PositionListDto>().ReverseMap();
            CreateMap<CreatePositionDto, UpdatePositionDto>().ReverseMap();
            CreateMap<CreatePositionDto, PositionListDto>().ReverseMap();
            CreateMap<UpdatePositionDto, PositionListDto>().ReverseMap();
            CreateMap<UpdatePositionDto, Positions>().ReverseMap();


            CreateMap<UserCalendarListDto, UserCalendar>().ReverseMap();
            CreateMap<UserCalendarInsertDto, UserCalendarListDto>().ReverseMap();
            CreateMap<UserCalendarUpdateDto, UserCalendarListDto>().ReverseMap();


            CreateMap<FormAssignDto, FormAssign>().ReverseMap();

            CreateMap<TicketProjectsListDto, TicketProjects>().ReverseMap();
            CreateMap<TicketProjectsInsertDto, TicketProjects>().ReverseMap();
            CreateMap<TicketProjectsUpdateDto, TicketProjects>().ReverseMap();

            CreateMap<PCTrackDto, PCTrack>().ReverseMap();

            CreateMap<FormAuthDto, FormAuth>().ReverseMap();
            CreateMap<FormAuthInsertDto, FormAuthDto>().ReverseMap();
            CreateMap<FormAuthUpdateDto, FormAuthDto>().ReverseMap();

            CreateMap<ProjectTasks, ProjectTasksListDto>().ReverseMap();
            CreateMap<ProjectTasks, ProjectTasksInsertDto>().ReverseMap();
            CreateMap<ProjectTasks, ProjectTasksUpdateDto>().ReverseMap();

            CreateMap<ProjectCategories, ProjectCategoriesInsertDto>().ReverseMap();
            CreateMap<ProjectCategories, ProjectCategoriesListDto>().ReverseMap();

            CreateMap<Inventory, InventoryListDto>().ReverseMap();
            CreateMap<Inventory, InventoryUpdateDto>().ReverseMap();
            CreateMap<Inventory, InventoryInsertDto>().ReverseMap();
            CreateMap<InventoryListDto, InventoryInsertDto>().ReverseMap();
            CreateMap<InventoryListDto, InventoryUpdateDto>().ReverseMap();

            CreateMap<Kanban, KanbanTasksListDto>().ReverseMap();

            // UserTenant
            CreateMap<UserTenant, UserTenantListDto>().ReverseMap();
            CreateMap<UserTenant, UserTenantInsertDto>().ReverseMap();
            CreateMap<UserTenant, UserTenantUpdateDto>().ReverseMap();

            // RoleTenant
            CreateMap<RoleTenant, formneo.core.DTOs.RoleTenants.RoleTenantListDto>().ReverseMap();
            CreateMap<RoleTenant, formneo.core.DTOs.RoleTenants.RoleTenantInsertDto>().ReverseMap();
            CreateMap<RoleTenant, formneo.core.DTOs.RoleTenants.RoleTenantUpdateDto>().ReverseMap();

            // UserTenantRole
            CreateMap<UserTenantRole, UserTenantRole>().ReverseMap();

			// CRM
			CreateMap<CustomerAddress, CustomerAddressDto>().ReverseMap();
			CreateMap<CustomerAddress, CustomerAddressInsertDto>().ReverseMap();
			CreateMap<CustomerAddress, CustomerAddressUpdateDto>().ReverseMap();

			CreateMap<CustomerOfficial, CustomerOfficialDto>().ReverseMap();

			CreateMap<CustomerEmail, CustomerEmailDto>()
				.ForMember(dest => dest.ConcurrencyToken, opt => opt.MapFrom(src => src.ConcurrencyToken))
				.ReverseMap();
			CreateMap<CustomerEmail, CustomerEmailInsertDto>().ReverseMap();
			CreateMap<CustomerEmail, CustomerEmailUpdateDto>()
				.ForMember(dest => dest.ConcurrencyToken, opt => opt.MapFrom(src => src.ConcurrencyToken))
				.ReverseMap();

			CreateMap<CustomerPhone, CustomerPhoneDto>().ReverseMap();
			CreateMap<CustomerPhone, CustomerPhoneInsertDto>().ReverseMap();
			CreateMap<CustomerPhone, CustomerPhoneUpdateDto>().ReverseMap();

			CreateMap<CustomerNote, CustomerNoteDto>().ReverseMap();
			CreateMap<CustomerNote, CustomerNoteInsertDto>().ReverseMap();
			CreateMap<CustomerNote, CustomerNoteUpdateDto>().ReverseMap();
			CreateMap<formneo.core.Models.CRM.Customer, CustomerListDto>()
				.ForMember(dest => dest.Emails, opt => opt.MapFrom(src => src.SecondaryEmails))
				.ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses))
				.ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.Phones))
				.ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
				.ForMember(dest => dest.Officials, opt => opt.MapFrom(src => src.Officials))
				.ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(t => t.Tag).ToList()))
				.ForMember(dest => dest.Sectors, opt => opt.MapFrom(src => src.Sectors.Select(s => s.Sector).ToList()))
				.ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents.Select(d => d.FilePath).ToList()))
				.ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields))
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
				.ForMember(dest => dest.LegalName, opt => opt.MapFrom(src => src.LegalName))
				.ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
				.ForMember(dest => dest.TaxOffice, opt => opt.MapFrom(src => src.TaxOffice))
				.ForMember(dest => dest.TaxNumber, opt => opt.MapFrom(src => src.TaxNumber))
				.ForMember(dest => dest.IsReferenceCustomer, opt => opt.MapFrom(src => src.IsReferenceCustomer))
				.ForMember(dest => dest.LogoFilePath, opt => opt.MapFrom(src => src.LogoFilePath))
				.ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note))
				.ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
				.ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
				.ForMember(dest => dest.FacebookUrl, opt => opt.MapFrom(src => src.FacebookUrl))
				.ForMember(dest => dest.LinkedinUrl, opt => opt.MapFrom(src => src.LinkedinUrl))
				.ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
				.ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
				.ForMember(dest => dest.LifecycleStage, opt => opt.MapFrom(src => (int)src.LifecycleStage))
				.ForMember(dest => dest.NextActivityDate, opt => opt.MapFrom(src => src.NextActivityDate))
				.ForMember(dest => dest.CustomerTypeId, opt => opt.MapFrom(src => src.CustomerTypeId))
				.ForMember(dest => dest.CustomerTypeText, opt => opt.MapFrom(src => src.CustomerTypeItem != null ? src.CustomerTypeItem.Name : null))
				.ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
				.ForMember(dest => dest.CategoryText, opt => opt.MapFrom(src => src.CategoryItem != null ? src.CategoryItem.Name : null))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Status) ? int.Parse(src.Status) : 0));

			// Insert/Update: DTO -> Entity
			CreateMap<CustomerInsertDto, formneo.core.Models.CRM.Customer>()
				.ForMember(dest => dest.CustomerTypeId, opt => opt.MapFrom(src => src.CustomerTypeId))
				.ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
				.ForMember(dest => dest.LifecycleStage, opt => opt.MapFrom(src => (LifecycleStage)src.LifecycleStage));

			CreateMap<CustomerUpdateDto, formneo.core.Models.CRM.Customer>()
				.ForMember(dest => dest.CustomerTypeId, opt => opt.MapFrom(src => src.CustomerTypeId))
				.ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId));

			CreateMap<CustomerEmail, string>().ConvertUsing(x => x.Email);
			CreateMap<string, CustomerEmail>().ForMember(dest => dest.Email, opt => opt.MapFrom(src => src));

			CreateMap<CustomerTag, string>().ConvertUsing(x => x.Tag);
			CreateMap<string, CustomerTag>().ForMember(dest => dest.Tag, opt => opt.MapFrom(src => src));

			CreateMap<CustomerDocument, string>().ConvertUsing(x => x.FilePath);
			CreateMap<string, CustomerDocument>().ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => src));

			CreateMap<CustomerDocument, CustomerDocumentDto>().ReverseMap();
			CreateMap<CustomerDocument, CustomerDocumentInsertDto>().ReverseMap();
			CreateMap<CustomerDocument, CustomerDocumentUpdateDto>().ReverseMap();

			CreateMap<CustomerSector, string>().ConvertUsing(x => x.Sector);
			CreateMap<string, CustomerSector>().ForMember(dest => dest.Sector, opt => opt.MapFrom(src => src));

			CreateMap<CustomerCustomField, CustomFieldDto>()
				.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.FieldId))
				.ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.FieldType))
				.ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
				.ForMember(dest => dest.ValueJson, opt => opt.MapFrom(src => src.ValueJson))
				.ReverseMap()
				.ForMember(dest => dest.FieldId, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.FieldType, opt => opt.MapFrom(src => src.Type))
				.ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.Label))
				.ForMember(dest => dest.ValueJson, opt => opt.MapFrom(src => src.ValueJson));

			// Performans için optimize edilmiş mapping'ler
			CreateMap<formneo.core.Models.CRM.Customer, CustomerBasicDto>()
				.ForMember(dest => dest.CustomerTypeId, opt => opt.MapFrom(src => src.CustomerTypeId))
				.ForMember(dest => dest.CustomerTypeText,
					opt => opt.MapFrom(src => src.CustomerTypeItem != null ? src.CustomerTypeItem.Name : null))
				.ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
				.ForMember(dest => dest.CategoryText,
					opt => opt.MapFrom(src => src.CategoryItem != null ? src.CategoryItem.Name : null))
				.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
				.ForMember(dest => dest.LegalName, opt => opt.MapFrom(src => src.LegalName))
				.ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
				.ForMember(dest => dest.TaxNumber, opt => opt.MapFrom(src => src.TaxNumber))
				.ForMember(dest => dest.IsReferenceCustomer, opt => opt.MapFrom(src => src.IsReferenceCustomer))
				.ForMember(dest => dest.LogoFilePath, opt => opt.MapFrom(src => src.LogoFilePath))
				.ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
				.ForMember(dest => dest.TwitterUrl, opt => opt.MapFrom(src => src.TwitterUrl))
				.ForMember(dest => dest.FacebookUrl, opt => opt.MapFrom(src => src.FacebookUrl))
				.ForMember(dest => dest.LinkedinUrl, opt => opt.MapFrom(src => src.LinkedinUrl))
				.ForMember(dest => dest.InstagramUrl, opt => opt.MapFrom(src => src.InstagramUrl))
				.ForMember(dest => dest.LifecycleStage, opt => opt.MapFrom(src => (int)src.LifecycleStage))
				.ForMember(dest => dest.NextActivityDate, opt => opt.MapFrom(src => src.NextActivityDate))
				.ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
				.ForMember(dest => dest.Status, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Status) ? int.Parse(src.Status) : 0));
	
			CreateMap<CustomerBasicDto, formneo.core.Models.CRM.Customer>();

			// CRM Extras - Opportunity Mappings
			CreateMap<Opportunity, OpportunityDto>()
				.ForMember(dest => dest.StageText, opt => opt.MapFrom(src => src.Stage.ToString()));
			
			CreateMap<Opportunity, OpportunityListDto>()
				.ForMember(dest => dest.StageText, opt => opt.MapFrom(src => src.Stage.ToString()))
				.ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null));
			
			CreateMap<OpportunityInsertDto, Opportunity>()
				.ForMember(dest => dest.Stage, opt => opt.MapFrom(src => (OpportunityStage)src.Stage));
			
			CreateMap<OpportunityUpdateDto, Opportunity>()
				.ForMember(dest => dest.Stage, opt => opt.MapFrom(src => (OpportunityStage)src.Stage));

			// Return mappings for Create/Update operations
			CreateMap<OpportunityInsertDto, OpportunityDto>()
				.ForMember(dest => dest.Id, opt => opt.Ignore())
				.ForMember(dest => dest.StageText, opt => opt.MapFrom(src => ((OpportunityStage)src.Stage).ToString()))
				.ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
				.ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());


			CreateMap<OpportunityUpdateDto, OpportunityDto>()
				.ForMember(dest => dest.StageText, opt => opt.MapFrom(src => ((OpportunityStage)src.Stage).ToString()))
				.ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
				.ForMember(dest => dest.UpdatedDate, opt => opt.Ignore());

			// Other CRM Mappings
			CreateMap<Activity, ActivityDto>().ReverseMap();
			CreateMap<Meeting, MeetingDto>().ReverseMap();
			CreateMap<Reminder, ReminderDto>().ReverseMap();
			CreateMap<Quote, QuoteDto>().ReverseMap();
			CreateMap<QuoteLine, QuoteLineDto>().ReverseMap();
			CreateMap<SpecialDay, SpecialDayDto>().ReverseMap();
			CreateMap<CrmChangeLog, CrmChangeLogDto>().ReverseMap();

			// Lookup
			CreateMap<TenantLookupCategory, LookupCategoryDto>().ReverseMap();
			CreateMap<TenantLookupItem, LookupItemDto>().ReverseMap();

            // Lookup
            CreateMap<TenantLookupCategory, LookupCategoryDto>();
            CreateMap<LookupCategoryDto, TenantLookupCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<TenantLookupItem, LookupItemDto>().ReverseMap();
            // Tenant lookup mappings removed; Lookup* are tenant-scoped (BaseEntity)
            CreateMap<TenantLookupModule, LookupModuleDto>().ReverseMap();

			// ProjectTask
			CreateMap<formneo.core.Models.ProjectTask, formneo.core.DTOs.ProjectTask.ProjectTaskListDto>()
				.ForMember(d => d.Status, o => o.MapFrom(s => (int)s.Status))
				.ForMember(d => d.StatusText, o => o.MapFrom(s => s.Status.ToString()))
				.ForMember(d => d.AssigneeName, o => o.MapFrom(s => s.Assignee != null ? (s.Assignee.FirstName + " " + s.Assignee.LastName).Trim() : null))
				.ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : null));
			CreateMap<formneo.core.DTOs.ProjectTask.ProjectTaskInsertDto, formneo.core.Models.ProjectTask>()
				.ForMember(d => d.Status, o => o.MapFrom(s => (formneo.core.Models.ProjectTaskStatus)s.Status));
			CreateMap<formneo.core.DTOs.ProjectTask.ProjectTaskUpdateDto, formneo.core.Models.ProjectTask>()
				.ForMember(d => d.Status, o => o.MapFrom(s => (formneo.core.Models.ProjectTaskStatus)s.Status));
        }
    }
}
