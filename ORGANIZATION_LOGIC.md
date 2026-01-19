# Organizasyon Şeması Mantığı - Detaylı Açıklama

## 🎯 Genel Bakış

Organizasyon şeması, şirket içindeki **hiyerarşik yapıyı**, **çalışan atamalarını**, **yönetici ilişkilerini** ve **pozisyon hiyerarşisini** yönetmek için tasarlanmıştır.

## 📊 Temel Yapı

```
┌─────────────────────────────────────────────────────────┐
│              ORGANIZASYON ŞEMASI YAPISI                 │
└─────────────────────────────────────────────────────────┘

1. OrganizationStructure (Organizasyon Birimleri)
   └── Hiyerarşik yapı (Şirket → Departman → Birim → Ekip)

2. OrganizationEmployee (Çalışan-Organizasyon İlişkisi)
   └── Çalışanların organizasyon birimlerine atanması

3. EmployeeManager (Çalışan-Yönetici İlişkisi)
   └── Yönetici-çalışan ilişkileri

4. OrganizationPosition (Organizasyon-Pozisyon İlişkisi)
   └── Organizasyon birimlerindeki pozisyonlar

5. PositionHierarchy (Pozisyon Hiyerarşisi)
   └── Pozisyonlar arası üst-alt ilişkileri
```

---

## 1️⃣ OrganizationStructure (Organizasyon Birimi)

### Mantık:
Organizasyonun **temel yapı taşıdır**. Şirket, departman, birim, ekip gibi tüm organizasyon birimlerini temsil eder.

### Özellikler:

#### A. Hiyerarşik Yapı
```csharp
ParentOrganizationId → Üst birim
SubOrganizations → Alt birimler
Level → Seviye (1=En üst, 2=Alt seviye)
HierarchyPath → "id1/id2/id3" (performans için)
```

**Örnek:**
```
Şirket (Level: 1, ParentOrganizationId: null)
└── IT Bölümü (Level: 2, ParentOrganizationId: Şirket.Id)
    └── Backend Departmanı (Level: 3, ParentOrganizationId: IT Bölümü.Id)
        └── API Ekibi (Level: 4, ParentOrganizationId: Backend Departmanı.Id)
```

#### B. Yönetici İlişkisi
```csharp
ManagerId → Bu birimin yöneticisi (UserApp)
```

**Örnek:**
```
IT Bölümü
├── ManagerId: Ahmet Yılmaz (UserApp)
└── Backend Departmanı
    └── ManagerId: Mehmet Kaya (UserApp)
```

#### C. Organizasyon Tipi
```csharp
OrganizationUnitType:
- Company = 1      // Şirket
- Division = 2     // Bölüm
- Department = 3   // Departman
- Unit = 4         // Birim
- Team = 5         // Ekip
- Branch = 6       // Şube
```

### Kullanım Senaryoları:

**Senaryo 1: Basit Hiyerarşi**
```
Şirket
└── IT Departmanı
    ├── Backend Ekibi
    └── Frontend Ekibi
```

**Senaryo 2: Çok Seviyeli Yapı**
```
Şirket (Level: 1)
└── Teknoloji Bölümü (Level: 2)
    ├── Yazılım Departmanı (Level: 3)
    │   ├── Backend Birimi (Level: 4)
    │   │   └── API Ekibi (Level: 5)
    │   └── Frontend Birimi (Level: 4)
    └── Altyapı Departmanı (Level: 3)
```

---

## 2️⃣ OrganizationEmployee (Çalışan-Organizasyon İlişkisi)

### Mantık:
Bir çalışanın **birden fazla organizasyon birimine bağlı olabilmesini** sağlar. **Matrix organizasyon** yapıları için kritiktir.

### Özellikler:

#### A. Çoklu Atama Desteği
```csharp
EmployeeId → Çalışan (UserApp)
OrganizationId → Organizasyon birimi
AssignmentType → Atama tipi
```

**Atama Tipleri:**
```csharp
Primary = 1      // Ana atama (asıl departmanı)
Secondary = 2    // İkincil atama (proje ekibi, geçici görev)
Temporary = 3   // Geçici atama
Consultant = 4   // Danışman
```

