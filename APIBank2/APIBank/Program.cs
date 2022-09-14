using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace APIBank
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            {
                var bServices = builder.Services;
                // Add services to the container.
                bServices.AddDbContext<postgresContext>();
                //bServices.AddCors();
                bServices.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

                // configure automapper
                bServices.AddAutoMapper(typeof(Program));

                // configure strongly typed settings object
                bServices.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
            
                //Adicionado para DbConnection
                bServices.AddDbContext<postgresContext>(
                        options => {
                            options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"));       options.EnableSensitiveDataLogging(true);
                        });


                bServices.AddDatabaseDeveloperPageExceptionFilter(); // adicionado de tutorial MS https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/intro?view=aspnetcore-6.0&tabs=visual-studio

                // configure DI for application services
                bServices.AddScoped<IJwtUtils, JwtUtils>();
                bServices.AddScoped<IUserService, UserService>();
                bServices.AddScoped<IAccountService, AccountService>();
                bServices.AddScoped<ITransferService, TransferService>();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                bServices.AddEndpointsApiExplorer();
                bServices.AddSwaggerGen(options =>
                {
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Name = "Authorization",
                        Description = "Bearer Authentication with JWT Token",
                        Type = SecuritySchemeType.Http
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new List<string>()
                        }
                    });
                });

                bServices.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });
                bServices.AddAuthorization();
            }
            #region ParaAutenticação
            /*bServices.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Bearer Authentication with JWT Token",
                    Type = SecuritySchemeType.Http
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });*/

            /*bServices.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateActor = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });
            bServices.AddAuthorization();*/
            #endregion

            var app = builder.Build();

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // adicionado para prevenir erro com Timestamps no Postgres

            // Configure the HTTP request pipeline.
            {
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                else // adicionado de tutorial MS https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/intro?view=aspnetcore-6.0&tabs=visual-studio
                {
                    app.UseDeveloperExceptionPage();
                    app.UseMigrationsEndPoint();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();

                //app.UseCors(x => x
                //    .SetIsOriginAllowed(origin => true)
                //    .AllowAnyMethod()
                //    .AllowAnyHeader()
                //    .AllowCredentials());

                // global error handler
                app.UseMiddleware<ErrorHandlerMiddleware>();

                // custom jwt auth middleware
                app.UseMiddleware<JwtMiddleware>();

                app.MapControllers();
            }
            
            app.Run();
        }
    }
}