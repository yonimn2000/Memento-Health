$(() => {
    $(".json-data").each(function () {
        let json = $(this).data("json");
        switch ($(this).data("type")) {
            case 'Checkboxes':
            case 'Radiobuttons':
                $(this).text(json.Labels.join(", "));
                break;
            case 'Image':
                $(this).append(`<img width='100px' src='${json.ImageBase64}' />`);
                break;
        }
    });
});