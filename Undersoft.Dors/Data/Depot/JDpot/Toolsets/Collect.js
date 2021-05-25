class Collect {

    constructor(obj, key, values, result, stop = null, subkey = null) {
        this.Obj = obj;
        this.Key = key;
        this.Values = [];
        if (Object.isExtensible(values))
            this.Values = values
        else
            this.Values.push(values);
        this.Result = result;
        this.Stop = stop;
        this.SubKey = subkey;
    }
    
    static All(obj, key, values, subkey = null) {
        let result = [];
        let keycheck = false;
        if (subkey !== null) {
            if (Seek.HasKey(obj, subkey))
                keycheck = Seek.HasKey(obj, key);
        } else
            keycheck = Seek.HasKey(obj, key);
        if (keycheck) {
            let collect = new Collect(obj, key, values, result, null, subkey);
            collect.findItems();
        }
        return result.length > 0 ? result : null;
    }
    static First(obj, key, values, subkey = null) {
        let result = [];
        let stop = { Now: false }
        let keycheck = false;
        if (subkey !== null) {
            if (Seek.HasTopKey(obj, subkey))
                keycheck = Seek.HasKey(obj, key);
        } else
            keycheck = Seek.HasTopKey(obj, key);
        if (keycheck) {
            let collect = new Collect(obj, key, values, result, stop, subkey);
            collect.findItems();
        }
        return result.length > 0 ? result : null;
    }

    findItems() {
        if (Object.isExtensible(this.Obj)) {
            let key = this.Key;
            let values = this.Values
            let subkey = this.SubKey;
            let havesub = subkey !== null ? true : false;
            let valength = values.length;
            let length = this.Obj.length;
            let found = [];
            let count = 0;
            for (var i = 0; i < length; i++) {
                if (this.Stop != null && this.Stop.Now)
                    break;
                let item = (havesub) ? this.Obj[i][subkey] : this.Obj[i];
                for (var x = 0; x < valength; x++) {
                    if (values[x] === item[key]) {
                        if (this.Stop !== null) {
                            let singlecheck = false;
                            for (var z in found)
                                if (found[z] === x) {
                                    singlecheck = true;
                                    break;
                                }
                            if (!singlecheck) {
                                found.push(x);
                                this.Result.push(item);
                                count++;
                            }
                            if (count === valength)
                                this.Stop.Now = true;
                        }
                        else {
                            this.Result.push(item);
                            count++;
                        }
                    }
                }
            }
        }
    }
}