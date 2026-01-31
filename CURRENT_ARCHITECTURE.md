# Mevcut Organizasyon Yapısı - Genel Bakış

## 🏗️ Genel Mimari

```
┌─────────────────────────────────────────────────────────────────┐
│                    ORGANIZASYON YAPISI                          │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐
│    UserApp      │  (IdentityUser - Global, tenant-bağımsız)
├─────────────────┤
│ • FirstName     │
│ • LastName      │
│ • Email         │
│ • UserName      │
│ • ...           │
│                 │
│ DEPRECATED:     │  ⚠️ Backward compatibility için tutuluyor
│ • OrgUnitId     │     Aktif kullanım: EmployeeAssignment
│ • PositionId    │
└─────────────────┘
         │
         │ 1:N
         ▼
┌─────────────────────────────────────────────────────────────────┐
│         EmployeeAssignment (Tenant-Bağımlı)                     │
├─────────────────────────────────────────────────────────────────┤
│ • UserId → UserApp                                              │
│ • MainClientId → Tenant (BaseEntity)                            │
│ • OrgUnitId → OrgUnit                                           │
│ • PositionId → Positions                                         │
│ • ManagerId → UserApp (Direkt yönetici)                         │
│ • StartDate → Başlangıç                                         │
│ • EndDate → Bitiş (null = aktif)                                │
│ • AssignmentType → Primary/Secondary/Temporary/Matrix           │
│                                                                  │
│ ✅ Effective Dating Pattern                                      │
│ ✅ Geçmiş atamalar korunur                                       │
│ ✅ Tenant-bağımlı (her tenant'ta farklı yapı)                    │
└─────────────────────────────────────────────────────────────────┘
         │                    │                    │
         │                    │                    │
         ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   OrgUnit    │    │  Positions   │    │   UserApp    │
├──────────────┤    ├──────────────┤    │  (Manager)   │
│ • Name       │    │ • Name       │    │              │
│ • Code       │    │ • Description│    │              │
│ • Type       │    │              │    │              │
│ • ManagerId  │    │ • ParentPosId│    │              │
│ • ParentId   │    │              │    │              │
│              │    │              │    │              │
│ ✅ Hiyerarşik│    │ ✅ Hiyerarşik│    │              │
│ ✅ Tenant    │    │ ✅ Tenant    │    │              │
└──────────────┘    └──────────────┘    └──────────────┘
```

---

## 📊 İlişki Diyagramı

### 1. UserApp → EmployeeAssignment (1:N)
```
UserApp (Ahmet)
├── EmployeeAssignment #1 (Tenant A, IT Dept, 2024-01-01 → 2024-06-30) ❌ Bitti
├── EmployeeAssignment #2 (Tenant A, Sales Dept, 2024-07-01 → null) ✅ Aktif
└── EmployeeAssignment #3 (Tenant B, HR Dept, 2024-01-01 → null) ✅ Aktif
```

### 2. EmployeeAssignment → OrgUnit (N:1)
```
EmployeeAssignment
├── OrgUnitId → IT Departmanı
└── OrgUnit.ManagerId → IT Yöneticisi (Mehmet)
```

### 3. EmployeeAssignment → Positions (N:1)
```
EmployeeAssignment
├── PositionId → Senior Developer
└── Position.ParentPositionId → Lead Developer
```

### 4. EmployeeAssignment → Manager (N:1)
```
EmployeeAssignment
├── ManagerId → Direkt Yönetici (Ali)
└── Manager (UserApp)
```

---

## 🎯 Temel Prensipler

### ✅ Single Source of Truth
- **Manager**: `EmployeeAssignment.ManagerId` (tenant-bağımlı)
- **Departman**: `EmployeeAssignment.OrgUnitId` (tenant-bağımlı)
- **Pozisyon**: `EmployeeAssignment.PositionId` (tenant-bağımlı)

### ✅ Effective Dating Pattern
- Geçmiş atamalar korunur
- Tarih bazlı raporlama yapılabilir
- Audit trail sağlanır

### ✅ Tenant İzolasyonu
- Her tenant'ın kendi organizasyon yapısı
- Bir kullanıcı farklı tenant'larda farklı rollerde olabilir
- Veri güvenliği artar

---

## 🔄 Kullanım Senaryoları

### Senaryo 1: Kullanıcının Aktif Atamasını Bulma

```csharp
// Tenant filtresi otomatik (AppDbContext query filter)
var assignment = await EmployeeAssignmentHelper
    .GetActiveAssignmentAsync(context.EmployeeAssignments, userId);

if (assignment != null)
{
    var orgUnit = assignment.OrgUnit;        // Departman
    var position = assignment.Position;      // Pozisyon
    var manager = assignment.Manager;        // Yönetici
}
```

### Senaryo 2: Departman Değişikliği

