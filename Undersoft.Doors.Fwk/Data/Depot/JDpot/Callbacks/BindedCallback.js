class BindedCallback {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {
        this.Method.Input = input;  
        let element = Inject.Parse(this.Method.Input.Content);
        this.Method.Object.Bind = element;
        if (this.Method.Id !== null) {
            Inject.Node(this.Method.Id, element);
        }
        if (this.Method.Callback != null)
            this.Method.Callback.Execute(this.Method);
    } 
}
