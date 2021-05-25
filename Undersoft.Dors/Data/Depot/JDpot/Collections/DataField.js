class DataField {

    static Type() { return "System.Dors.Data.DataField"; }
    GetType() { return "System.Dors.Data.DataField"; }

    constructor(data, fieldid, value, isproperty = false, istier = true) {

        this._data = data;
        this._bind = null;      

        this.value = value;
        this.fieldId = fieldid;   
        this.isProperty = isproperty;
        this.isTier = istier;
        this.valueKey = !this.isProperty ? 'value' : fieldid;
        this.valueObj = !this.isProperty ? this : this._data;        

        this.Definitions();
    }

    get FieldId() {
        return this.fieldId
    }
    set FieldId(fieldId) {
        this.fieldId = fieldId;
    }

    get Value() {

        if (this._bind != null)
            if (this.valueObj[this.valueKey] !== this._bind.Value)
                this.valueObj[this.valueKey] = this._bind.Value;

        return this.valueObj[this.valueKey];
    }
    set Value(value) {
        if (this.valueObj[this.valueKey] !== value) {
            this.valueObj[this.valueKey] = value;
            if (!this._data._syncing) {
                if(this.isTier)
                    this._data._tiers.ToBag(this._data);
            }
        }
        if (this._bind != null)
            if (this._bind.Value !== value)
                this._bind.Value = value;
    }

    Revalue() {
        if (this._bind != null)
            if (this.valueObj[this.valueKey] !== this._bind.Value)
                this._bind.Value = this.valueObj[this.valueKey];
    }

    get PylonName() {
        if (!this.isProperty) {
            let pyl = this._data.GetPylon(this.FieldId);
            return pyl != null ?
                pyl.PylonName : "";
        }
        return this.fieldId;
    }

    get DisplayName() {
        if (!this.isProperty) {
        let pyl = this._data.GetPylon(this.FieldId);
        return pyl != null ? pyl.DisplayName : "";
        }
        return this.fieldId;
    }

    async Impact(field) {
        this.FieldId = field.PylonId;
        this.Value = field.Value;
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element);
    }

    Definitions() {
        Object.defineProperty(DataField.prototype, 'Value', { enumerable: true });
        Object.defineProperty(DataField.prototype, 'FieldId', { enumerable: true });
        Object.defineProperty(DataField.prototype, 'PylonName', { enumerable: true });
        Object.defineProperty(DataField.prototype, 'DisplayName', { enumerable: true });     
    }

    toJSON() {
        return {
            Value: this.Value
        }
    }
}