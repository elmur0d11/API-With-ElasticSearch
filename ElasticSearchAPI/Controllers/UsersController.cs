using ElasticSearchAPI.Models;
using ElasticSearchAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElasticSearchAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region Constructor
        private readonly ILogger<UsersController> _logger;
        private readonly IElasticService _elasticService;

        public UsersController(ILogger<UsersController> logger, IElasticService elasticService)
        {
            _logger = logger;
            _elasticService = elasticService;
        }
        #endregion

        #region Create Index
        /*Elastic Searchda indeks yaratish*/
        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndexAsync(string indexName)
        {
            /*CreateIndexIfNotExistsAsync metodini chaqiradi. 
              Agar indeks mavjud bo'lmasa, uni yaratadi.
            Agar indeks mavjud bo'lsa, "Index already exist" deb xabar beradi.
            Ok javobi bilan muvaffaqiyatli natija qaytariladi.*/
            await _elasticService.CreateIndexIfNotExistsAsync(indexName);
            return Ok($"Index {indexName} created or already exist.");
        }
        #endregion

        #region Add User
        /*Yangi foydalanuvchi qoshish*/
        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            /*AddOrUpdate metodidan foydalanib, foydalanuvchini Elasticsearch'ga qo'shadi.
             Agar muvaffaqiyatli bo'lsa, "User added successfully" xabarini beradi,
            aks holda server xatosi (500) qaytaradi.*/
            var result = await _elasticService.AddOrUpdate(user);
            return result ? Ok("User added successfully.") 
                : StatusCode(StatusCodes.Status500InternalServerError, "Failed to add user.");
        }
        #endregion

        #region Update User
        /*Foydalanuvchini yangilash.*/
        [HttpPost("Update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] User user)
        {
            /*AddOrUpdate metodidan foydalangan holda foydalanuvchini yangilaydi.
             Agar yangilash muvaffaqiyatli bo'lsa, "User updated successfully" 
            deb xabar beradi, aks holda server xatosi (500) qaytaradi.*/
            var result = await _elasticService.AddOrUpdate(user);
            return result ? Ok("User updated successfully.")
                : StatusCode(StatusCodes.Status500InternalServerError, "Failed to update user.");
        }
        #endregion

        #region Get User
        /*Berilgan kalit (key) bo'yicha foydalanuvchini olish.*/
        [HttpGet("get-user/{key}")]
        public async Task<IActionResult> GetUser(string key)
        {
            /*AddOrUpdate metodidan foydalangan holda foydalanuvchini yangilaydi.
             Agar yangilash muvaffaqiyatli bo'lsa, "User updated successfully" 
            deb xabar beradi, aks holda server xatosi (500) qaytaradi.*/
            var user = await _elasticService.Get(key);
            return user != null ? Ok(user)
                : NotFound("User not found!");
        }
        #endregion

        #region Get All Users
        /*: Barcha foydalanuvchilarni olish.*/
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            /*GetAll metodini chaqirib, barcha foydalanuvchilarni Elasticsearch'dan oladi.
             Agar foydalanuvchilar mavjud bo'lsa, Ok (200) bilan foydalanuvchilar ro'yxatini qaytaradi,
            aks holda "No users found!" (404) qaytariladi.*/
            var users = await _elasticService.GetAll();
            return users != null ? Ok(users)
                : NotFound("No users found!");
        }
        #endregion

        #region Remove User
        /*Berilgan kalit bo'yicha foydalanuvchini o'chirish.*/
        [HttpDelete("remove-user/{key}")]
        public async Task<IActionResult> RemoveUser(string key)
        {
            /*Remove metodini chaqirib, kalitga asoslangan foydalanuvchini Elasticsearch'dan o'chiradi.
             Agar foydalanuvchi muvaffaqiyatli o'chirilsa, "User removed successfully" 
            deb xabar beradi, aks holda "User not found!" (404) qaytariladi.*/
            var result = await _elasticService.Remove(key);
            return result ? Ok("User removed successfully.")
                : NotFound("User not found!");
        }
        #endregion

    }
}
