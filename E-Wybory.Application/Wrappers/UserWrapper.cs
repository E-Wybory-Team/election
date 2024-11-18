using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_Wybory.Application.Wrappers
{
    public class UserWrapper
    {
        
        private readonly ClaimsPrincipal? _user;
        public UserWrapper(ClaimsPrincipal user) 
        {
            _user = user;
        }
            
        public string? Username
        {
            get => _user?.FindFirst(c => c.Type.Equals("name"))?.Value;
        }

        public int Id
        {
            get => Convert.ToInt32(_user?.FindFirst(c => c.Type.Equals("IdElectionUser"))?.Value);
        }

        public int IdUserType
        {
            get => Convert.ToInt32(_user?.FindFirst(c => c.Type.Equals("IdUserType"))?.Value);
        }

        public string? Issuer
        {
            get => _user?.FindFirst(c => c.Type.Equals("iss"))?.Value;
        }

        public bool TwoFAenabled
        {
            get => Convert.ToBoolean(_user?.FindFirst(c => c.Type.Equals("TwoFAenabled"))?.Value);
        }

        public bool IsValid()
        {
            if (_user is null || _user.Claims is null || _user.Claims.Count() == 0) return false;

            bool validField = true;

            foreach(Claim claim in _user.Claims)
            {
                if(string.IsNullOrEmpty(claim.Value) || string.IsNullOrEmpty(claim.Type))
                {
                    validField = false;
                    break;
                }
                    
            }

            return validField;
        } 

    }
}
