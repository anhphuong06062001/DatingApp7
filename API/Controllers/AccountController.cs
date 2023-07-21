using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interface;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenServeice _tokenServeice;
        private readonly IMapper _mapper;
        public AccountController(DataContext context, ITokenServeice tokenServeice, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _tokenServeice = tokenServeice;
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            if (await UserExits(registerDto.Username)) return BadRequest("Username is taken");

            var user = _mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();

            
            user.UserName = registerDto.Username;
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;
            

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTO {
                Username = user.UserName,
                Token = _tokenServeice.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO) 
        {
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDTO.Username);

            if(user == null) return Unauthorized("invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));
            
            for(var i=0; i < computedHash.Length; i++) {
                if(computedHash[i] != user.PasswordHash[i]) return Unauthorized("invalid password");
            }

            return new UserDTO {
                Username = user.UserName,
                Token = _tokenServeice.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }

        private async Task<bool> UserExits(string username) 
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}