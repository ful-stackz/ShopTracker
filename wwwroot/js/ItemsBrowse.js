// GLOBAL VARIABLES
var G_CURRENCIES = [];
var G_GROUPS = [];
var G_ITEMS = [];
var V_ITEMS = [];
var P_ITEM;

// EVENTS
$(window).on('load', function() {

    // load all items
    // show all items

    LoadAllItems()
    .then(MakeAllVisible)
    .then(ShowItems);

});

$('#searchBar').on('keyup', function(sb) {
    
    // get searchBar value
    // if length is 0 then show all items
    // if length is < 3 then do nothing
    // if length is >= 3 then search through items

    let search = sb.target.value;

    if (search.length == 0) {
        
        MakeAllVisible().then(ShowItems);
        return;

    } else if (search.length < 3) return;

    SearchItems(search).then(ShowItems);

});

$(document).on('click', '.btn-modal', function(elem) {
    
    // get the associated item with the link clicked
    // pass the id as parameter to the function
    // that will load the info about the item
    // and then will call a function to show it
    // also if the global currencies array is empty
    // load all the currencies
    // also if the global groups array is empty
    // load all the user groups
    
    let itemId = elem.target.getAttribute('target-item');
    
    if (G_CURRENCIES.length == 0) LoadAllCurrencies();

    if (G_GROUPS.length == 0) LoadAllGroups();

    LoadItemInfo(itemId);

});
    
$(document).on('click', '.close-modal', function() {

    // get the modal from the .container
    // remove modal

    $('#modal').modal('hide');

});

$(document, '#modal').on('hidden.bs.modal', function() {
    $('.container #modal').first().remove();
    $('.container #modal').modal('toggle');
});

$(document).on('change loadend', '#groupId', function() {

    let newG = $('#groupId').val();

    for (var i = 0; i < G_GROUPS.length; i++) {

        if (G_GROUPS[i].groupID == newG) $('#currency').val(G_GROUPS[i].prefCurrID);

    }
    
});


// FUNCTIONS
var LoadAllItems = function() {

    let request = $.ajax('/api/items');

    request.done(function(items) {

        // store all the items in a global array
        // for easier search access

        G_ITEMS = items;

    });

    request.fail(function(data) {

        console.log('Error loading items!\n', data);

    });

    return request;
}

var LoadAllGroups = function() {

    let request = $.ajax('/api/groups/' + $('#userId').val());

    request.done(function(groups) {

        G_GROUPS = groups;

    });

    request.fail(function(data) {

        console.log('Error loading groups!\n', data);

    });

    return request;

}

var LoadAllCurrencies = function() {
    
    let request = $.ajax('/api/currencies');

    request.done(function(allC) {

        // store all currencies in a global array
        // for easier access
        // shortcut the currencies container
        // get loaded currency for comparison
        // append every other currency as new <option>

        G_CURRENCIES = allC;

    });

    request.fail(function(data) {

        console.log('Error loading currencies\n', data);

    });

    return request;

}

var MakeAllVisible = function() {

    let func = $.Deferred(function() {

        V_ITEMS = G_ITEMS.slice();

        this.resolve();

    });

    return func;
}

var SearchItems = function(search) {

    let func = $.Deferred(function() {

        // shortcut all the items
        // empty the visible items
        // search through them
        // the results that match
        // append to the visible items

        V_ITEMS = [];

        let allI = G_ITEMS;
        let visI = V_ITEMS;

        for (var i = 0; i < allI.length; i++) {

            if (allI[i].name.toLowerCase().includes(search)) visI.push(allI[i]);
        }

        this.resolve();

    });

    return func;
}

var ShowItems = function() {

    let func = $.Deferred(function() {

        // shortcut the visible items
        // shortcut the tbody of the items table
        // empty it and fill it with
        // all the visible items

        let items = V_ITEMS;

        let tBody = $('#itemsTable > tbody');
        
        tBody.html('');

        for (var i = 0; i < items.length; i++) {

            tBody.append($('<tr>')
                .append($('<td>').html(items[i].name))
                .append($('<td>').html(items[i].description))
                .append($('<td>').html(items[i].category.name))
                .append($('<td>').html(items[i].measure.name))
                .append($('<td>')
                    .append($('<button>')
                        .attr('class', 'btn btn-primary btn-modal')
                        .attr('target-item', items[i].itemID)
                        .html('Details'))
            ));

        }

        this.resolve();

    });

    return func;

}

var LoadItemInfo = function(id) {
    
    console.log('LoadItemInfo', id);

    let request = $.ajax('/api/items/' + id);

    request.done(MakeInfoModal);

    request.fail(function(data) {

        console.log('Error loading the item!\n', data);

    });

    return request;
}

var MakeInfoModal = function(itemData) {

    let func = $.Deferred(function() {

        P_ITEM = itemData;

        let modalData = {

            title: '',
            body: '',
            useButton: ''

        }

        modalData.title = $('<span>').html(itemData.name).append(' ')
                            .append($('<span>').attr('class', 'badge badge-secondary').html(itemData.category.name)).append(' ')
                            .append($('<span>').attr('class', 'badge badge-info').html(itemData.measure.name));

        modalData.body = 'This item has been bought ' + itemData.bought + ' times.';

        modalData.useButton = $('<button>').attr('class', 'btn btn-primary close-modal').on('click', MakeFormModal).html('Add purchase');

        MakeModal(modalData);

        this.resolve();

    });

    return func;
}

