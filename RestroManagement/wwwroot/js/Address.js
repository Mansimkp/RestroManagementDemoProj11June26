$(document).ready(function () {

    $("#StateId").change(function () {

        var stateId = $(this).val();

        $("#CityId").empty();

        $("#CityId").append(
            '<option value="">Select City</option>'
        );


        $.ajax({

            url: '/Account/GetCities',

            type: 'GET',

            data: {
                stateId: stateId
            },

            success: function (data) {

                $.each(data, function (i, item) {

                    $("#CityId").append(
                        '<option value="' + item.name + '">' +
                        item.name +
                        '</option>'
                    );

                });

            },

            error: function (xhr) {

                console.log(xhr.responseText);
                alert("City is not loaded");

            }

        });

    });

});