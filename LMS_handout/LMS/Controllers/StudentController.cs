using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : CommonController
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolled
                        where e.StudentId == uid
                        join c in db.Classes
                        on e.ClassId equals c.ClassId
                        join course in db.Courses
                        on c.CatalogId equals course.CatalogId
                        select new
                        {
                            subject = course.Dept,
                            number = course.Number,
                            name = course.Name,
                            season = c.Semester,
                            year = c.Year,
                            grade = e.Grade
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON a\rray</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var cQuery = from course in db.Courses
                         where course.Number == num && course.Dept == subject
                         join cl in db.Classes
                         on course.CatalogId equals cl.CatalogId
                         where cl.Semester == season && cl.Year == year
                         join aCat in db.AsgCats
                         on cl.ClassId equals aCat.ClassId
                         join asg in db.Assignments
                         on aCat.AsgCatId equals asg.AsgCatId
                         select new
                         {
                             aname = asg.Name,
                             cname = aCat.Name,
                             due = asg.Due,
                             score = (from sub in db.Submission
                                      where sub.AId == asg.AId && sub.StudentId == uid
                                      select sub.Score).FirstOrDefault()
                         };

            return Json(cQuery);
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query = (from d in db.Courses
                        where d.Dept == subject && d.Number == num
                        join cl in db.Classes
                        on d.CatalogId equals cl.CatalogId
                        where cl.Semester == season && cl.Year == year
                        join aCat in db.AsgCats
                        on cl.ClassId equals aCat.ClassId
                        where aCat.Name == category
                        join a in db.Assignments
                        on aCat.AsgCatId equals a.AsgCatId
                        where a.Name == asgname
                        select new
                        {
                            aid = a.AId
                        }).ToArray();

            var subQuery = from s in db.Submission.DefaultIfEmpty()
                           where s.AId == query[0].aid && s.StudentId == uid
                           select s;
            Submission sub = subQuery.SingleOrDefault();
            if (sub != null)
            {
                subQuery.ToArray()[0].Contents = contents;
                subQuery.ToArray()[0].Time = DateTime.Now;

                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                Submission sub2 = new Submission();
                sub2.AId = query[0].aid;
                sub2.StudentId = uid;
                sub2.Score = 0;
                sub2.Contents = contents;
                sub2.Time = DateTime.Now;

                db.Submission.Add(sub2);
                db.SaveChanges();

                return Json(new { success = true });
            }

        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = (from c in db.Courses
                        where c.Dept == subject && c.Number == num
                        join cl in db.Classes
                        on c.CatalogId equals cl.CatalogId
                        where cl.Semester == season && cl.Year == year
                        from e in db.Enrolled.DefaultIfEmpty()
                        where cl.ClassId == e.ClassId && e.StudentId == uid
                        select new
                        {
                            grade = e.Grade,
                            classID = cl.ClassId
                        }).ToList();
            if (query.Count > 0)
            {
                return Json(new { success = false });
            }

            var classQuery = (from c in db.Courses
                             where c.Dept == subject && c.Number == num
                             join cl in db.Classes
                             on c.CatalogId equals cl.CatalogId
                             where cl.Semester == season && cl.Year == year
                             select new
                             {
                                 classID = cl.ClassId
                             }).ToArray();


            Enrolled en = new Enrolled();
            en.Grade = "--";
            en.ClassId = classQuery[0].classID;
            en.StudentId = uid;
            db.Enrolled.Add(en);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = from e in db.Enrolled
                        where e.StudentId == uid
                        select new
                        {
                            grade = e.Grade
                        };

            double creds = 0.0;
            double hours = 0.0;
            foreach(var g in query)
            {
                switch (g.ToString())
                {
                    case "--":
                        break;
                    case "A":
                        creds += 4 * 4.0;
                        hours += 4;
                        break;
                    case "A-":
                        creds += 4 * 3.7;
                        hours += 4;
                        break;
                    case "B+":
                        creds += 4 * 3.3;
                        hours += 4;
                        break;
                    case "B":
                        creds += 4 * 3.0;
                        hours += 4;
                        break;
                    case "B-":
                        creds += 4 * 2.7;
                        hours += 4;
                        break;
                    case "C+":
                        creds += 4 * 2.3;
                        hours += 4;
                        break;
                    case "C":
                        creds += 4 * 2.0;
                        hours += 4;
                        break;
                    case "C-":
                        creds += 4 * 1.7;
                        hours += 4;
                        break;
                    case "D+":
                        creds += 4 * 1.3;
                        hours += 4;
                        break;
                    case "D":
                        creds += 4 * 1.0;
                        hours += 4;
                        break;
                    case "D-":
                        creds += 4 * 0.7;
                        hours += 4;
                        break;
                    case "E":
                        hours += 4;
                        break;
                }
            }

            if (hours == 0.0)
            {
                return Json(new { gpa = 0.0 });
            }

            double GPA = creds / hours;
            return Json(new { gpa = GPA });
        }

        /*******End code to modify********/

    }
}