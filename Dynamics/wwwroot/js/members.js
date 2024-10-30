$(document).ready(function () {
    loadDataTable('@ViewBag.ProjectID');
});

function loadDataTable(projectID) {
    $('#tblMembers').DataTable({
        "ajax": {
            "url": "/api/project/ManageProjectMemberTable/" + projectID,
            "dataSrc": "data"
        },
        "columns": [
            { "data": "userFullName", "width": "15%" },
            { "data": "userDOB", "width": "15%" },
            { "data": "userEmail", "width": "15%" },
            { "data": "userPhoneNumber", "width": "15%" },
            { "data": "userAddress", "width": "15%" },
            { "data": "userAvatar", "width": "15%" },
            { "data": "userDescription", "width": "15%" }
        ]
    });
};