class DataTier {

    static Type() { return "System.Doors.Data.DataTier"; }
    GetType() { return "System.Doors.Data.DataTier"; }

    constructor(tiers) {      
        this._tiers = tiers;
        this._subtiers = null;
        this._bind = null;
        this._syncing = false;

        this.fields = new DataFields(this, "fields", false);

        this.childjoins = new DataJoins(this, "Child");
        this.parentjoins = new DataJoins(this, "Parent");

        this.Checked = false;      
        this.Index = -1;
        this.Page = -1;
        this.PageIndex = -1;
        this.viewindex = 0;       
        this.Position = 0;
        this.BagIndex = -1;
        this.NoId = null;
        this.Edited = false;
        this.Deleted = false;
        this.Added = false;
        this.Synced = false;
        this.Saved = false;

        this.Definitions();
    }

    Get(idorname) {
       return this.fields.Get(idorname);
    }
    Set(idorname, value) {
       return this.fields.Set(idorname, value);
    }

    GetHeader() {
        return this._tiers._trell;
    }
    GetMessage() {
        let data = {}
        data[this._tiers._trell.TrellName] = { DataTiers: [this] }
        return new DataBag(data, false);
    } 

    get ViewIndex() {
        return this.viewindex;
    }
    set ViewIndex(vid) {
        this.viewindex = vid;
        this.Position = parseInt(vid) + 1;
    }

    get Fields() {
        return this.fields;
    }
    set Fields(fields) {
        for (var i in fields) {
            this.fields.Put(i, fields[i]);
        }
    }

    get ChildJoins() {
        return this.childjoins;
    }
    set ChildJoins(expands) {
        this.childjoins.Add(expands);
    }

    get ParentJoins() {      
        return this.parentjoins;
    }
    set ParentJoins(expands) {
        this.parentjoins.Add(expands);
    }

    GetPylon(idorname) {
        let pylon = null;
        if (isNaN(parseInt(idorname))) {
            let pylonid = this._tiers._trell.pylons.Registry[idorname];
            if (typeof (pylonid) !== "undefined")
                pylon = this._tiers._trell.pylons.List[pylonid];
        }
        else
            pylon = this._tiers._trell.pylons.List[idorname];        
        return pylon;
    }

    Impact(tier) {
        if (tier != null) {
            let keys = Object.keys(tier);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = tier[key];
            }
            this._syncing = false;
        }
        return this;
    }

    Reimpact(tier) {
        if (tier != null) {
            this.Checked = tier.Checked;
            this.Edited = tier.Edited;
            this.Synced = tier.Synced;
            this.Saved = tier.Saved;
        }
        return this;
    }

    Revalue() {
        for (var i in this.fields) {
            if (!i.includes('_')) {
                this.fields[i].Revalue();
            }
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }

    Check() {
        this.Checked = true;
        this._tiers.ToBag(this);
    }
    Uncheck() {
        this.Checked = false;
        this._tiers.OffBag(this);
    }

    Definitions() {

        this.fields.Put("Checked", this.Checked, true);
        this.fields.Put("Position", this.Position, true);

        Object.defineProperty(DataTier.prototype, 'ViewIndex', { enumerable: true });
        Object.defineProperty(DataTier.prototype, 'ChildJoins', { enumerable: true });
        Object.defineProperty(DataTier.prototype, 'ParentJoins', { enumerable: true });
        Object.defineProperty(DataTier.prototype, 'Bind', { enumerable: true });       
        Object.defineProperty(DataTier.prototype, 'Fields', { enumerable: true });       
    }

    get FieldsBag() {
        let fields = {}
        for (var i in this.Fields) {
            if (!i.includes('_')) {
                let field = this.Fields[i];
                if (!field.isProperty)
                    fields[i] = field.Value;
            }
        }
        return fields;
    }

    toJSON() {
        return {
            Checked: this.Checked,
            Index: this.Index,
            Page: this.Page,
            PageIndex: this.PageIndex,
            ViewIndex: this.ViewIndex,
            NoId: this.NoId,
            Edited: this.Edited,
            Added: this.Added,
            Synced: this.Synced,
            Saved: this.Saved,
            Fields: this.FieldsBag
        }
    }
}