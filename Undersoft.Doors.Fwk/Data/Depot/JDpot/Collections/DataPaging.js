class DataPageDetails {

    static Type() { return "System.Doors.Data.DataPageDetails"; }
    GetType() { return "System.Doors.Data.DataPageDetails"; }

    constructor(trell = null) {
        this._trell = trell;
        this._bind = null;      
        this._fields = new DataFields(this);
        this._switches = 5;

        this._page = 1;
        this._pagecount = 0;
        this._pagesize = 50;
        this._cachepages = 1;

        this.page = 1;
        this.PageActive = true;     
        this.CachedPages = 1;
        this.PageCount = 0;
        this.PageSize = 50;
        this.StartPage = 1;       
    }

    get Page() {
        return this.page;
    }
    set Page(page) {
        if (this._bind !== null)
            this.BuildPaging(page);
        else
            this.page = page;
    }

    BuildPaging(page = this.page) {
        if ((page !== this.page &&
            this._bind.element.childNodes.length > 0) ||
            this._bind.element.childNodes.length === 0) {
            // clear aktualnych linkow
            Flush.Node(this._bind.element);
            let fragment = document.createDocumentFragment();
            let pageSide = Math.floor((this._switches - 1) / 2);
            if (page <= pageSide) pageSide = this._switches - page;
            else if (this.PageCount - page < pageSide) pageSide = this._switches - (this.PageCount - page);

            for (var i = page - pageSide; i <= page + pageSide; i++) {
                if (i > 0 && i <= this.PageCount) {
                    fragment.append(Inject.Html(`<a class="page ${(i === page) ? 'active' : ''}" data-page="${i}">${i}</a>`));
                }
            }
            if (page > pageSide + 2) {
                fragment.prepend(Inject.Html('<span class="page">...</span>'));
            }
            if (page > pageSide + 1) {
                fragment.prepend(Inject.Html('<a class="page" data-page="1">1</a>'));
            }
            if (page < this.PageCount - pageSide - 1) {
                fragment.append(Inject.Html('<span class="page">...</span>'));
            }
            if (page < this.PageCount - pageSide) {
                fragment.append(Inject.Html(`<a class="page" data-page="${this.PageCount}">${this.PageCount}</a>`));
            }
            this._bind.element.appendChild(fragment);
            this.page = page;
        } else {
            console.log({ 'Pagination not prepared.': this._bind.element, 'page': page });
        }
    }

    get Bind() {
        return this._bind;
    }
    set Bind(element) {
        this._bind = new DataBind(this, element, true);
        this.BuildPaging();
    }

    async  Impact(paging) {
        if (paging != null) {
            let keys = Object.keys(paging);
            for (var i = 0; i < keys.length; i++) {
                let key = keys[i];
                if (Seek.HasTopKey(this, key, true, IgnoreKeys))
                    this[key] = paging[key];
            }          
        }
    }

    toJSON() {
        return {
            Page: this.Page,
            PageActive: this.PageActive,
            CachedPages: this.CachedPages,
            PageSize: this.PageSize
        }
    }
}