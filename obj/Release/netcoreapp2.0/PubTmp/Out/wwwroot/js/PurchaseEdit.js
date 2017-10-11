/*!
 *
 *
!*/

// GLOBAL VARIABLES
var G_GROUPS = [];
var G_CURRENCIES = [];

// EVENTS
$(window).on('load', function() {
    
    LoadAllGroups()
    .then(LoadAllCurrencies);

});

// FUNCTIONS
var LoadAllGroups = function() {

    let request = $.ajax('/api/groups/' + $('#userId').val());

    request.done(function(groups) {

        // insert all groups into the global array
        // for easier access
        // shortcut the groups container
        // get loaded group for comparison
        // append every other group as new <option>

        G_GROUPS = groups;

        let gc = $('#group');
        let og = $('#group > option');

        for (var i = 0; i < groups.length; i++) {

            if (groups[i].groupID == og.val()) continue;

            gc.append($('<option>').attr('value', groups[i].groupID).html(groups[i].name));

        }

    });

    request.fail(function(data) {

        console.log('Error loading groups\n', data);

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

        let cc = $('#currency');
        let oc = $('#currency > option');

        for (var i = 0; i < allC.length; i++) {

            if (allC[i].currencyID == oc.val()) continue;

            cc.append($('<option>').attr('value', allC[i].currencyID).html(allC[i].name));

        }

    });

    request.fail(function(data) {

        console.log('Error loading currencies\n', data);

    });

    return request;

}