using System.Data.SqlClient;
using aplikacja5.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

namespace aplikacja5.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private const string CONNECTION_STRING = "Data Source=db-mssql;Initial Catalog=s16478;Integrated Security=True";
       
        [HttpGet]
        public IActionResult GetStudents()
        {
            var students = new List<Student>();

            // -------------------------------------   zadanie 4.1
            using (var client = new SqlConnection(CONNECTION_STRING))   
            using (var command = new SqlCommand())
            {
                command.Connection = client;

                // --------------------------------    zadanie 4.2 // dla IndexNumber wyświetla się null, bo nie ma go w zapytaniu SQLowym, gdyż nie było go w poleceniu do zadania
                command.CommandText = "SELECT Student.IndexNumber, Student.FirstName, Student.LastName, Student.BirthDate, Enrollment.Semester, Studies.Name From Enrollment JOIN Student on Enrollment.IdEnrollment = Student.IdEnrollment JOIN Studies on Enrollment.IdStudy = Studies.IdStudy";
                
                
                
                
                client.Open();
                SqlDataReader dataReader = command.ExecuteReader();  // strumień typu forward only
                while (dataReader.Read())
                {
                    var st = new Student();
                    st.FirstName = dataReader["FirstName"].ToString();
                    st.LastName = dataReader["LastName"].ToString();
                    st.BirthDate = DateTime.Parse(dataReader["BirthDate"].ToString());
                    st.Semester = (int)dataReader["Semester"];
                    st.Studies = dataReader["Name"].ToString();
                    students.Add(st);  // mam liste studentów sparsowaną do formatu JSON
       
                }
            }
            return Ok(students);
        }

        // ----------------------------------------   zadanie 4.5
        [HttpGet("{IndexNumber}")]
        public IActionResult GetStudent(string IndexNumber)
        {
            using (var client = new SqlConnection(CONNECTION_STRING))
            using (var command = new SqlCommand())
            {
                command.Connection = client;             
                command.CommandText = "SELECT Student.IndexNumber, Student.FirstName, Student.LastName, Student.BirthDate, Enrollment.Semester, Studies.Name FROM Enrollment JOIN Student ON Enrollment.IdEnrollment = Student.IdEnrollment JOIN Studies ON Enrollment.IdStudy = Studies.IdStudy WHERE IndexNumber = @IndexNo";


                // sposob I (dłuższy, ale można różne dodatkowe opcje używać)
                /*
                SqlParameter param = new SqlParameter();
                param.Value = IndexNumber;
                param.ParameterName = "IndexNo";
                command.Parameters.Add(param);
                */

                // sposob II (krótszy i wygodniejszy)
                command.Parameters.AddWithValue("IndexNo", IndexNumber);


                client.Open();
                var dataReader = command.ExecuteReader();
                if (dataReader.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dataReader["IndexNumber"].ToString();
                    st.FirstName = dataReader["FirstName"].ToString();
                    st.LastName = dataReader["LastName"].ToString();
                    st.BirthDate = DateTime.Parse(dataReader["BirthDate"].ToString());
                    st.Semester = (int)dataReader["Semester"];
                    st.Studies = dataReader["Name"].ToString();
                    return Ok(st);

                }

            }

            return NotFound();
        }

        // -----------------------------------------   zadanie 4.4

        /*
         * Atak SQL Injection jest bardzo częsty.
         * Jest wynikiem tego, że nie zabezpieczamy tego co nam klient bazy przekazuje (jako parametr).
           To co przekazuje klient zawsze może być potencjalnie niebezpieczne.

         * przykład takiego parametru dla powyższej metody
           to: http://localhost:49950/api/students/podany_parametr
           w miejscu podany_parametr możemy dokleić różne polecenia, np. jakiś ciąg znaków
           zakończony apostrofem, średnik, a pośredniuku kolejne polecenie SQL, które może
           być potencjalnie niebezpieczne, np. DROP TABLE Student (tak jak w poleceniu zadania).
           
           Cały doklejony parametr może wyglądać następująco:
           6423';DROP TABLE Student;--
                apostrof domyka ciąg znaków, apodwójny myślnik na końcu usuwa ostatni średnik
           
           Finalnie mamy:  http://localhost:49950/api/students/6423';DROP TABLE Student;--
         
           W ten sposób wykonywane są 2 polecenia SQL na raz.
         * 
         */

    }
}