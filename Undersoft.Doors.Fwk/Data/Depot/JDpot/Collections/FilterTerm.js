class FilterTerm {

    static GetType() { return "System.Doors.Data.FilterTerm"; }

    constructor(pylon = "", oper = "", val = null, logic = "And", stage = "First") {
        this._bind = null;
        this._syncing = false;
        this._fields = new DataFields(this);

        this.Index = -1;
        this.PylonName = pylon;
        this.Operand = oper;
        this.Value = val;
        this.Logic = logic;
        this.Stage = stage
    }

    async Impact(term) {
        if (term != null) {
            let keys = Object.keys(term);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = term[key];
            }
            this._syncing = false;
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }

    toJSON() {
        return {
            Index: this.Index,
            PylonName: this.PylonName,
            Operand: this.Operand,
            Value: this.Value,
            Logic: this.Logic,
            Stage: this.Stage
        }
    }
}