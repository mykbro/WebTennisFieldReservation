﻿namespace WebTennisFieldReservation.Models.Users
{
    public class PasswordResetModel
    {
        public Guid OldSecurityStamp { get; set; }
        public Guid NewSecurityStamp { get; set; }
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
        public int Iters { get; set; }
    }
}
