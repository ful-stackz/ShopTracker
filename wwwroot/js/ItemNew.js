// File: ItemNew.js
// Page: /item/new

// load all categories
// load all measures

$(window).on('load', function() {

    LoadAllCategories()
    .then(LoadAllMeasures);

});

var LoadAllCategories = function() {

    let request = $.ajax('/api/categories');

    request.done(function(allC) {

        // shortcut categories container
        // append all categories as options

        let cc = $('#category');
        
                for (var i = 0; i < allC.length; i++) {
        
                    cc.append($('<option>').attr('value', allC[i].categoryID).html(allC[i].name));
        
                }

    });

    request.fail(function(data) {

        console.log('Error loading categories\n', data);
    
    });

    return request;

}

var LoadAllMeasures = function() {

    let request = $.ajax('/api/measures');

    request.done(function(allM) {
        
        // shortcut measures container
        // append all measures as options

        let mc = $('#measure');

        for (var i = 0; i < allM.length; i++) {

            mc.append($('<option>').attr('value', allM[i].measureID).html(allM[i].name));

        }

    });

    request.fail(function(data) {

        console.log('Error loading measures\n', data);

    });

    return request;
}