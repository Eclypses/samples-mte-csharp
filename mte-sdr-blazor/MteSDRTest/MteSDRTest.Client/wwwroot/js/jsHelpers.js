/**
 * Sets focus to the html element with the incoming id
 * and selects the text within that element.
 * @param {any} id The id of the element you wish to set focus to.
 */
function focusInput(id) {
    var el = document.getElementById(id);
    if (el) {
        el.focus();
        el.select();
    }
}
/**
 * Sets focus to the html element with the incoming id.
 * @param {any} id The id of the element you wish to set focus to.
 */
function focusButton(id) {
    var el = document.getElementById(id);
    if (el) {
        el.focus();
    }
}
