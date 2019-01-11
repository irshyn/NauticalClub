/*
 * The ISMemberMetadata class allows to create a self-validating class to validate
 * and edit the inputs
 * 
 * Revision History
 *         Iryna Shynkevych 2018-11-25 : created
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ISClassLibrary;

namespace ISSail.Models
{
    // partial class that links the metadata class to the model class, and that implements
    // Validate function that will handle all validations and edits
    [ModelMetadataType(typeof(TAMemberMetadata))]
    public partial class Member : IValidatableObject
    {
        // we need to create a context variable in order to access the database records
        SailContext _context = SailContext.Context;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            string countryCodeFromInput = "";

            // 2. b. Trim all strings of leading and trailing spaces (fields not trimmed here
            // are trimmed by TACapitalize and TAExtractDigits methods below)
            if (ProvinceCode != null) ProvinceCode = ProvinceCode.Trim();
            if (Email != null) Email = Email.Trim();
            if (Comment != null) Comment = Comment.Trim();

            // 2.d. Using our library method to capitalize names, address and city
            FirstName = ISValidations.ISCapitalize(FirstName);
            LastName = ISValidations.ISCapitalize(LastName);
            SpouseFirstName = ISValidations.ISCapitalize(SpouseFirstName);
            SpouseLastName = ISValidations.ISCapitalize(SpouseLastName);
            Street = ISValidations.ISCapitalize(Street);
            City = ISValidations.ISCapitalize(City);

            // 2.e. Compiling the full name from member's last and first name and
            // their spouse's names

            if (SpouseLastName == "" && SpouseFirstName == "")
            {
                FullName = LastName + ", " + FirstName;
            }
            else
            {
                if (SpouseLastName == "")
                {
                    FullName = LastName + ", " + FirstName + " & " + SpouseFirstName;
                }
                else
                {
                    if (SpouseLastName == LastName)
                    {
                        FullName = LastName + ", " + FirstName + " & " + SpouseFirstName;
                    }
                    else
                    {
                        FullName = LastName + ", " + FirstName + " & " + SpouseLastName +
                            ", " + SpouseFirstName;
                    }
                }
                if (SpouseFirstName == "")
                {
                    FullName = LastName + ", " + FirstName + " & " + SpouseLastName;
                }
            }

            // 2.f. Force the province code to upper cases and validate it
            if (ProvinceCode != null && ProvinceCode != "")
            {
                string errors = "";
                Province province = new Province();

                ProvinceCode = ProvinceCode.ToUpper();
                // we try to fetch a province record with the same province code from database
                try
                {
                    province = _context.Province.FirstOrDefault(p => p.ProvinceCode == ProvinceCode);
                }
                catch (Exception ex)
                {
                    errors = ex.GetBaseException().Message;
                }

                // if fetching province code throws an exception, put its innermost message in ModelState
                if (errors != "")
                {
                    yield return new ValidationResult(errors,
                    new[] { "ProvinceCode" });
                }
                // if it's not on the file, display an appropriate message
                if (province == null)
                    yield return new ValidationResult("This province code is not on the file",
                    new[] { "ProvinceCode" });
                else
                    // 2.f.iii. Retain a record's Country field to use it in the postal code validation
                    countryCodeFromInput = province.CountryCode;

                // 2.g.i. Validate and format the postal/zip code depending on the country of origin
                if (countryCodeFromInput == "CA")
                {
                    if (ISValidations.ISPostalCodeValidation(PostalCode))
                    {
                        PostalCode = ISValidations.ISPostalCodeFormat(PostalCode);
                    }
                    else
                    {
                        yield return new ValidationResult("The postal code must be in format A1A 1A1",
                            new[] { "PostalCode" });
                    }

                }
                else if (countryCodeFromInput == "US")
                {
                    string postalCode = PostalCode;
                    if (!ISValidations.ISZipCodeValidation(ref postalCode))
                    {
                        yield return new ValidationResult("This zip code must contain 5 or 9 digits",
                            new[] { "PostalCode" });
                    }
                    else PostalCode = postalCode;
                }
            }

            // 2.g. If the postal code is provided, but the province code isn't, throw an error
            else
            {
                if (PostalCode != null && PostalCode != string.Empty)
                    yield return new ValidationResult("The province/state code is required to validate the " +
                        "postal/zip code", new[] { "ProvinceCode" });
            }

            // 2.h.i. Home phone must contain 10 digits
            HomePhone = ISValidations.ISExtractDigits(HomePhone);
            if (HomePhone.Length != 10)
                yield return new ValidationResult("The phone number must be 10 digits long",
                    new[] { "HomePhone" });
            else
                // 2.h.ii. If it does, reformat it into dash notation
                HomePhone = HomePhone.Substring(0, 3) + "-" + HomePhone.Substring(3, 3) + "-" +
                    HomePhone.Substring(6);

            // Email address must be a valid email pattern
            if (!(new EmailAddressAttribute()).IsValid(Email))
                yield return new ValidationResult("The Email address is in incorrect format ",
                    new[] { "Email" });

            // 2.j. Year Joined can only be null when editing an existing record
            if (MemberId == 0 && YearJoined == null)
            {
                yield return new ValidationResult("Year Joined cannot be empty for a new " +
                        "record", new[] { "YearJoined" });
            }
            // 2.j. Year Joined cannot be in the future
            if (YearJoined != null)
            {
                if (YearJoined > DateTime.Now.Year)
                    yield return new ValidationResult("The year the member joined the club cannot " +
                        "be in the future", new[] { "YearJoined" });
            }

            // 2.k. default taskExepmpt and useCanadaPost to false
            if (TaskExempt == null)
                TaskExempt = false;
            if (UseCanadaPost == null)
                UseCanadaPost = false;

            // 2.l.i If useCanadaPost is false, valid email is required
            if (UseCanadaPost == false)
            {
                if (Email == null || Email == "")
                    yield return new ValidationResult("Email address is required unless member has " +
                        "restricted communication to Canada Post", new[] { "Email" });
            }
            // 2.l.ii. If useCanadaPost is true, all the postal fields are required
            else
            {
                if (Street == null || Street == "")
                    yield return new ValidationResult("Member wants to use Canada Post - Street " +
                    "Address is required", new[] { "Street" });
                if (City == null || City == "")
                    yield return new ValidationResult("Member wants to use Canada Post - City/Town " +
                    "is required", new[] { "City" });
                if (ProvinceCode == null || ProvinceCode == "")
                    yield return new ValidationResult("Member wants to use Canada Post - Province " +
                    "Code is required", new[] { "ProvinceCode" });
                if (PostalCode == null || PostalCode == "")
                    yield return new ValidationResult("Member wants to use Canada Post - Postal " +
                    "Code is required", new[] { "PostalCode" });
            }
            yield return ValidationResult.Success;
        }
    }

    // TAMemberMetadata metadata class that will allow us to define/add restraints to our class members
    // while avoiding overwriting validation attributes when generating class files
    public class TAMemberMetadata
    {
        public int MemberId { get; set; }
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Spouse First Name")]
        public string SpouseFirstName { get; set; }
        [Display(Name = "Spouse Last Name")]
        public string SpouseLastName { get; set; }
        [Display(Name = "Street Address")]
        public string Street { get; set; }
        [Display(Name = "City")]
        public string City { get; set; }
        [Display(Name = "Province Code")]
        public string ProvinceCode { get; set; }
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        [Required]
        [Display(Name = "Home Phone")]
        public string HomePhone { get; set; }
        public string Email { get; set; }
        [Display(Name = "Year Joined")]
        public int? YearJoined { get; set; }
        [Display(Name = "Comments")]
        public string Comment { get; set; }
        [Display(Name = "Task Exempt?")]
        public bool TaskExempt { get; set; }
        [Display(Name = "Use Canada Post?")]
        public bool UseCanadaPost { get; set; }
    }
}
