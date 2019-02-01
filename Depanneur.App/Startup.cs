using System;
using System.Security.Claims;
using Depanneur.App.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Depanneur.App.Entities;
using Depanneur.App.Hangfire;
using Depanneur.App.Helpers;
using Depanneur.App.Schema;
using FluentEmail.Razor;
using FluentEmail.SendGrid;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Types;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Depanneur.App.Services;
using GraphQL.Authorization;
using GraphQL.Validation;
using FluentEmail.Core.Interfaces;
using FluentEmail.Smtp;
using System.Net.Mail;
using Microsoft.AspNetCore.Authentication;

namespace Depanneur.App
{
    public class Startup
    {
        public static string DefaultTimezoneName { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            DefaultTimezoneName = Configuration.GetValue<string>("DefaultTimezone");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            var connection = Configuration.GetConnectionString("DepanneurDb");
            services.AddDbContext<DepanneurContext>(options => options.UseSqlServer(connection));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<DepanneurContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication()
                .AddGoogle(options => {
                    var googleAuthConfig = Configuration.GetSection("GoogleAuth");

                    options.ClientId = googleAuthConfig["ClientId"];
                    options.ClientSecret = googleAuthConfig["ClientSecret"];

                    // https://github.com/aspnet/AspNetCore/issues/6069#issuecomment-449461197
                    options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
                    options.ClaimActions.Clear();
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                    options.ClaimActions.MapJsonKey("urn:google:profile", "link");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey("urn:google:image", "picture");
                });

            /* TODO PERFO: Faudrait mettre un interval raisonnable ici, ex: 1 minute.
             * Sauf qu'il y a un problèmee que je ne comprend pas.
             *
             * Supposons qu'un user se log, et qu'il a le rôle PRODUCTS.
             * Si on lui retire le rôle pendant qu'il est loggé, je m'attendrais à ce que
             * son cookie soit updaté après le délais indiqué.
             *
             * Toutefois, ce qui ce produit, c'est qu'à la première requête après le délais,
             * ses rôles sont bel et bien à jour (et ya même un Set-Cookie envoyé), sauf que
             * pour les autres requêtes d'après il revient à ses rôles d'avant.
             *
             * Je ne comprend pas pourquoi ce problème survient. Pour le moment, je configure
             * une interval négative afin de forcer un refresh à toutes les requêtes. Ça a
             * l'avantage que les rôles sont toujours à jour, mais l'inconvénient que ça
             * génère des requêtes BD pour chaque requête HTTP.
             */
            services.Configure<SecurityStampValidatorOptions>(x => {
                x.ValidationInterval = TimeSpan.FromSeconds(-1);
            });

            services.AddHangfire(x => x.UseSqlServerStorage(connection));
            services.AddScoped<SendWeekRecapJob>();
            services.AddScoped<ProcessSubscriptionsJob>();

            services.AddScoped<ProductRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<SubscriptionRepository>();
            services.AddScoped<TransactionRepository>();

            services.AddScoped<TransactionService>();
            services.AddScoped<UserService>();

            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor>();
            services.AddSingleton<DataLoaderDocumentListener>();
            services.AddSingleton<DataLoader>();
            services.AddScoped<ISchema>(sp => {
                var resolver = new FuncDependencyResolver(type => sp.GetService(type) ?? ActivatorUtilities.CreateInstance(sp, type));
                return new DepanneurSchema(resolver);
            });

            services.AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddScoped<IValidationRule, AuthorizationValidationRule>();
            services.AddSingleton(s => {
                var authSettings = new AuthorizationSettings();

                authSettings.AddPolicy(Policies.ManageUsers, x => x.RequireClaim(ClaimTypes.Role, Roles.Users));
                authSettings.AddPolicy(Policies.Balances, x => x.RequireClaim(ClaimTypes.Role, Roles.Balances));
                authSettings.AddPolicy(Policies.ManageProducts, x => x.RequireClaim(ClaimTypes.Role, Roles.Products));

                authSettings.AddPolicy(Policies.ReadUsers, policy => {
                    // Allowed if current user has any role
                    policy.RequireClaim(ClaimTypes.Role);
                });

                return authSettings;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (Configuration.GetValue<bool>("ForceSSL"))
                app.UseHttpsRedirection();

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseHangfireServer();
            app.UseHangfireDashboard(options: new DashboardOptions
            {
                Authorization = new[] {
                    new RoleBasedAuthorizationFilter(Roles.Users)
                }
            });

            if (Configuration.GetValue<bool>("GraphQL:EnableGraphiQL")) {
                app.UseGraphiQl();
            }

            app.UseStaticFiles();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "App route",
                    template: "{*path}",
                    defaults: new {
                        controller = "App",
                        action = "Index"
                    });
            });

            FluentEmail.Core.Email.DefaultRenderer = new RazorRenderer();
            FluentEmail.Core.Email.DefaultSender = GetEmailSender(Configuration);
            
            RecurringJob.AddOrUpdate<SendWeekRecapJob>("SendWeekRecap", x => x.Run(), Cron.Weekly(DayOfWeek.Monday, 7), DateExtensions.DefaultTimezone);
            RecurringJob.AddOrUpdate<ProcessSubscriptionsJob>("WeeklySubscriptions", x => x.RunWeekly(), Cron.Weekly(DayOfWeek.Friday, 17), DateExtensions.DefaultTimezone);
            RecurringJob.AddOrUpdate<ProcessSubscriptionsJob>("DailySubscriptions", x => x.RunDaily(), "0 16 * * 1-5", DateExtensions.DefaultTimezone);
        }

        private ISender GetEmailSender(IConfiguration config)
        {
            if (config.GetValue<string>("Email:Mode") == "SendGrid")
                return new SendGridSender(Configuration.GetValue<string>("Email:SendGrid:ApiKey"));

            return new SmtpSender(new SmtpClient(
                host: Configuration.GetValue<string>("Email:Smtp:Host"),
                port: Configuration.GetValue<int>("Email:Smtp:Port")
            ));
        }
    }
}
