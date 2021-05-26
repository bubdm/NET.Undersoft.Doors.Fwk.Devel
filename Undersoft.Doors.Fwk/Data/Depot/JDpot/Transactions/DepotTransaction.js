
class DepotTransaction {

    constructor(identity = null) {
        this.Identity = identity === null ? new DepotIdentity() : identity;
        this.MyHeader = new TransactionHeader(this);
        this.MyMessage = new TransactionMessage();
        this.HeaderReceived = null;
        this.MessageReceived = null;
    }

   Request() {
        return {
            DepotHeader: this.MyHeader,
            DepotMessage: this.MyMessage
        }
    }

   Response(response) {
        this.HeaderReceived = response.DepotHeader;
        this.MessageReceived = response.DepotMessage;

        let token =  this.HeaderReceived.Context.Identity.Token;
        let userId = this.HeaderReceived.Context.Identity.UserId;
        let deptId = this.HeaderReceived.Context.Identity.DeptId;

        if (token  !== null && token  !== "") DepotToken  = token;
        if (userId !== null && userId !== "") DepotUserId = userId;
        if (deptId !== null && deptId !== "") DepotDeptId = deptId;
    }
}