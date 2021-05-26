async function DepotSync(data, complexity, callbackFunction = null)
{     
    let dcn = new DepotConnection(new SyncedCallback(callbackFunction));
    Complexity = complexity;
    dcn.Complexity = complexity;
    dcn.Content = data; 
    dcn.Initiate();     
}