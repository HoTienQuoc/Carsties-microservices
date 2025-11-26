using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
        var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();
        if (!string.IsNullOrEmpty(date))
        {
            query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }
        return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
        //_context là DbContext, đối tượng quản lý kết nối và thao tác với cơ sở dữ liệu.
        //Auctions là DbSet trong DbContext, đại diện cho bảng Auctions trong database.
        //Nghĩa là: mình đang truy xuất tất cả các bản ghi trong bảng Auctions.
        //Include: EF Core sẽ tạo SQL JOIN giữa Auctions và Items.
        // var auctions = await _context.Auctions
        //     .Include(x => x.Item)
        //     .OrderBy(x => x.Item.Make)
        //     .ToListAsync();
        // var auctionDtos = _mapper.Map<List<AuctionDto>>(auctions);
        // return auctionDtos;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        var auctionDto = _mapper.Map<AuctionDto>(auction);
        return auctionDto;
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
        //- Sử dụng **AutoMapper** để chuyển (map):
        //  - `CreateAuctionDto` → `Auction` (entity).
        //-DTO dùng cho request, còn entity dùng để lưu vào database.
        var auction = _mapper.Map<Auction>(auctionDto);

        // TODO: add current user as seller
        auction.Seller = "test";

        //- Thêm entity `auction` vào DbSet để EF Core theo dõi.
        //- Chưa gửi vào database — chỉ **track**.
        _context.Auctions.Add(auction);

        var newAuction = _mapper.Map<AuctionDto>(auction);

        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        //Lưu các thay đổi vào database.
        //SaveChangesAsync() trả về số bản ghi bị ảnh hưởng.
        //Nếu > 0 → lưu thành công (result = true).
        var result = await _context.SaveChangesAsync() > 0;



        //- Nếu lưu thất bại → trả về lỗi HTTP 400.
        if (!result)
        {
            return BadRequest("Could not save changes to the DB");
        }


        //🎯 CreatedAtAction — dùng để trả về 201 Created
        // Khi tạo thành công một resource mới(ví dụ tạo Auction), chuẩn REST khuyến nghị trả về:
        // HTTP 201 Created
        // Header Location: chứa đường dẫn tới resource vừa tạo
        // Body: chứa object vừa tạo
        return CreatedAtAction(
            //Đây là tên của action mà API sẽ sử dụng để tạo URL.
            //Action này thường là: public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
            nameof(GetAuctionById),

            //Đây là route values — các giá trị để lắp vào route của action GetAuctionById.
            new { auction.Id },

            //Đây là body trả về cho client.
            //Chuyển Entity → DTO để client không thấy dữ liệu nhạy cảm.
            //Trả về nội dung của Auction vừa tạo.
            newAuction
        );


    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        //Todo: check seller == username
        //🧠 Điểm quan trọng nhất: EF Core tracking
        //Khi bạn thay đổi giá trị:
        //EF Core sẽ ghi nhận:
        //“Item này đã bị sửa đổi”
        //Khi bạn gọi:
        // EF Core sẽ tạo SQL Update

        auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            return Ok();
        }

        return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions.FindAsync(id);

        if (auction == null)
        {
            return NotFound();
        }

        //Todo: check seller == username
        _context.Auctions.Remove(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _context.SaveChangesAsync() > 0;
        if (result)
        {
            return Ok();
        }
        return BadRequest("Could not updating DB");
    }

}
