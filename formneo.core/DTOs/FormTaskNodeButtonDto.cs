using System;

namespace formneo.core.DTOs
{
    /// <summary>
    /// FormTaskNode'daki buton bilgisi (workflow definition'dan alınır)
    /// Sadece source: "user" olan butonlar API yanıtlarına dahil edilir
    /// </summary>
    public class FormTaskNodeButtonDto
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Action { get; set; }
        public string? Type { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? Visible { get; set; }
        public string? Source { get; set; }
    }
}
