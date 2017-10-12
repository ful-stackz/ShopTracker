// GLOBAL VARIABLES
var SELECTORS = [];
var G_PURCHASES = [];
var V_PURCHASES = [];
var G_CATEGORIES = [];
var V_CATEGORIES = [];

// SPINNER
var opts = {
    lines: 5 // The number of lines to draw
  , length: 50 // The length of each line
  , width: 5 // The line thickness
  , radius: 45 // The radius of the inner circle
  , scale: 1.00 // Scales overall size of the spinner
  , corners: 1 // Corner roundness (0..1)
  , color: '#67221b' // #rgb or #rrggbb or array of colors
  , opacity: 0 // Opacity of the lines
  , rotate: 2 // The rotation offset
  , direction: 1 // 1: clockwise, -1: counterclockwise
  , speed: 0.8 // Rounds per second
  , trail: 73 // Afterglow percentage
  , fps: 20 // Frames per second when using setTimeout() as a fallback for CSS
  , zIndex: 2e9 // The z-index (defaults to 2000000000)
  , className: 'spinner' // The CSS class to assign to the spinner
  //, top: '50%' // Top position relative to parent
  //, left: '50%' // Left position relative to parent
  , shadow: false // Whether to render a shadow
  , hwaccel: false // Whether to use hardware acceleration
  , position: 'relative' // Element positioning
}

$(window).on('load', function() {

    // when the page is loaded
    // load all groups for the logged in user
    // then load all purchases for that user
    // and store them in a global array
    // then load all the categories that
    // are contained in these purchases
    // then load all the selectors from the page
    // and put them in a global array as objects
    // which can be manipulated
    // then show all the search categories
    // then make all purchases visible
    // then show purchases

    var spinner = new Spinner(opts).spin();
    $('.container').append(spinner.el);
    $.when(LoadAllGroups()
        .then(LoadAllPurchases)
        .then(LoadAllCategories)
        .then(LoadSelectors)
        .then(MakeAllVisible)
        .then(ShowSearchCategories)
        .then(FixPurchasesDate)
        .then(SortPurchases)
        .then(ShowPurchases)).then(function() { spinner.stop(); });
});

$('#itemSearch').on('keyup', function(elem) {

	// get element's value
    // eg. the search term
    
	var searchT = elem.target.value;

	if (searchT.length === 0) {
		
		// if search term's length = 0
		// make all purchases visible
		// apply filters and display
		
		V_PURCHASES = G_PURCHASES.slice(); // make all purchases visible
		ApplyAllFilters().then(ShowPurchases);

	} else if (searchT.length < 3) {

		// if search term length is < 3
		// do nothing

		return;

	} else {

		// search term length >= 3
		// search through visible purchases
		// splice those not valid
		// then display purchases

		SearchPurchases(searchT).then(ShowPurchases);

	}

});

$('.filter').on('change', function() {

    // get all the filter properties
    // pass them to their
    // corresponding functions
    // then show purchases

    let gF = $('.filter#group').val();
    let cF = $('.filter#category').val();
    let dF = $('.filter#timeSpan').val();

    FilterGroup(gF)
    .then(FilterCategory(cF))
    .then(FilterDate(dF))
    .then(ShowPurchases);

});

$('input[type="checkbox"]').on('change', function(elem) {

    // shortcut the selectors
    // loop through the selectors
    // and change the checked state
    // of the one that was clicked
    // then toggle columns
    
    let allS = SELECTORS;

    for (var i = 0; i < allS.length; i++) {

        if (allS[i].id == elem.target.id) allS[i].checked = elem.target.checked;

    }

    ToggleColumns();

});

$('.sort').on('change', function() {

    // invoke the @SortPurchases function
    // then show the sorted purchases

    SortPurchases().then(ShowPurchases);

});
var ApplyAllFilters = function() {
	
	return $.Deferred(function() {

		// get filter values
		let gf = $(".filter#group").val();		// group filter
		let cf = $(".filter#category").val();	// category filter
		let df = $(".filter#timeSpan").val();	// date filter

		// apply filters
		FilterGroup(gf, false)
			.then(FilterCategory(cf, false))
			.then(FilterDate(df, false));

        this.resolve();

	});

}

var SearchPurchases = function(searchT) {

	return $.Deferred(function() {

		// make a shortcut of visible purchases array
		// splice the elements that don't fit
		// the search term @searchT

		let vp = V_PURCHASES;

		for (var i = vp.length - 1; i >= 0; i--) {

			// get current element item name
			// compare lower case of item name and search term

			let iname = vp[i].item.name.toLowerCase();
			searchT = searchT.toLowerCase();

			if (iname.includes(searchT) == false) {

				// splice visible purchases from that element

				vp.splice(i, 1);

			}

        }
        
        this.resolve();

	});

}

