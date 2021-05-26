class Flush {

    static Class(i) {
        for (var e in Pick.Class(i)) {
            let last = null;
            while (last = e.lastChild) e.removeChild(last);
        }
    }

    static Id(i) {
        let e = Pick.Id(i);
        let last = null;
        while (last = e.lastChild) e.removeChild(last);
    }

    static Name(i) {
        let e = Pick.Name(i);
        let last = null;
        while (last = e.lastChild) e.removeChild(last);
    }

    static Tag(i) {
        for (var e in Pick.Tag(i)) {
            let last = null;
            while (last = e.lastChild) e.removeChild(last);
        }
    }

    static Data(i, html, value = null) {
        for (var e in Pick.Data(i, value)) {
            let last = null;
            while (last = e.lastChild) e.removeChild(last);
        }
    }   

    static Node(i) {
        let e = i;
        let last = null;
        while (last = e.lastChild) e.removeChild(last);
    }
}