using Microsoft.AspNetCore.Mvc;
using Rugal.LocalFiler.Service;

namespace LocalFiler.Test.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class TestController : ControllerBase
{
    private readonly Rugal.LocalFiler.Service.FilerService Filer;
    public TestController(Rugal.LocalFiler.Service.FilerService _Filer)
    {
        Filer = _Filer;
    }

    [HttpPost]
    public dynamic Test(IFormFile File)
    {
        var Buffer = Filer.ReadFile("test.jpg", new[] { "A" });
        Filer.SaveFile("test2", Buffer, Item => Item.AddPath("A").Extension = "jpg");
        Filer.DeleteFile("test2.jpg", new[] { "A" });

        var Buffer2 = Filer.ReadFile<A>("test.jpg");
        Filer.SaveFile<A>("test3", Buffer2, Item => Item.Extension = "jpg");
        Filer.DeleteFile<A>("test3.jpg");

        Filer.SaveFile("test4", File);
        return true;
    }
}
public class A
{

}
