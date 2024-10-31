// Script to check for enough money
console.log('Check money script loaded!')
// const axios = require('axios/dist/browser/axios.cjs'); // browser
// The main form
let donateByMoneyForm = document.querySelector('#donateByMoneyForm');
// Modals
let confirmDonationModal = document.querySelector('#confirmDonationModal');
let needMoreMoneyModal = document.querySelector('#needMoreMoneyModal');
// Loading
let loading = `<span class="loading loading-spinner loading-sm"></span>`


// Add event listener to the buttons in the modal
confirmDonationModal.querySelector('#confirmDonationBtn').addEventListener('click', (e) => {
    donateByMoneyForm.submit();
})
needMoreMoneyModal.querySelector('#cfmBtn').addEventListener('click', (e) => {
    needMoreMoneyModal.querySelector('#confirmTopup').submit();
})
donateByMoneyForm.addEventListener('submit', (e) => {
    e.preventDefault();
    let userId = document.querySelector('#userId').value;
    let donationAmount = donateByMoneyForm.querySelector('input[name="Amount"]').value;
    // Query to server to know if the money is enough
    let donateByMoneyBtn = donateByMoneyForm.querySelector('#donateByMoneyBtn');
    let tempBtn = donateByMoneyBtn.innerHTML;
    donateByMoneyBtn.innerHTML = loading;
    axios.post('/Wallet/CheckIfWalletIsEnough', {
        UserId: userId,
        AmountToDonate: parseInt(donationAmount, 10),
    }).then(r => {
        console.log(r.data);
        let data = r.data;
        let info = {
            status : data.status,
            curBal : data.currentBalance,
            amountToDon : data.amountToDonate,
            userId : data.userId,
        }
        if (info.status === "ok") {
            // setup values
            confirmDonationModal.querySelector('.balance span').innerText = (info.curBal).toLocaleString();
            confirmDonationModal.querySelector('.after span').innerText = Number(info.curBal - info.amountToDon).toLocaleString();
            confirmDonationModal.querySelector('.donAmount span').innerText = info.amountToDon.toLocaleString();
            confirmDonationModal.querySelector('input[name=amount]').value = info.amountToDon; // The amount to donate
            confirmDonationModal.showModal();
        } else if (info.status === "not_enough_money") {
            // setup values
            needMoreMoneyModal.querySelector('.balance span').innerText = info.curBal.toLocaleString();
            needMoreMoneyModal.querySelector('.needed span').innerText = Number(info.amountToDon - info.curBal).toLocaleString();
            needMoreMoneyModal.querySelector('.donAmount span').innerText = info.amountToDon.toLocaleString();
            let topupAmount = Number(info.amountToDon - info.curBal);
            if (topupAmount < 10000) topupAmount = 10000;
            needMoreMoneyModal.querySelector('input[name=amount]').value = topupAmount; // The amount to top up
            let msgInput = document.querySelector('#donationMsg');
            needMoreMoneyModal.querySelector('input[name=Message]').value = msgInput.value;
            needMoreMoneyModal.querySelector('input[name=payAmount]').value = info.amountToDon // Get the amount of money user want to donate
            needMoreMoneyModal.showModal();
        }
    }).catch(e => {
        console.error(e);
        console.log(e.config.data);
    }).finally(() => {
        donateByMoneyBtn.innerHTML = tempBtn;
    })
    
    
})


