$(() => {
    $(".json-data").each(function () {
        let json = $(this).data("json");
        switch ($(this).data("type")) {
            case 'Checkboxes':
            case 'Radiobuttons':
                $(this).text(json.labels.join(", "));
                break;
            case 'Image':
                $(this).append(`<img width='100px' src='${json.image.url}' />`);
                break;
        }
    });
});