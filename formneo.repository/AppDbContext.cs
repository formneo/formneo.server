using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using formneo.core.Models;
using formneo.core.Models.BudgetManagement;
using formneo.core.Models.Inventory;
using formneo.core.Models.NewFolder;
using formneo.core.Models.PCTracking;
using formneo.core.Models.TaskManagement;
using formneo.core.Models.Ticket;
using formneo.core.Models.CRM;
using formneo.core.Models.Lookup;
using formneo.core.Services;
using formneo.core.Models.Security;
using formneo.core.Models.EntityManager;


namespace formneo.repository
{
    public class AppDbContext : IdentityDbContext<UserApp, IdentityRole, string>
    {
        public DbSet<MainClient> MainClients { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Plant> Plant { get; set; }
        public DbSet<BudgetPeriod> BudgetPeriod { get; set; }
        public DbSet<WorkflowItem> WorkflowItem { get; set; }
        public DbSet<ApproveItems> ApproveItems { get; set; }
        public DbSet<FormItems> FormItems { get; set; }
        public DbSet<FormInstance> FormInstance { get; set; }
        public DbSet<WorkflowHead> WorkflowHead { get; set; }
        public DbSet<BudgetJobCodeRequest> BudgetJobCodeRequest { get; set; }
        public DbSet<BudgetNormCodeRequest> BudgetNormCodeRequest { get; set; }
        public DbSet<BudgetPromotionRequest> BudgetPromotionRequest { get; set; }
        public DbSet<UserRefreshToken> UserRefreshToken { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TenantProject> TenantProjects { get; set; }
        public DbSet<Positions> Positions { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<TicketComment> TicketComment { get; set; }
        public DbSet<TicketFile> TicketFile { get; set; }
        public DbSet<TicketNotifications> TicketNotifications { get; set; }

        public DbSet<TicketRuleEngine> TicketRuleEngine { get; set; }

        public DbSet<TicketDepartment> TicketDepartment { get; set; }
        public DbSet<OrgUnit> OrgUnits { get; set; }
        public DbSet<EmployeeAssignment> EmployeeAssignments { get; set; }
        public DbSet<AspNetRolesMenu> AspNetRolesMenu { get; set; }
        public DbSet<AspNetRolesTenantMenu> AspNetRolesTenantMenu { get; set; }
        public DbSet<AspNetRolesTenantForm> AspNetRolesTenantForm { get; set; }
        public DbSet<FormTenantRole> FormTenantRoles { get; set; }
        public DbSet<TicketTeam> TicketTeam { get; set; }
        public DbSet<TicketTeamUserApp> TicketTeamUserApp { get; set; }

        //public DbSet<BudgetAdminUser> BudgetAdminUser { get; set; }
        public DbSet<FormRuleEngine> FormRuleEngine { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<FormRuntime> FormRuntime { get; set; }

        public DbSet<WorkCompany> WorkCompany { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
        public DbSet<RoleTenant> RoleTenants { get; set; }
        public DbSet<UserTenantRole> UserTenantRoles { get; set; }
        public DbSet<UserTenantFormRole> UserTenantFormRoles { get; set; }

        public DbSet<WorkCompanySystemInfo> WorkCompanySystemInfo { get; set; }
        public DbSet<WorkCompanyTicketMatris> WorkCompanyTicketMatris { get; set; }

        public DbSet<DepartmentUser> DepartmentUsers { get; set; }
        public DbSet<FormAssign> FormAssign { get; set; }
        public DbSet<PCTrack> PCTrack { get; set; }
        public DbSet<Kanban> Kanban { get; set; }
        public DbSet<FormAuth> FormAuth { get; set; }
        public DbSet<TicketProjects> TicketProjects { get; set; }
        public DbSet<ProjectTasks> ProjectTasks { get; set; }
        public DbSet<ProjectCategories> ProjectCategories { get; set; }
        public DbSet<ProjectTask> ProjectTask { get; set; }
        public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; }
        public DbSet<ProjectTaskComment> ProjectTaskComments { get; set; }
        public DbSet<ProjectActivityLog> ProjectActivityLogs { get; set; }
        public DbSet<ProjectRelation> ProjectRelations { get; set; }
        public DbSet<Inventory> Inventory { get; set; }

        public DbSet<UserCalendar> UserCalendar { get; set; }
        // Security
        public DbSet<Resource> Resources { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        public DbSet<formneo.core.Models.CRM.Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<CustomerOfficial> CustomerOfficials { get; set; }
        public DbSet<CustomerEmail> CustomerEmails { get; set; }
        public DbSet<CustomerTag> CustomerTags { get; set; }
        public DbSet<CustomerDocument> CustomerDocuments { get; set; }
        public DbSet<CustomerSector> CustomerSectors { get; set; }
        public DbSet<CustomerCustomField> CustomerCustomFields { get; set; }
        public DbSet<CustomerPhone> CustomerPhones { get; set; }
        public DbSet<CustomerNote> CustomerNotes { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteLine> QuoteLines { get; set; }
        public DbSet<SpecialDay> SpecialDays { get; set; }
        public DbSet<CrmChangeLog> CrmChangeLogs { get; set; }
        public DbSet<formneo.core.Models.Onboarding.OnboardingActivation> OnboardingActivations { get; set; }

        // Lookup
        public DbSet<TenantLookupCategory> TenantLookupCategories { get; set; }
        public DbSet<TenantLookupItem> TenantLookupItems { get; set; }
        public DbSet<TenantLookupModule> TenantLookupModules { get; set; }

        // Entity Manager
        public DbSet<FormEntity> FormEntities { get; set; }
        public DbSet<FormEntityField> FormEntityFields { get; set; }
        public DbSet<FormEntityFieldType> FormEntityFieldTypes { get; set; }
        public DbSet<FormEntityRelation> FormEntityRelations { get; set; }
        public DbSet<FormFieldMapping> FormFieldMappings { get; set; }

        //public DbSet<Employee> Employees { get; set; }
        //public DbSet<EmpSalary> EmpSalary { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ITenantContext tenantContext) : base(options)
        {

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _tenantContext = tenantContext;

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tickets>()
    .HasOne(t => t.TicketProject)
    .WithMany()
    .HasForeignKey(t => t.TicketProjectId)
    .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Project>()
            .Property(p => p.Category)
            .HasConversion<int>();

            modelBuilder.Entity<BudgetPeriod>(entity =>
            {
                entity.HasKey(bp => new { bp.PeriodCode });

            });


            modelBuilder.Entity<BudgetJobCodeRequest>(entity =>
            {
                entity.HasKey(bp => new { bp.Id });

            });


            modelBuilder.Entity<BudgetPeriodUser>(entity =>
            {
                entity.HasKey(bpu => bpu.Id);

                entity
                    .HasOne(bpu => bpu.BudgetPeriod)
                    .WithMany(bp => bp.BudgetPeriodUsers)
                    .HasForeignKey(bpu => new { bpu.BudgetPeriodCode });
            });


            modelBuilder.Entity<Tickets>()
                       .Property(e => e.UniqNumber)
                       .ValueGeneratedOnAdd()
                       .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);  // Güncelleme engellenir


           


            // modelBuilder.pro<int>().Where(p => p.Name == "OrderId").Configure(c => c.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity))
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


            modelBuilder.Entity<TicketDepartment>()
            .HasOne(td => td.Manager)
            .WithMany() // UserApp tarafında ters bir koleksiyon tanımlamıyorsanız WithMany boş bırakılır
            .HasForeignKey(td => td.ManagerId)
            .OnDelete(DeleteBehavior.Restrict); // İlişki silinme davranışı

            modelBuilder.Entity<OrgUnit>()
                .HasOne(ou => ou.ParentOrgUnit)
                .WithMany(ou => ou.SubOrgUnits)
                .HasForeignKey(ou => ou.ParentOrgUnitId);

            modelBuilder.Entity<OrgUnit>()
                .HasOne(ou => ou.Manager)
                .WithMany()
                .HasForeignKey(ou => ou.ManagerId);

            // NOT: UserApp.OrgUnitId ve OrgUnit navigation property kaldırıldı
            // Organizasyon bilgileri artık EmployeeAssignment tablosunda tutuluyor
            // Manager bilgisi EmployeeAssignment.ManagerId veya OrgUnit.ManagerId'den alınır


            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.NoAction; // veya DeleteBehavior.SetNull
                }
            }

            // CRM Enum configurations are handled in CustomerConfiguration.cs
            // PostgreSQL için xmin Concurrency Token - En iyi çözüm
            modelBuilder.Entity<formneo.core.Models.CRM.Customer>()
                .UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerAddress>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerEmail>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerTag>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerSector>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerDocument>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerCustomField>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerOfficial>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<CustomerPhone>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<CustomerNote>().UseXminAsConcurrencyToken();
            
            // Diğer CRM entity'leri
            modelBuilder.Entity<formneo.core.Models.CRM.Activity>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<formneo.core.Models.CRM.Meeting>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<formneo.core.Models.CRM.Reminder>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<formneo.core.Models.CRM.Quote>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<formneo.core.Models.CRM.QuoteLine>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<formneo.core.Models.CRM.Opportunity>().UseXminAsConcurrencyToken();
            
            modelBuilder.Entity<formneo.core.Models.CRM.SpecialDay>().UseXminAsConcurrencyToken();

            modelBuilder.Entity<CustomerAddress>()
                .Property(p => p.Type)
                .HasConversion<int>();

            modelBuilder.Entity<CustomerOfficial>()
                .Property(p => p.Role)
                .HasConversion<int>();

            modelBuilder.Entity<Opportunity>()
                .Property(p => p.Stage).HasConversion<int>();
            modelBuilder.Entity<Activity>()
                .Property(p => p.Type).HasConversion<int>();
            modelBuilder.Entity<Activity>()
                .Property(p => p.Status).HasConversion<int>();
            modelBuilder.Entity<Quote>()
                .Property(p => p.Status).HasConversion<int>();

            // Global tenant query filter for all BaseEntity (exclude GlobalBaseEntity and some specific types)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == null) continue;
                if (typeof(GlobalBaseEntity).IsAssignableFrom(clrType)) continue;
                if (!typeof(BaseEntity).IsAssignableFrom(clrType)) continue;
                // UserTenant artık GlobalBaseEntity'den türediği için zaten filtre dışında

                var parameter = System.Linq.Expressions.Expression.Parameter(clrType, "e");
                var isDeleteProp = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDelete));
                var notDeleted = System.Linq.Expressions.Expression.Not(isDeleteProp);

                var mainClientProp = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.MainClientId));
                var ctxConst = System.Linq.Expressions.Expression.Constant(this);
                var tenantProp = System.Linq.Expressions.Expression.Property(ctxConst, nameof(CurrentTenantIdForFilter));
                var equalsTenant = System.Linq.Expressions.Expression.Equal(mainClientProp, tenantProp);