#### B. Pozisyon Bilgisi
```csharp
PositionId → Bu organizasyondaki pozisyonu
Role → Rolü (Manager, Member, Lead vb.)
```

#### C. Tarih Yönetimi
```csharp
StartDate → Başlangıç tarihi
EndDate → Bitiş tarihi (null = aktif)
IsActive → Aktif mi?
```

### Kullanım Senaryoları:

**Senaryo 1: Ana Departman**
```
Çalışan: Ayşe Demir
├── OrganizationEmployee
│   ├── EmployeeId: Ayşe Demir
│   ├── OrganizationId: IT Departmanı
│   ├── PositionId: Senior Developer
│   ├── AssignmentType: Primary
│   └── IsActive: true
```

**Senaryo 2: Matrix Organizasyon**
```
Çalışan: Ayşe Demir
├── Ana Atama (Primary)
│   ├── OrganizationId: IT Departmanı
│   └── PositionId: Senior Developer
│
└── İkincil Atama (Secondary)
    ├── OrganizationId: Proje Alpha Ekibi
    └── PositionId: Tech Lead
```

**Senaryo 3: Geçici Görevlendirme**
```
Çalışan: Mehmet Kaya
├── Ana Atama (Primary)
│   └── OrganizationId: IT Departmanı
│
└── Geçici Atama (Temporary)
    ├── OrganizationId: Yeni Proje Ekibi
    ├── StartDate: 2024-01-01
    ├── EndDate: 2024-06-30
    └── IsActive: true
```

### Sorgu Örnekleri:

**1. Bir çalışanın ana departmanını bul:**
```csharp
var primaryOrg = await _context.OrganizationEmployees
    .Include(oe => oe.Organization)
    .Where(oe => oe.EmployeeId == employeeId 
        && oe.AssignmentType == EmployeeAssignmentType.Primary
        && oe.IsActive)
    .FirstOrDefaultAsync();
```

**2. Bir çalışanın tüm atamalarını bul:**
```csharp
var allAssignments = await _context.OrganizationEmployees
    .Include(oe => oe.Organization)
    .Include(oe => oe.Position)
    .Where(oe => oe.EmployeeId == employeeId && oe.IsActive)
    .ToListAsync();
```

---

## 3️⃣ EmployeeManager (Çalışan-Yönetici İlişkisi)

### Mantık:
Çalışanların **yöneticilerini** tanımlar. Bir çalışanın **birden fazla yöneticisi** olabilir (farklı organizasyon birimlerinde).

### Özellikler:

#### A. Yönetici Tipleri
```csharp
ManagerType:
- Direct = 1           // Direkt yönetici (asıl yöneticisi)
- Functional = 2       // Fonksiyonel yönetici (Matrix organizasyon)
- Project = 3          // Proje yöneticisi
- Department = 4       // Departman yöneticisi
```

#### B. Yönetici Seviyesi
```csharp
Level:
- 1 = Direkt Manager (bir üst seviye)
- 2 = Manager'ın Manager'ı (iki üst seviye)
- 3 = Üç üst seviye
```

#### C. Organizasyon Bağlantısı
```csharp
OrganizationId → Hangi organizasyon biriminde bu yönetici-çalışan ilişkisi geçerli
```

### Kullanım Senaryoları:

**Senaryo 1: Direkt Yönetici**
```
Çalışan: Ayşe Demir
├── EmployeeManager
│   ├── EmployeeId: Ayşe Demir
│   ├── ManagerId: Mehmet Kaya
│   ├── ManagerType: Direct
│   ├── Level: 1
│   └── OrganizationId: IT Departmanı
```

**Senaryo 2: Matrix Organizasyon (Çoklu Yönetici)**
```
Çalışan: Ayşe Demir
├── Direkt Yönetici (Direct)
│   ├── ManagerId: Mehmet Kaya
│   ├── ManagerType: Direct
│   └── OrganizationId: IT Departmanı
│
└── Proje Yöneticisi (Project)
    ├── ManagerId: Ali Veli
    ├── ManagerType: Project
    └── OrganizationId: Proje Alpha Ekibi
```

