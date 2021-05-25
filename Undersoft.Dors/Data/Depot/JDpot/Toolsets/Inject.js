class Inject {

    static Class(i, html) {
        for (var e in Pick.Class(i))
            e.appendChild(Inject.Html(html));
    }

    static Id(i, html) {
        Pick.Id(i).appendChild(Inject.Html(html));
    }

    static Name(i, html) {
        Pick.Name(i).innerHTML = html;
    }

    static Tag(i, html) {
        for (var e in Pick.Tag(i))
            e.appendChild(Inject.Html(html));
    }

    static Data(i, html, value = null) {
        for (var e in Pick.Data(i, value))
            e.appendChild(Inject.Html(html));
    }   

    static Node(i, element) {
        Pick.Id(i).appendChild(element);
    }   

    static Html(html) {
        var t = document.createElement('template');
        t.innerHTML = html;
        return t.content;
    }

    static Parse(html) {
        let frame = document.createElement('iframe');
        frame.style.display = 'none';
        document.body.appendChild(frame);
        frame.contentDocument.open();
        frame.contentDocument.write(html);
        frame.contentDocument.close();
        let element = frame.contentDocument.body.firstChild;
        document.body.removeChild(frame);
        return element;      
    }
}