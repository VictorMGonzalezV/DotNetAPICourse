using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using DotnetAPI.Data;
using DotNetAPI.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace DotNetAPI.Helpers
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;

        private readonly DataContextDapper _dapper;
        public AuthHelper(IConfiguration config)
        {
            _dapper=new DataContextDapper(config);
            _config=config;
        }

    
        public byte[] GetPasswordHash(string password,byte[] passwordSalt)
            {
                //By appending the PasswordKey string from the app settings, we make it even harder for hackers to damage the data, even if they had access to the DB itself
                        //The PasswordKey is only stored in the app settings so it cannot be retrieved from the DB itself
                        string passwordSaltPlusKey=_config.GetSection("AppSettings:PasswordKey").Value+Convert.ToBase64String(passwordSalt);

                    return  KeyDerivation.Pbkdf2(
                            password: password,
                            salt: Encoding.ASCII.GetBytes(passwordSaltPlusKey),
                            prf: KeyDerivationPrf.HMACSHA256,
                            iterationCount: 100000,
                            numBytesRequested:256/8
                        );
            }

            public string CreateToken(int userId)
            {
                Claim[] claims=new Claim[]{
                    new Claim("userId",userId.ToString())
                };

                string? tokenKeyString = _config.GetSection("AppSettings:TokenKey").Value;
            
                //Need to account for the nullable quality of strings after .NET 8
                SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        tokenKeyString != null ? tokenKeyString : ""
                    )
                );
                
                SigningCredentials credentials=new SigningCredentials(tokenKey,SecurityAlgorithms.HmacSha512Signature);

                SecurityTokenDescriptor descriptor=new SecurityTokenDescriptor()
                {
                Subject= new ClaimsIdentity(claims),
                SigningCredentials=credentials,
                Expires=DateTime.Now.AddDays(1)
                };
                //This class contains the functionality to turn the descriptor into an actual token
                JwtSecurityTokenHandler tokenHandler= new JwtSecurityTokenHandler();

                //This token object is not ready to use, it needs to be turned into a string for portability
                SecurityToken token=tokenHandler.CreateToken(descriptor);

                return tokenHandler.WriteToken(token);
            }

            public bool SetPassword(UserForLoginDTO userForSetPassword)
            {
                //This block implements 128-bit encryption
                byte[] passwordSalt=new byte[128/8];
                using(RandomNumberGenerator rng=RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }

                //By appending the PasswordKey string from the app settings, we make it even harder for hackers to damage the data, even if they had access to the DB itself
                //The PasswordKey is only stored in the app settings so it cannot be retrieved from the DB itself
                string passwordSaltPlusKey=_config.GetSection("AppSettings:PasswordKey").Value+Convert.ToBase64String(passwordSalt);

                byte[] passwordHash=GetPasswordHash(userForSetPassword.Password,passwordSalt);
                
                //Adding SQL parameters is not straightforward, first they are declared in the query with @ParamName, then we need to create a list to store them, define them
                string sqlAddAuth=@"EXEC TutorialAppSchema.spRegistration_Upsert @Email=@EmailParam
                , @PasswordHash= @PasswordHashParam
                , @PasswordSalt= @PasswordSaltParam";
                
                /*List<SqlParameter> sqlParameters=new List<SqlParameter>();
                //After creating the list, we declare them in the code, and assign the desired variables as their values

                SqlParameter emailParameter=new SqlParameter("@EmailParam",SqlDbType.VarChar);
                emailParameter.Value=userForSetPassword.Email;

                SqlParameter passwordSaltParameter=new SqlParameter("@PasswordSaltParam",SqlDbType.VarBinary);
                passwordSaltParameter.Value=passwordSalt;

                SqlParameter passwordHashParameter=new SqlParameter("@PasswordHashParam",SqlDbType.VarBinary);
                passwordHashParameter.Value=passwordHash;
                //Then we add them to the list
                sqlParameters.Add(emailParameter);
                sqlParameters.Add(passwordSaltParameter);
                sqlParameters.Add(passwordHashParameter);*/

                //Dapper expects DbType.Binary for VARBINARY
                DynamicParameters sqlParameters=new DynamicParameters();
                sqlParameters.Add("@EmailParam",userForSetPassword.Email,DbType.String);
                sqlParameters.Add("@PasswordHashParam",passwordHash,DbType.Binary);
                sqlParameters.Add("@PasswordSaltParam",passwordSalt,DbType.Binary);
                
              
                return _dapper.ExecuteSqlWithParameters(sqlAddAuth,sqlParameters);
                
            }
    }
}