var LoadAllGroups = function() {

	let request = $.ajax("/api/groups/" + $("#userId").val());

	request.done(function(groups) {

		// shortcut the groups control
		// clear the control
		// add 'all' option
		// and fill it with the groups retrieved

		let gc = $("#group");

		gc.html("");

		gc.append($('<option>').attr('value', '0').html('All'));

		for (var i = 0; i < groups.length; i++) {

			// append every group as an option element
			
			gc.append($('<option>').attr('value', groups[i].groupID).html(groups[i].name));

		}

	});

	request.fail(function(data) {

		// make a console log with the data received

		console.log('Error while loading groups!\n', data);

	});

	return request;

}

var LoadAllPurchases = function() {

	// collect the data necessary for the ajax request
	// get userId and the verification token

	let rdata = {
		id: $('#userId').val(),
		__RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
	}

	let request = $.ajax('/api/purchases/user', {
		type: 'POST',
		data: rdata
	});

	request.done(function(purchases) {

		// load the requested purchases
		// into the global purchases array

		G_PURCHASES = purchases;

	});

	request.fail(function(data) {

		// make a console log with the data

		console.log('Error while loading the purchases!\n', data);

	});

	return request;

}

var LoadAllCategories = function() {

	let func = $.Deferred(function() {

        let allP = G_PURCHASES;
        let allC = G_CATEGORIES;


        for (var i = 0; i < allP.length; i++) {

            if (allC.length == 0) {
                
                allC.push(allP[i].item.category);
                continue;
            }

            for (var j = 0; j < allC.length; j++) {
                
                if (allC[j].categoryID == allP[i].item.category.categoryID) break;
                
                if (j == allC.length - 1) allC.push(allP[i].item.category)
            }

        }

        this.resolve();

    });

    return func;

}

var LoadSearchCategories = function() {

    let func = $.Deferred(function() {

        // shortcut global variables
        // loop through visible purchases
        // if their category is not visible
        // add it to visibles

        let visP = V_PURCHASES;
        let visC = V_CATEGORIES;

        visC.splice(1);

        for (var i = 0; i < visP.length; i++) {

            if (visC.length == 0) {

                visC.push(visP[i].item.category);
                continue;

            }

            for (var j = 0; j < visC.length; j++) {

                if (visC[j].categoryID == visP[i].item.category.categoryID) break;

                if (j == visC.length - 1) visC.push(visP[i].item.category);

            }

        }

        this.resolve();

    });

    return func;
}

var LoadSelectors = function() {

    let func = $.Deferred(function() {

        // get all checkbox elements from html
        // turn them into objects
        // and add the to the global array

        let gloS = SELECTORS;
        let allS = $('input[type="checkbox"]');
        
        for (var i = 0; i < allS.length; i++) {

            let newS = {
                'id': allS[i].id,
                'checked': allS[i].checked,
                'data': allS[i].getAttribute('control')
            }

            gloS.push(newS);

        }

        this.resolve();

    });

    return func;

}

