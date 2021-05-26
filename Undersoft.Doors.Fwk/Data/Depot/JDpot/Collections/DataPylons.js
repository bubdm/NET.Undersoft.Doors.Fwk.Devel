class DataPylons {

    static Type() { return "System.Doors.Data.DataPylons"; }
    GetType() { return "System.Doors.Data.DataPylons"; }

    constructor(trellis) {
        this._trell = trellis;
        this._bind = null;
        this._fields = new DataFields(this);

        this.List = [];
        this.Registry = {}
    }

    Get(idorname) {
        if (isNaN(parseInt(idorname))) {
            let pylonid = this.Registry[idorname];
            if (typeof (pylonid) !== "undefined")
                return this.List[pylonid];
        }
        else
            return this.List[idorname];
        return null;
    }
    Set(idorname, value) {
        if (isNaN(parseInt(idorname))) {
            let pylonid = this.Registry[idorname];
            if (typeof (pylonid) !== "undefined") {
                this.List[pylonid] = value;
                return this.List[pylonid];
            }
        }
        else {
            this.List[idorname] = value;
            return this.List[idorname];
        }
        return null;
    }

    GetHeader() {
        return this._trell;
    }
    GetMessage() {
        return this._trell._bag;
    }

    AddNew() {
        let pylon = new DataPylon(this._trell);
        pylon.PylonId = this.List.length;
        this.List.push(pylon)
        this.Registry[pylon.PylonName] = pid;
    }
    Add(pylon) {
        pylon.PylonId = this.List.length;
        pylon._trell = this._trell;
        this.List.push(pylon)
        this.Registry[pylon.PylonName] = pid;
    }
   
    Remove(pylon) {
        for (i = 0; i < this.length; i++) {
            if (this[i].PylonName == pylon.PylonName) {
                this.List[i] = null;
            }
        }
    }

    async Impact(pylons) {
        if (pylons != null) {
            let length = pylons.length;
            let mylength = this.List.length;
            if (mylength == 0 && length > 0)
                this.List = new Array(length);
            for (var i = 0; i < length; i++) {
                let pylon = pylons[i];
                let pid = pylon["PylonId"];
                if (pid < this.List.length) {
                    let isnew = false;
                    if (typeof (this.List[pid]) === "undefined") {
                        this.List[pid] = new DataPylon(this._trell);
                        this.Registry[pylon.PylonName] = pid;
                        isnew = true;
                    }
                    this.List[pid].Impact(pylon);
                    if(isnew)
                        this.List[pid].Definitions();
                }
            }    
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }
}