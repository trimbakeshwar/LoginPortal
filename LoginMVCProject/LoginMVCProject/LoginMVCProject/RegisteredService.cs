
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System;
using AppDbContext;

namespace LoginMVCProject
{
    public class RegisteredService
    {
        public static void RegisteredServices(IServiceCollection services, IConfiguration config)
        {


            // ---------DEPENDENCY FOR DB------------
            services.AddDbContext<LoginContext>(Options =>
            {
                Options.UseSqlServer(config.GetConnectionString("DbConnection"));
            });


        }


    }
    
}
