$(document).ready(function () {
    
    let currentFilterType = "All";

    $('#filterBy .dropdown-item').click(function () {
        currentFilterType = $(this).text().trim(); // Update the filter type
        $('#filterBy .dropdown-toggle').text(currentFilterType); // Update dropdown label
        filterAndSearch();
    });

    document.getElementById('searchBox').addEventListener('input', function() {
        filterAndSearch();
    });
    function filterAndSearch() {
        let searchQuery = document.getElementById('searchBox').value.toLowerCase();
        let transactionCards = document.querySelectorAll('[data-type]');
        let noResults = true;

        transactionCards.forEach(card => {
            let cardType = card.getAttribute('data-type').replace(/\s+/g, '').toLowerCase();
            let textContent = card.innerText.toLowerCase();

            // Check if the card matches the filter and search query
            if ((currentFilterType === "All" || cardType === currentFilterType.toLowerCase().replace(/\s+/g, '')) &&
                textContent.includes(searchQuery)) {
                card.style.display = '';
                noResults = false;
            } else {
                card.style.display = 'none';
            }
        });
        document.getElementById('noResults').style.display = noResults ? '' : 'none';
    }
});