class FilterTerms {

    static Type() { return "System.Doors.Data.FilterTerms"; }
    GetType() { return "System.Doors.Data.FilterTerms"; }

    constructor(trellis) {
        this._trell = trellis;
        this._bind = null;
        this._fields = new DataFields(this);

        this.List = [];
    }



    AddNew() {
        let term = new FilterTerm();
        term.Index = this.List.length;
        this.List.push(term)
    }

    Add(term) {
        term.Index = this.List.length;
        this.List.push(term);
    }

    AddTerm(pylon, oper, val, logic = "And", stage = "First") {
        let term = new FilterTerm(pylon, oper, val, logic, stage);
        term.Index = this.List.length;
        this.List.push(term);
    }

    Remove(term) {
        for (var i = 0; i < this.List.length; i++) {
            if (this.List[i].Index == term.Index) {
                this.List.splice(i, 1);
            }
        }
    }

    Clear() {
        this.List = [];
    }

    async Impact(terms) {
        if (terms != null) {
            let length = terms.length;
            for (var i = 0; i < length; i++) {
                let term = terms[i];
                if (!Seek.HasTopValue(this.List, term.PylonName)) {
                    let newterm = new FilterTerm();
                    newterm.Impact(term);
                    this.Add(newterm);
                }
                else {
                    let myterm = Seek.TopValue(this.List, term.PylonName);
                    if (myterm.length > 0)
                        myterm[0].Impact(term);
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