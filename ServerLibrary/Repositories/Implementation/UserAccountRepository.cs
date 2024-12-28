using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repositories.Contracts;
using System.Reflection.Metadata;
using Constant = ServerLibrary.Helpers.Constants;

namespace ServerLibrary.Repositories.Implementation
{
    public class UserAccountRepository(IOptions<JwtSection> config, AppDbcontext appDbcontext) : IUserAccount
    {
        public async Task<GeneralResponse> IUserAccount.CreateAsync(Register user)
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
                    RoleId = response.Id,
                    UserId = applicationUser.Id
                });
            }
            return new GeneralResponse(true, "Account created!");
        }

        Task<LoginResponse> IUserAccount.SignInAsync(Login user)
        {
            throw new NotImplementedException();
        }

        private async Task<ApplicationUser> FindUserByEmail(string email) =>
            await appDbcontext.ApplicationUser.FirstOrDefaultAsync(_ => _.Email!.ToLower()!.Equals(email!.ToLower()));
        private async Task<T> AddToDatabase<T>(T model)
        {
            var result = appDbcontext.Add(model!);
            await appDbcontext.SaveChangesAsync();
            return (T)result.Entity;
        }


    }
}
