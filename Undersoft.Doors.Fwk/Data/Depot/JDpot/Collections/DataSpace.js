
var DataSpace = null; 
var Complexity = "Standard";

class DataArea {

    GetType() { return this.Config.DataType; }

    constructor(name = "") {        
        this.SpaceName = name;
        this.Config = new DataConfig(this);
        this.Area = {};
    }       
}