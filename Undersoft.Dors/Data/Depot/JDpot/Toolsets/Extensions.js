/*
   Funkcja odnajsuje element o podanym id w calym DOM
   param id - szukane id elementu, bez części generowanej przez ASP)
   param context - opcjonalny argument umożliwiający zawęrzenie szukania do określonego elementu w formie obiektu jQuery

   @return jQuery object
*/
function $$(id, context) {
	var el = $(`#${id}`, context);
	if (el.length < 1)
		el = $(`[id$=_${id}]`, context);
	return el;
}
$.expr[":"].contains = $.expr.createPseudo(function (arg) {
	return function (elem) {
		return $(elem).text().toUpperCase().indexOf(arg.toUpperCase()) >= 0;
	};
});

/*
 * Cookies
 */
function setCookie(cname, cvalue, exdays) {
	var d = new Date();
	d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
	var expires = "expires=" + d.toUTCString();
	document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}
function getCookie(cname) {
	var name = cname + "=";
	var ca = document.cookie.split(';');
	for (var i = 0; i < ca.length; i++) {
		var c = ca[i];
		while (c.charAt(0) === ' ') {
			c = c.substring(1);
		}
		if (c.indexOf(name) === 0) {
			return c.substring(name.length, c.length);
		}
	}
	return "";
}

function Debug() {
	let currentState = getCookie('debug');
	setCookie('debug', currentState === 'true' ? 'false' : 'true', 1);
	location.reload();
}

var charts;
$(document).ready(function () {
	// ladowanie biblioteki tylko gdy jest potrzebna
	if ($("[data-chart], div.progress").length > 0) {
		$.getScript(`${scriptsDirectoryPath}neptun/charts.js`,
			function () {
				charts = new Charts();
				charts.initialize();
				charts.gauges();
				charts.lines();
				charts.progress();
				charts.bars();
			});
	}

	// ladowanie warunkowe obslugi bogatych multi-list
	if ($(".inputMenu").length > 0) {
		$.getScript(`${scriptsDirectoryPath}neptun/richList.js`);
	}


	// ladowanie obslugi dla menu bocznego
	//$.getScript(`${scriptsDirectoryPath}neptun/menu.js`,
	//    function () {
	//        const menu = new Menu();
	//        menu.initialize();
	//    });

	// ladowanie warunkowe obslugi liczb i walut
	//if ($("[class*='js-numeric'], [id$=FiltersGeneralCurrency]").length > 0) {
	//    $.getScript(`${scriptsDirectoryPath}neptun/numerics.js`);
	//}


    /*
        Helpy i tooltipy
    */
	//$(document).tooltip();
	$(document).tooltip({
		items: "[data-tooltip]",
		content: function () {
			return $$($(this).data("tooltip")).html();
		}
	});

	// obsługa gridow
	//if ($(".js-grid").length > 0) {
	//    $.getScript(`${scriptsDirectoryPath}neptun/gridSorter.js`);
	//    $.getScript(`${scriptsDirectoryPath}neptun/gridFilters.js`);
	//}


});

function dropDownUpdate() {
	$(".dd_chk_select").each(function () {
		let items = $(this).find(".dd_chk_drop input[type=checkbox]:checked").length;
		let caption = "Wybierz plik";

		if (items > 0) {
			caption = `Wybrano ${items} pozycji`;
		}

		$(this).find('#caption').html(caption);
	});
}

let grid = {},
    currentPage = 1,
    gridDataLoading = false,
    paginationObject = null,
    paginationSize = 5,
    recaling = [],
    recalLimit = 10;


/**
 * Pokazanie / ukrycie komunikatu ładowania danych przez Ajax
 * @param {boolean} hide loading window
 * @param {string} content (optional) of loading window
 */
function AjaxLoading(hide, content) {
	let loadingStatus = $$('LoadingStatus');
	if (hide === true) {
		loadingStatus.addClass('hidden');
		$(document.body).css({ 'cursor': 'default' });
	} else if (loadingStatus.is(":not(:visible)")) {
		let popupText = loadingStatus.find('.text');
		popupText.html(content ? content : popupText.data("defaulttext"));

		loadingStatus.removeClass('hidden');
		$(document.body).css({ 'cursor': 'wait' });
	}
}

//  Zdalne przełączanie paneli

var modalWindow = null;

