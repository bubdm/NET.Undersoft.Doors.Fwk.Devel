class ExpandCallback {

    constructor(trellis, element) {
        this.Trell = trellis;   
        this.Element = element;
    }

    async Execute(input = null) {
        let oldfragment = document.createDocumentFragment();
        let fragment = this.Trell.tiers.HtmlUpdate(true); 
        let length = fragment.childNodes.length;
        for (var i = 0; i < length; i++) {
            let tr = fragment.childNodes[0];
            let oldtr = Pick.Id(tr.id);
            if (oldtr !== null) {
                let parentElement = oldtr.parentNode;                
                parentElement.replaceChild(tr, oldtr);
            }          
        }
        let eid = this.Element.id;        
        let element = Pick.Id(eid);
        if (element.classList.contains('visible')) {
            element.classList.remove('active');
            element.classList.remove('visible');
            element.classList.add('hidden');
        } else if (element.classList.contains('hidden')) {
            element.classList.add('active');
            element.classList.remove('hidden');
            element.classList.add('visible');
        }
        this.Trell.ResetPosition();
    } 
}
