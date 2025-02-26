namespace DotNetAPI.DTOs
{
    public partial class PostToEditDTO
    {
        public int PostId {get; set;}
        
        public string PostTitle {get; set;}
        
        public string PostContent {get; set;}
        
        public DateTime PostUpdated {get; set;}

        public PostToEditDTO()
        {
            if(PostTitle==null)
            {
                PostTitle="";
            }

            if(PostContent==null)
            {
                PostContent="";
            }
        }
    }
}