**Senaryo 3: Yönetici Hiyerarşisi**
```
Çalışan: Ayşe Demir
├── Level 1: Direkt Manager (Mehmet Kaya)
├── Level 2: Manager'ın Manager'ı (Ahmet Yılmaz)
└── Level 3: Üç üst seviye (Fatma Öz)
```

### Sorgu Örnekleri:

**1. Direkt yöneticiyi bul:**
```csharp
var directManager = await _context.EmployeeManagers
    .Include(em => em.Manager)
    .Where(em => em.EmployeeId == employeeId 
        && em.ManagerType == ManagerType.Direct 
        && em.Level == 1 
        && em.IsActive)
    .FirstOrDefaultAsync();
```

**2. Tüm yöneticileri bul (seviye bazlı):**
```csharp
var allManagers = await _context.EmployeeManagers
    .Include(em => em.Manager)
    .Where(em => em.EmployeeId == employeeId && em.IsActive)
    .OrderBy(em => em.Level)
    .ToListAsync();
```

**3. "Manager'ın Manager'ı" bul:**
```csharp
var secondLevelManager = await _context.EmployeeManagers
    .Include(em => em.Manager)
    .Where(em => em.EmployeeId == employeeId 
        && em.Level == 2 
        && em.IsActive)
    .FirstOrDefaultAsync();
```

---

## 4️⃣ OrganizationPosition (Organizasyon-Pozisyon İlişkisi)

### Mantık:
Bir organizasyon biriminde **hangi pozisyonların bulunduğunu** tanımlar. Bir pozisyon **birden fazla organizasyon biriminde** olabilir.

### Özellikler:

#### A. Pozisyon Ataması
```csharp
OrganizationId → Organizasyon birimi
PositionId → Pozisyon
MaxEmployees → Bu pozisyonda kaç kişi çalışabilir (null = sınırsız)
```

#### B. Kapasite Yönetimi
```csharp
CurrentEmployeeCount → Şu an bu pozisyonda kaç kişi var (computed)
MaxEmployees → Maksimum kişi sayısı
```

### Kullanım Senaryoları:

**Senaryo 1: Departman Pozisyonları**
```
IT Departmanı
├── OrganizationPosition
│   ├── PositionId: Senior Developer
│   └── MaxEmployees: 5
│
├── OrganizationPosition
│   ├── PositionId: Junior Developer
│   └── MaxEmployees: 10
│
└── OrganizationPosition
    ├── PositionId: Tech Lead
    └── MaxEmployees: 2
```

**Senaryo 2: Aynı Pozisyon Farklı Departmanlarda**
```
Senior Developer Pozisyonu
├── IT Departmanı (MaxEmployees: 5)
├── Finans Departmanı (MaxEmployees: 2)
└── Satış Departmanı (MaxEmployees: 3)
```

### Sorgu Örnekleri:

**1. Bir departmandaki pozisyonları bul:**
```csharp
var positions = await _context.OrganizationPositions
    .Include(op => op.Position)
    .Where(op => op.OrganizationId == organizationId && op.IsActive)
    .ToListAsync();
```

**2. Pozisyon kapasitesini kontrol et:**
```csharp
var position = await _context.OrganizationPositions
    .Include(op => op.Organization)
    .Where(op => op.OrganizationId == orgId && op.PositionId == positionId)
    .FirstOrDefaultAsync();

// Şu an kaç kişi var?
var currentCount = await _context.OrganizationEmployees
    .CountAsync(oe => oe.OrganizationId == orgId 
        && oe.PositionId == positionId 
        && oe.IsActive);

if (position.MaxEmployees.HasValue && currentCount >= position.MaxEmployees)
{
    // Kapasite dolu!
}
```

---

## 5️⃣ PositionHierarchy (Pozisyon Hiyerarşisi)

### Mantık:
Pozisyonlar arasındaki **üst-alt ilişkisini** tanımlar. Onay akışları ve kariyer yolu planlaması için kullanılır.

### Özellikler:

