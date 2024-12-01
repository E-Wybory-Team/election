namespace E_Wybory.Services.Interfaces
{
    public interface IPdfGeneratorService
    {
        Task<string> GeneratePdfWithImage_Syncfusion(string title, string content);
    }
}
