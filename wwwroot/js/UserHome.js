// GLOBAL VARIABLES
var E_GROUP = null;
var G_CURRENCIES = [];

// EVENTS
$(document).on('click', '.group-edit', function(elem) {

    // get the associated group id
    // load the information about 
    // this particular group
    // pass the id to a function
    // to make the correct form
    // and put it in a modal

    var groupId = elem.target.getAttribute('group-id');

    LoadGroup(groupId)
    .then(LoadAllCurrencies)
    .then(MakeFormModal);

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

// FUNCTIONS
var LoadGroup = function(id) {

    var request = $.ajax('/api/groups/details/' + id);

    request.done(function(group) { E_GROUP = group; });

    request.fail(function(data) { console.log('Error loading group!\n', data); });

    return request;

}

var LoadAllCurrencies = function() {
    
    var request = $.ajax('/api/currencies');

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

var UpdateGroupDisplay = function() {

    var func = $.Deferred(function(){

        var id = E_GROUP.groupID;
        var name = E_GROUP.name;
        var curr = E_GROUP.preferredCurrency.fullName;

        var div =  $('div[group-id=' + id + ']');
        var divName = div.find('.card-header');
        var divCurr = div.find('.card-body .prefcurr');

        divName.html(name);
        divCurr.html(curr);

        OkMessage('Group updated successfully!');

        this.resolve();

    });

    return func;

}

var MakeFormModal = function() {

    var func = $.Deferred(function() {

        // get loaded group
        // if empty return error

        var group = E_GROUP;
        if (group == null) {
            
            ErrorMessage('Couldn\'t load the group!');
            return;

        }

        // check if currencies are loaded

        var allC = G_CURRENCIES;
        if (allC == null) {
            
            ErrorMessage('Couldn\'t load the currencies!');
            return;

        }

        // create form containers
        // form -> form-row -> columns

        var form = $('<form>').attr('method', 'patch').attr('action', '/api/groups').attr('id', 'edit-group');
        var formRow = $('<div>').attr('class', 'form-row');
        var nameCol = $('<div>').attr('class', 'col-12 mb-2');
        var currCol = $('<div>').attr('class', 'col-12 mb-2');
        
        // create id box for groupId

        var groupIdBox = $('<input>').attr('type', 'text').attr('hidden', 'true').attr('name', 'groupId').attr('id', 'groupId').val(group.groupID);

        // create name box for group.name
        // create currency select for preferred currency

        var nameBox = $('<input>').attr('type', 'text').attr('class', 'form-control').attr('name', 'name').attr('placeholder', 'Name').val(group.name);
        var currBox = $('<select>').attr('class', 'custom-select form-control').attr('name', 'prefcurrId').attr('id', 'prefcurr');

        // assemble the form elements

        currCol.append(currBox);
        nameCol.append(nameBox);
        formRow.append(nameCol);
        formRow.append(currCol);
        form.append(groupIdBox);
        form.append(formRow);

        // load currencies into the select
        for (var i = 0; i < allC.length; i++) {

            currBox.append($('<option>').attr('value', allC[i].currencyID).html(allC[i].name + ' (' + allC[i].fullName + ')'));

            if (allC[i].currencyID == group.prefCurrID) currBox.children().last().attr('selected', 'true');

        }

        // assemble an object with data
        // for the modal maker
        
        var modalData = {
            title: 'Edit group',
            body: form,
            useButton: $('<button>').attr('class', 'btn btn-success close-modal').html('Save changes').on('click', function() {
                var gID = $('#edit-group #groupId').val();
                var gNM = $('#edit-group input[name="name"]').val();
                var gPC = $('#edit-group select[name="prefcurrId"]').val();
                
                $.ajax({
                    url: '/api/groups',
                    type: 'PATCH',
                    data: {
                        groupId: gID,
                        name: gNM,
                        prefcurrId: gPC
                    },
                    success: function() { LoadGroup(gID).then(UpdateGroupDisplay); },
                    fail: function() { ErrorMessage('Couldn\'t update group!'); }
                });
            })
        }

        MakeModal(modalData);

    });
    
    return func;

}

var MakeModal = function(data) {

    var func = $.Deferred(function() {

        // create modal container elements
        // create modal content elements
        // put elements inside one another
        // append modal to the end of container
        // toggle modal

        var modalContainer = $('<div>').attr('class', 'modal fade').attr('aria-hidden', 'true').attr('role', 'dialog').attr('id', 'modal');
        var modalDocument = $('<div>').attr('class', 'modal-dialog').attr('role', 'document');
        var modalContent = $('<div>').attr('class', 'modal-content');
        var modalHeader = $('<div>').attr('class', 'modal-header');
        var modalBody = $('<div>').attr('class', 'modal-body');
        var modalFooter = $('<div>').attr('class', 'modal-footer');

        var modalTitle = $('<h5>').attr('class', 'modal-title').html(data.title);
        var modalText = $('<p>').html(data.body);
        var modalClose = $('<button>').attr('class', 'btn btn-secondary close-modal').attr('data-dismiss', 'modal').html('Close');
        var modalUse = data.useButton;
        
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

var ErrorMessage = function(message) {

    var func = $.Deferred(function() {

        if (title == null || title == "") title = "Error Message!";

        var msgCont = $('<div>').attr('class', 'alert alert-danger fixed-bottom w-50 text-center mx-auto alert-dismissible fade show').attr('role', 'alert');
        var msgBtn = $('<button>').attr('type', 'button').attr('class', 'close').attr('data-dismiss', 'alert').attr('aria-label', 'Close')
            .append($('<span>').attr('aria-hidden', 'true').html('&times;'));
        var msgTtl = '<b>' + title + '</b><br />';
        var msgTxt = message;

        msgCont.append(msgBtn).append(msgTtl).append(msgTxt);

        $('.container').append(msgCont);

        this.resolve();

    });

    return func;

}

var OkMessage = function(message, title) {

    var func = $.Deferred(function() {

        if (title == null || title == "") title = "Success!";
        
        var msgCont = $('<div>').attr('class', 'alert alert-success fixed-bottom w-50 text-center mx-auto alert-dismissible fade show').attr('role', 'alert');
        var msgBtn = $('<button>').attr('type', 'button').attr('class', 'close').attr('data-dismiss', 'alert').attr('aria-label', 'Close')
            .append($('<span>').attr('aria-hidden', 'true').html('&times;'));
        var msgTtl = '<b>' + title + '</b><br />';
        var msgTxt = message;

        msgCont.append(msgBtn).append(msgTtl).append(msgTxt);

        $('.container').append(msgCont);

        this.resolve();

    });

    return func;

}