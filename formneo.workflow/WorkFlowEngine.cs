
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using System.Reflection.Metadata;
using formneo.core.Models;
using formneo.core.Services;
using formneo.core.Helpers;
using formneo.core.DTOs.EmployeeAssignments;
using formneo.workflow;
using Microsoft.AspNetCore.Mvc;
using formneo.core.Operations;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json.Linq;
using JsonLogic.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Jint;
using Jint.Native;
using NLayer.Core.Services;

public class WorkflowNode
{
    public string Id { get; set; }
    public string Type { get; set; }
    public Position Position { get; set; }
    public string ClassName { get; set; }
    public NodeData Data { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Selected { get; set; }
    public Position PositionAbsolute { get; set; }
    public bool Dragging { get; set; }

    public Guid HeadId { get; set; }


    public WorkflowItem Execute()
    {

        WorkflowItem item = new WorkflowItem();
        item.approveItems = new List<ApproveItems>();
        item.formItems = new List<FormItems>();

        item.WorkflowHeadId = HeadId;
        item.NodeName = Data.Name != null ? Data.Name : "WorkFlowName";
        item.NodeType = Type;
        item.NodeDescription = Data.Name != null ? Data.Name : "Description";
        item.CreatedDate = DateTime.Now;
        item.NodeId = Id;
        return item;
    }
}
public class Position
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class NodeData
{
    public string Name { get; set; }
    public string Text { get; set; }
    public string approvername { get; set; }
    public bool isManager { get; set; }
    public string code { get; set; }
    public string sqlQuery { get; set; }
    public stoptype stoptype { get; set; }
    /// <summary>
    /// ScriptNode için JavaScript kodu
    /// </summary>
    public string script { get; set; }
    /// <summary>
    /// ScriptNode için processDataTree (form verilerine erişim için)
    /// </summary>
    public object processDataTree { get; set; }
    /// <summary>
    /// FormTaskNode için mesaj (component'te gösterilecek)
    /// </summary>
    public string message { get; set; }
    /// <summary>
    /// FormTaskNode için mesaj (alternatif alan adı)
    /// </summary>
    public string formTaskMessage { get; set; }
    /// <summary>
    /// FormTaskNode için açıklama/mesaj (alternatif alan adı)
    /// </summary>
    public string description { get; set; }
    /// <summary>
    /// FormNode ve FormTaskNode için formId (Guid string olarak)
    /// </summary>
    public string formId { get; set; }
    /// <summary>
    /// FormNode ve FormTaskNode için formName
    /// </summary>
    public string formName { get; set; }
    
    /// <summary>
    /// FormTaskNode için assignmentType (direct_manager, department_manager, department_all, position, manual)
    /// </summary>
    public string assignmentType { get; set; }
    
    /// <summary>
    /// FormTaskNode için selectedOrgUnit (departman id'si)
    /// </summary>
    public string selectedOrgUnit { get; set; }
    
    /// <summary>
    /// FormTaskNode için selectedPosition (pozisyon id'si)
    /// </summary>
    public string selectedPosition { get; set; }
    
    /// <summary>
    /// FormTaskNode için selectionData (manual assignment için)
    /// </summary>
    public object selectionData { get; set; }
}

public class stoptype
{
    public string name { get; set; }
    public string code { get; set; }
}

public class Edges
{
    public string Source { get; set; }
    public string SourceHandle { get; set; }
    public string Target { get; set; }
    public string TargetHandle { get; set; }
    public bool Animated { get; set; }
    public LinkStyle Style { get; set; }
    public string Id { get; set; }
    public bool Selected { get; set; }
}



public class LinkStyle
{
    public string Stroke { get; set; }
}

// Kullanım örneği
public class WorkflowExample
{
    public List<WorkflowNode> Nodes { get; set; }
    public List<Edges> Edges { get; set; }
    public Viewport Viewport { get; set; }
    public int FirstNode { get; set; }
}

public class Viewport
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Zoom { get; set; }
}


public class Workflow
{

    public Guid _HeadId { get; set; }

    public List<WorkflowNode> Nodes { get; set; }
    public List<Edges> Edges { get; set; }
    public Dictionary<string, object> Properties { get; set; }

    public List<WorkflowItem>  _workFlowItems;

    private WorkflowHead _workFlowHead;

    public WorkFlowParameters _parameters;

    public string _payloadJson;

    public string _ApiSendUser { get; set; }
    
    /// <summary>
    /// Form başlatılırken gönderilen action kodu (örn: SAVE, APPROVE)
    /// </summary>
    public string _Action { get; set; }
    
    /// <summary>
    /// ✅ THREADING FIX: EmployeeAssignments'i class seviyesinde cache'le
    /// Böylece her ExecuteFormTaskNode çağrısında tekrar sorgu yapılmaz
    /// </summary>
    private List<formneo.core.Models.EmployeeAssignment> _cachedAssignments = null;

    /// <summary>
    /// ✅ THREADING FIX: EmployeeAssignments'i önceden yükle
    /// Bu metod Start() çağrısından ÖNCE çağrılmalı
    /// </summary>
    public async Task PreloadAssignmentsAsync()
    {
        if (_parameters?.EmployeeAssignmentService != null && _cachedAssignments == null)
        {
            try
            {
                var assignmentsQuery = await _parameters.EmployeeAssignmentService.Include();
                _cachedAssignments = await assignmentsQuery
                    .Include(ea => ea.OrgUnit)
                    .Include(ea => ea.Manager)
                    .ToListAsync();  // ✅ Tek seferlik DB çağrısı
            }
            catch
            {
                // Hata durumunda null kalır, ExecuteFormTaskNode içinde tekrar denenecek
                _cachedAssignments = null;
            }
        }
    }

