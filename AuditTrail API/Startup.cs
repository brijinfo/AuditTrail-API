using AuditTrail_API.Data;
using AuditTrail_API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

namespace AuditTrail_API
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
           
            services.AddControllers()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
       });
            services.AddSwaggerGen();

            // Register DbContext
    //        services.AddDbContext<AuditDbContext>(options =>
    //options.UseSqlServer(Configuration.GetConnectionString("AuditDb")));

            services.AddDbContext<AuditDbContext>(options =>
       options.UseInMemoryDatabase("AuditDb"));


            // Register Audit service
            services.AddScoped<IAuditService, AuditService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuditTrail API v1"));
            }

            
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
               // db.Database.Migrate();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
