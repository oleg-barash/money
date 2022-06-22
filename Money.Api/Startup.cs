using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Money.Business;
using Newtonsoft.Json;
using Money.Business.Models;
using Action = Money.Business.Models.Action;

namespace Money.Api
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            services.AddSingleton(typeof(IConfigurationRoot), configuration);
            services.AddSingleton(typeof(DataContext), new DataContext(configuration.GetConnectionString("MyMoney")));
            //services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.Map("/action/list", builder =>
            {
                DataContext context = builder.ApplicationServices.GetService<DataContext>();
                builder.Run(async httpContext =>
                {
                    IEnumerable<Action> actions = context.GetActions();
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(actions));
                });
            });

            app.Map("/action", builder =>
            {
                DataContext context = builder.ApplicationServices.GetService<DataContext>();
                builder.Run(async httpContext =>
                {
                    switch (httpContext.Request.Method)
                    {
                        case "GET":
                            const string idParameterName = "id";
                            string idParameterValue;
                            if (!httpContext.Request.Query.ContainsKey(idParameterName))
                            {
                                if (!httpContext.Request.Form.ContainsKey(idParameterName))
                                {
                                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    await httpContext.Response.WriteAsync("Parameter 'id' is required");
                                    break;
                                }
                                idParameterValue = httpContext.Request.Form[idParameterName];
                            }
                            else
                            {
                                idParameterValue = httpContext.Request.Query[idParameterName];
                            }
                            if (!int.TryParse(idParameterValue, out var id))
                            {
                                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                await httpContext.Response.WriteAsync("Parameter 'id' should be a number");
                                break;
                            }
                            var action = context.GetAction(id);
                            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(action));
                            break;
                        case "POST":
                            var form = await httpContext.Request.ReadFormAsync();
                            Action newAction = new Action();
                            newAction.Date = DateTime.Parse(form["Date"]);
                            newAction.Type = Enum.Parse<ActionType>(form["Type"]);
                            newAction.Value = decimal.Parse(form["Value"]);
                            newAction.Category = form.ContainsKey("Category") ? Enum.Parse<Category>(form["Category"]) : Category.NotSpecified;
                            newAction.Description = form["Description"];
                            if (form.ContainsKey("Credit"))
                            {
                                newAction.Credit = int.Parse(form["Credit"]);
                            }
                            await context.AddAction(newAction);
                            httpContext.Response.StatusCode = (int)HttpStatusCode.Created;
                            break;
                        default:
                            httpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                            break;
                    }
                });
            });

            app.Map("/credit", builder =>
            {
                DataContext context = builder.ApplicationServices.GetService<DataContext>();
                builder.Run(async httpContext =>
                {
                                        switch (httpContext.Request.Method)
                    {
                        case "GET":
                            var credits = context.GetCredits().ToList();
                            if (credits.Any())
                            {
                                CreditCalculator calculator = new CreditCalculator(context);
                                var res = new List<Action>();
                                foreach (var credit in credits)
                                {
                                    res.AddRange(calculator.GetPaymentsForCredit(credit));
                                }
                                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(res));
                            }
                            break;
                        case "POST":
                            var form = await httpContext.Request.ReadFormAsync();
                            Credit newCredit = new Credit
                            {
                                Date = DateTime.Parse(form["Date"]),
                                Sum = decimal.Parse(form["Sum"]),
                                Duration = int.Parse(form["Duration"]),
                                Interest = decimal.Parse(form["Interest"]),
                                Description = form["Description"],
                                PaymentValue = decimal.Parse(form["PaymentValue"])
                            };
                            await context.AddCredit(newCredit);
                            httpContext.Response.StatusCode = (int)HttpStatusCode.Created;
                            break;
                        default:
                            httpContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                            break;
                    }
                });
            });
            app.Map("/points", builder =>
            {
                DataContext context = builder.ApplicationServices.GetService<DataContext>();
                builder.Run(async httpContext =>
                {

                    if (!httpContext.Request.Query.ContainsKey("from") || !DateTime.TryParse(httpContext.Request.Query["from"], out var from))
                    {
                        from = DateTime.MinValue;
                    };
                    if (!httpContext.Request.Query.ContainsKey("to") || !DateTime.TryParse(httpContext.Request.Query["to"], out var to))
                    {
                        to = DateTime.MaxValue;
                    }
                    IEnumerable<Action> actions = context.GetActions().Where(x => x.Date > from && x.Date < to);
                    var points = actions.OrderBy(a => a.Date).GroupBy(a => a.Date);
                    var currentBalance = 0m;
                    var result = points.Select(p =>
                    {
                        var spendings = p.Where(a => a.Type == ActionType.Spending).Sum(a => a.Value);
                        var incomings = p.Where(a => a.Type == ActionType.Incoming).Sum(a => a.Value);
                        currentBalance += incomings - spendings;
                        return new
                        {
                            Date = p.Key,
                            Balance = currentBalance,
                            Details = new
                            {
                                Spendings = spendings,
                                Incomings = incomings,
                                Actions = p.ToArray()
                            }
                        };
                    });
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
                });
            });
            app.Map("", builder =>
            {
                builder.Run(httpContext =>
                {
                    return Task.Run(() => 
                    {
                        httpContext.Response.StatusCode = (int) HttpStatusCode.Redirect;
                        httpContext.Response.Headers.Add("location", "/index.html");
                    });
                });
            });
            // app.UseMvc();
        }
    }
}
