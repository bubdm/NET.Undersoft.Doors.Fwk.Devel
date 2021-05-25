class SortTerms {

    static GetType() { return "System.Dors.Data.SortTerms"; }

    constructor(trellis) {
        this._trell = trellis;
        this._bind = null;
        this._fields = new DataFields(this);

        this.List = [];
    }

    Get(index) {
        return this.List[index];
    }
    Set(index, value) {
        this.List[index] = value;
        return this.List[index];
    }

    AddNew() {
        let term = new SortTerm();
        term.Index = this.List.length;
        this.Add(term)
    }

    Add(term) {
        term.Index = this.List.length;
        this.List.push(term);
    }

    AddTerm(pylon,direction = "ASC", orderid = -1) {
        let term = new SortTerm(pylon, direction);
        if (orderid < 0)
            term.Index = this.List.length;
        else
            term.Index = orderid;
        this.List.push(term);
    }

    Remove(term) {
        for (i = 0; i < this.List.length; i++) {
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
                    let newterm = new SortTerm();
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

    toJSON() {
        return null;        
    }
}