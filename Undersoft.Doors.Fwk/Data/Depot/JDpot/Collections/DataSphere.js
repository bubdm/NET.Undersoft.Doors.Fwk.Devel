
class DataSphere {

    static Type() { return "System.Doors.Data.DataSphere"; }
    GetType() { return "System.Doors.Data.DataSphere";}

    constructor(sphereId = "") {        
        this._bind = null;
        this._syncing = false;
        this._fields = new DataFields(this);

        this.trellises = new DataTrellises();
        this.config = new DataConfig(this);
        this.state = new DataState();
        this.spheresIn = new DataSpheres();

        this.SphereId = sphereId;
        this.DisplayName = sphereId;
        this.Relays = [];

        this.Definitions();
    }       

    GetHeader() {
        return this;
    }
    GetMessage() {      
        return this.trellises.GetMessage();
    }

    get Trellises() { return this.trellises; }
    set Trellises(trells) {
        for (var i in trells) {
            let trell = Seek.TopKey(this.trellises, i);
            if (trell === null) {
                let newtrell = new DataTrellis(trells[i].TrellName);
                newtrell.Impact(trells[i]);
                this.trellises[i] = newtrell;
            }
            else if (trell.length > 0)
                trell[0].Impact(trells[i]);
            else
                trell.Impact(trells[i]);            
        }
    }

    get Config() { return this.config; }
    set Config(config) { this.config.Impact(config); }

    get State() { return this.state; }
    set State(state) { this.state.Impact(state); }

    get SpheresIn() { return this.spheresIn; }
    set SpheresIn(spheres) { this.spheresIn.Impact(spheres); }

    async Impact(sphere) {
        if (sphere != null) {
            let keys = Object.keys(sphere);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];             
                this[key] = sphere[key];
            }
            this._syncing = false;
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
    }

    Definitions() {
        Object.defineProperty(DataSphere.prototype, 'Trellises', { enumerable: true });
        Object.defineProperty(DataSphere.prototype, 'Config', { enumerable: true });
        Object.defineProperty(DataSphere.prototype, 'State', { enumerable: true });
        Object.defineProperty(DataSphere.prototype, 'SpheresIn', { enumerable: true });
        Object.defineProperty(DataSphere.prototype, 'Bind', { enumerable: true });
    }

    toJSON() {
        switch (Complexity) {
            case "Basic":
                return {
                    SphereId: this.SphereId,
                    Trellises: this.Trellises,
                    SpheresIn: this.SpheresIn
                }
            case "Standard":
                return {
                    SphereId: this.SphereId,
                    DisplayName: this.DisplayName,
                    Trellises: this.Trellises,
                    Config: this.Config,
                    State: this.State,
                    SpheresIn: this.SpheresIn
                }
            default:
                return {
                    SphereId: this.SphereId,
                    Trellises: this.Trellises,
                    SpheresIn: this.SpheresIn
                }
        }
    }
}