﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSiteHocTiengNhat.Models;
using WebSiteHocTiengNhat.Repository;
using WebSiteHocTiengNhat.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace WebSiteHocTiengNhat.Areas.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsApiController : ControllerBase
    {
        private readonly ICoursesRepository _coursesRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILessonRepository _lessonRepository;

        public LessonsApiController(ICoursesRepository coursesRepository, ICategoryRepository categoryRepository,
        ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
            _coursesRepository = coursesRepository;
            _categoryRepository = categoryRepository;
        }

        // GET: api/LessonsApi
        [HttpGet("course/{courseId}/lessons")]
        public async Task<IActionResult> GetLessons(int courseId, int? categoryId)
        {
            var lessons = await _lessonRepository.GetAllAsync();
            var filteredLessons = lessons.Where(l => l.CourseId == courseId);

            if (categoryId.HasValue)
            {
                filteredLessons = filteredLessons.Where(l => l.CategoryId == categoryId.Value);
            }

            return Ok(filteredLessons);
        }
        [HttpGet("GetLessonByCourse/{courseId}")]
        public async Task<IActionResult> GetLessonByCourseId(int courseId)
        {
            var lesson= await _lessonRepository.GetListLessonByCourseId(courseId);
            if (lesson == null)
            {
                return NotFound();
            }
            return Ok(lesson);
        }
        // GET: api/LessonsApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            return Ok(lesson);
        }

        // POST: api/LessonsApi
        [HttpPost]
        public async Task<IActionResult> CreateLesson([FromBody] Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                await _lessonRepository.AddAsync(lesson);
                return CreatedAtAction(nameof(GetLessonById), new { id = lesson.LessonId }, lesson);
            }
            return BadRequest(ModelState);
        }

        // PUT: api/LessonsApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLesson(int id,[FromBody] Lesson lesson)
        {
            if (id != lesson.LessonId)
            {
                return BadRequest("Lesson ID mismatch.");
            }

            var existingLesson = await _lessonRepository.GetByIdAsync(id);
            if (existingLesson == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingLesson.LessonName = lesson.LessonName;
                existingLesson.CategoryId = lesson.CategoryId;
                existingLesson.Content = lesson.Content;

                await _lessonRepository.UpdateAsync(existingLesson);
                return NoContent();
            }
            return BadRequest(ModelState);
        }

        // DELETE: api/LessonsApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            await _lessonRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}