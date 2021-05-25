class PagingCallback {

    constructor(trellis, clean = true) {
        this.Trell = trellis;   
        this.Clean = clean;
    }

    async Execute(input = null) {
        let element = this.Trell.tiers._bind.element;
        let fragment = this.Trell.tiers.HtmlUpdate(true);
        if(this.Clean)
            Flush.Node(element);
        element.appendChild(fragment);     
    } 
}
