//using Demo_Annoncement.Common;
//using Demo_Annoncement.Model.CommonModel;
//using Demo_Annoncement.Model.Models.AnnouncementDB;
//using Demo_Annoncement.Model.RequestModel;
//using Demo_Annoncement.Model.ResponseModel;
//using Demo_Annoncement.Model.SpDbContext;
//using Demo_Annoncement.Service.Repository.Interfaces;
//using Demo_Annoncement.Service.UnitOfWork;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using static Demo_Annoncement.Common.Enums;

//namespace Demo_Annoncement.Services.Repository.Implementations
//{
//    public class AnnouncementRepository : IAnnouncementRepository
//    {
//        private readonly AnnouncementDBContext _context;
//        private readonly ILogger<AnnouncementRepository> _logger;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly AnnouncementSpContext _spContext;

//        public AnnouncementRepository(
//            AnnouncementDBContext context,
//            ILogger<AnnouncementRepository> logger,
//            IUnitOfWork unitOfWork,
//            AnnouncementSpContext spContext)
//        {
//            _context = context;
//            _logger = logger;
//            _unitOfWork = unitOfWork;
//            _spContext = spContext;
//        }

//        public async Task<Page> GetAnnouncemetsAsync(Dictionary<string, object> parameters)
//        {
//            try
//            {
//                var xmlParam = CommonHelper.DictionaryToXml(parameters, "Search");
//                string sqlQuery = "sp_get_announcement_list {0}";
//                object[] param = { xmlParam };

//                var result = await _spContext.ExecutreStoreProcedureResultList(sqlQuery, param);

//                var jsonResult = JsonConvert.DeserializeObject<List<AnnouncementResponseModel>>(result.Result?.ToString() ?? "[]") ?? [];

//                result.Result = jsonResult;

//                return result;
//            }
//            catch (Exception e)
//            {
//                var errorMessage = e.InnerException?.Message ?? e.Message;
//                _logger.LogError(e, "SQL execution failed: {Message}", errorMessage);
//                throw new HttpStatusCodeException(500, errorMessage);
//            }
//        }

//        public async Task<AnnouncementResponseModel?> GetAnnouncementBySidAsync(string announcementSid)
//        {
//            var announcement = await _context.Announcements
//    .Include(x => x.Category)
//    .FirstOrDefaultAsync(x =>
//        x.AnnouncementSid == announcementSid &&
//        x.Status == (int)StatusTypeDB.Active);

//            if (announcement == null)
//                return null;

//            _logger.LogInformation("Fetched announcement with SID {Sid}", announcementSid);

//            return MapToResponse(announcement);
//        }

//        public async Task<AnnouncementResponseModel> AddAnnouncementAsync(AnnouncementRequestModel model)
//        {
//            try
//            {
//                var category = await _unitOfWork
//    .GetRepository<AnnouncementCategory>()
//    .SingleOrDefaultAsync(x =>
//        x.CategorySid == model.CategorySID &&
//        x.Status == (int)StatusTypeDB.Active);

//                if (category == null)
//                {
//                    throw new HttpStatusCodeException(400, "Invalid category selected.");
//                }
//                var entity = new Announcement
//                {
//                    AnnouncementSid = "ANC" + Guid.NewGuid().ToString("N").ToUpper(),
//                    Title = model.Title,
//                    Body = model.Body,
//                    StartDate = model.StartDate,
//                    EndDate = model.EndDate,
//                    CategoryId = category.CategoryId,
//                    Status = (int)StatusTypeDB.Active,
//                    CreatedDatetime = DateTime.UtcNow,
//                    LastModifiedDateTime = DateTime.UtcNow
//                };

//                await _unitOfWork.GetRepository<Announcement>().InsertAsync(entity);
//                await _unitOfWork.CommitAsync();

//                _logger.LogInformation("Created new announcement with SID {Sid}", entity.AnnouncementSid);

//                return MapToResponse(entity);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while creating announcement");
//                throw new HttpStatusCodeException(500, "Failed to create announcement");
//            }
//        }

