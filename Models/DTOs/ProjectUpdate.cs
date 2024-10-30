using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    // خاص بتحديث معلومات المشروع
    public class ProjectUpdate
    {
        [StringLength(200)]
        [MinLength(2, ErrorMessage = "يرجى ادخال  عنوان صالح ")]
        public string Title { get; set; }

        public string Description { get; set; }
    }
}
