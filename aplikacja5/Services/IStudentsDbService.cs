using System.Collections.Generic;
using aplikacja5.DTOs.Requests;
using aplikacja5.Models;


namespace aplikacja5.Services
{
    public interface IStudentsDbService
    {
        // from StudentsController
        public List<Student> GetStudents();  // cala lista studentow
        public Student GetStudent(string IndexNumber); // pojedynczy student po indeksie
        public void UpdateStudent(int id);
        public void DeleteStudent(int id);


        // from EnrollmentsController
        public Enrollment EnrollStudent(EnrollStudentRequest request);
        // from PromoteStudentsController
        public Enrollment PromoteStudents(PromoteStudentsRequest request);



    }
}
