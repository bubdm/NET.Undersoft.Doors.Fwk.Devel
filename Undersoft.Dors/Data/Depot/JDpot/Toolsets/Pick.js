class Pick {

    static Class(i, obj = null) {
        let node = (obj != null) ? obj : document;
        return typeof i === 'object' ? i : node.getElementsByClassName(i);
    }

    static Id(i, obj = null) {
        let node = (obj != null) ? obj : document;
        return typeof i === 'object' ? i : node.getElementById(i);
    }

    static Name(i, obj = null) {
        let node = (obj != null) ? obj : document;
        return typeof i === 'object' ? i : node.getElementsByName(i);
    }

    static Style(i, obj = null) {
        let node = (obj != null) ? obj : document;
        return this.Id(i, node).style;
    }

    static Tag(i, obj = null) {
        let node = (obj != null) ? obj : document;
        return typeof i === 'object' ? i : node.getElementsByTagName(i);
    }

    static Data(param, value = null, obj = null) {
        let node = (obj != null) ? obj : document;
        if (typeof param === 'object')
            return param;
        else
            if (value !== null)
                return $(node).filter(function (param, value) { return $(this).data(param) === value; });            
            else 
                return $(node).filter(function (param) { return $(this).data(param) !== undefined; });            
    }

    static AspId(i, obj = null) {
        let node = (obj != null) ? obj : document;
        if (typeof param === 'object')
            return param;
        else
            if (value !== null)
                return $(node).filter(function (param, value) { return $(this).data(param) === value; });
            else
                return $(node).filter(function (param) { return $(this).data(param) !== undefined; });
    }
}