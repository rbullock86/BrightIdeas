using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

using BeltExam.Models;

namespace BeltExam.Controllers
{
    public class UserController : Controller
    {
        private BeltExamContext dbContext;
        public UserController(BeltExamContext context)
        {
            dbContext = context;
        }

        [HttpGet]
        [Route("/Users")]
        public IActionResult Index()
        {
            Console.WriteLine("Hitting User Index");
            return View("Index");
        }

        [HttpGet]
        [Route("/Users/{_UserId}")]
        public IActionResult UserHome(int _UserId)
        {
            Console.WriteLine("Hitting UserHome");
            // Check to see if logged in user is the matching one
            if(!SessionCheck()){
                return RedirectToAction("Index");
            }
            if(_UserId != HttpContext.Session.GetInt32("_UserId").GetValueOrDefault())
            {
                return RedirectToAction("Index");
            }
            ViewBag._UserId = _UserId;
            User CurrentUser = dbContext.Users.FirstOrDefault(u => u.UserId == _UserId);
            ViewBag.UserAlias = CurrentUser.LastName;
            var Posts = dbContext.Posts.Include(p => p.PostUser)
                                              .Include(p => p.Likes)
                                              .OrderByDescending(p => p.Likes.Count)
                                              .ToList();
            ViewBag.Posts = Posts;
            return View("UserHome");
        }

        [HttpGet]
        [Route("/Users/{_UserId}/Show/{UserToShow}")]
        public IActionResult ShowUser(int _UserId, int UserToShow)
        {
            Console.WriteLine("Hitting the Show User route");
            if(!SessionCheck())
            {
                return RedirectToAction("Index");
            }
            if(_UserId != HttpContext.Session.GetInt32("_UserId").GetValueOrDefault())
            {
                return RedirectToAction("Index");
            }
            List<Post> UserPosts = dbContext.Posts.Where(p => p.UserId == UserToShow).ToList();
            if(UserPosts != null)
            {
                ViewBag.NumberOfPosts = UserPosts.Count;
            }
            else
            {
                ViewBag.NumberOfPosts = 0;
            }
            List<Like> UserLikes = dbContext.Likes.Where(l => l.UserId == UserToShow).ToList();
            if(UserLikes != null)
            {
                ViewBag.NumberOfLikes = UserLikes.Count;
            }
            else
            {
                ViewBag.NumberOfLikes = 0;
            }
            User ShownUser = dbContext.Users.FirstOrDefault(u => u.UserId == UserToShow);
            ViewBag.ShownUserName = ShownUser.FirstName;
            ViewBag.ShownUserAlias = ShownUser.LastName;
            ViewBag.ShownUserEmail = ShownUser.Email;
            ViewBag._UserId = HttpContext.Session.GetInt32("_UserId").GetValueOrDefault();

            return View("ShowUser");
        }

        [HttpGet]
        [Route("Users/{_UserId}/{_PostId}/Like")]
        public IActionResult LikePost( int _UserId, int _PostId)
        {
            Console.WriteLine("Hitting LikePost");
            Like _Like = new Like();
            _Like.UserId = _UserId;
            _Like.PostId = _PostId;
            dbContext.Likes.Add(_Like);
            dbContext.SaveChanges();
            return RedirectToAction("UserHome", new { _UserId = _UserId });
        }

        [HttpGet]
        [Route("Users/Delete/{_PostId}")]
        public IActionResult DeletePost( int _PostId)
        {
            Post PostToDelete = dbContext.Posts.SingleOrDefault(p => p.PostId == _PostId);
            if(PostToDelete != null){

                dbContext.Posts.Remove(PostToDelete);
                dbContext.SaveChanges();
            }
            return RedirectToAction("UserHome", new { _UserId = HttpContext.Session.GetInt32("_UserId").GetValueOrDefault()});
        }

