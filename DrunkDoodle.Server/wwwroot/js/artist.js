"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.start().catch(function (err) {
    return console.error(err.toString());
});

// PLAYING GAME
$("#btnStartRound").click(function () {
    startNewRound();
    event.preventDefault();
    $("#btnStartRound").hide();
    $("#roomId").hide();
});

function startNewRound() {
    connection.invoke("StartRound").catch(function (err) {
        return console.error(err.toString());
    });
}

function endRound() {
    connection.invoke("EndRound").catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("NewRound", function (word) {
    $("#artistCanvas").show();
    $("#word").show();
    $("#countdown").show();
    ProgressCountdown(5, 'countdown').then(() => endRound());
    $("#word").text(word);
});

connection.on("PrepareRound", function prepareRound(drawer) {
    alert("Time's up!\nNow drawing: " + drawer);
    $("#countdown").text(30);
    connection.invoke("ClearCanvas").catch(function (err) {
        return console.error(err.toString());
    });
    startNewRound();
});

connection.on("ClearCanvas", function () {
    clearCanvas();
});

function ProgressCountdown(timeleft, /*bar,*/ text) {
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

// CREATE ROOM
connection.on("roomCreated", function (roomId) {
    var labelRoom = $("#roomId");
    labelRoom.text("Room " + roomId);
    labelRoom.show();
    labelRoom.attr("href", "/Audience?roomId=" + roomId);  
    $("#btnStartRound").show();
    $("#btnCreateRoom").hide();
    $("#teams").hide();
});

$("#btnCreateRoom").click(function (event) {
    connection.invoke("CreateRoom", getTeams()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

function getTeams() {
    var inputs = $("#teams > .row > .col > input");
    var teams = [
        {
            'teamNo': 1,
            'members': [inputs[0].value, inputs[2].value]
        },
        {
            'teamNo': 2,
            'members': [inputs[4].value, inputs[6].value]
        },
        {
            'teamNo': 3,
            'members': [inputs[8].value, inputs[10].value]
        }];
    return teams;
}


// DRAWING
var clickX = new Array();
var clickY = new Array();
var clickDrag = new Array();
var paint;

function addClick(x, y, dragging) {
    clickX.push(x);
    clickY.push(y);
    clickDrag.push(dragging);
    connection.invoke("IsDrawing", x, y, dragging === undefined ? false : true).catch(function (err) {
        return console.error(err.toString());
    });
}

function clearCanvas() {
    context.clearRect(0, 0, context.canvas.width, context.canvas.height); // Clears the canvas
    clickX = new Array();
    clickY = new Array();
    clickDrag = new Array();
}

var context = document.getElementById('artistCanvas').getContext("2d");

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

$('#artistCanvas').mousedown(function (e) {
    var mouseX = e.pageX - this.offsetLeft;
    var mouseY = e.pageY - this.offsetTop;

    paint = true;
    addClick(e.pageX - this.offsetLeft, e.pageY - this.offsetTop);
    redraw();
});
$('#artistCanvas').mousemove(function (e) {
    if (paint) {
        addClick(e.pageX - this.offsetLeft, e.pageY - this.offsetTop, true);
        redraw();
    }
});
$('#artistCanvas').mouseup(function (e) {
    paint = false;
});
$('#artistCanvas').mouseleave(function (e) {
    paint = false;
});