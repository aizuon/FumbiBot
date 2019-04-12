﻿using Dapper.FastCrud;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public static class UserService
    {
        private static IDbConnection _connection => Database.GetCurrentConnection();

        public static async Task<User> FindUser(ulong uid, string name)
        {
            var user = (await _connection.FindAsync<User>(statement => statement
                .Where($"{nameof(User.Uid):C} = @Uid")
                .WithParameters(new { Uid = uid }))).FirstOrDefault();

            if (user == null)
                user = await CreateAndInsertNewUserAsync(uid, name);

            return user;
        }

        private static async Task<User> CreateAndInsertNewUserAsync(ulong uid, string name)
        {
            var newUser = new User
            {
                Uid = uid,
                Name = name,
                Level = 0,
                Exp = 0,
                Pen = 0,
                ProfileTheme = 0
            };

            if (!name.All(char.IsLetterOrDigit))
                newUser.Name = "invalid name";

            await _connection.InsertAsync(newUser);
            return newUser;
        }

        public static async Task<bool> UpdateUserAsync(User user) { return await _connection.UpdateAsync(user); }

        public static async Task<uint> CalculateRankAsync(ulong uid)
        {
            var users = (await Database.GetCurrentConnection().FindAsync<User>()).ToList();
            var rankList = users.OrderByDescending(i => i.Exp).ToList();
            return (uint)rankList.FindIndex(i => i.Uid == uid) + 1;
        }
    }
}