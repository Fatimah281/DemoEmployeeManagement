using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Constant = ServerLibrary.Helpers.Constants;

namespace ServerLibrary.Repositories.Implementation
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbcontext appDbcontext) : IUserAccount
    {
        public async Task<GeneralResponse> CreateAsync(Register user)
        {
            if (user is null) return new GeneralResponse(false, "Model is empty");
            var checkUser = await FindUserByEmail(user.Email!);
            if (checkUser != null) return new GeneralResponse(false, "User registered already");

            //SAVE user
            var applicationUser = await AddToDatabase(new ApplicationUser()
            {
                Fullname = user.FullName,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            });

            //check, create and assign role
            var checkAdminRole = await AddToDatabase(new SystemRole() { Name = Constant.Admin });
            if (checkAdminRole != null)
            {
                var createAdminRole = await AddToDatabase(new SystemRole() { Name = Constant.Admin });
                await AddToDatabase(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationUser.Id });
                return new GeneralResponse(true, "Account created!");
            }
            var checkUserRole = await appDbcontext.SystemRole.FirstOrDefaultAsync(_ => _.Name!.Equals(Constants.User));
            SystemRole response = new();
            if (checkUserRole is null)
            {
                response = await AddToDatabase(new SystemRole()
                {
                    Name = Constant.User,
                });
                await AddToDatabase(new UserRole()
                {
                    RoleId = response.Id,
                    UserId = applicationUser.Id
                });
            }
            else
            {
                await AddToDatabase(new UserRole()
                {
                    RoleId = checkUserRole.Id,
                    UserId = applicationUser.Id
                });
            }
            return new GeneralResponse(true, "Account created!");
        }

        public async Task<LoginResponse> SignInAsync(Login user)
        {
            if (user is null) return new LoginResponse(false, "model is empty ");
            var applicationUser = await FindUserByEmail(user.Email!);
            if (applicationUser is null) return new LoginResponse(false, "user not found!");

            //verfiy password 
            if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
                return new LoginResponse(false, "Email/Password not valid");

            var getUserRole = await FindUserRole(applicationUser.Id);
            if (getUserRole is null) return new LoginResponse(false, "User Role Not Found");

            var getRoleName = await FindRoleName(getUserRole.RoleId);
            if (getUserRole is null) return new LoginResponse(false, "User Role Not Found");

            //user is vailed we have to create a token 
            string jwtToken = GenerateToken(applicationUser, getRoleName!.Name!);
            string refreshToken = GenerateRefreshToken();
            //store the token for user in db & return it in response to to store it in client locally 
            //سيصبح كمفتاح لليوزر على انه مخول في الدخول الى الداتا بيز واجراء اي تعديلات تخص حسابه 

            var findUser = await appDbcontext.RefreshTokenInfo.FirstOrDefaultAsync(_ => _.UserId == applicationUser.Id);
            if (findUser is not null)
            {
                findUser!.Token = refreshToken;
                await appDbcontext.SaveChangesAsync();
            }
            else
            {
                await AddToDatabase(new refreshTokenInfo()
                {
                    Token = refreshToken,
                    UserId = applicationUser.Id,
                });
            }
            if (getUserRole is null) return new LoginResponse(false, "User Role Not Found");

            return new LoginResponse(true, "Login successfully", jwtToken, refreshToken);
            ;
        }

        private string GenerateToken(ApplicationUser user, string role)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Fullname!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, role!),

            };

            var token = new JwtSecurityToken(
                issuer: config.Value.Issuer,
                audience: config.Value.Audience,
                claims: userClaims,
                expires: DateTime.Now.AddSeconds(2),
                signingCredentials: credentials

                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<UserRole> FindUserRole(int userId) => await appDbcontext.UserRole.FirstOrDefaultAsync(_ => _.UserId == userId);
        private async Task<SystemRole> FindRoleName(int roleId) => await appDbcontext.SystemRole.FirstOrDefaultAsync(_ => _.Id == roleId);
        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        private async Task<ApplicationUser> FindUserByEmail(string email) =>
            await appDbcontext.ApplicationUser.FirstOrDefaultAsync(_ => _.Email!.ToLower()!.Equals(email!.ToLower()));
        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbcontext.Add(model!);
            await appDbcontext.SaveChangesAsync();
            return (T)result.Entity;
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
        {
            if (token is null) return new LoginResponse(false, "Model is empty");
            var findToken = await appDbcontext.RefreshTokenInfo.FirstOrDefaultAsync(_ => _.Token!.Equals(token.Token));
            if (findToken is null) return new LoginResponse(false, "refresh token is required");

            //get user details
            var user = await appDbcontext.ApplicationUser.FirstOrDefaultAsync(_ => _.Id == findToken.UserId);
            if (user is null) return new LoginResponse(false, "refresh token cant generated because user not found");

            var userRole = await FindUserRole(user.Id);
            var roleName = await FindRoleName(userRole.RoleId);
            string jwtToken = GenerateToken(user, roleName.Name!);
            string refreshToken = GenerateRefreshToken();

            var updatRefreshToken = await appDbcontext.RefreshTokenInfo.FirstOrDefaultAsync(_ => _.UserId == user.Id);
            if (updatRefreshToken is null) return new LoginResponse(false, "refresh token cant generated because user has not signIn");

            updatRefreshToken.Token = refreshToken;
            await appDbcontext.SaveChangesAsync();
            return new LoginResponse(true, "Token refreshed successfully!", jwtToken, refreshToken);




        }
    }
}
