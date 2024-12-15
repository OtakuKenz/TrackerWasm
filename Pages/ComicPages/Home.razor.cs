using System.Net;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using TrackerWasm.Models.ComicModels;
using System;
using System.Net.Http.Json;
using System.Text;

namespace TrackerWasm.Pages.ComicPages;

public partial class Home : ComponentBase
{
    private List<Comic> _comics = [];

    private ColumnsToDisplay _columnsToDisplay = new();
    private ColumnsToDisplay _newValue = new();

    private Collapse _columnSetting = default!;

    private Collapse _filterSetting = default!;

    private Grid<Comic> _grid = default!;

    private Comic _search = new();

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected PreloadService PreloadService { get; set; } = default!;

    [Inject] public HttpClient Http { get; set; } = default!;

    [Inject]
    protected ToastService ToastService { get; set; } = default!;

    private class ColumnsToDisplay
    {
        public bool ReadStatus { get; set; } = false;
        public bool Type { get; set; } = false;
        public bool PublishingStatus { get; set; } = false;
    }
    
    private async Task<GridDataProviderResult<Comic>> EmployeesDataProvider(GridDataProviderRequest<Comic> request)
    {
        var searchUrlParam = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(_search.Title))
        {
            searchUrlParam.Append($"title={_search.Title}&");
        }
        if (_search.ReadStatusId != null)
        {
            searchUrlParam.Append($"readStatusId={_search.ReadStatusId}&");
        }
        if (_search.ComicTypeId != null)
        {
            searchUrlParam.Append($"comicTypeId={_search.ComicTypeId}&");
        }
        if (_search.PublishingStatusId != null)
        {
            searchUrlParam.Append($"publishingStatusId={_search.PublishingStatusId}&");
        }
        PreloadService.Show();
        var data = await Http.GetFromJsonAsync<List<Comic>>($"api/Comic?{searchUrlParam.ToString()}") ?? [];
        PreloadService.Hide();
        return await Task.FromResult(request.ApplyTo(data));
    }

    private void SelectionChanged(int value)
    {
        _search.ComicTypeId = value;
    }

    private void Add()
    {
        NavigationManager.NavigateTo("/comic/add");
    }

    private void Edit(int id)
    {
        NavigationManager.NavigateTo($"/comic/edit/{id}");
    }

    string _readStatusDisplayed = "";

    private async Task ApplySetting()
    {
        _readStatusDisplayed = _newValue.ReadStatus ? "" : "d-none";
        _columnsToDisplay = new ColumnsToDisplay
        {
            ReadStatus = _newValue.ReadStatus,
            Type = _newValue.Type,
            PublishingStatus = _newValue.PublishingStatus,
        };
        PreloadService.Show(SpinnerColor.Light, "Updating Table...");
        await Task.Delay(200);
        StateHasChanged();
        PreloadService.Hide();
    }

    private async Task ToggleContentAsync()
    {
        _newValue = new ColumnsToDisplay
        {
            ReadStatus = _columnsToDisplay.ReadStatus,
            Type = _columnsToDisplay.Type,
            PublishingStatus = _columnsToDisplay.PublishingStatus,
        };
        await _columnSetting.ToggleAsync();
    }

    private async Task ToggleFilterAsync()
    {
        await _filterSetting.ToggleAsync();
    }

    private async Task FilterTable()
    {
        PreloadService.Show(SpinnerColor.Light, "Filtering data...");
        // await LoadComicData(search);
        await _grid.RefreshDataAsync();
        PreloadService.Hide();
    }

    private async Task ClearFilterForm()
    {
        _search = new();
        await FilterTable();
    }
}