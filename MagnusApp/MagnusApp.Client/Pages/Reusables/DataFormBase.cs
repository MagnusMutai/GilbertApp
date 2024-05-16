﻿using Syncfusion.Blazor.DataForm;
using Syncfusion.Blazor.Notifications;
namespace MagnusApp.Client.Pages.Reusables;

public class DataFormBase : ComponentBase
{
    protected SfToast ToastObj;
    protected string ToastPosition = "Right";
    protected string ToastContent = "We are doing our best to reach out to you within 12 hrs, if not sooner. We appreciate your patience and apologize in advance if it takes a little longer.";

    [CascadingParameter]
    public EmailDto ClientModel { get; set; }

    public SfDataForm dataForm { get; set; }

    [Inject]
    public IEmailService EmailService { get; set; }

    [Parameter]
    public EventCallback<EmailDto> OnFormSubmited { get; set; }

    protected async void HandleValidSubmit()
    {
        await EmailService.SendEmail(ClientModel);
        await OnFormSubmited.InvokeAsync(ClientModel);
        await this.ToastObj.ShowAsync();
        ClientModel = new EmailDto();
        dataForm.Refresh();
    }

    protected  async Task ShowOnClick()
    {
        await this.ToastObj.ShowAsync();
    }

    protected async Task HideOnClick()
    {
        await this.ToastObj.HideAsync("All");
    }
}
