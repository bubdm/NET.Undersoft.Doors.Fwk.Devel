var HtmlClient = null;

class HttpConnection {   

    constructor(url = "", callback = null, identity = null,) {           
        this.Identity = identity === null ? new DepotIdentity() : identity;
        this.Url = url;
        this.Content = null;
        this.Callback = callback;
        this.Completed = false;
    }

    HttpHeaders() {        
        let headers = {
            DepotToken: this.Identity.Token,
            DepotUserId: this.Identity.UserId,
            DepotDeptId: this.Identity.DeptId,
        }       
        return headers;
    }

    Uncomplete() { this.Completed = false; }
    Complete()   { this.Completed = true; }

    Response(content) { this.Content = content; }

   async Initiate() {       
        let bag = this;
        bag.Uncomplete();
        $.ajax({
            type: "GET",
            url: this.Identity.Host + "/" + this.Url,
            headers: this.HttpHeaders(),            
            async: true,
            contentType: "text/html; charset=utf-8",
            dataType: "html",
            success: function successAjaxCall(response) {
                bag.Response(response);
                if (bag.Callback !== null) { bag.Callback.Execute(bag); }
                bag.Complete();
            }
        });
    }
}