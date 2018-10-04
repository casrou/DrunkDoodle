"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/gameHub").build();

connection.start().catch(function (err) {
    return console.error(err.toString());
});

// PLAYING GAME
$("#btnStartRound").click(function () {
    endRound();
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
    connection.invoke("CreateRoom", getPlayers()).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

function getPlayers() {
    var inputs = $("#teams > .row");
    var players = [];
    //for (var i = 0; i < inputs.length; i++) {
    //    var player = {};
    //    player.name = inputs[i].firstElementChild.firstElementChild.value;
    //    player.team = inputs[i].lastElementChild.firstElementChild.value;
    //    if (player.name !== "" && player.team !== "")
    //        players.push(player);
    //}
    players.push({ 'name': 'casper', 'team': '1' });
    players.push({ 'name': 'ida', 'team': '1' });
    players.push({ 'name': 'anna', 'team': '2' });
    players.push({ 'name': 'marcus', 'team': '2' });
    players.push({ 'name': 'jeppe', 'team': '3' });
    players.push({ 'name': 'peter', 'team': '3' });
    return players;
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

var context = document.getElementById('artistCanvas').getContext("2d");

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