    public async Task Start(string apiSendUser, string payloadJson, string action = "")
    {
        _ApiSendUser = await ResolveApiSendUserIdAsync(apiSendUser);
        _payloadJson = payloadJson;
        _Action = action; // Action'ı set et - formNode'a gelince kullanılacak
        
        // ✅ THREADING FIX: Fallback kaldırıldı - PreloadAssignmentsAsync() çağrılmalı
        // Eğer cache yoksa, ExecuteFormTaskNode içinde boş liste kullanılacak
        
        // İş akışını başlatmak için ilk düğümü bulun
        WorkflowNode startNode = Nodes.Find(node => node.Type == "startNode");
        
        if (startNode == null)
        {
            throw new Exception("StartNode bulunamadı!");
        }

        // StartNode'dan başla - action formNode'a gelince kullanılacak
        await ExecuteNode(startNode.Id, "");
    }

    public async Task Continue(WorkflowItem workfLowItem, string nodeId, string apiSendUser, string Parameter = "", WorkflowHead head = null, WorkFlowDefination defination = null,string payloadJson=null)
    {
        _ApiSendUser = await ResolveApiSendUserIdAsync(apiSendUser);
        // payloadJson'ı set et (FormData için)
        if (!string.IsNullOrEmpty(payloadJson))
        {
            _payloadJson = payloadJson;
        }
        
        // ✅ THREADING FIX: EmployeeAssignments'i bir kere yükle ve cache'le
        // Böylece ExecuteFormTaskNode içinde tekrar sorgu yapılmaz
        if (_parameters?.EmployeeAssignmentService != null && _cachedAssignments == null)
        {
            try
            {
                var assignmentsQuery = await _parameters.EmployeeAssignmentService.Include();
                _cachedAssignments = await assignmentsQuery
                    .Include(ea => ea.OrgUnit)
                    .Include(ea => ea.Manager)
                    .ToListAsync();  // ✅ Tek seferlik DB çağrısı
            }
            catch
            {
                // Hata durumunda null kalır, ExecuteFormTaskNode içinde tekrar denenecek
                _cachedAssignments = null;
            }
        }
        
        // İş akışını başlatmak için ilk düğümü bulun
        WorkflowNode startNode;
        _workFlowHead = head;

        startNode = Nodes.Find(node => node.Id == nodeId);

        // İlk düğümü çalıştırın
        await ExecuteNode(startNode.Id, Parameter, workfLowItem);
    }

    private async Task<string> ResolveApiSendUserIdAsync(string apiSendUser)
    {
        if (string.IsNullOrWhiteSpace(apiSendUser))
        {
            return apiSendUser;
        }

        var userManager = _parameters?.UserManager;
        if (userManager == null)
        {
            return apiSendUser;
        }

        try
        {
            // Önce ID olarak dene; bulunamazsa UserName ile dene.
            var user = await userManager.FindByIdAsync(apiSendUser);
            if (user != null)
            {
                return user.Id;
            }

            user = await userManager.FindByNameAsync(apiSendUser);
            if (user != null)
            {
                return user.Id;
            }
        }
        catch
        {
            // Hata durumunda gelen değeri kullan
        }

        return apiSendUser;
    }

    private async Task ExecuteNode(string nodeId, string Parameter = "", WorkflowItem workflowItem =null)
    {

        _ApiSendUser = _ApiSendUser;

        WorkflowNode currentNode = Nodes.Find(node => node.Id == nodeId);
        currentNode.HeadId = _HeadId;


        WorkflowItem result = null;
        // Düğümü çalıştırın
        if (workflowItem != null)
        {
            result = workflowItem;
        }
        else
        {
            result = currentNode.Execute();
        }

        if (result.NodeType == "stopNode")
        {
            string nextNode = ExecuteStopNode(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);

            }
            else
            {
                return;
            }

        }

