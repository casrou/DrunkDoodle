"use strict";

var clickX = new Array();
var clickY = new Array();
var clickDrag = new Array();

function addClick(x, y, dragging) {
    clickX.push(x);
    clickY.push(y);
    clickDrag.push(dragging);
}

$(function () {
    $("#divSplash").show();
    $("#divSplash").fadeOut(() => $("#divGame").show());
});

var context = document.getElementById('audienceCanvas').getContext("2d");

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.on("UpdateDrawing", function (x, y, dragging) {
    addClick(x, y, dragging);
    redraw();
});

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

function endRound() {
    // do nothing
}

function initialize() {
    alert("test");
    $("#divSplash").show();
    $("#divSplash").fadeOut(3000, () => $("#divGame").show());
}