using System;
using System.ComponentModel.DataAnnotations;

namespace CarUpdaterBot.Models
{
    public class CarInfo
    {
        [Key]
        public int Id { get; set; }

        public string Url { get; set; }

        public string Price { get; set; }

        public string Name { get; set; }

        public string Year { get; set; }

        public string Data { get; set; } = DateTime.Now.ToShortDateString();

        public string Description { get; set; }
    }
}