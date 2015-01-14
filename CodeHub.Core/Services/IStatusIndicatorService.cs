namespace CodeHub.Core.Services
{
    public interface IStatusIndicatorService
    {
        void Show(string text);

        void ShowSuccess(string text);

        void ShowError(string text);

        void Hide();
    }
}
