// If there is no ValidationMessageFor on the form, then don't show the div container with class <alert alert-danger>
// The default span container of the ValidationMessageFor has a length of 140 characters
// If the div containing the span has a innerHTML.length > 140 then there is a ValidationMessage inside
// In that case remove the class that makes it invisible - d-none
//
var ValidationContainers = document.getElementsByClassName("alert alert-danger");
for (var i = ValidationContainers.length - 1; i >= 0; i--) {
    if (ValidationContainers[i].innerHTML.length > 145) ValidationContainers[i].classList.remove("d-none");
}