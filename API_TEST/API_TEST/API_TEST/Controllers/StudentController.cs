using API_TEST.DB_Context;
using API_TEST.Models;
using Azure;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Text;



namespace API_TEST.Controllers
{
    /// <summary>
    /// student Contorller 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly DB_Connection _databaseService;
        private readonly string _imageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");
        private readonly string _connectionString;
        private IConfiguration _configuration;
        

        /// <summary>
        /// initializing ojects
        /// </summary>
        /// <param name="databaseService"></param>
        /// <param name="configuration"></param>
        public StudentController(DB_Connection databaseService, IConfiguration configuration)
        {
            _databaseService = databaseService;
            _connectionString = configuration.GetConnectionString("DbConnection");
            _configuration = configuration;
        }
        /// <summary>
        /// Add new Profile
        /// </summary>
        /// <param name="studentDto"></param>
        /// <returns></returns>
        [HttpPost("Profile")]
        
        public async Task<IActionResult> CreateStudent([FromForm] Student studentDto)
        {
            // Validate input
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure directory exists
            if (!Directory.Exists(_imageFolderPath))
            {
                Directory.CreateDirectory(_imageFolderPath);
            }

            // Compress and save main image
            var mainImagePath = await SaveCompressedImage(studentDto.MainImage);
            if (string.IsNullOrEmpty(mainImagePath)) { return NotFound(); }
            // Compress and save secondary images
            var secondaryImagePaths = new List<string>();
            if (studentDto.SecondaryImages != null && studentDto.SecondaryImages.Length > 0)
            {
                foreach (var imageFile in studentDto.SecondaryImages)
                {
                    var imagePath = await SaveCompressedImage(imageFile);
                    secondaryImagePaths.Add(imagePath);
                }
            }

            // Save student to the database
           
            using (var connection = new SqlConnection(_connectionString))
            {
                
                string storeproc = "Save_Student";
                var command = new SqlCommand(storeproc, connection);
                command.CommandType = CommandType.StoredProcedure;
              
                
                command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 50) { Value = studentDto.StudentName });
                command.Parameters.Add(new SqlParameter("@RegistrationNumber", SqlDbType.NVarChar, 10) { Value = studentDto.StudentRegNo });
                command.Parameters.Add(new SqlParameter("@RegistrationDate", SqlDbType.DateTime) { Value = studentDto.RegistrationDate.HasValue ? (object)studentDto.RegistrationDate.Value : DBNull.Value });
                command.Parameters.Add(new SqlParameter("@MainImagePath", SqlDbType.NVarChar, -1) { Value = mainImagePath });
                command.Parameters.Add(new SqlParameter("@SecondaryImagePaths", SqlDbType.NVarChar, -1) { Value = string.Join(",", secondaryImagePaths) });

