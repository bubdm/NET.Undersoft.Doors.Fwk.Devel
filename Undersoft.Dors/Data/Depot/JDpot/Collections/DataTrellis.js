class DataTrellis {

    static Type() { return "System.Dors.Data.DataTrellis"; }    
    
    GetType() { return "System.Dors.Data.DataTrellis"; }    
    GetTypeName() { let names = this.GetType().split('.'); return names[names.length - 1]; }

    constructor(trellName = "", sphere = null) {     

        this.TrellName = trellName;
        this.DisplayName = trellName;

        let _msg = {}
        _msg[this.TrellName] = { DataTiers: [] }
        this._bag = new DataBag(_msg, false);    
        this._fields = new DataFields(this);
        this._sphere = sphere;
        this._childs = {};
        this._parents = {};
        this._bind = null; 
        this._syncing = false;
        
        this.Checked = false;       
        this.pylons = new DataPylons(this);
        this.config = new DataConfig(this);
        this.state = new DataState();
        this.filter = new FilterTerms(this);
        this.sort = new SortTerms(this);
        this.tiers = new DataTiers(this, "Tiers");      
        this.sims = new DataTiers(this, "Sims");
        this.tiersTotal = new DataTiers(this, "Tiers");       
        this.simsTotal = new DataTiers(this, "Sims");       
        this.paging = new DataPageDetails(this);
        this.relaying = {};       
        this.primeKey = [];

        this.TrellName = trellName;
        this.DisplayName = trellName;
        this.Mode = "Tiers";       
      
        this.Count = 0;
        this.CountView = 0;
        this.EditLevel = 1;
        this.SimLevel = 1;
        this.Favorites = {};   

        this.Definitions();
    }  

    SetPaging(page, size = null, pagecount = null, active = null) {
        if (page !== null)
            this.paging.Page = page;
        if (page !== null)
            this.paging.PageSize = size;
        if (page !== null)
            this.paging.CachedPages = pagecount;
        if (page !== null)
            this.paging.PageActive = active;
    }

    SetPosition(position, rangesize = 1) {
        if (position !== null) {         
            this.paging._page = this.paging.page;
            this.paging.page = position;
        }
        this.paging._pagecount = this.paging.PageCount;
        this.paging._pagesize = this.paging.PageSize;
        this.paging._cachepages = this.paging.CachedPages;

        this.paging.PageSize = rangesize;
        this.paging.CachedPages = 1;
    }

    ResetPosition() {
        this.paging.page = this.paging._page;
        this.paging.PageCount = this.paging._pagecount;
        this.paging.PageSize = this.paging._pagesize;
        this.paging.CachedPages = this.paging._cachepages;
    }

    Get(index) {
        return this.tiers.VId(index);
    }
    Set(index, idorname, value) {
        let tier = this.tiers.VId(index);
        if (tier !== null)
            if (idorname !== null)
                tier.Set(idorname, value);
            else
                tier = value;
        return tier;
    }

    GetHeader() {
        return this;
    }
    GetMessage() { return this._bag; }  

    get Quered() { return this.state.Quered; }
    set Quered(quered) {
        this.state.Quered = quered;
    }

    get Edited() { return this.state.Edited; }
    set Edited(edited) {
        if(!this._syncing)
            this.state.Edited = edited;        
    }

    get Saved() { return this.state.Saved; }
    set Saved(saved) {
        this.state.Saved = saved;
    }

    get Synced() { return this.state.Synced; }
    set Synced(synced) {
        this.state.Synced = synced;
    }

    get Canceled() { return this.state.Canceled; }
    set Canceled(canceled) {
        this.state.Canceled = canceled;
    }

    get Config() { return this.config; }
    set Config(config) { this.config.Impact(config); }

    get State() { return this.state; }
    set State(data) { this.state.Impact(data); }

    get PagingDetails() { return this.paging; }
    set PagingDetails(paging) { this.paging.Impact(paging); }

    get Pylons() { return this.pylons.List; }
    set Pylons(pylons) { this.pylons.Impact(pylons); }

    get Filter() { return this.filter.List; }
    set Filter(filter) { this.filter.Impact(filter); }

    get Sort() { return this.sort.List; }
    set Sort(sort) { this.sort.Impact(sort); }

    get Tiers() { return this.tiers.List; }
    set Tiers(trs) { this.tiers.Impact(trs); }

    get TiersTotal() { return this.tiersTotal.List; }
    set TiersTotal(trs) { this.tiersTotal.Impact(trs); }

    get Sims() { return this.sims.List; }
    set Sims(sms) { this.sims.Impact(sms); }

    get SimsTotal() { return this.simsTotal.List; }
    set SimsTotal(trs) { this.simsTotal.Impact(trs); }

    get PrimeKey() { return this.primeKey; }
    set PrimeKey(keys) {
        let arr = [];
        for (var i in keys) {
            arr.push(this.pylons.List[keys[i]["Ordinal"]]);
        }
        this.primeKey = arr;
    }

    get Relaying() { return this.relaying; }
    set Relaying(relaying) {
        this.relaying = relaying;

        if (Seek.HasKey(this.relaying, "ChildRelays"))
            if (this.relaying.ChildRelays.length > 0) {
                let cr = this.relaying.ChildRelays;
                for (var c in cr) {
                    if (!this._childs.hasOwnProperty(cr[c].Child.TrellName)) {
                        let fnd = Space.FindTrells(cr[c].Child.TrellName);
                        if (fnd !== null)
                            for (var fname in fnd)
                                this._childs[fname] = fnd[fname];
                    }
                }
            }

        if (Seek.HasKey(this.relaying, "ParentRelays"))
            if (this.relaying.ParentRelays.length > 0) {
                let parentnames = [];
                let pr = this.relaying.ParentRelays;
                for (var p in pr) {
                    if (!this._parents.hasOwnProperty(pr[p].Parent.TrellName)) {
                        let fnd = Space.FindTrells(pr[p].Parent.TrellName);
                        if (fnd !== null)
                            for (var fname in fnd)
                                this._parents[fname] = fnd[fname];
                    }
                }
            }
    }

    async Impact(trellis) {
        if (trellis !== null) {
            let keys = Object.keys(trellis);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = trellis[key];
            }
            this._syncing = false;       
        }
    }
   
    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        HtmlBinder.TrellBind(this, element);
    }

    Definitions() {

        this._fields.Put("TrellName", this.TrellName, true, false);
        this._fields.Put("DisplayName", this.DisplayName, true, false);
        this._fields.Put("Checked", this.Checked, true, false);
        this._fields.Put("CountView", this.CountView, true, false);
        this._fields.Put("Count", this.Count, true, false);
        this._fields.Put("Mode", this.Mode, true, false);

        Object.defineProperty(DataTrellis.prototype, 'Quered', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Canceled', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Saved', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Synced', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Edited', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Config', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'State', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'PagingDetails', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Pylons', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Filter', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Sort', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Tiers', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'TiersTotal', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Sims', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'SimsTotal', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'PrimeKey', { enumerable: true });
        Object.defineProperty(DataTrellis.prototype, 'Relaying', { enumerable: true });
    }

    toJSON() {
        switch (Complexity) {
            case "Basic":
                return {
                    TrellName: this.TrellName,
                    Mode: this.Mode,
                    Checked: this.Checked,
                    Edited: this.Edited,
                    Synced: this.Synced,
                    Saved: this.Saved,
                    Quered: this.Quered,
                    Config: this.Config,
                    State: this.State,
                    Filter: this.Filter,
                    Sort: this.Sort,
                    PagingDetails: this.PagingDetails
                }
            case "Standard":
                return {
                    TrellName: this.TrellName,
                    DisplayName: this.DisplayName,
                    Mode: this.Mode,
                    Checked: this.Checked,
                    Edited: this.Edited,
                    Synced: this.Synced,
                    Saved: this.Saved,
                    Quered: this.Quered,
                    EditLevel: this.EditLevel,
                    SimLevel: this.SimLevel,
                    Config: this.Config,
                    State: this.State,
                    Pylons: this.Pylons,
                    Filter: this.Filter,
                    Sort: this.Sort,
                    Favorites: this.Favorites,
                    PagingDetails: this.PagingDetails
                }
            default:
                return {
                    TrellName: this.TrellName,
                    Mode: this.Mode,
                    Checked: this.Checked,
                    Edited: this.Edited,
                    Synced: this.Synced,
                    Saved: this.Saved,
                    Quered: this.Quered,
                    Config: this.Config,
                    State: this.State,
                    Filter: this.Filter,
                    Sort: this.Sort,
                    PagingDetails: this.PagingDetails
                }
        }
    }

}