class DataConfig {

    static Type() { return "System.Dors.Data.DataConfig"; }
    GetType() { return "System.Dors.Data.DataConfig"; }

    constructor(data = null) {
        this._data = data;
        this._syncing = false;
        this._fields = new DataFields(this);

        this.DataType = "";
        this.Place = "";
        this.DataIdx = "";
        this.DepotIdx = "";
        this.Path = "";
    }

    async Impact(config) {
        if (config != null) {
            let keys = Object.keys(config);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = config[key];
            }
            this._syncing = false;
        }
    }

    toJSON() {
        return {
            DataType: this.DataType,
            Place: this.Place,
            DataIdx: this.DataIdx,
            DepotIdx: this.DepotIdx,
            Path: this.Path
        }
    }
}