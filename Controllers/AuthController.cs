using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;
using DotNetAPI.DTOs;
using DotNetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetAPI.Controllers
{
    //The Authorize attribute applies to all endpoints defined in this controller
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly AuthHelper _authHelper;
        private readonly DataContextDapper _dapper;
        private readonly ReusableSql _reusableSql;
        private readonly Mapper _mapper;

        public AuthController(IConfiguration config)
        {
            _dapper=new DataContextDapper(config);
            _authHelper=new AuthHelper(config);
            _reusableSql=new ReusableSql(config);
            _mapper=new Mapper(new MapperConfiguration(cfg=>{
                cfg.CreateMap<UserForRegistrationDTO,UserComplete>();
            }));
        }
    
    //This attribute allows unregistered users to access this endpoint
    [AllowAnonymous]
    [HttpPost("Register")]

    public IActionResult Register(UserForRegistrationDTO userForRegistration)
    {
        if(userForRegistration.Password==userForRegistration.PasswordConfirm)
        {
            string sqlCheckUserExists="SELECT Email FROM TutorialAppSchema.Auth WHERE Email= '"+userForRegistration.Email+"'";

            IEnumerable<string> existingUsers=_dapper.LoadData<string>(sqlCheckUserExists);

            if(existingUsers.Count()==0)
            {  
                UserForLoginDTO userForSetPassword= new UserForLoginDTO(){
                    Email=userForRegistration.Email,
                    Password=userForRegistration.Password
                }; 
                
              
                if(_authHelper.SetPassword(userForSetPassword))
                {
                    //By mapping the userForRegistration into a UserComplete, we can send it to the Upsert method using the ReusableSql object, 
                    //instead of duplicating the logic here in the AuthController and using a clunky sql proc setup with plenty of opening and closing of strings

                    UserComplete userComplete=_mapper.Map<UserComplete>(userForRegistration);
                    //The userForRegistrationDTO doesn't have an Active property, but the actual UserComplete model does, after the mapping, we can add it directly
                    userComplete.Active=true;

                    /*string sqlAddUser=@"EXEC TutorialAppSchema.spUser_Upsert
                        @FirstName='"+userForRegistration.FirstName+
                    "', @LastName ='"+userForRegistration.LastName+
                    "', @Email ='"+userForRegistration.Email+
                    "', @Gender ='"+userForRegistration.Gender+
                    "', @Active =1"+
                    ",  @JobTitle ='"+userForRegistration.JobTitle+
                    "', @Department ='"+userForRegistration.Department+
                    "', @Salary ='"+userForRegistration.Salary+                                
                    "'";
                            
                    string sqlAddUser=@"INSERT INTO TutorialAppSchema.Users(
                                        [FirstName],
                                        [LastName],
                                        [Email],
                                        [Gender],
                                        [Active]
                                        ) VALUES ("+
                                        "'"+userForRegistration.FirstName+
                                        "','"+userForRegistration.LastName+
                                        "','"+userForRegistration.Email+
                                        "','"+userForRegistration.Gender+
                                        "',1)";*/

                    if(_reusableSql.UpsertUser(userComplete))
                    {
                        return Ok();
                    }
                    throw new Exception("Adding of user is fail");
                    
                }

                throw new Exception("Registering of user is no");

                 
            }

            throw new Exception("User email is of existing!");
           
        }

        throw new Exception("Passwords not do matchings!");
        
    }
    
    //Don't forget to make the login endpoint allow anonymous!
    [AllowAnonymous]
    [HttpPost("Login")]
    
    public IActionResult Login(UserForLoginDTO userForLogin)
    {
        string sqlForHashAndSalt=@"EXEC TutorialAppSchema.spLoginConfirmation_Get
            @Email= @EmailParam";

        //After creating the list, we declare them in the code, and assign the desired variables as their values

        /*SqlParameter emailParameter=new SqlParameter("@EmailParam",SqlDbType.VarChar);
        emailParameter.Value=userForLogin.Email;
        sqlParameters.Add(emailParameter);*/

        //The List<sqlParameter> method does NOT work if Dapper runs a Query/QuerySingle instruction, in that case wee need to use a DynamicParameters object
        DynamicParameters sqlParameters=new DynamicParameters();
        sqlParameters.Add("@EmailParam",userForLogin.Email,DbType.String);

        UserForLoginConfirmationDTO userForLoginConfirmationDTO=_dapper.LoadDataSingleWithParams<UserForLoginConfirmationDTO>(sqlForHashAndSalt,sqlParameters);

        byte[] passwordHash=_authHelper.GetPasswordHash(userForLogin.Password,userForLoginConfirmationDTO.PasswordSalt); 
        //if(passwordHash==userForConfirmation.PasswordHash)...yeah,nah, it won't work because the password hashes are objects
        for(int index=0;index<passwordHash.Length;index++)
        {
            if(passwordHash[index]!=userForLoginConfirmationDTO.PasswordHash[index])
            {
                return StatusCode(401,"Password cannot into match");
            }
        }
        
        string userIdSql=@"SELECT UserId FROM TutorialAppSchema.Users WHERE Email='"+
                    userForLogin.Email+"'";

        int userId=_dapper.LoadDataSingle<int>(userIdSql);          
        return Ok(new Dictionary<string,string>{

            {"token",_authHelper.CreateToken(userId)}
        });
    }

    [HttpGet("RefreshToken")]

    public IActionResult RefreshToken()
    {
        //Adding an empty string at the end is a quick and easy way to prevent a null value from exiting the method
        string userId= User.FindFirst("userId")?.Value+"";

        string userIdSql="SELECT UserId FROM TutorialAppSchema.Users WHERE UserId= "+ userId;

        int userIdFromDB=_dapper.LoadDataSingle<int>(userIdSql);

        return Ok(new Dictionary<string,string>{
            {"token",_authHelper.CreateToken(userIdFromDB)}
        });
    }

    [HttpPut("ResetPassword")]
    public IActionResult ResetPassword(UserForLoginDTO userForSetPassword)
    {
        if(_authHelper.SetPassword(userForSetPassword))
        {
            return Ok();
        }
        throw new Exception("Cannot make reset of password");
    }

    }
}