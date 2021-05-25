
var DepotUserId = "";
var DepotDeptId = "";
var DepotToken =  "";

class DepotIdentity {

    constructor(name = "", key = "") {
        this.Host = DepotHost;
        this.Name = name;
        this.Key = key;
        this.Token =  DepotToken;
        this.DeptId = DepotDeptId;
        this.UserId = DepotUserId;
        this.DataPlace = "";
        this.RegisterTime = "";
        this.LastAction = "";
        this.LifeTime = "";
        this.Site = "Client"
    }

    SetToken(token = "") {
        if (token != "") {
            this.Token = token;
            DepotToken = token;
        }
    }

    SetIds(auth = "", dept = "") {
        if (auth != "") {
            this.UserId = auth;
            DepotUserId = auth;
        }
        if (dept != "") {
            this.DeptId = dept;
            DepotDeptId = dept;
        }
    }
}