#### A. Hiyerarşi Yapısı
```csharp
LowerPositionId → Alt pozisyon
HigherPositionId → Üst pozisyon
Level → Hiyerarşi seviyesi (1=Direkt üst, 2=Üstün üstü)
```

### Kullanım Senaryoları:

**Senaryo 1: Basit Hiyerarşi**
```
Junior Developer
└── PositionHierarchy
    ├── LowerPositionId: Junior Developer
    ├── HigherPositionId: Senior Developer
    └── Level: 1

Senior Developer
└── PositionHierarchy
    ├── LowerPositionId: Senior Developer
    ├── HigherPositionId: Tech Lead
    └── Level: 1
```

**Senaryo 2: Çok Seviyeli Hiyerarşi**
```
Junior Developer (Level 1)
└── Senior Developer (Level 2)
    └── Tech Lead (Level 3)
        └── Engineering Manager (Level 4)
```

### Sorgu Örnekleri:

**1. Bir pozisyonun üst pozisyonlarını bul:**
```csharp
var higherPositions = await _context.PositionHierarchies
    .Include(ph => ph.HigherPosition)
    .Where(ph => ph.LowerPositionId == positionId && ph.IsActive)
    .Select(ph => ph.HigherPosition)
    .ToListAsync();
```

**2. Bir pozisyonun alt pozisyonlarını bul:**
```csharp
var lowerPositions = await _context.PositionHierarchies
    .Include(ph => ph.LowerPosition)
    .Where(ph => ph.HigherPositionId == positionId && ph.IsActive)
    .Select(ph => ph.LowerPosition)
    .ToListAsync();
```

**3. Pozisyon hiyerarşisini yukarı doğru takip et:**
```csharp
var hierarchy = new List<Positions>();
var currentPositionId = juniorDeveloperId;

while (currentPositionId != null)
{
    var hierarchyItem = await _context.PositionHierarchies
        .Include(ph => ph.HigherPosition)
        .Where(ph => ph.LowerPositionId == currentPositionId && ph.IsActive)
        .FirstOrDefaultAsync();
    
    if (hierarchyItem == null) break;
    
    hierarchy.Add(hierarchyItem.HigherPosition);
    currentPositionId = hierarchyItem.HigherPositionId;
}
```

---

## 🔄 Workflow ile Entegrasyon

### Onay Akışı Senaryoları:

#### Senaryo 1: Direkt Yöneticiye Git
```csharp
// ApproverNode'da kullanım
var directManager = await _context.EmployeeManagers
    .Include(em => em.Manager)
    .Where(em => em.EmployeeId == currentUserId 
        && em.ManagerType == ManagerType.Direct 
        && em.Level == 1 
        && em.IsActive)
    .FirstOrDefaultAsync();

// Onay için direkt yöneticiyi ata
approveItem.ApproveUser = directManager.ManagerId;
```

#### Senaryo 2: Departman Yöneticisine Git
```csharp
// Çalışanın ana organizasyonunu bul
var primaryOrg = await _context.OrganizationEmployees
    .Include(oe => oe.Organization)
    .Where(oe => oe.EmployeeId == currentUserId 
        && oe.AssignmentType == EmployeeAssignmentType.Primary
        && oe.IsActive)
    .FirstOrDefaultAsync();

// Departman yöneticisini al
var departmentManager = primaryOrg.Organization.ManagerId;
```

#### Senaryo 3: Pozisyon Bazlı Onay
```csharp
// Çalışanın pozisyonunu bul
var employeePosition = await _context.OrganizationEmployees
    .Include(oe => oe.Position)
    .Where(oe => oe.EmployeeId == currentUserId && oe.IsActive)
    .Select(oe => oe.PositionId)
    .FirstOrDefaultAsync();

// Pozisyon hiyerarşisinde üst pozisyonu bul
var higherPosition = await _context.PositionHierarchies
    .Include(ph => ph.HigherPosition)
    .Where(ph => ph.LowerPositionId == employeePosition && ph.IsActive)
    .Select(ph => ph.HigherPosition)
    .FirstOrDefaultAsync();

// Üst pozisyondaki çalışanları bul ve onay için ata
var approvers = await _context.OrganizationEmployees
    .Where(oe => oe.PositionId == higherPosition.Id && oe.IsActive)
    .Select(oe => oe.EmployeeId)
    .ToListAsync();
```

