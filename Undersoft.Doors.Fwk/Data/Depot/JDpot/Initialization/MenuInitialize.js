class MenuInitialize {
    
  Initialize() {
        $(".js-menuToggle").click(function MainMenuToggle() {
            $("body").toggleClass("smallSideMenu");
            setCookie("smallSideMenu", $("body").hasClass("smallSideMenu"));
        });

        $('#MainMenu >ul>li>a').click(function MainMenuButtonClick(event) {
            if ($(this).siblings('ul').length > 0) {
                event.preventDefault();
                let parent = $(this).closest('li');

                if (parent.hasClass('active')) {
                    parent.removeClass("active");
                    $(this).find(".text .glyphicon").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
                } else {
                    $(`#MainMenu >ul>li.active .text .glyphicon`).removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
                    $(`#MainMenu >ul>li.active`).removeClass("active");
                    parent.addClass("active");
                    $(this).find(".text .glyphicon").addClass("glyphicon-chevron-up").removeClass("glyphicon-chevron-down");
                }
            }
        })       
    }
}