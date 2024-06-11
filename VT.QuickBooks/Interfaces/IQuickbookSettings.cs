using VT.Data.Entities;
using VT.QuickBooks.DTOs;

namespace VT.QuickBooks.Interfaces
{
    public interface IQuickbookSettings
    {
        QuickbookBaseResponse SyncQuickbookSettings(QuickbooksSettingsRequest request);
        QuickbookSettings GetQuickbookSettings(int companyId);
    }
}
