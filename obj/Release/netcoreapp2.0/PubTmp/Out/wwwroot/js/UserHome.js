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

    let groupId = elem.target.getAttribute('group-id');

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

    let request = $.ajax('/api/groups/details/' + id);

    request.done(function(group) { E_GROUP = group; });

    request.fail(function(data) { console.log('Error loading group!\n', data); });

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

var UpdateGroupDisplay = function() {

    let func = $.Deferred(function(){

        let id = E_GROUP.groupID;
        let name = E_GROUP.name;
        let curr = E_GROUP.preferredCurrency.fullName;

        let div =  $('div[group-id=' + id + ']');
        let divName = div.find('.card-header');
        let divCurr = div.find('.card-body .prefcurr');

        divName.html(name);
        divCurr.html(curr);

        OkMessage('Group updated successfully!');

        this.resolve();

    });

    return func;

}

var MakeFormModal = function() {

    let func = $.Deferred(function() {

        // get loaded group
        // if empty return error

        let group = E_GROUP;
        if (group == null) {
            
            ErrorMessage('Couldn\'t load the group!');
            return;

        }

        // check if currencies are loaded

        let allC = G_CURRENCIES;
        if (allC == null) {
            
            ErrorMessage('Couldn\'t load the currencies!');
            return;

        }

        // create form containers
        // form -> form-row -> columns

        let form = $('<form>').attr('method', 'patch').attr('action', '/api/groups').attr('id', 'edit-group');
        let formRow = $('<div>').attr('class', 'form-row');
        let nameCol = $('<div>').attr('class', 'col-12 mb-2');
        let currCol = $('<div>').attr('class', 'col-12 mb-2');
        
        // create id box for groupId

        let groupIdBox = $('<input>').attr('type', 'text').attr('hidden', 'true').attr('name', 'groupId').attr('id', 'groupId').val(group.groupID);

        // create name box for group.name
        // create currency select for preferred currency

        let nameBox = $('<input>').attr('type', 'text').attr('class', 'form-control').attr('name', 'name').attr('placeholder', 'Name').val(group.name);
        let currBox = $('<select>').attr('class', 'custom-select form-control').attr('name', 'prefcurrId').attr('id', 'prefcurr');

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
        
        let modalData = {
            title: 'Edit group',
            body: form,
            useButton: $('<button>').attr('class', 'btn btn-success close-modal').html('Save changes').on('click', function() {
                let gID = $('#edit-group #groupId').val();
                let gNM = $('#edit-group input[name="name"]').val();
                let gPC = $('#edit-group select[name="prefcurrId"]').val();
                
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

var ErrorMessage = function(message, title = 'Error Message!') {

    let func = $.Deferred(function() {

        let msgCont = $('<div>').attr('class', 'alert alert-danger fixed-bottom w-50 text-center mx-auto alert-dismissible fade show').attr('role', 'alert');
        let msgBtn = $('<button>').attr('type', 'button').attr('class', 'close').attr('data-dismiss', 'alert').attr('aria-label', 'Close')
            .append($('<span>').attr('aria-hidden', 'true').html('&times;'));
        let msgTtl = '<b>' + title + '</b><br />';
        let msgTxt = message;

        msgCont.append(msgBtn).append(msgTtl).append(msgTxt);

        $('.container').append(msgCont);

        this.resolve();

    });

    return func;

}

var OkMessage = function(message, title = 'Success!') {

    let func = $.Deferred(function() {
        
        let msgCont = $('<div>').attr('class', 'alert alert-success fixed-bottom w-50 text-center mx-auto alert-dismissible fade show').attr('role', 'alert');
        let msgBtn = $('<button>').attr('type', 'button').attr('class', 'close').attr('data-dismiss', 'alert').attr('aria-label', 'Close')
            .append($('<span>').attr('aria-hidden', 'true').html('&times;'));
        let msgTtl = '<b>' + title + '</b><br />';
        let msgTxt = message;

        msgCont.append(msgBtn).append(msgTtl).append(msgTxt);

        $('.container').append(msgCont);

        this.resolve();

    });

    return func;

}

var InfoMessage = function(message, title = 'Info Message!') {
    
}