                // Add output parameter
                var newStudentIdParam = new SqlParameter("@NewStudentId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(newStudentIdParam);
                //connection opened
                await connection.OpenAsync();
                var studentId = await command.ExecuteScalarAsync();
                // Retrieve last Input ID
                int newStudentId = (int)newStudentIdParam.Value;
                // Return data as JSON
                return Ok(new
                {
                    Id = newStudentId,
                    studentDto.StudentName,
                    studentDto.StudentRegNo,
                    studentDto.RegistrationDate,
                    MainImagePath = mainImagePath,
                    SecondaryImagePaths = secondaryImagePaths
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
         async Task<string> SaveCompressedImage(IFormFile imageFile)
        {

            var fileName = Path.GetFileName(imageFile.FileName);
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

            // Validate file extension
            if (fileExtension != ".jpg" && fileExtension != ".jpeg")
            {
                throw new InvalidOperationException("Only JPG files are supported.");
            }

            var filePath = Path.Combine(_imageFolderPath, fileName);

            // Generate a unique file name if the file already exists
            var uniqueFilePath = filePath;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            int counter = 1;
            string returnfilename = "";
            while (System.IO.File.Exists(uniqueFilePath))
            {
                var newFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
                uniqueFilePath = Path.Combine(_imageFolderPath, newFileName);
                counter++;
                returnfilename = $"UploadedImages/{newFileName}";
            }
            
            // Process and save the image
            using (var image = await Image.LoadAsync(imageFile.OpenReadStream()))
            {
                var encoder = new JpegEncoder
                {
                    Quality = 75 // Adjust the quality as needed
                };

                using (var fileStream = new FileStream(uniqueFilePath, FileMode.Create))
                {
                    await image.SaveAsync(fileStream, encoder);
                }
            }

            return returnfilename; 
        }
        /// <summary>
        /// Display All Data of Students
        /// </summary>
        /// <returns></returns>
        [HttpGet("Profile_List")]
        public async Task<IActionResult> ViewProfile() 
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                string storeproc = "Get_Student_Data";
                var command = new SqlCommand(storeproc, connection);
                command.CommandType = CommandType.StoredProcedure;


                var students = new List<StudentResponse>();
                //connection opened
                await connection.OpenAsync();
                
                using (DataTable DTB = new DataTable()) 
                {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    // Fill the DTB with the results
                    adapter.Fill(DTB);
                        if (DTB is not null) 
                        {
                            foreach(DataRow dr in DTB.Rows) 
                            {
                                students.Add(new StudentResponse() 
                                {
                                    ID =Convert.ToInt32( dr["ID"]),

                                    StudentName = dr["Name"].ToString(),
                                    StudentRegNo = dr["RegistrationNumber"].ToString(),
                                    RegistrationDate = dr["RegistrationDate"].ToString(),
                                    MainImage = dr["MainImagePath"].ToString(),
                                    SecondaryImages = dr["SecondaryImagePaths"].ToString()
                                });
                            }
                        }
                }
                }


                
                // Return data as JSON
                if (students.Count > 0)
                {
                    return Ok(students);
                }
                else 
                {
                    return Ok("no data Found");
                }

            }
        }
        /// <summary>
        /// Get Student ID and Return Token
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost("Profile_Details")]
        public async Task<IActionResult> ProfileDetails(int ID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {

                string storeproc = "GET_STD_BY_ID";
                var command = new SqlCommand(storeproc, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@ID", ID));


                string AccessToken="";
                //connection opened
                await connection.OpenAsync();

                using (DataTable DTB = new DataTable())
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        // Fill the DTB with the results
                        adapter.Fill(DTB);
                        if (DTB is not null)
                        {
                            //Generating Token
                            
                            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            var iat = (int)(DateTime.UtcNow - unixEpoch).TotalSeconds;
                            var claims = new[] {
                            new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.Iat, iat.ToString()),
                            new Claim("ID", ID.ToString()),
                            
                            };


                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                            var token = new JwtSecurityToken(
                                _configuration["Jwt:Issuer"],
                                _configuration["Jwt:Audience"],
                                claims,
                                expires: DateTime.UtcNow.AddMinutes(180),
                                signingCredentials: signIn);


                             AccessToken = new JwtSecurityTokenHandler().WriteToken(token);



                        }
                    }
                }


                // Retrieve the output parameter value

                var response = new
                {
                    access_token = AccessToken,
                    token_type = "bearer"
                };
                var jsonResponse = JsonConvert.SerializeObject(response);
                // Return data as JSON
                return Ok(jsonResponse);
            }
        }
        /// <summary>
        /// Retrieve Images after validating Token
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="ImageType"></param>
        /// <returns></returns>
        [HttpPost("Retrieve_Images")]
        public async Task<IActionResult> Retrieve_Images([FromQuery] RetrieveImage OBJ)
        {
            int ID;
            string type=OBJ.imageType.ToString();
        
            if (!ValidateJwtToken(OBJ.Token, _configuration["Jwt:Key"])) 
            
            {
                return BadRequest("Token Expire or image not found");
            }
            else
            {

                using (var connection = new SqlConnection(_connectionString))
                {
                    ID = Convert.ToInt32(GetClaimValue(OBJ.Token));
                    string storeproc = "GET_STD_BY_ID";
                    var command = new SqlCommand(storeproc, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ID", ID));

                    string pathfile = "";
                    var imglist=new List<string>();
                    //connection opened
                    await connection.OpenAsync();

                    using (DataTable DTB = new DataTable())
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            // Fill the DTB with the results
                            adapter.Fill(DTB);
                            if (DTB is not null)
                            {
                                foreach (DataRow dr in DTB.Rows)
                                {
                                    pathfile = dr["MainImagePath"].ToString();

                                    string[] filePathArray = dr["SecondaryImagePaths"].ToString().Split(',');
                                    
                                    foreach(var item in filePathArray) 
                                    {
                                        imglist.Add(item);
                                    }
                                }


                            }
                        }
                    }
                   

                    // Display Images in Browser
                    if (!string.IsNullOrEmpty(pathfile)&& type==ImageType.Main.ToString())
                    {
                       

                        // Check if the file exists
                        if (!System.IO.File.Exists(pathfile))
                        {
                            return NotFound(); // Return 404 if file does not exist
                        }

                        // Read the file bytes
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(pathfile);

                        // Determine the content type (e.g., image/jpeg)
                        var contentType = "image/jpeg"; // Adjust content type based on the file extension

                        // Return the file as a FileResult
                        return File(fileBytes, contentType);
                        
                    }
                    else if(imglist !=null) 
                    {
                        foreach(var item in imglist) 
                        {
                            

                            // Check if the file exists
                            if (!System.IO.File.Exists(item))
                            {
                                return NotFound(); // Return 404 if file does not exist
                            }

                            // Read the file bytes
                            //var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                            var fileExtension = Path.GetExtension(item).ToLowerInvariant();
                           
                            var fileStream = new FileStream(item, FileMode.Open, FileAccess.Read);
                            // Determine the content type (e.g., image/jpeg)
                            var contentType = "image/jpeg"; // Adjust content type based on the file extension

                            // Return the file as a FileResult
                             return File(fileStream, contentType);
                           

                           
                        }
                        
                    }
                 
                    return Ok();
                }



            }
            
        }
        /// <summary>
        /// Get Student ID
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        string GetClaimValue(string token)
        {
            var principal = GetClaimsPrincipal(token, _configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], _configuration["Jwt:Key"]);

            if (principal != null)
            {
                var claim = principal.FindFirst("ID");
                return claim?.Value;
            }

            return null;
        }
        /// <summary>
        /// Validate Token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        bool ValidateJwtToken(string token, string secretKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Claim Values
        /// </summary>
        /// <param name="token"></param>
        /// <param name="_issuer"></param>
        /// <param name="_audience"></param>
        /// <param name="_key"></param>
        /// <returns></returns>
         ClaimsPrincipal GetClaimsPrincipal(string token,string _issuer,string _audience,string _key)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _issuer,
                    ValidAudience = _audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
                };

                // Validate the token and get the ClaimsPrincipal
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                // Token validation failed
                return null;
            }
        }
    }
}
