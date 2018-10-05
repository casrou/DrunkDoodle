"use strict";

const COUNTDOWN_TIME = 10;

connection.on("NewRound", function (word) {
    $("#artistCanvas").show();
    $("#word").show();
    $("#countdown").show();    
    ProgressCountdown(COUNTDOWN_TIME, 'countdown').then(() => endRound());
    $("#word").text(word);
});

connection.on("ClearCanvas", function () {
    clearCanvas();
});

function ProgressCountdown(timeleft, text) {
    return new Promise((resolve, reject) => {
        var countdownTimer = setInterval(() => {
            timeleft--;

            document.getElementById(text).textContent = timeleft;

            if (timeleft <= 0) {
                clearInterval(countdownTimer);
                resolve(true);
            }
        }, 1000);
    });
}

function clearCanvas() {
    $("#countdown").text(COUNTDOWN_TIME);
    context.clearRect(0, 0, context.canvas.width, context.canvas.height); // Clears the canvas
    clickX = new Array();
    clickY = new Array();
    clickDrag = new Array();
}

function redraw() {
    context.clearRect(0, 0, context.canvas.width, context.canvas.height); // Clears the canvas

    context.strokeStyle = "#df4b26";
    context.lineJoin = "round";
    context.lineWidth = 5;

    for (var i = 0; i < clickX.length; i++) {
        context.beginPath();
        if (clickDrag[i] && i) {
            context.moveTo(clickX[i - 1], clickY[i - 1]);
        } else {
            context.moveTo(clickX[i] - 1, clickY[i]);
        }
        context.lineTo(clickX[i], clickY[i]);
        context.closePath();
        context.stroke();
    }
}