                var body = System.Linq.Expressions.Expression.AndAlso(notDeleted, equalsTenant);

                var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
            

            // Hybrid lookup unique constraints (global + tenant override)
            modelBuilder.Entity<TenantLookupCategory>()
                .HasIndex(c => new { c.ModuleId, c.Key })
                .IsUnique();

            modelBuilder.Entity<TenantLookupItem>()
                .HasIndex(i => new { i.CategoryId, i.Code })
                .IsUnique();

            // Otomatik tenant-aware unique constraints
            ConfigureTenantAwareUniqueConstraints(modelBuilder);
            // Tenant-aware concurrency tokens (UPDATE/DELETE WHERE'e MainClientId eklemek için)
            ConfigureTenantConcurrencyTokens(modelBuilder);
            modelBuilder.Entity<CustomerAddress>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerOfficial>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerEmail>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerTag>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerDocument>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerSector>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerCustomField>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerPhone>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CustomerNote>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<Opportunity>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<Activity>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<Reminder>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<Meeting>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<Quote>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<QuoteLine>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<SpecialDay>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<CrmChangeLog>().HasQueryFilter(e => !e.IsDelete);
            modelBuilder.Entity<formneo.core.Models.Onboarding.OnboardingActivation>().HasQueryFilter(e => !e.IsDelete);

