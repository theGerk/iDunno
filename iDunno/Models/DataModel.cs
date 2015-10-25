using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;

namespace iDunno.Models
{

    public class HomeScreen
    {
        public IEnumerable<TargetItem> Items { get; set; }
        public HomeScreen()
        {
            Items = new List<TargetItem>();
        }
    }



    //MongoDB command line -- mongod.exe --replSet idnSet
    public class LoginScreen
    {
        [Required]
        [Display(Name ="User name")]
        public string UserName { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }
    }
    public class PasswordAndConfirmPasswordMatch:ValidationAttribute
    {
        public PasswordAndConfirmPasswordMatch()
        {

        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //value is password field
            dynamic duo = validationContext.ObjectInstance;
            string confirmPassword = duo.ConfirmPassword.ToString();
            return confirmPassword == value as string ? ValidationResult.Success : new ValidationResult("Password and confirm password don't match.");
        }
    }
    public class RegistrationScreen
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }
        [PasswordAndConfirmPasswordMatch]
        [DataType(DataType.Password)]
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Required]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }



    }

    public class UserInformation
    {
        public string Username { get; set; }
        
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IpAddress { get; set; }
        public DateTime LastAccessTime { get; set; }

    }

    public class SessionInformation
    {
        public BsonObjectId Id { get; set; }
        public string SessionID { get; set; }
        public string UserName { get; set; }
    }

    public class iDunnoDB
    {
        IMongoDatabase db;
        public async Task<bool> IsLoginValid(string username, string password)
        {
           var users =  await (await db.GetCollection<UserInformation>("users").FindAsync(Builders<UserInformation>.Filter.Eq(m => m.Username, username))).ToListAsync();
            if(!users.Any())
            {
                return false;
            }
            var user = users.First();
            using (Rfc2898DeriveBytes mDerive = new Rfc2898DeriveBytes(password, user.Salt))
            {
                byte[] hash = mDerive.GetBytes(32);
                for(int i = 0;i<hash.Length;i++)
                {
                    if(hash[i] != user.Password[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public async Task CreateUser(RegistrationScreen screen)
        {
            
            using (RandomNumberGenerator mrand = RandomNumberGenerator.Create()) {
                byte[] salt = new byte[32];
                mrand.GetBytes(salt);
                using (Rfc2898DeriveBytes mderive = new Rfc2898DeriveBytes(screen.Password,salt))
                {
                    byte[] hash = mderive.GetBytes(32);
                    UserInformation info = new UserInformation() { FirstName = screen.FirstName, LastName = screen.LastName, IpAddress = HttpContext.Current.Request.UserHostName, LastAccessTime = DateTime.Now };
                    info.Password = hash;
                    info.Salt = salt;
                    await db.GetCollection<UserInformation>("users").InsertOneAsync(info);
                }
            }

           
        }


        public async Task<SessionInformation> getCurrentSession()
        {
            var request = System.Web.HttpContext.Current.Request;
            var response = System.Web.HttpContext.Current.Response;
            if(request.Cookies["session"] == null)
            {
                //No session cookie
                var thisSessionInformation = new SessionInformation();
                thisSessionInformation.SessionID = Guid.NewGuid().ToString();
                response.AppendCookie(new HttpCookie("session", thisSessionInformation.SessionID));
                await db.GetCollection<SessionInformation>("sessions").InsertOneAsync(thisSessionInformation);
                return thisSessionInformation;
            }

            return (await (await db.GetCollection<SessionInformation>("sessions").FindAsync(Builders<SessionInformation>.Filter.Eq(m => m.SessionID, request.Cookies["session"].Value))).ToListAsync()).First();


        }

        public iDunnoDB()
        {
            MongoDB.Driver.MongoClient client = new MongoDB.Driver.MongoClient(new MongoDB.Driver.MongoClientSettings() { ReplicaSetName = "idnSet" });
            db = client.GetDatabase("iDunno");

        }

    }
}