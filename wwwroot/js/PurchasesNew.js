// File: css/PurchasesNew.js
// Page: /purchase/new

// GLOBAL VARIABLES
var G_ITEMS = [];

// EVENTS

// on window.load => load user groups, load currencies, show current date, load all items into @item option list
$(window).on('load', function() {

    // load user groups
    // load all items
    // load all currencies
    // show current date

    LoadUserGroups()
    .then(LoadAllItems)
    .then(LoadCurrencies)
    .then(ShowCurrentDate)
    .then(function() {
        $('#item').selectpicker('refresh');
    });

});

$('#item').on('change', function() {

    PickItem();

});


// FUNCTIONS
var LoadUserGroups = function() {

    let request = $.ajax("/api/groups/" + $("#userId").val());
    
        request.done(function(groups) {
    
            // shortcut the groups control
            // clear the control
            // add 'all' option
            // and fill it with the groups retrieved
            
            let gc = $('#group');
    
            gc.html('');
    
            for (var i = 0; i < groups.length; i++) {
    
                // append every group as an option element
                
                gc.append($('<option>').attr('value', groups[i].groupID).html(groups[i].name));
    
            }
    
        });
    
        request.fail(function(data) {
    
            // make a console log with the data received
    
            console.log('Error while loading groups!\n' + data);
    
        });
    
        return request;

}

var LoadCurrencies = function() {

    let request = $.ajax('/api/currencies');

    request.done(function(allC) {

        // shortcut the currencies container
        // shortcut
        // empty the container just in case
        // append all the currencies loaded

        let cc = $('#currency');

        cc.html('');

        for (var i = 0; i < allC.length; i++) {

            cc.append($('<option>').attr('value', allC[i].currencyID).html(allC[i].name));

        }

    });

    request.fail(function(data) {

        console.log('Error loading currencies\n' + data);

    });

    return request;

}

var ShowCurrentDate = function() {

    let func = $.Deferred(function() {

        // get today as a Date object
        // get date, month and year for today
        // make a formatted date string
        // append that string to the 
        // date input field

        let today = new Date();
        let day = today.getDate();
        let month = today.getMonth() + 1;
        let year = today.getFullYear();

        let fDate = day + '/' + month + '/' + year;
        
        $('#date').val(fDate);

        this.resolve();

    });

    return func;

}

var LoadAllItems = function() {

    let request = $.ajax('/api/items');

    request.done(function(items) {

        // store all the items in a global array
        // for easier access store them
        // at array[index] => index = item.itemID
        // shortcut the item pick input
        // empty it just in case
        // append all the items
        // as options with item id as value

        let itemC = $('#item');

        itemC.html('');

        for (var i = 0; i < items.length; i++) {

            G_ITEMS[items[i].itemID] = items[i];

            let item = $('<span>');
            item.append(items[i].name);
            item.append(' ');
            item.append($('<span>').attr('class', 'badge badge-secondary').html(items[i].category.name));
            item.append(' ');
            item.append($('<span>').attr('class', 'badge badge-info').html(items[i].measure.name));

            itemC.append($('<option>').attr('value', items[i].itemID).html(item));

        }

    });

    request.fail(function(data) {

        console.log('Error loading items\n' + data);

    });

    return request;
}

var PickItem = function() {

    let func = $.Deferred(function() {
        
        // get item info from item input field
        // load the corrsponding fields with 
        // data for the picked item

        let itemP = $('#item').val(); // itemPicked
        let itemIdC = $('#itemId');
        let measC = $('#measure');
        let catC = $('#category');

        console.log('New item picked! id: ' + itemP + '; item name: ' + G_ITEMS[itemP].name);

        measC.html(G_ITEMS[itemP].measure.name);
        catC.html(G_ITEMS[itemP].category.name);

        this.resolve();
    });

    return func;

}

// #item on keyup => search items for input

// #item on item choose @UseItem => set item name, item id, item category and measure