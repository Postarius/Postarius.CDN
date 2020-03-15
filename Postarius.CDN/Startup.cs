using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Data;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Services;
using Services.BackgroundJobs;
using Web.StaticFiles.App;

namespace Web.StaticFiles
{
    public class Startup
    {
        public AppSettings AppSettings { get; set; }

        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            AppSettings = configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            ConfigureAppSettings(services);
            
            services.AddDbContext<PostariusCdnContext>();
            
            services.AddAuthentication(JwtAuthorizationConsts.FrontendAuthenticationScheme)
                .AddJwtBearer(JwtAuthorizationConsts.FrontendAuthenticationScheme, o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(AppSettings.SigninSecretKey.GetBytes()),
                        ValidIssuers = AppSettings.IssuerNames
                    };
                });

            services.AddAuthorization(o =>
            {
                o.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtAuthorizationConsts.FrontendAuthenticationScheme)
                    .Build();
            });
            
            services.AddHangfire(c =>
                c.UsePostgreSqlStorage(Configuration.GetConnectionString("PostgreSQL")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            
            // EnqueueImageProcessingJob();
        }
        
        private void ConfigureAppSettings(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        }
        
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new Services());
        }

        // TODO : Figure out more elegant way
        private void EnqueueImageProcessingJob()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }
            
            RecurringJob.AddOrUpdate<ImageProcessingJob>(j => j.Execute(), Cron.Minutely);
        }
    }
    
    public class Services : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FileSystemImageService>().As<IImageService>();
            builder.RegisterType<MediaRepository>().As<IMediaRepository>();
            builder.RegisterType<MediaService>().As<IMediaService>();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
        }
    }
}