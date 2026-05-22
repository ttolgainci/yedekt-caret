using MarbleWebProject.Infrastructure;
using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Helper
{
    public class CmsClient : IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        public TokenResponse getSession()
        {
            SessionCmsHelper _session = new SessionCmsHelper(new HttpContextAccessor());
            var sessionToken = _session.GetSession();
            if (!string.IsNullOrEmpty(sessionToken?.Token))
            {
                return sessionToken;
            }

            return login();
        }
        public TokenResponse login()
        {
            AccountLoginRequest loginRequest = new AccountLoginRequest();
            SessionCmsHelper _session = new SessionCmsHelper(new HttpContextAccessor());

            using (var client = CreateRequest("/AccountManager/login", RestSharp.Method.POST, ""))
            {
                loginRequest.UserName = AppConfig.CMSService.UserName;
                loginRequest.Password = AppConfig.CMSService.Password;
                loginRequest.CustomName = AppConfig.CMSService.CustomName;
                client.AddJsonBody(loginRequest);
                var rtn = client.Execute<TokenResponse>("login");
                _session.SetSession(rtn);
                return rtn;
            }
        }
        public BaseResponse<List<CategoryListModel>> GetCagetory(CategoryRequest request, string token)
        {
            using (var client = CreateRequest("/Category/GetCategory", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<CategoryListModel>>>("GetCategory");
            }
        }
        public BaseResponse<ProductsByCageoryResponse> GetProductByCategory(ProductsByCageoryRequest request, string token)
        {
            using (var client = CreateRequest("/Product/GetProductByCategory", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<ProductsByCageoryResponse>>("GetProductByCategory");
            }
        }
        public BaseResponse<List<CategoryRouteModel>> GetCagetoryRoute(string token)
        {
            using (var client = CreateRequest("/Category/GetCategoryRoute", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                return client.Execute<BaseResponse<List<CategoryRouteModel>>>("GetCategoryRoute");
            }
        }
        public BaseResponse<List<CategoryRouteModel>> GetProductRoute(string token)
        {
            using (var client = CreateRequest("/Product/GetProductRoute", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                return client.Execute<BaseResponse<List<CategoryRouteModel>>>("GetProductRoute");
            }
        }

        public BaseResponse<PlaceOrderResponseDto> PlaceGuestOrder(PlaceGuestOrderRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(AppConfig.StorefrontGuestCheckoutKey))
            {
                var disabled = new BaseResponse<PlaceOrderResponseDto>();
                disabled.Error("Misafir sipariş anahtarı yapılandırılmamış (StorefrontApi:GuestCheckoutKey).");
                return disabled;
            }

            using (var client = CreateRequest("/api/orders/place-guest", RestSharp.Method.POST, string.Empty))
            {
                client.AddHeader("X-Checkout-Key", AppConfig.StorefrontGuestCheckoutKey);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<PlaceOrderResponseDto>>("PlaceGuestOrder");
            }
        }

        public BaseResponse<List<ProductVariantListItemDto>> GetCatalogProductVariants(int productId, string token)
        {
            using (var client = CreateRequest($"/api/catalog/product/{productId}/variants", RestSharp.Method.GET, token))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.AddHeader("Authorization", "Bearer " + token);
                }

                return client.Execute<BaseResponse<List<ProductVariantListItemDto>>>("GetCatalogProductVariants");
            }
        }

        public BaseResponse<List<VehicleMakeListItemDto>> GetCatalogVehicleMakes(string token)
        {
            using (var client = CreateRequest("/api/catalog/vehicle-makes", RestSharp.Method.GET, token))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.AddHeader("Authorization", "Bearer " + token);
                }

                return client.Execute<BaseResponse<List<VehicleMakeListItemDto>>>("GetCatalogVehicleMakes");
            }
        }

        public BaseResponse<List<VehicleModelListItemDto>> GetCatalogVehicleModels(int makeId, string token)
        {
            using (var client = CreateRequest($"/api/catalog/vehicle-makes/{makeId}/models", RestSharp.Method.GET, token))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.AddHeader("Authorization", "Bearer " + token);
                }

                return client.Execute<BaseResponse<List<VehicleModelListItemDto>>>("GetCatalogVehicleModels");
            }
        }

        public BaseResponse<List<VehicleCompatibilityListItemDto>> GetCatalogVehicleCompatibilities(int productId, string token)
        {
            using (var client = CreateRequest($"/api/catalog/product/{productId}/vehicle-compatibilities", RestSharp.Method.GET, token))
            {
                if (!string.IsNullOrEmpty(token))
                {
                    client.AddHeader("Authorization", "Bearer " + token);
                }

                return client.Execute<BaseResponse<List<VehicleCompatibilityListItemDto>>>("GetCatalogVehicleCompatibilities");
            }
        }

        /// <summary>Ürün detayına varyant ve araç uyumluluk listelerini doldurur; API hata verirse sessizce boş bırakır.</summary>
        public void AttachProductCatalogExtras(ProductDetailResponse model, int productId, string token)
        {
            if (model == null || productId <= 0)
            {
                return;
            }

            model.ProductVariants = new List<ProductVariantListItemDto>();
            model.VehicleCompatibilities = new List<VehicleCompatibilityListItemDto>();

            try
            {
                var variants = GetCatalogProductVariants(productId, token);
                if (variants.Status && variants.Data != null)
                {
                    model.ProductVariants = variants.Data;
                }

                var compat = GetCatalogVehicleCompatibilities(productId, token);
                if (compat.Status && compat.Data != null)
                {
                    model.VehicleCompatibilities = compat.Data;
                }
            }
            catch
            {
                // Storefront API veya katalog uçları erişilemezse ürün sayfası yine gösterilir.
            }
        }

        public BaseResponse<ResolveSeoPathResponse> ResolveSeoPath(ResolveSeoPathRequest request, string token)
        {
            using (var client = CreateRequest("/Seo/ResolvePath", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<ResolveSeoPathResponse>>("ResolvePath");
            }
        }

        public BaseResponse<List<SitemapPathItemDto>> GetSitemapProductPaths(string token)
        {
            using (var client = CreateRequest("/Seo/GetSitemapProductPaths", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                return client.Execute<BaseResponse<List<SitemapPathItemDto>>>("GetSitemapProductPaths");
            }
        }

        public BaseResponse<ProductDetailResponse> GetProductDetail(ProductDetailRequest request, string token)
        {
            using (var client = CreateRequest("/Product/GetProductDetail", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<ProductDetailResponse>>("GetProductDetail");
            }
        }
		public BaseResponse<ProductCartInfo> GetProductForCart(ProductCartRequest request, string token)
		{
			using (var client = CreateRequest("/Product/GetProductForCart", RestSharp.Method.POST, token))
			{
				client.AddHeader("Authorization", "Bearer " + token);
				client.AddJsonBody(request);
				return client.Execute<BaseResponse<ProductCartInfo>>("GetProductForCart");
			}
		}
		public BaseResponse<List<ProductBreadcrumbsResponse>> GetProductBreadcrumb(ProductBreadcrumbRequest request, string token)
        {
            using (var client = CreateRequest("/Product/GetProductBreadcrumb", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<ProductBreadcrumbsResponse>>>("GetProductBreadcrumb");
            }
        }
        public BaseResponse<TranslateResponse> GetTranslate(TranslateRequest request, string token)
        {
            using (var client = CreateRequest("/Language/getTranslate", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);

                return client.Execute<BaseResponse<TranslateResponse>>("getTranslate");
            }
        }
        public BaseResponse<List<TranslateAllResponse>> GetTranslateAll(string token)
        {
            using (var client = CreateRequest("/Language/GetTranslateAll", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);


                return client.Execute<BaseResponse<List<TranslateAllResponse>>>("getTranslateAll");
            }
        }
        public BaseResponse<OrderBasket> CreateOrUpdateBasket(OrderBasket request, string token)
        {
            using (var client = CreateRequest("/Basket/CreateOrUpdateBasket", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<OrderBasket>>("CreateOrUpdateBasket");
            }
        }
        public BaseResponse<List<OrderBasket>> GetByIDBasket(BasketRequest request, string token)
        {
            using (var client = CreateRequest("/Basket/GetByIDBaskets", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<OrderBasket>>>("GetByIDBaskets");
            }
        }
        public BaseResponse<List<OrderBasket>> GetBasketAll(BasketAllRequest request, string token)
        {
            using (var client = CreateRequest("/Basket/GetBasketAll", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<OrderBasket>>>("GetBasketAll");
            }
        }
		public BaseResponse<List<OrderBasket>> DeleteProductFromCart(DeleteBasketRequest request, string token)
		{
			using (var client = CreateRequest("/Basket/DeleteProductFromCart", RestSharp.Method.POST, token))
			{
				client.AddHeader("Authorization", "Bearer " + token);
				client.AddJsonBody(request);
				return client.Execute<BaseResponse<List<OrderBasket>>>("DeleteProductFromCart");
			}
		}
		public BaseResponse<List<CategoryBreadcrumbModel>> GetCategoryBreadcrumbAsync(CategoryBreadcrumbRequest request, string token)
		{
			using (var client = CreateRequest("/Category/GetCategoryBreadcrumbAsync", RestSharp.Method.POST, token))
			{
				client.AddHeader("Authorization", "Bearer " + token);
				client.AddJsonBody(request);
				return client.Execute<BaseResponse<List<CategoryBreadcrumbModel>>>("GetCategoryBreadcrumbAsync");
			}
		}
        public BaseResponse<List<CategoryBreadcrumbModel>> GetProductBreadcrumbAsync(ProductBreadcrumbRequest request, string token)
        {
            using (var client = CreateRequest("/Product/GetProductBreadcrumbAsync", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<CategoryBreadcrumbModel>>>("GetProductBreadcrumbAsync");
            }
        }
        public BaseResponse<List<AllBannerResponse>> GetBannerAll(BannerAllRequest request, string token)
        {
            using (var client = CreateRequest("/Banner/GetBannerAll", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<AllBannerResponse>>>("GetBannerAll");
            }
        }
        public BaseResponse<List<LanguageCultureResponse>> GetLanguageCulture(string token)
        {
            using (var client = CreateRequest("/Language/getLanguageCulture", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                return client.Execute<BaseResponse<List<LanguageCultureResponse>>>("GetLanguageCulture");
            }
        }
        public BaseResponse<AllInfoResponse> GetInfoByUrl(InfoPageRequest request, string token)
        {
            using (var client = CreateRequest("/Information/GetInfoByUrl", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<AllInfoResponse>>("GetInfoByUrl");
            }
        }
        public BaseResponse<List<ProductList>> GetProductDetailSimilar(ProductsByCageoryRequest request, string token)
        {
            using (var client = CreateRequest("/Product/GetProductDetailSimilar", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                client.AddJsonBody(request);
                return client.Execute<BaseResponse<List<ProductList>>>("GetProductDetailSimilar");
            }
        }
        public BaseResponse<List<InformationRouteModel>> GetInformationForRoute(string token)
        {
            using (var client = CreateRequest("/Information/GetInformationForRoute", RestSharp.Method.POST, token))
            {
                client.AddHeader("Authorization", "Bearer " + token);
                return client.Execute<BaseResponse<List<InformationRouteModel>>>("GetInformationForRoute");
            }
        }
        private WebRequest CreateRequest(string resource, RestSharp.Method method, string token)
        {
            var client = new WebRequest(resource, method, token);
            client.Endpoint = AppConfig.StorefrontApiBaseUrl;
            client.LogEnable = true;
            var cid = CorrelationIdAmbient.Current;
            if (!string.IsNullOrWhiteSpace(cid))
                client.AddHeader(CorrelationIdDefaults.HeaderName, cid);
            return client;
        }
    }
}
