/*!
 * ItemDetails.js
 * Page: /items/browse
 * Action: When the button 'details' of an item is clicked display a modal with information about this item
!*/

// EVENTS
$(document).on('click', '.btn-modal', function(elem) {

    // get the associated item with the link clicked
    // pass the id as parameter to the function
    // that will load the info about the item
    // and then will call a function to show it
    
    let itemId = elem.target.getAttribute('target-item');
    console.log('ClickEvent', itemId);
    LoadItemInfo(itemId);

});

$(document).on('click', '.close-modal', function(elem) {

    // get the modal from the .container
    // remove modal

    $('#modal').hide();

});

$(document, '#modal').on('hidden.bs.modal', function() {
    $('.container #modal').remove();
});

// FUNCTIONS
var LoadItemInfo = function(id) {

    console.log('LoadItemInfo', id);

    let request = $.ajax('/api/items/' + id);

    request.done(MakeModal);

    request.fail(function(data) {

        console.log('Error loading the item!\n', data);

    });

    return request;
}

var MakeModal = function(itemData) {
    console.log('MakeModal', itemData.itemID);
    let func = $.Deferred(function() {

        let modalContainer = $('<div>').attr('class', 'modal fade').attr('aria-hidden', 'true').attr('role', 'dialog').attr('id', 'modal');
        let modalDocument = $('<div>').attr('class', 'modal-dialog').attr('role', 'document');
        let modalContent = $('<div>').attr('class', 'modal-content');
        let modalHeader = $('<div>').attr('class', 'modal-header');
        let modalBody = $('<div>').attr('class', 'modal-body');
        let modalFooter = $('<div>').attr('class', 'modal-footer');

        let modalCat = $('<span>').attr('class', 'badge badge-secondary').html(itemData.category.name);
        let modalMsr = $('<span>').attr('class', 'badge badge-info').html(itemData.measure.name);
        let modalTitle = $('<h5>').attr('class', 'modal-title').html(itemData.name + ' ').append(modalCat, ' ').append(modalMsr);
        let modalText = $('<p>').html('This item has been bought ' + itemData.bought + ' times!');
        let modalClose = $('<button>').attr('class', 'btn btn-secondary close-modal').attr('data-dismiss', 'modal').html('Close');
        let modalUse = $('<button>').attr('class', 'btn btn-primary').html('Purchase');
        
        modalHeader.append(modalTitle);
        modalBody.append(modalText);
        modalFooter.append(modalClose).append(modalUse);

        modalContent.append(modalHeader).append(modalBody).append(modalFooter);

        modalDocument.append(modalContent);

        modalContainer.append(modalDocument);

        $('.container').append(modalContainer);

        $('#modal').modal('toggle');

        this.resolve();

    });

    return func;

}