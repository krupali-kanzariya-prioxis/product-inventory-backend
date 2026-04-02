//using Demo_Annoncement.Model.CommonModel;
//using Demo_Annoncement.Model.RequestModel;
//using Demo_Annoncement.Model.ResponseModel;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Demo_Annoncement.Service.Repository.Interfaces
//{
//    public interface IAnnouncementRepository
//    {
//        Task<Page> GetAnnouncemetsAsync(Dictionary<string, object> parameters);
//        Task<AnnouncementResponseModel?> GetAnnouncementBySidAsync(string announcementSid);
//        Task<AnnouncementResponseModel> AddAnnouncementAsync(AnnouncementRequestModel model);
//        Task<AnnouncementResponseModel> UpdateAnnouncementAsync(string announcementSid, AnnouncementRequestModel model);
//        Task<bool> DeleteAnnouncementAsync(string announcementSid);
//        Task<IEnumerable<SelectListItem>?> DDLAnnouncementCategory(
//            List<string>? filterSid = null,
//            List<int>? filterId = null);
//    }
//}
