using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.Models.FormEnums;

namespace formneo.core.Models
{
    public class Form : BaseEntity
    {

        public string FormName { get; set; }
        public string FormDescription { get; set; }

        public int Revision { get; set; }
        public string FormDesign { get; set; }

        public int IsActive { get; set; }

        public string JavaScriptCode { get; set; }

        [Required]
        [EnumDataType(typeof(FormType))]
        public FormType FormType { get; set; }

        [Required]
        [EnumDataType(typeof(FormCategory))]
        public FormCategory FormCategory { get; set; }

        [Required]
        [EnumDataType(typeof(FormPriority))]
        public FormPriority FormPriority { get; set; }

        [ForeignKey("WorkFlowDefination")]
        public Guid? WorkFlowDefinationId { get; set; }

        public virtual WorkFlowDefination? WorkFlowDefination { get; set; }

        public Guid? ParentFormId { get; set; }
        public bool CanEdit { get; set; }
        public bool ShowInMenu { get; set; }

        [Required]
        [EnumDataType(typeof(FormPublicationStatus))]
        public FormPublicationStatus PublicationStatus { get; set; }

        /// <summary>
        /// Form ile ilişkili entity ilişkileri (Entity Manager)
        /// </summary>
        public virtual ICollection<EntityManager.FormEntityRelation> EntityRelations { get; set; }

        /// <summary>
        /// Form ile ilişkili field mapping'ler (Entity Manager)
        /// </summary>
        public virtual ICollection<EntityManager.FormFieldMapping> FieldMappings { get; set; }

    }
}
