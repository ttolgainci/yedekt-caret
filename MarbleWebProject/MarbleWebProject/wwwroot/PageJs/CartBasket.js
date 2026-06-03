function resolveMediaUrl(path) {
    if (!path) return '';
    if (path.indexOf('http://') === 0 || path.indexOf('https://') === 0) {
        return path.replace(/^https:\/\/(localhost|127\.0\.0\.1)/i, 'http://$1');
    }
    var base = (window.MarbleStore && window.MarbleStore.mediaBase) || '';
    if (!base) return path.charAt(0) === '/' ? path : '/' + path;
    if (base.charAt(base.length - 1) !== '/') base += '/';
    return base + path.replace(/^\/+/, '');
}

function setProductToBasket(d) {



    var pID = d.getAttribute("data-pid");
    /*var qty = $("#productQty_" + pID + "").val();*/
    var qty = $('.product-detail-code_' + pID +' input[type="number"].product-detail-Quantity').val();
    var purl = d.getAttribute("data-purl");
    $.ajax({
        type: 'POST',
        url: '/Cart/AddToCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: pID, Url: purl, CartQuantity: parseInt(qty),},
        success: function (result) {
            var data = result.cartList;
            var dataTotal = result.info;
            var str = "";
            /*$("#setBasketProductDiv").empty();*/
            $(".basket-area-main").empty();
            str += "<div class='dropdown-cart-products basket-list'>";
            for (var i = 0; i < data.length; i++) {
                var item = data[i];

                str += "   <div class='product cartProductItem basket-product-code_" + item.productID + "'>";
                str += "       <div class='product-cart-details productItem'>";
                str += "           <h4 class='product-title'>";
                str += "               <a href='" + item.url + "'>" + item.name + "</a>";
                str += "           </h4>";
                str += "           <span class='cart-product-info'>";
                str += "               <span class='cart-product-qty basket-product-quantity'>" + parseInt(item.quantity) + "</span>";
                str += "              x <span class='cartProductPrice basket-product-price'>" + item.price + "</span> " + item.currency + " ";
                str += "           </span>";
                str += "       </div>";
                str += "       <figure class='product-image-container'>";
                str += "           <a href='" + item.url + "' class='product-image'>";
                str += "               <img src='" + resolveMediaUrl(item.image) + "' alt='product'>";
                str += "           </a>";
                str += "       </figure>";
                str += "       <a onclick='removeProductFromBasket(this)' data-pid='" + item.productID + "' class='btn-remove' title='Remove Product'><i class='icon-close'></i></a>";
                str += "   </div>";
            }
            str += "   </div>";
            str += "<div class='dropdown-cart-total'>";
            str += "<span>total</span>";
            str += "<span class='cart-total-price'>" + dataTotal.totalPrice + "</span>";
            str += "</div>";
            str += " <div class='dropdown-cart-action'>";
            str += "   <a href='/cart' class='btn btn-primary'>View Cart</a>";
            str += "   <a href='checkout.html' class='btn btn-outline-primary-2'><span>Checkout</span><i class='icon-long-arrow-right'></i></a>";
            str += " </div>";
            $(".basket-quantity-count").text(parseInt(dataTotal.totalQuantity));
            $(".basket-quantity-count").show();
            $('.product-detail-code_' + pID + ' .btn-product').text("in the basket");
            $(".basket-area-main").append(str);
            $(".basket-area-main").show();
        },
        error: function () {

        }
    })




}
function removeProductFromBasket(d) {
    var pID = d.getAttribute("data-pid");

    //var total = $("#basketTotal").text();
    //var price = $("#cartproductPrice_" + pID + "").text();
    //var dropCount = $(".basket-quantity-count").text();
    //var cartproductqty = $("#cartproductqty_" + pID + "").text();
    //var setCartCount = parseInt(dropCount) - parseInt(cartproductqty);
    //$(".basket-quantity-count").empty();
    //$(".basket-quantity-count").append(setCartCount);
    //var dropTotal = parseFloat(total) - (parseFloat(price) * parseInt(cartproductqty));
    //$("#basketTotal").text(dropTotal);
    //$("#basket_" + pID + "").remove();
    var total = $(".basket-area-main .basket-total-price").text();
    var price = $('.basket-product-code_' + pID + ' .basket-product-price').text();
    var basketproductqty = $('.basket-product-code_' + pID + ' .basket-product-quantity').text();
    var dropCount = $(".basket-quantity-count").text();
    var setBasketCount = parseInt(dropCount) - parseInt(basketproductqty);
    $(".basket-quantity-count").empty();
    $(".basket-quantity-count").append(setBasketCount);
    var dropTotal = parseFloat(total) - (parseFloat(price) * parseInt(basketproductqty));
    $(".basket-area-main .basket-total-price").text(dropTotal);
    $(".basket-product-code_" + pID + "").remove();

    $(".cart-table-total .basket-total-price").text(dropTotal);
    $(".cart-product-code_" + pID + "").remove();

    if ($('.cartProductItem').length == 0) {
        $(".basket-list").empty();
        $(".basket-area-main").hide();
        $(".basket-quantity-count").empty();
        $(".basket-quantity-count").hide();
    }

    $.ajax({
        type: 'POST',
        url: '/Cart/DeleteFromCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: pID },
        success: function (result) {

        },
        error: function () {

        }
    })
}
function removeProductFromCart(d) {
    var pID = d.getAttribute("data-pid");

    var total = $(".basket-area-main .basket-total-price").text();
    var price = $('.cart-product-code_' + pID + ' .cart-product-price').text();
    var dropCount = $(".basket-quantity-count").text();
    var cartproductqty = $('.cart-product-code_' + pID + ' input[type="number"].cart-product-quantity-new').val();
    var setCartCount = parseInt(dropCount) - parseInt(cartproductqty);
    $(".basket-quantity-count").empty();
    $(".basket-quantity-count").append(setCartCount);
    var dropTotal = parseFloat(total) - (parseFloat(price) * parseInt(cartproductqty));
    $(".cart-table-total .basket-total-price").text(dropTotal);
    $(".basket-area-main .basket-total-price").text(dropTotal);
    $(".cart-product-code_" + pID + "").remove();
    $(".basket-product-code_" + pID + "").remove();

    if ($('.cartProductItem').length == 0) {
        $(".basket-list").empty();
        $(".basket-area-main").hide();
        $(".basket-quantity-count").empty();
        $(".basket-quantity-count").hide();
    }

    $.ajax({
        type: 'POST',
        url: '/Cart/DeleteFromCart',
        contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        data: { ProductID: pID },
        success: function (result) {

        },
        error: function () {

        }
    })
}
(function () {
    $('body', document)
        .on('click', '.cartBasketList .icon-plus', function () {
            var mainDivId = $(this).closest('.cartProductItem').attr('id').split("_")[1];
            //var getTotal = $("#basketTotal").text();
            //var getHasQty = $("#cartproductqty_" + mainDivId + "").text();
            //var getQty = $("#productQtySpinner_" + mainDivId + "").val();
            //var getHasPrice = $("#cartproductPrice_" + mainDivId + "").text();
            //var getUrl = $("#cartproductUrl_" + mainDivId + "").attr("href");
            var getTotal = $(".basket-area-main .basket-total-price").text();
            var getHasQty = $('.cart-product-code_'+mainDivId+' input[type="text"].cart-product-quantity-old').val();
            var getQty = $('.cart-product-code_' + mainDivId + ' input[type="number"].cart-product-quantity-new').val();
            var getHasPrice = $('.cart-product-code_' + mainDivId + ' .cart-product-price').text();
            var getUrl = $('.cart-product-code_' + mainDivId + ' .cart-product-url').attr("href");
           
            
            
            if (parseInt(getQty) > parseInt(getHasQty)) {
                var newPrice = (parseInt(getQty) - parseInt(getHasQty)) * parseFloat(getHasPrice);
                 //$(".cartListTotal #basketTotal").text(parseFloat(getTotal) + parseFloat(newPrice));
                //$("#cartproductqty_" + mainDivId + "").text(parseInt(getQty));
                //var oldQty = $(".basket-quantity-count").text();
                //$(".basket-quantity-count").text(parseInt(oldQty) + (parseInt(getQty) - parseInt(getHasQty)));
                //$("#setTotalPriceForCart #basketTotal").text(parseFloat(getTotal) + parseFloat(newPrice));

                $(".cart-table-total .basket-total-price").text(parseFloat(getTotal) + parseFloat(newPrice)); 
                $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val(parseInt(getQty));
                var oldQty = $(".basket-quantity-count").text();
                $(".basket-quantity-count").text(parseInt(oldQty) + (parseInt(getQty) - parseInt(getHasQty)));
                $(".basket-area-main .basket-total-price").text(parseFloat(getTotal) + parseFloat(newPrice));
            }        
            $.ajax({
                type: 'POST',
                url: '/Cart/AddToCart',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: { ProductID: mainDivId, Url: getUrl, CartQuantity: getQty },
                success: function (result) {

                },
                error: function () {

                }
            })

        })
        .on('click', '.cartBasketList .icon-minus', function () {
            var mainDivId = $(this).closest('.cartProductItem').attr('id').split("_")[1];

            //var getTotal = $("#basketTotal").text();
            //var getHasQty = $("#cartproductqty_" + mainDivId + "").text();
            //var getQty = $("#productQtySpinner_" + mainDivId + "").val();
            //var getHasPrice = $("#cartproductPrice_" + mainDivId + "").text();
            //var getUrl = $("#cartproductUrl_" + mainDivId + "").attr("href");
            var getTotal = $(".basket-area-main .basket-total-price").text();
            var getHasQty = $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val();
            var getQty = $('.cart-product-code_' + mainDivId + ' input[type="number"].cart-product-quantity-new').val();
            var getHasPrice = $('.cart-product-code_' + mainDivId + ' .cart-product-price').text();
            var getUrl = $('.cart-product-code_' + mainDivId + ' .cart-product-url').attr("href");
            if (parseInt(getQty) < parseInt(getHasQty)) {
                var newPrice = (parseInt(getHasQty) - parseInt(getQty)) * parseFloat(getHasPrice);
                //$(".cartListTotal #basketTotal").text(parseFloat(getTotal) - parseFloat(newPrice));

                //$("#cartproductqty_" + mainDivId + "").text(parseInt(getQty));
                //var oldQty = $(".basket-quantity-count").text();
                //$(".basket-quantity-count").text(parseInt(oldQty) - (parseInt(getHasQty) - parseInt(getQty)));
                //$("#setTotalPriceForCart #basketTotal").text(parseFloat(getTotal) - parseFloat(newPrice));
                $(".cart-table-total .basket-total-price").text(parseFloat(getTotal) - parseFloat(newPrice));
                $('.cart-product-code_' + mainDivId + ' input[type="text"].cart-product-quantity-old').val(parseInt(getQty));
                var oldQty = $(".basket-quantity-count").text();
                $(".basket-quantity-count").text(parseInt(oldQty) - (parseInt(getHasQty) - parseInt(getQty)));
                $(".basket-area-main .basket-total-price").text(parseFloat(getTotal) - parseFloat(newPrice));
            }
            //$("#basketTotal").text(parseFloat(getTotal) + parseFloat(getHasPrice));
            $.ajax({
                type: 'POST',
                url: '/Cart/AddToCart',
                contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
                data: { ProductID: mainDivId, Url: getUrl, CartQuantity: getQty },
                success: function (result) {

                },
                error: function () {

                }
            })

        })
        ;
})();