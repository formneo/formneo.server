using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using formneo.core.DTOs;

namespace formneo.api.Helpers
{
    /// <summary>
    /// Workflow definition JSON'dan formNode ve formTaskNode'lardaki source: "user" butonlarını çıkarır.
    /// </summary>
    public static class WorkflowDefinitionButtonExtractor
    {
        /// <summary>
        /// Birden fazla workflow definition'dan bu forma ait formTaskNode ve formNode'lardaki source: "user" butonlarını çıkarır.
        /// </summary>
        public static List<FormTaskNodeButtonDto>? ExtractUserButtons(
            List<string> definitionJsonList,
            Guid formId,
            Guid? parentFormId = null)
        {
            if (definitionJsonList == null || definitionJsonList.Count == 0)
                return null;

            var allButtons = new List<FormTaskNodeButtonDto>();
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var formIdsToMatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                formId.ToString()
            };
            if (parentFormId.HasValue)
                formIdsToMatch.Add(parentFormId.Value.ToString());

            foreach (var definitionJson in definitionJsonList)
            {
                if (string.IsNullOrEmpty(definitionJson)) continue;

                try
                {
                    var definition = JObject.Parse(definitionJson);
                    var nodes = definition["nodes"] as JArray;
                    if (nodes == null) continue;

                    foreach (var node in nodes)
                    {
                        var nodeType = node["type"]?.ToString() ?? node["nodeClazz"]?.ToString();
                        var isFormTaskOrFormNode = !string.IsNullOrEmpty(nodeType) && (
                            nodeType.Contains("FormTask", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(nodeType, "formNode", StringComparison.OrdinalIgnoreCase));
                        if (!isFormTaskOrFormNode)
                            continue;

                        var nodeFormId = node["data"]?["formId"]?.ToString()
                            ?? node["data"]?["FormId"]?.ToString()
                            ?? node["data"]?["code"]?.ToString();
                        if (string.IsNullOrEmpty(nodeFormId))
                        {
                            var nodeId = node["id"]?.ToString();
                            if (!string.IsNullOrEmpty(nodeId) && nodeId.StartsWith("formNode-", StringComparison.OrdinalIgnoreCase))
                                nodeFormId = nodeId.Substring("formNode-".Length);
                        }
                        if (string.IsNullOrEmpty(nodeFormId) || !formIdsToMatch.Contains(nodeFormId))
                            continue;

                        var buttonsArray = node["data"]?["buttons"] as JArray;
                        if (buttonsArray == null)
                            continue;

                        var currentNodeId = node["id"]?.ToString();

                        foreach (var b in buttonsArray)
                        {
                            // Sadece source: "user" olan butonları al
                            var source = b["source"]?.ToString();
                            if (!string.Equals(source, "user", StringComparison.OrdinalIgnoreCase))
                                continue;

                            var btnId = b["id"]?.ToString();
                            if (!string.IsNullOrEmpty(btnId) && seenIds.Contains(btnId))
                                continue;
                            if (!string.IsNullOrEmpty(btnId))
                                seenIds.Add(btnId);

                            allButtons.Add(new FormTaskNodeButtonDto
                            {
                                Id = btnId,
                                Label = b["label"]?.ToString(),
                                Action = b["action"]?.ToString(),
                                Type = b["type"]?.ToString(),
                                Icon = b["icon"]?.ToString(),
                                Color = b["color"]?.ToString(),
                                Name = b["name"]?.ToString(),
                                Description = b["description"]?.ToString(),
                                Visible = b["visible"]?.Value<bool?>(),
                                Source = "user",  // Sadece user butonları döndüğü için her zaman "user"
                                NodeId = currentNodeId,
                                NodeType = nodeType
                            });
                        }
                    }
                }
                catch
                {
                    // Parse hatası diğerlerini etkilemesin
                }
            }

            return allButtons.Count > 0 ? allButtons : null;
        }
    }
}
