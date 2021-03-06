﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LaughAndGroan.Api
{
    using System.Security.Claims;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.Runtime;
    using Features.Posts;
    using Features.Users;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Serilog;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var inDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            services.AddControllers();

            services.AddCors(o =>
            {
                o.AddDefaultPolicy(p =>
                {
                    p.WithHeaders("Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key", "X-Amz-Security-Token",
                        "X-Amz-User-Agent");
                    p.WithMethods("DELETE", "GET", "HEAD", "OPTIONS", "PATCH", "POST", "PUT");
                    p.AllowAnyOrigin();
                    p.SetPreflightMaxAge(TimeSpan.FromHours(1.0));
                });
            });

            // Enable this only in development, prod handles this from API Gateway authorizers
            if (inDevelopment)
            {
                // Documentation: https://auth0.com/docs/quickstart/backend/aspnet-core-webapi/01-authorization#configure-the-middleware
                var authDomain = $"https://{Configuration["Auth0:Domain"]}/";
                var authAudience = Configuration["Auth0:Audience"];
                services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(o =>
                    {
                        o.Authority = authDomain;
                        o.Audience = Configuration["Auth0:Audience"];
                    });

                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo {Title = "Laugh and Groan", Version = "v1"});

                    // Getting this right is *hard*, this article helped me with the details:
                    // https://dotnetcoretutorials.com/2021/02/14/using-auth0-with-an-asp-net-core-api-part-3-swagger/

                    c.AddSecurityDefinition("default", new OpenApiSecurityScheme()
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                Scopes = new Dictionary<string, string>
                                {
                                    {"openid", "Open ID"},
                                    {"users.posts:write", "Write and Read Posts"}
                                },
                                AuthorizationUrl = new Uri($"{authDomain}authorize?audience={authAudience}")
                            }
                        }
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme()
                            {
                                Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "default"}
                            },
                            new List<string> {"users.posts:write"}
                        }
                    });
                });
            }

            services.Configure<Settings>(Configuration.GetSection("LaughAndGroan"));

            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            // when in local development mode, the service URL is set and SOME credentials must be provided
            var dynamoOptions = Configuration.GetAWSOptions("DynamoDB");
            if (dynamoOptions.DefaultClientConfig.ServiceURL == "http://localhost:8000")
                dynamoOptions.Credentials = new BasicAWSCredentials("DUMMY", "DUMMY");
            services.AddAWSService<IAmazonDynamoDB>(dynamoOptions);

            services.AddSingleton<IDynamoDBContext>(p =>
            {
                var options = p.GetRequiredService<IOptions<Settings>>();
                var client = p.GetRequiredService<IAmazonDynamoDB>();

                return new DynamoDBContext(client, new DynamoDBContextConfig
                {
                    TableNamePrefix = options.Value.TableNamePrefix,
                    ConsistentRead = true
                });
            });

            services
                .AddSingleton<UsersService>()
                .AddSingleton<PostsService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            // only turn Swagger on in development
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "Laugh and Groan V1");
                    c.OAuthClientId("W2KN1CQdSCt2doY3NYNVbiyWp9Bij347");
                });
            }

            app.UseRouting();

            app.UseCors();

            // only turn authentication on in development
            if (env.IsDevelopment())
            {
                app.UseAuthentication();
            }

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                    });
            });
        }
    }
}