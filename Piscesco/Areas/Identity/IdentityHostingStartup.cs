using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piscesco.Areas.Identity.Data;
using Piscesco.Data;

[assembly: HostingStartup(typeof(Piscesco.Areas.Identity.IdentityHostingStartup))]
namespace Piscesco.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<PiscescoContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("PiscescoContextConnection")));

                services.AddDefaultIdentity<PiscescoUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<PiscescoContext>();
            });
        }
    }
}