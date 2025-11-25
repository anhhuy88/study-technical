using Microsoft.AspNetCore.Mvc;

namespace DemoAlantisTheme.Areas.Customer.Components
{
    public class DemoViewComponent: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var data = ("Hello", "World");
            return View(data);
        }
    }
}