/*
 *   Wyświtlanie popupa z tekstem
 *   @options:
 *     - header
 *     - content
 *     - listing
 *     - button_action
 *     - button_text
 *     - width
 *     - height
*/
function TextPopUp(options) {
	if (modalWindow === null) {
		modalWindow = $('<div id="modal" class="modal fade" tabindex="-1" role="dialog"><div class="modal-dialog" role="document"><div class="modal-content"><div class="modal-header"><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button><h4 class="modal-title">Modal title</h4></div><div class="modal-body"><p>One fine body&hellip;</p></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Zamknij</button><button type="button" class="btn btn-primary modal-action hidden">Save changes</button></div></div></div></div>');

		$("body").append(modalWindow);
	}

	if (options.title !== undefined)
		modalWindow.find(".modal-title").html(options.title);
	if (options.content !== undefined)
		modalWindow.find(".modal-body").html(options.content);

	if (options.listing !== undefined && options.listing !== null && options.listing !== "") {
		modalWindow.find(".modal-body").append("<div class='row'><textarea class='listing col-sm-12'>" + options.listing + "</textarea></div>");
	}

	if (options.button_text !== undefined) {
		modalWindow.find(".modal-action").text(options.button_text);
	}
	if (options.button_action === undefined) {
		modalWindow.find(".modal-action").addClass('hidden');
	} else {
		modalWindow.find(".modal-action").removeClass('hidden');
		modalWindow.find(".modal-action").attr("onClick", options.button_action);
	}

	if (options.width !== undefined) {
		if (options.width === '100%') {
			modalWindow.find(".modal-dialog").addClass('fullsize');
		}
		else {
			modalWindow.find(".modal-dialog").width(options.width);
		}
	}

	modalWindow.modal("show");

	return false;
}

function toJSON(node) {
	node = node || this;
	var inType = false;
	var selType = false;
	var obj = {
		nodeType: node.nodeType
	};
	if (node.tagName) {
		var tagname = node.tagName.toLowerCase();
		obj.tagName = tagname;
		if (tagname === 'input')
			inType = true;
		else if (tagname === 'option')
			selType = true;
	} else
		if (node.nodeName) {
			obj.nodeName = node.nodeName;
		}
	if (node.nodeValue) {
		obj.nodeValue = node.nodeValue;
	}
	var attrs = node.attributes;
	if (attrs) {
		var length = attrs.length;
		var arr = obj.attributes = new Array(length);
		var type = '';
		var valIndex = -1;
		for (var i = 0; i < length; i++) {
			attr = attrs[i];
			var remVal = false;
			if (inType) {
				if (attr.nodeName === 'type')
					type = attr.nodeValue;
				if (attr.nodeName === 'checked' || attr.nodeName === 'value')
					valIndex = i;
			}
			if (selType) {
				if (attr.nodeName === 'selected')
					valIndex = i;
			}
			arr[i] = [attr.nodeName, attr.nodeValue];
		}
		if (inType) {
			if (type !== '' && (type === 'checkbox' || type === 'radio'))
				if (valIndex >= 0)
					arr[valIndex] = ['checked', node.checked + ""];
				else
					arr[i++] = ['checked', node.checked + ""];
			else if (type !== '')
				if (valIndex >= 0)
					arr[valIndex] = ['value', node.value + ""];
				else
					arr[i++] = ['value', node.value + ""];
		}
		if (selType) {
			if (valIndex >= 0)
				arr[valIndex] = ['selected', node.selected + ""];
			else
				arr[i++] = ['selected', node.selected + ""];
		}
	}
	var childNodes = node.childNodes;
	if (childNodes) {
		length = childNodes.length;
		arr = obj.childNodes = new Array(length);
		for (i = 0; i < length; i++) {
			arr[i] = toJSON(childNodes[i]);
		}
	}
	return obj;
}

function toDOM(obj) {
	if (typeof obj === 'string') {
		obj = JSON.parse(obj);
	}
	var node, nodeType = obj.nodeType;
	switch (nodeType) {
		case 1: //ELEMENT_NODE
			node = document.createElement(obj.tagName);
			var attributes = obj.attributes || [];
			for (var i = 0, len = attributes.length; i < len; i++) {
				var attr = attributes[i];
				node.setAttribute(attr[0], attr[1]);
			}
			break;
		case 3: //TEXT_NODE
			node = document.createTextNode(obj.nodeValue);
			break;
		case 8: //COMMENT_NODE
			node = document.createComment(obj.nodeValue);
			break;
		case 9: //DOCUMENT_NODE
			node = document.implementation.createDocument();
			break;
		case 10: //DOCUMENT_TYPE_NODE
			node = document.implementation.createDocumentType(obj.nodeName);
			break;
		case 11: //DOCUMENT_FRAGMENT_NODE
			node = document.createDocumentFragment();
			break;
		default:
			return node;
	}
	if (nodeType === 1 || nodeType === 11) {
		var childNodes = obj.childNodes || [];
		for (i = 0, len = childNodes.length; i < len; i++) {
			node.appendChild(toDOM(childNodes[i]));
		}
	}
	return node;
}

