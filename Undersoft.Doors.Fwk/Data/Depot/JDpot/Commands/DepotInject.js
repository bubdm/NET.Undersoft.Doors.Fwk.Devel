class DepotInject {

    constructor(url, id, callback = null) {
        this.Url = url;
        this.Id = id;
        this.Input = null;
        this.Callback = callback;
    }

    async Initiate() {
        let hcn = new HttpConnection(this.Url, new InjectedCallback(this));
        hcn.Initiate();
    }
}