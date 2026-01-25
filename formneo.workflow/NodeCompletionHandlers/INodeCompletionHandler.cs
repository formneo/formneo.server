using System;
using System.Threading.Tasks;
using formneo.core.DTOs;
using formneo.core.Models;
using formneo.core.Operations;

namespace formneo.workflow.NodeCompletionHandlers
{
    /// <summary>
    /// Node tamamlandığında yapılacak işlemleri tanımlar
    /// Her node tipi için ayrı handler implementasyonu olacak
    /// </summary>
    public interface INodeCompletionHandler
    {
        /// <summary>
        /// Bu handler hangi node tipini handle eder?
        /// </summary>
        string NodeType { get; }

        /// <summary>
        /// Node tamamlandığında çağrılır
        /// </summary>
        Task<NodeCompletionResult> HandleAsync(NodeCompletionContext context);
    }

    /// <summary>
    /// Node completion context (handler'a gönderilen data)
    /// </summary>
    public class NodeCompletionContext
    {
        public WorkflowItem Node { get; set; }
        public WorkFlowDto Dto { get; set; }
        public string PayloadJson { get; set; }
        public Guid WorkflowHeadId { get; set; }
        public WorkFlowParameters Parameters { get; set; }
    }

    /// <summary>
    /// Node completion sonucu
    /// </summary>
    public class NodeCompletionResult
    {
        public ApproveItems ApproverItem { get; set; }
        public FormItems FormItem { get; set; }
        public FormInstance FormInstance { get; set; }

        public static NodeCompletionResult Empty => new NodeCompletionResult();
    }
}

