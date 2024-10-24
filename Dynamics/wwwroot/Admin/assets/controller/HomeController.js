document.addEventListener("DOMContentLoaded", function () {
    // Hàm tính thời gian trôi qua
    function timeAgo(date) {
        const seconds = Math.floor((new Date() - new Date(date)) / 1000);
        if (seconds < 60) return seconds + " seconds ago";
        const minutes = Math.floor(seconds / 60);
        if (minutes < 60) return minutes + " minutes ago";
        const hours = Math.floor(minutes / 60);
        if (hours < 24) return hours + " hours ago";
        const days = Math.floor(hours / 24);
        if (days < 30) return days + " days ago";
        const months = Math.floor(days / 30);
        if (months < 12) return months + " months ago";
        const years = Math.floor(months / 12);
        return years + " years ago";
    }

    // Cập nhật thời gian liên tục mỗi giây
    function updateTimeAgo() {
        document.querySelectorAll('.time-ago').forEach(function (element) {
            const creationDate = element.getAttribute('data-creation-date');
            element.textContent = timeAgo(creationDate);
        });
    }

    // Cập nhật thời gian mỗi 1 giây
    setInterval(updateTimeAgo, 1000);
});