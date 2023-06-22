
using LoginMVCProject.Models;
using LoginMVCProject.Repo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Security.Claims;
using System.Text.Json;
using System.Security.Principal;

namespace LoginMVCProject.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Repoisitory repoisitory;

        public HomeController(Repoisitory repoisitory, ILogger<HomeController> logger)
        {
            this.repoisitory = repoisitory;
            _logger = logger;
        }
        [BindProperty]
        public Registration Register { set; get; }
        [BindProperty]
        public List<Registration> RegistrationList { set; get; }
        [BindProperty]
        public InputModel Input { set; get; }
        [BindProperty]
        public string Message { set; get; }
        [BindProperty]
        public string logintoken { set; get; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Password must contain Special charector, number,uppercase ,lower case lettrs and minimun length should be 8 ")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string message { get; set; }
        }


        public IActionResult Index()
        {
            return View("Registerations");
        }
     
        public IActionResult Registerations()
        {
            try
            {
                if (Register != null)
                {
                    string message = "SUCCESS";
                    var result = this.repoisitory.Registerations(Register);
                    if (result.Equals(message))
                    {
                        return View("Login");

                    }
                    else
                    {
                        Message = "Registration fail";
                        return View("Registerations");

                    }
                }
                else
                {
                    Message = "Registration fail";
                    return View("Registrations");
                }
            }catch(Exception ex)
            {
                Message = "Registration fail";
                return View("Registrations");
            }
        }
        public IActionResult Login(Registration Register, string ReturnUrl)
        {
            ViewData["ReturnedUrl"] = ReturnUrl;
            return View();
        }
        [HttpPost("Home/Login")]
        public IActionResult Verify(Registration Register,string ReturnUrl)
          {
            try
            {
                
                if(string.IsNullOrEmpty(Register.Email) || string.IsNullOrEmpty(Register.Password))
                {
                    return View("Login");
                }
                string message = "LOGIN SUCCESS";
                var result = this.repoisitory.Login(Register.Email, Register.Password);
                if (result.Equals("LOGIN SUCCESS"))
                {
                    var token = this.repoisitory.GenerateToken(Register.Email);
                    ClaimsPrincipal claimsPrincipal = this.repoisitory.Validating(token);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(1)
                    });
                    // this.repoisitory.GenerateToken(Register.Email);
                    //GenerateTicket(Register.Email, ReturnUrl);
                    /*List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, Register.Email));
                    claims.Add(new Claim(ClaimTypes.Name, Register.Email));
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
*/
                    
                    return RedirectToAction("Dashbord");
                    // return View(new { Status = true, Message = "Login Sucessfully", Data = result, token });
                }
                if (result.Equals(message))
                {
                    return View("Dashbord");
                }
                else
                {
                            
                    return View("Login");

                }
            }catch(Exception ex)
            {
                return View("Login");
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult Dashbord()
        {
            try
            {
                RegistrationList = this.repoisitory.GetUser().ToList();
                return View(RegistrationList);
            }
            catch(Exception ex)
            {
                return View("Login");
            }
            
        }
        public IActionResult ResetPassword(InputModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var result = "";
                    //var email = TempData["email"].ToString();
                    var userlogCheck = repoisitory.GetPasswordLogHistorys(model.Email);
                    if (userlogCheck.Count == 3)
                    {

                        var log = userlogCheck.OrderBy(x => x.Id).First();
                        repoisitory.DeletePasswordLogHistorys(log);
                    }
                    var userlog = repoisitory.GetPasswordLogHistorys(model.Email);
                    var encryptedPass = Repoisitory.EncryptPassword(model.Password);
                    var checkPassword = userlog.Where(x => x.PasswordHash == encryptedPass).ToList();
                    if (checkPassword.Count == 0)
                    {
                        result = this.repoisitory.ResetPassword(model.Email, model.Password);


                    }
                    else
                    {
                        InputModel models = new InputModel()
                        {
                            message = "password already used"
                        };
                        return View(models);
                    }
                    if (result.Equals("SUCCESS"))
                    {
                        var user1 = repoisitory.getUserDetails(model.Email);
                        var row = new PasswordLogHistory
                        {
                            UserId = Convert.ToString(user1.Id),
                            PasswordHash = user1.Password,
                            ExpireDate = DateTime.Now.AddDays(30),
                            CreatedAT = DateTime.Now,
                            CreatedBy = Convert.ToString(user1.Email)
                        };
                        var data = repoisitory.InsertPasswordLogHistorys(row);

                        return View("Login");

                    }
                    else
                    {
                        return View("ResetPassword");

                    }
                }
                else
                {
                    return View("ResetPassword");
                }
            }catch (Exception ex)
            {
                return View("ResetPassword");
            }
        }
       
        private void GenerateTicket(string email,string ReturnUrl)
        {
            List<Claim> claims= new List<Claim>(); 
            claims.Add(new Claim(ClaimTypes.NameIdentifier, email));
            claims.Add(new Claim(ClaimTypes.Name, email));
            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal claimsPrincipal= new ClaimsPrincipal(claimsIdentity);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
            });
        }
        public IActionResult ForgotPassword(string emailAddress)
        { 
            try
            {
                TempData["email"] = emailAddress;
                var result = this.repoisitory.SendEmail(emailAddress);
                if (result.Equals("SUCCESS"))
                {
                    return View("Login");
                }
                else
                {
                    return View("ForgotPassword");
                }
            }catch(Exception ex)
            {
                return View("ForgotPassword");
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}