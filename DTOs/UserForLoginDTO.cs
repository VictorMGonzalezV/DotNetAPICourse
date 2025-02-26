using System.ComponentModel.DataAnnotations;
using Microsoft.Net.Http.Headers;

namespace DotNetAPI.DTOs

{
    public partial class UserForLoginDTO
    {
        public string Email{get; set;}

        public string Password{get; set;}

        public UserForLoginDTO()
        {
            if(Email==null)
            {
                Email="";
            }

            if(Password==null)
            {
                Password="";
            }

        }
    }
}