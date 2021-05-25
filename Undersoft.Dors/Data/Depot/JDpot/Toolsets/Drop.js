class Drop {

    static Class(i) {
        for (var e in Pick.Class(i))
            e.parentNode.removeChild(e);
    }

    static Id(i) {
        let e = Pick.Id(i);
        e.parentNode.removeChild(e);
    }

    static Name(i) {
        let e = Pick.Name(i);
        e.parentNode.removeChild(e);
    }

    static Tag(i) {
        for (var e in Pick.Tag(i))
            e.parentNode.removeChild(e);
    }

    static Data(i, html, value = null) {
        for (var e in Pick.Data(i, value))
            e.parentNode.removeChild(e);
    }   
}