            // Entity Manager Configurations
            ConfigureEntityManagerRelationships(modelBuilder);

            base.OnModelCreating(modelBuilder);






        }

        // Otomatik tenant-aware unique constraints
        private void ConfigureTenantAwareUniqueConstraints(ModelBuilder modelBuilder)
        {
            // BaseEntity'den türeyen tüm tablolardaki unique index'leri tenant-aware yap
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == null) continue;
                if (typeof(GlobalBaseEntity).IsAssignableFrom(clrType)) continue;
                if (!typeof(BaseEntity).IsAssignableFrom(clrType)) continue;

                // Bu entity'deki tüm unique index'leri bul
                var uniqueIndexes = entityType.GetIndexes().Where(i => i.IsUnique).ToList();
                
                foreach (var index in uniqueIndexes)
                {
                    // Eğer MainClientId zaten index'te yoksa, ekle
                    var propertyNames = index.Properties.Select(p => p.Name).ToList();
                    if (!propertyNames.Contains("MainClientId"))
                    {
                        // Eski index'i kaldır
                        entityType.RemoveIndex(index);
                        
                        // Yeni tenant-aware index ekle
                        var newPropertyNames = new[] { "MainClientId" }.Concat(propertyNames).ToArray();
                        modelBuilder.Entity(clrType)
                            .HasIndex(newPropertyNames)
                            .IsUnique();
                    }
                }
            }
        }

        // UPDATE/DELETE WHERE koşullarına MainClientId eklemek için concurrency token ayarı
        private void ConfigureTenantConcurrencyTokens(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == null) continue;
                if (typeof(GlobalBaseEntity).IsAssignableFrom(clrType)) continue;
                if (!typeof(BaseEntity).IsAssignableFrom(clrType)) continue;

                // MainClientId alanı concurrency token olsun
                modelBuilder.Entity(clrType)
                    .Property("MainClientId")
                    .IsConcurrencyToken();
            }
        }

        // Entity Manager relationship configurations
        private void ConfigureEntityManagerRelationships(ModelBuilder modelBuilder)
        {
            // FormEntity self-referencing relationship (Parent-Child)
            modelBuilder.Entity<FormEntity>()
                .HasOne(e => e.ParentEntity)
                .WithMany(e => e.ChildEntities)
                .HasForeignKey(e => e.ParentEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormEntityField -> FormEntity
            modelBuilder.Entity<FormEntityField>()
                .HasOne(f => f.FormEntity)
                .WithMany(e => e.Fields)
                .HasForeignKey(f => f.FormEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormEntityField -> FormEntityFieldType
            modelBuilder.Entity<FormEntityField>()
                .HasOne(f => f.FieldType)
                .WithMany(t => t.Fields)
                .HasForeignKey(f => f.FieldTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormEntityField -> RelatedEntity (for Lookup fields)
            modelBuilder.Entity<FormEntityField>()
                .HasOne(f => f.RelatedEntity)
                .WithMany()
                .HasForeignKey(f => f.RelatedEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormEntityRelation -> Form
            modelBuilder.Entity<FormEntityRelation>()
                .HasOne(r => r.Form)
                .WithMany()
                .HasForeignKey(r => r.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormEntityRelation -> FormEntity
            modelBuilder.Entity<FormEntityRelation>()
                .HasOne(r => r.FormEntity)
                .WithMany(e => e.FormEntityRelations)
                .HasForeignKey(r => r.FormEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormEntityRelation self-referencing (for nested relations)
            modelBuilder.Entity<FormEntityRelation>()
                .HasOne(r => r.ParentRelation)
                .WithMany()
                .HasForeignKey(r => r.ParentRelationId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormFieldMapping -> Form
            modelBuilder.Entity<FormFieldMapping>()
                .HasOne(m => m.Form)
                .WithMany()
                .HasForeignKey(m => m.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormFieldMapping -> FormEntityField
            modelBuilder.Entity<FormFieldMapping>()
                .HasOne(m => m.FormEntityField)
                .WithMany(f => f.FormFieldMappings)
                .HasForeignKey(m => m.FormEntityFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // FormFieldMapping -> FormEntityRelation
            modelBuilder.Entity<FormFieldMapping>()
                .HasOne(m => m.FormEntityRelation)
                .WithMany()
                .HasForeignKey(m => m.FormEntityRelationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            modelBuilder.Entity<FormEntity>()
                .HasIndex(e => e.EntityName);

            modelBuilder.Entity<FormEntityField>()
                .HasIndex(f => new { f.FormEntityId, f.FieldName });

            modelBuilder.Entity<FormEntityFieldType>()
                .HasIndex(t => t.TypeName)
                .IsUnique();

            modelBuilder.Entity<FormEntityRelation>()
                .HasIndex(r => new { r.FormId, r.FormEntityId });

            modelBuilder.Entity<FormFieldMapping>()
                .HasIndex(m => new { m.FormId, m.FormFieldName });

            modelBuilder.Entity<FormFieldMapping>()
                .HasIndex(m => m.FormElementId);

            // Performance indexes for ProcessHub workflow history
            // Index on FormUserId for faster joins with UserApp
            modelBuilder.Entity<FormItems>()
                .HasIndex(f => f.FormUserId);

            // Composite index for active employee assignments lookup
            // Filters by UserId first, then AssignmentType, and checks EndDate for active assignments
            modelBuilder.Entity<EmployeeAssignment>()
                .HasIndex(ea => new { ea.UserId, ea.AssignmentType, ea.EndDate })
                .HasDatabaseName("IX_EmployeeAssignment_ActiveLookup");

            // Additional index for OrgUnit and Position lookups
            modelBuilder.Entity<EmployeeAssignment>()
                .HasIndex(ea => ea.OrgUnitId);

            modelBuilder.Entity<EmployeeAssignment>()
                .HasIndex(ea => ea.PositionId);

            // Performance indexes for Menu table
            // Index on ParentMenuId for hierarchical menu queries (self-referencing FK)
            modelBuilder.Entity<Menu>()
                .HasIndex(m => m.ParentMenuId)
                .HasDatabaseName("IX_Menus_ParentMenuId");

            // Composite index for most common query pattern: root menus with soft delete check and ordering
            // Covers queries like: Where(m => m.ParentMenuId == null && m.IsDelete == false).OrderBy(m => m.Order)
            modelBuilder.Entity<Menu>()
                .HasIndex(m => new { m.IsDelete, m.ParentMenuId, m.Order })
                .HasDatabaseName("IX_Menus_IsDelete_ParentMenuId_Order");

            // Composite index for global admin menu filtering
            // Covers queries like: Where(e => e.IsDelete == false && e.IsTenantOnly == false)
            modelBuilder.Entity<Menu>()
                .HasIndex(m => new { m.IsDelete, m.IsTenantOnly })
                .HasDatabaseName("IX_Menus_IsDelete_IsTenantOnly");

            // Index on MenuCode for lookups and uniqueness checks
            modelBuilder.Entity<Menu>()
                .HasIndex(m => m.MenuCode)
                .HasDatabaseName("IX_Menus_MenuCode");
        }

        // Tenant-aware query filter backing field (reads from ITenantContext)
        private Guid? CurrentTenantIdForFilter => _tenantContext?.CurrentTenantId;
        private Guid ResolveContextIds()
        {
            Guid tenantId = Guid.Empty;

            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;

                string headerTenant = httpContext?.Request?.Headers?["X-Tenant-Id"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(headerTenant) && Guid.TryParse(headerTenant, out var tid)) tenantId = tid;

                // HTTP isteği YOKSA (örn. seed, background job), appsettings fallback uygula.
                // HTTP isteği VARSA ve header boşsa, fallback kullanma: tenant damgası yapılmasın.
                if (tenantId == Guid.Empty && httpContext == null)
                {
                    var cfgTenant = _configuration?["MultiTenancy:DefaultMainClientId"];
                    if (!string.IsNullOrWhiteSpace(cfgTenant) && Guid.TryParse(cfgTenant, out var tid2)) tenantId = tid2;
                }
            }
            catch { }

            return tenantId;
        }

        public override int SaveChanges()
        {
            var tenantId = ResolveContextIds();


            try
            {
                //var currentUserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                foreach (var item in ChangeTracker.Entries())
                {
                    // AsNoTracking ile getirilen ve sonradan Modified olarak attach edilen varlıklar için
                    // OriginalValues boş olduğundan audit diff üretilemez. DB'den mevcut değerleri çekerek doldur.
                    if (item.State == EntityState.Modified)
                    {
                        try
                        {
                            var dbValues = item.GetDatabaseValues();
                            if (dbValues != null)
                            {
                                item.OriginalValues.SetValues(dbValues);
                            }
                        }
                        catch { }
                    }
                    if (item.Entity is BaseEntity entityReference)
                    {
                        switch (item.State)
                        {
                            case EntityState.Added:
                                {
                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";

                                    //var customHeaderValue = _httpContextAccessor.HttpContext?.Request?.Headers["X-Custom-Header"].FirstOrDefault();


                                    Entry(entityReference).Property(x => x.UpdatedBy).IsModified = false;
                                    Entry(entityReference).Property(x => x.UpdatedDate).IsModified = false;




                                    entityReference.CreatedDate = DateTime.Now;
                                    if (_httpContextAccessor?.HttpContext != null && tenantId == Guid.Empty)
                                    {
                                        throw new InvalidOperationException($"Tenant context is required for write operations for entity {item.Entity.GetType().Name}. Provide X-Tenant-Id header.");
                                    }
                                    if (tenantId != Guid.Empty) entityReference.MainClientId = tenantId;
                                    if (entityReference.Id == Guid.Empty) entityReference.Id = Guid.NewGuid();




                                    if (userName != null)
                                        entityReference.CreatedBy = userName;
                                    else
                                        entityReference.CreatedBy = "nulldata";


                                    entityReference.UpdatedBy = "";

                                    // WorkflowHeadId alanını kontrol edip null olarak ayarlama
                                    if (entityReference is BudgetJobCodeRequest budgetJobCodeRequest)
                                    {
                                        budgetJobCodeRequest.WorkflowHeadId = budgetJobCodeRequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : budgetJobCodeRequest.WorkflowHeadId;
                                    }

                                    if (entityReference is BudgetNormCodeRequest normrequest)
                                    {
                                        normrequest.WorkflowHeadId = normrequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : normrequest.WorkflowHeadId;
                                    }


                                    break;
                                }
                            case EntityState.Modified:
                                {

                                    //var customHeaderValue = _httpContextAccessor.HttpContext?.Request?.Headers["X-Custom-Header"].FirstOrDefault();

                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";



                                    if (_httpContextAccessor?.HttpContext != null && tenantId == Guid.Empty)
                                    {
                                        throw new InvalidOperationException($"Tenant context is required for write operations for entity {item.Entity.GetType().Name}. Provide X-Tenant-Id header.");
                                    }
                                    if (tenantId != Guid.Empty) entityReference.MainClientId = tenantId;
                                    // CompanyId şu an kullanılmıyor, boş bırakılabilir
                                    entityReference.UpdatedDate = DateTime.Now;

                                    if (userName != null)
                                        entityReference.UpdatedBy = userName;
                                    else
                                        entityReference.UpdatedBy = "nulldata";


                                    Entry(entityReference).Property(x => x.UniqNumber).IsModified = false;
                                    Entry(entityReference).Property(x => x.CreatedBy).IsModified = false;
                                    Entry(entityReference).Property(x => x.CreatedDate).IsModified = false;

                                    // WorkflowHeadId alanını kontrol edip null olarak ayarlama
                                    if (entityReference is BudgetJobCodeRequest budgetJobCodeRequest)
                                    {
                                        budgetJobCodeRequest.WorkflowHeadId = budgetJobCodeRequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : budgetJobCodeRequest.WorkflowHeadId;
                                    }

                                    if (entityReference is BudgetNormCodeRequest normrequest)
                                    {
                                        normrequest.WorkflowHeadId = normrequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : normrequest.WorkflowHeadId;
                                    }

                                    break;
                                }
                        }
                        // CRM change logging for BaseEntity
                        try
                        {
                            var entityName = item.Entity.GetType().Name;
                            var entityId = entityReference.Id;
                            var changedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";
                            var changedDate = DateTime.Now;

                            if (item.State == EntityState.Added)
                            {
                                CrmChangeLog log = new CrmChangeLog
                                {
                                    EntityName = entityName,
                                    EntityId = entityId,
                                    Action = CrmChangeAction.Added,
                                    PropertyName = "*",
                                    OldValue = null,
                                    NewValue = null,
                                    ChangedBy = changedBy,
                                    ChangedDate = changedDate
                                };
                                // Try to set CustomerId if exists
                                var custProp = item.Entity.GetType().GetProperty("CustomerId");
                                if (custProp != null)
                                {
                                    var val = custProp.GetValue(item.Entity);
                                    if (val is Guid gid) log.CustomerId = gid;
                                }
                                CrmChangeLogs.Add(log);
                            }
                            else if (item.State == EntityState.Modified)
                            {
                                foreach (var prop in item.Properties)
                                {
                                    if (!prop.IsModified) continue;
                                    var propName = prop.Metadata.Name;
                                    if (propName == nameof(BaseEntity.UpdatedDate) || propName == nameof(BaseEntity.UpdatedBy) || propName == nameof(BaseEntity.UniqNumber) || propName == nameof(BaseEntity.CreatedBy) || propName == nameof(BaseEntity.CreatedDate)) continue;


                                    string oldVal = prop.OriginalValue?.ToString();
                                    string newVal = prop.CurrentValue?.ToString();
                                    if (oldVal == newVal) continue;

                                    CrmChangeLog log = new CrmChangeLog
                                    {
                                        EntityName = entityName,
                                        EntityId = entityId,
                                        Action = CrmChangeAction.Modified,
                                        PropertyName = propName,
                                        OldValue = oldVal,
                                        NewValue = newVal,
                                        ChangedBy = changedBy,
                                        ChangedDate = changedDate
                                    };
                                    var custProp = item.Entity.GetType().GetProperty("CustomerId");
                                    if (custProp != null)
                                    {
                                        var val = custProp.GetValue(item.Entity);
                                        if (val is Guid gid) log.CustomerId = gid;
                                    }
                                    CrmChangeLogs.Add(log);
                                }
                            }
                        }
                        catch {}
                    }

                    // Global varlıklar için: tenant alanları yok, sadece temel iz bilgilerinin yönetimi
                    if (item.Entity is GlobalBaseEntity globalEntity)
                    {
                        switch (item.State)
                        {
                            case EntityState.Added:
                                {
                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";
                                    Entry(globalEntity).Property(x => x.UpdatedBy).IsModified = false;
                                    Entry(globalEntity).Property(x => x.UpdatedDate).IsModified = false;

                                    globalEntity.Id = globalEntity.Id == Guid.Empty ? Guid.NewGuid() : globalEntity.Id;
                                    globalEntity.CreatedDate = DateTime.Now;
                                    globalEntity.CreatedBy = userName ?? "nulldata";
                                    globalEntity.UpdatedBy = "";
                                    break;
                                }
                            case EntityState.Modified:
                                {
                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";
                                    Entry(globalEntity).Property(x => x.UniqNumber).IsModified = false;
                                    Entry(globalEntity).Property(x => x.CreatedBy).IsModified = false;
                                    Entry(globalEntity).Property(x => x.CreatedDate).IsModified = false;

                                    globalEntity.UpdatedDate = DateTime.Now;
                                    globalEntity.UpdatedBy = userName ?? "nulldata";
                                    break;
                                }
                        }
                        // CRM change logging for GlobalBaseEntity
                        try
                        {
                            var entityName = item.Entity.GetType().Name;
                            var entityId = globalEntity.Id;
                            var changedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";
                            var changedDate = DateTime.Now;

                            if (item.State == EntityState.Added)
                            {
                                CrmChangeLog log = new CrmChangeLog
                                {
                                    EntityName = entityName,
                                    EntityId = entityId,
                                    Action = CrmChangeAction.Added,
                                    PropertyName = "*",
                                    OldValue = null,
                                    NewValue = null,
                                    ChangedBy = changedBy,
                                    ChangedDate = changedDate
                                };
                                var custProp = item.Entity.GetType().GetProperty("CustomerId");
                                if (custProp != null)
                                {
                                    var val = custProp.GetValue(item.Entity);
                                    if (val is Guid gid) log.CustomerId = gid;
                                }
                                CrmChangeLogs.Add(log);
                            }
                            else if (item.State == EntityState.Modified)
                            {
                                foreach (var prop in item.Properties)
                                {
                                    if (!prop.IsModified) continue;
                                    var propName = prop.Metadata.Name;
                                    if (propName == nameof(GlobalBaseEntity.UpdatedDate) || propName == nameof(GlobalBaseEntity.UpdatedBy) || propName == nameof(GlobalBaseEntity.UniqNumber) || propName == nameof(GlobalBaseEntity.CreatedBy) || propName == nameof(GlobalBaseEntity.CreatedDate)) continue;

                                    string oldVal = prop.OriginalValue?.ToString();
                                    string newVal = prop.CurrentValue?.ToString();
                                    if (oldVal == newVal) continue;

                                    CrmChangeLog log = new CrmChangeLog
                                    {
                                        EntityName = entityName,
                                        EntityId = entityId,
                                        Action = CrmChangeAction.Modified,
                                        PropertyName = propName,
                                        OldValue = oldVal,
                                        NewValue = newVal,
                                        ChangedBy = changedBy,
                                        ChangedDate = changedDate
                                    };
                                    var custProp = item.Entity.GetType().GetProperty("CustomerId");
                                    if (custProp != null)
                                    {
                                        var val = custProp.GetValue(item.Entity);
                                        if (val is Guid gid) log.CustomerId = gid;
                                    }
                                    CrmChangeLogs.Add(log);
                                }
                            }
                        }
                        catch {}
                    }
                }


            }
            catch (Exception ex)
            {


            }
            return base.SaveChanges();

        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = ResolveContextIds();

            try
            {
                foreach (var item in ChangeTracker.Entries())
                {
                    // AsNoTracking sonrası attach edilen Modified varlıklar için OriginalValues'ı DB'den yükle
                    if (item.State == EntityState.Modified)
                    {
                        try
                        {
                            var dbValues = item.GetDatabaseValues();
                            if (dbValues != null)
                            {
                                item.OriginalValues.SetValues(dbValues);
                            }
                        }
                        catch { }
                    }
                    if (item.Entity is BaseEntity entityReference)
                    {
                        switch (item.State)
                        {
                            case EntityState.Added:
                                {

                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";


                                    //var customHeaderValue = _httpContextAccessor.HttpContext?.Request?.Headers["X-Custom-Header"].FirstOrDefault();


                                    Entry(entityReference).Property(x => x.UpdatedDate).IsModified = false;
                                    Entry(entityReference).Property(x => x.UpdatedBy).IsModified = false;

                                    entityReference.CreatedDate = DateTime.Now;
                                    if (tenantId != Guid.Empty) entityReference.MainClientId = tenantId;
                                    entityReference.Id = Guid.NewGuid();

                                    if (userName != null)
                                        entityReference.CreatedBy = userName;
                                    else
                                        entityReference.CreatedBy = "nulldata";

                                    entityReference.UpdatedBy = "";

                                    // WorkflowHeadId alanını kontrol edip null olarak ayarlama
                                    if (entityReference is BudgetJobCodeRequest budgetJobCodeRequest)
                                    {
                                        budgetJobCodeRequest.WorkflowHeadId = budgetJobCodeRequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : budgetJobCodeRequest.WorkflowHeadId;
                                    }

                                    if (entityReference is BudgetNormCodeRequest normrequest)
                                    {
                                        normrequest.WorkflowHeadId = normrequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : normrequest.WorkflowHeadId;
                                    }

                                    break;
                                }
                            case EntityState.Modified:
                                {

                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";

                                    //var customHeaderValue = _httpContextAccessor.HttpContext?.Request?.Headers["X-Custom-Header"].FirstOrDefault();


                                    Entry(entityReference).Property(x => x.CreatedDate).IsModified = false;
                                    Entry(entityReference).Property(x => x.CreatedBy).IsModified = false;
                                    Entry(entityReference).Property(x => x.UniqNumber).IsModified = false;

                                    if (tenantId != Guid.Empty) entityReference.MainClientId = tenantId;
                                    // CompanyId şu an kullanılmıyor, boş bırakılabilir
                                    entityReference.UpdatedDate = DateTime.Now;

                                    if (userName != null)
                                        entityReference.UpdatedBy = userName;
                                    else
                                        entityReference.UpdatedBy = "nulldata";

                                    if (entityReference is BudgetJobCodeRequest budgetJobCodeRequest)
                                    {
                                        budgetJobCodeRequest.WorkflowHeadId = budgetJobCodeRequest.WorkflowHeadId == Guid.Empty ? (Guid?)null : budgetJobCodeRequest.WorkflowHeadId;
                                    }

                                    break;
                                }


                        }
                    }

                    // Global varlıklar için Created/Updated alanları yönetimi
                    if (item.Entity is GlobalBaseEntity globalEntity)
                    {
                        switch (item.State)
                        {
                            case EntityState.Added:
                                {
                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";

                                    Entry(globalEntity).Property(x => x.UpdatedDate).IsModified = false;
                                    Entry(globalEntity).Property(x => x.UpdatedBy).IsModified = false;

                                    globalEntity.CreatedDate = DateTime.Now;
                                    globalEntity.Id = globalEntity.Id == Guid.Empty ? Guid.NewGuid() : globalEntity.Id;

                                    if (userName != null)
                                        globalEntity.CreatedBy = userName;
                                    else
                                        globalEntity.CreatedBy = "nulldata";

                                    globalEntity.UpdatedBy = "";

                                    break;
                                }
                            case EntityState.Modified:
                                {
                                    var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "defaultUser";

                                    Entry(globalEntity).Property(x => x.CreatedDate).IsModified = false;
                                    Entry(globalEntity).Property(x => x.CreatedBy).IsModified = false;
                                    Entry(globalEntity).Property(x => x.UniqNumber).IsModified = false;

                                    globalEntity.UpdatedDate = DateTime.Now;
                                    if (userName != null)
                                        globalEntity.UpdatedBy = userName;
                                    else
                                        globalEntity.UpdatedBy = "nulldata";

                                    break;
                                }
                        }
                    }
                }
                var result = base.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {


                return null;

            }

        }
    }
}
