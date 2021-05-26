class ExtensionInitialize {

    Initialize() {

        $("body").on("click", "[data-showpanel]", function (event) {
            event.preventDefault();
            const panel = $$($(this).data("showpanel")).first();

            if (!panel.data('hiddenClass') && panel.attr('class')) {
                panel.data('hiddenClass', panel.attr('class').match(/hidden(\S+)?/g));
            }

            if ($(this).data("position") === "below") {
                panel.css({ 'top': $(this).offset().top + $(this).outerHeight(), 'position': "fixed", 'left': $(this).offset().left });
            }
            if ($(this).data("position") === "above") {
                panel.css({ 'bottom': $(this).offset().top + $(this).outerHeight(), 'position': "fixed", 'left': $(this).offset().left });
            }

            if (panel.is(':visible')) {
                $(this).removeClass("active");

                if (panel.data('hiddenClass')) {
                    panel.addClass(panel.data('hiddenClass').join(' '));
                }
                panel.hide();
            } else {
                $(this).addClass("active");
                panel.removeClass(function (index, className) {
                    return (className.match(/hidden(\S+)?/g) || []).join(' ');
                }).show();
            }
        });

        $("body").on("click", ".js-toTop", function (event) {
            event.preventDefault();
            $("html,body").animate({ scrollTop: 0 }, "slow");
        });

        $("body").on("change", ".dd_chk_select .dd_chk_drop input[type=checkbox]", function () {
            dropDownUpdate();
        });

        $('body').on('click', '.btn-deleteFile', function deleteClicked(event) {
            event.preventDefault();
            if (debug) console.info("Delete button clicked");
            TextPopUp({
                title: "Potwierdzenie usunięcia",
                content: "Czy jesteś pewny, że chcesz usunąc wybrane pozycje?<br/> Operacja jest nieodwracalna!",
                button_action: $(this).attr('href').replace('javascript:', ''),
                button_text: "Tak, usuń pozycje"
            });
        });

        $("body")
            .on("change",
            ".js-sorterOrder",
            function () {
                sortOrder = JSON.parse($(this).val());
                refreshSortOrder();
            });
        $("body")
            .on("click",
            "[sort-id]",
            function (event) {
                event.preventDefault();
                const id = $(this).attr("sort-id");

                if (!sortOrder[id]) {
                    sortOrder[id] = {
                        "order": Object.keys(sortOrder).length,
                        "column": id,
                        "direction": "asc",
                        "name": $(this).text()
                    };

                    $('.js-sorter>.btn').addClass('btn-primary');
                }
                refreshSortOrder();
            });
        $("body")
            .on("click",
            ".js-orderDirection",
            function (event) {
                event.preventDefault();
                sortOrder[$(this).data("ordername")]
                    .direction = $(this).data("orderdirection") === "desc" ? "asc" : "desc";
                refreshSortOrder();
            });
        $("body")
            .on("click",
            ".js-orderRemove",
            function (event) {
                event.preventDefault();
                delete sortOrder[$(this).data("ordername")];
                $('.js-sorter>.btn').addClass('btn-primary');
                refreshSortOrder();
            });

        if ($(".js-sorterOrder").val())
            sortOrder = JSON.parse($(".js-sorterOrder").val());

        $("body").on("click", ".dropdown-menu", function () {
            event.stopPropagation();
            console.log('clicked')
            //$(this).closest(".dropdown-menu").prev().dropdown("toggle");
        });

        $('body').on('click', '.js-tabs >nav >a', function (e) {
            e.preventDefault();

            let nav = $(this).closest('nav');
            nav.find('.active').removeClass('active');
            $(this).addClass('active');

            let tabs = $(this).closest('.js-tabs');
            tabs.find(`>.tab:not(${$(this).attr('href')})`).hide();
            tabs.find($(this).attr('href')).show();
        })

        $(document).ready(function () {
            $('.js-tabs').each(function () {
                if ($(this).find('>nav .active').length === 0) {
                    $(this).find('>nav a:first-of-type').click()
                }
            })
        })

        $('body').on('click', '.js-checkChecked', function (e) {
            let form = $(this).closest('.form');
            if (form.find('[data-check=all] input[type=radio]').is(':checked')) {
                if (!FlashKitty || FlashKitty.grid.pages === undefined) {
                    e.preventDefault();
                    TextPopUp({ title: "Błąd akcji", content: "Wybrano akceję dla wszystkich elementów gdy żadne nie spełniają ustawień wyświetlania" });
                    return false;
                } else {
                    if ($$('ActionMessageString').val().length === 0)
                        $$('ActionMessageString').text("all");
                }
            } else if (form.find('[data-check=selected] input[type=radio]').is(':checked')) {
                if ($$('ActionMessageString').val().length === 0) {
                    e.preventDefault();
                    TextPopUp({ title: "Błąd akcji", content: "Nie wybrano żadnych pozycji" });
                    return false;
                }
            }
        })

        $(document).on('click', 'a[href^="#"].animate', function (event) {
            var location = window.location.href.split("#").splice(0, 1)[0];
            var href = $.attr(this, 'href').split("#").splice(0, 1)[0];

            if (location.indexOf(href) > -1 || /^#/.test($.attr(this, 'href')) === true) {
                event.preventDefault();
                var anchor = $.attr(this, 'href').split("#").pop();

                $('html, body').animate({
                    scrollTop: $("#" + anchor).offset().top
                }, 500);

                window.location.hash = $(this).attr('href');
            }
        });

        $('body').on("keypress", "[id$=_SearchPanel]", function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
                window.location = $$('searchButton').attr('href');
            }
        });

        $('body').on('click', '.js-generateClick', function generateClick(event) {
            event.preventDefault();
            window.location = $$('GenerateResult').attr('href');
        });

        paginationObject = $('#PagingPanel');

        // podlaczenie sledzenia klikniec na podstrone
        paginationObject.on('click', 'a.page', function (event) {
            event.preventDefault();
            if ($(this).data('page') !== event.delegateTarget.depot.page) {
                //let trell = event.delegateTarget.depot._trell;
                event.delegateTarget.depot._trell.SetPaging(parseInt($(this).data('page')),
                    event.delegateTarget.depot._trell.paging.PageSize,
                    event.delegateTarget.depot._trell.paging.CachedPages, true);
                let trells = new DataTrellises();
                trells.Add(event.delegateTarget.depot._trell);
                DepotSync(trells, "Basic", new PagingCallback(event.delegateTarget.depot._trell));
                //trells.Add(trell._childs["Product^Markets"]);
                //DepotSync(trells, "Basic", new PagingCallback(trell));
                //    $('html, body').animate({
                //        scrollTop: grid.table.offset().top - grid.table.children('thead').outerHeight()
                //    }, 500);
            }
        });
    }
}