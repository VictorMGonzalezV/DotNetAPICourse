namespace DotnetAPI.Models

{
    public partial class User
    {
        public int UserId{get;set;}
        public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Email{get;set;}
        public string Gender{get;set;}
        public bool Active{get;set;}

        public User()
        {
            //This prevents the non-nullable string fields from exiting the constructor as null
            if(FirstName==null)
            {
                FirstName="";
            }

            if(LastName==null)
            {
                LastName="";
            }

            if(Email==null)
            {
                Email="";
            }

            if(Gender==null)
            {
                Gender="";
            }
        }
    }
}