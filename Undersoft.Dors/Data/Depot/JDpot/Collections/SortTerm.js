class SortTerm {

    static GetType() { return "System.Dors.Data.SortTerm"; }

    constructor(pylon = "", direction = "ASC", orderid = -1) {
        this._bind = null;
        this._syncing = false;
        this._fields = new DataFields(this);

        this.Index = orderid;
        this.PylonName = pylon;
        this.Direction = direction;
    }  

   async Impact(term) {
        if (term != null) {
            let keys = Object.keys(term);
            this._syncing = true;
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                this[key] = term[key];
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

   toJSON() {
       return {
           Index: this.Index,
           PylonName: this.PylonName,
           Direction: this.Direction
       }
   }
}