﻿namespace TehGM.EinherjiBot.Security.API
{
    public interface IJwtGenerator
    {
        string Generate(UserSecurityData userData);
    }
}
