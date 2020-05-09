using System;
using System.Data.SqlClient;
using aplikacja5.DTOs.Requests;
using aplikacja5.Models;
using Microsoft.AspNetCore.Mvc;


namespace aplikacja5.Controllers
{

    [ApiController] // -> implicit model validation, gdy ta linijka jest aktywna, wtedy deklaratywne sprawdzanie poprawnosci danych

    public class PromoteStudentsController : ControllerBase
    {
        private const string CONNECTION_STRING = "Data Source=db-mssql;Initial Catalog=s16478;Integrated Security=True";

        [HttpPost]
        [Route("api/enrollments/promotions")]
        // promocja studentow na nowy semestr danych studiow
        public IActionResult PromoteStudent(PromoteStudentsRequest request)
        {

            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                var transaction = con.BeginTransaction();
                com.Transaction = transaction;

                com.CommandText = "execute spPromoteStudentToNextSemester @Name, @Semester;";
                com.Parameters.AddWithValue("Name", request.Name);
                com.Parameters.AddWithValue("Semester", request.Semester);


                try
                {
                    com.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    return BadRequest("Error - studies with given name do not exist!");
                }


                com.CommandText = "SELECT * FROM Enrollment WHERE IdStudy = (SELECT IdStudy FROM Studies WHERE Name = @Name AND Semester = @Semester + 1)";
                Enrollment newPromotionEnrollment = null;
                var dataReader = com.ExecuteReader();
                {
                    if (dataReader.Read())
                    {
                        newPromotionEnrollment = new Enrollment()
                        {
                            IdEnrollment = (int)dataReader["IdEnrollment"],
                            IdStudy = (int)dataReader["IdStudy"],
                            Semester = (int)dataReader["Semester"],
                            StartDate = (DateTime)dataReader["StartDate"]
                        };
                    }
                    else
                    {
                        return null;
                    }

                    dataReader.Close();
                    // returning 201 -- return Created (uri, objectValue)
                    return Created("api/enrollments/promotions", newPromotionEnrollment);
                }  // end of method PromoteStudent



            }
        }
    }
}

