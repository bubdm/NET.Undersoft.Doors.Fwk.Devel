
class TransactionHeader {

    static Type() { return "System.Doors.Data.Depot.TransactionHeader"; }
    GetType() { return "System.Doors.Data.Depot.TransactionHeader"; }    

    constructor(transaction) {     
        this.transaction = transaction;
        this.content = null;

        this.Context = new TransactionContext();
        this.Context.Identity = this.transaction.Identity;
    }

    get Content() {
        return this.content;
    }
    set Content(data) {
        this.Context
            .ContentType = data.GetType();
        this.content =     data.GetHeader();              
        this.transaction
            .MyMessage
            .Content =     data;       
    }

    toJSON() {
        return {
            Content: this.Content,
            Context: this.Context
        }
    }        
}