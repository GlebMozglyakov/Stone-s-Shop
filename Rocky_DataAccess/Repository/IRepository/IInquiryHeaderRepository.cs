using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky_Models;
using System.Collections.Generic;

namespace Rocky_DataAccess.Repository.IRepository
{
    public interface IInquiryHeaderRepository : IRepository<InquiryHeader>
    {
        void Update(InquiryHeader obj);
    }
}
