using System.Data.SqlClient;
using aplikacja5.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using aplikacja5.Services;

namespace aplikacja5.Controllers
{

        [ApiController]
        
        public class StudentsController : ControllerBase
        {
            private readonly IStudentsDbService _studentsDbService;

            public StudentsController(IStudentsDbService studentsDbService)
            {
                _studentsDbService = studentsDbService;
            }

         private const string CONNECTION_STRING = "Data Source=db-mssql;Initial Catalog=s16478;Integrated Security=True";

        [Route("api/students")]
        [HttpGet]
        public IActionResult GetStudents()
        {
            return Ok(_studentsDbService.GetStudents());
        }

        // ----------------------------------------   zadanie 4.5
        [HttpGet("{IndexNumber}")]
        public IActionResult GetStudent(string IndexNumber)
        {
            var student = _studentsDbService.GetStudent(IndexNumber);
            if (student == null)
            {
                return BadRequest("Nie znaleziono studenta"); ;
            }
            return Ok(student);
        }

     

        [HttpPut("{id}")]

        public IActionResult PutStudent(int id)
        {

            return Ok("Update successful");
        }


        [HttpDelete("{id}")]

        public IActionResult DeleteStudent(int id)
        {

            return Ok("Student deleted");
        }

    }
}