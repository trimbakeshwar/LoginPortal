using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using AppDbContext;
using LoginMVCProject.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoginMVCProject.Repo
{
    public class Repoisitory 
    {

        LoginContext _loginContext;
        public static readonly SymmetricSecurityKey SIGNINGKEY = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Repoisitory.SECRETKEY));

        /// <summary>
        /// The secret key
        /// </summary>
        private const string SECRETKEY = "SuperSecretKey@345fghhhhhhhhhhhhhhhhhhhhhhhhhhhhhfggggggg";


        public Repoisitory(LoginContext loginContext)
        {
            this._loginContext = loginContext;
        }

        public string Registerations(Registration register)
        {
            register.Password = EncryptPassword(register.Password);
            this._loginContext.Registrations.Add(register);
            this._loginContext.SaveChanges();
            string message = "SUCCESS";
            return message;
        }
        public Registration getUserDetails(string email)
        {
          return this._loginContext.Registrations.Where(x=>x.Email==email).FirstOrDefault();
        }
        public static string EncryptPassword(string password)
        {
            try
            {
                byte[] encryptData = new byte[password.Length];
                encryptData = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encryptData);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
        public string Login(string Email, string Password)
        {
            string message;
            Password = EncryptPassword(Password);
            var login = this._loginContext.Registrations.Where(x => x.Email == Email && x.Password == Password).SingleOrDefault();
            if (login != null)
            {
                message = "LOGIN SUCCESS";
            }
            else
            {
                message = "LOGIN UNSUCCESSFUL";

            }
            return message;

        }

        public IEnumerable<Registration> GetUser()
        {
            IEnumerable<Registration> employeelist = this._loginContext.Registrations;
            return employeelist.ToList();
        }
        public string ResetPassword(string email, string newPassword)
        {
            var Entries = this._loginContext.Registrations.FirstOrDefault(x => x.Email == email);
            if (Entries != null)
            {
                Entries.Password = EncryptPassword(newPassword);
                this._loginContext.Entry(Entries).State = EntityState.Modified;
                this._loginContext.SaveChanges();
                return "SUCCESS";
            }
            else
            {
                return "NOT FOUND";
            }
        }
        public string GenerateToken(string userEmail)
        {
            try
            {
                var token = new JwtSecurityToken(
                claims: new Claim[]
                {
                    new Claim(ClaimTypes.Name, userEmail)
                },
                notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(DateTime.Now.AddMinutes(60)).DateTime,
                signingCredentials: new SigningCredentials(SIGNINGKEY, SecurityAlgorithms.HmacSha256));
               
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public  ClaimsPrincipal Validating(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var symmetricKey = Encoding.UTF8.GetBytes(SECRETKEY);

                var validationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = SIGNINGKEY
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /*public string SendEmail(string emailAddress)
        {
            string body;
            string subject = "Registration Credential";
            var entry = this._loginContext.Registrations.FirstOrDefault(x => x.Email == emailAddress);
            if (entry != null)
            {
                body = entry.Password;
            }
            else
            {
                return "Not Found";
            }
            string senderEmail = "trimbakeshwarh@gmail.com";
            string receiverEmail = emailAddress;
          

            MailMessage message = new MailMessage(senderEmail, receiverEmail, subject, body);
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); // replace with your SMTP server and port number

            client.EnableSsl = true; // set to true if your SMTP server requires SSL/TLS encryption
            client.Credentials = new NetworkCredential("trimbakeshwarh@gmail.com", "Suraj@#1234"); // replace with your SMTP server login credentials
            client.EnableSsl = true;
            client.UseDefaultCredentials= false;
            try
            {
                client.Send(message);
                return "success";
                Console.WriteLine("Email sent successfully.");
            }
            catch (SmtpException ex)
            {
                return "fail";
                Console.WriteLine("Error sending email: " + ex.Message);
            }

        }*/
        public List<PasswordLogHistory> GetPasswordLogHistorys(string gmail)
        {
            try
            {
               var result= this._loginContext.Registrations.Where(x => x.Email.Equals(gmail)).FirstOrDefault();
                var userloghistory =  this._loginContext.PasswordLogHistorys.Where(x=>x.UserId== Convert.ToString( result.Id)).ToList();
                return userloghistory;

            }
            catch (Exception Ex)
            {
                return null;
            }
        }

        public  int InsertPasswordLogHistorys(PasswordLogHistory model)
        {
            try
            {
                  this._loginContext.PasswordLogHistorys.Add(model);
                var result = this._loginContext.SaveChanges();
                return result;
            }
            catch (Exception Ex)
            {
                return 0;
            }
        }

        public  void DeletePasswordLogHistorys(PasswordLogHistory userPasswordLogHistory)
        {
            try
            {
                this._loginContext.PasswordLogHistorys.Remove(userPasswordLogHistory);
                this._loginContext.SaveChanges();
                // var userloghistory = await base.Remove(userPasswordLogHistory).ConfigureAwait(false);

            }
            catch (Exception Ex)
            {
                

            }
        }
        public bool SendEmail(string emailAddress)
        {
            try
            {
                string body;
                string subject = "Link To Reset Your FundooApp Credential";
                var entry = this._loginContext.Registrations.FirstOrDefault(x => x.Email == emailAddress);
                if (entry != null)
                {
                        body = entry.Password;
                    

                    using (MailMessage mailMessage = new MailMessage("vidyadharhudge1997@gmail.com", emailAddress))
                    {
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        NetworkCredential NetworkCred = new NetworkCredential("vidyadharhudge1997@gmail.com", "Dhiraj@123#");
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = NetworkCred;
                        smtp.Port = 587;
                        smtp.Send(mailMessage);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }
        }
        /*public string SendEmail(string emailAddress)
        {
            string body;
            string subject = "Registration Credential";
            var entry = this._loginContext.Registrations.FirstOrDefault(x => x.Email == emailAddress);
            if (entry != null)
            {
                body = entry.Password;
            }
            else
            {
                return "Not Found";
            }
            using (MailMessage mailMessage = new MailMessage("hudge.trimbakeshwar@gmail.com", emailAddress))
            {
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("hudge.trimbakeshwar@gmail.com", "Suraj@1234#");
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mailMessage);
                return "SUCCESS";
            }
        }*/
    }
}