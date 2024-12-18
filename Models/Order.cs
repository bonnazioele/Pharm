﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pharm.Models;

namespace Pharm.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Reference to the logged-in user

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
    }
}