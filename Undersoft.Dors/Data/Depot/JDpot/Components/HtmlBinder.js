class HtmlBinder {

    static TrellBind(trellis, element = null, prepared = false) {
        if(element != null)
            trellis._bind = new DataBind(trellis, element, true);
        if (trellis._bind != null) {            
            if (!prepared) {
                let dptbind = Seek.Keys(trellis._bind.element, "depot-trellis-bind", true, null, false, true);
                if (dptbind.length > 0) {
                    let fldsbind = Seek.ListHtml(dptbind, ["fields"]);
                    if (fldsbind.length > 0) {
                        for (var fi in fldsbind)
                            fldsbind[fi].classList.add('depot-trellis-fields');
                        HtmlBinder.FieldsBind(trellis._fields, fldsbind);
                    }
                    let pylsbind = Seek.ListHtml(dptbind, ["pylons"]);
                    if (pylsbind.length > 0) {
                        pylsbind[0].classList.add('depot-trellis-pylons');
                        HtmlBinder.PylonsBind(trellis.pylons, pylsbind[0]);
                    }
                    let trsbind = Seek.ListHtml(dptbind, ["tiers"]);
                    if (trsbind.length > 0) {
                        trsbind[0].classList.add('depot-trellis-tiers');
                        HtmlBinder.TiersBind(trellis.tiers, trsbind[0]);
                    }
                    let fltrbind = Seek.ListHtml(dptbind, ["filter"]);
                    if (fltrbind.length > 0) {
                        fltrbind[0].classList.add('depot-trellis-filters');
                        trellis.filter.Bind = fltrbind[0];
                    }
                    let sortbind = Seek.ListHtml(dptbind, ["sort"]);
                    if (sortbind.length > 0) {
                        sortbind[0].classList.add('depot-trellis-sorters');
                        trellis.sort.Bind = sortbind[0];
                    }
                    let paging = Seek.ListHtml(dptbind, ["paging"]);
                    if (paging.length > 0) {
                        paging[0].classList.add('depot-trellis-paging');
                        trellis.paging.Bind = paging[0];
                    }
                }
            }
            else {
                HtmlBinder.FieldsBind(trellis._fields, Pick.Class('depot-trellis-fields', trellis._bind.element), true);
                HtmlBinder.PylonsBind(trellis.pylons, Pick.Class('depot-trellis-pylons', trellis._bind.element)[0], true);
                HtmlBinder.TiersBind(trellis.tiers, Pick.Class('depot-trellis-tiers', trellis._bind.element)[0], true);            
            }
        }
    }

    static PylonsBind(pylons, element, prepared = false) {
        if (pylons._bind === null) {
            pylons.Bind = element;
            if (pylons.List.length > 0) {
                let fldreg = {};
                if (!prepared) {
                    let pylsbind = Seek.Keys(element, "depot-pylons", true, null, false, true);
                    if (pylsbind.length > 0) {
                        let temp = pylsbind;
                        for (var x in temp) {
                            temp[x].classList.add('depot-trellis-pylon');
                            let pylname = temp[x].getAttribute("depot-pylons");
                            fldreg[pylname] = {};
                            fldreg[pylname].element = temp[x];
                            let fldname = temp[x].getAttribute("depot-pylons-fields");
                            fldreg[pylname].field = fldname;
                        }
                    }
                }
                else
                    fldreg = Pick.Class('depot-trellis-pylon', element);

                for (var fldr in fldreg) {
                    let fld = null;
                    let pyl = null;
                    if (!prepared)
                        fld = fldr;
                    else
                        fld = fldreg[fldr].getAttribute("depot-pylons")
                    pyl = pylons.Get(fld);
                    if (pyl != null) {
                        let _fld = null;
                        if(!prepared)
                           _fld = pylons.List[pylons.Registry[fld]]
                                ._fields.Get(fldreg[fld].field, true);
                        else
                            _fld = pylons.List[pylons.Registry[fld]]
                                ._fields.Get(fldreg[fldr].getAttribute("depot-pylons-fields"), true);
                        if (_fld != null) {
                            let elem = null;
                            if (!prepared)
                                elem = fldreg[fld].element;
                            else
                                elem = fldreg[fldr];
                            pylons.List[pylons.Registry[fld]]
                                ._fields[fldreg[fld]
                                    .field.toLowerCase()]
                                .Bind = elem;
                        }
                    }
                }
            }
        }       
        else {
            let parentElement = element.parentNode;
            parentElement.replaceChild(pylons._bind.element, element);
        }

    }

    static TiersBind(tiers, element, prepared = false) {
        if (tiers !== null)
            if (tiers._bind === null) {
                tiers.Bind = element;
                if (!prepared) {
                    let template = tiers._bind.template;
                    let trsbind = Seek.Keys(template, "depot-tiers-bind", true, null, false, true);
                    if (trsbind.length > 0) {
                        let fldbinds = Seek.ListHtml(trsbind, ["fields"]);
                        let isfld = (fldbinds.length > 0) ? true : false;
                        if (isfld) {
                            fldbinds[0].classList.add('depot-tier-fields');
                            let dfld = Seek.Keys(fldbinds[0], "depot-fields", true, null, false, true);
                            if (dfld.length > 0) {
                                for (var x in dfld) {
                                    dfld[x].classList.add('depot-tier-field');
                                }
                            }
                        }
                        let chdbinds = Seek.ListHtml(trsbind, ["childjoins"]);
                        let ischd = (chdbinds.length > 0) ? true : false;
                        if (ischd) {
                            chdbinds[0].classList.add('depot-tier-childjoins');
                            let dchd = Seek.Keys(chdbinds[0], "depot-childjoins", true, null, false, true);
                            if (dchd.length > 0) {
                                for (var x in dchd) {
                                    dchd[x].classList.add('depot-tier-childjoin');
                                    dchd[x].classList.add(dchd[x].getAttribute('depot-childjoins'));
                                }
                            }
                        }
                        let prtbinds = Seek.ListHtml(trsbind, ["parentjoins"]);
                        let isprt = (prtbinds.length > 0) ? true : false;
                        if (isprt) {
                            prtbinds[0].classList.add('depot-tier-parentjoins');
                            let dprt = Seek.Keys(prtbinds[0], "depot-parentjoins", true, null, false, true);
                            if (dprt.length > 0) {
                                for (var x in dprt) {
                                    dprt[x].classList.add('depot-tier-parentjoin');
                                    dprt[x].classList.add(dprt[x].getAttribute('depot-parentjoins'));
                                }
                            }
                        }
                    }
                }
            }
            else {
                let parentElement = element.parentNode;
                parentElement.replaceChild(tiers._bind.element, element);
            }
    }

    static TierBind(tier, appendFragment = null) {
        let template = tier._tiers._bind.template;
        if (tier._subtiers != null)
            template = tier._subtiers._bind.template;
        let fldtierx = null;
        let chdtierx = null;
        let prttierx = null;
        let trellname = tier._tiers._trell.TrellName;
        let fldtr = Pick.Class('depot-tier-fields', template);
        if (fldtr.length > 0) {
            fldtierx = fldtr[0].cloneNode(true);
            fldtierx.id = trellname + "_Tiers_" + tier.ViewIndex;
            tier.Bind = fldtierx;
            let temp = Pick.Class('depot-tier-field', tier._bind.element);
            if (temp.length > 0) {
                let length = temp.length;
                for (var x = 0; x < length; x++) {
                    let fldname = temp[x].getAttribute("depot-fields");
                    let _fld = tier.fields.Get(fldname);
                    if (_fld != null) {
                        tier.fields[_fld.fieldId].Bind = temp[x];
                    }
                }
            }
            let appendElement = null;
            if (appendFragment != null) {
                appendElement = appendFragment;
            }
            else {
                if (tier._subtiers == null)
                    appendElement = tier._tiers._bind.element;
                else
                    appendElement = tier._subtiers._bind.element;
            }
            appendElement.appendChild(tier._bind.element);
            tier.Revalue();
        }

        let chdtr = Pick.Class('depot-tier-childjoins', template);
        if (chdtr.length > 0) {
            chdtierx = chdtr[0].cloneNode(true);
            chdtierx.id = trellname + "_Childs_" + tier.ViewIndex;
            tier.ChildJoins.Bind = chdtierx;
            let temp = Pick.Class('depot-tier-childjoin', tier.ChildJoins._bind.element);
            if (temp.length > 0) {
                let length = temp.length;
                for (var x = 0; x < length; x++) {
                    let fldname = temp[x].getAttribute("depot-childjoins");
                    let _fld = tier.ChildJoins[fldname];
                    if (typeof (_fld) !== "undefined") {
                        temp[x].id = tier.ChildJoins[fldname].JoinKey
                        tier.ChildJoins[fldname].Bind = temp[x];
                        tier.ChildJoins[fldname]._bind.element.appendChild(tier.ChildJoins[fldname].SubTrell._bind.element);
                    }
                }
            }
            let appendElement = null;
            if (appendFragment != null) {
                appendElement = appendFragment;
            }
            else {
                if (tier._subtiers == null)
                    appendElement = tier._tiers._bind.element;
                else
                    appendElement = tier._subtiers._bind.element;
            }
            appendElement.appendChild(tier.ChildJoins._bind.element);
            tier.Revalue();
        }
        let prttr = Pick.Class('depot-tier-parentjoins', template);
        if (prttr.length > 0) {
            prttierx = prttr[0].cloneNode(true);
            prttierx.id = trellname + "_Parents_" + tier.ViewIndex;
            tier.ParentJoins.Bind = prttierx;
            let temp = Pick.Class('depot-tier-parentjoin', tier.ParentJoins._bind.element);
            if (temp.length > 0) {
                let length = temp.length;
                for (var x = 0; x < length; x++) {
                    let fldname = temp[x].getAttribute("depot-parentjoins");
                    let _fld = tier.ParentJoins[fldname];
                    if (typeof (_fld) !== "undefined") {
                        temp[x].id = tier.ParentJoins[fldname].JoinKey
                        tier.ParentJoins[fldname].Bind = temp[x];
                        tier.ParentJoins[fldname]._bind.element.appendChild(tier.ParentJoins[fldname].SubTrell._bind.element);
                    }
                }
            }
            let appendElement = null;
            if (appendFragment != null) {
                appendElement = appendFragment;
            }
            else {
                if (tier._subtiers == null)
                    appendElement = tier._tiers._bind.element;
                else
                    appendElement = tier._subtiers._bind.element;
            }
            appendElement.appendChild(tier.ParentJoins._bind.element);
            tier.Revalue();
        }
        return tier._bind.element;
    }

    static FieldsBind(fields, elements, prepared = false) {
        let isfld = (elements.length > 0) ? true : false;
        let fldreg = {};
        if (isfld) {
            if (!prepared) {
                let temp = Seek.ListHtml(elements, ["depot-fields"], true);
                if (temp.length > 0) {
                    for (var x in temp) {
                        temp[x].classList.add('depot-trellis-field');
                        fldreg[temp[x].getAttribute("depot-fields")] = temp[x];
                    }
                }
            }
            else
                fldreg = Pick.Class('depot-trellis-fields', elements);

            for (var fldr in fldreg) {
                let fld = null;
                let pyl = null;
                if (!prepared)
                    fld = fldr;
                else
                    fld = fldreg[fldr].getAttribute("depot-fields")
                let _fld = fields.Get(fld, true);
                if (_fld != null) {
                    if (!prepared)
                        fields[_fld.fieldId].Bind = fldreg[fld];
                    else
                        fields[_fld.fieldId].Bind = fldreg[fldr];
                }
            }
        }
    }
}