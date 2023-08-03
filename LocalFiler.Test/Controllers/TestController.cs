using Microsoft.AspNetCore.Mvc;
using Rugal.LocalFiler.Service;

namespace LocalFiler.Test.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class TestController : ControllerBase
{
    private readonly LocalFilerService LocalFilerService;
    public TestController(LocalFilerService _LocalFilerService)
    {
        LocalFilerService = _LocalFilerService;
    }

    [HttpPost]
    public dynamic Test()
    {
        var Buffer = LocalFilerService.ReadFile("test.jpg", new[] { "A" });
        LocalFilerService.SaveFile("test2", Buffer, Item => Item.AddPath("A").Extension = "jpg");
        LocalFilerService.DeleteFile("test2.jpg", new[] { "A" });

        var Buffer2 = LocalFilerService.ReadFile<A>("test.jpg");
        LocalFilerService.SaveFile<A>("test3", Buffer2, Item => Item.Extension = "jpg");
        LocalFilerService.DeleteFile<A>("test3.jpg");
        return true;
    }
}
public class A
{

}