var ShowPurchases = function() {

    let func = $.Deferred(function() {

        // shortcut the visible purchases
        // check if there are any
        // if there aren't
        // hide the table and quit

        let visP = V_PURCHASES;

        if (visP.length == 0) {

            $('#itemsContainer').addClass('d-none');
            $('#itemsTable').addClass('d-none');

            this.resolve();

            return;

        }

        // there are visible purchases to be shown
        // shortcut the table's tbody
        // empty the tbody
        // loop through the purchases
        // and append them as rows of data

        let tBody = $('#itemsTable > tbody');

        tBody.html('');

        for (var i = 0; i < visP.length; i++) {

            // tBody.append($('<tr>')
            //     .append($('<td>').attr('class', 'data-name')
            //         .html(visP[i].item.name)
            //         .append($('<span>').attr('class', 'text-muted ml-1').html(visP[i].item.category.name)))
            //     .append($('<td>').attr('class', 'data-quantity')
            //         .html(visP[i].quantity)
            //         .append($('<span>').attr('class', 'text-muted ml-1').html(visP[i].item.measure.name)))
            //     .append($('<td>').attr('class', 'data-price')
            //         .html(visP[i].price)
            //         .append($('<span>').attr('class', 'text-muted ml-1').html(visP[i].currency.name)))
            //     .append($('<td>').attr('class', 'data-date')
            //         .html(visP[i].date))
            //     .append($('<td>').attr('class', 'data-provider')
            //         .html(visP[i].provider))
            //     .append($('<td>').attr('class', 'data-actions')
            //         .append($('<a>').attr('href', '/purchase/edit/' + visP[i].purchaseID).html('Edit '))
            //         .append($('<a>').attr('href', '/purchase/delete/' + visP[i].purchaseID).attr('class', 'text-danger').html('Delete'))
            // ));

            let tableRow = $('<tr>').attr('class', 'bg-xx-4');

            let colColor = $('<td>').attr('class', 'td-color');

            let colNameDesc = $('<td>').attr('class', 'td-name-desc data-name')
                .append(visP[i].item.name)
                .append('<br>')
                .append($('<small>').attr('class', 'text-muted').html(visP[i].item.description));

            let colCatPrvd = $('<td>').attr('class', 'td-cat-prvd')
                .append($('<div>').attr('class', 'td-border'))
                .append(visP[i].item.category.name)
                .append('<br>')
                .append($('<small>').attr('class', 'text-muted').html(visP[i].provider));

            let colQtyMsr = $('<td>').attr('class', 'td-qty-msr')
                .append($('<div>').attr('class', 'td-border'))
                .append(visP[i].quantity)
                .append('<br>')
                .append($('<small>').attr('class', 'text-muted').html(visP[i].item.measure.name));

            let colPrcCur = $('<td>').attr('class', 'td-prc-cur')
                .append($('<div>').attr('class', 'td-border'))
                .append(visP[i].price)
                .append('<br>')
                .append($('<small>').attr('class', 'text-muted').html(visP[i].currency.fullName));

            let colDate = $('<td>').attr('class', 'td-date')
                .append($('<div>').attr('class', 'td-border'))
                .append(visP[i].date.substring(0, 5))
                .append('<br>')
                .append($('<small>').attr('class', 'text-muted').html(visP[i].date.substring(6, 10)));

            let colAction = $('<td>').attr('class', 'td-actions bg-xx-2 align-middle')
                .append($('<a>').attr('href', '/purchase/edit/' + visP[i].purchaseID).append($('<i>').attr('class', 'material-icons').html('edit')))
                .append(' ')
                .append($('<a>').attr('href', '/purchase/delete/' + visP[i].purchaseID).append($('<i>').attr('class', 'material-icons text-xx-9').html('delete')));
            
            tableRow.append(colColor).append(colNameDesc).append(colCatPrvd).append(colQtyMsr).append(colPrcCur).append(colDate).append(colAction);

            tBody.append(tableRow);
            
        }

        $('#itemsContainer').removeClass('d-none');
        $('#itemsTable').removeClass('d-none');

        this.resolve();

    });

    return func;
}

var ShowSearchCategories = function() {

    let func = $.Deferred(function() {

        // shortcut the visible categories
        // clear categories container
        // append an 'all' and
        // all of the above categories
        // as options to the category select element

        let visC = V_CATEGORIES;
        let catC = $('#category');

        if ($('#category > option[value="0"]').length == 0) {
         
            catC.append($('<option>').attr('value', '0').html('All'));
        
        }

        for (var i = 0; i < visC.length; i++) {

            if ($('#category > option[value="' + visC[i].categoryID + '"]').length == 0) {
        
                catC.append($('<option>').attr('value', visC[i].categoryID).html(visC[i].name));
        
            }

        }

        this.resolve();

    });

    return func;

}

var ToggleColumns = function() {

    let func = $.Deferred(function() {

        // loop through the selectors
        // see which one is checked
        // the checked make visible with d-table-cell
        // these which are not
        // make invisible with d-none

        let allS = SELECTORS;

        for (var i = 0; i < allS.length; i++) {

            if (allS[i].checked == true) {

                $('.' + allS[i].data).addClass('d-table-cell').removeClass('d-none');

            } else {

                $('.' + allS[i].data).addClass('d-none').removeClass('d-table-cell');

            }

        }

        this.resolve();

    });

    return func;
}

var FixPurchasesDate = function() {

    let func = $.Deferred(function() {

        // loop through all the purchases
        // and change their date from
        // yyyy-mm-ddThh-mm-ss to
        // dd/mm/yyyy

        let allP = G_PURCHASES;

        for (var i = 0; i < allP.length; i++) {

            let fulD = allP[i].date.split('T')[0]; // get yyyy-mm-dd
            let date = fulD.split('-')[2];
            let mont = fulD.split('-')[1];
            let year = fulD.split('-')[0];
            let newD = date + '/' + mont + '/' + year;

            allP[i].date = newD;

        }

        this.resolve();

    });

    return func;

}

