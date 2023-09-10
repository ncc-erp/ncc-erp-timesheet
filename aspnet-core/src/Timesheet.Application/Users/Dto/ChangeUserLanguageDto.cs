using System.ComponentModel.DataAnnotations;

namespace Ncc.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}