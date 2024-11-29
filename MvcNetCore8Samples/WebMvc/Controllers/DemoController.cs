using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMvc.Domains;
using WebMvc.Interfaces;

namespace WebMvc.Controllers;

[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private readonly IRepository<ParentItem> _parentRes;
    private readonly IUnitOfWork _unitOfWork;

    public DemoController(IRepository<ParentItem> parentRes,
        IUnitOfWork unitOfWork)
    {
        _parentRes = parentRes;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("parent")]
    public async Task<IActionResult> AddParentItemAsync([FromBody] ParentItem parentItem)
    {
        await _parentRes.AddAsync(parentItem);
        await _unitOfWork.SaveChangesAsync();
        return Ok(parentItem);
    }

    [HttpPut("parent/{itemId}")]
    public async Task<IActionResult> UpdateParentItemAsync(int itemId, [FromBody] ParentItem parentItem)
    {
        var item = await _parentRes.QueryTracking(x => x.Id == itemId).SingleOrDefaultAsync();
        if (item == null)
        {
            return NotFound();
        }

        item.Name = parentItem.Name;
        item.RowVersion = parentItem.RowVersion;
        await _unitOfWork.SaveChangesAsync();
        return Ok(item);
    }

    [HttpGet("parent/{itemId}")]
    public async Task<IActionResult> GetParentItemAsync(int itemId)
    {
        var item = await _parentRes.Query(x => x.Id == itemId).SingleOrDefaultAsync();
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