function bin2string(array) {
	var result = "";
	for (var i = 0; i < array.length; ++i) {
		result += (String.fromCharCode(array[i]));
	}
	return result;
}

function SystemMessage(options) {

	if (typeof options.message !== 'string') {
		console.log('Wywołanie funckji Message z niepoprawnym parametrem message');
		return false;
	}

	let msgBox = $(`<div class="media">
        <div class="media-left media-middle text-center ${options.type}"></div>
        <div class="media-body">${options.message}</div>
    </div>`);

	function SystemMessage_Close(event) {
		if (typeof event !== 'undefined') {
			event.preventDefault();
		}
		msgBox.fadeOut(200, function () {
			$(this).remove();

			if (options.onclose) {
				options.onclose.call();
			}
		});
	}

	if (options.closable !== false) {
		let closeBtn = $('<a class="close"><span class="glyphicon glyphicon-remove-circle"></span></a>');
		closeBtn.click(function () { SystemMessage_Close() })
		msgBox.append(closeBtn);
	}

	if (typeof options.foot !== 'undefined') {
		msgBox.append(`<div class="footer">${options.foot}</div>`);
	}

	if (typeof options.autoclose === 'number') {
		setTimeout(function () {
			SystemMessage_Close();
		}, options.autoclose * 1000);
	}

	$('#popupContainer').append(msgBox);

	return msgBox;
}

/**
     *   OBSLUGA ORDER DLA GRID
  */

var sortOrder = {};

function sortObject() {
    const arr = [];
    for (let prop in sortOrder) {
        if (sortOrder.hasOwnProperty(prop)) {
            arr.push({
                'key': prop,
                'value': sortOrder[prop]
            });
        }
    }
    arr.sort(function (a, b) {
        return a.value.order - b.value.order;
    });

    sortOrder = {};
    for (let key in arr) {
        if (!arr.hasOwnProperty(key)) continue;

        sortOrder[arr[key].key] = arr[key].value;
    }
}

function refreshSortOrder() {
    //console.log('refreshSortOrder() caled');
    sortObject();
    $(".js-sorterOrder").val(JSON.stringify(sortOrder));
    $(".js-sorterCrumbs *").remove();
    let terms = $(".js-sorterCrumbs")[0];
    terms.depot._trell.Quered = true;
    terms.depot.Clear();
    terms.depot.List = new Array(sortOrder.length);
    for (let key in sortOrder) {
        if (!sortOrder.hasOwnProperty(key)) continue;
        let term = new SortTerm(key, sortOrder[key].direction.toUpperCase(), sortOrder[key].order);
        let element = $("<span/>",
            {
                'class': "btn btn-default btn-xs",
                'html': `<span class="js-orderDirection glyphicon glyphicon-chevron-${
                sortOrder[key]
                    .direction ===
                    "desc"
                    ? "down"
                    : "up"}" data-orderName="${key}" data-orderDirection="${sortOrder[key].direction}"></span> ${
                sortOrder[key]
                    .name} <a class="circle dark glyphicon glyphicon-remove js-orderRemove" data-orderName="${key
                }"></a>`
            })
            .data({
                "id": key,
                "order": sortOrder[key].order,
                "column": key,
                "direction": sortOrder[key].direction,
                "name": sortOrder[key].name
            });
               
        term.Bind = element[0];
        terms.depot.List[sortOrder[key].order] = term;
        terms.appendChild(term._bind.element);
    }

    if ($('.js-sorterCrumbs .btn').length > 0) {
        $(".js-sorterCrumbs")
            .sortable({
                stop: function (event, ui) {
                    $(".js-sorterCrumbs .btn")
                        .each(function (index) {
                            sortOrder[$(this).data("id")].order = index;
                        });
                    refreshSortOrder();
                },
                scroll: false
            });
        $('.js-sorter')
            .removeClass('hidden');
    }
}
