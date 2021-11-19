let selectedPoint;

$(() => {
    $(".json-data").each(function () {
        let jsonQuestion = $(this).data("json-question");
        let jsonAnsewer = $(this).data("json-answer");
        switch ($(this).data("question-type")) {
            case 'Text':
            case 'Number':
            case 'Date':
            case 'Radiobuttons':
                $(this).text(jsonAnsewer.answer.length == 0 ? "[Blank]" : jsonAnsewer.answer);
                break;

            case 'Checkboxes':
                $(this).text(jsonAnsewer.answer.length == 0 ? "[Blank]" : jsonAnsewer.answer.join(", "));
                break;

            case 'Image':
                $(this).append($('<div>').prop({ id: 'img-wrapper' })
                    .append($('<img>').prop({
                        id: 'image',
                        class: 'img-fluid',
                        src: jsonQuestion.image.url
                    }).on('load', () => {
                        if (jsonAnsewer.answer) {
                            selectedPoint = jsonAnsewer.answer;
                            drawSelectedPointOnImage();
                        }
                        $(document).trigger("stickyTable");
                    })));
                break;
        }
    });
});

let doit;
window.onresize = () => {
    if (selectedPoint) {
        clearTimeout(doit);
        doit = setTimeout(drawSelectedPointOnImage, 100);
    }
};

let drawSelectedPointOnImage = () => {
    if (selectedPoint.x) {
        $('.img-point').remove();
        let img = $('#img-wrapper');
        let point = $('<div class="img-point"></div>');

        let x = selectedPoint.x * $("#image").width();
        let y = selectedPoint.y * $("#image").height();
        point.css({
            left: x + "px",
            top: y + "px"
        });
        point.appendTo(img);
    }
}