class Noid {

    constructor() {
        var self = this;
        self = {};
    }

    Contains(obj) {
        let key = Seek.TopKey(obj, "NoId");
        if (key != null)
            return this.ContainsKey(key);
        return false;
    }
    ContainsKey(key) {
        return Seek.HasTopKey(this, key);          
    }

    Get(key) {        
        return Seek.TopKey(this, key);
    }   
    GetRange(keys) {
        return Seek.TopKey(this, keys);
    }   

    TryGet(key, outresult = null) {
        let item = this.Get(key);
        if (item != null) {
            outresult = item;
            return true;
        }
        return false;
    }

    GetObj(obj) {
        let key = Seek.TopKey(obj, "NoId");
        if (key != null) {
            return this.Get(key);
        }
        return null;
    }
    GetObjRange(objs) {
        let keys = Seek.Key(objs, "NoId");
        return this.GetRange(keys);
    }

    TryAdd(obj, outresult) {
        let key = Seek.TopKey(obj, "NoId");
        if (key != null) {
            let check = this.Get(key);
            if (check === null) {
                this[key] = new DataTier();
                let result = this[key].Impact(obj);
                outresult.Value = result;
                return true;
            }
        }
        return false;
    }   

    TryPut(obj, outresult) {        
        let key = Seek.TopKey(obj, "NoId");
        if (key != null) {
            let result = {};            
            if (!this.TryAdd(obj, result)) {
                outresult.Value = this[key].Impact(obj);
                return false;
            }
            else {
                outresult.Value = result.Value;
                return true;
            }
        }
        return false;
    }

    PutRange(objs, outresults) {
        let added = [];
        outresults = [];
        for (var i in objs) {
            let result = {};
            if (this.TryPut(objs[i], result))
                added.push(result.Value);
            outresults.push(result.Value);
        }
        return added;
    }

    TryDelete(key, outresult) {
        let check = this.ContainsKey(key);
        if (check) {
            outresult = this[key];
            delete this[key];
        }
        return check;
    }  
    TryRemove(obj, outresult) {
        let check = false;
        let key = Seek.TopKey(obj, "NoId");
        if (key != null) {
            check = this.TryDelete(key, outresult);
        }
        return check;
    }  

    DeleteRange(keys, outresults) {
        for (var i in objs) {
            let result = null;
            if (this.TryDelete(objs[i], result))
                outresults.push(result);
        }
    }
    RemoveRange(objs, outresults) {
        for (var i in objs) {
            let result = null;
            if (this.TryRemove(objs[i], result))
                outresults.push(result);
        }
    }
}