﻿using System.ComponentModel.DataAnnotations;
namespace MagnusApp.Shared.Models;

public class ClientInfo
{
    [Required(ErrorMessage = "The field Name is required")]
    //[Display(Name = "Full Name")]
    public string Name { get; set; }
    [Required(ErrorMessage = "Please enter an Email Address")]
    [EmailAddress(ErrorMessage = "Please enter a valid Email Address")]
    //[Display(Name = "Email Address")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Please enter a message")]
    //[Display(Name = "Message")]
    public string Message { get; set; }
}