var MakeFormModal = function() {

    let func = $.Deferred(function() {

        // shortcut the globaly loaded item
        // shortcut he global array with currencies

        let itemData = P_ITEM;
        let allC = G_CURRENCIES;
        let allG = G_GROUPS;

        // create form containers
        // form -> form row -> 3 columns x 12

        let form = $('<form>').attr('method', 'post').attr('action', '/api/purchases/new').attr('id', 'quick-purchase');
        let formRow = $('<div>').attr('class', 'form-row');
        let nameCol = $('<div>').attr('class', 'col-12 mb-2');
        let quantCol = $('<div>').attr('class', 'col-12 mb-2');
        let priceCol = $('<div>').attr('class', 'col-12 mb-2');
        let dateCol = $('<div>').attr('class', 'col-12 mb-2');
        let groupCol = $('<div>').attr('class', 'col-12 mb-2');

        // create id boxes for
        // userid and itemid

        let userIdBox = $('<input>').attr('type', 'text').attr('hidden', 'true').attr('name', 'userId').val($('#userId').val());
        let itemIdBox = $('<input>').attr('type', 'text').attr('hidden', 'true').attr('name', 'itemId').val(itemData.itemID);

        // create static text for item name; category and measure as badges

        let nameBox = $('<span>').attr('class', 'form-control').attr('name', 'name');
        let itemCat = $('<span>').attr('class', 'badge badge-secondary').html(itemData.category.name);
        let itemMsr = $('<span>').attr('class', 'badge badge-info').html(itemData.measure.name);
        nameBox.append(itemData.name + ' ').append(itemCat).append(' ').append(itemMsr);

        // create text field for the quantity

        let quantBox = $('<input>').attr('type', 'text').attr('class', 'form-control').attr('name', 'quantity').attr('autofocus', 'autofocus').attr('placeholder', 'Quantity (2/2,5/10,00)');
        
        // create input group for price and currency
        // create input field for price
        // create select input for currency
        // assemble into price group

        let priceGroup = $('<div>').attr('class', 'input-group');
        let priceBox = $('<input>').attr('type', 'text').attr('class', 'form-control').attr('name', 'price').attr('placeholder', 'Price');
        let currAddon = $('<span>').attr('class', 'input-group-addon p-0');
        let currSelect = $('<select>').attr('class', 'custom-select').attr('name', 'currency').attr('id', 'currency');
        currAddon.append(currSelect);
        priceGroup.append(priceBox).append(currAddon);

        // create input field for date
        // and pre-load today as dd/mm/yyyy

        let todayD = new Date();
        let todayS = todayD.getDate() + '/' + (todayD.getMonth() + 1) + '/' + todayD.getFullYear();

        let dateBox = $('<input>').attr('type', 'text').attr('name', 'date').attr('class', 'form-control').val(todayS).attr('placeholder', 'Day/Month/Year');

        // create select group for user group

        let groupSelect = $('<select>').attr('class', 'custom-select form-control').attr('id', 'groupId').attr('name', 'groupId');

        // assemble the form elements

        form.append(userIdBox);
        form.append(itemIdBox);
        nameCol.append(nameBox);
        quantCol.append(quantBox);
        priceCol.append(priceGroup);
        dateCol.append(dateBox);
        groupCol.append(groupSelect);

        form.append(formRow.append(nameCol).append(quantCol).append(priceCol).append(dateCol).append(groupCol));

        // load user groups into the group select

        for (var i = 0; i < allG.length; i++) {
            groupSelect.append($('<option>').attr('value', allG[i].groupID).html(allG[i].name));
        }

        // load currencies into the currency select
        
        for (var i = 0; i < allC.length; i++) {

            currSelect.append($('<option>').attr('value', allC[i].currencyID).html(allC[i].name));

            for (var j = 0; j < allG.length; j++) {

                let gid = groupSelect.val();

                if (allG[j].groupID == gid && allC[i].currencyID == allG[j].prefCurrID) currSelect.children().last().attr('selected', 'true');
        
            }
        }

        // assemble an object with data
        // for the modal maker
        
        let modalData = {
            title: 'New purchase',
            body: form,
            useButton: $('<button>').attr('class', 'btn btn-success close-modal').html('Add purchase').on('click', function() { $('#quick-purchase').submit(); })
        }

        // make a new modal

        MakeModal(modalData)

        this.resolve();

    });

    return func;
}

var MakeModal = function(data) {
    
    let func = $.Deferred(function() {

        // create modal container elements
        // create modal content elements
        // put elements inside one another
        // append modal to the end of container
        // toggle modal

        let modalContainer = $('<div>').attr('class', 'modal fade').attr('aria-hidden', 'true').attr('role', 'dialog').attr('id', 'modal');
        let modalDocument = $('<div>').attr('class', 'modal-dialog').attr('role', 'document');
        let modalContent = $('<div>').attr('class', 'modal-content');
        let modalHeader = $('<div>').attr('class', 'modal-header');
        let modalBody = $('<div>').attr('class', 'modal-body');
        let modalFooter = $('<div>').attr('class', 'modal-footer');

        let modalTitle = $('<h5>').attr('class', 'modal-title').html(data.title);
        let modalText = $('<p>').html(data.body);
        let modalClose = $('<button>').attr('class', 'btn btn-secondary close-modal').attr('data-dismiss', 'modal').html('Close');
        let modalUse = data.useButton;
        
        modalHeader.append(modalTitle);
        modalBody.append(modalText);
        modalFooter.append(modalClose).append(modalUse);

        modalContent.append(modalHeader).append(modalBody).append(modalFooter);
        modalDocument.append(modalContent);
        modalContainer.append(modalDocument);

        $('.container').append(modalContainer);

        $('#modal').last().modal('toggle');

        this.resolve();

    });

    return func;

}