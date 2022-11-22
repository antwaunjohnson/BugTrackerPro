using BugTrackerPro.Models.Enums;
using BugTrackerPro.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BugTrackerPro.Data;

public static class DataUtility
{
    //Company Ids
    private static int company1Id;
    private static int company2Id;
  

    public static string GetConnectionString(IConfiguration configuration)
    {
        //The default connection string will come from appSettings like usual
        var connectionString = configuration.GetSection("pgSettings")["pgConnection"];
        //It will be automatically overwritten if we are running on Heroku
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        return string.IsNullOrEmpty(databaseUrl) ? connectionString : BuildConnectionString(databaseUrl);
    }

    public static string BuildConnectionString(string databaseUrl)
    {
        //Provides an object representation of a uniform resource identifier (URI) and easy access to the parts of the URI.
        var databaseUri = new Uri(databaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        //Provides a simple way to create and manage the contents of connection strings used by the NpgsqlConnection class.
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Prefer,
            TrustServerCertificate = true
        };
        return builder.ToString();
    }

    public static async Task ManageDataAsync(IHost host, IConfiguration configuration)
    {
        using var svcScope = host.Services.CreateScope();
        var svcProvider = svcScope.ServiceProvider;
        //Service: An instance of RoleManager
        var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
        //Service: An instance of RoleManager
        var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
        //Service: An instance of the UserManager
        var userManagerSvc = svcProvider.GetRequiredService<UserManager<BTProUser>>();
        //Migration: This is the programmatic equivalent to Update-Database
        await dbContextSvc.Database.MigrateAsync();


        //Custom  Bug Tracker Seed Methods
        await SeedRolesAsync(roleManagerSvc);
        await SeedDefaultCompaniesAsync(dbContextSvc);
        await SeedDefaultUsersAsync(userManagerSvc, configuration);
        await SeedDemoUsersAsync(userManagerSvc, configuration);
        await SeedDefaultTicketTypeAsync(dbContextSvc);
        await SeedDefaultTicketStatusAsync(dbContextSvc);
        await SeedDefaultTicketPriorityAsync(dbContextSvc);
        await SeedDefaultProjectPriorityAsync(dbContextSvc);
        await SeedDefautProjectsAsync(dbContextSvc);
        await SeedDefautTicketsAsync(dbContextSvc);
    }


    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        //Seed Roles
        await roleManager.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.ProjectManager.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.Developer.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.Submitter.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Roles.DemoUser.ToString()));
    }

    public static async Task SeedDefaultCompaniesAsync(ApplicationDbContext context)
    {
        try
        {
            IList<Company> defaultcompanies = new List<Company>() {
                    new Company() { Name = "WidgiSoft", Description="We're Making Software Widgets Great Again!" },
                    new Company() { Name = "TechBuilder", Description="Helping You Build Technological Strength" },
                };

            var dbCompanies = context.Companies!.Select(c => c.Name).ToList();
            await context.Companies!.AddRangeAsync(defaultcompanies.Where(c => !dbCompanies.Contains(c.Name)));
            await context.SaveChangesAsync();

            //Get company Ids
            company1Id = context.Companies.FirstOrDefault(p => p.Name == "WidgiSoft")!.Id;
            company2Id = context.Companies.FirstOrDefault(p => p.Name == "TechBuilder")!.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Companies.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

    public static async Task SeedDefaultProjectPriorityAsync(ApplicationDbContext context)
    {
        try
        {
            IList<Models.ProjectPriority> projectPriorities = new List<ProjectPriority>() {
                                                    new ProjectPriority() { Name = BTProProjectPriority.Low.ToString() },
                                                    new ProjectPriority() { Name = BTProProjectPriority.Medium.ToString() },
                                                    new ProjectPriority() { Name = BTProProjectPriority.High.ToString() },
                                                    new ProjectPriority() { Name = BTProProjectPriority.Urgent.ToString() },
                };

            var dbProjectPriorities = context.ProjectPriorities!.Select(c => c.Name).ToList();
            await context.ProjectPriorities!.AddRangeAsync(projectPriorities.Where(c => !dbProjectPriorities.Contains(c.Name)));
            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Project Priorities.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

    public static async Task SeedDefautProjectsAsync(ApplicationDbContext context)
    {

        //Get project priority Ids
        int priorityLow = context.ProjectPriorities!.FirstOrDefault(p => p.Name == BTProProjectPriority.Low.ToString())!.Id;
        int priorityMedium = context.ProjectPriorities!.FirstOrDefault(p => p.Name == BTProProjectPriority.Medium.ToString())!.Id;
        int priorityHigh = context.ProjectPriorities!.FirstOrDefault(p => p.Name == BTProProjectPriority.High.ToString())!.Id;
        int priorityUrgent = context.ProjectPriorities!.FirstOrDefault(p => p.Name == BTProProjectPriority.Urgent.ToString())!.Id;

        try
        {
            IList<Project> projects = new List<Project>() {
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build a student Dashboard",
                         Description="Some State College is requesting a dashboard that allows students to see their academic progress in realtime. The dashboard should show grades as well as realtime attendance and assignment progress" ,
                         StartDate = new DateTime(2022,10,20),
                         EndDate = new DateTime(2022,10,20).AddMonths(6),
                         ProjectPriorityId = priorityLow
                     },
                     new Project()
                     {
                         CompanyId = company2Id,
                         Name = "Build a Marketing Automation Application",
                         Description="Candidate's custom built web application using .Net Core with MVC, a postgres database and hosted in a heroku container.  The app is designed for the candidate to create, update and maintain a live marketing automation site.",
                         StartDate = new DateTime(2022,10,20),
                         EndDate = new DateTime(2022,10,20).AddMonths(4),
                         ProjectPriorityId = priorityMedium
                     },
                     new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build an Issue Tracking Web Application",
                         Description="A custom designed .Net Core application with postgres database.  The application is a multi tennent application designed to track issue tickets' progress.  Implemented with identity and user roles, Tickets are maintained in projects which are maintained by users in the role of projectmanager.  Each project has a team and team members.",
                         StartDate = new DateTime(2022,10,20),
                         EndDate = new DateTime(2022,10,20).AddMonths(7),
                         ProjectPriorityId = priorityHigh
                     },
                     new Project()
                     {
                         CompanyId = company2Id,
                         Name = "Build an Address Book Web Application",
                         Description="A custom designed .Net Core application with postgres database.  This is an application to serve as a rolodex of contacts for a given user..",
                         StartDate = new DateTime(2022,10,20),
                         EndDate = new DateTime(2022,10,20).AddMonths(3),
                         ProjectPriorityId = priorityLow
                     },
                    new Project()
                     {
                         CompanyId = company1Id,
                         Name = "Build a Movie Information Web Application",
                         Description="A custom designed .Net Core application with postgres database.  An API based application allows users to input and import movie posters and details including cast and crew information.",
                         StartDate = new DateTime(2022,10,20),
                         EndDate = new DateTime(2022,10,20).AddMonths(5),
                         ProjectPriorityId = priorityHigh
                     }
                };

            var dbProjects = context.Projects!.Select(c => c.Name).ToList();
            await context.Projects!.AddRangeAsync(projects.Where(c => !dbProjects.Contains(c.Name)));
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Projects.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }



    public static async Task SeedDefaultUsersAsync(UserManager<BTProUser> userManager, IConfiguration configuration)
    {
        //Seed Default Admin User
        var defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Admin1UserName"],
            Email = configuration["Credentials:Admin1UserName"],
            FirstName = configuration["Credentials:Admin1FirstName"],
            LastName = configuration["Credentials:Admin1LastName"],
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Admin1Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Admin User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default Admin User
             defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Admin2UserName"],
            Email = configuration["Credentials:Admin2UserName"],
            FirstName = configuration["Credentials:Admin2FirstName"],
            LastName = configuration["Credentials:Admin2LastName"],
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Admin2Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Admin User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default ProjectManager1 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:PM1UserName"],
            Email = configuration["Credentials:PM1UserName"],
            FirstName = configuration["Credentials:PM1FirstName"],
            LastName = configuration["Credentials:PM1LastName"],
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:PM1Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default ProjectManager1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default ProjectManager2 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:PM2UserName"],
            Email = configuration["Credentials:PM2UserName"],
            FirstName = configuration["Credentials:PM2FirstName"],
            LastName = configuration["Credentials:PM2LastName"],
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:PM2Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default ProjectManager2 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer1 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Dev1UserName"],
            Email = configuration["Credentials:Dev1UserName"],
            FirstName = configuration["Credentials:Dev1FirstName"],
            LastName = configuration["Credentials:Dev1LastName"],
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Dev1Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer2 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Dev2UserName"],
            Email = configuration["Credentials:Dev2UserName"],
            FirstName = configuration["Credentials:Dev2FirstName"],
            LastName = configuration["Credentials:Dev2LastName"],
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Dev2Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer2 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer3 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Dev3UserName"],
            Email = configuration["Credentials:Dev3UserName"],
            FirstName = configuration["Credentials:Dev3FirstName"],
            LastName = configuration["Credentials:Dev3LastName"],
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Dev3Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer3 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer4 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Dev4UserName"],
            Email = configuration["Credentials:Dev4UserName"],
            FirstName = configuration["Credentials:Dev4FirstName"],
            LastName = configuration["Credentials:Dev4LastName"],
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Dev4Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer4 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Developer5 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Dev5UserName"],
            Email = configuration["Credentials:Dev5UserName"],
            FirstName = configuration["Credentials:Dev5FirstName"],
            LastName = configuration["Credentials:Dev5LastName"],
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Dev5Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer5 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default Developer6 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Dev6UserName"],
            Email = configuration["Credentials:Dev6UserName"],
            FirstName = configuration["Credentials:Dev6FirstName"],
            LastName = configuration["Credentials:Dev6LastName"],
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Dev6Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Developer5 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

        //Seed Default Submitter1 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Submitter1UserName"],
            Email = configuration["Credentials:Submitter1UserName"],
            FirstName = configuration["Credentials:Submitter1FirstName"],
            LastName = configuration["Credentials:Submitter1LastName"],
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Submitter1Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Submitter1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Default Submitter2 User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:Submitter2UserName"],
            Email = configuration["Credentials:Submitter2UserName"],
            FirstName = configuration["Credentials:Submitter2FirstName"],
            LastName = configuration["Credentials:Submitter2LastName"],
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:Submitter2Password"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Default Submitter2 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }

    }

    public static async Task SeedDemoUsersAsync(UserManager<BTProUser> userManager, IConfiguration configuration)
    {
        //Seed Demo Admin User
        var defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:DemoAdminUsername"],
            Email = configuration["Credentials:DemoAdminUsername"],
            FirstName = "Derek",
            LastName = "Jeter",
            EmailConfirmed = true,
            CompanyId = company1Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:DemoAdminPassword"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo Admin User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo ProjectManager User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:DemoPmUsername"],
            Email = configuration["Credentials:DemoPmUsername"],
            FirstName = "Bernie",
            LastName = "Williams",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:DemoPmPassword"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.ProjectManager.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo ProjectManager1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo Developer User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:DemoDevUsername"],
            Email = configuration["Credentials:DemoDevUsername"],
            FirstName = "Aaron",
            LastName = "Judge",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:DemoDevPassword"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Developer.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo Developer1 User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo Submitter User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:DemoSubmitterUsername"],
            Email = configuration["Credentials:DemoSubmitterUsername"],
            FirstName = "Nester",
            LastName = "Cortez",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:DemoSubmitterPassword"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo Submitter User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }


        //Seed Demo New User
        defaultUser = new BTProUser
        {
            UserName = configuration["Credentials:DemoNewUserUsername"],
            Email = configuration["Credentials:DemoNewUserUsername"],
            FirstName = "Oswaldo",
            LastName = "Cabrera",
            EmailConfirmed = true,
            CompanyId = company2Id
        };
        try
        {
            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, configuration["Credentials:DemoNewUserPassword"]);
                await userManager.AddToRoleAsync(defaultUser, Roles.Submitter.ToString());
                await userManager.AddToRoleAsync(defaultUser, Roles.DemoUser.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Demo New User.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }



    public static async Task SeedDefaultTicketTypeAsync(ApplicationDbContext context)
    {
        try
        {
            IList<TicketType> ticketTypes = new List<TicketType>() {
                new TicketType() { Name = BTProTicketType.NewDevelopment.ToString() },      // Ticket involves development of a new, uncoded solution 
                new TicketType() { Name = BTProTicketType.WorkTask.ToString() },            // Ticket involves development of the specific ticket description 
                     new TicketType() { Name = BTProTicketType.Defect.ToString()},               // Ticket involves unexpected development/maintenance on a previously designed feature/functionality
                     new TicketType() { Name = BTProTicketType.ChangeRequest.ToString() },       // Ticket involves modification development of a previously designed feature/functionality
                     new TicketType() { Name = BTProTicketType.Enhancement.ToString() },         // Ticket involves additional development on a previously designed feature or new functionality
                     new TicketType() { Name = BTProTicketType.GeneralTask.ToString() }          // Ticket involves no software development but may involve tasks such as configuations, or hardware setup
                };

            var dbTicketTypes = context.TicketTypes!.Select(c => c.Name).ToList();
            await context.TicketTypes!.AddRangeAsync(ticketTypes.Where(c => !dbTicketTypes.Contains(c.Name)));
            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Ticket Types.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

    public static async Task SeedDefaultTicketStatusAsync(ApplicationDbContext context)
    {
        try
        {
            IList<TicketStatus> ticketStatuses = new List<TicketStatus>() {
                new TicketStatus() { Name = BTProTicketStatus.New.ToString() },                 // Newly Created ticket having never been assigned
                new TicketStatus() { Name = BTProTicketStatus.Development.ToString() },         // Ticket is assigned and currently being worked 
                new TicketStatus() { Name = BTProTicketStatus.Testing.ToString()  },            // Ticket is assigned and is currently being tested
                    new TicketStatus() { Name = BTProTicketStatus.Resolved.ToString()  },           // Ticket remains assigned to the developer but work in now complete
                };

            var dbTicketStatuses = context.TicketStatuses!.Select(c => c.Name).ToList();
            await context.TicketStatuses!.AddRangeAsync(ticketStatuses.Where(c => !dbTicketStatuses.Contains(c.Name)));
            await context.SaveChangesAsync();

        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Ticket Statuses.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

    public static async Task SeedDefaultTicketPriorityAsync(ApplicationDbContext context)
    {
        try
        {
            IList<TicketPriority> ticketPriorities = new List<TicketPriority>() {
                                                    new TicketPriority() { Name = BTProTicketPriority.Low.ToString()  },
                                                    new TicketPriority() { Name = BTProTicketPriority.Medium.ToString() },
                                                    new TicketPriority() { Name = BTProTicketPriority.High.ToString()},
                                                    new TicketPriority() { Name = BTProTicketPriority.Urgent.ToString()},
                };

            var dbTicketPriorities = context.TicketPriorities!.Select(c => c.Name).ToList();
            await context.TicketPriorities!.AddRangeAsync(ticketPriorities.Where(c => !dbTicketPriorities.Contains(c.Name)));
            context.SaveChanges();

        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Ticket Priorities.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }



    public static async Task SeedDefautTicketsAsync(ApplicationDbContext context)
    {
        //Get project Ids
        int dashboardId = context.Projects!.FirstOrDefault(p => p.Name == "Build a student Dashboard")!.Id;
        int marketingId = context.Projects!.FirstOrDefault(p => p.Name == "Build a Marketing Automation Application")!.Id;
        int bugtrackerId = context.Projects!.FirstOrDefault(p => p.Name == "Build an Issue Tracking Web Application")!.Id;
        int movieId = context.Projects!.FirstOrDefault(p => p.Name == "Build a Movie Information Web Application")!.Id;

        //Get ticket type Ids
        int typeNewDev = context.TicketTypes!.FirstOrDefault(p => p.Name == BTProTicketType.NewDevelopment.ToString())!.Id;
        int typeWorkTask = context.TicketTypes!.FirstOrDefault(p => p.Name == BTProTicketType.WorkTask.ToString())!.Id;
        int typeDefect = context.TicketTypes!.FirstOrDefault(p => p.Name == BTProTicketType.Defect.ToString())!.Id;
        int typeEnhancement = context.TicketTypes!.FirstOrDefault(p => p.Name == BTProTicketType.Enhancement.ToString())!.Id;
        int typeChangeRequest = context.TicketTypes!.FirstOrDefault(p => p.Name == BTProTicketType.ChangeRequest.ToString())!.Id;

        //Get ticket priority Ids
        int priorityLow = context.TicketPriorities!.FirstOrDefault(p => p.Name == BTProTicketPriority.Low.ToString())!.Id;
        int priorityMedium = context.TicketPriorities!.FirstOrDefault(p => p.Name == BTProTicketPriority.Medium.ToString())!.Id;
        int priorityHigh = context.TicketPriorities!.FirstOrDefault(p => p.Name == BTProTicketPriority.High.ToString())!.Id;
        int priorityUrgent = context.TicketPriorities!.FirstOrDefault(p => p.Name == BTProTicketPriority.Urgent.ToString())!.Id;

        //Get ticket status Ids
        int statusNew = context.TicketStatuses!.FirstOrDefault(p => p.Name == BTProTicketStatus.New.ToString())!.Id;
        int statusDev = context.TicketStatuses!.FirstOrDefault(p => p.Name == BTProTicketStatus.Development.ToString())!.Id;
        int statusTest = context.TicketStatuses!.FirstOrDefault(p => p.Name == BTProTicketStatus.Testing.ToString())!.Id;
        int statusResolved = context.TicketStatuses!.FirstOrDefault(p => p.Name == BTProTicketStatus.Resolved.ToString())!.Id;


        try
        {
            IList<Ticket> tickets = new List<Ticket>() {
                                //PORTFOLIO
                                new Ticket() {Title = "Dashboard Ticket 1", Description = "Ticket details for dashboard ticket 1", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Dashboard Ticket 2", Description = "Ticket details for dashboard ticket 2", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Dashboard Ticket 3", Description = "Ticket details for dashboard ticket 3", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Dashboard Ticket 4", Description = "Ticket details for dashboard ticket 4", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Dashboard Ticket 5", Description = "Ticket details for dashboard ticket 5", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Dashboard Ticket 6", Description = "Ticket details for dashboard ticket 6", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Dashboard Ticket 7", Description = "Ticket details for dashboard ticket 7", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Dashboard Ticket 8", Description = "Ticket details for dashboard ticket 8", Created = DateTimeOffset.Now, ProjectId = dashboardId, TicketPriorityId = priorityUrgent, TicketStatusId = statusTest, TicketTypeId = typeDefect},
                                //BLOG
                                new Ticket() {Title = "Marketing Ticket 1", Description = "Ticket details for marketing ticket 1", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Marketing Ticket 2", Description = "Ticket details for marketing ticket 2", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Marketing Ticket 3", Description = "Ticket details for marketing ticket 3", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Marketing Ticket 4", Description = "Ticket details for marketing ticket 4", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Marketing Ticket 5", Description = "Ticket details for marketing ticket 5", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Marketing Ticket 6", Description = "Ticket details for marketing ticket 6", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Marketing Ticket 7", Description = "Ticket details for marketing ticket 7", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Marketing Ticket 8", Description = "Ticket details for marketing ticket 8", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Marketing Ticket 9", Description = "Ticket details for marketing ticket 9", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Marketing Ticket 10", Description = "Ticket details for marketing ticket 10", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Marketing Ticket 11", Description = "Ticket details for marketing ticket 11", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Marketing Ticket 12", Description = "Ticket details for marketing ticket 12", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Marketing Ticket 13", Description = "Ticket details for marketing ticket 13", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Marketing Ticket 14", Description = "Ticket details for marketing ticket 14", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Marketing Ticket 15", Description = "Ticket details for marketing ticket 15", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Marketing Ticket 16", Description = "Ticket details for marketing ticket 16", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Marketing Ticket 17", Description = "Ticket details for marketing ticket 17", Created = DateTimeOffset.Now, ProjectId = marketingId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                //BUGTRACKER                                                                                                                         
                                new Ticket() {Title = "Bug Tracker Ticket 1", Description = "Ticket details for bug tracker ticket 1", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 2", Description = "Ticket details for bug tracker ticket 2", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 3", Description = "Ticket details for bug tracker ticket 3", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 4", Description = "Ticket details for bug tracker ticket 4", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 5", Description = "Ticket details for bug tracker ticket 5", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 6", Description = "Ticket details for bug tracker ticket 6", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 7", Description = "Ticket details for bug tracker ticket 7", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 8", Description = "Ticket details for bug tracker ticket 8", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 9", Description = "Ticket details for bug tracker ticket 9", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 10", Description = "Ticket details for bug tracker 10", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 11", Description = "Ticket details for bug tracker 11", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 12", Description = "Ticket details for bug tracker 12", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 13", Description = "Ticket details for bug tracker 13", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 14", Description = "Ticket details for bug tracker 14", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 15", Description = "Ticket details for bug tracker 15", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 16", Description = "Ticket details for bug tracker 16", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 17", Description = "Ticket details for bug tracker 17", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 18", Description = "Ticket details for bug tracker 18", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 19", Description = "Ticket details for bug tracker 19", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 20", Description = "Ticket details for bug tracker 20", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 21", Description = "Ticket details for bug tracker 21", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 22", Description = "Ticket details for bug tracker 22", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 23", Description = "Ticket details for bug tracker 23", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 24", Description = "Ticket details for bug tracker 24", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 25", Description = "Ticket details for bug tracker 25", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 26", Description = "Ticket details for bug tracker 26", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 27", Description = "Ticket details for bug tracker 27", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 28", Description = "Ticket details for bug tracker 28", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 29", Description = "Ticket details for bug tracker 29", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Bug Tracker Ticket 30", Description = "Ticket details for bug tracker 30", Created = DateTimeOffset.Now, ProjectId = bugtrackerId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                //MOVIE
                                new Ticket() {Title = "Movie Ticket 1", Description = "Ticket details for movie ticket 1", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 2", Description = "Ticket details for movie ticket 2", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 3", Description = "Ticket details for movie ticket 3", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 4", Description = "Ticket details for movie ticket 4", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 5", Description = "Ticket details for movie ticket 5", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusDev,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 6", Description = "Ticket details for movie ticket 6", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 7", Description = "Ticket details for movie ticket 7", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew, TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 8", Description = "Ticket details for movie ticket 8", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 9", Description = "Ticket details for movie ticket 9", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew,  TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 10", Description = "Ticket details for movie ticket 10", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusNew, TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 11", Description = "Ticket details for movie ticket 11", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 12", Description = "Ticket details for movie ticket 12", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 13", Description = "Ticket details for movie ticket 13", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityLow, TicketStatusId = statusNew, TicketTypeId = typeDefect},
                                new Ticket() {Title = "Movie Ticket 14", Description = "Ticket details for movie ticket 14", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 15", Description = "Ticket details for movie ticket 15", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 16", Description = "Ticket details for movie ticket 16", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 17", Description = "Ticket details for movie ticket 17", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusDev,  TicketTypeId = typeNewDev},
                                new Ticket() {Title = "Movie Ticket 18", Description = "Ticket details for movie ticket 18", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityMedium, TicketStatusId = statusDev,  TicketTypeId = typeEnhancement},
                                new Ticket() {Title = "Movie Ticket 19", Description = "Ticket details for movie ticket 19", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityHigh, TicketStatusId = statusNew,  TicketTypeId = typeChangeRequest},
                                new Ticket() {Title = "Movie Ticket 20", Description = "Ticket details for movie ticket 20", Created = DateTimeOffset.Now, ProjectId = movieId, TicketPriorityId = priorityUrgent, TicketStatusId = statusNew, TicketTypeId = typeNewDev},

                };


            var dbTickets = context.Tickets!.Select(c => c.Title).ToList();
            await context.Tickets!.AddRangeAsync(tickets.Where(c => !dbTickets.Contains(c.Title)));
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("*************  ERROR  *************");
            Console.WriteLine("Error Seeding Tickets.");
            Console.WriteLine(ex.Message);
            Console.WriteLine("***********************************");
            throw;
        }
    }

}
