﻿using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServerLibrary.Repositories.Contracts;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserAccount accountInterface) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> CreateAsync(Register user)
        {
            if (user == null) return BadRequest("model is empty");

            var result = await accountInterface.CreateAsync(user);
            return Ok(result);

        }

        [HttpPost("login")]
        public async Task<IActionResult> SignInAsync(Login user)
        {
            if (user == null) return BadRequest("model is empty");

            var result = await accountInterface.SignInAsync(user);
            return Ok(result);

        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshToken refreshToken)
        {
            if (refreshToken == null) return BadRequest("model is empty");

            var result = await accountInterface.RefreshTokenAsync(refreshToken);
            return Ok(result);

        }
    }
}
