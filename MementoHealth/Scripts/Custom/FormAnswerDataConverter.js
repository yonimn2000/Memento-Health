$(() => {
    $(".json-data").each(function (index) {
        let jsonQuestion = $(this).data("json-question");
        let jsonAnswer = $(this).data("json-answer");
        switch ($(this).data("question-type")) {
            case 'Text':
            case 'Number':
            case 'Date':
            case 'Radiobuttons':
                $(this).text(jsonAnswer.answer.length == 0 ? "[Blank]" : jsonAnswer.answer);
                break;

            case 'Checkboxes':
                $(this).text(jsonAnswer.answer.length == 0 ? "[Blank]" : jsonAnswer.answer.join(", "));
                break;

            case 'Image':
                $(this).append($('<div>').prop({ id: 'img-wrapper-' + index })
                    .css("position", "relative")
                    .append($('<img>').prop({
                        class: 'img-fluid answer-image',
                        src: jsonQuestion.image.url
                    }).on('load', () => {
                        $(document).trigger("stickyTable");
                        if (jsonAnswer.answer)
                            drawPointOnImage(jsonAnswer.answer, index);
                    })));
                break;
        }
    });
});

let doit;
window.onresize = () => {
    clearTimeout(doit);
    doit = setTimeout(() => {
        $(".answer-image").each(function (index) {
            let jsonAnswer = $(this).parent().parent().data("json-answer");
            drawPointOnImage(jsonAnswer.answer, index);
        });
    }, 100);
};

let drawPointOnImage = (point, index) => {
    if (point.x && index >= 0) {
        let imgWrapperSelector = '#img-wrapper-' + index;
        $(imgWrapperSelector + ' .img-point').remove();

        let img = $(imgWrapperSelector);
        let x = point.x * $(imgWrapperSelector + ' img').width();
        let y = point.y * $(imgWrapperSelector + ' img').height();

        let pointDiv = $('<div class="img-point"></div>');
        pointDiv.css({
            left: x + "px",
            top: y + "px"
        });
        pointDiv.appendTo(img);
    }
}