async function ToolbarsLayout()
{     
    let topbar    = new DepotInject("Html/Toolbars/TopToolbar.html",    "TopToolbar");
    let bottombar = new DepotInject("Html/Toolbars/BottomToolbar.html", "BottomToolbar");
    let leftbar   = new DepotInject("Html/Toolbars/LeftToolbar.html",   "LeftToolbar",
                                       new InitializedCallback(new MenuInitialize()));

    leftbar.Initiate();
    topbar.Initiate();
    bottombar.Initiate();
}