        [HttpGet]
        [Route("Users/{_UserId}/{_PostId}")]
        public IActionResult ShowPost( int _UserId, int _PostId)
        {
            Console.WriteLine("Hitting ShowPost");
            if(!SessionCheck()){
                return RedirectToAction("Index");
            }
            if(_UserId != HttpContext.Session.GetInt32("_UserId").GetValueOrDefault())
            {
                return RedirectToAction("Index");
            }
            var ThePost = dbContext.Posts.Where(p => p.PostId == _PostId)
                                         .Include(p => p.PostUser)
                                         .Include(p => p.Likes)
                                         .ThenInclude(l => l.LikeUser)
                                         .ToList();

            var LikeUsers = ThePost[0].Likes.GroupBy(l => l.LikeUser).Select(group => group.First());



            ViewBag.ThePost = ThePost[0];
            ViewBag.LikeUsers = LikeUsers;
            ViewBag._UserId = _UserId;
                                           



            return View("ShowPost");
        }

        [HttpGet]
        [Route("Users/Logout")]
        public IActionResult Logout()
        {
            Console.WriteLine("Hitting Logout");
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/Users/Create")]
        public IActionResult UserCreate(User _User)
        {
            Console.WriteLine("Hitting UserCreate");
            
            // Dbstuff
            if(ModelState.IsValid)
            {
                Console.WriteLine("CreateUser form is valid");                
                User _PotentialMatch = dbContext.Users.FirstOrDefault(u => u.Email == _User.Email);
                if(_PotentialMatch != null)
                {
                    ModelState.AddModelError("Email", "That Email Address is already used.");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                _User.Password = Hasher.HashPassword(_User, _User.Password);
                dbContext.Add(_User);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("_UserId", _User.UserId);
                return RedirectToAction("UserHome", new { _UserId = _User.UserId });
            }
            return View("Index");
        }

        [HttpPost]
        [Route("/Users/{_UserId}/Post/Create")]
        public IActionResult PostCreate(Post _Post, int _UserId)
        {
            Console.WriteLine($"Hitting CreatePost for User {_UserId}");
            if(ModelState.IsValid)
            {
                dbContext.Posts.Add(_Post);
                dbContext.SaveChanges();
                return RedirectToAction("UserHome", new { _UserId = _UserId});
            }
            return View("UserHome", new { _UserId = _UserId });
        }

        [HttpPost]
        [Route("/Users/Update/{_UserId}")]
        public IActionResult UserUpdate(User _User, int _UserId)
        {
            Console.WriteLine($"Hitting UserUpdate for {_UserId}");
            // db stuff
            // User _UserToUpdate = dbContext.Users.FirstOrDefault(u => u.UserId == _UserId);
            // _UserToUpdate.UpdatedAt = DateTime.Now;
            // _UserToUpdate.FirstName = _User.FirstName;
            // _UserToUpdate.LastName = _User.LastName;
            // _UserToUpdate.Email = _User.Email;
            return RedirectToAction("UserHome", new { _UserId = _UserId});
        }

        [HttpPost]
        [Route("/Users/ProcessLogin")]
        public IActionResult ProcessLogin(LoginUser _LoginUser)
        {
            if(ModelState.IsValid)
            {
                User _PotentialMatch = dbContext.Users.FirstOrDefault(u => u.Email == _LoginUser.LoginEmail);
                if(_PotentialMatch != null)
                {
                    var hasher = new PasswordHasher<LoginUser>();
                    var PassHash = hasher.VerifyHashedPassword(_LoginUser, _PotentialMatch.Password, _LoginUser.LoginPassword);
                    if(PassHash == 0)
                    {
                        ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                        return View("Index");
                    }
                    HttpContext.Session.SetInt32("_UserId", _PotentialMatch.UserId);
                    return RedirectToAction("UserHome", new { _UserId = _PotentialMatch.UserId });
                }
            }
            ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
            return View("Index");
        }

        [HttpGet]
        [Route("/Users/Remove/{_UserId}")]
        public IActionResult UserRemove(int _UserId)
        {
            Console.WriteLine("Hitting UserRemove");
            // var _UserToRemove = dbContext.Users.FirstOrDefault(u => u.UserId == _UserId);
            // dbContext.Users.Remove(_UserToRemove);
            // dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool SessionCheck()
        {
            if(HttpContext.Session.GetInt32("_UserId") == null)
            {
                Console.WriteLine("No user currently in sessionon");
                HttpContext.Session.Clear();
                return false;
            }
            return true;
        }
    }

}