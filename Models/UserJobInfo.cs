namespace DotnetAPI.Models

{
    public partial class UserJobInfo
    {
        public int UserId{get;set;}
        public string JobTitle{get;set;}
        public string Department{get;set;}
        
        public UserJobInfo()
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