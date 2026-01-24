using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.UnitOfWorks;
using formneo.repository;
using formneo.repository.Repositories;
using formneo.repository.UnitOfWorks;
using static System.Formats.Asn1.AsnWriter;

namespace formneo.service.Services
{
    public class WorkFlowService : Service<WorkflowHead>, IWorkFlowService
    {

        private readonly IWorkflowRepository _workFlowRepository;
        private readonly IWorkFlowItemRepository _workFlowItemRepository;
        private readonly IApproveItemsRepository _approveItemsRepository;
        private readonly IFormItemsRepository _formItemsRepository;
        private readonly IFormInstanceRepository _formInstanceRepository;


        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkFlowService(IGenericRepository<WorkflowHead> repository, IUnitOfWork unitOfWork, IMapper mapper, IWorkflowRepository workFlowRepository, IWorkFlowItemRepository workFlowItemRepository, IApproveItemsRepository approveItemsRepository, IFormItemsRepository formItemsRepository, IFormInstanceRepository formInstanceRepository) : base(repository, unitOfWork)
        {
            _mapper = mapper;

           //var s=  workFlowItemRepository.GetAll();
 
            _workFlowRepository = workFlowRepository;
            _workFlowItemRepository = workFlowItemRepository;
            _approveItemsRepository = approveItemsRepository;
            _formItemsRepository = formItemsRepository;
            _formInstanceRepository = formInstanceRepository;

            _unitOfWork = unitOfWork;
        }

        public async Task<WorkflowHead> GetWorkFlowWitId(Guid guid)
        {

            var result = await _workFlowRepository.GetWorkFlowWitId(guid);
            return result;
        }