//        public async Task<AnnouncementResponseModel> UpdateAnnouncementAsync(
//            string announcementSid,
//            AnnouncementRequestModel model)
//        {
//            try
//            {
//                var announcement = await _unitOfWork
//                    .GetRepository<Announcement>()
//                    .SingleOrDefaultAsync(x =>
//                        x.AnnouncementSid == announcementSid &&
//                        x.Status == (int)StatusTypeDB.Active);

//                if (announcement == null)
//                    return null;

//                var category = await _unitOfWork
//    .GetRepository<AnnouncementCategory>()
//    .SingleOrDefaultAsync(x =>
//        x.CategorySid == model.CategorySID &&
//        x.Status == (int)StatusTypeDB.Active);

//                if (category == null)
//                {
//                    throw new HttpStatusCodeException(400, "Invalid category selected.");
//                }
//                announcement.Title = model.Title;
//                announcement.Body = model.Body;
//                announcement.CategoryId = category.CategoryId;
//                announcement.StartDate = model.StartDate;
//                announcement.EndDate = model.EndDate;
//                announcement.LastModifiedDateTime = DateTime.UtcNow;

//                _unitOfWork.GetRepository<Announcement>().Update(announcement);
//                await _unitOfWork.CommitAsync();

//                _logger.LogInformation("Updated announcement with SID {Sid}", announcementSid);

//                return MapToResponse(announcement);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while updating announcement {Sid}", announcementSid);
//                throw new HttpStatusCodeException(500, "Failed to update announcement");
//            }
//        }

//        public async Task<bool> DeleteAnnouncementAsync(string announcementSid)
//        {
//            var announcement = await _unitOfWork
//                .GetRepository<Announcement>()
//                .SingleOrDefaultAsync(x =>
//                    x.AnnouncementSid == announcementSid &&
//                    x.Status == (int)StatusTypeDB.Active);

//            if (announcement == null)
//                return false;

//            announcement.Status = (int)StatusTypeDB.Delete;
//            announcement.LastModifiedDateTime = DateTime.UtcNow;

//            _unitOfWork.GetRepository<Announcement>().Update(announcement);
//            await _unitOfWork.CommitAsync();

//            _logger.LogInformation("Soft deleted announcement with SID {Sid}", announcementSid);

//            return true;
//        }
//        public async Task<IEnumerable<SelectListItem>?> DDLAnnouncementCategory(
//            List<string>? filterSid = null,
//            List<int>? filterId = null)
//        {
//            IList<AnnouncementCategory> data =
//                await _unitOfWork
//                .GetRepository<AnnouncementCategory>()
//                .GetAllAsync(x => x.Status == (int)StatusTypeDB.Active);

//            if (data == null)
//                return null;

//            if (filterSid != null && filterSid.Count > 0)
//                data = data.Where(x => filterSid.Contains(x.CategorySid)).ToList();

//            if (filterId != null && filterId.Count > 0)
//                data = data.Where(x => filterId.Contains(x.CategoryId)).ToList();

//            IEnumerable<SelectListItem> dropdown =
//                data.Select(x => new SelectListItem
//                {
//                    Value = x.CategorySid.ToString(),
//                    Text = x.Name
//                }).OrderBy(x => x.Text);

//            return dropdown;
//        }

//        private static AnnouncementResponseModel MapToResponse(Announcement entity)
//        {
//            return new AnnouncementResponseModel
//            {
//                AnnouncementID = entity.AnnouncementId,
//                AnnouncementSID = entity.AnnouncementSid,
//                Title = entity.Title,
//                Body = entity.Body,
//                StartDate = entity.StartDate,
//                EndDate = entity.EndDate,
//                Status = entity.Status,
//                CategorySID = entity.Category?.CategorySid,
//                CategoryName = entity.Category?.Name ?? string.Empty,
//                CreatedDatetime = entity.CreatedDatetime,
//                LastModifiedDateTime = entity.LastModifiedDateTime
//            };
//        }
//    }
//}