class DataJoins {

    static GetType() { return "System.Doors.Data.DataTiers"; }

    constructor(tier, type) {
        this._tier = tier;
        this._bind = null;
        this._fields = new DataFields(this);
        this._registry = {};
        this.Type = type;
    }

    Get(index) {
        return this._registry[index];
    }
    Set(index, value) {
        this._registry[index] = value;
        return this._registry[index];
    }

    Add(expands) {
        if (expands !== null && typeof (expands) !== "undefined") {
            for (var s in expands) {
                let j = new DataJoin(this, expands[s], s, this.Type);
                let n = j.JoinName;
                this._registry[j.JoinMember] = j;
                this._fields.Put(n, this._registry[j.JoinMember], true, false);
                this._tier.fields.Put(j.JoinName, j.JoinKey, true, false);
            }
        }
    }   

    Remove(expand) {
        delete this._registry[expand]        
    }

    async Impact(expands) {
        this.Add(expands);
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }
}