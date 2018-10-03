"use strict";

var clickX = new Array();
var clickY = new Array();
var clickDrag = new Array();

function addClick(x, y, dragging) {
    clickX.push(x);
    clickY.push(y);
    clickDrag.push(dragging);
}

var context = document.getElementById('audienceCanvas').getContext("2d");

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

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.on("UpdateDrawing", function (x, y, dragging) {
    addClick(x, y, dragging);
    redraw();
});

connection.on("ClearCanvas", function () {
    clearCanvas();
});

function clearCanvas() {
    context.clearRect(0, 0, context.canvas.width, context.canvas.height); // Clears the canvas
    clickX = new Array();
    clickY = new Array();
    clickDrag = new Array();    
}

connection.start()
    .catch(function (err) {
        return console.error(err.toString());
    })
    .then(function () {
        var queries = {};
        $.each(document.location.search.substr(1).split('&'), function (c, q) {
            var i = q.split('=');
            queries[i[0].toString()] = i[1].toString();
        });
        connection.invoke("JoinAudience", queries["roomId"]).catch(function (err) {
            return console.error(err.toString());
        });
    });