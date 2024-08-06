using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using catalog.Data;
using catalog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;
using LibGit2Sharp;
using System.IO;

namespace catalog.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly BookContext _context;
        private readonly IConfiguration _config;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration config, BookContext context)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }

        public void OnGet()
        {
            var books=new List<Book>();

            try  {
                books = _context.Books.ToList();
            }
            catch (Exception ex)  {
                ViewData["Error"]=ex.Message;
                ViewData["books"] = books;
                return;
            }
            
            // UNCOMMENT AFTER ADDING REDIS
            //Get data about the shopping cart
            //var client = GetRedisClient();
            //var cartItems = client.GetListCount("cart");
            //ViewData["cartNo"] = cartItems;

            ViewData["books"] = books;
            
        }

        public IActionResult OnPostAddToShoppingCart()
        {
            // UNCOMMECT AFTER ADDING REDIS
            // var client = GetRedisClient();
            // var bookId = int.Parse(Request.Form["bookId"]);
                
            // if (!client.GetAllItemsFromList("cart").Contains(bookId.ToString()))
            // {
            //     var book = _context.Books.Find(bookId);
            //     book.InStock--;
            //     _context.SaveChanges();
            
            //     client.AddItemToList("cart", bookId.ToString());
            // }

            return RedirectToPage();
        }

        public IActionResult OnPostLoad()
        {
            BookLoader.LoadBooks(_context);
            return RedirectToPage();
        }

        public void OnPostCommit()
        {
            string repoPath = Directory.GetCurrentDirectory().; // Path to your local Git repository
            string remoteName = "startup"; // The name of the remote repository
            string branchName = "main"; // The branch you want to push to
            string username = "RD-FD"; // Your remote repository username
            string password = "needWork@12"; // Your remote repository password

            try {
                PushChanges(repoPath, remoteName, branchName, username, password);
            } catch (Exception ex) {
                throw ex;
            }
        }

        private void PushChanges(string repoPath, string remoteName, string branchName, string username, string password)
        {
            try
            {
                // Open the repository
                using (var repo = new Repository(repoPath))
                {
                    // Create credentials for authentication
                    var credentials = new UsernamePasswordCredentials
                    {
                        Username = username,
                        Password = password
                    };

                    // Find the remote
                    var remote = repo.Network.Remotes[remoteName];
                    if (remote == null)
                    {
                        Console.WriteLine($"Remote '{remoteName}' not found.");
                        return;
                    }

                    // Create a push options object
                    var pushOptions = new PushOptions
                    {
                        CredentialsProvider = (url, user, cred) => credentials
                    };

                    // Get the reference to the branch
                    var branch = repo.Branches[branchName];
                    if (branch == null)
                    {
                        Console.WriteLine($"Branch '{branchName}' not found.");
                        return;
                    }

                    // Push the branch to the remote
                    repo.Network.Push(branch, pushOptions); //repo.Network.Push(branch, pushOptions);
                    // if (result.Status == PushStatus.UpToDate)
                    // {
                    //     Console.WriteLine("Push was successful.");
                    // }
                    // else
                    // {
                    //     Console.WriteLine("Push was not successful.");
                    // }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        private IRedisClient GetRedisClient()
        {
            var conString = _config.GetValue<String>("Redis:ConnectionString");
            var manager = new RedisManagerPool(conString);
            return manager.GetClient();
        }
    }
}
