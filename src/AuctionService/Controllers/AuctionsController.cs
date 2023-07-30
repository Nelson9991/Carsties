using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[Route("api/auctions")]
[ApiController]
public class AuctionsController : ControllerBase
{
  private readonly AuctionDbContext _context;
  private readonly IMapper _mapper;

  public AuctionsController(AuctionDbContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  [HttpGet]
  public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
  {
    var auctions = await _context.Auctions
      .Include(a => a.Item)
      .OrderBy(a => a.Item.Make)
      .ToListAsync();

    return _mapper.Map<List<AuctionDto>>(auctions);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
  {
    var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);

    if (auction == null)
    {
      return NotFound();
    }

    return _mapper.Map<AuctionDto>(auction);
  }

  [HttpPost]
  public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
  {
    var auction = _mapper.Map<Auction>(createAuctionDto);

    // TODO : Add current user as seller
    auction.Seller = "test";

    _context.Auctions.Add(auction);
    var result = await _context.SaveChangesAsync() > 0;

    if (!result)
    {
      return BadRequest("Could not save changes in the Db");
    }

    return CreatedAtAction(
      nameof(GetAuctionById),
      new { id = auction.Id },
      _mapper.Map<AuctionDto>(auction)
    );
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
  {
    var auction = await _context.Auctions.Include(a => a.Item).FirstOrDefaultAsync(a => a.Id == id);

    if (auction == null)
    {
      return NotFound();
    }

    // TODO : Check seller == username

    auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
    auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
    auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
    auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
    auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

    var result = await _context.SaveChangesAsync() > 0;

    if (!result)
    {
      return BadRequest("Could not save changes in the Db");
    }

    return Ok();
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteAuction(Guid id)
  {
    var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id);

    if (auction == null)
    {
      return NotFound();
    }

    // TODO : Check seller == username

    _context.Auctions.Remove(auction);
    var result = await _context.SaveChangesAsync() > 0;

    if (!result)
    {
      return BadRequest("Could not save changes in the Db");
    }

    return Ok();
  }
}
