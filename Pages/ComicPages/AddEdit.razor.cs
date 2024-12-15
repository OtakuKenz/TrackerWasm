using System.Net;
using System.Net.Http.Json;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using TrackerWasm.Models.ComicModels;

namespace TrackerWasm.Pages.ComicPages;

public partial class AddEdit : ComponentBase
{
    [Parameter] public int? ComicId { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject] protected PreloadService PreloadService { get; set; } = default!;

    [Inject] protected ToastService ToastService { get; set; } = default!;

    [Inject] public HttpClient Http { get; set; } = default!;

    private Comic comic = new();
    private ConfirmDialog deleteDialog = default!;

    protected override async Task OnInitializedAsync()
    {
        PreloadService.Show();
        // Load dropdown options

        if (ComicId.HasValue)
        {
            try
            {
                comic = await Http.GetFromJsonAsync<Comic>($"api/Comic/{ComicId}") ?? throw new HttpRequestException();
            }
            catch (HttpRequestException ex)
            {
                ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to retrieve Comic detail."));
                NavigationManager.NavigateTo("/comic/home");
            }
        }

        PreloadService.Hide();
    }

    private async Task HandleValidSubmit()
    {
        var isSuccess = false;
        var isDuplicated = false;
        PreloadService.Show(loadingText: "Saving comic...");
        if (ComicId == null)
        {
            var result = await Http.PostAsJsonAsync("api/Comic", comic);
            isSuccess = result.IsSuccessStatusCode;
            isDuplicated = result.StatusCode == HttpStatusCode.Conflict;
        }
        else
        {
            var result = await Http.PutAsJsonAsync($"api/Comic/{ComicId}", comic);
            isSuccess = result.IsSuccessStatusCode;
            isDuplicated = result.StatusCode == HttpStatusCode.Conflict;
        }

        PreloadService.Hide();
        if (isSuccess)
        {
            ToastService.Notify(new ToastMessage(ToastType.Success,
                $"Record {(ComicId == null ? "added" : "updated")}."));
            NavigationManager.NavigateTo("comic/home");
        }
        else if (isDuplicated)
        {
            ToastService.Notify(new ToastMessage(ToastType.Warning,
                $"Failed to {(ComicId == null ? "add" : "update")}. An entry with the same title already exist."));
        }
        else
        {
            ToastService.Notify(new ToastMessage(ToastType.Warning,
                $"Failed to {(ComicId == null ? "add" : "update")}. Unexpected error occured. Please try again."));
        }
    }

    private void NavigateBack()
    {
        NavigationManager.NavigateTo(" /comic/home");
    }

    private async Task ShowConfirmationAsync()
    {
        var confirmation = await deleteDialog.ShowAsync(
            title: "Are you sure you want to delete this?",
            message1: $"Title: {comic.Title}"
        );
        if (confirmation)
        {
            PreloadService.Show(SpinnerColor.Light, "Deleteing...");
            var result = await Http.DeleteAsync($"api/Comic/{ComicId}");
            PreloadService.Hide();
            if (result.IsSuccessStatusCode)
            {
            ToastService.Notify(new ToastMessage(ToastType.Success, "Record deleted."));
            NavigationManager.NavigateTo("/comic/home");
            }
            else
            {
                ToastService.Notify(new ToastMessage(ToastType.Danger, "Failed to delete."));
            }
        }
    }
}