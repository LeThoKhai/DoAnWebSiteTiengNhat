using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2019.Excel.ThreadedComments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebSiteHocTiengNhat.Models;
using WebSiteHocTiengNhat.Repository;
using WebSiteHocTiengNhat.ViewModels;

namespace WebSiteHocTiengNhat.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class ExercisesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICoursesRepository _coursesRepository;
        private readonly IExercisesRepository _exercisesRepository;
        private readonly IQuestionRepository _questionRepository;

        public ExercisesController(IQuestionRepository questionRepository,ICategoryRepository categoryRepository, ICoursesRepository coursesRepository, IExercisesRepository exercisesRepository)
        {
            _categoryRepository = categoryRepository;
            _coursesRepository = coursesRepository;
            _questionRepository = questionRepository;
            _exercisesRepository = exercisesRepository;
        }
        public async Task<IActionResult> Index()
        {
            var course = await _coursesRepository.GetAllAsync();
            return View(course);
        }
        public async Task<IActionResult> ExerciseList(int courseId, int? categoryId)
        {
            var exercise = await _exercisesRepository.GetAllAsync();
            var filteredexercise = exercise.Where(n => n.CourseId == courseId);
            if (categoryId.HasValue)
            {
                filteredexercise = filteredexercise.Where(n => n.CategoryId == categoryId);
            }
            var course = await _coursesRepository.GetByIdAsync(courseId);
            course.quantity=course.quantity ++;
            var courseName = course?.CourseName;
            var categories = await _categoryRepository.GetAllAsync();
            var categoryList = categories.Select(n => new SelectListItem
            {
                Value = n.CategoryId.ToString(),
                Text = n.CategoryName,
            }).ToList();
            var viewModel = new ExerciseViewModel
            {
                CourseId = courseId,
                CourseName = courseName,
                Exercises = filteredexercise.ToList(),
                Categories = categoryList,

            };
            return View(viewModel);
        }

        public async Task<IActionResult> Create(int courseId)
        {
            var exercise = new Exercise { CourseId = courseId };
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(exercise);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Exercise exercise)
        {
            if (ModelState.IsValid)
            {
                await _exercisesRepository.AddAsync(exercise);
                return RedirectToAction(nameof(ExerciseList), new { courseId = exercise.CourseId });
            }
            return RedirectToAction(nameof(ExerciseList), new { courseId = exercise.CourseId });
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int Id)
        {
            var exercise = await _exercisesRepository.GetByIdAsync(Id);
            var question = await _questionRepository.GetAllAsync();
            var filterquestion= question.Where(n=>n.ExerciseId == Id);
            ViewBag.ExerciseId = exercise.ExerciseId;
            var viewmodel = new ExerciseDetailViewModel
            {
                ExerciseId = Id,
                ExerciseName = exercise.ExerciseName,
                questions = filterquestion.ToList(),
                CourseId=exercise.CourseId,
            };
            if (exercise == null)
            {
                return NotFound();
            }
            
            return View(viewmodel);
        }
        public async Task<IActionResult> Update(int id)
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            var exercise = await _exercisesRepository.GetByIdAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            return View(exercise);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, Exercise exercise)
        {
            if (id != exercise.ExerciseId)
            {
                return NotFound();
            }
            var exs = await _exercisesRepository.GetByIdAsync(id);
            if (ModelState.IsValid)
            {
                var existingProduct = await _exercisesRepository.GetByIdAsync(id);
                existingProduct.ExerciseName = exercise.ExerciseName;
                existingProduct.CategoryId = exercise.CategoryId;
                existingProduct.Content = exercise.Content;
                await _exercisesRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(ExerciseList), new { courseId = existingProduct.CourseId });
            }
            return RedirectToAction(nameof(ExerciseList), new { courseId = exs.CourseId });
        }
        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var exercise = await _exercisesRepository.GetByIdAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            return View(exercise);
        }
        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id, Exercise exercise)
        {
            var lesson = await _exercisesRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            int idcourse = lesson.CourseId;
            await _exercisesRepository.DeleteAsync(id);
            return RedirectToAction(nameof(ExerciseList), new { courseId = idcourse });
        }
    }
}
