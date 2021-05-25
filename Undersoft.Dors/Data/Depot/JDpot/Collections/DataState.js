class DataState {

    static Type() { return "System.Dors.Data.DataState"; }
    GetType() { return "System.Dors.Data.DataState"; }

    constructor() {
        this._syncing = false;
        this._fields = new DataFields(this);

        this.edited = false;
        this.Deleted = false;
        this.Synced = false;
        this.Added = false;
        this.Canceled = false;
        this.Saved = false;
        this.Quered = false;
    } 

    get Edited() { return this.edited; }
    set Edited(edited) {
        if (!this._syncing)
            this.edited = edited;
    }

    async  Impact(state) {
        if (state != null) {
            let keys = Object.keys(state);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = state[key];
            }
            this._syncing = false;
        }
    }

    toJSON() {
        return {
            Checked: this.Checked,
            Edited: this.Edited,
            Deleted: this.Deleted,
            Synced: this.Synced,
            Added: this.Added,
            Canceled: this.Canceled,
            Saved: this.Saved,
            Quered: this.Quered
        }
    }
}