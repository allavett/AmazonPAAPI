/**
 * Converts one currency to another using openexchangerates.org data
 *
 * @version 1.0
 * @author Allar Vendla <allarvendla@gmail.com>
 */

var currencyCodes = [];
var currencyRates;
var initialCurrency = "";
var appId;

$(document).ready(function () {
    appId = $('#app-id').val();
    // Get currency data
    $.getJSON('http://openexchangerates.org/api/latest.json?app_id=' + appId + '',
        function (data) {
            // Get currency codes
            for (var k in data.rates)
                currencyCodes.push(k);
            // Get currency rates
            currencyRates = data.rates;
            // Generate currency selection options
            selectCurrencyOptions();
        }
    );
});
function selectCurrencyOptions() {
    // Get initial currency code
    $("div.amount").each(function () {
        if (initialCurrency == "") {
            initialCurrency = $(this).children("input.currency").val();
        }
    });
    // Generate first currency options
    CurrencyOption(initialCurrency);
    // Generate other options
    $.each(currencyCodes, function (index, item) {
        if (item != initialCurrency) {
            CurrencyOption(item);
        }
    });
}

function CurrencyOption(value) {
    var option = document.createElement("option");
    option.text = value;
    option.value = value;
    var select = document.getElementById("currency");
    select.appendChild(option);
}
// Change price according to selected currency
function selectCurrency() {
    if (initialCurrency != "") {
        var newCurrency = $("#currency").val();
        var newRate = currencyRates[newCurrency];
        $("div.amount").each(function () {
            var initialPrice = Number($(this).children("input.original").val());
            var initialRate = currencyRates[initialCurrency];
            if ($.trim($(this).children("span").text()) != "N/A") {
                if (newCurrency != initialCurrency) {
                    var basePrice = (initialPrice / initialRate).toFixed(2);
                    var newPrice = (basePrice * newRate).toFixed(2)
                    $(this).children("span").html(newCurrency + " " + newPrice);
                } else {
                    $(this).children("span").html(initialCurrency + " " + initialPrice.toFixed(2));
                }
            }
        });
    }
}