﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ASP.NetCoreMVC.Models;
using ASP.NetCoreMVC.Services;
using System.Reflection;
using DAL;
using Identity.Extensitions.Model;

namespace ASP.NetCoreMVC
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
				options.UseMySql(Configuration.GetConnectionString("MySqlConnection")));//添加Mysql支持

			foreach (var item in GetClassName("Service"))
			{
				foreach (var typeArray in item.Value)
				{
					services.AddScoped(typeArray, item.Key);
				}
			}

			services.AddUnitOfWork<ProductContext>();//添加UnitOfWork支持

			services.AddIdentity<ApplicationUser, CustomRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = false;
				options.Password.RequiredUniqueChars = 6;

				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
				options.Lockout.MaxFailedAccessAttempts = 10;
				options.Lockout.AllowedForNewUsers = true;

				// User settings
				options.User.RequireUniqueEmail = true;
			});

			services.ConfigureApplicationCookie(options =>
			{
				// Cookie settings
				options.Cookie.HttpOnly = true;
				options.Cookie.Expiration = TimeSpan.FromDays(150);
				options.LoginPath = "/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
				options.LogoutPath = "/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
				options.AccessDeniedPath = "/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
				options.SlidingExpiration = true;
			});


			// Add application services.
			services.AddTransient<IEmailSender, EmailSender>();

			services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action}/{id?}",
					defaults: new { controller = "Home", action = "Index" });
			});
		}


		/// <summary>  
		/// 获取程序集中的实现类对应的多个接口
		/// </summary>  
		/// <param name="assemblyName">程序集</param>
		public Dictionary<Type, Type[]> GetClassName(string assemblyName)
		{
			if (!String.IsNullOrEmpty(assemblyName))
			{
				Assembly assembly = Assembly.Load(assemblyName);
				List<Type> ts = assembly.GetTypes().ToList();

				var result = new Dictionary<Type, Type[]>();
				foreach (var item in ts.Where(s => !s.IsInterface))
				{
					var interfaceType = item.GetInterfaces();
					result.Add(item, interfaceType);
				}
				return result;
			}
			return new Dictionary<Type, Type[]>();
		}
	}
}
