jQuery && function(t) {
    function i(i, n) {
        var d = i ? t(this) : n,
            o = t(d.attr("spid-idp-button")),
            r = d.hasClass("spid-idp-button-open")
        if (i) {
            if (t(i.target).hasClass("spid-idp-button-ignore")) return
            i.preventDefault(), i.stopPropagation()
        } else if (d !== n.target && t(n.target).hasClass("spid-idp-button-ignore")) return
        s(), r || d.hasClass("spid-idp-button-disabled") || (d.addClass("spid-idp-button-open"), o.data("spid-idp-button-trigger", d).show(), e(), o.trigger("show", { spidIDPButton: o, trigger: d }))
    }

    function s(i) {
        var s = i ? t(i.target).parents().addBack() : null
        if (s && s.is(".spid-idp-button")) {
            if (!s.is(".spid-idp-button-menu")) return
            if (!s.is("A")) return
        }
        t(document).find(".spid-idp-button:visible").each(function() {
            var i = t(this)
            i.hide().removeData("spid-idp-button-trigger").trigger("hide", { spidIDPButton: i })
        }), t(document).find(".spid-idp-button-open").removeClass("spid-idp-button-open")
    }

    function e() {
        var i = t(".spid-idp-button:visible").eq(0),
            s = i.data("spid-idp-button-trigger"),
            e = s ? parseInt(s.attr("data-horizontal-offset") || 0, 10) : null,
            n = s ? parseInt(s.attr("data-vertical-offset") || 0, 10) : null
        0 !== i.length && s && (i.hasClass("spid-idp-button-relative") ? i.css({ left: i.hasClass("spid-idp-button-anchor-right") ? s.position().left - (i.outerWidth(!0) - s.outerWidth(!0)) - parseInt(s.css("margin-right"), 10) + e : s.position().left + parseInt(s.css("margin-left"), 10) + e, top: s.position().top + s.outerHeight(!0) - parseInt(s.css("margin-top"), 10) + n }) : i.css({ left: i.hasClass("spid-idp-button-anchor-right") ? s.offset().left - (i.outerWidth() - s.outerWidth()) + e : s.offset().left + e, top: s.offset().top + s.outerHeight() + n }))
    }
    t.extend(t.fn, {
        spidIDPButton: function(e, n) {
            switch (e) {
                case "show":
                    return i(null, t(this)), t(this)
                case "hide":
                    return s(), t(this)
                case "attach":
                    return t(this).attr("spid-idp-button", n)
                case "detach":
                    return s(), t(this).removeAttr("spid-idp-button")
                case "disable":
                    return t(this).addClass("spid-idp-button-disabled")
                case "enable":
                    return s(), t(this).removeClass("spid-idp-button-disabled")
            }
        }
    }), t(document).on("click.spid-idp-button", "[spid-idp-button]", i), t(document).on("click.spid-idp-button", s), t(window).on("resize", e)
}(jQuery);

function ClickB2CButton(value) {
    $("#" + value).click();
}

function randomSort(a, b) {
    return 0.5 - Math.random();
}

function AddSPIDButton(buttonList, container) {

    var containerId = $(container).attr("id");

    //spid button
    container.append("<a href=\"#\" class=\"italia-it-button italia-it-button-size-m button-spid\" spid-idp-button=\"#spid-idp-button-medium-get-" + containerId + "\" aria-haspopup=\"true\" aria-expanded=\"false\"></a>");

    //spid UL that will contain idps buttons
    container.append("<div id=\"spid-idp-button-medium-get-" + containerId + "\" class=\"spid-idp-button spid-idp-button-tip spid-idp-button-relative\">" +
        "<ul id=\"spid-idp-list-large-root-get-" + containerId + "\" class=\"spid-idp-button-menu\" aria-labelledby=\"spid-idp\">");

    //get a reference to the newly created UL
    var spidUL = $("#spid-idp-list-large-root-get-" + containerId);

    //for each idp...
    buttonList.each(function(index) {

        var imageName = $(this).text();
        var buttonId = $(this).attr('id');

        //create the li item with corresponding content (image, alt, span, a with onlick)
        var li = $("<li class='spid-idp-button-link'>");
        var a = $("<a href='#' onclick='ClickB2CButton(\"" + buttonId + "\")'>");
        a.append("<span class='spid-sr-only'>" + imageName + " ID</span>");
        a.append("<img src='{Settings:CustomUiBlobStorageUrl}/spididp/" + imageName + ".png' alt='" + imageName + " ID' />");
        li.append(a);

        //append the li to the UL
        spidUL.append(li);
    });

    //complete the UL with support links
    spidUL.append("<li class=\"spid-idp-support-link\">" +
        "<a href=\"http://www.spid.gov.it\">Maggiori info</a>" +
        "</li>" +
        "<li class=\"spid-idp-support-link\">" +
        "<a href=\"http://www.spid.gov.it/#registrati\">Non hai SPID?</a>" +
        "</li>");
}

function MoveButtons(buttonList, container) {
    $(buttonList).appendTo(container);
}

$(document).ready(function() {

    var spidpfarea = $("#spidpf-button-container");
    var spidpgarea = $("#spidpg-button-container");
    var ciearea = $("#cie-button-container");
    var cnsarea = $("#cns-button-container");

    var spidPfIdps = $("button[id^='SPID-PF'").sort(randomSort);
    var spidPgIdps = $("button[id^='SPID-PG'").sort(randomSort);
    var cieIdps = $("button[id^='CIE-'").sort(randomSort);
    var cnsIdps = $("button[id^='CNS-'").sort(randomSort);

    AddSPIDButton(spidPfIdps, spidpfarea);
    // AddSPIDButton(spidPgIdps, spidpgarea);
    MoveButtons(cieIdps, ciearea);
    MoveButtons(cnsIdps, cnsarea);
});