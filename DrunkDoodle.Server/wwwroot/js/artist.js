"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.start().catch(function (err) {
    return console.error(err.toString());
});

$(function () {
    // $("#divSplash").show();
    // $("#divSplash").delay(2000).fadeOut(() => $("#divCreateRoom").show());
    $("#divCreateRoom").show();
});

// PLAYING GAME
$("#btnStartRound").click(function () {
    endRound();
    $("#divPrepareGame").hide();
    $("#divGame").show();
});

function startNewRound() {
    canDraw = true;
    $("#btnGuessed").show();
    connection.invoke("StartRound").catch(function (err) {
        return console.error(err.toString());
    });
}

function endRound() {
    clearInterval(countdownTimer);
    canDraw = false;
    $("#btnGuessed").hide();      
    connection.invoke("EndRound").catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("PrepareRound", function (drawer) {    
    $("#divNowDrawing").show();
    $("#spanArtist").text(drawer);    
});

$("#btnReady").click(function () {
    $("#divNowDrawing").hide();
    connection.invoke("ClearCanvas").catch(function (err) {
        return console.error(err.toString());
    });
    startNewRound();
});

$("#btnGuessed").click(function(){
    connection.invoke("WordGuessed").catch(function (err) {
        return console.error(err.toString());
    });
    endRound();
});

function notGuessed(){
    connection.invoke("WordNotGuessed").catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("NowDrinking", function (players, amount, type) {  
    var drinking = players.map(p => p.name).join(", ");
    // var drinkHtml = "<h1>" + drinking + "</br>" + "drink " + amount + "</h1>";
    $("#drinkModalWho").text(drinking);
    if(amount === 1){
        type = type.slice(0, -1);
    }
    $('#drinkModalAmount').text(amount + " " + type);
    $("#drinkModal").modal('show');
});

// CREATE ROOM

connection.on("roomCreated", function (roomId) {
    var labelRoom = $("#roomId");
    labelRoom.text("Room " + roomId);
    labelRoom.attr("href", "/Audience?roomId=" + roomId);  
    $("#divPrepareGame").show();
    $("#divCreateRoom").hide();
});

$("#btnCreateRoom").click(function (event) {
    var players = getPlayers();
    var drinkAmount = $("#penaltyAmount").val();
    var drinkType = $("#penaltyType").val();
    var wordLanguage = $("#wordLanguage").val();
    console.log(drinkAmount + drinkType);
    if (players.length > 0) {
        connection.invoke("CreateRoom", players, drinkAmount, drinkType, wordLanguage).catch(function (err) {
            return console.error(err.toString());
        });
    } else {
        $("#alertNoPlayers").show();
    }    
    event.preventDefault();
});

$("#btnAddPlayerRow").click(function () {
    $("#teams").append(
        `<div class="form-row">
            <div class="col">
                <input type="text" class="form-control" placeholder="">
            </div>
                <div class="col">
                    <input type="text" class="form-control" placeholder="">
            </div>
        </div>`
    );
});

function getPlayers() {
    var inputs = $("#teams > .form-row");
    var players = [];
    for (var i = 0; i < inputs.length; i++) {
       var player = {};
       player.name = inputs[i].firstElementChild.lastElementChild.value;
       player.team = inputs[i].lastElementChild.lastElementChild.value;
       if (player.name && player.team) //https://stackoverflow.com/a/5515349
           players.push(player);
    }
    // players.push({ 'name': 'Casper', 'team': '1' });
    // players.push({ 'name': 'Ida', 'team': '1' });
    // players.push({ 'name': 'Anna', 'team': '2' });
    // players.push({ 'name': 'Marcus', 'team': '2' });
    // players.push({ 'name': 'Jeppe', 'team': '3' });
    // players.push({ 'name': 'Peter', 'team': '3' });
    //console.log(players);
    return players;
}

// DRAWING
var clickX = new Array();
var clickY = new Array();
var clickDrag = new Array();
var paint;
var canDraw = false;

function addClick(x, y, dragging) {
    if (canDraw) {
        clickX.push(x);
        clickY.push(y);
        clickDrag.push(dragging);
        connection.invoke("IsDrawing", x, y, dragging === undefined ? false : true).catch(function (err) {
            return console.error(err.toString());
        });
    }    
}

var canvas = document.getElementById('artistCanvas');
var context = canvas.getContext("2d");

canvas.addEventListener("mousedown", startDraw, false);
canvas.addEventListener("mouseup", cancelDraw, false);
canvas.addEventListener("mouseleave", cancelDraw, false);
canvas.addEventListener("mousemove", moveDraw, false);

canvas.addEventListener("touchstart", startDraw, false);
canvas.addEventListener("touchend", cancelDraw, false);
canvas.addEventListener("touchcancel", cancelDraw, false);
canvas.addEventListener("touchmove", moveDraw, false);

function startDraw(e) {
    paint = true;
    var coordinate = getCoordinates(e);
    addClick(coordinate.x, coordinate.y);
    redraw();
    e.preventDefault(); 
}

function moveDraw(e) {
    if (paint) {
        var coordinate = getCoordinates(e);
        addClick(coordinate.x, coordinate.y, true);
        redraw();
    }
    e.preventDefault();
}

function getCoordinates(e) {
    var coordinate = {};
    if (e.changedTouches) {
        coordinate.x = e.changedTouches[0].clientX - canvas.offsetLeft;
        coordinate.y = e.changedTouches[0].clientY - canvas.offsetTop;        
    } else {
        coordinate.x = e.clientX - canvas.offsetLeft;
        coordinate.y = e.clientY - canvas.offsetTop;
    }
    return coordinate;
}

function cancelDraw(e) {
    paint = false;
    e.preventDefault();
}