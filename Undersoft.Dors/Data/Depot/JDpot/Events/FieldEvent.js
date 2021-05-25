class FieldEvent {
    
    OnChange(func = null) {
        let fn = (func !== null) ? func :
            function (event) {
                let depot = event.target.dataDepot;
                depot.value = event.target[depot.bind.valueKey];
                depot._tier.Edited = true;
                depot._tier.Synced = false;
                depot._tier._tiers.ToBag(depot._tier);
            }
        return fn;
    }

    OnClick(func = null) {
        let fn = (func !== null) ? func :
            function (event) {
                let depot = event.target.dataDepot;
                depot.value = event.target[depot.bind.valueKey];
                depot._tier._tiers.ToBag(depot._tier);
            }
        return fn;
    }
}