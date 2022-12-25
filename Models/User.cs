using System.ComponentModel.DataAnnotations;

namespace Qr_Generator.Models
{
    public class User
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public byte[]? QrCode { get; set; }


    }
}
