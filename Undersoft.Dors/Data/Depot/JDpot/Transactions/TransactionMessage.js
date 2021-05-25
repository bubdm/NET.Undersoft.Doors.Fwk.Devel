
class TransactionMessage {

    constructor(content = null, notice = "") {
        this.content = content;
        this.Notice = notice;
    }

    get Content() { return this.content; }
    set Content(data) {
        this.content = data.GetMessage();
    }

    toJSON() {
        return {
            Content: this.Content,
            Notice: this.Notice
        }
    }   

}
