function toggleDescription(el) {
    const link = el;
    const contentElement = link.previousElementSibling;
    const fullText = link.getAttribute('data-full-text');
    const shortText = link.getAttribute('data-short-text');

    if (link.innerText === 'Show more') {
        contentElement.innerText = fullText;
        link.innerText = 'Show less';
    } else {
        contentElement.innerText = shortText;
        link.innerText = 'Show more';
    }
}

document.querySelectorAll('.toggle-description').forEach(function (el) {
    el.addEventListener('click', function () {
        toggleDescription(this);
    });
});