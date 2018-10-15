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
    // $("#divSplash").show();
    // $("#divSplash").delay(2000).fadeOut(() => $("#divGame").show());
    $("#divGame").show();
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
    clearInterval(countdownTimer);
}

connection.on("NowDrinking", function (players, amount, type) {  
    clearInterval(countdownTimer);
    $('.modal-footer').hide();
    var drinking = players.map(p => p.name).join(", ");
    // var drinkHtml = "<h1>" + drinking + "</br>" + "drink " + amount + "</h1>";
    $("#drinkModalWho").text(drinking);
    if(amount === 1){
        type = type.slice(0, -1);
    }
    $('#drinkModalAmount').text(amount + " " + type);    
    $("#drinkModal").modal('show');
});

function notGuessed(){
    // do nothing
}

function initialize() {
    alert("test");
    $("#divSplash").show();
    $("#divSplash").fadeOut(3000, () => $("#divGame").show());
}