using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebSiteHocTiengNhat.Models;
using WebSiteHocTiengNhat.Repositories;
using WebSiteHocTiengNhat.Repository;

namespace WebSiteHocTiengNhat.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    // Class để chứa thông tin câu trả lời của người dùng

    public class APIExercisesController : ControllerBase
    {

        private readonly IExercisesRepository _repository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IScoreTableRepository _scoreTableRepository;
        private readonly IExercisesRepository _exercisesRepository;
        private readonly IUserCourseRepository _userCourseRepository;
        public APIExercisesController(IExercisesRepository repository, IQuestionRepository questionRepository, IScoreTableRepository scoreTableRepository,
            IExercisesRepository exercisesRepository, IUserCourseRepository userCourseRepository)
        {
            _repository = repository;
            _questionRepository = questionRepository;
            _scoreTableRepository = scoreTableRepository;
            _exercisesRepository = exercisesRepository;
            _userCourseRepository = userCourseRepository;
        }

        // GET: api/APIExercises
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises()
        {
            var exercises = await _repository.GetAllAsync();
            return Ok(exercises);
        }

        // GET: api/APIExercises/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Exercise>> GetExercise(int id)
        {
            var exercise = await _repository.GetByIdAsync(id);

            if (exercise == null)
            {
                return NotFound();
            }

            return Ok(exercise);
        }

        // GET: api/APIExercises/course/{courseId}
        [HttpGet("GetExerciseByCourse/{courseId}")]
        public async Task<ActionResult<Exercise>> GetExerciseByCourseId(int courseId)
        {
            var exercise = await _repository.GetListByCourseIdAsync(courseId);

            if (exercise == null)
            {
                return NotFound();
            }

            return Ok(exercise);
        }

        // GET: api/APIExercises/course/{courseId}/category/{categoryId}
        [HttpGet("course/{courseId}/category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExerciseByCourseIdAndCategoryId(int courseId, int categoryId)
        {
            var exercises = await _repository.GetByCourseIdAndCategoryIdAsync(courseId, categoryId);

            if (exercises == null || exercises.Count == 0)
            {
                return NotFound();
            }

            return Ok(exercises);
        }

        // POST: api/APIExercises
        [HttpPost]
        public async Task<ActionResult<Exercise>> PostExercise([FromBody] Exercise exercise)
        {
            // Kiểm tra tính hợp lệ của ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Thêm bài tập mới vào cơ sở dữ liệu
            await _exercisesRepository.AddAsync(exercise);

            // Trả về trạng thái Created cùng với đường dẫn đến bài tập vừa được tạo
            return CreatedAtAction(nameof(GetExercise), new { id = exercise.ExerciseId }, exercise);
        }

        // PUT: api/APIExercises/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExercise(int id, [FromBody] Exercise exercise)
        {
            if (id != exercise.ExerciseId)
            {
                return BadRequest("Exercise ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _repository.UpdateAsync(exercise);
            return NoContent();
        }

        // DELETE: api/APIExercises/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var exercise = await _repository.GetByIdAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("{exerciseId}/questions")]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestionsByExercise(int exerciseId)
        {
            var questions = await _questionRepository.GetByExerciseId(exerciseId);

            if (questions == null || !questions.Any())
            {
                return NotFound();
            }

            return Ok(questions);
        }

        // API để nhận danh sách câu trả lời và tính điểm
        [HttpPost("{exerciseId}/submit")]
        [Authorize]
        public async Task<ActionResult<int>> SubmitAnswers(int exerciseId, string userId, [FromBody] List<AnswerSubmission> submissions)
        {

            // Lấy bài tập dựa trên exerciseId
            var exercise = await _repository.GetByIdAsync(exerciseId);
            if (exercise == null)
            {
                return NotFound("Exercise not found.");
            }

            var questions = await _questionRepository.GetByExerciseId(exerciseId);
            if (questions == null || !questions.Any())
            {
                return NotFound("No questions found for this exercise.");
            }

            int score = 0;
            foreach (var question in questions)
            {
                var userAnswer = submissions.FirstOrDefault(a => a.QuestionId == question.QuestionId);
                if (userAnswer != null && userAnswer.SelectedAnswer == question.CorrectAnswer)
                {
                    score++;
                }
            }           

            // Kiểm tra xem bản ghi điểm đã tồn tại hay chưa
            var existingScoreTable = await _scoreTableRepository.GetByExerciseIdAndUserIdAsync(exerciseId, userId);
            if (existingScoreTable != null)
            {
                // Cập nhật điểm
                existingScoreTable.Score = score;
                await _scoreTableRepository.UpdateAsync(existingScoreTable);

            }
            else
            {
                // Tạo mới bản ghi điểm
                var newScoreTable = new ScoreTable
                {
                    ExerciseId = exerciseId,
                    UserId = userId,
                    CategoryId = exercise.CategoryId, 
                    Score = score
                };
                await _scoreTableRepository.AddAsync(newScoreTable);
               
            }
            if (userId != null)
            {
                var userCourse = await _userCourseRepository.GetByUserIdAndCourseIdAsync(userId, exercise.CourseId);
                if (userCourse != null)
                {
                    int countcourse = await _exercisesRepository.CountByCourseIdAsync(exercise.CourseId);
                    int countscoretable = await _scoreTableRepository.CountByUserIdAsync(userId);


                    if (countcourse > 0)
                    {
                        userCourse.progress = (double)countscoretable / countcourse * 100;
                        await _userCourseRepository.UpdateAsync(userCourse);
                    }
                    else
                    {
                        userCourse.progress = 0;
                    }
                }
            }
           
            return Ok(score);
        }

    }
    public class AnswerSubmission
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
    }


}
