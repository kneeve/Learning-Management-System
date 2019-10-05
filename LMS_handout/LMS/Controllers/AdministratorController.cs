﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subject">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from c in db.Courses
                        where c.Dept == subject
                        select new
                        {
                            number = c.Number,
                            name = c.Name
                        };
            return Json(query.ToArray());
        }





        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from p in db.Professors where p.Dept == subject
                        select new
                        {
                            lname = p.LastName,
                            fname = p.FirstName,
                            uid = p.UId
                        };
            return Json(query.ToArray());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            var query = from d in db.Departments
                        where d.Subject == subject
                        select new
                        {
                            courses = from c in db.Courses
                                      where c.Dept == d.Subject
                                      select new
                            {
                                number = c.Number,
                                name = c.Name
                            }
                        };

            var a = query.ToArray()[0];
            var x = a.courses.DefaultIfEmpty();
            var temp = x.ToArray()[0];
            if (temp == null || temp.number != number)
            {
                Courses newCourse = new Courses();
                newCourse.Name = name;
                newCourse.Number = (uint?)number;
                newCourse.Dept = subject;

                db.Courses.Add(newCourse);
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            DateTime sTime = DateTime.MinValue.Date + start.TimeOfDay;
            DateTime eTime = DateTime.MinValue.Date + end.TimeOfDay;

            var query = from d in db.Courses
                        select new
                        {
                            classes = from c in d.Classes.DefaultIfEmpty()
                                      select new
                                      {
                                          location = c.Location,
                                          startTime = c.Start,
                                          endTime = c.End,
                                          professor = c.Professor,
                                          semester = c.Semester,
                                          year = c.Year
                                          
                                      },
                           catID = d.CatalogId
                        };

            var catQuery = (from c in db.Courses
                           where c.Dept == subject && c.Number == number
                           select new
                           {
                               catID = c.CatalogId
                           }).ToArray()[0];

            var a = query.ToArray()[0];
            var x = a.classes.ToArray();

            foreach (var cl in x)
            {

                if (cl != null)
                {
                    DateTime classStart = (DateTime)cl.startTime;
                    DateTime classEnd = (DateTime)cl.endTime;
                    if (cl.location == location && cl.semester == season && cl.year == year &&
                        ((sTime.CompareTo(classStart) <= 0 && eTime.CompareTo(classEnd) <= 0) || (sTime.CompareTo(classStart) >= 0 && eTime.CompareTo(classEnd) >= 0)))
                    {
                        return Json(new { success = false });
                    }
                    else if(catQuery.catID == a.catID && cl.year == year && cl.semester == season)
                    {
                        return Json(new { success = false });
                    }
                    else
                    {
                        Classes newClass = new Classes();
                        newClass.ProfessorId = instructor;
                        newClass.Semester = season;
                        newClass.Location = location;
                        newClass.Start = sTime;
                        newClass.End = eTime;
                        newClass.Year = (uint?)year;
                        newClass.CatalogId = catQuery.catID;

                        db.Classes.Add(newClass);
                        db.SaveChanges();

                        return Json(new { success = true });
                    }
                }
                else
                {
                    Classes newClass = new Classes();
                    newClass.ProfessorId = instructor;
                    newClass.Semester = season;
                    newClass.Location = location;
                    newClass.Start = sTime;
                    newClass.End = eTime;
                    newClass.Year = (uint?)year;
                    newClass.CatalogId = a.catID;

                    db.Classes.Add(newClass);
                    db.SaveChanges();

                    return Json(new { success = true });
                }

            }
            return Json(new { success = false });
        }


        /*******End code to modify********/

    }
}