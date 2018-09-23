﻿using Akka.Actor;
using Bookstore.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContextPool<BookstoreContext>(options =>
                options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BookstoreEFCore;Trusted_Connection=True;"));

            services.AddScoped<IRepository<Book>, BookstoreRepository<Book>>();

            /* Register the ActorSystem*/
            services.AddSingleton(_ => ActorSystem.Create("bookstore", ConfigurationLoader.Load()));

            services.AddSingleton<BooksManagerActorProvider>(provider =>
            {
                var actorSystem = provider.GetService<ActorSystem>();
                var serviceScopeFactory = provider.GetService<IServiceScopeFactory>();
                var booksManagerActor = actorSystem.ActorOf(Props.Create(() => new BooksManagerActor(serviceScopeFactory)));
                return () => booksManagerActor;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            lifetime.ApplicationStarted.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>(); // start Akka.NET
            });
            lifetime.ApplicationStopping.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>().Terminate().Wait();
            });
        }
    }
}
