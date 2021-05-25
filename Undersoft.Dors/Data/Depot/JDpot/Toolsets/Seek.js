class Seek {

    constructor(obj, param, result, stop = null, top = false, onlykeys = false) {
        this.Obj = obj;
        this.Parent = obj;
        this.Param = (!Object.isExtensible(param)) ? new Array(param) : param;
        this.Result = result;
        this.Stop = stop;
        this.Top = top;
        this.OnlyKeys = onlykeys;
    }

    static Key(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let stop = { Now: false }
        let seek = new Seek(obj, key, result, stop, false, onlykeys);
        if (!html)
            seek.findKey(false, ignorecase, ignorekeys);
        else
            seek.findKeyHtml(false, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static Keys(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false, include = false) {
        let result = [];
        let seek = new Seek(obj, key, result, null, false, onlykeys);
        if (!html)
            seek.findKey(false, ignorecase, ignorekeys);
        else
            seek.findKeyHtml(false, ignorecase, ignorekeys, include);
        return result.length > 0 ? result : null;
    } 
    static TopKey(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let istop = true;
        let stop = { Now: false }
        let seek = new Seek(obj, key, result, stop, istop, onlykeys);
        seek.findKey(false, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static TopKeys(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let istop = true;
        let seek = new Seek(obj, key, result, null, istop, onlykeys);
        if (!html)
            seek.findKey(false, ignorecase, ignorekeys);
        else
            seek.findKeyHtml(false, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static HasKey(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let stop = { Now: false }
        let seek = new Seek(obj, key, result, stop, false, onlykeys);
        if (!html)
            seek.findKey(false, ignorecase, ignorekeys);
        else
            seek.findKeyHtml(false, ignorecase, ignorekeys);
        return result.length > 0 ? true : false;
    }
    static HasTopKey(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false) {
        let result = [];
        let istop = true; 
        let stop = { Now: false }
        let seek = new Seek(obj, key, result, stop, istop, onlykeys);
        seek.findKey(false, ignorecase, ignorekeys);
        return result.length > 0 ? true : false;
    }

    static WithKey(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let stop = { Now: false }
        let seek = new Seek(obj, key, result, stop, false, onlykeys);
        if (!html)
            seek.findKey(true, ignorecase, ignorekeys);
        else
            seek.findKeyHtml(true, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static WithKeys(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let seek = new Seek(obj, key, result);
        if (!html)
            seek.findKey(true, ignorecase, ignorekeys);
        else
            seek.findKeyHtml(true, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }     
    static WithTopKey(obj, key, ignorecase = true, ignorekeys = null, onlykeys = false) {
        let result = [];
        let istop = true;
        let stop = { Now: false }
        let seek = new Seek(obj, key, result, stop, istop, onlykeys);
        seek.findKey(true, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }

    static Value(obj, val, parent = false, ignorecase = false, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let stop = { Now: false }
        let seek = new Seek(obj, val, result, stop, false, onlykeys);
        if(!html)
            seek.findValue(parent, ignorecase, ignorekeys);
        else
            seek.findValueHtml(parent, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static Values(obj, val, parent = false, ignorecase = false, ignorekeys = null, onlykeys = false, html = false) {
        let result = [];
        let seek = new Seek(obj, val, result);
        if (!html)
            seek.findValue(parent, ignorecase, ignorekeys, null, false, onlykeys);
        else
            seek.findValueHtml(parent, ignorecase, ignorekeys, null, false, onlykeys);
        return result.length > 0 ? result : null;
    }  
    static TopValue(obj, val, parent = false, ignorecase = false, ignorekeys = null, onlykeys = false) {
        let result = [];
        let stop = { Now: false }
        let istop = true;
        let seek = new Seek(obj, val, result, stop, istop, onlykeys);
        seek.Top = true;
        seek.findValue(parent, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static TopValues(obj, val, parent = false, ignorecase = false, ignorekeys = null, onlykeys = false) {
        let result = [];
        let istop = true;
        let seek = new Seek(obj, val, result, null, istop, onlykeys);
        seek.findValue(parent, ignorecase, ignorekeys);
        return result.length > 0 ? result : null;
    }
    static HasValue(obj, val, parent = false, ignorecase = false, ignorekeys = null, onlykeys = false) {
        let result = [];
        let stop = { Now: false }
        let seek = new Seek(obj, val, result, stop, false);
        seek.findValue(false, ignorecase, ignorekeys);
        return result.length > 0 ? true : false;
    }
    static HasTopValue(obj, vals, parent = false, ignorecase = false, ignorekeys = null, onlykeys = false) {

        let result = [];
        let istop = true;
        let stop = { Now: false }
        let seek = new Seek(obj, vals, result, stop, istop, onlykeys);
        seek.findValue(false, ignorecase, ignorekeys);
        return result.length > 0 ? true : false;
    }

    static Pair(obj, keys, vals, ignorecase = true, html = true) {
        let preresult = Seek.Keys(obj, keys, true, null, false, true);   
        let array = (!Object.isExtensible(vals)) ? new Array(vals) : vals;
        let result = Seek.ListHtml(preresult, array);
        return result.length > 0 ? result : null;
    }
    static Pairs(obj, keys, vals, ignorecase = true, html = true) {
        let preresult = Seek.Keys(obj, keys, true, null, false, true);   
        let array = (!Object.isExtensible(vals)) ? new Array(vals) : vals;
        let result = Seek.ListHtml(preresult, array);
        return result.length > 0 ? result : null;
    }
    static TopPair(obj, keys, vals, ignorecase = true) {

        let result = [];
        let stop = { Now: false }
        let istop = true;
        let onlykeys = true;
        let seek = new Seek(obj, vals, result, stop, istop, onlykeys);
        seek.findValue(false, ignorecase, keys);
        return result.length > 0 ? result : null;
    }
    static TopPairs(obj, keys, vals, ignorecase = true) {
        let result = [];
        let stop = null;
        let istop = true;
        let onlykeys = true;
        let seek = new Seek(obj, vals, result, stop, istop, onlykeys);
        seek.findValue(false, ignorecase, keys);
        return result.length > 0 ? result : null;
    }
    static HasPair(obj, keys, vals, ignorecase = true) {
        let result = [];
        let istop = false;
        let stop = { Now: false }
        let onlykeys = true;
        let seek = new Seek(obj, vals, result, stop, istop, onlykeys);
        seek.findValue(false, ignorecase, keys);
        return result.length > 0 ? true : false;
    }
    static HasTopPair(obj, keys, vals, ignorecase = true) {
        let result = [];
        let istop = true;
        let stop = { Now: false }
        let onlykeys = true;
        let seek = new Seek(obj, vals, result, stop, istop, onlykeys);
        seek.findValue(false, ignorecase, keys);
        return result.length > 0 ? true : false;
    }
    
    async findKey(parent = false, ignorecase = true, ignorekeys = null) {

        let keys = [];
        let isArray = false;
        if (Object.isExtensible(this.Obj)) {

            let allkeys = Object.keys(this.Obj);

            if (allkeys.length > 0 && allkeys[0] !== "0")
                for (var alk in allkeys) {
                    let keyok = allkeys[alk];
                    let ignorecheck = keyok.includes('_') ? true : false;
                    if (ignorekeys != null)
                        if (!ignorecheck) {
                            for (var igk in ignorekeys) {

                                if (ignorecase)
                                    ignorecheck = ignorekeys[igk].toLowerCase().includes(keyok.toLowerCase());
                                else
                                    ignorecheck = ignorekeys[igk].includes(keyok);
                                if (ignorecheck)
                                    break;
                            }
                            if (this.OnlyKeys) {
                                if (ignorecheck)
                                    ignorecheck = false;
                                else
                                    ignorecheck = true;
                            }
                        }
                    if (!ignorecheck)
                        keys.push(keyok)
                }
            else {
                keys = allkeys;
                isArray = true;
            }
        }

        let istop = false;
        if (this.Top === true && !isArray)
            istop = true;

        let deleteparams = [];

        for (var i = 0; i < keys.length; i++) {

            if (this.Stop != null && this.Stop.Now) break;

            let key = keys[i];
            let obj = this.Obj[key];

            if (!isArray)
                for (var p in this.Param) {

                    let par = this.Param[p];

                    let keycheck = (ignorecase) ? (key.toLowerCase() === par.toLowerCase()) :
                        (key === par);
                    if (keycheck) {

                        if (!parent)
                            this.Result.push(obj);
                        else
                            this.Result.push(this.Obj);

                        if (this.Stop != null) {
                            deleteparams.push(p);
                            if (deleteparams.length == this.Param.length) {
                                this.Stop.Now = true;
                                break;
                            }
                        }
                    }
                }

            if (deleteparams.length > 0)
                for (var d in deleteparams)
                    this.Param.splice(d, 1);

            if (Object.isExtensible(obj) && !istop) {
                let recursive = new Seek(obj, this.Param, this.Result)
                recursive.Stop = this.Stop;
                recursive.Top = this.Top;
                recursive.OnlyKeys = this.OnlyKeys;
                recursive.findKey(parent, ignorecase, ignorekeys);
            }
        }
    }

    async findValue(parent = false, ignorecase = true, ignorekeys = null) {

        let vals = [];
        let isArray = false;
        if (Object.isExtensible(this.Obj)) {
            
            let allkeys = Object.keys(this.Obj);

            if (allkeys.length > 0 && allkeys[0] !== "0")
                for (var alk in allkeys) {
                    let keyok = allkeys[alk];
                    let ignorecheck = keyok.includes('_') ? true : false;
                    if (ignorekeys != null)
                        if (!ignorecheck) {
                            for (var igk in ignorekeys) {

                                if (ignorecase)
                                    ignorecheck = ignorekeys[igk].toLowerCase().includes(keyok.toLowerCase());
                                else
                                    ignorecheck = ignorekeys[igk].includes(keyok);
                                if (ignorecheck)
                                    break;
                            }
                            if (this.OnlyKeys) {
                                if (ignorecheck)
                                    ignorecheck = false;
                                else
                                    ignorecheck = true;
                            }
                        }
                    if (!ignorecheck)
                        vals.push(this.Obj[keyok])
                }
            else {
                if (allkeys.length != 0) {
                    vals = this.Obj;
                    isArray = true;
                }
                else
                    return;
            }
        }

        let istop = false;
        if (this.Top === true && !isArray)
            istop = true;

        let deleteparams = [];

        for (var i = 0; i < vals.length; i++) {

            if (this.Stop != null && this.Stop.Now) break;

            let val = vals[i];
          
            if (Object.isExtensible(val) && !istop) {
                let recursive = new Seek(val, this.Param, this.Result)
                recursive.Stop = this.Stop;
                recursive.Top = this.Top;
                recursive.OnlyKeys = this.OnlyKeys;
                if (parent)
                    recursive.Parent = this.Obj;
                recursive.findValue(parent, ignorecase, ignorekeys);

            }
            else {

                for (var p in this.Param) {

                    let par = this.Param[p];

                    let valcheck = (ignorecase && typeof (val) === 'string') ?
                        (val.toLowerCase() === par.toLowerCase()) :
                        (val === par);

                    if (valcheck) {

                        if (!parent)
                            this.Result.push(this.Obj);
                        else
                            this.Result.push(this.Parent);

                        if (this.Stop != null) {
                            deleteparams.push(p);
                            if (deleteparams.length >= this.Param.length) {
                                this.Stop.Now = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (deleteparams.length > 0)
                for (var d in deleteparams)
                    this.Param.splice(d, 1);

           
        }
    }

    async findKeyHtml(parent = false, ignorecase = true, ignorekeys = null, include = false) {

        let vals = [];
        let istop = false;

        if (this.Top === true)
            istop = true;

        let deleteparams = [];

        if (this.Obj.hasChildNodes || this.Obj.hasAttributes) {
          
            if (this.Obj.hasAttributes) {
                let length = this.Obj.attributes["length"];
                if (length > 0)
                    for (var p in this.Param) {

                        if (this.Stop != null && this.Stop.Now) break;

                        let par = this.Param[p];

                        for (var i = 0; i < length; i++) {
                            let key = this.Obj.attributes[i].name;
                            let valcheck = false;
                            if (include) {
                                valcheck = (ignorecase && typeof (key) === 'string') ?
                                    (key.toLowerCase().includes(par.toLowerCase())) :
                                    (key.includes(par));
                            }
                            else {
                                valcheck = (ignorecase && typeof (key) === 'string') ?
                                    (key.toLowerCase() === par.toLowerCase()) :
                                    (key === par);
                            }
                            if (valcheck) {

                                if (!parent)
                                    this.Result.push(this.Obj);
                                else
                                    this.Result.push(this.Parent);

                                if (this.Stop != null) {
                                    deleteparams.push(p);
                                    if (deleteparams.length >= this.Param.length) {
                                        this.Stop.Now = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
            }           

            if (this.Obj.hasChildNodes && !istop) {
                for (var chd in this.Obj.childNodes) {
                    let recursive = new Seek(this.Obj.childNodes[chd], this.Param, this.Result)
                    recursive.Stop = this.Stop;
                    recursive.Top = this.Top;
                    recursive.OnlyKeys = this.OnlyKeys;
                    if (parent)
                        recursive.Parent = this.Obj;
                    recursive.findKeyHtml(parent, ignorecase, ignorekeys);
                }
            }

            if (deleteparams.length > 0)
                for (var d in deleteparams)
                    this.Param.splice(d, 1);
        }
    }

    async findValueHtml(parent = false, ignorecase = true, ignorekeys = null, include = false) {

        let vals = [];
        let isArray = false;
        let istop = false;

        if (this.Top === true)
            istop = true;

        let deleteparams = [];

        if (this.Obj.hasChildNodes || this.Obj.hasAttributes) {

            if (this.Obj.hasAttributes) {
                let length = this.Obj.attributes["length"];
                if (length > 0)
                    for (var p in this.Param) {

                        if (this.Stop != null && this.Stop.Now) break;

                        let par = this.Param[p];

                        for (var i = 0; i < length; i++) {
                            let key = this.Obj.attributes[i].value;           
                            let valcheck = false;
                            if (include) {
                                valcheck = (ignorecase && typeof (key) === 'string') ?
                                    (key.toLowerCase().includes(par.toLowerCase())) :
                                    (key.includes(par));
                            }
                            else {
                                valcheck = (ignorecase && typeof (key) === 'string') ?
                                    (key.toLowerCase() === par.toLowerCase()) :
                                    (key === par);
                            }

                            if (valcheck) {

                                if (!parent)
                                    this.Result.push(this.Obj);
                                else
                                    this.Result.push(this.Parent);

                                if (this.Stop != null) {
                                    deleteparams.push(p);
                                    if (deleteparams.length >= this.Param.length) {
                                        this.Stop.Now = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
            }
          
            if (this.Obj.hasChildNodes && !istop) {
                for (var chd in this.Obj.childNodes) {
                    let recursive = new Seek(this.Obj.childNodes[chd], this.Param, this.Result)
                    recursive.Stop = this.Stop;
                    recursive.Top = this.Top;
                    recursive.OnlyKeys = this.OnlyKeys;
                    if (parent)
                        recursive.Parent = this.Obj;
                    recursive.findValueHtml(parent, ignorecase, ignorekeys);
                }
            }

            if (deleteparams.length > 0)
                for (var d in deleteparams)
                    this.Param.splice(d, 1);

        }
    }  

    static ListHtml(array, values, lookname = false, include = false) {
        let result = [];

        for (var p in array) {

            let a = array[p];
            if (a.hasAttributes) {
                let length = a.attributes["length"];
                if (length > 0)
                    for (var v in values) {

                        let par = values[v];

                        for (var i = 0; i < length; i++) {
                            let key = "";
                            if (lookname)
                                key = a.attributes[i].name;
                            else
                                key = a.attributes[i].value;
                            let valcheck = false;
                            if (include) {
                                valcheck = (typeof (key) === 'string') ?
                                    (key.toLowerCase().includes(par.toLowerCase())) :
                                    (key.includes(par));
                            }
                            else {
                                valcheck = (typeof (key) === 'string') ?
                                    (key.toLowerCase() === par.toLowerCase()) :
                                    (key === par);
                            }

                            if (valcheck) {
                                result.push(a);
                                break;
                            }
                        }
                    }
            }
        }
        return result;
    }
}