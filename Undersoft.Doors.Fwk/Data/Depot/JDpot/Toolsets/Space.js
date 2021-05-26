class Space {

    static SearchKey(key, obj = null, ignore = null) {
        let source = (obj != null) ? obj : DataSpace;
        let result = Seek.Key(source, key, false, ignore);
        return result;
    }

    static SearchValue(value, obj = null, ignore = null) {
        let source = (obj != null) ? obj : DataSpace;
        result = Seek.Value(source, value, false, false, ignore);
        return result;
    }   

    static FindPylons(names, obj = null) {
        let source = (obj != null) ? obj : DataSpace;
        let subresult = Seek.Value(source, names, false, false, ["Tiers", "Sims", "State", "Relays",
                                                          "Relaying", "SimsTotal", "Favorites", "Filter",
                                                          "Sort", "PagingDetails", "PrimaKey", "TiersTotal",
                                                          "SphereOn", "Config"]); 
        return subresult;
    }

    static FindTrells(names, obj = null) {
        
        let source = (obj != null) ? obj : DataSpace;
        let trells = Seek.Key(source, names, true, ["Pylons", "Tiers", "Sims", "State", "Relays",
                                                    "Relaying", "SimsTotal", "Favorites", "Filter",
                                                    "Sort", "PagingDetails", "PrimaKey", "TiersTotal",
                                                    "SphereOn", "Config"]);      
         let trellbag = new DataTrellises();
         trellbag.AddRange(trells);
         return trellbag;
    }   

    static ConfigValue(names, obj = null) {
        let source = (obj != null) ? obj : DataSpace;
        let subresult = Seek.WithKeys(source, "Config", true, ["Pylons", "Tiers", "Sims", "State", "Relays",
                                                               "Relaying", "SimsTotal", "Favorites", "Filter",
                                                                "Sort", "PagingDetails", "PrimaKey", "TiersTotal"]);
        let result = Seek.Value(subresult, names, true, true, ["Pylons", "Tiers", "Sims", "State",   "Relays",
                                                               "Relaying", "TiersTotal", "Favorites", "Filter",
                                                               "Sort", "PagingDetails", "PrimaKey", "Spheres",
                                                               "SpheresIn", "Trellises", "SphereIn", "SphereOn"]);        
        return result;
    }

    static ConfigKey(names, obj = null) {
        let source = (obj != null) ? obj : DataSpace;
        let subresult = Seek.Key(source, names, true, ["Pylons", "Tiers", "Sims", "State", "Relays",
                                                        "Relaying", "TiersTotal", "Favorites", "Filter",
                                                        "Sort", "PagingDetails", "PrimaKey", "PrimaKey", "Spheres",
                                                        "SpheresIn", "Trellises", "SphereIn", "SphereOn", "SimsTotal"]);
        return subresult;
    }

    static FindSphere(names, obj = null) {
        let source = (obj != null) ? obj : DataSpace;
        let subresult = Seek.Key(source, names, true, ["Pylons", "Tiers", "Sims", "Config",
                                                       "State", "Relays", "Relaying", "Trellises"]);
        return subresult;
    }

    static Emulate(obj) {
        let type = Seek.Key(obj, "DataType");
        if (type != null) {
            let instance = Tools.Instance(obj);
            if (instance != null)
                return instance.Impact(obj);
        }
        return null;
    }

    static Locate(obj) {        
        let idx = Space.ConfigKey("DataIdx", obj);
        if (idx !== null && idx.length > 0)
            return Space.ConfigValue(idx);
        return null;
    }

    static Impact(obj, dropatend = true) {
        let arr = [];
        let itr = Object.keys(obj);
        if (itr.length > 0 && itr[0] === "0")
            arr = obj;
        else
            arr.push(obj);
        let result = [];
        for (var a in arr) {
            let ext = arr[a];
            let founds = Space.Locate(arr[a]);
            if (founds != null) {
                for (var f in founds) {
                    //let found = founds[f];
                    founds[f].Impact(ext);
                    //let keys = Object.keys(ext);
                    //for (var i = 0; i < keys.length; i++) {
                    //    let key = keys[i];
                    //    found[key] = ext[key];
                    //}
                    result.push(founds[f]);
                }
            }
        }
        if (dropatend)
            obj = null;
        return result;
    }

    static Instance(dataType) {
        switch (dataType) {
            case "System.Doors.Data.DataSpheres":
                return new DataSpheres
            case "System.Doors.Data.DataSphere":
                return new DataSphere();
            case "System.Doors.Data.DataTrellises":
                return new DataTrellises();
            case "System.Doors.Data.DataTrellis":
                return new DataTrellis();
            case "System.Doors.Data.DataTiers":
                return new DataTiers();
            case "System.Doors.Data.DataTier":
                return new DataTier();
            case "System.Doors.Data.DataCells":
                return new DataCells();
            case "System.Doors.Data.DataCell":
                return new DataCell();
            case "System.Doors.Data.FilterTerms":
                return new FilterTerms();
            case "System.Doors.Data.FilterTerm":
                return new FilterTerm();
            case "System.Doors.Data.SortTerms":
                return new SortTerms();
            case "System.Doors.Data.SortTerm":
                return new SortTerm();
            case "System.Doors.Data.DataPylons":
                return new DataPylons();
            case "System.Doors.Data.DataPylon":
                return new DataPylons();
            case "System.Doors.Data.DataPageDetails":
                return new DataPageDetails();
            case "System.Doors.Data.DataConfig":
                return new DataConfig();
            case "System.Doors.Data.DataState":
                return new DataState();
            default:
                return null;
        }
    }

    static TypeName(obj) {
        let names = obj.GetType().split('.');
        return names[names.length - 1];      
    }

    static TypeClass(obj) {
        let name = Space.TypeName(obj);
        let classname = name.substring(4, name.length - 4).toLoweCase();
        return classname;
    }
}