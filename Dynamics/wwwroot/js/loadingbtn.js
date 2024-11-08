document.addEventListener("DOMContentLoaded", function () {
    let loadingBtns = document.getElementsByClassName("loading-btn");
    let tempBtnContents = [];
    for (let btn of loadingBtns) {
        console.log(btn);
        btn.addEventListener("click", function () {
            tempBtnContents.push(btn.innerHTML);
            btn.innerHTML = `<span class="loading loading-spinner loading-md"></span>`
        })
    }
    let labels = document.getElementsByName("label");
    for (let label of labels) {
        label.addEventListener("change", function () {
            for (let btn of loadingBtns) {
                btn.innerHTML = tempBtnContents.;
            }
        })
    }
})