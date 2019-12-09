using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortfolioAnalyzer.Models
{
    public class Security
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Ticker { get; set; }
        [Required]
        public string Description { get; set; }

        public virtual ICollection<PortfolioSecurity> PortfolioSecurities { get; set; }
    }
}