#### Senaryo 4: "Manager'ın Manager'ı" Onayı
```csharp
// Level 2 yöneticiyi bul
var secondLevelManager = await _context.EmployeeManagers
    .Include(em => em.Manager)
    .Where(em => em.EmployeeId == currentUserId 
        && em.Level == 2 
        && em.IsActive)
    .FirstOrDefaultAsync();

// Onay için Level 2 yöneticiyi ata
approveItem.ApproveUser = secondLevelManager.ManagerId;
```

---

## 📊 İlişki Diyagramı (Detaylı)

```
┌─────────────────────────────────────────────────────────────┐
│                    ORGANIZASYON ŞEMASI                      │
└─────────────────────────────────────────────────────────────┘

OrganizationStructure (Organizasyon Birimi)
│
├── ParentOrganizationId → OrganizationStructure (Self-reference)
│   └── Hiyerarşik yapı: Şirket → Bölüm → Departman → Birim
│
├── ManagerId → UserApp
│   └── Bu birimin yöneticisi
│
├── Employees → OrganizationEmployee[]
│   └── Bu birime bağlı çalışanlar
│
└── Positions → OrganizationPosition[]
    └── Bu birimdeki pozisyonlar

OrganizationEmployee (Çalışan-Organizasyon)
│
├── EmployeeId → UserApp
│   └── Çalışan
│
├── OrganizationId → OrganizationStructure
│   └── Bağlı olduğu organizasyon birimi
│
└── PositionId → Positions
    └── Bu organizasyondaki pozisyonu

EmployeeManager (Çalışan-Yönetici)
│
├── EmployeeId → UserApp
│   └── Çalışan
│
├── ManagerId → UserApp
│   └── Yönetici
│
├── OrganizationId → OrganizationStructure (opsiyonel)
│   └── Hangi organizasyonda bu ilişki geçerli
│
├── ManagerType → Direct, Functional, Project, Department
│   └── Yönetici tipi
│
└── Level → 1, 2, 3...
    └── Yönetici seviyesi

OrganizationPosition (Organizasyon-Pozisyon)
│
├── OrganizationId → OrganizationStructure
│   └── Organizasyon birimi
│
├── PositionId → Positions
│   └── Pozisyon
│
└── MaxEmployees → int?
    └── Maksimum çalışan sayısı

PositionHierarchy (Pozisyon Hiyerarşisi)
│
├── LowerPositionId → Positions
│   └── Alt pozisyon
│
├── HigherPositionId → Positions
│   └── Üst pozisyon
│
└── Level → 1, 2, 3...
    └── Hiyerarşi seviyesi
```

---

## 🎯 Özet

### Organizasyon Şeması Mantığı:

1. **OrganizationStructure**: Hiyerarşik organizasyon yapısı
2. **OrganizationEmployee**: Çalışanların organizasyon birimlerine atanması (Matrix destekli)
3. **EmployeeManager**: Yönetici-çalışan ilişkileri (çoklu yönetici desteği)
4. **OrganizationPosition**: Organizasyon birimlerindeki pozisyonlar
5. **PositionHierarchy**: Pozisyonlar arası hiyerarşi

### Avantajlar:

✅ **Esneklik**: Matrix organizasyonlar desteklenir
✅ **Hiyerarşi**: Çok seviyeli organizasyon yapıları
✅ **Tarih Yönetimi**: Geçmiş veriler saklanabilir
✅ **Performans**: HierarchyPath ile hızlı sorgular
✅ **Workflow Entegrasyonu**: Onay akışları için hazır

### Kullanım Alanları:

- ✅ Onay akışları (yönetici bulma)
- ✅ Yetkilendirme (organizasyon bazlı)
- ✅ Raporlama (organizasyon hiyerarşisi)
- ✅ Kariyer yolu planlaması (pozisyon hiyerarşisi)
- ✅ Kapasite yönetimi (pozisyon bazlı)

