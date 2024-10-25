function ValidateSearchForm(searchFormId, DateFromName, DateToName, QueryName, FilterName, validationSpanId) {
    let searchForm = document.getElementById('searchForm');
    if (searchForm == null) alert('Cannot find search form');
    // Setting the dates dynamically
    const dateFrom = searchForm.querySelector('input[name="DateFrom"]');
    const dateTo = searchForm.querySelector('input[name="DateTo"]');

    dateFrom.addEventListener('change', (e) => {
        let dateFromValue = dateFrom.value;
        if (dateFromValue) {
            dateTo.min = dateFromValue;
        } else {
            dateTo.removeAttribute('min'); // If date from is cleared, cleared the date to as well
        }
    })
    dateTo.addEventListener('change', function() {
        const dateToValue = dateTo.value;
        if (dateToValue) {
            dateFrom.max = dateToValue;
        } else {
            dateFrom.removeAttribute('max');
        }
    });

    function validateSearchForm() {

        console.log(dateFrom);
        console.log(dateTo);
        if (dateFrom.value == null || dateTo.value == null) {
            return "Please select a value for date from / date to"
        }

        // Get the filter select element
        const filter = searchForm.querySelector('select[name="Filter"]');
        console.log(filter)
        // Check for query
        const query = searchForm.querySelector('input[name="Query"]');
        console.log(query)
        if (query.value === "") return "Please enter query to search"

        return true; // Allow form submission
    }

    searchForm.addEventListener('submit', (e) => {
        let msg = validateSearchForm();
        console.log(msg);
        if (msg !== true) {
            let validationSpan = document.querySelector('#searchValidation');
            validationSpan.innerHTML = msg
            e.preventDefault();
        }
    })
}

export default ValidateSearchForm;