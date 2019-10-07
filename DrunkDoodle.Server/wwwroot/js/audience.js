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

connection.on("EndRound", function (word) {
    $("#audienceWord").text(word);
    $("#audienceWord").show();
});

connection.start()
    .catch(function (err) {
        return console.error(err.toString());
    })
    .then(function () {
        //var queries = {};
        //$.each(document.location.search.substr(1).split('&'), function (c, q) {
        //    var i = q.split('=');
        //    queries[i[0].toString()] = i[1].toString();
        //});
        var roomId = document.location.pathname.split('/')[1];
        connection.invoke("JoinAudience", roomId).catch(function (err) {
            return console.error(err.toString());
        });
        //connection.invoke("JoinAudience", queries["room"]).catch(function (err) {
        //    return console.error(err.toString());
        //});
    });

function endRound() {
    clearInterval(countdownTimer);
}

connection.on("NowDrinking", function (players, amount, type) {  
    clearInterval(countdownTimer);
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

$("#btnScoreboard").click(function () {
    connection.invoke("ShowScoreboard").catch(function (err) {
        return console.error(err.toString());
    });
});


connection.on("ShowScoreboard", function (teams) {
    $("#scoreModalTable > tbody > tr").remove();
    // teams sorted by score (highest first)
    for (var i = 0; i < teams.length; i++) {
        var tr = document.createElement("tr");
        var placement = document.createElement("th");
        var players = document.createElement("td");
        var score = document.createElement("td");
        
        placement.textContent = i + 1;
        players.textContent = teams[i].players.map(p => p.name).join(', ');
        score.textContent = teams[i].teamScore;

        tr.appendChild(placement);
        tr.appendChild(players);
        tr.appendChild(score);

        console.log(tr);
        $("#scoreModalTable > tbody").append(tr);
    }    

    $("#scoreModal").modal('show');
});