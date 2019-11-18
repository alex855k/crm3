using CRM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace CRM.Application.Core.ViewModels
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            UsersList = new List<User>();
        }
        public Guid Id { get; set; }
        [Required(ErrorMessageResourceName = "FirstNameRequired", ErrorMessageResourceType = typeof(Resources.Administration.User))]
        public string FirstName { get; set; }
        [Required(ErrorMessageResourceName = "LastNameRequired", ErrorMessageResourceType = typeof(Resources.Administration.User))]
        public string LastName { get; set; }
        [Required(ErrorMessageResourceName = "UserNameRequired", ErrorMessageResourceType = typeof(Resources.Administration.User))]
        [Remote("IsUserNameExist", "Users", AdditionalFields = "UserName,Id", ErrorMessageResourceName = "UserNameExist", ErrorMessageResourceType = typeof(Resources.Administration.User), HttpMethod = "Post")]
        public string UserName { get; set; }
        [Required, MinLength(6, ErrorMessageResourceName = "PasswordTooShort", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string Password { get; set; }
        [Compare("Password", ErrorMessageResourceName = "ConfirmPassword", ErrorMessageResourceType = typeof(Resources.Errors))]
        public string ConfirmPassword { get; set; }
        public  short MonHours { get; set; }
        public  short TueHours { get; set; }
        public  short WedHours { get; set; }
        public  short ThursHours { get; set; }
        public  short FriHours { get; set; }
        public  short SatHours { get; set; }
        public  short SunHours { get; set; }
        [Required(ErrorMessageResourceName = "EmailRequired", ErrorMessageResourceType = typeof(Resources.Administration.User))]
        [EmailAddress(ErrorMessageResourceName = "ValidEmail", ErrorMessageResourceType = typeof(Resources.Administration.User))]
        [Remote("IsEmailExist", "Users", AdditionalFields = "Email,Id", ErrorMessageResourceName = "EmailExist", ErrorMessageResourceType = typeof(Resources.Administration.User), HttpMethod = "Post")]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public List<User> UsersList { get; set; } 
        public List<Role> UserRoles { get; set; }
        public bool IsEnabled { get; set; }
    }
}