using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Models.LMSModels;

namespace LMS.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from q_course in db.Courses
                        where q_course.Dept == subject && q_course.Number == num
                        select new
                        {
                            classes = from q_class in q_course.Classes
                                      where q_class.Semester == season && q_class.Year == year
                                      select new
                                      {
                                          enrolled = from q_enrolled in q_class.Enrolled
                                                     select new
                                                     {
                                                         fname = q_enrolled.Student.FirstName,
                                                         lname = q_enrolled.Student.LastName,
                                                         uid = q_enrolled.StudentId,
                                                         dob = q_enrolled.Student.Dob,
                                                         grade = q_enrolled.Grade
                                                     }
                                      }
                        };
            var j_query = query.ToArray()[0];

            return Json(j_query.classes.ToArray()[0].enrolled.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category != null)
            {
                var asgQuery = from course in db.Courses
                               where course.Dept == subject && course.Number == num
                               join cl in db.Classes
                               on course.CatalogId equals cl.CatalogId
                               where cl.Semester == season && cl.Year == year
                               join aCat in db.AsgCats.DefaultIfEmpty()
                               on cl.ClassId equals aCat.ClassId
                               where aCat.Name == category
                               join asg in db.Assignments.DefaultIfEmpty()
                               on aCat.AsgCatId equals asg.AsgCatId
                               select new
                               {
                                   aname = asg.Name,
                                   cname = asg.AsgCat.Name,
                                   due = asg.Due,
                                   submissions = (from s in asg.Submission
                                                  select s).Count()
                               };

                return Json(asgQuery.ToArray());
            }
            else
            {
                var asgQuery = from course in db.Courses
                                              where course.Dept == subject && course.Number == num
                                              join cl in db.Classes
                                              on course.CatalogId equals cl.CatalogId
                                              where cl.Semester == season && cl.Year == year
                                              join aCat in db.AsgCats.DefaultIfEmpty()
                                              on cl.ClassId equals aCat.ClassId
                                              join asg in db.Assignments.DefaultIfEmpty()
                                              on aCat.AsgCatId equals asg.AsgCatId
                                              select new
                                              {
                                                  aname = asg.Name,
                                                  cname = asg.AsgCat.Name,
                                                  due = asg.Due,
                                                  submissions = (from s in asg.Submission
                                                                 select s).Count()
                                              };


                return Json(asgQuery.ToArray());
            }

            
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from q_course in db.Courses
                        where q_course.Dept == subject && q_course.Number == num
                        select new
                        {
                            classes = from q_class in q_course.Classes
                                      where q_class.Semester == season && q_class.Year == year
                                      select new
                                      {
                                          asg_cats = from q_asg_cats in q_class.AsgCats
                                                     select new
                                                     {
                                                         name = q_asg_cats.Name,
                                                         weight = q_asg_cats.Weight
                                                     }
                                      }
                        };
            return Json(query.ToArray()[0].classes.ToArray()[0].asg_cats.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {

            var query = (from q_courses in db.Courses
                         where q_courses.Dept == subject && q_courses.Number == num
                         select new
                         {
                             classes = from q_classes in q_courses.Classes
                                       where q_classes.Semester == season && q_classes.Year == year
                                       select new
                                       {
                                           class_id = q_classes.ClassId,
                                           asg_cats = q_classes.AsgCats
                                       }
                         }).ToArray();

            var assignmentCats = query[0].classes.ToArray()[0].asg_cats.ToArray();
            foreach(var ac in assignmentCats)
            {
                if (ac.Name == category)
                {
                    return Json(new { success = false });
                }
            }

            AsgCats a = new AsgCats();
            a.ClassId = query[0].classes.ToArray()[0].class_id;
            a.Name = category;
            a.Weight = (byte?) catweight;
            db.AsgCats.Add(a);
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var aQuery = from course in db.Courses
                         where course.Number == num && course.Dept == subject
                         join cl in db.Classes
                         on course.CatalogId equals cl.CatalogId
                         where cl.Semester == season && cl.Year == year
                         join aCat in db.AsgCats
                         on cl.ClassId equals aCat.ClassId
                         where aCat.Name == category
                         join asg in db.Assignments
                         on aCat.AsgCatId equals asg.AsgCatId
                         select asg;

            // Check if assignment name has been used already 
            foreach(var x in aQuery)
            {
                if (x.Name.Equals(asgname))
                {
                    return Json(new { success = false });
                }
            }


            // Add the assignment
            Assignments a = new Assignments();
            a.Name = asgname;
            a.MaxPoints = (ushort?)asgpoints;
            a.Contents = asgcontents;
            a.Due = asgdue;
            a.AsgCatId = aQuery.ToArray()[0].AsgCatId;
            db.Assignments.Add(a);
            db.SaveChanges();
            var gradeQuery = from course in db.Courses
                              where course.Number == num && course.Dept == subject
                              join cl in db.Classes
                              on course.CatalogId equals cl.CatalogId
                              where cl.Semester == season && cl.Year == year
                              join e in db.Enrolled
                              on cl.ClassId equals e.ClassId
                              select e;
            foreach (var v in gradeQuery)
            {
                string grade = AutoGrade(subject, num, year, season, v.StudentId);
                v.Grade = grade;
            }

            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from q_courses in db.Courses
                        where q_courses.Dept == subject && q_courses.Number == num
                        select new
                        {
                            classes = from q_classes in q_courses.Classes
                                      where q_classes.Semester == season && q_classes.Year == year
                                      select new
                                      {
                                          asg_cats = from q_asq_cats in q_classes.AsgCats
                                                     where q_asq_cats.Name == category
                                                     select new
                                                     {
                                                         assignments = from q_assignments in q_asq_cats.Assignments
                                                                       where q_assignments.Name == asgname
                                                                       select new
                                                                       {
                                                                           submissions = from q_submissions in q_assignments.Submission
                                                                                         select new
                                                                                         {
                                                                                             fname = q_submissions.Student.FirstName,
                                                                                             lname = q_submissions.Student.LastName,
                                                                                             uid = q_submissions.Student.UId,
                                                                                             time = q_submissions.Time,
                                                                                             score = q_submissions.Score
                                                                                         }
                                                                       }
                                                     }
                                      }
                        };

            return Json(query.ToArray()[0].classes.ToArray()[0].asg_cats.ToArray()[0].assignments.ToArray()[0].submissions.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = (from course in db.Courses
                         where course.Number == num && course.Dept == subject
                         join cl in db.Classes
                         on course.CatalogId equals cl.CatalogId
                         where cl.Semester == season && cl.Year == year
                         join aCat in db.AsgCats
                         on cl.ClassId equals aCat.ClassId
                         join asg in db.Assignments
                         on aCat.AsgCatId equals asg.AsgCatId
                         where asg.Name == asgname
                         join sub in db.Submission
                         on asg.AId equals sub.AId
                         where sub.StudentId == uid
                         select sub).SingleOrDefault();
            
            if (query != null)
            {
                query.Score = (uint?)score;
                string grade = AutoGrade(subject, num, year, season, uid);
                var gradeQuery = (from course in db.Courses
                                  where course.Number == num && course.Dept == subject
                                  join cl in db.Classes
                                  on course.CatalogId equals cl.CatalogId
                                  where cl.Semester == season && cl.Year == year
                                  join e in db.Enrolled
                                  on cl.ClassId equals e.ClassId
                                  where e.StudentId == uid
                                  select e).SingleOrDefault();

                gradeQuery.Grade = grade;
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = (from q_prof in db.Professors
                        where q_prof.UId == uid
                        select new
                        {
                            classes = from q_classes in q_prof.Classes
                                      select new
                                      {
                                          subject = q_classes.Catalog.Dept,
                                          number = q_classes.Catalog.Number,
                                          name = q_classes.Catalog.Name,
                                          season = q_classes.Semester,
                                          year = q_classes.Year
                                      }
                        }).ToArray();


            return Json(query[0].classes.ToArray());
        }

        /// <summary>
        /// Returns a grade letter as a string.
        /// </summary>
        /// <returns></returns>
        public string AutoGrade(string classSubject, int classNum, int year, string season, string uid)
        {
            var query = (from course in db.Courses
                        where course.Dept == classSubject && course.Number == classNum
                        join cl in db.Classes
                        on course.CatalogId equals cl.CatalogId
                        where cl.Year == year && cl.Semester == season
                        join e in db.Enrolled
                        on cl.ClassId equals e.ClassId
                        where e.StudentId == uid
                        join aCat in db.AsgCats
                        on e.ClassId equals aCat.ClassId
                        select new
                        {
                            aCatID = aCat.AsgCatId,
                            aCatWeight = aCat.Weight,
                            assignments = from asg in aCat.Assignments
                                           select new
                                           {
                                               asgTotalPoints = asg.MaxPoints,
                                               asgEarnedPoints = from sub in asg.Submission.DefaultIfEmpty()
                                                                 where sub.StudentId == uid
                                                                 select sub
                                                             
                                           }
                        }).DefaultIfEmpty();

            double totalCatWeight = 0.0;  // Sum of all assignment category weights (excluding categories with no assignments)
            double percentAsgInCat = 0.0;  // Sum of percentage calculation of (EARNED POINTS / TOTAL POINTS) * CATEGORY WEIGHT for all categories

            foreach (var category in query)
            {
                double asgSum = 0.0;
                double asgEarned = 0.0;
                double weight = (double)category.aCatWeight / 100;

                foreach(var asg in category.assignments)
                {
                    if (asg.asgEarnedPoints.ToArray()[0] != null)  // Only add to earned points total if there is a submission
                    {
                        asgEarned += (double)asg.asgEarnedPoints.ToArray()[0].Score;
                    }

                    asgSum += (double)asg.asgTotalPoints;  // Always account for assignment points in total, even if no submission
                }

                if (category.assignments != null)  // Only add to sum of category weights if there are assignments in the category
                {
                    totalCatWeight += (double)category.aCatWeight;
                }

                percentAsgInCat += (asgEarned / asgSum) * weight;
            }

            // Calculate total percentage by multiplying scaling factor by percentAsgInCat
            double scaleFactor = 100 / totalCatWeight;
            double percentage =  scaleFactor * percentAsgInCat;

            return GetLetterGrade(percentage);

        }

        /// <summary>
        /// Given a percentage, returns a letter based off of the percentage.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        public string GetLetterGrade(double percentage)
        {
            double x = 100 * percentage;

            if (x >= 93) return "A";
            else if (x < 93 && x >= 90) return "A-";
            else if (x < 90 && x >= 87) return "B+";
            else if (x < 87 && x >= 83) return "B";
            else if (x < 83 && x >= 80) return "B-";
            else if (x < 80 && x >= 77) return "C+";
            else if (x < 77 && x >= 73) return "C";
            else if (x < 73 && x >= 70) return "C-";
            else if (x < 70 && x >= 67) return "D+";
            else if (x < 67 && x >= 63) return "D";
            else if (x < 63 && x >= 60) return "D-";
            else return "E";
        }


        /*******End code to modify********/

    }
}