using System;
using formneo.core.Models;

namespace formneo.core.DTOs
{
    /// <summary>
    /// Kullanıcının başlattığı formların listesi için DTO
    /// </summary>
    public class MyStartedFormsResultDto
    {
        public int TotalCount { get; set; }
        public List<MyStartedFormDto> Data { get; set; }
    }

    /// <summary>
    /// Başlatılan form detay DTO
    /// </summary>
    public class MyStartedFormDto
    {
        // Temel Bilgiler
        public Guid Id { get; set; }
        public string SurecAdi { get; set; }
        public string FormAdi { get; set; }
        public Guid WorkFlowDefinationId { get; set; }
        public int UniqNumber { get; set; }

        // Başlatan Bilgileri
        public string Baslatan { get; set; }
        public string BaslatanAdSoyad { get; set; }
        public string BaslatanDepartman { get; set; }
        public string BaslatanPozisyon { get; set; }

        // Süreç Durumu
        public string MevcutAdim { get; set; }
        public string Durum { get; set; }
        public WorkflowStatus? DurumEnum { get; set; }

        // Zaman Bilgileri
        public DateTime BaslangicTarihi { get; set; }
        public int Sure { get; set; }
        public string SureDetayli { get; set; }

        // Kimde Bekliyor Bilgileri
        public string KimdeOnayda { get; set; }
        public string BekleyenKullanici { get; set; }
        public string BekleyenDepartman { get; set; }
        public string BekleyenPozisyon { get; set; }
    }
}


