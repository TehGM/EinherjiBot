function registerDiscordSpoilers() {
    document.querySelectorAll(".discord-spoiler").forEach(function (s) {
        s.addEventListener('click', function () {
            this.classList.remove("hidden");
        });
    });
}