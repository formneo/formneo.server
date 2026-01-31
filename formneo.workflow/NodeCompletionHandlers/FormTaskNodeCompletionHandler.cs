using System;
using System.Linq;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Services;
using formneo.core.Operations;
using Microsoft.EntityFrameworkCore;

namespace formneo.workflow.NodeCompletionHandlers
{
    /// <summary>
    /// FormTaskNode tamamlandığında yapılacak işlemler (CONTINUE durumu)
    /// SADE VE TEMİZ: Completion handler sadece tamamlama işlerini yapar
    /// </summary>
    public class FormTaskNodeCompletionHandler : INodeCompletionHandler
    {
        public string NodeType => "formTaskNode";

        public async Task<NodeCompletionResult> HandleAsync(NodeCompletionContext context)
        {
            // FormItem yoksa çık
            if (context.Node.formItems == null || !context.Node.formItems.Any())
            {
                return NodeCompletionResult.Empty;
            }

            // 1. Kullanıcının FormItem'ını bul
            var userId = await ResolveUserIdAsync(context.Dto.UserName, context.Parameters);
            var formItem = context.Node.formItems.FirstOrDefault(fi => fi.FormUserId == userId);

            // ✅ İlgili kullanıcının FormItem'ı OLMALIDIR!
            // Yoksa bu kullanıcı bu forma atanmamış demektir
            if (formItem == null)
            {
                // Güvenlik: Bu kullanıcı bu forma yetkili değil!
                throw new UnauthorizedAccessException(
                    $"User {userId} is not assigned to this FormTask. " +
                    $"Only assigned users can complete this form.");
            }

            // 2. ✅ FormItem'ı COMPLETED YAP (Handler'ın ana görevi!)
            formItem.FormItemStatus = FormItemStatus.Completed;

            // 3. ✅ FormData'yı KAYDET
            if (!string.IsNullOrEmpty(context.PayloadJson))
            {
                formItem.FormData = context.PayloadJson;
            }

            // 4. Note ekle
            if (context.Dto.Note != null)
            {
                formItem.FormUserMessage = context.Dto.Note;
            }

            // 5. Diğer FormItem'ları da Completed yap (biri doldurunca diğerleri kapanır)
            if (context.Node.formItems.Count > 1)
            {
                foreach (var otherFormItem in context.Node.formItems)
                {
                    if (otherFormItem.Id != formItem.Id)
                    {
                        otherFormItem.FormItemStatus = FormItemStatus.Completed;
                    }
                }
            }

            // 6. FormInstance oluştur/güncelle
            FormInstance formInstance = null;
            if (!string.IsNullOrEmpty(context.PayloadJson))
            {
                formInstance = await CreateOrUpdateFormInstanceAsync(
                    formItem, 
                    context.Dto, 
                    context.PayloadJson, 
                    context.WorkflowHeadId,
                    context.Parameters);
            }

            return new NodeCompletionResult
            {
                FormItem = formItem,
                FormInstance = formInstance
            };
        }

        #region Helper Methods

        private async Task<string> ResolveUserIdAsync(string userName, WorkFlowParameters parameters)
        {
            if (parameters?.UserManager == null || string.IsNullOrEmpty(userName))
                return userName;

            try
            {
                var user = await parameters.UserManager.FindByNameAsync(userName);
                return user?.Id ?? userName;
            }
            catch
            {
                return userName;
            }
        }

        private async Task<FormInstance> CreateOrUpdateFormInstanceAsync(
            FormItems formItem,
            WorkFlowDto dto,
            string payloadJson,
            Guid workflowHeadId,
            WorkFlowParameters parameters)
        {
            // FormDesign yükle (gerekirse)
            string formDesign = formItem.FormDesign;
            if (string.IsNullOrEmpty(formDesign) && formItem.FormId.HasValue)
            {
                formDesign = await LoadFormDesignAsync(formItem.FormId.Value, parameters);
                if (!string.IsNullOrEmpty(formDesign))
                {
                    formItem.FormDesign = formDesign;
                }
            }

            // Mevcut FormInstance kontrol et
            var existingInstance = await parameters._formInstanceService
                .Where(e => e.WorkflowHeadId == workflowHeadId)
                .FirstOrDefaultAsync();

            var utils = new Utils();
            string userNameSurname = utils.GetNameAndSurnameAsync(dto.UserName).ToString();

            if (existingInstance != null)
            {
                // Güncelle
                existingInstance.FormData = payloadJson;
                existingInstance.FormDesign = formDesign;
                existingInstance.FormId = formItem.FormId;
                existingInstance.UpdatedBy = dto.UserName;
                existingInstance.UpdatedByNameSurname = userNameSurname;
                existingInstance.UpdatedDate = DateTime.Now;
                return existingInstance;
            }
            else
            {
                // Yeni oluştur
                return new FormInstance
                {
                    WorkflowHeadId = workflowHeadId,
                    FormId = formItem.FormId,
                    FormDesign = formDesign,
                    FormData = payloadJson,
                    UpdatedBy = dto.UserName,
                    UpdatedByNameSurname = userNameSurname
                };
            }
        }

        private async Task<string> LoadFormDesignAsync(Guid formId, WorkFlowParameters parameters)
        {
            try
            {
                var form = await parameters._formService.GetByIdStringGuidAsync(formId);
                return form?.FormDesign ?? "";
            }
            catch
            {
                return "";
            }
        }

        #endregion
    }
}

