# 🏗️ Entity Manager Mimarisi

## 📋 İçindekiler
1. [Genel Bakış](#genel-bakış)
2. [Mimari Yapı](#mimari-yapı)
3. [Temel Bileşenler](#temel-bileşenler)
4. [Kullanım Senaryoları](#kullanım-senaryoları)
5. [Kurulum](#kurulum)
6. [Örnekler](#örnekler)

---

## Genel Bakış

Entity Manager, form tasarım ekranında formları veritabanı entity'lerine bağlamak, alanları ve tipleri merkezi olarak yönetmek için geliştirilmiş kapsamlı bir mimaridir.

### Temel Özellikler

✅ **Entity Tanımlama**: Formlarla ilişkili entity'leri (Customer, Order, Employee vb.) tanımlama
✅ **Field Management**: Her entity için alanları ve tiplerini merkezi yönetim
✅ **Type-Safe**: Güçlü tip kontrolü ile veri bütünlüğü
✅ **Auto-Mapping**: Form elemanlarını entity alanlarına otomatik eşleme
✅ **Validation**: Alan seviyesinde validation kuralları
✅ **Reusability**: Entity'leri ve alanları birden fazla formda kullanabilme
✅ **Lookup Support**: Entity'ler arası ilişkiler (Foreign Key, Many-to-Many)

---

## Mimari Yapı

```
┌─────────────────────────────────────────────────────────────┐
│                         FORM LAYER                          │
│  ┌───────────────────────────────────────────────────────┐  │
│  │  Form Design (JSON)                                   │  │
│  │  - Input Fields                                       │  │
│  │  - Components                                         │  │
│  │  - Validation Rules                                   │  │
│  └───────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ FormEntityRelation
                            │ FormFieldMapping
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     ENTITY MANAGER LAYER                    │
│  ┌────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │  FormEntity    │  │ FormEntityField │  │ FieldType    │ │
│  │  - Customer    │  │ - FirstName     │  │ - String     │ │
│  │  - Order       │  │ - Email         │  │ - Number     │ │
│  │  - Employee    │  │ - BirthDate     │  │ - Date       │ │
│  └────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Entity Definition
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                      DATABASE LAYER                         │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐            │
│  │  Customers │  │   Orders   │  │ Employees  │            │
│  │   Table    │  │   Table    │  │   Table    │            │
│  └────────────┘  └────────────┘  └────────────┘            │
└─────────────────────────────────────────────────────────────┘
```

---

## Temel Bileşenler

### 1. FormEntity (Entity Tanımları)
Form ile ilişkilendirilebilen entity tanımlarını içerir.

**Özellikler:**
- Entity adı ve açıklaması
- Veritabanı tablo bilgileri (TableName, SchemaName)
- C# sınıf bilgileri (ClassName, NamespacePath)
- CRUD izinleri (AllowCreate, AllowRead, AllowUpdate, AllowDelete)
- API endpoint bilgisi
- Display ve OrderBy alanları
- Parent-Child ilişkiler

**Örnek:**
```json
{
  "entityName": "Customer",
  "entityDescription": "Müşteri bilgileri",
  "tableName": "Customers",
  "schemaName": "dbo",
  "className": "Customer",
  "namespacePath": "formneo.core.Models",
  "allowCreate": true,
  "allowRead": true,
  "allowUpdate": true,
  "allowDelete": false,
  "apiEndpoint": "/api/customers",
  "displayField": "FullName",
  "orderByField": "CreatedAt"
}
```

### 2. FormEntityField (Alan Tanımları)
Entity'lerin alanlarını ve özelliklerini tanımlar.

**Özellikler:**
- Alan adı ve açıklaması
- Alan tipi (FieldType ile ilişkili)
- Veritabanı kolon adı
- Validation kuralları (Required, Unique, MinLength, MaxLength, Regex)
- Display özellikleri (Label, Placeholder, HelpText)
- Lookup ayarları (RelatedEntity, DisplayField, ValueField)
- Default değer

**Örnek:**
```json
{
  "fieldName": "Email",
  "fieldDescription": "Müşteri email adresi",
  "fieldTypeId": "00000000-0000-0000-0000-000000000009",
  "columnName": "Email",
  "isRequired": true,
  "isUnique": true,
  "maxLength": 255,
  "displayLabel": "E-posta Adresi",
  "placeholder": "ornek@domain.com",
  "helpText": "Geçerli bir email adresi giriniz"
}
```

### 3. FormEntityFieldType (Alan Tipleri)
Alan tiplerini ve davranışlarını tanımlar.

**Built-in Tipler:**

#### Primitive Types:
- **String**: Kısa metin
- **Text**: Uzun metin (textarea)
- **Integer**: Tam sayı
- **Decimal**: Ondalık sayı
- **Boolean**: Evet/Hayır
- **Date**: Tarih
- **DateTime**: Tarih ve saat
- **Time**: Saat
- **Email**: Email adresi
- **Phone**: Telefon numarası
- **URL**: Web adresi
- **Guid**: Unique identifier

#### Reference Types:
- **Lookup**: Tek referans (Foreign Key)
- **MultiLookup**: Çoklu referans (Many-to-Many)

#### Collection Types:
- **StringArray**: Metin dizisi
- **NumberArray**: Sayı dizisi

#### Complex Types:
- **JSON**: JSON verisi
- **Object**: Karmaşık nesne

#### File Types:
- **File**: Tek dosya
- **Image**: Resim
- **MultiFile**: Çoklu dosya

### 4. FormEntityRelation (Form-Entity İlişkileri)
Bir formun hangi entity'lere bağlı olduğunu tanımlar.

**İlişki Tipleri:**
- `OneToOne`: Bire-bir ilişki
- `OneToMany`: Bire-çok ilişki
- `ManyToMany`: Çoka-çok ilişki
- `Embedded`: Gömülü entity (JSON içinde)

**Örnek:**
```json
{
  "formId": "form-guid",
  "formEntityId": "customer-entity-guid",
  "relationName": "MainCustomer",
  "relationType": "OneToOne",
  "isPrimary": true,
  "isRequired": true,
  "formDataPath": "customer"
}
```

### 5. FormFieldMapping (Alan Mapping'leri)
Form elemanları ile entity alanları arasındaki eşleşmeyi tanımlar.

**Özellikler:**
- Form element ID ve field name
- Entity field ile ilişki
- Component tipi
- Transform kuralları (veri dönüşüm)
- Validation override
- Read-only flag
- Auto-mapping flag

**Örnek:**
```json
{
  "formId": "form-guid",
  "formEntityFieldId": "email-field-guid",
  "formElementId": "x-designer-id-123",
  "formFieldName": "customerEmail",
  "formComponentType": "Input",
  "isActive": true,
  "isReadOnly": false,
  "transformRules": {
    "type": "lowercase",
    "trim": true
  }
}
```

---

## Kullanım Senaryoları

### Senaryo 1: Müşteri Kayıt Formu

#### Adım 1: Entity Tanımla
```csharp
// Customer entity'sini oluştur
var customerEntity = new FormEntity
{
    EntityName = "Customer",
    EntityDescription = "Müşteri bilgileri",
    TableName = "Customers",
    SchemaName = "dbo",
    IsActive = true
};
```

#### Adım 2: Alanları Tanımla
```csharp
// FirstName field
var firstNameField = new FormEntityField
{
    FormEntityId = customerEntity.Id,
    FieldName = "FirstName",
    FieldTypeId = stringTypeId, // "00000000-0000-0000-0000-000000000001"
    IsRequired = true,
    MaxLength = 100,
    DisplayLabel = "Ad",
    DisplayOrder = 1
};

// Email field
var emailField = new FormEntityField
{
    FormEntityId = customerEntity.Id,
    FieldName = "Email",
    FieldTypeId = emailTypeId, // "00000000-0000-0000-0000-000000000009"
    IsRequired = true,
    IsUnique = true,
    MaxLength = 255,
    DisplayLabel = "E-posta",
    DisplayOrder = 2
};

// Phone field
var phoneField = new FormEntityField
{
    FormEntityId = customerEntity.Id,
    FieldName = "Phone",
    FieldTypeId = phoneTypeId, // "00000000-0000-0000-0000-000000000010"
    IsRequired = false,
    MaxLength = 50,
    DisplayLabel = "Telefon",
    DisplayOrder = 3
};
```

#### Adım 3: Form ile İlişkilendir
```csharp
// Form-Entity ilişkisi oluştur
var formEntityRelation = new FormEntityRelation
{
    FormId = formId,
    FormEntityId = customerEntity.Id,
    RelationName = "MainCustomer",
    RelationType = EntityRelationType.OneToOne,
    IsPrimary = true,
    IsRequired = true,
    FormDataPath = "customer"
};
```

#### Adım 4: Field Mapping Oluştur
```csharp
// Auto-mapping ile form elemanlarını entity alanlarına eşle
var autoMapDto = new AutoMapFormFieldsDto
{
    FormId = formId,
    FormEntityId = customerEntity.Id,
    FormEntityRelationId = formEntityRelation.Id,
    OverwriteExisting = false,
    MapOnlyUnmapped = true
};

// Service çağrısı ile auto-mapping yap
await formFieldMappingService.AutoMapFormFields(autoMapDto);
```

### Senaryo 2: Sipariş Formu (İlişkili Entity'ler)

```csharp
// 1. Customer entity (Müşteri)
var customerEntity = ...; // Yukarıdaki gibi

// 2. Order entity (Sipariş)
var orderEntity = new FormEntity
{
    EntityName = "Order",
    EntityDescription = "Sipariş bilgileri",
    TableName = "Orders",
    IsActive = true
};

// Order fields
var orderDateField = new FormEntityField
{
    FormEntityId = orderEntity.Id,
    FieldName = "OrderDate",
    FieldTypeId = dateTypeId,
    IsRequired = true,
    DisplayLabel = "Sipariş Tarihi"
};

var totalAmountField = new FormEntityField
{
    FormEntityId = orderEntity.Id,
    FieldName = "TotalAmount",
    FieldTypeId = decimalTypeId,
    IsRequired = true,
    DisplayLabel = "Toplam Tutar"
};

// Customer lookup field (Foreign Key)
var customerIdField = new FormEntityField
{
    FormEntityId = orderEntity.Id,
    FieldName = "CustomerId",
    FieldTypeId = lookupTypeId, // "00000000-0000-0000-0000-000000000020"
    RelatedEntityId = customerEntity.Id,
    LookupDisplayField = "FullName",
    LookupValueField = "Id",
    IsRequired = true,
    DisplayLabel = "Müşteri"
};

// 3. Form ile her iki entity'yi ilişkilendir
var customerRelation = new FormEntityRelation
{
    FormId = orderFormId,
    FormEntityId = customerEntity.Id,
    RelationName = "OrderCustomer",
    RelationType = EntityRelationType.OneToOne,
    IsPrimary = false,
    FormDataPath = "order.customer"
};

var orderRelation = new FormEntityRelation
{
    FormId = orderFormId,
    FormEntityId = orderEntity.Id,
    RelationName = "MainOrder",
    RelationType = EntityRelationType.OneToOne,
    IsPrimary = true,
    FormDataPath = "order"
};
```

### Senaryo 3: Dinamik Validation

```csharp
// Entity field seviyesinde validation
var emailField = new FormEntityField
{
    FieldName = "Email",
    FieldTypeId = emailTypeId,
    IsRequired = true,
    IsUnique = true,
    RegexPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexErrorMessage = "Geçerli bir email adresi giriniz",
    CustomValidationRules = @"{
        ""email"": true,
        ""domain"": ""company.com""
    }"
};

// Form mapping seviyesinde validation override
var fieldMapping = new FormFieldMapping
{
    FormEntityFieldId = emailField.Id,
    FormFieldName = "companyEmail",
    ValidationOverride = @"{
        ""required"": true,
        ""email"": true,
        ""pattern"": ""^[a-zA-Z0-9._%+-]+@company\.com$"",
        ""message"": ""Sadece company.com uzantılı emailler kabul edilir""
    }"
};
```

---

## Kurulum

### 1. Migration Oluştur

```bash
cd formneo.repository
dotnet ef migrations add AddEntityManagerTables -s ../formneo.api
```

### 2. Database Update

```bash
dotnet ef database update -s ../formneo.api
```

### 3. Seed Data Ekle

`formneo.api/Program.cs` veya startup kodunda:

```csharp
using formneo.core.Seed;

// Seed field types
var fieldTypes = FormEntityFieldTypeSeed.GetSeedData();
foreach (var fieldType in fieldTypes)
{
    if (!context.FormEntityFieldTypes.Any(x => x.Id == fieldType.Id))
    {
        context.FormEntityFieldTypes.Add(fieldType);
    }
}
await context.SaveChangesAsync();
```

---

## Örnekler

### API Kullanımı

#### Entity Oluşturma
```http
POST /api/form-entities
Content-Type: application/json

{
  "entityName": "Customer",
  "entityDescription": "Müşteri bilgileri",
  "tableName": "Customers",
  "isActive": true,
  "allowCreate": true,
  "allowRead": true,
  "allowUpdate": true,
  "allowDelete": false
}
```

#### Field Ekleme
```http
POST /api/form-entity-fields
Content-Type: application/json

{
  "formEntityId": "entity-guid",
  "fieldName": "Email",
  "fieldTypeId": "00000000-0000-0000-0000-000000000009",
  "isRequired": true,
  "isUnique": true,
  "maxLength": 255,
  "displayLabel": "E-posta Adresi"
}
```

#### Auto-Mapping
```http
POST /api/form-field-mappings/auto-map
Content-Type: application/json

{
  "formId": "form-guid",
  "formEntityId": "entity-guid",
  "overwriteExisting": false,
  "mapOnlyUnmapped": true
}
```

#### Field Listesi Alma
```http
GET /api/form-entities/entity-guid/fields
```

---

## Avantajlar

### 1. Merkezi Yönetim
- Tüm entity'ler ve alanları tek yerden yönetilir
- Değişiklikler tüm formlara otomatik yansır

### 2. Tip Güvenliği
- Her alan belirli bir tip ile tanımlanır
- Validation kuralları tip bazında kontrol edilir

### 3. Yeniden Kullanılabilirlik
- Entity'ler birden fazla formda kullanılabilir
- Aynı alanlar farklı formlarda tekrar kullanılabilir

### 4. Kolay Bakım
- Değişiklikler tek noktadan yapılır
- Alan tanımları merkezi olduğu için tutarlılık sağlanır

### 5. Otomatik Mapping
- Form elemanları otomatik olarak entity alanlarına eşlenebilir
- Manuel mapping de desteklenir

### 6. Validasyonlarda Esneklik
- Entity seviyesinde global validation
- Form seviyesinde override validation
- Custom validation kuralları

### 7. İlişkili Entity'ler
- Foreign Key ilişkileri
- Lookup alanları
- Many-to-Many ilişkiler

---

## Gelecek Geliştirmeler

- [ ] Entity migration tool (veritabanından entity import)
- [ ] Visual entity designer (drag & drop)
- [ ] Field dependency management (conditional fields)
- [ ] Version control için entity versiyonlama
- [ ] Entity template'leri (Customer, Order, Product vb.)
- [ ] Bulk import/export (Excel, CSV)
- [ ] Entity validation simulator
- [ ] GraphQL support
- [ ] Real-time collaboration

---

## Destek

Sorularınız için:
- 📧 Email: support@formneo.com
- 📚 Dokümantasyon: https://docs.formneo.com
- 💬 Slack: #entity-manager
