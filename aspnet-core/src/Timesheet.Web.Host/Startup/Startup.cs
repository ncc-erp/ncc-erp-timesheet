using Abp.AspNetCore;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.Log4Net;
using Abp.Extensions;
using Abp.Timing;
using Castle.Facilities.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Office.Interop.Word;
using Ncc.Configuration;
using Ncc.Identity;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Timesheet.Constants;
using Timesheet.Services.File;
using Timesheet.Services.Komu;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon;
using Timesheet.UploadFilesService;
using Timesheet.Services;
using Timesheet.Services.Project;
using Timesheet.Services.HRM;
using Timesheet.Services.FaceIdService;
using Timesheet.Services.HRMv2;
using Timesheet.Services.FaceId;
using Timesheet.Services.Tracker;

namespace Ncc.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "localhost";

        private readonly IConfigurationRoot _appConfiguration;

        public Startup(IHostingEnvironment env)
        {
            //Clock.Provider = ClockProviders.Utc;
            Clock.Provider = ClockProviders.Local;
            _appConfiguration = env.GetAppConfiguration();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddMvc(
                options => options.Filters.Add(new CorsAuthorizationFilterFactory(_defaultCorsPolicyName))
            );
            //

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            services.AddSignalR();
   


            // Configure CORS for angular2 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )

                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed(s => true)
                //.AllowAnyOrigin()
                )
            );
            //
            
            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Timesheet API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("bearerAuth", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
            });

            services.AddTransient<ExportFileService>();
            services.AddHttpClient<KomuService>();
            services.AddHttpClient<TrackerService>();
            services.AddHttpClient<ProjectService>();
            services.AddHttpClient<HRMService>();
            services.AddHttpClient<HRMv2Service>();


            RegisterFileService(services);
            RegisterFaceIdService(services);


            // Configure Abp and Dependency Injection
            return services.AddAbp<TimesheetWebHostModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig("log4net.config")
                )
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); // Initializes ABP framework.

            app.UseCors(_defaultCorsPolicyName); // Enable CORS!

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAbpRequestLocalization();

            app.UseSignalR(routes =>
            {
                routes.MapHub<AbpCommonHub>("/signalr");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(_appConfiguration["App:ServerRootAddress"].EnsureEndsWith('/') + "swagger/v1/swagger.json", "Timesheet API V1");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Timesheet.Web.Host.wwwroot.swagger.ui.index.html");
            }); // URL: /swagger

            WebContentDirectoryFinder.RootFolder = env.ContentRootPath;
        }
        void CreateAWSCredentialProfile()
        {
            var options = new CredentialProfileOptions
            {
                AccessKey = ConstantAmazonS3.AccessKeyId,
                SecretKey = ConstantAmazonS3.SecretKeyId
            };
            var profile = new CredentialProfile(ConstantAmazonS3.Profile, options);
            profile.Region = RegionEndpoint.GetBySystemName(ConstantAmazonS3.Region);

            var sharedFile = new SharedCredentialsFile();
            sharedFile.RegisterProfile(profile);
        }

        private void LoadUploadFileConfig()
        {
            ConstantAmazonS3.Profile = _appConfiguration.GetValue<string>("AWS:Profile");
            ConstantAmazonS3.AccessKeyId = _appConfiguration.GetValue<string>("AWS:AccessKeyId");
            ConstantAmazonS3.SecretKeyId = _appConfiguration.GetValue<string>("AWS:SecretKeyId");
            ConstantAmazonS3.Region = _appConfiguration.GetValue<string>("AWS:Region");
            ConstantAmazonS3.BucketName = _appConfiguration.GetValue<string>("AWS:BucketName");
            ConstantAmazonS3.Prefix = _appConfiguration.GetValue<string>("AWS:Prefix");
            ConstantAmazonS3.CloudFront = _appConfiguration.GetValue<string>("AWS:CloudFront");
            

            ConstantUploadFile.AvatarFolder = _appConfiguration.GetValue<string>("UploadFile:AvatarFolder");

            ConstantUploadFile.Provider = _appConfiguration.GetValue<string>("UploadFile:Provider");
            var strAllowImageFileType = _appConfiguration.GetValue<string>("UploadFile:AllowImageFileTypes");
            ConstantUploadFile.AllowImageFileTypes = strAllowImageFileType.Split(",");
            ConstantInternalUploadFile.RootUrl = _appConfiguration.GetValue<string>("App:ServerRootAddress");
        }
        private void LoadTeamBuildingFileConfig()
        {
            var strAllowFileType = _appConfiguration.GetValue<string>("UploadTeamBuildingFile:AllowFileTypes");
            ConstantTeamBuildingFile.AllowFileTypes = strAllowFileType.Split(",");
            ConstantTeamBuildingFile.ParentFolder= _appConfiguration.GetValue<string>("UploadTeamBuildingFile:ParentFolder");
            ConstantTeamBuildingFile.FileFolder = _appConfiguration.GetValue<string>("UploadTeamBuildingFile:FileFolder"); ;
        }

        private void RegisterFileService(IServiceCollection services)
        {
            LoadUploadFileConfig();
            LoadTeamBuildingFileConfig();
            if (ConstantUploadFile.Provider == ConstantUploadFile.AMAZONE_S3)
            {
                CreateAWSCredentialProfile();
                services.AddAWSService<IAmazonS3>();
                services.AddTransient<IUploadFileService, AmazonS3Service>();
            }
            else
            {
                services.AddTransient<IUploadFileService, InternalUploadFileService>();
            }

        }
        private void RegisterFaceIdService(IServiceCollection services)
        {
            Timesheet.Constants.ConstantFaceId.baseAddress = _appConfiguration.GetValue<string>("FaceIdService:BaseAddress");
            Timesheet.Constants.ConstantFaceId.securityCode = _appConfiguration.GetValue<string>("FaceIdService:SecurityCode");
            Timesheet.Constants.ConstantFaceId.ImageCheckInPath = _appConfiguration.GetValue<string>("FaceIdService:PathImage");

            services.AddHttpClient<FaceIdService>();
        }

    }
}