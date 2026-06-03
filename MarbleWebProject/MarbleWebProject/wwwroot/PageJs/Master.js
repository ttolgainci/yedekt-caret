//$(document).ready(function () {
//    var userID = getCookie("UserIDForBasket");
//    if (userID == null) {

//        var getUserGuid = randomGuid();

//        var userCookie = {
//            UserGuid: getUserGuid,
//        };
//        setCookie("UserIDForBasket", JSON.stringify(userCookie), 14);
//    }

//});
function setLang_Click(d) {

    var selectedMarketRow = d.getAttribute("data-id");
    var selectedMarketRowName = d.getAttribute("data-name");
    var selectedMarketRowCulture = d.getAttribute("data-culture");
    var data = {
        id: selectedMarketRow,
        name: selectedMarketRowName,
        culture: selectedMarketRowCulture
    };
    $.ajax({
        type: 'POST',
        url: '/Language/SetLanguageToUser',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: data,
        success: function (result) {

            if (result.redirectToUrl != "") { window.location.href = result.redirectToUrl; }
        },
        error: function () {
            alert('Failed to receive the Data');
        }
    })
}
//function functionRediretOnlyResultForConcept(d) {
//    var getCode = d.getAttribute("data-code");
//    var type = d.getAttribute("data-type");
//    $("#" + getCode + "").submit();

//}

$(document).on('click', '.page-link', function (e) {
    e.preventDefault();
    var page = $(this).data('page');

    $.ajax({
        url: '/Category/GetProducts',
        type: 'GET',
        data: { pageNumber: page },
        success: function (result) {
            $('#productContainer').html(result);
            window.scrollTo(0, 0);
            //window.scrollTo({
            //    top: 0,
            //    behavior: 'smooth' // For smooth scrolling
            //});
        },
        error: function () {
            alert('Failed to load products. Please try again.');
        }
    });
});
$(document).on('click', '.scroll-bottom-text', function (e) {
    //var element = document.getElementById("cat-desc-bottom");
    //if (element) {
    //    element.scrollIntoView({  behavior: 'smooth' });
    //}
    if (this.hash !== "") {
        e.preventDefault();
        var hash = this.hash;
        $('html, body').animate({
            scrollTop: $(hash).offset().top - 100
        }, 800, function () {
            window.location.hash = hash;
        });
    } 
});