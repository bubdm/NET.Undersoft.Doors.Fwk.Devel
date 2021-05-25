class SyncedCallback {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {
        let head = input.Transaction.HeaderReceived;
        let msg = input.Transaction.MessageReceived;
        let reg = Object.keys(head.Content);
        if (!Seek.HasTopKey(reg, "Register")) {
            let headContent = head.Content;
            let msgContent = msg.Content;
            console.log(msgContent);
            let headImpact = Space.Impact(headContent);
            let messageImpact = new DataBag(msgContent);
            console.log(messageImpact.Impact(true));
            console.log(headImpact);
            if (this.Method != null)
                this.Method.Execute();
        }
    }
}