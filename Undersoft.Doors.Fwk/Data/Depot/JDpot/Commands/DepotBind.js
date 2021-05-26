class DepotBind {

    constructor(url, obj, callback = null, id = null) {
        this.Url = url;
        this.Object = obj;
        this.Input = null;
        this.Id = id
        this.Callback = callback;
    }

    async Initiate() {
        let hcn = new HttpConnection(this.Url, new BindedCallback(this));
        hcn.Initiate();
    }
}