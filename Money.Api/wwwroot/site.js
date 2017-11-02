'use strict';
moment.lang('ru');

accounting.settings = Object.assign({},
    accounting.settings,
    {
        currency: {
            symbol: "₽", // default currency symbol is '$'
            format: "%v %s", // controls output: %s = symbol, %v = value/number (can be object: see below)
            decimal: ".", // decimal point separator
            thousand: " ", // thousands separator
            precision: 2 // decimal places
        }
    });

var getCategory = function (category) {
    switch (category) {
        case 1:
            return "Необходимые";
        case 2:
            return "Здоровье";
        case 3:
            return "Счета";
        case 4:
            return "Запланированное";
        case 5:
            return "Зарплата";
        case 6:
            return "Подарок";
        default:
            return "Не указана";
    }
}

var load = function (from, to) {
    var url = '/points';
    var params = [];
    if (from !== "") {
        params.push("from=" + from);
    }
    if (to !== "") {
        params.push("to=" + to);
    }
    if (params.length > 0) {
        url += "?" + params.join("&");
    }

    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (data) {
            var table = document.getElementById("dataTable");
            for (var i = 0; i < data.length; i++) {
                var tr = table.insertRow();
                var dateTd = tr.insertCell();
                var incomeTd = tr.insertCell();
                var spendingTd = tr.insertCell();
                var summaryTd = tr.insertCell();

                tr.bgColor = "gray";
                summaryTd.innerText = accounting.formatMoney(data[i].Balance);
                dateTd.innerText = moment(data[i].Date).format("D MMM YYYY");
                incomeTd.innerText = accounting.formatMoney(data[i].Details.Incomings);
                spendingTd.innerText = accounting.formatMoney(data[i].Details.Spendings);
                for (var y = 0; y < data[i].Details.Actions.length; y++) {
                    var actionTr = table.insertRow();
                    actionTr.insertCell();
                    var descTd = actionTr.insertCell();
                    var valueTd = actionTr.insertCell();
                    descTd.innerText = data[i].Details.Actions[y].Description + " (" +
                        getCategory(data[i].Details.Actions[y].Category) + ")";
                    valueTd.innerText = accounting.formatMoney(data[i].Details.Actions[y].Value);
                }

            }
            console.log(data);
        })
        .catch(alert);
}

document.addEventListener('DOMContentLoaded', function () {
    var loadButton = document.getElementById("load");
    var fromInput = document.getElementById("from");
    fromInput.valueAsDate = new Date();
    var toInput = document.getElementById("to");
    loadButton.onclick = function () {
        load(fromInput.value, toInput.value);
    };
});
