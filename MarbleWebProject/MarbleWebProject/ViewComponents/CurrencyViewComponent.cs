using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class CurrencyViewComponent : ViewComponent
    {
        private readonly ApiBaseUrl _clientFactory;
        public CurrencyViewComponent(ApiBaseUrl clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public async Task<IViewComponentResult> InvokeAsync(List<CategoryListModel> models)
        {
            //var asad = await _clientFactory.getCagetory(1);
            return View();
        }
    }
}
