using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models.FormEnums;

namespace formneo.core.DTOs.FormDatas
{
    public class FormDataListDto : BaseListDto
    {
      
        public string? FormName { get; set; }
        public string? FormDescription { get; set; }
        public int Revision { get; set; }
        public string FormDesign { get; set; }

        public int IsActive { get; set; }

        public string JavaScriptCode { get; set; }
        public FormType FormType { get; set; }
        public string FormTypeText { get; set; }
        public FormCategory FormCategory { get; set; }
        public string FormCategoryText { get; set; }
        public FormPriority FormPriority { get; set; }
        public string FormPriorityText { get; set; }
        public Guid? WorkFlowDefinationId { get; set; }
        public string? WorkFlowName { get; set; }
        public Guid? ParentFormId { get; set; }
        public bool CanEdit { get; set; }
        public bool ShowInMenu { get; set; }

        public FormPublicationStatus PublicationStatus { get; set; }
        public string? PublicationStatusText { get; set; }

        /// <summary>
        /// Workflow definition'daki formTaskNode'lardan alınan kullanıcı butonları (source: "user" olanlar)
        /// Bu forma ait formTaskNode'lardaki buttons dizisinden filtrelenir
        /// </summary>
        public List<FormTaskNodeButtonDto>? Buttons { get; set; }
    }
}
