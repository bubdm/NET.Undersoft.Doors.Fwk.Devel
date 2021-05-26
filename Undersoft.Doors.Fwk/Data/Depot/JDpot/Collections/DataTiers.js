class DataTiers {

    static Type() { return "System.Doors.Data.DataTiers"; }
    GetType() { return "System.Doors.Data.DataTiers"; }

    constructor(trellis, mode = "Tiers") {
        this._trell = trellis;
        this._bind = null;
        this._bag = trellis._bag;
        this._countview = 0;
        this._realcount = 0;

        this._tiersbag = trellis._bag
                                .DataBag[trellis
                                .TrellName];
        this._lastSync = [];

        this.Mode = mode;
        this.List = [];
    }

    Get(viewindex) {
        if (typeof (this.List[viewindex]) !== "undefined")
            return this.List[viewindex];
        else
            return null;
    }
    GetRange(viewindex, subtiers = null) {
        let result = (subtiers != null) ? subtiers : new DataTiers(this._trell, this.Mode);
        if (this._bind !== null)
            if (this._bind.isTemplate)
                result._bind = new DataBind(result, this._bind.template.cloneNode(true), true, true);
        
        for (var i in viewindex) {
            let item = this.Get(viewindex[i]);
     
            if (item != null) {
                if (subtiers != null) {
                    item._subtiers = subtiers;
                }
                result.List.push(item);
                result._countview++;
                result._realcount++;
            }
            else
                return null;
        }
        return result;
    }

    Set(tier, viewindex = null) {
        let vid = viewindex == null ? tier["ViewIndex"] : viewindex;
        if (vid < this._countview) {
            if (typeof (this.List[vid]) !== "undefined")
                return this.List[vid].Impact(tiers[id]);
        }
    }
    SetRange(tiers) {
        let result = [];
        for (var i in tiers) {
            let vid = tiers[id]["ViewIndex"];
            result.push(this.List[vid].Impact(tiers[id]));
        }
        return result;
    }
   
    Collect(key, values, distinct = true) {
        if(distinct)
            return Seek.TopPair(this.List, key, values, true);
        else
            return Seek.TopPairs(this.List, key, values, true);
    }   

    async ToBag(tier) {
        if (tier.BagIndex < 0) {
            tier.BagIndex = this._tiersbag.DataTiers.length;
            this._tiersbag.DataTiers.push(tier);
        }
    }
    async OffBag(tier) {
        this._tiersbag.DataTiers[tier.BagIndex] = null;
        tier.BagIndex = -1;
    }

    async PutRange(tiers) {
        this._lastSync = [];
        for (var id in tiers) {
            let vid = tiers[id]["ViewIndex"];
            if (vid < this._countview) {
                if (typeof (this.List[vid]) === "undefined") {
                    this.List[vid] = new DataTier(this);
                    this._realcount++;                    
                }                
                this._lastSync.push(this.List[vid].Impact(tiers[id]));
            }
        }
    } 
    async RefReplace(tiers) {      
        this.List = tiers;
    }

    GetHeader() {
        return this._trell;
    }
    GetMessage() {                
        return this._bag;
    }

    Remove(tier) {
        this.List[tier["ViewIndex"]] = null;
    }      
    RemoveRange(tiers) {
        for (var i in tiers)
            this.List[tiers[i]["ViewIndex"]] = null;
    }  

    Clear() {
        this.List = [];
    }

    GetChecked(state, objs = null) {
        let o = (objs == null) ? this.List : objs;
        return Collect.All(o, state);
    }

    CheckAll(objs = null) {
        let o = (objs == null) ? this.List : objs;
        for (var i in o)
            this.List[i].Check();
    }
    UncheckAll(objs = null) {
        let o = (objs == null) ? this.List : objs;
        for (var i in o)
            this.List[i].Uncheck();
    }

    async Impact(tiers) {
        if (tiers != null) {                                    
            let length = this.List.length;
            if (length === 0 || this._trell.CountView === 0 || this._trell.CountView !== length) {
                let count = this._trell.CountView;
                this.List = new Array(count);
                this._countview = count;
                this._realcount = 0;
            }                              
           return this.PutRange(tiers);
        }
    }
    async Replace(tiers) {
        if (tiers != null) {
            let results = [];
            this.PutRange(tiers, results);
            this.CheckAll(results);
            let toremove = GetChecked(false, this.List);
            this.RemoveRange(toremove);
            this.UncheckAll();
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true, true);
    }

    HtmlUpdate(appendbatch = true) {
        let fragment = null;
        if (appendbatch)
          fragment = document.createDocumentFragment();       
        for (var i in this._lastSync) {
            HtmlBinder.TierBind(this._lastSync[i], fragment);
        }              
        return fragment;
    }
}  