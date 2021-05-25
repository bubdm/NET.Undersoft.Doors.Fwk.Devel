$(document).ready(function () {
    let login = new DepotInject("Html/Panels/LoginPanel.html", "WorkSpace");
    login.Initiate();
})

async function DepotEnter(name = null, key = null)
{     
    let identity = new DepotIdentity(name, key);
    let dcn = new DepotConnection(new EnteredCallback(), identity);
    dcn.Initiate();     
}