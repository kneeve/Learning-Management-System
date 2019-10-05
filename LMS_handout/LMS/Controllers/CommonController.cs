using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        // TODO: Uncomment and change 'X' after you have scaffoled


        protected Team20LMSContext db;

        public CommonController()
        {
            db = new Team20LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        // TODO: Uncomment and change 'X' after you have scaffoled

        public void UseLMSContext(Team20LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Departments
                        select new
                        {
                            name = d.Name,
                            subject = d.Subject
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from d in db.Departments
                        join c in db.Courses
                        on d.Subject equals c.Dept
                        into dJoinc
                        select new
                        {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = from dc in dJoinc
                                      select new
                                      {
                                          number = dc.Number,
                                          cname = dc.Name
                                      }
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var q1 = from course in db.Courses
                     where course.Dept == subject && course.Number == number
                     select new
                     {
                         q_classes = from c in course.Classes.DefaultIfEmpty()
                                     select new
                                     {
                                         season = c.Semester,
                                         year = c.Year,
                                         location = c.Location,
                                         start = c.Start.ToString().Split()[1],
                                         end = c.End.ToString().Split()[1],
                                         fname = c.Professor.FirstName,
                                         lname = c.Professor.LastName
                                     }
                     };

            return Json(q1.ToArray()[0].q_classes);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = (from q_course in db.Courses
                        where q_course.Dept == subject && q_course.Number == num
                        select new
                        {
                            classes = from q_classes in q_course.Classes
                                      where q_classes.Semester == season && q_classes.Year == year
                                      select new
                                      {
                                          asg_cats = from q_asg_cats in q_classes.AsgCats
                                                     where q_asg_cats.Name == category
                                                     select new
                                                     {
                                                         assignment = from q_assignment in q_asg_cats.Assignments
                                                                      where q_assignment.Name == asgname
                                                                      select new
                                                                      {
                                                                          contents = q_assignment.Contents
                                                                      }
                                                     }
                                      }
                        }).ToArray();
            var contents = query[0].classes.ToArray()[0].asg_cats.ToArray()[0].assignment.ToArray()[0].contents;

            return Content(contents);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query = (from course in db.Courses
                         where course.Dept == subject && course.Number == num
                         join cl in db.Classes
                         on course.CatalogId equals cl.CatalogId
                         where cl.Semester == season && cl.Year == year
                         join aCat in db.AsgCats
                         on cl.ClassId equals aCat.ClassId
                         where aCat.Name == category
                         join asg in db.Assignments
                         on aCat.AsgCatId equals asg.AsgCatId
                         where asg.Name == asgname
                         join sub in db.Submission
                         on asg.AId equals sub.AId
                         where sub.StudentId == uid
                         select sub.Contents).FirstOrDefault();
            
            return Content(query);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var s_query = (from q_students in db.Students
                          where q_students.UId == uid
                          select new
                          {
                              fname = q_students.FirstName,
                              lname = q_students.LastName,
                              uid = q_students.UId,
                              department = q_students.Major
                          }).FirstOrDefault();
            if (s_query == null)
            {
                var p_query = (from q_professors in db.Professors
                              where q_professors.UId == uid
                              select new
                              {
                                  fname = q_professors.FirstName,
                                  lname = q_professors.LastName,
                                  uid = q_professors.UId,
                                  department = q_professors.Dept
                              }).FirstOrDefault();
                if (p_query == null)
                {
                    var a_query = (from q_admins in db.Administrators
                                  where q_admins.UId == uid
                                  select new
                                  {
                                      fname = q_admins.FirstName,
                                      lname = q_admins.LastName,
                                      uid = q_admins.UId,
                                  }).FirstOrDefault();
                    if (a_query == null)
                    {
                        return Json(new { success = false });
                    }
                    else
                    {
                        return Json(a_query);
                    }
                }

                return Json(p_query);
            }

            return Json(s_query);
        }


        /*******End code to modify********/

    }
}