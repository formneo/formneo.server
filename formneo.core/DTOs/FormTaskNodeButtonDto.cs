using System;

namespace formneo.core.DTOs
{
    /// <summary>
    /// FormTaskNode ve formNode'daki buton bilgisi (workflow definition'dan alınır)
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
        /// <summary>
        /// Her zaman "user" - sadece kullanıcı tanımlı butonlar döner
        /// </summary>
        public string? Source { get; set; }
        /// <summary>
        /// Bu butonun geldiği node'un id'si (örn: formNode-xxx, formTaskNode guid)
        /// </summary>
        public string? NodeId { get; set; }
        /// <summary>
        /// Bu butonun geldiği node tipi (formNode veya formTaskNode)
        /// </summary>
        public string? NodeType { get; set; }
    }
}
