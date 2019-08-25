using System;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.DataModels.Permits
{
    public class NetflixPermitInfo : PermitInfo
    {
        [JsonProperty("login")]
        public string Login { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }

        protected override void AddFieldsToEmbed(ref EmbedBuilder embed)
        {
            embed
                .AddField("Login", Login)
                .AddField("Password", Password)
                .WithThumbnailUrl("https://historia.org.pl/wp-content/uploads/2018/04/netflix-logo.jpg");
        }

        protected override UpdateResult UpdateData(SocketCommandContext message, Match match)
        {
            SetMode mode = StringToSetMode(match.Groups[1].Value);
            string value = match.Groups[2].Value;
            if (mode == SetMode.Login)
            {
                this.Login = value;
                return new UpdateResult(true, $"You have set Netflix account login to `{value}`.");
            }
            if (mode == SetMode.Password)
            {
                this.Password = value;
                return new UpdateResult(true, $"You have set Netflix account password to `{value}`.");
            }
            return new UpdateResult(false, $"Incorrect param {match.Groups[1].Value}.");
        }

        private static SetMode StringToSetMode(string value)
        {
            switch (value.ToLower())
            {
                case "login":
                case "email":
                case "username":
                    return SetMode.Login;
                case "password":
                case "pass":
                case "pwd":
                    return SetMode.Password;
                default:
                    throw new ArgumentException($"Invalid {nameof(SetMode)} \"{value.ToLower()}\"");
            }
        }

        private enum SetMode
        {
            Password,
            Login
        }
    }
}
