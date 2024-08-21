using System.ComponentModel.DataAnnotations;

namespace API_TEST.Models
{/// <summary>
/// 
/// </summary>
    public class StudentResponse
    {/// <summary>
    /// 
    /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
    public string StudentName { get; set; }
        ///
    public string StudentRegNo { get; set; }

 
    public string MainImage { get; set; }


    public string SecondaryImages { get; set; }

    public string RegistrationDate { get; set; }
}
}
