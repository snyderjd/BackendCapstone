﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class PortfolioSecurity
    {
        public int Id { get; set; }
        [Required]
        public int PortfolioId { get; set; }
        public Portfolio Portfolio { get; set; }
        [Required]
        public int SecurityId { get; set; }
        public Security Security { get; set; }
        [Required]
        public int Weight { get; set; }
        [Required]
        public int AssetClassId { get; set; }
        public AssetClass AssetClass { get; set; }
        public List<Price> Prices { get; set; } = new List<Price>();
    }
}
