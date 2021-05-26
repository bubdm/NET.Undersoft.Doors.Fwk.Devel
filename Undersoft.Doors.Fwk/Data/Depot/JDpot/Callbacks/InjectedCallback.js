class InjectedCallback {

    constructor(method = null) {
        this.Method = method;
    }

    async Execute(input = null) {
        this.Method.Input = input;
        Inject.Id(this.Method.Id, this.Method.Input.Content);
        if (this.Method.Callback != null)
            this.Method.Callback.Execute(this.Method);
    }
}