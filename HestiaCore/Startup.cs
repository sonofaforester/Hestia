using HestiaDatabase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HestiaCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            services.AddDbContext<HestiaCoreContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddAuthentication().AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options => {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = RetreiveCognitoSigningKeys(),
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["Cognito:Authority"],
                        ValidateAudience = false,
                        ValidAudiences = new[] { "1qrnki8m3p5eagj2jggltphccb" },
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),
                        AudienceValidator = JwtAudienceValidation,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = UpdateValidatedCognitoToken,
                    };
                });

            services.AddSwaggerGen(c => {
                c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Scheme = "bearer"
                });
                
                c.OperationFilter<AuthenticationRequirementsOperationFilter>();
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Bearer", policy =>
                {
                    policy.AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });
                

            services.AddWorkflow(Configuration);
        }

        public class AuthenticationRequirementsOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Security == null)
                    operation.Security = new List<OpenApiSecurityRequirement>();


                var scheme = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" } };
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [scheme] = new List<string>()
                });
            }
        }

        private Task UpdateValidatedCognitoToken(TokenValidatedContext context)
        {
            var identity = context.Principal.Identity as ClaimsIdentity;

            
            //var userManager = context
            //    .HttpContext
            //    .RequestServices
            //    .GetRequiredService<ApplicationUserManager>();

            //// The Cognito username will be in one of two forms;
            //// - If the user is federated through an IDP, it will be the user's IDP email address
            ////   prefixed with the IDP name;
            ////   `SomeIDP_my@email.addr`, or `KCFAzureAD_dpirrone-brusse@kcftech.com`.
            //// - If the user is held in Cognito, it will be anything **but** the user's email
            ////   address and will be exactly equal to the user's `identity.users.user_name`. Most
            ////   commonly, this is a GUID or a user-selected string. We can make no guarantees about
            ////   the shape of these strings; only that `cognito.username` and
            ////   `identity.users.user_name` will be the same.
            //// When users are federated though an IDP, their token will include the `cognito:groups`
            //// claim. Use that to determine which method to use when querying for the related user.
            //var username = identity.Claims.First(c => c.Type == "username").Value;
            //ApplicationUser user;
            //if (identity.Claims.Where(c => c.Type == "cognito:groups").Any())
            //{
            //    // TODO: At present, we assume IDP names will never include an `_`, and just strip
            //    //       `.*?_` from the username to find the user's email address. We could instead
            //    //       lift the name from the value of the cognito:groups claim -- those look like
            //    //       `us-east-1_UfgeHWojR_KCFAzureAD`.
            //    var email = username.Substring(username.IndexOf('_') + 1);
            //    user = await userManager.FindByEmailAsync(email);
            //}
            //else
            //{
            //    user = await userManager.FindByNameAsync(username);
            //}

            //if (user == null)
            //{
            //    throw new Exception("Bearer token identified a non-existant user");
            //}

            //// ASP.NET Identity populates the NameIdentifier claim with the cognito sub claim. We
            //// want to use our PSQL identity.users.id primary key for this claim.
            //identity.TryRemoveClaim(
            //    identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier));

            //// Add the set of claims KCF expects to the ResultContext Principal.
            //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            identity.AddClaim(new Claim("name", identity.Claims.First(c => c.Type == "username").Value));
            
            return Task.CompletedTask;
            //identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        }
    

        private bool JwtAudienceValidation(
            IEnumerable<string> audiences,
            SecurityToken securityToken,
            TokenValidationParameters validationParameters)
        {
            var token = securityToken as JwtSecurityToken;
            var clientId = token.Claims
                .Where(c => c.Type == "client_id")
                .FirstOrDefault();

            if (clientId == null)
            {
                throw new Exception("JWT Audience Validation requested for token with no client_id claim");
            }

            return validationParameters.ValidAudiences.Any(a => a.Contains(clientId.Value));
        }

        private IEnumerable<SecurityKey> RetreiveCognitoSigningKeys()
        {
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                Configuration["Cognito:MetadataAddress"],
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()
            );

            var connectionConfig = configurationManager.GetConfigurationAsync(CancellationToken.None);
            connectionConfig.Wait();
            var signingKeys = connectionConfig.Result.SigningKeys;
            return signingKeys;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
            {
                
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            app.UseWorkflow();
        }
    }
}
