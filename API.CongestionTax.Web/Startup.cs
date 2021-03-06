using API.CongestionTax.Business.Business;
using API.CongestionTax.Business.Interfaces;
using API.CongestionTax.Data.Interfaces;
using API.CongestionTax.Data.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace API.CongestionTax.Web
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
      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "API.CongestionTax", Version = "v1" });
        var filePath = Path.Combine(AppContext.BaseDirectory, "API.CongestionTax.Web.xml");
        c.IncludeXmlComments(filePath, true);
      });

      ConfigureDependencies(services);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API.CongestionTax v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }

    private void ConfigureDependencies(IServiceCollection services)
    {
      services.AddSingleton<ICongestionTaxCalculator, CongestionTaxCalculator>();
      services.AddTransient<ITaxationInfoProvider, MockedDatabaseProvider>();
    }
  }
}
