class DataBind {

    constructor(data, element, valueAsText = false, istemplate = false) {
        this._data = data;
        this.isTemplate = istemplate;
        this.template = null;
        this.element = null;
        if (!this.isTemplate) {
            this.element = element;
        }
        else {
            this.template = element.cloneNode(true);
            this.element = element;
            Flush.Node(this.element);
        }
        this.element.depot = data;

        this.valueAsText = valueAsText;
        if (this.element.hasAttributes) {
            let eventstring = this.element.getAttribute('depot-field-events');
            if (eventstring != null && eventstring.length > 0) {
                let events = eventstring.split(' ');
                for (var x in events) {
                    let event = events[x].split(':');
                    let cmd = event[0];
                    let mtd = event[1];
                    this.element[cmd] = this[mtd];
                }
            }
        }
        if (!valueAsText)
            this.valueKey = Clues.ValueKey(element);
        else
            this.valueKey = Clues.TextKey(element);

        this.textKey = Clues.TextKey(element);
        this.element.onblur = this.EditFieldEvent;

        this.Definitions();
    }

    get Element() {
        return this.element;
    }
    set Element(element) {
        this.element = element;
        this.element.depot = this._data;
        if (!valueAsText)
            this.valueKey = Clues.ValueKey(element);
        else
            this.valueKey = Clues.TextKey(element);
        this.textKey = Clues.TextKey(element);

        this.textKey = Clues.TextKey(element);
        this.element.onblur = this.EditFieldEvent;
    }

    get Id() {
        return this.element.id;
    }
    set Id(value) {
        this.element.id = value;
    }

    get Class() {
        return this.element.className;
    }
    set Class(value) {
        this.element.className = value;
    }  

    get Style() {
        return this.element.style;
    }
    set Style(value) {
        this.element.style = value;
    }  

    get Value() {
        return this.element[this.valueKey];
    }
    set Value(value) {
        this.element[this.valueKey] = value;
    }  

    get Text() {
        return this.element[this.textKey];
    }
    set Text(text) {
        this.element[this.textKey] = text;
    }    

    get OnChange() {
       return this.element.onblur;
    }    
    set OnChange(func) {
        this.element.onblur = func;
    }    

    get EditFieldEvent() {
        let fn =
            function (event) {
                event.preventDefault();
                let depot = event.target.depot;
                depot.value = event.target[depot._bind.valueKey];
                if (depot._data.hasOwnProperty('_tiers')) {
                    depot._data.Edited = true;
                    depot._data._tiers.ToBag(depot._data);
                }
            }
        return fn;
    }

    get CheckAllEvent() {
        let fn =
            function (event) {
                event.preventDefault();
                let depot = event.target.depot;
                if (this.checked)
                    depot._data.tiers.CheckAll();
                else
                    depot._data.tiers.UncheckAll();
            
            }
        return fn;
    }

    get ExpandDynamicEvent() {
        let fn =
            function (event) {
                event.preventDefault();
                let depot = event.target.depot;
                let element = Pick.Id(event.target.depot.value);
                if (element.depot.subtrell.tiers !== null &&
                    element.depot.subtrell.tiers._bind !== null &&
                    element.depot.subtrell.tiers._bind.element.childNodes.length > 0) {
                    if (element.classList.contains('visible')) {
                        element.classList.remove('active');
                        element.classList.remove('visible');
                        element.classList.add('hidden');
                    } else if (element.classList.contains('hidden')) {
                        element.classList.add('active');
                        element.classList.remove('hidden');
                        element.classList.add('visible');
                    }
                }
                else {
                    let pos = depot._data.Position;
                    let trl = depot._data._tiers._trell;
                    trl.SetPosition(parseInt(pos), 1);
                    let subtrl = element.depot._trell;
                    let trls = new DataTrellises();
                    trls.Add(trl);
                    trls.Add(subtrl);
                    DepotSync(trls, "Basic", new ExpandCallback(trl, element));
                }
            }
        return fn;
    }

    get ExpandStaticEvent() {
        let fn =
            function (event) {
                event.preventDefault();
                let depot = event.target.depot;
                let element = Pick.Id(event.target.depot.value);
                if (element.classList.contains('visible')) {
                    element.classList.remove('active');
                    element.classList.remove('visible');
                    element.classList.add('hidden');
                } else if (element.classList.contains('hidden')) {
                    element.classList.add('active');
                    element.classList.remove('hidden');
                    element.classList.add('visible');
                }
            }
        return fn;
    }

    Definitions() {
        Object.defineProperty(DataBind.prototype, 'Element', { enumerable: true });
        Object.defineProperty(DataBind.prototype, 'Id', { enumerable: true });
        Object.defineProperty(DataBind.prototype, 'Class', { enumerable: true });
        Object.defineProperty(DataBind.prototype, 'Style', { enumerable: true });
        Object.defineProperty(DataBind.prototype, 'Value', { enumerable: true });
        Object.defineProperty(DataBind.prototype, 'Text', { enumerable: true });
    }
}

class Clues {

    static ValueKey(element) {
        let tagName = element.tagName.toLowerCase();
        switch (tagName) {
            case "span":
                return "innerText";
            case "input":
                switch (element.type) {                 
                    case "checkbox":
                        return "checked";
                    default:
                        return "value";
                }
            case "option":
                return "value";
            case "select":
                return "value";
            case "a":
                return "href";
            case "img":
                return "src";
            case "label":
                return "innerText";
            default:
                return "value";
        }
    }

    static TextKey(element) {
        let tagName = element.tagName.toLowerCase();

        switch (tagName) {
            case "span":
                return "innerText";
            case "input":
                switch (element.type) {
                    case "checkbox":
                        return "innerText";
                    case "button":
                        return "value";
                    default:
                        return "text";
                }
            case "option":
                return "text";
            case "select":
                return "text";
            case "a":
                return "text";
            case "img":
                return "title";
            case "label":
                return "innerText";
            case "table":
                return "caption";
            default:
                return "innerText";
        }
    }

}