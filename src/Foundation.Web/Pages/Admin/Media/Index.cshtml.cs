using System.ComponentModel.DataAnnotations;
using Foundation.Application.Abstractions;
using Foundation.Application.Common;
using Foundation.Application.Features.Cms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foundation.Web.Pages.Admin.Media;

public class IndexModel : PageModel
{
    private readonly IQueryHandler<GetMediaFilesQuery, Result<PagedList<MediaFileDto>>> _getMediaHandler;
    private readonly ICommandHandler<UploadMediaCommand, Result<MediaFileDto>> _uploadMediaHandler;
    private readonly ICommandHandler<DeleteMediaCommand, Result> _deleteMediaHandler;

    public IndexModel(
        IQueryHandler<GetMediaFilesQuery, Result<PagedList<MediaFileDto>>> getMediaHandler,
        ICommandHandler<UploadMediaCommand, Result<MediaFileDto>> uploadMediaHandler,
        ICommandHandler<DeleteMediaCommand, Result> deleteMediaHandler)
    {
        _getMediaHandler = getMediaHandler;
        _uploadMediaHandler = uploadMediaHandler;
        _deleteMediaHandler = deleteMediaHandler;
    }

    public PagedList<MediaFileDto>? MediaList { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select a file to upload.")]
    public IFormFile? UploadFile { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int page = 1)
    {
        var result = await _getMediaHandler.HandleAsync(new GetMediaFilesQuery(PageNumber: page, PageSize: 20));
        if (result.IsSuccess)
        {
            MediaList = result.Value;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        if (UploadFile == null || UploadFile.Length == 0)
        {
            ErrorMessage = "Please select a valid file.";
            return RedirectToPage();
        }

        using var stream = UploadFile.OpenReadStream();
        var command = new UploadMediaCommand(stream, UploadFile.FileName, UploadFile.ContentType, UploadFile.Length);
        var result = await _uploadMediaHandler.HandleAsync(command);

        if (result.IsSuccess)
        {
            StatusMessage = $"File '{UploadFile.FileName}' uploaded successfully.";
        }
        else
        {
            ErrorMessage = result.Error;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var result = await _deleteMediaHandler.HandleAsync(new DeleteMediaCommand(id));
        if (result.IsSuccess)
        {
            StatusMessage = "Media file deleted successfully.";
        }
        else
        {
            ErrorMessage = result.Error;
        }

        return RedirectToPage();
    }
}
