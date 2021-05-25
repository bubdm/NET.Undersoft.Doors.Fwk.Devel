async function ProductManagmentLayout() {
    DepotSync(Space.FindSphere(["Inventory"])[0], "Standard", new ProductManagmentInject());  
}

class ProductManagmentInject {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {      
        let inject = new DepotInject("Html/Modules/ProductManagment.html", "WorkSpace", new ProductGeneralBind());
        inject.Initiate();
    }   
}

class ProductGeneralBind {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {     

        let trells = Space.FindTrells(["Product^General", "Product^Markets"]);       

        let bind2 = new DepotBind("Html/Grids/ProductGeneral_ProductMarkets.html",
            trells["Product^Markets"]);
        bind2.Initiate();

        let bind = new DepotBind("Html/Grids/ProductGeneral_GridPanel.html",
            trells["Product^General"], new ProductGeneralSync());
        bind.Initiate();                  
    }
}

class ProductGeneralSync {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {
        let trl = Space.FindTrells(["Product^General"]);
        trl["Product^General"].ResetPosition();
        DepotSync(trl["Product^General"], "Basic", new ProductGeneralInject());
    }
}


class ProductGeneralInject {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {       

        let t = Space.FindTrells(["Product^General"])["Product^General"];
        let bind = new DepotBind("Html/Grids/ProductGeneral_StatePanel.html", t, null, "StatePanel");
        bind.Initiate();      

        let element = t.tiers._bind.element;        
        let fragment = t.tiers.HtmlUpdate(true);
        Flush.Node(element);
        element.appendChild(fragment);
        Inject.Node("GridPanel", t._bind.element);
       
        let query = new DepotInject("Html/Grids/ProductGeneral_QueryPanel.html", "QueryModal");
        query.Initiate();
        let pylon = new DepotInject("Html/Grids/ProductGeneral_PylonsPanel.html", "PylonsModal");
        pylon.Initiate();         
        let actions = new DepotInject("Html/Grids/ProductGeneral_ActionsPanel.html", "ActionsPanel", new InitializedCallback(new ExtensionInitialize()));
        actions.Initiate();
    }

}