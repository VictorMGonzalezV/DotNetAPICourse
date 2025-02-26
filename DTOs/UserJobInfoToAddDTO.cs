namespace DotnetAPI.DTOs

{
    public partial class UserJobInfoToAddDTO
    {
     
        public string JobTitle{get;set;}
        public string Department{get;set;}
        
        public UserJobInfoToAddDTO()
        {
            //This prevents the non-nullable string fields from exiting the constructor as null
            if(JobTitle==null)
            {
                JobTitle="";
            }

            if(Department==null)
            {
                Department="";
            }

        }
    }
}