using System;
using System.Linq;
using AutoMapper;
using formneo.core.DTOs;
using formneo.core.Models;
using Newtonsoft.Json.Linq;

namespace formneo.workflow.Services
{
    /// <summary>
    /// Workflow response'u oluşturmak için helper service
    /// Separation of Concerns - Controller'dan logic'i ayırır
    /// </summary>
    public class WorkflowResponseBuilder
    {
        private readonly NodeInfoExtractorFactory _nodeInfoExtractorFactory;
        private readonly IMapper _mapper;

        public WorkflowResponseBuilder(IMapper mapper)
        {
            _nodeInfoExtractorFactory = new NodeInfoExtractorFactory();
            _mapper = mapper;
        }

        /// <summary>
        /// WorkflowHead'den response DTO oluşturur
        /// AlertNode'a gelince rollback yapıldıysa (Id == Guid.Empty) alert bilgilerini çıkarır
        /// </summary>
        public WorkFlowHeadDtoResultStartOrContinue BuildResponse(WorkflowHead? workflowHead)
        {
            // Rollback durumu: workflowHead.Id == Guid.Empty ise rollback yapıldı demektir
            if (workflowHead != null && workflowHead.Id == Guid.Empty)
            {
                // Rollback yapıldı, alert bilgilerini çıkar
                var pendingAlertNode = workflowHead.workflowItems?.FirstOrDefault(item => 
                    item.NodeType == "alertNode" && item.workFlowNodeStatus == WorkflowStatus.Pending);
                
                if (pendingAlertNode != null && !string.IsNullOrEmpty(workflowHead.WorkFlowDefinationJson))
                {
                    JObject? rollbackWorkflowDefinition = null;
                    try
                    {
                        rollbackWorkflowDefinition = JObject.Parse(workflowHead.WorkFlowDefinationJson);
                    }
                    catch { }
                    
                    var rollbackResponse = new WorkFlowHeadDtoResultStartOrContinue
                    {
                        Id = Guid.Empty.ToString(), // Rollback yapıldı, ID yok
                        WorkFlowInfo = workflowHead.WorkFlowInfo,
                        WorkFlowStatus = WorkflowStatus.Pending,
                        PendingNodeId = pendingAlertNode.NodeId,
                        IsCompleted = false,
                        IsSuccessfullyCompleted = false
                    };
                    
                    rollbackResponse.CurrentNodeInfo = _nodeInfoExtractorFactory.ExtractNodeInfo(pendingAlertNode, rollbackWorkflowDefinition);
                    PopulateBackwardCompatibility(rollbackResponse, workflowHead, rollbackWorkflowDefinition);
                    
                    return rollbackResponse;
                }
            }
            
            if (workflowHead == null)
            {
                return new WorkFlowHeadDtoResultStartOrContinue
                {
                    Id = Guid.Empty.ToString(),
                    WorkFlowStatus = null
                };
            }

            var response = _mapper.Map<WorkFlowHeadDtoResultStartOrContinue>(workflowHead);
            
            // Workflow durumunu belirle
            response.WorkFlowStatus = workflowHead.workFlowStatus;
            response.IsCompleted = workflowHead.workFlowStatus == WorkflowStatus.Completed;
            
            // StopNode'a ulaşıldı mı kontrol et
            var stopNode = workflowHead.workflowItems?.FirstOrDefault(item => 
                item.NodeType == "stopNode" && item.workFlowNodeStatus == WorkflowStatus.Completed);
            response.IsSuccessfullyCompleted = stopNode != null;

            // Workflow definition'ı parse et
            JObject? workflowDefinition = null;
            if (!string.IsNullOrEmpty(workflowHead.WorkFlowDefinationJson))
            {
                try
                {
                    workflowDefinition = JObject.Parse(workflowHead.WorkFlowDefinationJson);
                }
                catch
                {
                    // Parse hatası durumunda devam et
                }
            }

            // Pending durumundaki node'u bul ve bilgilerini çıkar
            var pendingNode = FindPendingNode(workflowHead);
            if (pendingNode != null)
            {
                response.PendingNodeId = pendingNode.NodeId;
                response.CurrentNodeInfo = _nodeInfoExtractorFactory.ExtractNodeInfo(pendingNode, workflowDefinition);
            }
            else
            {
                // Pending node yoksa, son completed node'u veya stopNode'u göster
                var lastNode = workflowHead.workflowItems?
                    .OrderByDescending(item => item.CreatedDate)
                    .FirstOrDefault();
                
                if (lastNode != null)
                {
                    response.CurrentNodeInfo = _nodeInfoExtractorFactory.ExtractNodeInfo(lastNode, workflowDefinition);
                }
            }

            // Backward compatibility için eski property'leri doldur
            PopulateBackwardCompatibility(response, workflowHead, workflowDefinition);

            return response;
        }

        /// <summary>
        /// Pending durumundaki node'u bulur
        /// </summary>
        private WorkflowItem? FindPendingNode(WorkflowHead workflowHead)
        {
            if (workflowHead.workflowItems == null) 
                return null;

            // Öncelik sırası: alertNode > formNode > formTaskNode > approverNode
            return workflowHead.workflowItems.FirstOrDefault(item => 
                item.workFlowNodeStatus == WorkflowStatus.Pending && 
                (item.NodeType == "alertNode" || item.NodeType == "formNode" || item.NodeType == "formTaskNode" || item.NodeType == "approverNode")) 
                ?? workflowHead.workflowItems.FirstOrDefault(item => 
                    item.workFlowNodeStatus == WorkflowStatus.Pending);
        }

        /// <summary>
        /// Backward compatibility için eski property'leri doldurur
        /// </summary>
        private void PopulateBackwardCompatibility(
            WorkFlowHeadDtoResultStartOrContinue response, 
            WorkflowHead workflowHead, 
            JObject? workflowDefinition)
        {
            // AlertInfo için backward compatibility
            if (response.CurrentNodeInfo is AlertNodeInfo alertInfo)
            {
                response.AlertInfo = alertInfo;
            }

            // FormNodeCompleted için backward compatibility
            if (response.CurrentNodeInfo is FormNodeInfo formInfo)
            {
                response.FormNodeCompleted = formInfo.IsCompleted;
                response.CompletedFormNodeId = formInfo.IsCompleted ? formInfo.NodeId : null;
            }
        }
    }
}

