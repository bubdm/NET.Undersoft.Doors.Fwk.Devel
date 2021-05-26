class EnteredCallback {

    async Execute(input = null) {
        let header = input.Transaction.HeaderReceived;
        if (header.Content.Register) {

            DataSpace = new DataSpheres();
            DataSpace.SphereOn = new DataSphere();
            DataSpace.SphereOn.Config.Place = header.Context.Identity.DataPlace;
            DataSpace.SphereOn.Config.DataType = DataSpace.SphereOn.GetType();

            Flush.Id("WorkSpace");

            ToolbarsLayout();
            DepotGuide();            
        }
    }
}