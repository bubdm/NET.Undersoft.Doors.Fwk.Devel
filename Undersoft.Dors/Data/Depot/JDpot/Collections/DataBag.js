class DataBag {

    constructor(tiersbag = null, isbag = true) {
        this.DataBag = null;
        if (tiersbag != null)
            if (isbag) {
                let rs = Object.keys(tiersbag);
                for (var r in rs) {
                    let tiers = tiersbag[rs[r]];
                    this.DataBag = tiers;
                    break;
                }
            }
            else
                this.DataBag = tiersbag;
    }

    async Impact(dropatend = false) {             
        if (this.DataBag != null) {
            let trells = Object.keys(this.DataBag);
            for (var i in trells) {
                let key = trells[i];
                let trell = Space.FindTrells(key)[key];
                if (typeof (trell) !== "undefined") {
                    if (trell.length > 0)
                        trell = trell[0];
                    if (trell != null) {
                        let groups = Object.keys(this.DataBag[key]);
                        for (var g in groups) {
                            let group = groups[g];
                            if (trell.Mode != "Sims") {
                                trell.Tiers = this.DataBag[key][group];
                                for (var t in trell.tiers._tiersbag.DataTiers) {
                                    trell.tiers._tiersbag.DataTiers[t].BagIndex = -1;
                                }                               
                                trell.tiers._tiersbag.DataTiers = [];
                            }
                            else if (trell.Mode == "Sims") {
                                trell.Sims = this.DataBag[key][group];                                
                                for (var t in trell.tiers._tiersbag.DataTiers) {
                                    trell.sims._tiersbag.DataTiers[t].BagIndex = -1;
                                }
                                trell.sims._tiersbag.DataTiers = [];
                            }
                        }
                    }
                }
            }
            if (dropatend)
                this.DataBag = null;
        }               
    }  

    async Outpost(dropatend = false) {
        if (this.DataBag != null) {
            let rs = Object.keys(this.DataBag);
            for (var r in rs) {
                let root = this.DataBag[rs[r]];
                let trells = Object.keys(root);
                for (var i in trells) {
                    let key = trells[i];
                    let trell = Space.FindTrells(key)[key];
                    if (typeof (trell) !== "undefined") {
                        if (trell.length > 0)
                            trell = trell[0];
                        if (trell != null) {

                            let groups = Object.keys(root[key]);
                            for (var g in groups) {
                                let group = groups[g];
                                trell._bagIn.DataBag = root;
                                trell._bagOut.DataBag[key][group] = {}
                                if (trell.Mode != "Sims") {
                                    trell.Tiers = root[key][group];
                                }
                                else if (trell.Mode == "Sims") {
                                    trell.Sims = root[key][group];
                                }
                            }
                        }
                    }
                }
            }
        }
    }      

    Clear(trellName) {
        this.DataBag[trellName].DataTiers = [];
    }

    Remove(trellName) {
        let toremove = Seek.Value(this, trellName);
        delete this[trellName];        
    }  
}