class GuidedCallback {

    async Execute(input = null) {
        let head = input.ResponseHeader();
        if (!Seek.HasKey(head.Content, "Register")) {
            DataSpace.SphereOn.SphereId = head.Content.SphereId;
            DataSpace.SpheresId = head.Content.SpheresIn.SpheresId;
            DataSpace.Config = head.Content.SpheresIn.Config;
            DataSpace.Spheres = head.Content.SpheresIn.Spheres;
            console.log(DataSpace);
        }
    }
}