```csharp
// 1. Eski aktif atamayı sonlandır
await EmployeeAssignmentHelper.EndActiveAssignmentAsync(
    context.EmployeeAssignments, 
    userId, 
    DateTime.UtcNow);

// 2. Yeni atama oluştur
var newAssignment = new EmployeeAssignment
{
    UserId = userId,
    MainClientId = currentTenantId, // Tenant-bağımlı
    OrgUnitId = newOrgUnitId,
    PositionId = newPositionId,
    ManagerId = newOrgUnit.ManagerId, // Yeni departmanın yöneticisi
    StartDate = DateTime.UtcNow,
    EndDate = null, // Aktif
    AssignmentType = AssignmentType.Primary
};

// ✅ Geçmiş atama kaybolmaz, yeni atama eklenir!
```

### Senaryo 3: Workflow'da Manager Bulma

```csharp
// WorkflowEngine.ExecuteApprove içinde
if (currentNode.Data.isManager == true)
{
    var user = await _userManager.FindByIdAsync(_ApiSendUser);
    
    // Aktif atamayı bul (tenant filtresi otomatik)
    var assignment = await EmployeeAssignmentHelper
        .GetActiveAssignmentAsync(
            _parameters._context.EmployeeAssignments, 
            user.Id);
    
    if (assignment?.Manager != null)
    {
        var manager = assignment.Manager;
        workFlowItem.approveItems.Add(new ApproveItems 
        { 
            ApproveUser = manager.Id,
            ApproveUserNameSurname = $"{manager.FirstName} {manager.LastName}"
        });
    }
}
```

### Senaryo 4: Çok Tenant Senaryosu

```csharp
// Ahmet → Tenant A'da IT Departmanında
var assignmentA = await EmployeeAssignmentHelper
    .GetActiveAssignmentByTenantAsync(
        context.EmployeeAssignments, 
        "ahmet-id", 
        tenantAId);

// Ahmet → Tenant B'de Sales Departmanında
var assignmentB = await EmployeeAssignmentHelper
    .GetActiveAssignmentByTenantAsync(
        context.EmployeeAssignments, 
        "ahmet-id", 
        tenantBId);

// ✅ Her tenant'ta farklı atama!
```

---

## 📋 Model Detayları

### UserApp (Global)
```csharp
public class UserApp : IdentityUser
{
    // Temel bilgiler
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // ...
    
    // DEPRECATED (Backward compatibility)
    public Guid? OrgUnitId { get; set; }
    public Guid? PositionId { get; set; }
    
    // Navigation
    public virtual List<EmployeeAssignment> EmployeeAssignments { get; set; }
}
```

### EmployeeAssignment (Tenant-Bağımlı)
```csharp
public class EmployeeAssignment : BaseEntity
{
    public string UserId { get; set; }
    public Guid? MainClientId { get; set; } // Tenant
    
    public Guid? OrgUnitId { get; set; }
    public Guid? PositionId { get; set; }
    public string? ManagerId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; } // null = aktif
    
    public AssignmentType AssignmentType { get; set; }
}
```

### OrgUnit (Tenant-Bağımlı)
```csharp
public class OrgUnit : BaseEntity
{
    public string Name { get; set; }
    public OrgUnitType Type { get; set; }
    
    public Guid? ParentOrgUnitId { get; set; } // Hiyerarşik
    public string? ManagerId { get; set; } // Departman yöneticisi
    
    public virtual List<UserApp> Users { get; set; }
}
```

### Positions (Tenant-Bağımlı)
```csharp
public class Positions : BaseEntity
{
    public string Name { get; set; }
    
    public Guid? ParentPositionId { get; set; } // Hiyerarşik
    
    public virtual List<UserApp> UserApps { get; set; }
}
```

---

## 🎯 Avantajlar

### ✅ Geçmiş Korunur
- Departman değişiklikleri kaybolmaz
- Tarih bazlı raporlama yapılabilir
- Audit trail sağlanır

### ✅ Tenant İzolasyonu
- Her tenant'ın kendi organizasyon yapısı
- Veri güvenliği artar
- Çok tenant desteği

### ✅ Esneklik
- Matrix organizasyon desteği
- Çoklu atama (Primary + Secondary)
- Geçici atamalar

### ✅ Tutarlılık
- Single Source of Truth
- Otomatik tenant filtresi
- Veri tutarsızlığı önlenir

---

## ⚠️ Önemli Notlar

### Deprecated Alanlar
- `UserApp.OrgUnitId` → Artık `EmployeeAssignment.OrgUnitId` kullanılmalı
- `UserApp.PositionId` → Artık `EmployeeAssignment.PositionId` kullanılmalı
- `UserApp.ManagerId` → Kaldırıldı, `EmployeeAssignment.ManagerId` kullanılmalı

### Migration Stratejisi
1. Mevcut `UserApp.OrgUnitId` ve `PositionId` verilerini `EmployeeAssignment`'a migrate et
2. `UserApp.ManagerId` verilerini `EmployeeAssignment.ManagerId`'ye taşı
3. Eski alanları nullable yap veya kaldır

---

## 📚 İlgili Dosyalar

- `formneo.core/Models/EmployeeAssignment.cs`
- `formneo.core/Models/UserApp.cs`
- `formneo.core/Models/OrgUnit.cs`
- `formneo.core/Models/Positions.cs`
- `formneo.core/Helpers/EmployeeAssignmentHelper.cs`
- `formneo.core/DTOs/EmployeeAssignments/`
- `formneo.repository/Configurations/EmployeeAssignmentConfiguration.cs`











