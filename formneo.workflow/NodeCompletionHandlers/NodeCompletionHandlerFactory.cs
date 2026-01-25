using System;
using System.Collections.Generic;
using System.Linq;

namespace formneo.workflow.NodeCompletionHandlers
{
    /// <summary>
    /// Factory pattern: Node tipine göre doğru handler'ı döndürür
    /// </summary>
    public class NodeCompletionHandlerFactory
    {
        private readonly Dictionary<string, INodeCompletionHandler> _handlers;

        public NodeCompletionHandlerFactory()
        {
            // Tüm handler'ları kaydet
            _handlers = new Dictionary<string, INodeCompletionHandler>
            {
                { "formTaskNode", new FormTaskNodeCompletionHandler() },
                { "approverNode", new ApproverNodeCompletionHandler() }
                // Gelecekte yeni node tipleri buraya eklenebilir:
                // { "emailNode", new EmailNodeCompletionHandler() },
                // { "scriptNode", new ScriptNodeCompletionHandler() },
            };
        }

        /// <summary>
        /// Node tipine göre handler döndür
        /// </summary>
        public INodeCompletionHandler GetHandler(string nodeType)
        {
            if (_handlers.TryGetValue(nodeType, out var handler))
            {
                return handler;
            }

            // Default: Hiçbir şey yapma
            return new DefaultNodeCompletionHandler();
        }
    }

    /// <summary>
    /// Default handler - Bilinmeyen node tipleri için
    /// </summary>
    public class DefaultNodeCompletionHandler : INodeCompletionHandler
    {
        public string NodeType => "default";

        public Task<NodeCompletionResult> HandleAsync(NodeCompletionContext context)
        {
            // Hiçbir şey yapma
            return Task.FromResult(NodeCompletionResult.Empty);
        }
    }
}