var FilterGroup = function(g, s = false) {

    let func = $.Deferred(function() {

        // if @g is 0 then show 'all'
        // make all purchases visible
        // and if @s is true then
        // call @ShowPurchases

        console.log('group filter', 'g = ', g);

        if (g == 0) {

            V_PURCHASES = G_PURCHASES.slice();

            if (s) ShowPurchases();

            this.resolve();
            return;

        }

        // if @g is not null then
        // filter all the purchases
        // with the given @g for groupID
        // and make those who match visible
        // and if @s is true then
        // call @ShowPurchasesw

        let allP = G_PURCHASES;
        V_PURCHASES = [];
        let visP = V_PURCHASES;

        console.log('visP', visP);
        console.log('V_PURCHASES', V_PURCHASES);

        for (var i = 0; i < allP.length; i++) {
            console.log('allP[i].groupID == g', allP[i].groupID == g);
            if (allP[i].groupID == g) visP.push(allP[i]);

        }

        if (s) ShowPurchases();

        this.resolve();

    });

    return func;
}

var FilterCategory = function(c, s = false) {

    let func = $.Deferred(function() {
        
        // if @c is 0 then do nothing
        // and if @s is true then
        // call @ShowPurchases

        if (c == 0) {

            if (s) ShowPurchases();

            this.resolve();
            return;

        }

        // if @c is not null then
        // filter all the purchases
        // with the given @c for categoryID
        // and make those who match visible
        // and if @s is true then
        // call @ShowPurchasesw

        let visP = V_PURCHASES;

        for (var i = visP.length - 1; i >= 0; i--) {

            if (visP[i].item.categoryID != c) visP.splice(i, 1);

        }

        if (s) ShowPurchases();

        this.resolve();
        
    });

    return func;

}

var FilterDate = function(d, s = false) {
    
    let func = $.Deferred(function() {
        
        // if @d is 0 then do nothing
        // and if @s is true then
        // call @ShowPurchases

        if (d == 0) {

            if (s) ShowPurchases();

            this.resolve();
            return;

        }

        // get current date
        // set condition('conditions expl')
        // shortcut visible purchases
        // loop through them and apply filter
        // conditions expl:
        // 1 = today => if purchase.date = currentDate
        // 2 = this week => if purchase.date - currentDate >= 0
        // 3 = this month => if purchase.date - dateOneMonthAgo >= 0
        // 4 = this year => if purchase.date - dateOneYearAgo >= 0

        let conD = new Date();
        let visP = V_PURCHASES;

        switch (d) {

            case '2':
                conD.setDate(conD.getDate() - 6);
                break;

            case '3':
                conD.setMonth(conD.getMonth() - 1);
                break;

            case '4':
                conD.setFullYear(conD.getFullYear() - 1);
                break;

        }

        for (var i = visP.length - 1; i >= 0; i--) {

            // get visible purchase's date data
            // create new Date object from
            // visible purchase's date data

            let purDate = visP[i].date.split('/')[0];
            let purMont = visP[i].date.split('/')[1];
            let purYear = visP[i].date.split('/')[2];

            let purD = new Date();
            purD.setDate(purDate);
            purD.setMonth(parseInt(purMont) - 1); // because Date.Month is 0 based
            purD.setFullYear(purYear);

            if (purD - conD < 0) visP.splice(i, 1);

        }
        
        if (s) ShowPurchases();

        this.resolve();
        
    });

    return func;

}

var MakeAllVisible = function() {

    let func = $.Deferred(function() {

        V_PURCHASES = G_PURCHASES.slice();

        V_CATEGORIES = G_CATEGORIES.slice();

        this.resolve();

    });

    return func;
}

var SortPurchases = function(o) {

    let func = $.Deferred(function() {

        // get orderby options from html
        // get sortby option
        // shortcut the visible purchases
        // set the order option according
        // to the orderby value
        // then sort according to the
        // sort option value

        let order = $('#orderby').val();
        let sort = $('#sortby').val();
        let visP = V_PURCHASES;

        switch (order) {
            case '0': order = 'date'; break;
            case '1': order = 'quantity'; break;
            case '2': order = 'price'; break;
        }
        
        if (order == 'date') {
            
            visP.sort(function(a, b) {
                
                let aD = new Date();
                aD.setDate(a.date.split('/')[0]);
                aD.setMonth(parseInt(a.date.split('/')[1]) - 1);
                aD.setFullYear(a.date.split('/')[2]);

                let bD = new Date();
                bD.setDate(b.date.split('/')[0]);
                bD.setMonth(parseInt(b.date.split('/')[1]) - 1);
                bD.setFullYear(b.date.split('/')[2]);

                return aD - bD;
    
            });

        }

        visP.sort(function(a, b) {
            
            return a[order] - b[order];

        });
        

        if (sort == 1) {

            visP.reverse();

        }

        this.resolve();

    });

    return func;
}