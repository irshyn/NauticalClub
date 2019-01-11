using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ISSail.Models
{
    public partial class MembershipType
    {
        public MembershipType()
        {
            Membership = new HashSet<Membership>();
        }
        [Display(Name = "Name")]
        public string MembershipTypeName { get; set; }
        public string Description { get; set; }
        [Display(Name = "Ratio To Full")]
        public double RatioToFull { get; set; }

        public ICollection<Membership> Membership { get; set; }
    }
}