        if (result.NodeType == "sqlConditionNode")
        {
            string nextNode = ExecuteSqlConditionNode(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);

            }
            else
            {
                return;
            }

        }
        
        if (result.NodeType == "scriptNode")
        {
            string nextNode = ExecuteScriptNode(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);
            }
            else
            {
                return;
            }
        }
        
        if (result.NodeType == "approverNode")
        {
            string nextNode = await ExecuteApprove(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);

            }
            else
            {
                return;
            }

        }
        if (result.NodeType == "formNode")
        {
            string nextNode = ExecuteFormNode(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);
            }
            else
            {
                return;
            }
        }
        if (result.NodeType == "formTaskNode")
        {
            string nextNode = await ExecuteFormTaskNode(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);
            }
            else
            {
                return;
            }
        }
        if (result.NodeType == "alertNode")
        {
            // AlertNode işleme - AlertNode'a gelince rollback yapılacak
            // AlertNode sadece error ve warning için kullanılır
            // Success ve info mesajları alert node'da kullanılmaz, normal component'te gösterilir
            string nextNode = ExecuteAlertNode(currentNode, result, Parameter);

            if (nextNode != "" && nextNode != null)
            {
                await ExecuteNode(nextNode);
            }
            else
            {
                return;
            }
        }
        if (result.NodeType == "EmailNode")
        {
            //string nextNode = ExecuteMailNode(currentNode, result, Parameter);

            //if (nextNode != "")
            //{
            //    ExecuteNode(nextNode);

            //}
            //else
            //{
            //    return;
            //}
        }
        if (result.NodeType != "EmailNode" && result.NodeType != "ApproverNode" && result.NodeType != "formNode" && result.NodeType != "formTaskNode" && result.NodeType != "alertNode" && result.NodeType != "scriptNode")
        {

            if (_workFlowItems.Contains(result))
            {

                return;
            }


            // Düğüme bağlı çıkış bağlantılarını bulun
            List<Edges> outgoingLinks = Edges.FindAll(link => link.Source == nodeId);
            result.workFlowNodeStatus = WorkflowStatus.Completed;



            _workFlowItems.Add(result);
            // Her çıkış bağlantısını takip edin
            foreach (var link in outgoingLinks)
            {
                string nextNodeId = link.Target;

                // Bir sonraki düğümü çalıştırın
                ExecuteNode(nextNodeId);
            }
        }
    }

    private string ExecuteSqlConditionNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    {
        //too

        //var normalized = JObject.Parse(JsonConvert.SerializeObject(JObject.Parse(_payloadJson)));
        //var list = new List<JObject> { normalized };

        //var expression = "TicketSubject == 2";
        //var result = list.AsQueryable().Where(expression).ToList();

        var rule = JObject.Parse(currentNode.Data.sqlQuery);

        var data = JObject.Parse(_payloadJson);
        // Create an evaluator with default operators.
        var evaluator = new JsonLogicEvaluator(EvaluateOperators.Default);

        // Apply the rule to the data.
        object result = evaluator.Apply(rule, data);


        //var expression = "Type  == 1";
        //var result = System.Linq.Dynamic.Core.DynamicExpressionParser.ParseLambda<ExpandoObject, bool>(new ParsingConfig(), false, expression).Compile().Invoke(payload);

        ////currentNode.Data.sqlQuery

        //var result = Utils.ExecuteSql(currentNode.Data.sqlQuery, _HeadId.ToString());


        if ((bool)result)
        {
            parameter = "yes";
        }

        // Düğüme bağlı çıkış bağlantılarını bulun
        var nextNode = FindLinkForPort(currentNode.Id, parameter);
        return nextNode;
    }

    private async Task<string> ExecuteApprove(WorkflowNode currentNode,WorkflowItem workFlowItem ,string parameter)
    {
        // TODO: ApproverNode mantığı buraya eklenecek
        // Şimdilik basit bir implementasyon - sonra güncellenecek
        
        if (parameter == "")
        {
            _workFlowItems.Add(workFlowItem);
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Pending;
            
            // TODO: ApproveItems oluşturma mantığı buraya gelecek
            // Şimdilik boş bırakıyoruz
            
            return "";
        }
        
        // Düğüme bağlı çıkış bağlantılarını bulun
        if (parameter == "yes" || parameter == "no")
        {
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
        }
        var nextNode = FindLinkForPort(currentNode.Id, parameter);
        return nextNode;
    }
    private string ExecuteStopNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    {
        //too
        if(currentNode.Data.stoptype.code=="FINISH")
        {
            /// _HeadId
            /// 
            _workFlowHead.workFlowStatus = WorkflowStatus.Completed;

  
       
            //if (_workFlowHead.WorkFlowDefinationId == new Guid("521d0cf8-c5a4-42ff-a031-aa61ab319e4a"))
            //{
            //    var positionRunner = new PositionCreateRunner();
            //    positionRunner.RunAsync(_HeadId.ToString());
            //}
            //else if (_workFlowHead.WorkFlowDefinationId == new Guid("b08b506c-1d98-4feb-a680-64ddc67b3c39"))
            //{
            //    var normRunner = new NormCreateRunner();
            //    normRunner.RunAsync(_HeadId.ToString());
            //}
        }
        else
        {

    
        }

        return "";
    }

    //private string ExecuteMailNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    //{
    //    //too

    //    //List<WorkflowLink> outgoingLinks = Links.FindAll(link => link.FromNode == currentNode.Id);


    //    //EmailNode emailNode = new EmailNode(currentNode.Configuration, workFlowItem);




    //    //workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;

    //    //_workFlowItems.Add(workFlowItem);

    //    //string nextNodeId = "";
    //    //// Her çıkış bağlantısını takip edin
    //    //foreach (var link in outgoingLinks)
    //    //{
    //    //    nextNodeId = link.ToNode;

    //    //}

    //    //return nextNodeId;
    //}

    private string ExecuteFormNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    {
        // FormNode işleme mantığı:
        // Start'ta action ile başlatıldıysa → action'a göre edge bul ve devam et
        // Action yoksa → pending olarak işaretle ve dur
        
        // Action kontrolü - Start'ta gönderilen action'ı kullan
        string actionToUse = _Action;
        
        // Eğer action boşsa ve parameter varsa (Continue'dan geliyorsa), parameter'ı action olarak kullan
        if (string.IsNullOrEmpty(actionToUse) && !string.IsNullOrEmpty(parameter))
        {
            actionToUse = parameter;
        }
        
        if (string.IsNullOrEmpty(actionToUse))
        {
            // Action belirtilmemişse, formNode'u pending olarak işaretle ve durdur
            // Kullanıcı formu doldurup butona basana kadar bekler
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Pending;
            _workFlowItems.Add(workFlowItem);
            return null; // Form doldurulana kadar durdur
        }
        
        // Action belirtilmişse (Start'ta gönderilen action), o action'a göre edge bul ve devam et
        workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
        _workFlowItems.Add(workFlowItem);
        
        // Action'a göre doğru edge'i bul
        var nextNode = FindLinkForPort(currentNode.Id, actionToUse);
        
        // Action kullanıldıktan sonra temizle (sadece Start'tan gelen action için)
        // NOT: Action sadece Start'ta gönderilen action için kullanılır
        // Sonraki formNode'lar için action Continue ile gönderilir
        if (actionToUse == _Action)
        {
            _Action = "";
        }
        
        // Eğer action'a göre edge bulunamadıysa, formNode'dan çıkan ilk edge'i kullan
        if (string.IsNullOrEmpty(nextNode))
        {
            List<Edges> outgoingLinks = Edges.FindAll(link => link.Source == currentNode.Id);
            if (outgoingLinks.Count > 0)
            {
                nextNode = outgoingLinks[0].Target;
            }
        }
        
        return nextNode;
    }

    private async Task<string> ExecuteFormTaskNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    {
        // ============================================================
        // FormTaskNode işleme mantığı (İKİ DURUM - SADE VE TEMİZ):
        // ============================================================
        // 1. START (parameter == ""):
        //    - YENİ FormItem oluştur (HER ZAMAN Pending!)
        //    - return "" → Workflow DURDUR
        //
        // 2. CONTINUE (parameter != ""):
        //    - WorkflowItem'ı Completed yap
        //    - return nextNode → Workflow DEVAM ET
        //    - NOT: FormItem güncellemesi NodeCompletionHandler'da yapılır!
        // ============================================================
        
        // DURUM 1: START - FormItem Oluştur (HER ZAMAN Pending!)
        if (parameter == "")
        {
            
            _workFlowItems.Add(workFlowItem);
            
            Utils utils = new Utils();
            
            // FormTaskNode'un Data'sından form bilgilerini al
            // FormTaskNode'da FormDesign yok, sadece formId var
            // FormDesign Form tablosundan alınacak (Start metodunda async olarak)
            string formDesign = ""; // FormTaskNode'da FormDesign yok, Form tablosundan alınacak
            string formId = ""; // Form ID (Guid string olarak)
            string formDescription = currentNode.Data?.Name ?? ""; // Form açıklaması
            
            // FormTaskNode'un Data'sından formId'yi al
            // Önce direkt property'den kontrol et, yoksa JSON'dan parse et
            if (currentNode.Data != null)
            {
                // Önce direkt property'den formId'yi al
                formId = currentNode.Data.formId 
                    ?? currentNode.Data.code 
                    ?? "";
                
                // Eğer hala boşsa, JSON'dan parse et (backward compatibility için)
                if (string.IsNullOrEmpty(formId))
                {
                    try
                    {
                        var nodeDataJson = JsonConvert.SerializeObject(currentNode.Data);
                        var nodeDataObj = JObject.Parse(nodeDataJson);
                        
                        // formId'yi al: formId (FormTaskNode'da bu şekilde tutuluyor) veya code (eski format)
                        formId = nodeDataObj["formId"]?.ToString() 
                            ?? nodeDataObj["FormId"]?.ToString() 
                            ?? nodeDataObj["code"]?.ToString() 
                            ?? "";
                    }
                    catch
                    {
                        // Parse hatası durumunda boş string kalır
                    }
                }
                
                // NOT: FormTaskNode'da FormDesign yok, sadece formId var
                // FormDesign Form tablosundan alınacak (Start metodunda async olarak)
            }
            
            // FormTaskNode'un Data'sından mesajı al (component'te gösterilecek)
            string formTaskMessage = "";
            if (currentNode.Data != null)
            {
                // NodeData'dan mesaj alanını al (message, formTaskMessage veya description)
                // Önce direkt property'den kontrol et, yoksa JSON'dan parse et
                formTaskMessage = currentNode.Data.message 
                    ?? currentNode.Data.formTaskMessage 
                    ?? currentNode.Data.description 
                    ?? "";
                
                // Eğer hala boşsa ve JSON'dan parse edilmişse, JObject'ten al
                if (string.IsNullOrEmpty(formTaskMessage))
                {
                    try
                    {
                        var nodeDataJson = JsonConvert.SerializeObject(currentNode.Data);
                        var nodeDataObj = JObject.Parse(nodeDataJson);
                        formTaskMessage = nodeDataObj["message"]?.ToString() 
                            ?? nodeDataObj["formTaskMessage"]?.ToString() 
                            ?? nodeDataObj["description"]?.ToString() 
                            ?? "";
                    }
                    catch
                    {
                        // Parse hatası durumunda boş string kalır
                    }
                }
            }
            
            // assignmentType'a göre kullanıcıları belirle
            List<string> userIds = new List<string>();
            
            // NodeData'dan assignmentType'ı al (JSON'dan parse et gerekirse)
            string assignmentType = currentNode.Data?.assignmentType;
            if (string.IsNullOrEmpty(assignmentType))
            {
                try
                {
                    var nodeDataJson = JsonConvert.SerializeObject(currentNode.Data);
                    var nodeDataObj = JObject.Parse(nodeDataJson);
                    assignmentType = nodeDataObj["assignmentType"]?.ToString();
                }
                catch
                {
                    // Parse hatası durumunda boş kalır
                }
            }
            
            // assignmentType'a göre kullanıcıları belirle
            if (!string.IsNullOrEmpty(assignmentType) && 
                _parameters?.UserManager != null && 
                _parameters?.EmployeeAssignmentService != null)
            {
                try
                {
                    // ✅ THREADING FIX: Cache'lenmiş assignments'i kullan
                    // Fallback kaldırıldı - PreloadAssignmentsAsync() veya Continue içinde cache doldurulmalı
                    List<formneo.core.Models.EmployeeAssignment> allAssignments = _cachedAssignments;
                    
                    // Eğer cache yoksa, boş liste kullan (assignmentType işlemlerini atla)
                    if (allAssignments == null)
                    {
                        allAssignments = new List<formneo.core.Models.EmployeeAssignment>();
                    }
                    
                    switch (assignmentType.ToLower())
                    {
                        case "direct_manager":
                            // Kullanıcının direkt manager'ı
                            if (!string.IsNullOrEmpty(_ApiSendUser))
                            {
                                // ✅ LINQ to Objects (memory'de, DbContext yok)
                                var now = DateTime.UtcNow;
                                var activeAssignment = allAssignments
                                    .Where(ea => ea.UserId == _ApiSendUser
                                        && ea.AssignmentType == formneo.core.Models.AssignmentType.Primary
                                        && ea.StartDate <= now
                                        && (ea.EndDate == null || ea.EndDate > now))
                                    .OrderByDescending(ea => ea.StartDate)
                                    .FirstOrDefault();
                                
                                if (activeAssignment?.ManagerId != null)
                                {
                                    userIds.Add(activeAssignment.ManagerId);
                                }
                            }
                            break;
                            
                        case "department_manager":
                            // Seçilen departmanın manager'ı
                            string selectedOrgUnitId = currentNode.Data?.selectedOrgUnit;
                            if (string.IsNullOrEmpty(selectedOrgUnitId))
                            {
                                try
                                {
                                    var nodeDataJson = JsonConvert.SerializeObject(currentNode.Data);
                                    var nodeDataObj = JObject.Parse(nodeDataJson);
                                    selectedOrgUnitId = nodeDataObj["selectedOrgUnit"]?.ToString();
                                }
                                catch { }
                            }
                            
                            if (!string.IsNullOrEmpty(selectedOrgUnitId) && Guid.TryParse(selectedOrgUnitId, out Guid orgUnitGuid))
                            {
                                // ✅ LINQ to Objects (memory'de, DbContext yok)
                                var orgUnitAssignments = allAssignments
                                    .FirstOrDefault(ea => ea.OrgUnitId == orgUnitGuid);
                                
                                if (orgUnitAssignments?.OrgUnit?.ManagerId != null)
                                {
                                    userIds.Add(orgUnitAssignments.OrgUnit.ManagerId);
                                }
                            }
                            break;
                            
                        case "department_all":
                            // Seçilen departmandaki tüm aktif kullanıcılar
                            string selectedOrgUnitIdAll = currentNode.Data?.selectedOrgUnit;
                            if (string.IsNullOrEmpty(selectedOrgUnitIdAll))
                            {
                                try
                                {
                                    var nodeDataJson = JsonConvert.SerializeObject(currentNode.Data);
                                    var nodeDataObj = JObject.Parse(nodeDataJson);
                                    selectedOrgUnitIdAll = nodeDataObj["selectedOrgUnit"]?.ToString();
                                }
                                catch { }
                            }
                            
                            if (!string.IsNullOrEmpty(selectedOrgUnitIdAll) && Guid.TryParse(selectedOrgUnitIdAll, out Guid orgUnitGuidAll))
                            {
                                // ✅ LINQ to Objects (memory'de, DbContext yok)
                                var now = DateTime.UtcNow;
                                var departmentUsers = allAssignments
                                    .Where(ea => ea.OrgUnitId == orgUnitGuidAll 
                                        && ea.StartDate <= now 
                                        && (ea.EndDate == null || ea.EndDate > now))
                                    .Select(ea => ea.UserId)
                                    .Distinct()
                                    .ToList();
                                
                                userIds.AddRange(departmentUsers);
                            }
                            break;
                            
                        case "position":
                            // Seçilen pozisyondaki tüm aktif kullanıcılar
                            string selectedPositionId = currentNode.Data?.selectedPosition;
                            if (string.IsNullOrEmpty(selectedPositionId))
                            {
                                try
                                {
                                    var nodeDataJson = JsonConvert.SerializeObject(currentNode.Data);
                                    var nodeDataObj = JObject.Parse(nodeDataJson);
                                    selectedPositionId = nodeDataObj["selectedPosition"]?.ToString();
                                }
                                catch { }
                            }
                            
                            if (!string.IsNullOrEmpty(selectedPositionId) && Guid.TryParse(selectedPositionId, out Guid positionGuid))
                            {
                                // ✅ LINQ to Objects (memory'de, DbContext yok)
                                var now = DateTime.UtcNow;
                                var positionUsers = allAssignments
                                    .Where(ea => ea.PositionId == positionGuid 
                                        && ea.StartDate <= now 
                                        && (ea.EndDate == null || ea.EndDate > now))
                                    .Select(ea => ea.UserId)
                                    .Distinct()
                                    .ToList();
                                
                                userIds.AddRange(positionUsers);
                            }
                            break;
                            
                        case "manual":
                            // selectionData.assignmentType'dan alınacak
                            object selectionData = currentNode.Data?.selectionData;
                            if (selectionData != null)
                            {
                                try
                                {
                                    var selectionDataJson = JsonConvert.SerializeObject(selectionData);
                                    var selectionDataObj = JObject.Parse(selectionDataJson);
                                    var manualAssignmentType = selectionDataObj["assignmentType"]?.ToString();
                                    
                                    // Manual assignmentType'a göre tekrar işle (recursive değil, sadece manual içindeki veriyi al)
                                    var manualUserIds = selectionDataObj["userIds"]?.ToObject<List<string>>();
                                    if (manualUserIds != null && manualUserIds.Count > 0)
                                    {
                                        userIds.AddRange(manualUserIds);
                                    }
                                }
                                catch { }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda log (production'da logger kullanılmalı)
                    // Şimdilik boş bırakıyoruz, userIds boş kalır
                }
            }
            
            // Eğer assignmentType yoksa veya kullanıcı bulunamadıysa, varsayılan olarak _ApiSendUser'ı kullan
            if (userIds.Count == 0)
            {
                userIds.Add(_ApiSendUser);
            }
            
            // FormItem'ları oluştur
            // Not: department_all ve position için birden fazla kullanıcı olabilir
            // Herkes için FormItem oluşturulur ama sadece birinin doldurması yeterli (ilk dolduran görevi alır)
            // FormInstance tek kalır (ilk dolduranın verisi kaydedilir)
            if (workFlowItem.formItems == null)
            {
                workFlowItem.formItems = new List<FormItems>();
            }
            
            // Her kullanıcı için FormItem oluştur
            foreach (var userId in userIds.Distinct())
            {
                string formUserNameSurname = utils.GetNameAndSurnameAsync(userId).ToString();
                
                var formItem = new FormItems
                {
                    WorkflowItemId = workFlowItem.Id,
                    FormDesign = formDesign,
                    FormId = !string.IsNullOrEmpty(formId) && Guid.TryParse(formId, out Guid parsedFormId) ? parsedFormId : null,
                    FormUserId = userId,
                    FormUserNameSurname = formUserNameSurname,
                    FormDescription = formDescription,
                    FormTaskMessage = formTaskMessage,
                    // ✅ SADE VE TEMİZ: START'ta HER ZAMAN Pending!
                    FormItemStatus = FormItemStatus.Pending,
                    FormData = null  // START'ta FormData yok, Continue'da doldurulacak
                };
                
                workFlowItem.formItems.Add(formItem);
            }
            
            // WorkflowItem status: Pending
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Pending;
            
            // Workflow'u DURDUR (kullanıcı formu dolduracak)
            return "";
        }
        
        // DURUM 2: CONTINUE - Workflow Devam Et
        if (!string.IsNullOrEmpty(parameter))
        {
            // ✅ SADE VE TEMİZ: Sadece WorkflowItem'ı Completed yap
            // FormItem güncellemesi NodeCompletionHandler'da yapılacak!
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
            
            // Sonraki node'a geç
            return FindLinkForPort(currentNode.Id, parameter);
        }
        
        // Buraya gelmemeli (yukarıdaki if'ler return yapıyor)
        return "";
    }

    private string ExecuteAlertNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    {
        // AlertNode işleme mantığı:
        // AlertNode sadece error/warning için kullanılır, success mesajları normal component'te gösterilir
        // 1. Start'ta alertNode'a gelince → tip kontrolü yap
        //    - error/warning → pending olarak işaretle ve dur (rollback yapılacak)
        //    - success/info → atla, sonraki node'a geç (rollback yok)
        // 2. Continue'da alertNode'dan devam edince → completed olarak işaretle ve sonraki node'a geç
        
        // AlertNode'un tipini kontrol et
        var alertType = currentNode.Data?.Text?.ToLower() ?? "info"; // Geçici olarak Text'ten al, sonra data'dan alınacak
        
        // Eğer workflowItem zaten pending ise (Continue'dan geliyorsa), alertNode'u completed yap ve devam et
        if (workFlowItem.workFlowNodeStatus == WorkflowStatus.Pending)
        {
            // Continue durumu: AlertNode'u completed olarak işaretle
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
            
            // Completed alertNode'u listeye ekle (güncelleme için)
            if (!_workFlowItems.Contains(workFlowItem))
            {
                _workFlowItems.Add(workFlowItem);
            }
            
            // AlertNode'dan sonraki node'u bul
            List<Edges> outgoingLinks = Edges.FindAll(link => link.Source == currentNode.Id);
            
            if (outgoingLinks.Count > 0)
            {
                // Sonraki node'a geç
                return outgoingLinks[0].Target;
            }
            
            return null;
        }
        
        // Start durumu: AlertNode tipine göre işlem yap
        // Success tipindeki alert'leri atla (normal component'te gösterilecek)
        // Error/Warning tipindeki alert'leri pending yap (rollback yapılacak)
        
        // AlertNode'un tipini workflow definition'dan al
        // NOT: Bu bilgiyi currentNode.Data'dan veya workflow definition'dan almak gerekiyor
        // Şimdilik tüm alert'leri pending yap, tip kontrolü WorkFlowExecute'da yapılacak
        
        // Start durumu: AlertNode'u pending olarak işaretle ve dur
        workFlowItem.workFlowNodeStatus = WorkflowStatus.Pending;
        _workFlowItems.Add(workFlowItem);
        
        // AlertNode'dan sonraki node'u bul (Continue için)
        List<Edges> outgoingLinksForContinue = Edges.FindAll(link => link.Source == currentNode.Id);
        
        if (outgoingLinksForContinue.Count > 0)
        {
            // Sonraki node ID'sini döndür (Continue'da kullanılacak)
            // NOT: Bu node execute edilmeyecek, sadece Continue'da kullanılacak
            return outgoingLinksForContinue[0].Target;
        }
        
        // Edge yoksa null döndür (workflow durur)
        return null;
    }

    private string ExecuteScriptNode(WorkflowNode currentNode, WorkflowItem workFlowItem, string parameter)
    {
        // ScriptNode işleme mantığı:
        // 1. Script'i çalıştır (JavaScript)
        // 2. Script'e previousNodes ve workflow bilgilerini ver
        // 3. Script true/false döndürür
        // 4. True ise "yes" edge'ine, false ise "no" edge'ine git
        
        if (string.IsNullOrEmpty(currentNode.Data?.script))
        {
            // Script yoksa, scriptNode'u completed olarak işaretle ve sonraki node'a geç
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
            _workFlowItems.Add(workFlowItem);
            
            List<Edges> outgoingLinks = Edges.FindAll(link => link.Source == currentNode.Id);
            if (outgoingLinks.Count > 0)
            {
                return outgoingLinks[0].Target;
            }
            return null;
        }
        
        try
        {
            // PreviousNodes yapısını oluştur (form verilerine erişim için)
            var previousNodes = BuildPreviousNodes();
            
            // Workflow bilgilerini oluştur
            var workflowInfo = new
            {
                instanceId = _HeadId.ToString(),
                startTime = DateTime.Now,
                currentStep = currentNode.Id,
                formId = "",
                formName = ""
            };
            
            // JavaScript engine oluştur ve güvenlik ayarları
            var engine = new Engine(options => 
            {
                // Timeout ekle (sonsuz döngü koruması)
                options.TimeoutInterval(TimeSpan.FromSeconds(30));
                // Memory limiti (DoS koruması)
                options.LimitMemory(4_000_000); // 4MB
                // Recursion limiti
                options.LimitRecursion(100);
            });
            
            // Context'i JavaScript'e aktar
            // Jint, Dictionary<string, object> tipini otomatik olarak JavaScript object'e çevirir
            // previousNodes.PERSONELTALEP.uuk80m63ix3 şeklinde erişim için nested dictionary'leri doğru aktar
            engine.SetValue("previousNodes", previousNodes);
            
            // workflow bilgilerini aktar
            var workflowDict = new Dictionary<string, object>
            {
                ["instanceId"] = workflowInfo.instanceId,
                ["startTime"] = workflowInfo.startTime,
                ["currentStep"] = workflowInfo.currentStep,
                ["formId"] = workflowInfo.formId,
                ["formName"] = workflowInfo.formName
            };
            engine.SetValue("workflow", workflowDict);
            
            // formdata değişkenini ekle (script içinde formdata.fieldName şeklinde erişim için)
            // BEST PRACTICE: ScriptNode'dan hemen önceki formNode'un verisini al
            // Eğer previousNodes varsa, son eklenen (en güncel) form verisini kullan
            if (previousNodes.Count > 0)
            {
                // Son formNode verisini al (en güncel form verisi)
                var lastFormData = previousNodes.Values.Last();
                engine.SetValue("formData", lastFormData);
                engine.SetValue("formdata", lastFormData);
            }
            else
            {
                // Eğer previousNodes boşsa, payload'dan al (ilk workflow çalıştırması)
                var payloadFormData = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(_payloadJson))
                {
                    try
                    {
                        var payload = JObject.Parse(_payloadJson);
                        foreach (var prop in payload.Properties())
                        {
                            payloadFormData[prop.Name] = prop.Value.ToObject<object>();
                        }
                    }
                    catch
                    {
                        // Parse hatası durumunda boş object
                    }
                }
                engine.SetValue("formdata", payloadFormData);
            }
            
            // Script'i çalıştır
            var scriptResult = engine.Evaluate(currentNode.Data.script);
            
            // Sonucu boolean'a çevir
            bool result = false;
            if (scriptResult != null)
            {
                if (scriptResult.IsBoolean())
                {
                    result = scriptResult.AsBoolean();
                }
                else if (scriptResult.IsString())
                {
                    result = scriptResult.AsString().ToLower() == "true";
                }
                else if (scriptResult.IsNumber())
                {
                    result = scriptResult.AsNumber() != 0;
                }
            }
            
            // ScriptNode'u completed olarak işaretle
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
            _workFlowItems.Add(workFlowItem);
            
            // Sonuca göre edge bul
            string port = result ? "yes" : "no";
            var nextNode = FindLinkForPort(currentNode.Id, port);
            
            // Eğer port'a göre edge bulunamadıysa
            if (string.IsNullOrEmpty(nextNode))
            {
                // Alternatif port isimlerini dene (case-insensitive ve alternatif isimler)
                var alternativePorts = result 
                    ? new[] { "yes", "YES", "Yes", "true", "TRUE", "True" }
                    : new[] { "no", "NO", "No", "false", "FALSE", "False" };
                
                foreach (var altPort in alternativePorts)
                {
                    nextNode = FindLinkForPort(currentNode.Id, altPort);
                    if (!string.IsNullOrEmpty(nextNode))
                    {
                        break;
                    }
                }
                
                // Hala bulunamadıysa ve "yes" ise, ilk edge'i kullan (backward compatibility)
                // "no" için varsayılan edge kullanma - workflow durmalı
                if (string.IsNullOrEmpty(nextNode) && result)
                {
                    List<Edges> outgoingLinks = Edges.FindAll(link => link.Source == currentNode.Id);
                    if (outgoingLinks.Count > 0)
                    {
                        nextNode = outgoingLinks[0].Target;
                    }
                }
                // "no" için edge bulunamazsa null döndür (workflow durur veya hata)
            }
            
            return nextNode;
        }
        catch (Exception ex)
        {
            // BEST PRACTICE: Exception'ı logla (debugging için kritik)
            Console.WriteLine($"[ScriptNode Error] NodeId: {currentNode.Id}, Error: {ex.Message}");
            Console.WriteLine($"[ScriptNode Error] Script: {currentNode.Data?.script}");
            Console.WriteLine($"[ScriptNode Error] StackTrace: {ex.StackTrace}");
            
            // Script hatası durumunda, scriptNode'u Completed olarak işaretle (hata detayı NodeDescription'da)
            workFlowItem.workFlowNodeStatus = WorkflowStatus.Completed;
            workFlowItem.NodeDescription = $"Script execution error: {ex.Message}";
            _workFlowItems.Add(workFlowItem);
            
            // BEST PRACTICE: Hata durumunda "error" port'una git (varsa)
            var errorNode = FindLinkForPort(currentNode.Id, "error");
            if (!string.IsNullOrEmpty(errorNode))
            {
                return errorNode;
            }
            
            // Error port yoksa, "no" port'una git
            var noNode = FindLinkForPort(currentNode.Id, "no");
            if (!string.IsNullOrEmpty(noNode))
            {
                return noNode;
            }
            
            // Hiçbir port bulunamazsa, ilk edge'i kullan (backward compatibility)
            List<Edges> outgoingLinks = Edges.FindAll(link => link.Source == currentNode.Id);
            if (outgoingLinks.Count > 0)
            {
                return outgoingLinks[0].Target;
            }
            
            // Hiçbir edge yoksa workflow durur
            return null;
        }
    }
    
    /// <summary>
    /// PreviousNodes yapısını oluşturur (form verilerine erişim için)
    /// Script içinde previousNodes.PERSONELTALEP.uuk80m63ix3 şeklinde erişim sağlar
    /// </summary>
    private Dictionary<string, object> BuildPreviousNodes()
    {
        var previousNodes = new Dictionary<string, object>();
        
        // PayloadJson'dan form verilerini parse et
        JObject payload = null;
        if (!string.IsNullOrEmpty(_payloadJson))
        {
            try
            {
                payload = JObject.Parse(_payloadJson);
            }
            catch
            {
                // Parse hatası durumunda boş payload
            }
        }
        
        // Workflow items'tan formNode'ları bul (completed olanlar)
        var completedFormNodes = _workFlowItems
            .Where(item => item.NodeType == "formNode" && item.workFlowNodeStatus == WorkflowStatus.Completed)
            .ToList();
        
        foreach (var formNodeItem in completedFormNodes)
        {
            // FormNode'un adını bul
            var formNode = Nodes.FirstOrDefault(n => n.Id == formNodeItem.NodeId);
            if (formNode == null) continue;
            
            string formNodeName = formNode.Data?.Name ?? formNodeItem.NodeName;
            
            // Form verilerini oluştur
            var formNodeData = new Dictionary<string, object>();
            
            // Payload'dan form field'larını al
            if (payload != null)
            {
                foreach (var prop in payload.Properties())
                {
                    // JToken'ı uygun tipe çevir
                    var value = prop.Value;
                    if (value.Type == JTokenType.String)
                    {
                        formNodeData[prop.Name] = value.ToString();
                    }
                    else if (value.Type == JTokenType.Integer)
                    {
                        formNodeData[prop.Name] = value.ToObject<int>();
                    }
                    else if (value.Type == JTokenType.Float)
                    {
                        formNodeData[prop.Name] = value.ToObject<double>();
                    }
                    else if (value.Type == JTokenType.Boolean)
                    {
                        formNodeData[prop.Name] = value.ToObject<bool>();
                    }
                    else if (value.Type == JTokenType.Null)
                    {
                        formNodeData[prop.Name] = null;
                    }
                    else
                    {
                        formNodeData[prop.Name] = value.ToString();
                    }
                }
            }
            
            // PreviousNodes'a ekle
            // previousNodes.PERSONELTALEP.uuk80m63ix3 şeklinde erişim için
            previousNodes[formNodeName] = formNodeData;
        }
        
        // Eğer hiç formNode yoksa ama payload varsa, payload'ı direkt ekle
        if (previousNodes.Count == 0 && payload != null)
        {
            var defaultFormData = new Dictionary<string, object>();
            foreach (var prop in payload.Properties())
            {
                var value = prop.Value;
                if (value.Type == JTokenType.String)
                {
                    defaultFormData[prop.Name] = value.ToString();
                }
                else if (value.Type == JTokenType.Integer)
                {
                    defaultFormData[prop.Name] = value.ToObject<int>();
                }
                else if (value.Type == JTokenType.Float)
                {
                    defaultFormData[prop.Name] = value.ToObject<double>();
                }
                else if (value.Type == JTokenType.Boolean)
                {
                    defaultFormData[prop.Name] = value.ToObject<bool>();
                }
                else if (value.Type == JTokenType.Null)
                {
                    defaultFormData[prop.Name] = null;
                }
                else
                {
                    defaultFormData[prop.Name] = value.ToString();
                }
            }
            
            // Varsayılan form adı kullan (eğer processDataTree'de form adı varsa onu kullan)
            string defaultFormName = "FORM";
            if (Nodes.Any(n => n.Type == "formNode"))
            {
                var firstFormNode = Nodes.FirstOrDefault(n => n.Type == "formNode");
                if (firstFormNode != null && !string.IsNullOrEmpty(firstFormNode.Data?.Name))
                {
                    defaultFormName = firstFormNode.Data.Name;
                }
            }
            
            previousNodes[defaultFormName] = defaultFormData;
        }
        
        return previousNodes;
    }

    private string FindLinkForPort(string fromNode, string port)
    {
        if (string.IsNullOrEmpty(port))
        {
            return null;
        }
        
        // Case-insensitive arama yap
        Edges matchingLink = Edges.Find(link => 
            link.Source == fromNode && 
            !string.IsNullOrEmpty(link.SourceHandle) &&
            link.SourceHandle.Equals(port, StringComparison.OrdinalIgnoreCase) && 
            Nodes.Any(node => node.Id == link.Target));

        if (matchingLink != null)
        {
            return matchingLink.Target;
        }

        return null; // If no link with the specified port is found
    }




}
