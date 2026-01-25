using System.Threading.Tasks;

namespace formneo.workflow.NodeCompletionHandlers
{
    /// <summary>
    /// ApproverNode tamamlandığında yapılacak işlemler
    /// ApproveItem zaten UpdateApproverItemAsync() tarafından güncellendi
    /// Ek işlem gerekmiyor!
    /// </summary>
    public class ApproverNodeCompletionHandler : INodeCompletionHandler
    {
        public string NodeType => "approverNode";

        public Task<NodeCompletionResult> HandleAsync(NodeCompletionContext context)
        {
            // ApproverNode için ek işlem yok
            // ApproveItem zaten ContinueWorkflowAsync içinde güncellendi
            // FormInstance güncellemeye gerek yok (form doldurulmadı, sadece onay verildi)
            
            return Task.FromResult(NodeCompletionResult.Empty);
        }
    }
}

