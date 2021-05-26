class DataPylon {

    static Type() { return "System.Doors.Data.DataPylon"; }
    GetType() { return "System.Doors.Data.DataPylon"; }

    constructor(trellis) {
        this._trell = trellis;
        this._fields = new DataFields(this);
        this._bind = null;
        this._syncing = false;

        this.PylonName = "Pylon";
        this.DisplayName = "Pylon";
        this.PylonId = -1;
        this.Ordinal = -1;
        this.DataType = "System.String";
        this.Default = null;
        this.Visible = true;
        this.Editable = false;
        this.isKey = false;
        this.TotalOperand = "";
        this.Revalue = 0;
        this.RevalType =    "Percent";
        this.RevalOperand = "Substract";
    }



    async  Impact(pylon) {
        if (pylon != null) {
            let keys = Object.keys(pylon);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = pylon[key];
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

    Definitions() {
        this._fields.Put("PylonName", this.PylonName, true, false);     
        this._fields.Put("DisplayName", this.DisplayName, true, false);     
        this._fields.Put("Visible", this.Visible, true, false);     
        this._fields.Put("TotalOperand", this.TotalOperand, true, false);          
        this._fields.Put("Revalue", this.Revalue, true, false);          
        this._fields.Put("RevalType", this.RevalType, true, false);          
        this._fields.Put("RevalOperand", this.RevalOperand, true, false);                 
    }

    toJSON() {
        switch (Complexity) {
            case "Basic":
                return {
                    PylonId: this.PylonId,
                    PylonName: this.PylonName,
                    DisplayName: this.DisplayName,
                    Ordinal: this.Ordinal
                }
            case "Standard":
                return {
                    PylonId: this.PylonId,
                    PylonName: this.PylonName,
                    DisplayName: this.DisplayName,
                    Ordinal: this.Ordinal,
                    Revalue: this.Revalue,
                    RevalType: this.RevalType,
                    RevalOperand: this.RevalOperand
                }
            default:
                return {
                    PylonId: this.PylonId,
                    PylonName: this.PylonName,
                    DisplayName: this.DisplayName,
                    Ordinal: this.Ordinal,
                    Revalue: this.Revalue,
                    RevalType: this.RevalType,
                    RevalOperand: this.RevalOperand
                }
        }
    }
}