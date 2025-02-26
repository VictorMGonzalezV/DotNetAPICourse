using System.Text;
using DotNetAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//Added manually to use Swashbuckle, which is not directly supported after .NET 9
builder.Services.AddEndpointsApiExplorer(); // <!-- Add this line
builder.Services.AddSwaggerGen(); // <!-- Add this line

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors((options) =>
    {
        options.AddPolicy("DevCors", (corsBuilder) =>
            {
                //4200 is the default port for React/Angular
                corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        options.AddPolicy("ProdCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("https://myProductionSite.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
    });
//This adds a scoped connection between the interface and the class implementing it. Remember to add ALL builder.Services call before calling Build()!
builder.Services.AddScoped<IUserRepository,UserRepository>();

string? tokenKeyString =builder.Configuration.GetSection("AppSettings:TokenKey").Value;
       
        //Need to account for the nullable quality of strings after .NET 8
        SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                tokenKeyString != null ? tokenKeyString : ""
            )
        );

TokenValidationParameters tokenValidationParameters=new TokenValidationParameters()
{
    /*This validates the token being passed to the API, this setup is not ideal from a security standpoint, but can be acceptable during development,
     setting these parameters to false enables external applications such as Postman to*/
    IssuerSigningKey=tokenKey,
    ValidateIssuer=false,
    ValidateIssuerSigningKey=false,
    ValidateAudience=false
};

//Here we implement the Bearer token scheme and pass the  symetric key as part of the validation parameters created before
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options=>
{
    options.TokenValidationParameters=tokenValidationParameters;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Adding a CORS policy isn't enough, it has to be implemented with this method
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

/*// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{    //Disabled so we use a controller instead of OpenAPI mapping
    //app.MapOpenApi();
    //Added manually to have Swagger UI from Swashbuckle
    app.UseSwagger(); // <!-- Add this line
    app.UseSwaggerUI(); // <!-- Add this line
}
else
{
    //it's almost never necessary to use HTTPS during development
    app.UseHttpsRedirection();

}*/

app.MapControllers();


//Authentication must be ALWAYS before Authorization, else the tokens won't work properly, but the program won't give error messages, just 401 unauthorized responses

app.UseAuthentication();

app.UseAuthorization();

app.Run();

