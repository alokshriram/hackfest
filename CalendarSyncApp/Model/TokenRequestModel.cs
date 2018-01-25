using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace service_pulse.Model
{
    public class TokenRequestModel
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string GrantType { get; set; }
        public string Scopes { get; set; }
    }
}