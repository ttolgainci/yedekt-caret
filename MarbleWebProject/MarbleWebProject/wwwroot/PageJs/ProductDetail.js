var index = 1;
//$(document).ready(function () {

//    $('.itemPlus').click(function () {


//    });
//});
//(function () {
//    $('body', document)
//        .on('click', '.inBasket .icon-plus', function () {
//            debugger;
//            var mainDivId = $(this).closest('.cartProductItem').attr('id').split("_")[1];

//            var getTotal = $("#basketTotal").text();
//            var getQty = $("#productQtySpinner_" + mainDivId + "").val();
//            var getHasQty = $("#cartproductqty_" + mainDivId + "").text();
//            var getHasPrice = $("#cartproductPrice_" + mainDivId + "").text();



//            $("#cartproductqty_" + mainDivId + "").text(parseInt(getQty));
//            $("#basketTotal").text(parseFloat(getTotal) + parseFloat(getHasPrice));


//            $.ajax({
//                type: 'POST',
//                url: '/Cart/AddToCart',
//                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
//                data: { ProductID: mainDivId, ProductName: "", Price: getHasPrice, CurrencyName: "", MainImage: "", Url: "", CartQuantity: getQty },
//                success: function (result) {

//                },
//                error: function () {

//                }
//            })

//        })

//        ;
//})();



