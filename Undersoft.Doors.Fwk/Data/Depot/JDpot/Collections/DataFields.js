class DataFields {

    static Type() { return "System.Doors.Data.DataField"; }
    GetType() { return "System.Doors.Data.DataField"; }

    constructor(data, fieldskey = "_fields", addproperty = true) {        
        this._data = data;
        this._bind = null;
        this._fieldsKey = fieldskey;
        let self = this;
        self = {}      
        if(addproperty)
           this.FieldsProperty(this._data, this._fieldsKey);       
    }

    Get(idorname, isproperty = false) {
        if (!isproperty) {
            let pyl = this._data.GetPylon(idorname);
            if (pyl != null) {
                return this[pyl.PylonId];
            }
            else {
                let test = this[idorname.toLowerCase()];
                if (typeof (test) != "undefined")
                    return this[idorname.toLowerCase()];
            }
        }
        else {
            return this[idorname.toLowerCase()];
        }
        return null;
    }
    Set(idorname, value, isproperty = false) {
        if (!isproperty) {
            let pyl = this._data.GetPylon(idorname);
            if (pyl != null) {
                this[pyl.PylonId].Value = value;
                return this[pyl.PylonId];
            }
        }
        else {
            this[idorname].Value = value;
            return this[idorname];
        }
        return null;
    }
           
    Put(fieldid, value, isproperty = false, istier = true, obj = null) {
        let check = this[fieldid];
        if (typeof (check) !== "undefined") {
            if (value != null)
                this[fieldid].Value = value;
            else if (obj != null)
                this[fieldid].Value = obj;
        }
        else
            this.Add(fieldid, value, isproperty, istier, obj);
    }
    Add(fieldid, value, isproperty = false, istier = true, obj = null) {
        if (obj == null)
            obj = this._data;
        if (isproperty && value !== null) {
            let priv = fieldid.toLowerCase();
            let pub = fieldid;
            delete obj[fieldid];
            fieldid = priv;
            obj[priv] = value;
            let key = this._fieldsKey;
            this[fieldid] = new DataField(obj, fieldid, value, isproperty, istier);
            Object.defineProperty(obj, pub,
                {
                    get() { return obj[key][fieldid].Value; },
                    set(value) { obj[key][fieldid].Value = value; },
                    configurable: true,
                    enumerable: true
                });
        }
        else {
            if (value != null)
                this[fieldid] = new DataField(obj, fieldid, value, isproperty, istier);   
            else if (obj != null)
                this[fieldid] = new DataField(obj, 'value', obj, isproperty, istier);                 
        }
    }

    Remove(field) {
        delete this[field.FieldId];
    }
    Delete(fieldid) {
        delete this[fieldid];
    }

    FieldsProperty(obj, key) {
        Object.defineProperty(obj, "Fields",
            {
                get() { return obj[key]; },
                set(value) {
                    for (var i in value) {
                        obj[key].Put(i, value[i]);
                    }
                }
            });
    }
}