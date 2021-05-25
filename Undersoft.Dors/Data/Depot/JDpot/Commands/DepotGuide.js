async function DepotGuide()
{  
    dcn = new DepotConnection(new GuidedCallback());
    dcn.Complexity = "Guide";
    dcn.Content = DataSpace.SphereOn; 
    dcn.Initiate();
}