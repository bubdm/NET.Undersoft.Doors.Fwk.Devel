var DepotClient = null;
var DepotHost = "http://127.0.0.1:44004";

class DepotConnection {   

    constructor(callback = null, identity = null) {        
        this.Transaction   = new DepotTransaction(identity);    
        let context        = this.RequestContext();
        this.Identity      = context.Identity;
        this.Callback      = callback;
        this.Completed     = false;
    }

    get Content() { return this.Transaction.MyHeader.Content; }
    set Content(content) { this.Transaction.MyHeader.Content = content; }

    get Complexity() { return this.Transaction.MyHeader.Context.Complexity; }
    set Complexity(complex) { this.Transaction.MyHeader.Context.Complexity = complex; }

    HttpHeaders() {
        let encode64 = 
        this.Identity.Name !== "" &&
            this.Identity.Key !== "" ?
            "Basic " + window.btoa(unescape(encodeURIComponent(this.Identity.Name + ':' + this.Identity.Key))) :
                "";
        let headers = {
            DepotToken:  this.Identity.Token,
            DepotUserId: this.Identity.UserId,
            DepotDeptId: this.Identity.DeptId,
        }        
        if (encode64 !== "") {
            headers.Authorization = encode64;
            this.Identity.Name = window.btoa(unescape(encodeURIComponent(this.Identity.Name)));
            this.Identity.Key  = window.btoa(unescape(encodeURIComponent(this.Identity.Key)));
        }
        return headers;
    }

    Uncomplete() { this.Completed = false; }
    Complete()   { this.Completed = true; }
    ClearKeys()  { this.Identity.Name = "";
                   this.Identity.Key  = ""; }

    RequestContext()  { return this.Transaction.MyHeader.Context; }
    RequestHeader()   { return this.Transaction.MyHeader; }
    RequestMessage()  { return this.Transaction.MyMessage; }

    ResponseContext() { return this.Transaction.HeaderReceived.Context; }
    ResponseHeader()  { return this.Transaction.HeaderReceived; }
    ResponseMessage() { return this.Transaction.MessageReceived; }

   async Initiate() {       
        let bag = this;
        bag.Uncomplete();  
        $.ajax({
            type: "POST",
            url: this.Identity.Host,
            headers: this.HttpHeaders(),
            data: JSON.stringify(this.Transaction.Request()),
            async: true,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function successAjaxCall(response) {
                bag.ClearKeys();
                bag.Transaction.Response(response);    
                if (bag.Callback !== null) { bag.Callback.Execute(bag); }
                bag.Complete();
            }
        });
    }
}

var IgnoreKeys = [
    "Tiers",
    "Sims",
    "tiers",
    "sims"
]