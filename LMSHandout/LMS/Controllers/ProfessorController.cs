using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
            {
                return Json(Array.Empty<object>());
            }

            var query = from e in db.Enrolleds
                        join s in db.Students on e.Student equals s.UId
                        where e.Class == classId
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob.ToString("yyyy-MM-dd"),
                            grade = e.Grade
                        };

            return Json(query.ToArray());
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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
                return Json(Array.Empty<object>());

            var query = from ac in db.AssignmentCategories
                        join a in db.Assignments on ac.CategoryId equals a.Category
                        where ac.InClass == classId &&
                              (string.IsNullOrEmpty(category) || ac.Name == category)
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count()
                        };

            return Json(query.ToArray());
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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
                return Json(Array.Empty<object>());

            var query = from ac in db.AssignmentCategories
                        where ac.InClass == classId
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };

            return Json(query.ToArray());
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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
                return Json(new { success = false });
            
            bool exists = db.AssignmentCategories.Any(ac => ac.InClass == classId && ac.Name == category);
            if (exists)
                return Json(new { success = false });

            var newCat = new AssignmentCategory
            {
                Name = category,
                Weight = (uint)catweight,
                InClass = classId
            };
            db.AssignmentCategories.Add(newCat);
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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
                return Json(new { success = false });

            var ac = (from cat in db.AssignmentCategories
                      where cat.InClass == classId && cat.Name == category
                      select cat).FirstOrDefault();
            if (ac == null)
                return Json(new { success = false });

            bool assignExists = db.Assignments.Any(a => a.Category == ac.CategoryId && a.Name == asgname);
            if (assignExists)
                return Json(new { success = false });

            var newAsg = new Assignment
            {
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Category = ac.CategoryId
            };
            db.Assignments.Add(newAsg);
            db.SaveChanges();

            UpdateClassGrades(classId);

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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
                return Json(Array.Empty<object>());

            var asgId = (from ac in db.AssignmentCategories
                         join a in db.Assignments on ac.CategoryId equals a.Category
                         where ac.InClass == classId && ac.Name == category && a.Name == asgname
                         select a.AssignmentId).FirstOrDefault();
            if (asgId == 0)
                return Json(Array.Empty<object>());

            var query = from s in db.Submissions
                        join st in db.Students on s.Student equals st.UId
                        where s.Assignment == asgId
                        select new
                        {
                            fname = st.FName,
                            lname = st.LName,
                            uid = st.UId,
                            time = s.Time,
                            score = s.Score
                        };

            return Json(query.ToArray());
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
            uint classId = GetClassId(subject, num, season, year);
            if (classId == 0)
            {
                return Json(new { success = false });
            }
            var asgId = (from ac in db.AssignmentCategories
                         join a in db.Assignments on ac.CategoryId equals a.Category
                         where ac.InClass == classId && ac.Name == category && a.Name == asgname
                         select a.AssignmentId).FirstOrDefault();
            if (asgId == 0)
            {
                return Json(new { success = false });
            }
            var submission = (from s in db.Submissions
                              where s.Assignment == asgId && s.Student == uid
                              select s).FirstOrDefault();
            if (submission == null)
            {
                submission = new Submission
                {
                    Assignment = asgId,
                    Student = uid,
                    Time = DateTime.Now,
                    Score = (uint)score,
                    SubmissionContents = ""
                };
                db.Submissions.Add(submission);
            }
            else
            {
                submission.Score = (uint)score;
            }
            db.SaveChanges();

            UpdateClassGrades(classId);
            return Json(new { success = true });
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
            var query = from cl in db.Classes
                        join c in db.Courses on cl.Listing equals c.CatalogId
                        where cl.TaughtBy == uid
                        select new
                        {
                            subject = c.Department,
                            number = c.Number,
                            name = c.Name,
                            season = cl.Season,
                            year = cl.Year
                        };
            return Json(query.ToArray());
        }

        /*******End code to modify********/

        /// <summary>
        /// Finds the ClassId.
        /// Returns 0 if not found.
        /// </summary>
        private uint GetClassId(string subject, int num, string season, int year)
        {
            var classId = (from co in db.Courses
                           join cl in db.Classes on co.CatalogId equals cl.Listing
                           where co.Department == subject &&
                                 co.Number == (uint)num &&
                                 cl.Season == season &&
                                 cl.Year == (uint)year
                           select cl.ClassId).FirstOrDefault();
            return classId;
        }

        /// <summary>
        /// Recalculates letter grades for every student enrolled in a class.
        /// For each assignment category (with at least one assignment), it computes:
        ///    fraction = (student's total score) / (total max points)
        ///    scaled   = fraction * (category weight)
        /// Sums across categories to get a final percentage, converts it to a letter grade,
        /// and stores it in Enrolled.Grade.
        /// </summary>
        private void UpdateClassGrades(uint classId)
        {
            var cats = db.AssignmentCategories
                         .Where(ac => ac.InClass == classId)
                         .Select(ac => new {
                             ac.CategoryId,
                             weight = ac.Weight,
                             MaxPoints = ac.Assignments.Sum(a => a.MaxPoints)
                         })
                         .Where(x => x.MaxPoints >= 0)
                         .ToList();

            double totalCatWeight = cats.Sum(x => x.weight);

            var students = db.Enrolleds.Where(e => e.Class == classId).ToList();

            foreach (var e in students)
            {
                double totalScaled = 0;

                foreach (var c in cats)
                {
                    double earned = db.Submissions
                                      .Where(s => s.AssignmentNavigation.Category == c.CategoryId
                                               && s.Student == e.Student)
                                      .Sum(s => (double?)s.Score) ?? 0;

                    double fraction = earned / c.MaxPoints;
                    totalScaled += fraction * c.weight;
                }

                double finalPct = totalCatWeight > 0 ? (totalScaled / totalCatWeight) * 100 : 0;
                e.Grade = ConvertToLetterGrade(finalPct);
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Converts a percentage to a letter grade.
        /// </summary>
        private static string ConvertToLetterGrade(double pct)
        {
            if (pct >= 93) return "A";
            if (pct >= 90) return "A-";
            if (pct >= 87) return "B+";
            if (pct >= 83) return "B";
            if (pct >= 80) return "B-";
            if (pct >= 77) return "C+";
            if (pct >= 73) return "C";
            if (pct >= 70) return "C-";
            if (pct >= 67) return "D+";
            if (pct >= 63) return "D";
            if (pct >= 60) return "D-";
            return "E";
        }
    }
}

