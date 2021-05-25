class InitializedCallback {

    constructor(method = null) {
        this.Method = method;
        this.Input = null;
    }

    async Execute(input = null) {
        this.Input = input;
        this.Method.Initialize()
    }
}