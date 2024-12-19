using Elastic.Clients.Elasticsearch;
using ElasticSearchAPI.Models;
using Microsoft.Extensions.Options;

namespace ElasticSearchAPI.Services
{
    public class ElasticService : IElasticService
    {
        #region Constructor
        private readonly ElasticsearchClient _client;
        private readonly ElasticsSettings _elasticsSettings;
        public ElasticService(IOptions<ElasticsSettings> optionsMonitor)
        {
            _elasticsSettings = optionsMonitor.Value;
            var settings = new ElasticsearchClientSettings(new Uri(_elasticsSettings.Url))
                //Authentication
                .DefaultIndex(_elasticsSettings.DefaultIndex);

            _client = new ElasticsearchClient(settings);
        }
        #endregion

        #region AddOrUpdate
        /*AddOrUpdate bu metod Elasticsearch bazasiga yangi malumot
         qo'shish yoki mavjud malumotni yangilash uchun ishlatiladi*/
        public async Task<bool> AddOrUpdate(User user)
        {
            /*IndexAsync metodi orqali User Obyektini korsatilgan indeksga saqlaydi
             Operatsiya turi sifatida Index (qoshish yoki yangilash) belgilanadi.
             */
            var response = await _client.IndexAsync(user, idx =>
            idx.Index(_elasticsSettings.DefaultIndex)
                .OpType(OpType.Index));

            return response.IsValidResponse;
        }
        #endregion

        #region AddOrUpdateBulk
        /*Bir vaqtning ozida bir nechta 
         User malumotlarini qoshadi yoki yangilaydi*/
        public async Task<bool> AddOrUpdateBulk(IEnumerable<User> users, string indexName)
        {
            /*BulkAsync yordamida barcha users royhatini bir operatsiyada indeksga qoshadi
             DocAsUpsert(true) - Agar makumot bolmasa, 
             yangi hujat qoshadi aks holda mavjudini yangilaydi*/
            var response = await _client.BulkAsync(
                b => b.Index(_elasticsSettings.DefaultIndex)
                    .UpdateMany(users, (ud, u) => ud.Doc(u).DocAsUpsert(true))
                );

            return response.IsValidResponse;
        }
        #endregion

        #region CreateIndexIfNotExistsAsync
        /*Agar indeks mavjud bolmasa, yangi indeks yaratadi*/
        public async Task CreateIndexIfNotExistsAsync(string? indexName)
        {
            /*Indices.Exists yordamida indeks mavjudligini tekshiradi
             Agar indeks yoq bolsa, Indices.CreateAsync yordamida uni yaratadi*/
            if (!_client.Indices.Exists(indexName).Exists)
            {
                await _client.Indices.CreateAsync(indexName);
            }
        }
        #endregion

        #region Get
        /*Indeksdan bitta hujatni kalit(key) boyicha oladi*/
        public async Task<User> Get(string key)
        {
            /*GetAsync chaqiriladi hujjatni indeksdan izlaydi
             Agar hujjat topilsa, response.Source qaytaradi, ya'ni User obyektini.*/
            var response = await _client.GetAsync<User>(key, idx => idx.Index(_elasticsSettings.DefaultIndex));

            return response.Source;
        }
        #endregion

        #region GetAll
        /*Indeksdan barcha User ma'lumotlarini oladi.*/
        public async Task<List<User>>? GetAll()
        {
            /*SearchAsync chaqiriladi, indeksdan barcha User ma'lumotlarini izlaydi
             Javob (response.IsValidResponse) muvaffaqiyatli bo'lsa, hujjatlarni
            ro'yxatga (List<User>) aylantirib qaytaradi.*/
            var response = await _client.SearchAsync<User>(s => s.Index(_elasticsSettings.DefaultIndex));

            return response.IsValidResponse ? response.Documents.ToList() : default;
        }
        #endregion

        #region Remove
        /*Indeksdan kalit(key) boyicha bitta malumotni ochiradi*/
        public async Task<bool> Remove(string key)
        {
            /* DeleteAsync chaqiriladi va hujjat o'chiriladi.
        Javob muvaffaqiyatli bo'lsa, true qaytaradi.*/
            var response = await _client.DeleteAsync<User>(key,
                idx => idx.Index(_elasticsSettings.DefaultIndex));

            return response.IsValidResponse;
        }
        #endregion

        #region RemoveAll
        /*Indeksdagi barcha malumotni ochiradi*/
        public async Task<long?> RemoveAll()
        {
            /*DeleteByQueryAsync chaqiriladi va barcha hujjatlar o'chiriladi.
             Muvaffaqiyatli bo'lsa, o'chirilgan hujjatlar sonini (response.Deleted) qaytaradi.*/
            var response = await _client.DeleteByQueryAsync<User>(d => d.Indices(_elasticsSettings.DefaultIndex));

            return response.IsValidResponse ? response.Deleted : default;
        }
        #endregion

        /*Ma'lumot qo'shish yoki yangilash: Yagona hujjat yoki bir nechta hujjatlar bilan ishlaydi.
        Qidiruv: Ma'lumotlarni kalit bo'yicha yoki butun indeksni olish orqali qidirish.
        O'chirish: Indeksdan bitta yoki barcha hujjatlarni o'chirish.
        Indeks yaratish: Agar kerak bo'lsa, yangi indeks yaratadi.*/

        //Ushbu kod tizimga Elasticsearch bilan sodda va samarali integratsiyani ta'minlaydi.
    }
}
