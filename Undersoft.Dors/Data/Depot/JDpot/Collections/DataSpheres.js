class DataSpheres {

    static Type() { return "System.Dors.Data.DataSpheres"; }

    GetType() { return "System.Dors.Data.DataSpheres"; }

    constructor(spheresId = "") {
        this._fields = new DataFields(this);

        this.config = new DataConfig(this);
        this.state = new DataState();
        this.spheres = {};      

        this.SpheresId = spheresId;
        this.DisplayName = "";
        this.SphereOn = {};
        this.SpheresIn = {};

        this.Definitions();
    }

    GetHeader() {
        return this;
    }
    GetMessage() {
        return null;
    }

    toJSON() {
        return {
            SpheresId: this.SpheresId,
            DisplayName: this.DisplayName,
            Config: this.Config,
            State: this.State,
            Spheres: this.Spheres,
            SpheresIn: this.SpheresIn
        }
    }

    get Config() { return this.config; }
    set Config(config) { this.config.Impact(config); }

    get State() { return this.state; }
    set State(state) { this.state.Impact(state); }

    get Spheres() { return this.spheres; }
    set Spheres(_spheres) {
        for (var i in _spheres) {
            let sphere = Seek.TopKey(this.spheres, i);
            if (sphere === null || sphere.length === 0) {
                let newsphere = new DataSphere(_spheres[i].SphereId);
                newsphere.Impact(_spheres[i]);
                this.spheres[i] = newsphere;

            }
            else {
                sphere[0].Impact(_spheres[i]);
            }
        }
    }

    AddNew(sphereId) {
        this.Spheres[sphereId] = new DataSphere(sphereId);        
    }

    Add(sphere) {
        this.Spheres[sphere.SphereId] = sphere;
    }

    Remove(sphereId) {
        delete this.Spheres[sphereId];
    }

    Definitions() {
        Object.defineProperty(DataSpheres.prototype, 'Spheres', { enumerable: true });
        Object.defineProperty(DataSpheres.prototype, 'Config', { enumerable: true });
        Object.defineProperty(DataSpheres.prototype, 'State', { enumerable: true });
    }

    async Impact(_spheres) {
        if (_spheres != null) {
            let keys = Object.keys(_spheres);
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = _spheres[key];
            }
        }
    }    
}    