using System;
using System.Data.SqlClient;
using aplikacja5.DTOs.Requests;
using aplikacja5.Models;
using Microsoft.AspNetCore.Mvc;


namespace aplikacja5.Controllers
{

    [ApiController] // -> implicit model validation, gdy ta linijka jest aktywna, wtedy deklaratywne sprawdzanie poprawnosci danych
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private const string CONNECTION_STRING = "Data Source=db-mssql;Initial Catalog=s16478;Integrated Security=True";
        int _idStudies;
        int _generalMaxIdEnrollment = 1;
        int _idEnrollment = 1;
        [HttpPost]
        // dodanie nowego studenta i zapisanie go na semestr
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            var st = new Student();
            st.FirstName = request.FirstName;
            st.LastName = request.LastName;
            st.IndexNumber = request.IndexNumber;
            st.BirthDate = request.BirthDate;
            st.Studies = request.Studies;

            using (var con = new SqlConnection(CONNECTION_STRING))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                var transaction = con.BeginTransaction();
                com.Transaction = transaction;

                try
                {
                    // sprawdzam czy podane studia istnieja w bazie?
                    com.CommandText = "SELECT IdStudy from Studies WHERE Name = @name";
                    com.Parameters.AddWithValue("name", request.Studies);

                    var dataReader = com.ExecuteReader();
                    if (!dataReader.Read())
                    {
                        transaction.Rollback();
                        return BadRequest("Studies with given name do not exist");
                    }
                    // numer studies id
                    _idStudies = (int)dataReader["IdStudy"];

                    dataReader.Close();


                    // wyciągam ogólnie największy IdEnrollment, żeby póżniej dodać nowy Enrollment z tą wartoscia zwiekszona o 1
                    com.CommandText = "select max(ISNULL(IdEnrollment,0)) from Enrollment";
                    dataReader = com.ExecuteReader();
                    if (dataReader.Read())
                    {
                        _generalMaxIdEnrollment = (int)dataReader[0];
                    }


                    dataReader.Close();

                    // Sprawdzam czy jest enrollment dla danych studiow na semestr 1?
                    com.CommandText = "SELECT Max(IdEnrollment) from Enrollment WHERE Semester = 1 and IdStudy = (SELECT IdStudy from Studies WHERE Name = @studyName)";
                    com.Parameters.AddWithValue("studyName", request.Studies);
                    dataReader = com.ExecuteReader();

                    if (!dataReader.Read())
                    {
                        DateTime currentDate = DateTime.Now;
                        _idEnrollment = _generalMaxIdEnrollment + 1;
                        com.CommandText = "INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate) VALUES(@nextIdEnrollment, 1, @idStudies, @currentDate)";
                        com.Parameters.AddWithValue("nextIdEnrollment", _idEnrollment);
                        com.Parameters.AddWithValue("idStudies", _idStudies);
                        com.Parameters.AddWithValue("currentDate", currentDate);
                        
                    }
                    _idEnrollment = (int)dataReader[0];
                    dataReader.Close();


                    // sprawdzenie czy istnieje w bazie ktos o podanym numerze indeksu
                    com.CommandText = "SELECT * FROM Student WHERE IndexNumber = @givenIndex";
                    com.Parameters.AddWithValue("givenIndex", request.IndexNumber);
                    dataReader = com.ExecuteReader();
                    if (dataReader.Read())
                    {
                        // jezeli istnieje wpis do bazy o podanym numerze indeksu
                        transaction.Rollback();
                        return BadRequest("Someone with given IndexNumber already exists in this database");
                    }
                    else
                    {
                        // jezeli nie istnieje wpis w tabeli Student z podanym numerem indeksu, to dodanie studenta
                        com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) " +
                            "VALUES(@indexx, @firstname, @lastname, @birthdate, @enrollmentId)";
                        com.Parameters.AddWithValue("indexx", request.IndexNumber);
                        com.Parameters.AddWithValue("firstname", request.FirstName);
                        com.Parameters.AddWithValue("lastname", request.LastName);
                        com.Parameters.AddWithValue("birthdate", request.BirthDate);
                        com.Parameters.AddWithValue("enrollmentId", _idEnrollment);
                        dataReader.Close();

                        com.ExecuteNonQuery();

                    }
                    

                    transaction.Commit();
                    
                }
                catch (SqlException exc)
                {
                    transaction.Rollback();
                }

            }
            Enrollment newStudentEnrollment = new Enrollment
            {
                IdEnrollment = _idEnrollment,
                Semester = 1,
                IdStudy = _idStudies,
                StartDate = DateTime.Now
            };

            return Ok(newStudentEnrollment);
        }  // end of method EnrollStudent

    }
}

