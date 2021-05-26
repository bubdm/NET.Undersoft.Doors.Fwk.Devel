class DataTrellises {

    static Type() { return "System.Doors.Data.DataTrellises"; }
    GetType() { return "System.Doors.Data.DataTrellises"; }

    constructor() {
        var self = this;
        self = {};
    }

    GetHeader() {
        return this;
    }
    GetMessage() {
        let databag = new DataBag(null);
        databag.DataBag = {}
        for (var w in this)
            databag.DataBag[w] = this[w]._bag.DataBag[w];
        return databag;
    }

    AddNew(trellName) {
        this[trellName] = new DataTrellis(trellName);
    }

    Add(trellis) {
        this[trellis.TrellName] = trellis;
    }
    AddRange(trells) {
        for (var i in trells)
            this.Add(trells[i]);
    }

    Remove(trellName) {
        delete this[trellName];
    }

    async Impact(trells) {
        if (trells != null) {
            for (var i in trells) {
                let trell = Seek.TopKey(this, i, true, IgnoreKeys);
                if (trell === null) {
                    let newtrell = new DataTrellis(trells[i].TrellName);
                    newtrell.Impact(trells[i]);
                    this[i] = newtrell;
                }
                else if (trell.length > 0)
                    trell[0].Impact(trells[i]);
                else
                    trell.Impact(trells[i]);            
            }
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }
}