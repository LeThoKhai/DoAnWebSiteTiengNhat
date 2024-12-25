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
    public class LessonsController : Controller
    {
        private readonly ICoursesRepository _coursesRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILessonRepository _lessonRepository;

        public LessonsController(ICoursesRepository coursesRepository, ICategoryRepository categoryRepository,
        ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
            _coursesRepository = coursesRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _coursesRepository.GetAllAsync();
            return View(courses);
        }
        public async Task<IActionResult> LessonList(int courseId, int? categoryId)
        {
            // Lấy danh sách tất cả các bài học
            var lessons = await _lessonRepository.GetAllAsync();

            // Lọc bài học theo courseId
            var filteredLessons = lessons.Where(l => l.CourseId == courseId);

            // Nếu categoryId có giá trị, tiếp tục lọc theo categoryId
            if (categoryId.HasValue)
            {
                filteredLessons = filteredLessons.Where(l => l.CategoryId == categoryId.Value);
            }

            // Lấy thông tin khóa học
            var course = await _coursesRepository.GetByIdAsync(courseId);
            var courseName = course?.CourseName;

            // Lấy danh sách các category để hiển thị trong combobox
            var categories = await _categoryRepository.GetAllAsync();
            var categoryList = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();

            // Tạo ViewModel và truyền dữ liệu vào
            var viewModel = new LessonsViewModel
            {
                CourseName = courseName,
                CourseId = courseId,
                Lessons = filteredLessons.ToList(),
                Categories = categoryList
            };

            // Trả về view với ViewModel
            return View(viewModel);
        }



        // Action Create nhận courseId
        public async Task<IActionResult> Create(int courseId)
        {
            var lesson = new Lesson { CourseId = courseId };
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                await _lessonRepository.AddAsync(lesson);
                return RedirectToAction(nameof(LessonList), new { courseId = lesson.CourseId });
            }
            return RedirectToAction(nameof(LessonList), new { courseId = lesson.CourseId });
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int Id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(Id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }
        public async Task<IActionResult> Update(int id)
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }
        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Lesson lesson)
        {
            if (id != lesson.LessonId)
            {
                return NotFound();
            }
            var ls = await _lessonRepository.GetByIdAsync(id);
          
            if (ModelState.IsValid)
            {
                var existingProduct = await _lessonRepository.GetByIdAsync(id); // Giả định có phương thức GetByIdAsync       
                // Cập nhật các thông tin khác của sản phẩm
                existingProduct.LessonName = lesson.LessonName;
                existingProduct.CategoryId = lesson.CategoryId;
                existingProduct.Content = lesson.Content;
                await _lessonRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(LessonList), new { courseId = existingProduct.CourseId });
            }
            return RedirectToAction(nameof(LessonList), new { courseId = ls.CourseId});

        }
        // Hiển thị form xác nhận xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }
        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(int id, Lesson ls)
        {   
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            int idcourse = lesson.CourseId;
            await _lessonRepository.DeleteAsync(id);
            return RedirectToAction(nameof(LessonList), new { courseId = idcourse });
        }
    }
}



