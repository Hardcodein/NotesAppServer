﻿namespace DataAccess.Models;

public class SessionUserModel
{
    public Guid Id { get; set; }
    public Guid Token_Id { get; set; }
    public virtual JwtTokenModel? JwtToken { get; set; }

}