        public async Task<bool> UpdateWorkFlowAndRelations(WorkflowHead head, List<WorkflowItem> workflowItems, ApproveItems approveItems = null, FormItems formItem = null, FormInstance formInstance = null)
        {
            _unitOfWork.BeginTransaction();

            try
            {
                // ✅ THREADING FIX: Tüm ID'leri topla ve TEK SEFERDE yükle
                var idsToLoad = new List<Guid>();
                if (approveItems != null && approveItems.Id != Guid.Empty) idsToLoad.Add(approveItems.Id);
                if (formItem != null && formItem.Id != Guid.Empty) idsToLoad.Add(formItem.Id);
                if (formInstance != null && formInstance.Id != Guid.Empty) idsToLoad.Add(formInstance.Id);
                if (workflowItems != null)
                {
                    foreach (var item in workflowItems)
                    {
                        if (item.formItems != null)
                        {
                            idsToLoad.AddRange(item.formItems.Where(fi => fi.Id != Guid.Empty).Select(fi => fi.Id));
                        }
                    }
                }

                // ✅ Tüm entity'leri TEK QUERY ile yükle (Context'e tek seferlik erişim)
                var unitOfWork = _unitOfWork as formneo.repository.UnitOfWorks.UnitOfWork;
                Dictionary<Guid, object> loadedEntities = new Dictionary<Guid, object>();
                
                if (unitOfWork != null && idsToLoad.Count > 0)
                {
                    // ApproveItems'ları yükle
                    if (approveItems != null && approveItems.Id != Guid.Empty)
                    {
                        var loadedApproveItems = await unitOfWork._context.ApproveItems
                            .Where(x => idsToLoad.Contains(x.Id))
                            .ToListAsync();
                        foreach (var item in loadedApproveItems) loadedEntities[item.Id] = item;
                    }
                    
                    // FormItems'ları yükle
                    var loadedFormItems = await unitOfWork._context.FormItems
                        .Where(x => idsToLoad.Contains(x.Id))
                        .ToListAsync();
                    foreach (var item in loadedFormItems) loadedEntities[item.Id] = item;
                    
                    // FormInstance'ları yükle
                    if (formInstance != null && formInstance.Id != Guid.Empty)
                    {
                        var loadedFormInstances = await unitOfWork._context.FormInstance
                            .Where(x => idsToLoad.Contains(x.Id))
                            .ToListAsync();
                        foreach (var item in loadedFormInstances) loadedEntities[item.Id] = item;
                    }
                }

                // WorkflowHead'i güncelle
                if (head.Id != Guid.Empty)
                {
                    WorkflowHead existingHead;
                    if (unitOfWork != null)
                    {
                        existingHead = await unitOfWork._context.WorkflowHead
                            .Include(e => e.workflowItems)
                            .FirstOrDefaultAsync(x => x.Id == head.Id);
                    }
                    else
                    {
                        existingHead = await _workFlowRepository.GetByIdStringGuidAsync(head.Id);
                    }
                    
                    if (existingHead == null)
                    {
                        throw new Exception($"WorkflowHead with id '{head.Id}' not found");
                    }
                    
                    existingHead.WorkflowName = head.WorkflowName;
                    existingHead.CurrentNodeId = head.CurrentNodeId;
                    existingHead.CurrentNodeName = head.CurrentNodeName;
                    existingHead.workFlowStatus = head.workFlowStatus;
                    existingHead.WorkFlowInfo = head.WorkFlowInfo;
                    existingHead.FormId = head.FormId;
                    existingHead.WorkFlowDefinationId = head.WorkFlowDefinationId;
                    existingHead.WorkFlowDefinationJson = head.WorkFlowDefinationJson;
                    
                    if (existingHead.workflowItems != null && workflowItems != null)
                    {
                        foreach (var incomingItem in workflowItems)
                        {
                            var existingItem = existingHead.workflowItems.FirstOrDefault(e => e.Id == incomingItem.Id);
                            if (existingItem != null)
                            {
                                existingItem.NodeName = incomingItem.NodeName;
                                existingItem.NodeType = incomingItem.NodeType;
                                existingItem.NodeDescription = incomingItem.NodeDescription;
                                existingItem.workFlowNodeStatus = incomingItem.workFlowNodeStatus;
                            }
                        }
                    }
                }
                else
                {
                    await _workFlowRepository.AddAsync(head);
                }

                // ApproverItem'ı güncelle (memory'den)
                if (approveItems != null)
                {
                    try
                    {
                        if (loadedEntities.TryGetValue(approveItems.Id, out var existingObj) && existingObj is ApproveItems existingApproverItem)
                        {
                            existingApproverItem.ApproverStatus = approveItems.ApproverStatus;
                            existingApproverItem.ApprovedUser_Runtime = approveItems.ApprovedUser_Runtime;
                            existingApproverItem.ApprovedUser_RuntimeNameSurname = approveItems.ApprovedUser_RuntimeNameSurname;
                            existingApproverItem.ApprovedUser_RuntimeNote = approveItems.ApprovedUser_RuntimeNote;
                            existingApproverItem.ApprovedUser_RuntimeNumberManDay = approveItems.ApprovedUser_RuntimeNumberManDay;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ApproverItem update error: {ex.Message}");
                    }
                }

                // FormItem'ı güncelle (memory'den)
                if (formItem != null)
                {
                    if (formItem.Id == Guid.Empty)
                    {
                        await _formItemsRepository.AddAsync(formItem);
                    }
                    else
                    {
                        try
                        {
                            if (loadedEntities.TryGetValue(formItem.Id, out var existingObj) && existingObj is FormItems existingFormItem)
                            {
                                existingFormItem.FormDesign = formItem.FormDesign;
                                existingFormItem.FormId = formItem.FormId;
                                existingFormItem.FormUser = formItem.FormUser;
                                existingFormItem.FormUserNameSurname = formItem.FormUserNameSurname;
                                existingFormItem.FormData = formItem.FormData;
                                existingFormItem.FormDescription = formItem.FormDescription;
                                existingFormItem.FormUserMessage = formItem.FormUserMessage;
                                existingFormItem.FormTaskMessage = formItem.FormTaskMessage;
                                existingFormItem.FormItemStatus = formItem.FormItemStatus;
                            }
                            else
                            {
                                formItem.Id = Guid.Empty;
                                await _formItemsRepository.AddAsync(formItem);
                            }
                        }
                        catch
                        {
                            formItem.Id = Guid.Empty;
                            await _formItemsRepository.AddAsync(formItem);
                        }
                    }
                }

                // FormInstance'ı güncelle (memory'den)
                if (formInstance != null)
                {
                    if (formInstance.Id == Guid.Empty)
                    {
                        await _formInstanceRepository.AddAsync(formInstance);
                    }
                    else
                    {
                        try
                        {
                            if (loadedEntities.TryGetValue(formInstance.Id, out var existingObj) && existingObj is FormInstance existingFormInstance)
                            {
                                existingFormInstance.WorkflowHeadId = formInstance.WorkflowHeadId;
                                existingFormInstance.FormId = formInstance.FormId;
                                existingFormInstance.FormDesign = formInstance.FormDesign;
                                existingFormInstance.FormData = formInstance.FormData;
                                existingFormInstance.UpdatedBy = formInstance.UpdatedBy;
                                existingFormInstance.UpdatedByNameSurname = formInstance.UpdatedByNameSurname;
                                existingFormInstance.UpdatedDate = formInstance.UpdatedDate;
                            }
                            else
                            {
                                formInstance.Id = Guid.Empty;
                                await _formInstanceRepository.AddAsync(formInstance);
                            }
                        }
                        catch
                        {
                            formInstance.Id = Guid.Empty;
                            await _formInstanceRepository.AddAsync(formInstance);
                        }
                    }
                }

                // WorkflowItems içindeki FormItems'ları kaydet (memory'den)
                if (workflowItems != null)
                {
                    foreach (var item in workflowItems)
                    {
                        if (item.formItems != null && item.formItems.Count > 0)
                        {
                            foreach (var fi in item.formItems)
                            {
                                if (fi.Id == Guid.Empty)
                                {
                                    await _formItemsRepository.AddAsync(fi);
                                }
                                else
                                {
                                    if (loadedEntities.TryGetValue(fi.Id, out var existingObj) && existingObj is FormItems existingFi)
                                    {
                                        existingFi.FormDesign = fi.FormDesign;
                                        existingFi.FormId = fi.FormId;
                                        existingFi.FormUser = fi.FormUser;
                                        existingFi.FormUserNameSurname = fi.FormUserNameSurname;
                                        existingFi.FormData = fi.FormData;
                                        existingFi.FormDescription = fi.FormDescription;
                                        existingFi.FormUserMessage = fi.FormUserMessage;
                                        existingFi.FormTaskMessage = fi.FormTaskMessage;
                                        existingFi.FormItemStatus = fi.FormItemStatus;
                                    }
                                    else
                                    {
                                        await _formItemsRepository.AddAsync(fi);
                                    }
                                }
                            }
                        }
                    }
                }

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _unitOfWork.Rollback();
                throw new Exception($"Concurrency error: The data may have been modified or deleted by another process. {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                throw;
            }
        }


    }
}
 