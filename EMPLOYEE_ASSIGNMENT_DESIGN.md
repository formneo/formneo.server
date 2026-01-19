# EmployeeAssignment Tasarım Dokümantasyonu

## 🎯 Amaç

Kullanıcının organizasyon birimi, pozisyon ve yönetici atamalarını **geçmişiyle birlikte** saklamak için **Effective Dating Pattern** kullanıyoruz.

## ⚠️ ÖNEMLİ: Tenant-Bağımlı Yapı

**EmployeeAssignment tenant-bağımlıdır** (`BaseEntity.MainClientId`)

### Senaryo
```
Ahmet → Tenant A'da IT Departmanında çalışıyor
Ahmet → Tenant B'de Sales Departmanında çalışıyor

Her tenant'ta farklı organizasyon yapısı ve atamalar olabilir!
```

### Neden Tenant-Bağımlı?
- ✅ Her tenant'ın kendi organizasyon yapısı var
- ✅ Bir kullanıcı birden fazla tenant'ta farklı rollerde olabilir
- ✅ Tenant izolasyonu sağlanır
- ✅ Veri güvenliği artar

## 📊 Model Yapısı

```csharp
EmployeeAssignment
├── UserId → Kullanıcı
├── OrgUnitId → Organizasyon birimi (Departman, Takım, vb.)
├── PositionId → Pozisyon
├── ManagerId → Yönetici (Direkt rapor edilen kişi)
├── StartDate → Atama başlangıç tarihi
├── EndDate → Atama bitiş tarihi (null = aktif)
├── AssignmentType → Atama tipi (Primary, Secondary, Temporary, Matrix)
└── Notes → Açıklama/Not
```

## 🔄 Senaryolar

### Senaryo 1: Yeni Atama Oluşturma

```csharp
// 1. Eski aktif atamayı sonlandır
await EmployeeAssignmentHelper.EndActiveAssignmentAsync(
    assignments, 
    userId, 
    DateTime.UtcNow);

// 2. Yeni atama oluştur
var newAssignment = new EmployeeAssignment
{
    UserId = userId,
    OrgUnitId = newOrgUnitId,
    PositionId = newPositionId,
    ManagerId = newManagerId,
    StartDate = DateTime.UtcNow,
    EndDate = null, // Aktif
    AssignmentType = AssignmentType.Primary
};
```

### Senaryo 2: Aktif Atamayı Bulma

```csharp
// Kullanıcının aktif atamasını bul
var activeAssignment = await EmployeeAssignmentHelper
    .GetActiveAssignmentAsync(context.EmployeeAssignments, userId);

if (activeAssignment != null)
{
    var orgUnit = activeAssignment.OrgUnit;
    var position = activeAssignment.Position;
    var manager = activeAssignment.Manager;
}
```

### Senaryo 3: Geçmiş Atamaları Sorgulama

```csharp
// Belirli bir tarihteki atamayı bul
var assignmentAtDate = await EmployeeAssignmentHelper
    .GetAssignmentAtDateAsync(
        context.EmployeeAssignments, 
        userId, 
        new DateTime(2024, 1, 1));

// Tüm geçmiş atamaları listele
var allAssignments = await context.EmployeeAssignments
    .Where(ea => ea.UserId == userId)
    .OrderByDescending(ea => ea.StartDate)
    .ToListAsync();
```

### Senaryo 4: Departman Değişikliği

```csharp
// Kullanıcı departman değiştiriyor
// 1. Eski atamayı sonlandır
var oldAssignment = await GetActiveAssignmentAsync(...);
oldAssignment.EndDate = DateTime.UtcNow;

// 2. Yeni atama oluştur
var newAssignment = new EmployeeAssignment
{
    UserId = userId,
    OrgUnitId = newOrgUnitId,
    PositionId = newPositionId,
    ManagerId = newOrgUnit.ManagerId, // Yeni departmanın yöneticisi
    StartDate = DateTime.UtcNow,
    EndDate = null,
    AssignmentType = AssignmentType.Primary
};

// ✅ Geçmiş atama kaybolmaz, yeni atama eklenir!
```

## 🎯 Avantajlar

### ✅ Geçmiş Korunur
- Kullanıcı departman değiştirince geçmiş atama kaybolmaz
- Tarih bazlı raporlama yapılabilir
- Audit trail sağlanır

### ✅ Esneklik
- Matrix organizasyon desteği (farklı departmandan yönetici)
- Çoklu atama desteği (Primary + Secondary)
- Geçici atamalar

### ✅ Tutarlılık
- Manager bilgisi atama ile birlikte saklanır
- Departman değişikliğinde otomatik güncellenir
- Veri tutarsızlığı önlenir

## 📋 Kullanım Örnekleri

### Workflow'da Manager Bulma

```csharp
// ÖNCE (UserApp.ManagerId):
var manager = user.Manager; // ❌ Geçmiş bilgisi yok

// SONRA (EmployeeAssignment):
var activeAssignment = await GetActiveAssignmentAsync(...);
var manager = activeAssignment?.Manager; // ✅ Aktif atama
```

### Organizasyon Şeması

```csharp
// Kullanıcının aktif atamasını bul
var assignment = await GetActiveAssignmentAsync(...);

// Departman bilgisi
var orgUnit = assignment.OrgUnit;

// Yönetici bilgisi
var manager = assignment.Manager;

// Pozisyon bilgisi
var position = assignment.Position;
```

## 🔧 Migration Stratejisi

### Mevcut Verileri Migrate Etme

```csharp
// 1. Mevcut UserApp.OrgUnitId ve PositionId'den EmployeeAssignment oluştur
var users = await context.Users
    .Where(u => u.OrgUnitId != null || u.PositionId != null)
    .ToListAsync();

foreach (var user in users)
{
    var assignment = new EmployeeAssignment
    {
        UserId = user.Id,
        OrgUnitId = user.OrgUnitId,
        PositionId = user.PositionId,
        ManagerId = user.OrgUnit?.ManagerId,
        StartDate = DateTime.UtcNow.AddYears(-1), // Varsayılan başlangıç
        EndDate = null, // Aktif
        AssignmentType = AssignmentType.Primary
    };
    context.EmployeeAssignments.Add(assignment);
}

await context.SaveChangesAsync();
```

## 📊 Index Stratejisi

```sql
-- Aktif atamaları hızlı bulmak için
CREATE INDEX IX_EmployeeAssignments_UserId_EndDate 
ON EmployeeAssignments(UserId, EndDate);

-- Departman bazlı sorgular için
CREATE INDEX IX_EmployeeAssignments_OrgUnitId_EndDate 
ON EmployeeAssignments(OrgUnitId, EndDate);

-- Tarih bazlı sorgular için
CREATE INDEX IX_EmployeeAssignments_StartDate_EndDate 
ON EmployeeAssignments(StartDate, EndDate);
```

## 🎯 Sonuç

**EmployeeAssignment** tablosu ile:
- ✅ Geçmiş atamalar korunur
- ✅ Tarih bazlı raporlama yapılabilir
- ✅ Matrix organizasyon desteği
- ✅ Veri tutarlılığı sağlanır
- ✅ Best practice pattern kullanılır

