function ToggleNext(event) {
    
        event.preventDefault();
        let element = event.target;
        let sybling = element.nextElementSibling;
        if (sybling.classList.contains('visible')) {
            sybling.classList.remove('active');
            sybling.classList.remove('visible');
            sybling.classList.add('hidden');
        } else if (sybling.classList.contains('hidden')) {
            sybling.classList.add('active');
            sybling.classList.remove('hidden');
            sybling.classList.add('visible');
        }    
}

function TogglePrevious(event) {

    event.preventDefault();
    let element = event.target;
    let sybling = element.previousElementSibling;
    if (sybling.classList.contains('visible')) {
        sybling.classList.remove('active');
        sybling.classList.remove('visible');
        sybling.classList.add('hidden');
    } else if (sybling.classList.contains('hidden')) {
        sybling.classList.add('active');
        sybling.classList.remove('hidden');
        sybling.classList.add('visible');
    }
}