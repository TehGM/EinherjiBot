﻿namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeAdminAttribute : DiscordAuthorizationAttribute
    {
        public AuthorizeAdminAttribute() : base(typeof(AuthorizeAdmin)) { }
    }
}
