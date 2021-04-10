using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PowerLinesAnalysisService.Data;
using PowerLinesAnalysisService.Messaging;
using Microsoft.EntityFrameworkCore;
using PowerLinesAnalysisService.Analysis;
using System;

namespace PowerLinesAnalysisService
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("PowerLinesAnalysisService"), options =>
                    options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null))
                );

            var messageConfig = Configuration.GetSection("Message").Get<MessageConfig>();
            services.AddSingleton(messageConfig);
            var threshold = Configuration.GetSection("Threshold").Get<Threshold>();
            services.AddSingleton(threshold);
            services.AddScoped<IAnalysisService, AnalysisService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            ApplyMigrations(dbContext);
        }

        public void ApplyMigrations(ApplicationDbContext dbContext)
        {
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }
        }
    }
}
