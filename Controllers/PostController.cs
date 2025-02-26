using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotNetAPI.DTOs;
using DotNetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

namespace DotNetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]

    public class PostController: ControllerBase
    {
       private readonly DataContextDapper _dapper;

       public PostController (IConfiguration config)
       {
        _dapper=new DataContextDapper(config);

       }

       [HttpGet("Posts/{postId}/{userId}/{searchValue}")]

       public IEnumerable<Post> GetPosts(int postId=0, int userId=0, string searchValue="None")
       {
        string sql=@"EXEC TutorialAppSchema.spPosts_Get";
        string stringParameters="";
        DynamicParameters sqlParameters=new DynamicParameters();

        if(postId!=0)
        {
            stringParameters+=", @PostId=@PostIdParam";
            sqlParameters.Add("@PostIdParam",postId,DbType.Int32);
        }

        if(userId!=0)
        {
            stringParameters+=", @UserId=@UserIdParam";
            sqlParameters.Add("@UserIdParam",userId,DbType.Int32);
        }

        if(searchValue!="None")
        {
            stringParameters+=", @SearchValue=@SearchValueParam";
            sqlParameters.Add("@SearchValueParam",searchValue,DbType.String);
        }

        if(stringParameters.Length>0)
        {
             sql+=stringParameters.Substring(1);
        }
  
        return _dapper.LoadDataWithParams<Post>(sql,sqlParameters);
       }

       [HttpGet("MyPosts")]
       public IEnumerable<Post> GetMyPosts()
       {
        //It's not mandatory to make the User reference explicit with this.User,in this case the User being called is the one inherited from ControllerBase
        //FindFirst will automatically pull the id from the user whose token is in use, simply adding the [Authorize] attribute to the controller makes this functionality available
             string sql=@"EXEC TutorialAppSchema.spPosts_Get @UserId= @UserIdParam";
             int userId;
             DynamicParameters sqlParameters=new DynamicParameters();
             sqlParameters.Add("@UserIdParam",int.TryParse(this.User.FindFirst("userId")?.Value,out userId),DbType.Int32);

        return _dapper.LoadDataWithParams<Post>(sql,sqlParameters);
       }

       [HttpPut("UpsertPost")]

       public IActionResult UpsertPost(Post postToUpsert)
       {
            string sql=@"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId=@UserIdParam
                , @PostTitle=@PostTitleParam
                , @PostContent=@PostContentParam";

            DynamicParameters sqlParameters=new DynamicParameters();

            sqlParameters.Add("@UserIdParam",this.User.FindFirst("userId")?.Value,DbType.Int32);
            sqlParameters.Add("@PostTitleParam",postToUpsert.PostTitle,DbType.String);
            sqlParameters.Add("@PostContentParam",postToUpsert.PostContent,DbType.String);
            
            if(postToUpsert.PostId>0)
            {
                sql+=", @PostId=@PostIdParam";
                sqlParameters.Add("@PostIdParam",postToUpsert.PostId,DbType.Int32);
            }
            
            if(_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Adding of post is no");
       }

       [HttpDelete("Post/{postId}")]

       public IActionResult DeletePost(int postId)
       {
            string sql=@"EXEC TutorialAppSchema.spPost_Delete @PostId=@PostIdParam
            , @UserId= @UserIdParam";

            DynamicParameters sqlParameters=new DynamicParameters();
            sqlParameters.Add("@PostIdParam",postId,DbType.Int32);
            sqlParameters.Add("@UserIdParam",this.User.FindFirst("userId")?.Value,DbType.Int32);
            if(_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
            {
                return Ok();
            }
            throw new Exception("Cannot into delete post");
       }
    }
}