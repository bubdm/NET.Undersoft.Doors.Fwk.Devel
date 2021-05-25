class DataJoin {

    static Type() { return "System.Dors.Data.DataTiers"; }
    GetType() { return "System.Dors.Data.DataTiers"; }

    constructor(joins, vids, joinmember, type = "Child") {
        this._joins = joins;
        this._tier = this._joins._tier;
        this._bind = null;       
        this._fields = new DataFields(this);  
        this._pylons = null;
        this._trell = null;
        this.JoinName = this._tier._tiers._trell.TrellName + "_" + joinmember;
        this.JoinMember = joinmember;
        this.JoinKey = this._tier._tiers._trell.TrellName + "_" + joinmember + "_" + this._tier.ViewIndex;;       
        this.JoinIds = vids;
        this.Type = type;

        this.subtrell = new DataTrellis(this.JoinKey);

        this.Definitions();
    }

    get Pylons() {
        if (this.Type === "Child")
            if (this._pylons === null) {
                let tr = this._tier._tiers._trell._childs[this.JoinMember];
                this._pylons = new DataPylons(tr);
                this._pylons.List = tr.pylons.List;
                this._pylons.Registry = tr.pylons.Registry;
                this._pylons._bind = new DataBind(this._pylons, tr.pylons._bind.element.cloneNode(true), true);
            }
           
        if (this.Type === "Parent")
            if (this._pylons === null) {
                let tr = this._tier._tiers._trell._parents[this.JoinMember];
                this._pylons = new DataPylons(tr);
                this._pylons.List = tr.pylons.List;
                this._pylons.Registry = tr.pylons.Registry;
                this._pylons._bind = new DataBind(this._pylons, tr.pylons._bind.element.cloneNode(true), true);
            }
        return this._pylons;
    }
    set Pylons(value) {
        this._pylons = value;
    }

    get Trell() {
        if (this._trell === null) {
            if (this.Type === "Child")
                this._trell =  this._tier._tiers._trell._childs[this.JoinMember];
            if (this.Type === "Parent")
                this._trell = this._tier._tiers._trell._parents[this.JoinMember];
        }
        return this._trell;
    }

    get Tiers() {
        let joinTiers = this.Trell.tiers.GetRange(this.JoinIds, this.subtrell.tiers);
        if (joinTiers === null)
            this._tier._tiers.ToBag(this._tier);
        return joinTiers;
    }

    get SubTrell() {
        if (this.subtrell._bind == null)
            this.Imitate(this.Trell);
        return this.subtrell;
    }

    async Impact(field) {
        this.JoinIds = field.JoinIds;
        if (this._bind != null && this.subtrell._bind != null) {
            this.subtrell.tiers.Clear();
            this.subtrell.tiers = this.Tiers;
            HtmlBinder.TiersBind(this.subtrell.tiers, this.subtrell.element, true);
            for (var i in this.subtrell.tiers.List)
                HtmlBinder.TierBind(this.subtrell.tiers.List[i]);
        }
    }   
    async Imitate(trellis) {
        if (trellis != null) {
            this.subtrell.pylons = this.Pylons;
            this.subtrell.tiers = this.Tiers;
            HtmlBinder.TrellBind(this.subtrell, trellis._bind.element.cloneNode(true), true);
            if (this.subtrell.tiers !== null)
                for (var i in this.subtrell.tiers.List)
                    HtmlBinder.TierBind(this.subtrell.tiers.List[i]);
        }
    }    

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }

    Definitions() {
        Object.defineProperty(DataJoin.prototype, 'Trell', { enumerable: true });
        Object.defineProperty(DataJoin.prototype, 'Tiers', { enumerable: true });       
        Object.defineProperty(DataJoin.prototype, 'Pylons', { enumerable: true });  
        Object.defineProperty(DataJoin.prototype, 'SubTrell', { enumerable: true });
    }
}