using System;
using System.Collections.Generic;
using System.Text;

namespace CloudGame.Domain.Model
{
    public class UserCreatedEvent
    {
        public int UserId { get; set; }
        public string? Nome { get; set; } 
        public string? Email { get; set; }

    }
}
