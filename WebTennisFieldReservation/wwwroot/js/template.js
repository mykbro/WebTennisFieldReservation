function selectAllCheckboxes(dayOfWeek, check) {
    var checkBoxes = document.getElementsByClassName(dayOfWeek);

    for (var i = 0; i < checkBoxes.length; i++) {
        checkBoxes[i].checked = check;
    }
}